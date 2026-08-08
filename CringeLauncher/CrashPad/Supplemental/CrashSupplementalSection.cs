namespace CringeLauncher.CrashPad.Supplemental;

/// <summary>One titled block of supplemental crash text for the crash dialog/report.</summary>
public sealed class CrashSupplementalSection
{
    public required string SourceId { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }

    /// <summary>Higher wins when ranking which summary line to surface first.</summary>
    public int Priority { get; init; }

    /// <summary>Short one-line summary suitable near the top of the report.</summary>
    public string? Summary { get; init; }
}
