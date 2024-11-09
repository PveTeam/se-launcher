using CringePlugins.Config;
using CringePlugins.Resolver;
using NuGet.Versioning;
using System.Collections.Immutable;
using System.Xml.Serialization;

namespace CringePlugins.Compatability;

[XmlType("PluginConfig")]
public class PluginLoaderConfig
{
    [XmlArrayItem("Id")]
    public string[] Plugins { get; set; } = [];

    [XmlArrayItem("Profile")]
    public PluginLoaderProfile[] Profiles { get; set; } = [];

    public PackagesConfig Migrate(PackagesConfig old)
    {
        var pluginsBuilder = ImmutableArray.CreateBuilder<PackageReference>();
        var defaultVersion = new NuGetVersion(1, 0, 0);
        foreach (var plugin in Plugins)
        {
            if (!IsValidId(plugin))
                continue;

            pluginsBuilder.Add(new PackageReference($"Plugin.{plugin.Replace('/', '.')}",
                new(defaultVersion)));
        }

        var profiles = new Dictionary<string, ImmutableArray<PackageReference>>();
        foreach (var profile in Profiles)
        {
            var builder = ImmutableArray.CreateBuilder<PackageReference>();
            foreach (var plugin in profile.Plugins)
            {
                if (!IsValidId(plugin))
                    continue;

                builder.Add(new PackageReference($"Plugin.{plugin.Replace('/', '.')}",
                new(defaultVersion)));
            }

            profiles[profile.Name] = builder.ToImmutable();
        }

        return old with
        {
            Packages = pluginsBuilder.ToImmutableArray(),
            Profiles = profiles
        };
    }

    private static bool IsValidId(string pluginId)
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
