namespace CringePlugins.Config;
public sealed record LauncherConfig(bool DisableLauncherUpdates, bool DisablePluginUpdates)
{
    public static LauncherConfig Default => new(false, false);
}
