using System.Reflection;
using System.Runtime.Loader;
using dnlib.DotNet;

namespace SharedCringe.Utils;

internal sealed class LoadContextAssemblyResolver(AssemblyLoadContext loadContext) : AssemblyResolver
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
