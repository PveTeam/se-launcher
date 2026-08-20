using System.Reflection;
using dnlib.DotNet;
using SharedCringe.Loader;

namespace CringePlugins.Utils;

internal sealed class LoadContextAssemblyResolver(DerivedAssemblyLoadContext loadContext) : AssemblyResolver
{
    protected override IEnumerable<string> PostFindAssemblies(IAssembly assembly, ModuleDef sourceModule, bool matchExactly)
    {
        foreach (var path in base.PostFindAssemblies(assembly, sourceModule, matchExactly))
            yield return path;

        Assembly runtimeAssembly;
        try
        {
            runtimeAssembly = loadContext.LoadFromAssemblyName(new(assembly.Name));
        }
        catch (Exception)
        {
            yield break;
        }

        if (runtimeAssembly.Location is { Length: > 0 } location)
            yield return location;
    }
}
