using System.Collections.Immutable;
using CringePlugins.Loader;
using SharedCringe.Loader;

namespace CringePlugins.Abstractions.Loader;

public interface IPluginInstance : IEquatable<IPluginInstance>
{
    bool HasConfig { get; }
    bool IsReloading { get; }
    bool IsLocal { get; }
    PluginMetadata Metadata { get; }
    IPluginDependencyContext Context { get; }
    void Instantiate();
    void RegisterLifetime();
    void OpenConfig();
    Task ReloadAsync();
}
