using System.Collections.Immutable;
using System.Reflection;
using Basic.Reference.Assemblies;
using CringePlugins.Loader;
using CringePlugins.Utils;
using dnlib.DotNet;
using ImGuiNET;
using Microsoft.CodeAnalysis;
using NLog;
using NuGet.Frameworks;
using NuGet.Models;
using NuGet.Versioning;
using Sandbox.Game;
using SpaceEngineers.Game;
using VRage.Utils;

namespace CringePlugins.Resolver;

public static class BuiltInPackages
{
    private const string SeReferenceAssemblies = "SpaceEngineersDedicated.ReferenceAssemblies";
    private const string ImGui = "ImGui.NET.DirectX";
    private const string Harmony = "Lib.Harmony.Thin";
    private const string Steamworks = "Steamworks.NET";

    public static ImmutableArray<ResolvedPackage> GetPackages(NuGetFramework runtimeFramework)
    {
        var nlog = FromAssembly<LogFactory>(runtimeFramework, version: new(5, 3, 4));
        Version seVersion = new MyVersion(MyPerGameSettings.BasicGameInfo.GameVersion!.Value);
        
        var se = FromAssembly<SpaceEngineersGame>(runtimeFramework, [
            nlog.AsDependency()
        ], SeReferenceAssemblies, new(seVersion));
        var imGui = FromAssembly<ImGuiKey>(runtimeFramework, id: ImGui);
        var harmony = FromAssembly<HarmonyLib.Harmony>(runtimeFramework, id: Harmony);
        var steam = FromAssembly<Steamworks.CSteamID>(runtimeFramework, id: Steamworks);

        BuiltInSdkPackage MapSdkPackage(
            (string FileName, byte[] ImageBytes, PortableExecutableReference Reference, Guid Mvid) r)
        {
            var def = ModuleDefMD.Load(r.ImageBytes, IntrospectionContext.Global.Context);
            var attribute = def.CustomAttributes.Find(typeof(AssemblyFileVersionAttribute).FullName);
            var version = attribute is null ? new(99, 0, 0) : NuGetVersion.Parse((string)attribute.ConstructorArguments[0].Value);
            
            return new BuiltInSdkPackage(
                new(0, Path.GetFileNameWithoutExtension(r.FileName), version), runtimeFramework,
                new(Path.GetFileNameWithoutExtension(r.FileName), version, [new(runtimeFramework, [])], null, []));
        }

        return
        [
            ..Net90.ReferenceInfos.AllValues.Select(MapSdkPackage),
            // ..Net80Windows.ReferenceInfos.AllValues.Select(MapSdkPackage),
            nlog,
            se,
            imGui,
            harmony,
            steam,
            FromAssembly<PluginsLifetime>(runtimeFramework,
                [se.AsDependency(), imGui.AsDependency(), harmony.AsDependency()]
#if DEBUG
                , version: new(0, 1, 21)
#endif
            ),
        ];
    }
    
    private static Dependency AsDependency(this ResolvedPackage package) => new(package.Package.Id, new(package.Package.Version));

    private static BuiltInPackage FromAssembly<T>(NuGetFramework runtimeFramework, ImmutableArray<Dependency>? dependencies = null, string? id = null, NuGetVersion? version = null)
    {
        var assembly = typeof(T).Assembly.GetName();
        id ??= assembly.Name!;
        version ??= new NuGetVersion(assembly.Version ?? new(0, 0, 0));
        dependencies ??= [];

        return new(
            new(0, id, version),
            runtimeFramework,
            new(id, version, [
                new(runtimeFramework, dependencies.Value)
            ], null, [])
        );
    }
}

public record BuiltInPackage(Package Package, NuGetFramework ResolvedFramework, CatalogEntry Entry) : ResolvedPackage(Package, ResolvedFramework, Entry);

public record BuiltInSdkPackage(Package Package, NuGetFramework ResolvedFramework, CatalogEntry Entry) : BuiltInPackage(Package, ResolvedFramework, Entry);