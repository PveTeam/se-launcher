using System.Collections.Frozen;
using System.Collections.Immutable;
using CringePlugins.Abstractions.Loader;
using CringePlugins.Utils;
using dnlib.DotNet;

namespace CringePlugins.Loader.ProviderProvider;

internal class PluginProviderPluginProvider : IPluginProvider
{
    public FrozenSet<string> PackageTypes { get; } = ["CringePluginProvider"];

    public IPluginInstance LoadComponent(PluginMetadata metadata, IPluginDependencyContextFactory dependencyContextFactory)
    {
       return new PluginProviderInstance(metadata, dependencyContextFactory);
    }

    public IPluginEntrypoint ResolveEntrypoint(PluginMetadata metadata, ModuleDefMD module)
    {
        var entrypointTypes = IntrospectionContext.Global.CollectDerivedTypeDefinitions<IPluginProvider>(module)
            .ToImmutableArray();

        if (entrypointTypes.Length == 0)
            throw new InvalidOperationException("Entrypoint does not contain any plugins");
        if (entrypointTypes.Length > 1)
            throw new InvalidOperationException("Entrypoint contains multiple plugins");
        
        return new ByTypeNamePluginEntrypoint(entrypointTypes[0].ClrFullName);
    }
}
