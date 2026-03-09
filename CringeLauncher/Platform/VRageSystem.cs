using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using CringeLauncher.CrashPad;
using CringeLauncher.Render;
using NLog;
using ParallelTasks;
using Sandbox.Game;
using VRage;
using VRage.FileSystem;
using VRage.Library.Threading;
using VRage.Platform.Windows.Win32;
using VRage.Utils;

namespace CringeLauncher.Platform;

internal class VRageSystem(string applicationName, VRageWindowSurrogate? surrogate, string? appdataPath) : IVRageSystem
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public string GetOsName() => RuntimeInformation.OSDescription;

    public string GetInfoCPU([UnscopedRef] out uint frequency, [UnscopedRef] out uint physicalCores)
    {
        frequency = 0;
        physicalCores = (uint)Environment.ProcessorCount;
        return CrashReportWriter.QueryProcessorName() ?? "Unknown";
    }

    public ulong GetTotalPhysicalMemory()
    {
        return (ulong)GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
    }

    public void LogEnvironmentInformation()
    {
    }

    public void GetGCMemory([UnscopedRef] out float allocated, [UnscopedRef] out float used)
    {
        var info = GC.GetGCMemoryInfo();
        allocated = (float)(info.TotalCommittedBytes / 1024.0 / 1024.0);
        used = (float)(info.HeapSizeBytes / 1024.0 / 1024.0);
    }

    public List<string> GetProcessesLockingFile(string path)
    {
        return Processes.GetProcessesLockingFile(path);
    }

    public void ResetColdStartRegister()
    {
    }

    public ulong GetThreadAllocationStamp() => 0;

    public ulong GetGlobalAllocationsStamp() => 0;

    public string? GetRootPath()
    {
        return null;
    }

    public string GetAppDataPath() => appdataPath ??
                                      Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                          applicationName);

    public void WriteLineToConsole(string msg) => Console.WriteLine(msg);

    public void LogToExternalDebugger(string message)
    {
    }

    public void DebuggerBreak()
    {
    }

    public void CollectGC(int generation = 2147483647, GCCollectionMode mode = GCCollectionMode.Default)
    {
        CollectGC(generation, mode, true, false);
    }

    public void CollectGC(int generation, GCCollectionMode mode, bool blocking, bool compacting)
    {
        Debug.WriteLine($"GC collect requested: generation={generation}, mode={mode}, blocking={blocking}, compacting={compacting}");
    }

    public bool OpenUrl(string url, bool predetermined = true)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            return false;

        try
        {
            using var process = Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
            
            return process != null;
        }
        catch (Exception e)
        {
            Log.Warn(e, "Failed to open {uri}", uri);
            return false;
        }
    }

    public void OnThreadpoolInitialized()
    {
    }

    public void LogRuntimeInfo(Action<string> log)
    {
    }

    public void OnSessionStarted(SessionType sessionType)
    {
    }

    public void OnSessionUnloaded()
    {
    }

    public int? GetExperimentalPCULimit(int safePcuLimit) => null;

    public string? GetPlatformSpecificCrashReport() => null;

    public ISharedCriticalSection CreateSharedCriticalSection(bool spinLock) =>
        spinLock ? new MyCriticalSection_SpinLock() : new MyCriticalSection_Mutex();

    public DateTime GetNetworkTimeUTC() => DateTime.UtcNow;

    public string? GetModsCachePath() => null;

    public float CPUCounter => 0;
    // available ram in mb
    public float RAMCounter
    {
        get
        {
            var info = GC.GetGCMemoryInfo();
            var availableBytes = info.TotalAvailableMemoryBytes - info.MemoryLoadBytes;
            return (float)(availableBytes / 1024.0 / 1024.0);
        }
    }

    public long RemainingMemoryForGame => long.MaxValue;
    public long ProcessPrivateMemory
    {
        get
        {
            var info = GC.GetGCMemoryInfo();
            return info.MemoryLoadBytes;
        }
    }

    public bool IsUsingGeforceNow => false; // not really an option for us kek

    public bool IsUsingGeforceNowCloud => false;

    public string Clipboard
    {
        get => surrogate is null ? string.Empty : surrogate.Window.ClipboardText;
        set => surrogate?.Window.ClipboardText = value;
    }

    public bool IsAllocationProfilingReady => false;
    public bool IsSingleInstance => true;
    public bool IsRemoteDebuggingSupported => false;
    public SimulationQuality SimulationQuality => SimulationQuality.Normal;
    public bool IsDeprecatedOS => false;
    public bool IsMemoryLimited => false;
    public bool HasSwappedMouseButtons => false;
    public string ThreeLetterISORegionName => CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName;
    public string TwoLetterISORegionName => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    public string RegionLatitude => string.Empty;
    public string RegionLongitude => string.Empty;
    public string TempPath => MyFileSystem.TempPath;
    public int? OptimalHavokThreadCount => null;
    public PrioritizedScheduler.ExplicitWorkerSetup? ExplicitWorkerSetup => null;
    public bool AreEnterBackButtonsSwapped => false;
    public float? ForcedUiRatio => null;
    public event Action<string>? OnSystemProtocolActivated;
    public event Action? OnResuming;
    public event Action? LeaveSession;
    public event Action? OnSuspending;
}
