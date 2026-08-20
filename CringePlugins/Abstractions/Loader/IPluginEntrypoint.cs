using System.Reflection;

namespace CringePlugins.Abstractions.Loader;

public interface IPluginEntrypoint
{
    Type GetEntrypointType(Assembly assembly);
}
