using System.Runtime.Versioning;
using NLog;

namespace CringeLauncher.CrashPad.Supplemental;

/// <summary>
/// Linux-only crash capture setup applied around the child game process.
/// Gates Phase B knobs (runtime crashreport, coredump_filter) so Windows builds are untouched.
/// </summary>
internal static class LinuxCrashProcessSetup
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// Environment variables for the child process that enable Unix-only .NET crash reporting.
    /// Safe no-op contributor on non-Linux (caller should still gate).
    /// </summary>
    [SupportedOSPlatform("linux")]
    public static void ApplyChildEnvironment(IDictionary<string, string?> environment)
    {
        // createdump JSON companion; not produced on Windows.
        // https://learn.microsoft.com/en-us/dotnet/core/diagnostics/collect-dumps-crash
        environment["DOTNET_EnableCrashReport"] = "1";
    }

    /// <summary>
    /// Widen the kernel coredump filter so a systemd-captured core retains anonymous
    /// mappings useful for managed post-mortem (dotnet docs recommend at least 0x3f).
    /// Must run inside the child process.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public static void TryApplyCoredumpFilter()
    {
        const string path = "/proc/self/coredump_filter";
        // 0x3f: anon private/shared + ELF headers + private/shared file-backed (see core(5)).
        const string desired = "0x3f\n";

        try
        {
            File.WriteAllText(path, desired);
            Log.Debug("Set {Path} to {Value}", path, desired.Trim());
        }
        catch (Exception e)
        {
            // Non-fatal: dump quality may be reduced if systemd ends up with the core.
            Log.Debug(e, "Unable to set {Path}; continuing without coredump_filter tweak", path);
        }
    }
}
