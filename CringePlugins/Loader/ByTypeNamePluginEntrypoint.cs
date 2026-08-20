using System.Reflection;
using CringePlugins.Abstractions.Loader;

namespace CringePlugins.Loader;

public record ByTypeNamePluginEntrypoint(string TypeName) : IPluginEntrypoint
{
    public Type GetEntrypointType(Assembly assembly) => assembly.GetType(TypeName, true, false)!;
}
