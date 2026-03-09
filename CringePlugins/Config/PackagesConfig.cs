using System.Collections.Immutable;
using CringePlugins.Resolver;
using NuGet;

namespace CringePlugins.Config;

public record PackagesConfig(ImmutableArray<PackageSource> Sources, ImmutableArray<PackageReference> Packages, ImmutableArray<Profile> Profiles)
{
    public static PackagesConfig Default { get; } = new([
            new("CringeLauncher Official",
                @"^SpaceEngineersDedicated\.ReferenceAssemblies$|^ImGui\.NET\.DirectX$|^NuGet$|^Cringe.+$|^SharedCringe$|^Plugin.+$",
                LauncherConfigRegionalDefaults.Current.NugetSource),
            new("nuget.org", string.Empty, "https://api.nuget.org/v3/index.json")
        ],
        [
            new PackageReference("Plugin.ClientModLoader", new(new(0, 0, 0)))
        ],
        []);
}
public record Profile(string Id, ImmutableArray<PackageReference> Plugins);