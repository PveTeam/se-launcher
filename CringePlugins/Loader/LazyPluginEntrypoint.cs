using System.Reflection;
using System.Runtime.Loader;
using CringePlugins.Abstractions.Loader;
using CringePlugins.Utils;
using SharedCringe.Loader;

namespace CringePlugins.Loader;

internal class LazyPluginEntrypoint(PluginMetadata metadata, IPluginProvider provider) : IPluginEntrypoint
{
    public Type GetEntrypointType(Assembly assembly)
    {
        IntrospectionContext context;
        if (AssemblyLoadContext.GetLoadContext(assembly) is DerivedAssemblyLoadContext alc)
            context = new(alc);
        else context = IntrospectionContext.Global;

        return provider.ResolveEntrypoint(metadata, context.Load(assembly.GetMainModule())).GetEntrypointType(assembly);
    }
}
