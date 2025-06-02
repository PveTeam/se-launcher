using System.Collections.Immutable;
using System.Runtime.InteropServices;
using CringePlugins.Config;
using CringePlugins.Render;
using CringePlugins.Resolver;
using CringePlugins.Splash;
using CringePlugins.Ui;
using NLog;
using NuGet;
using NuGet.Deps;
using NuGet.Frameworks;
using NuGet.Models;
using SharedCringe.Loader;
using VRage.FileSystem;

namespace CringePlugins.Loader;

internal class PluginsLifetime(ConfigHandler configHandler, HttpClient client) : IPluginsLifetime
{
    public static ImmutableArray<DerivedAssemblyLoadContext> Contexts { get; private set; } = [];

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public string Name => "Loading Plugins";
    
    private ImmutableArray<PluginInstance> _plugins = [];
    private readonly DirectoryInfo _dir = Directory.CreateDirectory(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CringeLauncher"));
    private readonly NuGetRuntimeFramework _runtimeFramework = new(NuGetFramework.ParseFolder("net9.0-windows10.0.19041.0"), RuntimeInformation.RuntimeIdentifier);
    
    private ConfigReference<PackagesConfig>? _configReference;

    public async ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(6);

        progress.Report("Discovering local plugins");

        DiscoverLocalPlugins(_dir.CreateSubdirectory("plugins"));

        progress.Report("Loading config");

        _configReference = configHandler.RegisterConfig("packages", PackagesConfig.Default);
        var packagesConfig = _configReference.Value;

        progress.Report("Resolving packages");

        var sourceMapping = new PackageSourceMapping(packagesConfig.Sources, client);
        // TODO take into account the target framework runtime identifier
        var resolver = new PackageResolver(_runtimeFramework.Framework, packagesConfig.Packages, sourceMapping);

        var packages = await resolver.ResolveAsync();

        progress.Report("Downloading packages");

        var builtInPackages = await BuiltInPackages.GetPackagesAsync(_runtimeFramework);
        var cachedPackages = await resolver.DownloadPackagesAsync(_dir.CreateSubdirectory("cache"), packages, builtInPackages.Keys.ToHashSet(), progress);

        progress.Report("Loading plugins");

        //we can move this, but it should be before plugin init
        RenderHandler.Current.RegisterComponent(new NotificationsComponent());

        await LoadPlugins(cachedPackages, sourceMapping, packagesConfig, builtInPackages);

        RenderHandler.Current.RegisterComponent(new PluginListComponent(_configReference, sourceMapping, MyFileSystem.ExePath, _plugins));
    }

    public void RegisterLifetime()
    {
        var contextBuilder = Contexts.ToBuilder();
        foreach (var instance in _plugins)
        {
            try
            {
                instance.Instantiate(contextBuilder);
                instance.RegisterLifetime();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to instantiate plugin {Plugin}", instance.Metadata);
            }
        }
        Contexts = contextBuilder.ToImmutable();
    }

    private async Task LoadPlugins(IReadOnlyCollection<CachedPackage> packages, PackageSourceMapping sourceMapping,
        PackagesConfig packagesConfig, ImmutableDictionary<string, ResolvedPackage> builtInPackages)
    {
        var plugins = _plugins.ToBuilder();
        
        var resolvedPackages = builtInPackages.ToDictionary();
        foreach (var package in packages)
        {
            resolvedPackages.TryAdd(package.Package.Id, package);
        }

        var manifestBuilder = new DependencyManifestBuilder(_dir.CreateSubdirectory("cache"), sourceMapping,
            dependency =>
            {
                resolvedPackages.TryGetValue(dependency.Id, out var package);
                return package?.Entry;
            });
        
        foreach (var package in packages)
        {
            if (builtInPackages.ContainsKey(package.Package.Id)) continue;

            var client = await sourceMapping.GetClientAsync(package.Package.Id);

            if (client == null)
            {
                Log.Warn("Client not found for {Package}", package.Package.Id);
                continue;
            }

            var dir = Path.Join(package.Directory.FullName, "lib", package.ResolvedFramework.GetShortFolderName());

            var path = Path.Join(dir, $"{package.Package.Id}.deps.json");
            if (!File.Exists(path))
            {
                try
                {
                    await using var stream = File.Create(path);

                    //client should not be null for calls to this
                    await manifestBuilder.WriteDependencyManifestAsync(stream, package.Entry, _runtimeFramework);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Failed to write dependency manifest for {path}");
                    File.Delete(path); //delete file to avoid breaking cache
                    throw;
                }
            }

            var sourceName = packagesConfig.Sources.First(b => b.Url == client.ToString()).Name;
            LoadComponent(plugins, Path.Join(dir, $"{package.Package.Id}.dll"),
                new(package.Package.Id, package.Package.Version, sourceName));
        }
        
        _plugins = plugins.ToImmutable();
    }

    private void DiscoverLocalPlugins(DirectoryInfo dir)
    {
        var plugins = ImmutableArray<PluginInstance>.Empty.ToBuilder();
        
        foreach (var directory in dir.EnumerateDirectories())
        {
            var files = directory.GetFiles("*.deps.json");
            
            if (files.Length != 1) continue;
            
            var path = files[0].FullName[..^".deps.json".Length] + ".dll";
            
            LoadComponent(plugins, path);
        }
        
        _plugins = plugins.ToImmutable();
    }

    private static void LoadComponent(ImmutableArray<PluginInstance>.Builder plugins, string path, PluginMetadata? metadata = null)
    {
        try
        {
            plugins.Add(metadata is null ? new PluginInstance(path) : new(metadata, path));
        }
        catch (Exception e)
        { 
            Log.Error(e, "Failed to load plugin {PluginPath}", path);
        }
    }
}