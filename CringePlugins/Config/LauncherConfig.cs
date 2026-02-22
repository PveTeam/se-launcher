using System.Globalization;

namespace CringePlugins.Config;

public sealed record LauncherConfig(
    bool DisableLauncherUpdates,
    bool DisablePluginUpdates,
    bool UsePreviewBranch = false,
    bool CacheModAssemblies = true,
    bool CacheScriptAssemblies = true,
    string? UpdatesSource = null)
{
    public static LauncherConfig Default => new(false, false);
}

internal record LauncherConfigRegionalDefaults(string UpdatesSource, string NugetSource, string SymbolsSource)
{
    private static readonly LauncherConfigRegionalDefaults GlobalDefaults =
        new("https://dl.zznty.ru/CringeLauncher/", "https://ng.zznty.ru/v3/index.json",
            "https://ng.zznty.ru/api/download/symbols/");

    private static readonly LauncherConfigRegionalDefaults RuDefaults =
        new("https://dl3.zznty.ru/launcher/", "https://ng3.zznty.ru/v3/index.json",
            "https://ng3.zznty.ru/api/download/symbols/");

    public static LauncherConfigRegionalDefaults Current { get; } =
        RegionInfo.CurrentRegion.Equals(new RegionInfo(1049)) ? RuDefaults : GlobalDefaults;
}