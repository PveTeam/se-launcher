using System.Reflection;
using dnlib.DotNet;

namespace CringePlugins.Loader;

public record PluginMetadata(string Name, Version Version)
{
    public static PluginMetadata ReadFromEntrypoint(string entrypointPath)
    {
        var module = ModuleDefMD.Load(entrypointPath);

        var titleAttribute = module.CustomAttributes.Find(typeof(AssemblyTitleAttribute).FullName);
        var versionAttribute = module.CustomAttributes.Find(typeof(AssemblyVersionAttribute).FullName);

        var name = titleAttribute?.ConstructorArguments[0].Value as string ?? module.FullName;
        if (!Version.TryParse(versionAttribute?.ConstructorArguments[0].Value as string ?? "0.0.0.0", out var version))
            version = new();
        
        return new(name, version);
    }
}