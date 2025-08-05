namespace CringePlugins.Config;
public sealed record LauncherConfig(bool DisableLauncherUpdates, bool DisablePluginUpdates, bool CacheModAssemblies = true, bool CacheScriptAssemblies = true)
{
    public static LauncherConfig Default => new(false, false, true, true);
}
