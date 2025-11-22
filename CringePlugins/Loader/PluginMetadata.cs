using System.Collections.Immutable;
using System.Reflection;
using CringePlugins.Utils;
using dnlib.DotNet;
using NuGet.Versioning;

namespace CringePlugins.Loader;

public record PluginMetadata(string Id, string Name, NuGetVersion Version, string Source)
{
    public required string EntrypointTypeName { get; init; }
    
    public DirectoryInfo? AssetsDirectory { get; init; }
    
    public static PluginMetadata? ReadFromEntrypoint(string entrypointPath)
    {
        var module = ModuleDefMD.Load(entrypointPath, IntrospectionContext.Global.Context);
        var assembly = module.Assembly;

        var titleAttribute = assembly.CustomAttributes.Find(typeof(AssemblyTitleAttribute).FullName);
        var versionAttribute = assembly.CustomAttributes.Find(typeof(AssemblyVersionAttribute).FullName);
        var fileVersionAttribute = assembly.CustomAttributes.Find(typeof(AssemblyFileVersionAttribute).FullName);

        var name = titleAttribute?.ConstructorArguments[0].Value as UTF8String ?? assembly.Name;
        if (!NuGetVersion.TryParse(
                (versionAttribute ?? fileVersionAttribute)?.ConstructorArguments[0].Value as UTF8String ?? "0.0.0.0",
                out var version))
            version = new(0, 0, 0, 0);

        return new(assembly.Name, name, version, "Local")
        {
            EntrypointTypeName = ResolveEntrypointTypeName(module)
        };
    }

    internal static string ResolveEntrypointTypeName(string entrypointPath)
    {
        var module = ModuleDefMD.Load(entrypointPath, IntrospectionContext.Global.Context);
        return ResolveEntrypointTypeName(module);
    }

    internal static string ResolveEntrypointTypeName(ModuleDefMD module)
    {
        var entrypointTypes = IntrospectionContext.Global.CollectDerivedTypeDefinitions<VRage.Plugins.IPlugin>(module)
            .ToImmutableArray();

        if (entrypointTypes.Length == 0)
            throw new InvalidOperationException("Entrypoint does not contain any plugins");
        if (entrypointTypes.Length > 1)
            throw new InvalidOperationException("Entrypoint contains multiple plugins");
        
        return entrypointTypes[0].ClrFullName;
    }
}