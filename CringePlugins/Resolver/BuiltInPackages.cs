using System.Collections.Immutable;
using System.Reflection;
using Basic.Reference.Assemblies;
using CringePlugins.Loader;
using CringePlugins.Utils;
using dnlib.DotNet;
using ImGuiNET;
using Microsoft.CodeAnalysis;
using NLog;
using NuGet.Deps;
using NuGet.Frameworks;
using NuGet.Models;
using NuGet.Versioning;
using Sandbox.Game;
using SpaceEngineers.Game;
using VRage.Utils;
using Dependency = NuGet.Models.Dependency;

namespace CringePlugins.Resolver;

public static class BuiltInPackages
{
    private const string SeReferenceAssemblies = "SpaceEngineersDedicated.ReferenceAssemblies";
    private const string ImGui = "ImGui.NET.DirectX";
    private const string Harmony = "Lib.Harmony.Thin";
    private const string Steamworks = "Steamworks.NET";
    private const string NLog = "NLog";

    public static async ValueTask<ImmutableDictionary<string, ResolvedPackage>> GetPackagesAsync(NuGetRuntimeFramework runtimeFramework)
    {
        ImmutableDictionary<ManifestPackageKey, DependencyLibrary> libraries;
        await using (var stream = File.OpenRead(Path.ChangeExtension(Assembly.GetEntryAssembly()!.Location, "deps.json")))
            (_, _, _, libraries) = await DependencyManifestSerializer.DeserializeAsync(stream);

        var framework = runtimeFramework.Framework;

        var nlog = FromAssembly<LogFactory>(framework, version: libraries.Keys.Single(b => b.Id == NLog).Version);
        Version seVersion = new MyVersion(MyPerGameSettings.BasicGameInfo.GameVersion!.Value);

        var se = FromAssembly<SpaceEngineersGame>(framework, [
            nlog.AsDependency(libraries)
        ], SeReferenceAssemblies, new(seVersion));
        var imGui = FromAssembly<ImGuiKey>(framework, id: ImGui);
        var harmony = FromAssembly<HarmonyLib.Harmony>(framework, id: Harmony, version: NuGetVersion.Parse("2.3.4-torch"));
        var steam = FromAssembly<Steamworks.CSteamID>(framework, id: Steamworks);

        BuiltInSdkPackage MapSdkPackage(
            (string FileName, byte[] ImageBytes, PortableExecutableReference Reference, Guid Mvid) r)
        {
            var def = ModuleDefMD.Load(r.ImageBytes, IntrospectionContext.Global.Context);
            var attribute = def.CustomAttributes.Find(typeof(AssemblyFileVersionAttribute).FullName);
            var version = attribute is null ? new(99, 0, 0) : NuGetVersion.Parse((string)attribute.ConstructorArguments[0].Value);

            return new BuiltInSdkPackage(
                new(0, Path.GetFileNameWithoutExtension(r.FileName), version), framework,
                new(Path.GetFileNameWithoutExtension(r.FileName), version, [new(framework, [])], null, []));
        }

        BuiltInPackage MapPackage(ManifestPackageKey key)
        {
            return new(new(0, key.Id, key.Version), framework,
                new(key.Id, key.Version, [new(framework, [])], null, []));
        }

        ResolvedPackage[] packages =
        [
            ..Net90.ReferenceInfos.AllValues.Select(MapSdkPackage),
            se,

            ..libraries.Where(kvp =>
            {
                if (kvp.Value.Type != LibraryType.Package) return false;

                // Special case as we want to claim we have currently running version of package
                // so that even if launcher is built with older version, plugins could still take explicit dependency on it
                if (kvp.Key.Id == SeReferenceAssemblies) return false;

                return true;
            }).Select(kvp => MapPackage(kvp.Key)),

            // CringePlugins package itself
            FromAssembly<PluginsLifetime>(framework,
                [
                    se.AsDependency(libraries),
                    imGui.AsDependency(libraries),
                    harmony.AsDependency(libraries),
                    steam.AsDependency(libraries)
                ]
#if DEBUG
                , version: new(0, 1, 84)
#endif
            ),
        ];

        var builder = ImmutableDictionary.CreateBuilder<string, ResolvedPackage>();
        foreach (var package in packages)
            builder.TryAdd(package.Package.Id, package);

        return builder.ToImmutable();
    }

    private static Dependency AsDependency(this ResolvedPackage package, ImmutableDictionary<ManifestPackageKey, DependencyLibrary> libraries)
    {
        //ignore the SE reference because the game can update without a launcher update
        if (package.Entry.Id != SeReferenceAssemblies && !libraries.ContainsKey(new(package.Package.Id, package.Package.Version)))
            throw new KeyNotFoundException($"Package {package.Package} not found in root dependencies manifest");

        return new Dependency(package.Package.Id, new(package.Package.Version));
    }

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