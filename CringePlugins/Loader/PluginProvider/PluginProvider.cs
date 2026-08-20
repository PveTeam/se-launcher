using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.Loader;
using CringePlugins.Abstractions;
using CringePlugins.Abstractions.Loader;
using CringePlugins.Utils;
using dnlib.DotNet;

namespace CringePlugins.Loader.PluginProvider;

internal class PluginProvider(string packageType) : IPluginProvider
{
    public FrozenSet<string> PackageTypes { get; } = [packageType];

    public IPluginInstance LoadComponent(PluginMetadata metadata, IPluginDependencyContextFactory dependencyContextFactory)
    {
        return new PluginInstance(metadata, dependencyContextFactory);
    }

    public IPluginEntrypoint ResolveEntrypoint(PluginMetadata metadata, ModuleDefMD module)
    {
        var entrypointTypes = IntrospectionContext.Global.CollectDerivedTypeDefinitions<VRage.Plugins.IPlugin>(module)
            .ToImmutableArray();

        if (entrypointTypes.Length == 0)
            throw new InvalidOperationException("Entrypoint does not contain any plugins");
        if (entrypointTypes.Length > 1)
            throw new InvalidOperationException("Entrypoint contains multiple plugins");
        
        return new ByTypeNamePluginEntrypoint(entrypointTypes[0].ClrFullName);
    }
}
