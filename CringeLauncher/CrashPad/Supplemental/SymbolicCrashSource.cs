using CliWrap;
using CliWrap.Buffered;
using CringePlugins.Config;
using NLog;

namespace CringeLauncher.CrashPad.Supplemental;

/// <summary>
/// Runs the external <c>symbolic-crash</c> CLI on the createdump artifact and embeds the
/// symbolicated stack (managed file:line via ppdb, PE/reexport frame labels via the pe-map
/// sidecar) as a section. Never fatal: missing tool or timeout returns null.
/// </summary>
internal sealed class SymbolicCrashSource : ICrashSupplementalSource
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);
    private const int MaxBodyChars = 48 * 1024;

    public string Id => "symbolic-crash";

    public CrashSupplementalSection? TryCollect(CrashSupplementalContext context)
    {
        if (!OperatingSystem.IsLinux())
            return null;
        if (!File.Exists(context.DumpPath))
            return null;

        try
        {
            var args = BuildArguments(context);
            var output = Run(args);
            if (string.IsNullOrWhiteSpace(output))
                return null;

            var body = output.Length > MaxBodyChars
                ? output[..MaxBodyChars] + "\n… truncated …"
                : output;

            return new CrashSupplementalSection
            {
                SourceId = Id,
                Title = "Symbolicated Stack (symbolic)",
                Body = body,
                // Above the raw runtime crash report: this subsumes it.
                Priority = 120,
                Summary = null
            };
        }
        catch (Exception e)
        {
            Log.Warn(e, "symbolic-crash invocation failed");
            return null;
        }
    }

    private static List<string> BuildArguments(CrashSupplementalContext context)
    {
        var args = new List<string> { "auto", context.DumpPath };

        var reportPath = context.DumpPath + ".crashreport.json";
        if (File.Exists(reportPath))
            args.AddRange(["--report", reportPath]);
        if (context.AlcMapPath is not null && File.Exists(context.AlcMapPath))
            args.AddRange(["--alc-map", context.AlcMapPath]);
        if (context.PeMapPath is not null && File.Exists(context.PeMapPath))
            args.AddRange(["--pe-map", context.PeMapPath]);

        foreach (var root in SymbolRoots(context))
            args.AddRange(["-s", root]);

        args.AddRange(["--pdb-server", LauncherConfigRegionalDefaults.Current.SymbolsSource]);
        return args;
    }

    private static IEnumerable<string> SymbolRoots(CrashSupplementalContext context)
    {
        // Launcher dir (CringeLauncher.dll + pdb, natives).
        yield return AppContext.BaseDirectory;

        // Shared runtime of this process (libcoreclr et al, bundled .dbg/.pdb).
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location);
        if (runtimeDir is not null)
            yield return runtimeDir;

        // Caches hold the transformed game/mod/script assemblies that actually ran
        // (their CodeView ids match the symbols server).
        var cacheRoot = Path.Join(
            Environment.GetEnvironmentVariable("DOTNET_USERDEV_RUNDIR") ??
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "CringeLauncher", "cache");

        if (context.CrossGenCacheKey is { } key)
        {
            foreach (var kind in new[] { "R2R", "NOOP", "mods", "scripts" })
            {
                var dir = Path.Join(cacheRoot, kind, key);
                if (Directory.Exists(dir))
                    yield return dir;
            }
            yield break;
        }

        // Fallback when the key is unknown (e.g. services unavailable): newest dir per kind.
        foreach (var kind in new[] { "R2R", "NOOP", "mods", "scripts" })
        {
            var dir = Path.Join(cacheRoot, kind);
            if (!Directory.Exists(dir)) continue;
            var newest = Directory.EnumerateDirectories(dir).OrderDescending().FirstOrDefault();
            if (newest is not null)
                yield return newest;
        }
    }

    private static string? Run(List<string> args)
    {
        try
        {
            using var cts = new CancellationTokenSource(Timeout);
            var (exitCode, stdout, stderr) = Cli.Wrap("symbolic-crash")
                .WithArguments(args)
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync(cts.Token)
                .GetAwaiter()
                .GetResult();

            if (exitCode != 0)
            {
                Log.Warn("symbolic-crash exited with {ExitCode}: {Stderr}", exitCode, stderr);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(stderr))
                Log.Debug("symbolic-crash stderr: {Stderr}", stderr);

            return stdout;
        }
        catch (OperationCanceledException)
        {
            // CliWrap kills the process tree on cancellation.
            Log.Warn("symbolic-crash timed out after {Timeout}", Timeout);
            return null;
        }
    }
}
