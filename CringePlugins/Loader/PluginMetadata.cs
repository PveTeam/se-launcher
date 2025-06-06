using System.Reflection;
using dnlib.DotNet;
using NuGet.Versioning;

namespace CringePlugins.Loader;

public record PluginMetadata(string Name, NuGetVersion Version, string Source)
{
    public static PluginMetadata ReadFromEntrypoint(string entrypointPath)
    {
        var assembly = AssemblyDef.Load(entrypointPath);

        var titleAttribute = assembly.CustomAttributes.Find(typeof(AssemblyTitleAttribute).FullName);
        var versionAttribute = assembly.CustomAttributes.Find(typeof(AssemblyVersionAttribute).FullName);
        var fileVersionAttribute = assembly.CustomAttributes.Find(typeof(AssemblyFileVersionAttribute).FullName);

        var name = titleAttribute?.ConstructorArguments[0].Value as UTF8String ?? assembly.Name;
        if (!NuGetVersion.TryParse(
                (versionAttribute ?? fileVersionAttribute)?.ConstructorArguments[0].Value as UTF8String ?? "0.0.0.0",
                out var version))
            version = new(0, 0, 0, 0);

        return new(name, version, "Local");
    }
}