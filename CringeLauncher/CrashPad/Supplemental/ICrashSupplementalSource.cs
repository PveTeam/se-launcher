namespace CringeLauncher.CrashPad.Supplemental;

/// <summary>
/// OS-/tooling-specific source of crash details beyond managed <see cref="CrashInformation"/>.
/// Implementations must never throw out of <see cref="TryCollect"/>; permission and missing-tool failures return null.
/// </summary>
internal interface ICrashSupplementalSource
{
    /// <summary>Stable id for logging (e.g. dotnet-crashreport, systemd-coredump).</summary>
    string Id { get; }

    /// <summary>
    /// Collect supplemental crash details for the given context.
    /// Returns null when the source has nothing useful or is unavailable.
    /// </summary>
    CrashSupplementalSection? TryCollect(CrashSupplementalContext context);
}
