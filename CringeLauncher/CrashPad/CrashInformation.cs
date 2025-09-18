namespace CringeLauncher.CrashPad;

public class CrashInformation
{
    public required NetworkConnectivity Network { get; init; }
    
    public required HashSet<InstalledPlugin> Plugins { get; init; }

    public required List<ModScript> ModScripts { get; init; }

    public required VersionInformation Version { get; init; }
    
    public ExceptionInformation? UnhandledException { get; set; }
    
    public class NetworkConnectivity
    {
        public bool CheckUpdatesFailed { get; set; }
        public bool NugetSourceFailed { get; set; }
    }

    public class VersionInformation
    {
        public string LauncherVersion { get; set; } = "unset";
        public string UpdatesChannel { get; set; } = "unset";
        public string GameVersion { get; set; } = "unset";
    }

    public record InstalledPlugin(string Name, string Version, string Source)
    {
        public ExceptionInformation.ExceptionFrame? Exception { get; set; }
    };

    public record ModScript(string Name, bool Cached, string? CompilationError);
}