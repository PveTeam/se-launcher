using System.Collections.Frozen;
using CringePlugins.Loader;
using dnlib.DotNet;
using Microsoft.Extensions.DependencyInjection;

namespace CringePlugins.Abstractions.Loader;

public interface IPluginProvider
{
    FrozenSet<string> PackageTypes { get; }

    IPluginInstance? LoadComponent(PluginMetadata metadata, IPluginDependencyContextFactory dependencyContextFactory);

    IPluginEntrypoint ResolveEntrypoint(PluginMetadata metadata, ModuleDefMD module);

    static virtual void RegisterServices(IServiceCollection services)
    {
    }
}
