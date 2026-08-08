namespace CringeLauncher.CrashPad.Supplemental;

/// <summary>Inputs available to supplemental crash sources after the child process exits.</summary>
internal sealed class CrashSupplementalContext
{
    public required int ProcessId { get; init; }
    public required int ExitCode { get; init; }
    public required string DumpPath { get; init; }
    public required string DumpLogPath { get; init; }
    public required string StderrPath { get; init; }

    /// <summary>Executable path the child was launched with, when known.</summary>
    public string? ExecutablePath { get; init; }

    /// <summary>AlcMapper JSONL sidecar path passed to the child, when enabled.</summary>
    public string? AlcMapPath { get; init; }

    /// <summary>Pe-map JSON sidecar path passed to the child, when enabled.</summary>
    public string? PeMapPath { get; init; }

    /// <summary>Cache key of the crossgen service that ran the child (R2R/NOOP/mods/scripts subdir name).</summary>
    public string? CrossGenCacheKey { get; init; }

    /// <summary>UTC timestamp taken immediately after observing process exit.</summary>
    public DateTimeOffset ExitedAtUtc { get; init; } = DateTimeOffset.UtcNow;
}
