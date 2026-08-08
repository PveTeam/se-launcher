using System.Runtime.InteropServices;
using System.Text.Json;
using CringeBootstrap.Abstractions;
using CringeLauncher.CrashPad.Supplemental;
using Havok;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using VRage.Library.Threading;

namespace CringeLauncher.CrashPad;

/// <summary>
/// Child-process entrypoint used by <c>--crash-test</c> to exercise CrashPad without a full game.
/// Modes via <c>CRINGE_CRASH_TEST_MODE</c>: managed | failfast | sigsegv | exit | havok (default: failfast).
/// </summary>
public sealed class CrashPathTestPlugin : ICorePlugin
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public const string TypeName = "CringeLauncher.CrashPad.CrashPathTestPlugin, CringeLauncher";

    public bool RestartRequested => false;

    public void Dispose()
    {
    }

    public bool Initialize(string[] args, ServiceCollection services)
    {
        if (OperatingSystem.IsLinux())
            LinuxCrashProcessSetup.TryApplyCoredumpFilter();
        return true;
    }

    public bool Run()
    {
        var mode = (Environment.GetEnvironmentVariable("CRINGE_CRASH_TEST_MODE") ?? "failfast")
            .Trim()
            .ToLowerInvariant();

        Log.Info("CrashPathTestPlugin running mode={Mode} pid={Pid}", mode, Environment.ProcessId);

        // Leave a crash-info JSON so the watchdog has version metadata even if we die hard.
        WriteCrashInfoSeed();

        switch (mode)
        {
            case "exit":
                Log.Error("CrashPathTestPlugin intentional non-zero exit");
                Environment.Exit(139);
                return false;

            case "managed":
                throw new InvalidOperationException("CrashPathTestPlugin intentional managed crash");

            case "sigsegv":
                Log.Error("CrashPathTestPlugin triggering SIGSEGV");
                TriggerSigsegv();
                return false;

            case "havok":
                Log.Error("CrashPathTestPlugin triggering FailFast on Havok job thread");
                TriggerHavokWorkerFailFast();
                return false;

            case "failfast":
            default:
                Log.Error("CrashPathTestPlugin triggering Environment.FailFast");
                Environment.FailFast("CrashPathTestPlugin intentional FailFast");
                return false;
        }
    }

    public void Restart() => throw new NotSupportedException();

    private static void WriteCrashInfoSeed()
    {
        try
        {
            var dir = Directory.CreateDirectory(Path.Join(
                Environment.GetEnvironmentVariable("DOTNET_USERDEV_RUNDIR") ??
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CringeLauncher",
                "logs")).FullName;

            var path = Path.Join(dir, $"crash-info-{Environment.ProcessId}.json");
            var info = new CrashInformation
            {
                Network = new(),
                Plugins = [],
                ModScripts = [],
                Version = new()
                {
                    LauncherVersion = "crash-test",
                    UpdatesChannel = "crash-test",
                    GameVersion = "crash-test"
                }
            };

            using var stream = File.Create(path);
            JsonSerializer.Serialize(stream, info);
            Log.Info("Wrote crash-info seed to {Path}", path);
        }
        catch (Exception e)
        {
            Log.Warn(e, "Failed to write crash-info seed");
        }
    }

    /// <summary>
    /// Init Havok PE runtime, spawn <see cref="HkJobThreadPool"/> workers, FailFast from a
    /// worker via PE→reexport stub→managed <c>ThreadAction</c> (same path as game physics).
    /// </summary>
    private static void TriggerHavokWorkerFailFast()
    {
        // Game ALC + LoadReexport already ran in bootstrap; HavokWrapper DllImports point at
        // libCringeBootstrap.Native.so → libhavok.so → Havok.dll.
        Log.Info("Initializing HkBaseSystem for crash-test");
        HkBaseSystem.Init(
            solverMemorySize: 16 * 1024 * 1024,
            LogCallback: static msg => Log.Info("Havok: {Message}", msg),
            deepProfiling: false,
            hkShapeCriticalSection: new NoopSharedCriticalSection());

        const int workerCount = 2;
        Log.Info("Creating HkJobThreadPool workers={Count}", workerCount);
        using var pool = new HkJobThreadPool(workerCount);

        // RunOnEachWorker stores Action, PE workers call ThreadAction stub → ThreadTaskExecutor → Action.
        pool.RunOnEachWorker(static () =>
        {
            try
            {
                ThreadInformationTracker.MarkCurrentThreadType(ExceptionInformation.ThreadType.HavokPool);
            }
            catch
            {
                // Tracker is best-effort; FailFast is the contract.
            }

            Environment.FailFast(
                $"CrashPathTestPlugin intentional Havok worker FailFast tid={Environment.CurrentManagedThreadId} name={Thread.CurrentThread.Name}");
        });

        // If PE runs callbacks async, wait briefly then fail on main so the test still dies.
        pool.WaitForCompletion();
        Thread.Sleep(TimeSpan.FromSeconds(2));
        Environment.FailFast("CrashPathTestPlugin Havok worker FailFast did not terminate process");
    }

    /// <summary>Shape critical section unused by pool-only crash path.</summary>
    private sealed class NoopSharedCriticalSection : ISharedCriticalSection
    {
        public SharedCriticalSection_UniqueLock EnterUnique() => new(this);

        public SharedCriticalSection_SharedLock EnterShared() => new(this);

        public void LeaveUnique_Internal()
        {
        }

        public void LeaveShared_Internal()
        {
        }

        public void Dispose()
        {
        }
    }

    private static void TriggerSigsegv()
    {
        // Null-pointer write: reliable SIGSEGV on Linux without involving the managed EH path first.
        if (OperatingSystem.IsLinux())
            Marshal.WriteInt32(nint.Zero, 0xDEAD);

        Environment.FailFast("CrashPathTestPlugin SIGSEGV fallback");
    }
}
