using CringePlugins.Config;
using CringePlugins.Resolver;
using NuGet.Versioning;
using System.Collections.Immutable;
using System.Xml.Serialization;

namespace CringePlugins.Compatability;

[XmlType("PluginConfig")]
public class PluginLoaderConfig
{
    /// <summary>
    /// Raw plugin and mod ids
    /// </summary>
    [XmlArrayItem("Id")] public string[] Plugins { get; set; } = [];

    /// <summary>
    /// Raw profiles
    /// </summary>
    [XmlArrayItem("Profile")] public PluginLoaderProfile[] Profiles { get; set; } = [];

    public PackagesConfig MigratePlugins(PackagesConfig old)
    {
        //ensure defaults are installed
        var defaultConfig = PackagesConfig.Default;
        var sources = old.Sources.ToBuilder();
        foreach (var source in defaultConfig.Sources)
        {
            if (!sources.Contains(source))
                sources.Add(source);
        }

        var pluginsBuilder = ImmutableArray.CreateBuilder<PackageReference>();
        var defaultVersion = new NuGetVersion(1, 0, 0);
        foreach (var plugin in GetPlugins())
        {
            pluginsBuilder.Add(new PackageReference($"Plugin.{plugin.Replace('/', '.')}",
                new(defaultVersion)));
        }
        foreach (var package in defaultConfig.Packages)
        {
            if (!pluginsBuilder.Any(x => x.Id == package.Id))
                pluginsBuilder.Add(package);
        }

        var profiles = ImmutableArray.CreateBuilder<Profile>();
        foreach (var profile in Profiles)
        {
            var builder = ImmutableArray.CreateBuilder<PackageReference>();
            foreach (var plugin in profile.Plugins)
            {
                if (!IsValidPluginId(plugin))
                    continue;

                builder.Add(new PackageReference($"Plugin.{plugin.Replace('/', '.')}",
                new(defaultVersion)));
            }

            foreach (var package in defaultConfig.Packages)
            {
                if (!builder.Any(x => x.Id == package.Id))
                    builder.Add(package);
            }

            profiles.Add(new(profile.Name, builder.ToImmutable()));
        }

        return old with
        {
            Packages = pluginsBuilder.ToImmutable(),
            Profiles = profiles.ToImmutable(),
            Sources = sources.ToImmutable()
        };
    }

    public HashSet<string> GetPlugins() => GetPlugins(Plugins);
    public HashSet<ulong> GetMods() => GetMods(Plugins);
    public Dictionary<string, HashSet<string>> GetPluginProfiles()
    {
        var dict = new Dictionary<string, HashSet<string>>(Profiles.Length);

        foreach (var profile in Profiles)
        {
            dict[profile.Name] = GetPlugins(profile.Plugins);
        }

        return dict;
    }
    public Dictionary<string, HashSet<ulong>> GetModProfiles()
    {
        var dict = new Dictionary<string, HashSet<ulong>>(Profiles.Length);

        foreach (var profile in Profiles)
        {
            dict[profile.Name] = GetMods(profile.Plugins);
        }

        return dict;
    }


    private static HashSet<string> GetPlugins(string[] mixed)
    {
        var plugins = new HashSet<string>();
        foreach (var plugin in mixed)
        {
            if (!IsValidPluginId(plugin))
                continue;

            plugins.Add(plugin);
        }

        return plugins;
    }

    private static HashSet<ulong> GetMods(string[] mixed)
    {
        var mods = new HashSet<ulong>();
        foreach (var plugin in mixed)
        {
            if (!ulong.TryParse(plugin, out var modId))
                continue;

            mods.Add(modId);
        }

        return mods;
    }

    private static bool IsValidPluginId(string pluginId)
    {
        var count = 0;
        foreach (var c in pluginId)
        {
            if (c != '/')
                continue;

            count++;

            if (count > 1)
                return false;
        }

        return count == 1;
    }
}
