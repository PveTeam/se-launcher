namespace CringePlugins.Config;
public sealed record LauncherConfig(bool DisableLauncherUpdates, bool DisablePluginUpdates, bool UsePreviewBranch = false, bool CacheModAssemblies = true, bool CacheScriptAssemblies = true)
{
    public static LauncherConfig Default => new(false, false);
}
