using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using CringeBootstrap.Abstractions;
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

    public event Action? BeforeExit;

    private Process? _actualHostProcess;
    private string? _stderrPath;

    private readonly string _appdataDir = Path.Join(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "CringeLauncher");

    private readonly string _logsDir;

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
            RestartRequested = false;
            Directory.CreateDirectory(_logsDir);
            _stderrPath = FindStderrRedirectPath(_logsDir);
            var appHost = FindValidAppHostPath(args);

            _actualHostProcess = Process.Start(new ProcessStartInfo(appHost, [..args, "--crashpad-stderr-redirect", _stderrPath])
            {
                Environment =
                {
                    ["DOTNET_BOOTSTRAP_ENTRYPOINT"] = LauncherConstants.ActualBootstrapEntrypoint
                }
            });

            // detach from console, the window would be closed when actual host process also detaches from it
            if (!ConsoleHandler.ShouldKeepConsole(args)) ConsoleHandler.FreeConsole();

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

            var path = Path.Join(_logsDir, $"crash-info-{_actualHostProcess.Id}.json");

            if (WaitForProcessExit())
            {
                File.Delete(path);
                BeforeExit?.Invoke();
                return true;
            }

            Log.Error("Actual host process exited with code {ExitCode:x8}", _actualHostProcess.ExitCode);

            RunCrashInfoDialog(_actualHostProcess.ExitCode, path);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Failed to run crashpad");
            BeforeExit?.Invoke();
            return false;
        }

        BeforeExit?.Invoke();
        return true;
    }

    private void RunCrashInfoDialog(int exitCode, string crashInfoPath)
    {
        InitializeCrashDialogServices();

        var configDir = Path.Join(_appdataDir, "config");
        ImGuiHandler.Instance = new(Directory.CreateDirectory(configDir));

        var exitEvent = new ManualResetEventSlim();

        RenderHandler.Current.RegisterComponent(new CrashPadComponent(ReadCrashInformation(crashInfoPath), _stderrPath, exitCode,
            exitEvent));

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

    [MemberNotNullWhen(false, nameof(_actualHostProcess))]
    private bool WaitForProcessExit()
    {
        if (_actualHostProcess is null) return true;

        _actualHostProcess.WaitForExit();

        if (_actualHostProcess.ExitCode == -2)
            RestartRequested = true;

        return _actualHostProcess.ExitCode is 0 or -2;
    }

    private static string FindStderrRedirectPath(string basePath)
    {
        const string name = "crashpad-stderr-redirect-{0:yyyy-MM-dd_HH}-{1}.txt";

        for (var i = 0; i < 1000; i++)
        {
            var path = Path.Join(basePath, string.Format(name, DateTime.Now, i));
            if (!File.Exists(path)) return path;
        }

        throw new InvalidOperationException("Unable to find free stderr redirect path");
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