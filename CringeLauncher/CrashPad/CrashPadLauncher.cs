using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using CliWrap;
using CringeBootstrap.Abstractions;
using CringeLauncher.CrashPad.Supplemental;
using CringeLauncher.Render;
using CringeLauncher.Utils;
using CringePlugins.Render;
using CringePlugins.Services;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace CringeLauncher.CrashPad;

public sealed class CrashPadLauncher : ICorePlugin
{
    public bool RestartRequested { get; private set; }

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private CommandTask<CommandResult>? _actualHostProcess;
    private string? _stderrPath;
    private string? _dumpPath;
    private string? _dumpLogPath;
    private string? _appHostPath;
    private string? _alcMapPath;
    private string? _peMapPath;
    private string? _crossGenCacheKey;

    private readonly string _appdataDir = Path.Join(
        Environment.GetEnvironmentVariable("DOTNET_USERDEV_RUNDIR") ??
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CringeLauncher");

    private readonly string _logsDir;
    private bool _isDedicated;

    private readonly CancellationTokenSource _gracefulCts = new();
    private readonly CrashSupplementalCollector _supplementalCollector = CrashSupplementalCollector.CreateDefault();

    public CrashPadLauncher()
    {
        _logsDir = Path.Join(_appdataDir, "logs");
    }

    public void Dispose()
    {
        _actualHostProcess?.Dispose();
    }

    public bool Initialize(string[] args, ServiceCollection services)
    {
        try
        {
            _isDedicated = args.Contains("--dedicated", StringComparer.OrdinalIgnoreCase);

            using var provider = services.BuildServiceProvider();
            _crossGenCacheKey = provider.GetService<ICrossGenService>()?.CacheKey;

            RestartRequested = false;
            Directory.CreateDirectory(_logsDir);
            _stderrPath = FindFreePath("crashpad-stderr-redirect.txt", _logsDir);
            _dumpPath = FindFreePath("crashpad-dump.dmp", _logsDir);
            _dumpLogPath = FindFreePath("crashpad-dump-log.log", _logsDir);
            _alcMapPath = FindFreePath("crashpad-alc-map.jsonl", _logsDir);
            _peMapPath = FindFreePath("crashpad-pe-map.json", _logsDir);
            _appHostPath = FindValidAppHostPath(args);

            var crashTest = args.Contains("--crash-test", StringComparer.OrdinalIgnoreCase);
            var childEntrypoint = crashTest
                ? CrashPathTestPlugin.TypeName
                : _isDedicated
                    ? LauncherConstants.DedicatedServerEntrypoint
                    : LauncherConstants.ActualBootstrapEntrypoint;

            if (crashTest)
                Log.Warn("Crash-test mode enabled; child entrypoint={Entrypoint}", childEntrypoint);

            var environment = new Dictionary<string, string?>
            {
                ["DOTNET_BOOTSTRAP_ENTRYPOINT"] = childEntrypoint,
                ["DOTNET_DbgEnableMiniDump"] =
                    "1", // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/collect-dumps-crash
                ["DOTNET_DbgMiniDumpType"] =
                    "3", // Triage, Same as Mini, but removes personal user information, such as paths and passwords.
                ["DOTNET_DbgMiniDumpName"] = _dumpPath,
                ["DOTNET_CreateDumpDiagnostics"] = "1", // logging of dump process won't hurt
                ["DOTNET_CreateDumpLogToFile"] = _dumpLogPath,
#if !WINDOWS
#if DEBUG
                ["LD_LIBRARY_PATH"] =
                    $"{Path.Join(AppContext.BaseDirectory, "prefix", "lib")}:{Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}",
                ["PATH"] =
                    $"{Path.Join(AppContext.BaseDirectory, "prefix", "bin")}:{Environment.GetEnvironmentVariable("PATH")}",
#endif
#endif
            };

            // Unix-only runtime crashreport.json (gated inside; no-op on Windows).
            if (OperatingSystem.IsLinux())
                LinuxCrashProcessSetup.ApplyChildEnvironment(environment);

            // Forward crash-test mode to the child if set.
            var crashTestMode = Environment.GetEnvironmentVariable("CRINGE_CRASH_TEST_MODE");
            if (!string.IsNullOrWhiteSpace(crashTestMode))
                environment["CRINGE_CRASH_TEST_MODE"] = crashTestMode;

            if (_isDedicated)
            {
                environment["SteamAppId"] = LauncherConstants.AppId.ToString();
            }

            var cmd = Cli.Wrap(_appHostPath)
                .WithArguments([
                    ..args, "--crashpad-stderr-redirect", _stderrPath, "--crashpad-alc-map", _alcMapPath,
                    "--crashpad-pe-map", _peMapPath
                ])
                .WithWorkingDirectory(AppContext.BaseDirectory)
                .WithEnvironmentVariables(environment)
                .WithValidation(CommandResultValidation.None);

            _actualHostProcess = cmd.ExecuteAsync(info =>
            {
                info.RedirectStandardInput = false;
                info.RedirectStandardOutput = false;
                info.RedirectStandardError = false;
            }, gracefulCancellationToken: _gracefulCts.Token);

            // detach from console, the window would be closed when actual host process also detaches from it
            if (!_isDedicated && !ConsoleHandler.ShouldKeepConsole(args)) ConsoleHandler.FreeConsole();

            return _actualHostProcess is not null;
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to initialize crashpad");
            return false;
        }
    }

    public bool Run()
    {
        try
        {
            if (_actualHostProcess is null) return true;

            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            var path = Path.Join(_logsDir, $"crash-info-{_actualHostProcess.ProcessId}.json");

            if (WaitForProcessExit(out var exitCode))
            {
                File.Delete(path);
                return true;
            }

            Log.Error("Actual host process exited with code {ExitCode:x8}", exitCode);

            var information = ReadCrashInformation(path);
            var processInformation = BuildProcessInformation(exitCode);

            if (_isDedicated)
                WriteCrashReport(information, processInformation);
            else
                RunCrashInfoDialog(information, processInformation);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to run crashpad");
            return false;
        }

        return true;
    }

    private CrashProcessInformation BuildProcessInformation(int exitCode)
    {
        var exitedAt = DateTimeOffset.UtcNow;
        var pid = _actualHostProcess?.ProcessId ?? 0;

        IReadOnlyList<CrashSupplementalSection> sections = [];
        try
        {
            sections = _supplementalCollector.Collect(new CrashSupplementalContext
            {
                ProcessId = pid,
                ExitCode = exitCode,
                DumpPath = _dumpPath!,
                DumpLogPath = _dumpLogPath!,
                StderrPath = _stderrPath!,
                ExecutablePath = _appHostPath,
                AlcMapPath = _alcMapPath,
                PeMapPath = _peMapPath,
                CrossGenCacheKey = _crossGenCacheKey,
                ExitedAtUtc = exitedAt
            });
        }
        catch (Exception e)
        {
            Log.Warn(e, "Supplemental crash collection failed");
        }

        return new CrashProcessInformation(_stderrPath!, _dumpPath!, _dumpLogPath!, exitCode, sections);
    }

    private void ConsoleOnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        if (_gracefulCts.IsCancellationRequested) return;
        Log.Info("Requesting graceful shutdown");
        _gracefulCts.Cancel();
    }

