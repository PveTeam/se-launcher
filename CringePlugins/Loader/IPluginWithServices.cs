using CringePlugins.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using VRage.Plugins;

namespace CringePlugins.Loader;

public interface IPluginWithServices : IPlugin
{
    void Init(IPluginContext context);
    void IPlugin.Init(object gameInstance)
    {
        Init((IPluginContext)gameInstance);
    }
    
    static abstract void RegisterServices(IServiceCollection services);
}