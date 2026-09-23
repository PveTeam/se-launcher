using System.Collections.Frozen;
using System.Collections.Immutable;
using CringePlugins.Loader;
using dnlib.DotNet;
using Microsoft.Extensions.DependencyInjection;
using SharedCringe.Abstractions.Transformers;

namespace CringePlugins.Abstractions.Loader;

public interface IPluginProvider
{
    FrozenSet<string> PackageTypes { get; }

    IPluginInstance? LoadComponent(PluginMetadata metadata, IPluginDependencyContextFactory dependencyContextFactory);

    IPluginEntrypoint ResolveEntrypoint(PluginMetadata metadata, ModuleDefMD module);

    ImmutableArray<ITransformer> PrepareTransformers(PluginMetadata metadata) => [];

    static virtual void RegisterServices(IServiceCollection services)
    {
    }
}
