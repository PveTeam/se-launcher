using CringePlugins.Splash;

namespace CringePlugins.Loader;

internal interface IPluginsLifetime : ILoadingStage
{
    void RegisterLifetime();
}