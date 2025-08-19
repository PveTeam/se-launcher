using System.Diagnostics.CodeAnalysis;
using VRage;
using VRage.Utils;

namespace CringeLauncher.Platform;

internal class CrashReportingSurrogate : IMyCrashReporting
{
    public ExceptionType GetExceptionType(Exception e)
    {
        return ExceptionType.Other;
    }

    public void WriteMiniDump(string dumpPath, MyMiniDump.Options dumpFlags, nint exceptionPointers)
    {
    }

    public void SetNativeExceptionHandler(Action<nint> handler)
    {
    }

    public void PrepareCrashAnalyticsReporting(string logPath, bool GDPRConsent, CrashInfo info, bool isUnsupportedGpu)
    {
    }

    public bool ExtractCrashAnalyticsReport(string[] args, [UnscopedRef] out string? logPath, [UnscopedRef] out CrashInfo info,
        [UnscopedRef] out bool isUnsupportedGpu, [UnscopedRef] out bool exitAfterReport)
    {
        logPath = null;
        info = new();
        isUnsupportedGpu = false;
        exitAfterReport = false;
        return false;
    }

    public void UpdateHangAnalytics(CrashInfo hangInfo, string logPath, bool GDPRConsent)
    {
    }

    public void CleanupCrashAnalytics()
    {
    }

    public bool MessageBoxCrashForm(ref MyCrashScreenTexts texts, [UnscopedRef] out string message,
        [UnscopedRef] out string email)
    {
        message = string.Empty;
        email = string.Empty;
        return false;
    }

    public void MessageBoxModCrashForm(ref MyModCrashScreenTexts texts)
    {
    }

    public void ExitProcessOnCrash(Exception exception)
    {
        ExitingProcessOnCrash?.Invoke(exception);
    }

    public IEnumerable<string> AdditionalReportFiles() => [];

    public void AttachFiles(string[] files)
    {
    }

    public IMySimplifiedErrorReporter? TryToCreateSimplifiedErrorReporter(MyLog defaultLog)
    {
        return null;
    }

    public bool IsCriticalMemory => false;
    public event Action<Exception>? ExitingProcessOnCrash;
}