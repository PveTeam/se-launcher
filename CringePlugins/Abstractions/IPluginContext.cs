using CringePlugins.Loader;

namespace CringePlugins.Abstractions;

public interface IPluginContext : IServiceProvider
{
    PluginMetadata Metadata { get; }
}