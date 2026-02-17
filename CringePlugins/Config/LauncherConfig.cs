namespace CringePlugins.Config;
public sealed record LauncherConfig(
    bool DisableLauncherUpdates,
    bool DisablePluginUpdates,
    bool UsePreviewBranch = false,
    bool CacheModAssemblies = true,
    bool CacheScriptAssemblies = true,
    string UpdatesSource = "https://dl.zznty.ru/CringeLauncher/")
{
    public static LauncherConfig Default => new(false, false);
}
