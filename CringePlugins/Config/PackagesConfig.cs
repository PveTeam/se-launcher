using System.Collections.Immutable;
using CringePlugins.Resolver;
using NuGet;
using NuGet.Models;

namespace CringePlugins.Config;

public record PackagesConfig(ImmutableArray<PackageSource> Sources, ImmutableArray<PackageReference> Packages)
{
    public static PackagesConfig Default { get; } = new([
        new(@"^SpaceEngineersDedicated\.ReferenceAssemblies$|^ImGui\.NET\.DirectX$|^NuGet$|^Cringe.+$|^SharedCringe$|^Plugin.+$", "https://ng.zznty.ru/v3/index.json"),
        new(string.Empty, "https://api.nuget.org/v3/index.json")
    ], []);
}