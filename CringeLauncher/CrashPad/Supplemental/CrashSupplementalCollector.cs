using System.Collections.Immutable;
using NLog;

namespace CringeLauncher.CrashPad.Supplemental;

/// <summary>
/// Selects and runs platform-appropriate <see cref="ICrashSupplementalSource"/> implementations.
/// Sources are independent; failures in one never block others.
/// </summary>
internal sealed class CrashSupplementalCollector(ImmutableArray<ICrashSupplementalSource> sources)
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public static CrashSupplementalCollector CreateDefault() => new(CreateDefaultSources());

    private static ImmutableArray<ICrashSupplementalSource> CreateDefaultSources()
    {
        // Phase A/B are Linux-gated: crashreport.json is Unix-capable in the runtime,
        // but this product only enables and consumes it on Linux.
        if (OperatingSystem.IsLinux())
        {
            return [new SymbolicCrashSource(), new DotnetRuntimeCrashReportSource(), new SystemdCoredumpCrashSource()];
        }

        return [];
    }

    public IReadOnlyList<CrashSupplementalSection> Collect(CrashSupplementalContext context)
    {
        if (sources.IsEmpty)
            return [];

        var sections = new List<CrashSupplementalSection>(sources.Length);
        foreach (var source in sources)
        {
            try
            {
                var section = source.TryCollect(context);
                if (section is null || string.IsNullOrWhiteSpace(section.Body))
                    continue;

                sections.Add(section);
                Log.Info("Supplemental crash source {SourceId} produced section '{Title}'", source.Id, section.Title);
            }
            catch (Exception e)
            {
                // Belt-and-suspenders: sources must not throw, but never break the crash dialog.
                Log.Warn(e, "Supplemental crash source {SourceId} failed", source.Id);
            }
        }

        sections.Sort(static (a, b) => b.Priority.CompareTo(a.Priority));
        return sections;
    }
}
