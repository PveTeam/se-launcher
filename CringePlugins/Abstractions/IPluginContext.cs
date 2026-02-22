using System.Collections.Immutable;
using CringePlugins.Loader;

namespace CringePlugins.Abstractions;

public interface IPluginContext : IServiceProvider
{
    PluginMetadata Metadata { get; }
    ImmutableDictionary<string, PluginMetadata> Plugins { get; }
}