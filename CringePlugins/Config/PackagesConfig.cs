using System.Collections.Immutable;
using CringePlugins.Resolver;
using NuGet;
using NuGet.Models;

namespace CringePlugins.Config;

public record PackagesConfig(ImmutableArray<PackageSource> Sources, ImmutableArray<PackageReference> Packages)
{
    public static PackagesConfig Default { get; } = new([
        new("SpaceEngineersDedicated.ReferenceAssemblies", "https://nuget.storage.yandexcloud.net/index.json"),
        new("ImGui.NET.DirectX", "https://nuget.storage.yandexcloud.net/index.json"),
        new(string.Empty, "https://api.nuget.org/v3/index.json")
    ], []);
}