    private void WriteCrashReport(CrashInformation? information, CrashProcessInformation processInformation)
    {
        // Still emit a report when only supplemental sources produced data.
        if (information is null && processInformation.SupplementalSections.Count == 0) return;

        var crashReportDir = Path.Join(_appdataDir, "crash-reports");
        Directory.CreateDirectory(crashReportDir);
        var path = Path.Join(crashReportDir, $"crash-report-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt");
        using (var stream = File.Create(path))
            new CrashReportWriter(information ?? EmptyCrashInformation(), processInformation).Write(stream);
        Log.Info("Crash report written to {Path}", path);
        Console.WriteLine(File.ReadAllText(path));
    }

    private void RunCrashInfoDialog(CrashInformation? information, CrashProcessInformation processInformation)
    {
        InitializeCrashDialogServices();

        var configDir = Path.Join(_appdataDir, "config");
        ImGuiHandler.Instance =
#if WINDOWS
            new Render.Win.WinImGuiHandler(Directory.CreateDirectory(configDir));
#else
            new Render.Xplat.XplatImGuiHandler(Directory.CreateDirectory(configDir));
#endif

        var exitEvent = new ManualResetEventSlim();

        RenderHandler.Current.RegisterComponent(new CrashPadComponent(information, processInformation, exitEvent));

        using var thread = new EarlyRenderThread(true);

        exitEvent.Wait();

        thread.Window?.Invoke(thread.Window.Dispose);
    }

    private static void InitializeCrashDialogServices()
    {
        var collection = new ServiceCollection();

        collection.AddHttpClient<ImGuiImageService>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            });

        collection.AddSingleton<IImGuiImageService>(s => s.GetRequiredService<ImGuiImageService>());
        collection.AddSingleton(_ => RenderHandler.Current);

        GameServicesExtension.GameServices = collection.BuildServiceProvider();
    }

    private static CrashInformation? ReadCrashInformation(string path)
    {
        if (!File.Exists(path)) return null;

        using var stream = File.OpenRead(path);

        try
        {
            return JsonSerializer.Deserialize<CrashInformation>(stream);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to read crash information");
            return null;
        }
    }

    private static CrashInformation EmptyCrashInformation() => new()
    {
        Network = new(),
        Plugins = [],
        ModScripts = [],
        Version = new()
    };

    [MemberNotNullWhen(false, nameof(_actualHostProcess))]
    private bool WaitForProcessExit(out int exitCode)
    {
        exitCode = 0;
        if (_actualHostProcess is null) return true;

        try
        {
            var result = _actualHostProcess.GetAwaiter().GetResult();
            exitCode = result.ExitCode;
        }
        catch (OperationCanceledException)
        {
        }

        if (exitCode == -2)
            RestartRequested = true;

        return exitCode is 0 or -2;
    }

    private static string FindFreePath(string fileName, string basePath)
    {
        var key = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        for (var i = 0; i < 1000; i++)
        {
            var path = Path.Join(basePath, $"{key}-{DateTime.Now:yyyy-MM-dd_HH}-{i}{ext}");
            if (!File.Exists(path)) return path;
        }

        throw new InvalidOperationException($"Unable to find free {key} path");
    }

    private static string FindValidAppHostPath(string[] args)
    {
        if (!args.Contains("--no-mask", StringComparer.OrdinalIgnoreCase))
        {
            var appHostPath = Path.Join(AppContext.BaseDirectory, LauncherConstants.AppName + ".exe");
            if (File.Exists(appHostPath)) return appHostPath;
        }

        // maybe support launching via dotnet/dnx later
        return Environment.ProcessPath ?? throw new FileNotFoundException("Unable to find app host path");
    }

    void ICorePlugin.Restart() => throw new NotSupportedException("Cannot restart the crashpad directly");
}

public record CrashProcessInformation(
    string StderrPath,
    string DumpPath,
    string DumpLogPath,
    int ExitCode,
    IReadOnlyList<CrashSupplementalSection> SupplementalSections);
