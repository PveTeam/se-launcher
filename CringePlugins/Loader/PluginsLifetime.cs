using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text.Json;
using CringePlugins.Config;
using CringePlugins.Render;
using CringePlugins.Resolver;
using CringePlugins.Splash;
using CringePlugins.Ui;
using NLog;
using NuGet;
using NuGet.Deps;
using NuGet.Frameworks;
using NuGet.Versioning;

namespace CringePlugins.Loader;

public class PluginsLifetime : ILoadingStage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public string Name => "Loading Plugins";
    
    private ImmutableArray<PluginInstance> _plugins = [];
    private readonly DirectoryInfo _dir = Directory.CreateDirectory(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CringeLauncher"));
    private readonly NuGetFramework _runtimeFramework = NuGetFramework.ParseFolder("net8.0-windows10.0.19041.0");

    public async ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(6);
        
        progress.Report("Discovering local plugins");
        
        DiscoverLocalPlugins(_dir.CreateSubdirectory("plugins"));
        
        progress.Report("Loading config");

        PackagesConfig? packagesConfig = null;
        var configPath = Path.Join(_dir.FullName, "packages.json");
        if (File.Exists(configPath))
            await using (var stream = File.OpenRead(configPath))
                packagesConfig = JsonSerializer.Deserialize<PackagesConfig>(stream, NuGetClient.SerializerOptions)!;

        if (packagesConfig == null)
        {
            packagesConfig = PackagesConfig.Default;
            await using var stream = File.Create(configPath);
            await JsonSerializer.SerializeAsync(stream, packagesConfig, NuGetClient.SerializerOptions);
        }
        
        progress.Report("Resolving packages");

        var sourceMapping = new PackageSourceMapping(packagesConfig.Sources);
        var resolver = new PackageResolver(_runtimeFramework, packagesConfig.Packages, sourceMapping);

        var packages = await resolver.ResolveAsync();
        
        progress.Report("Downloading packages");
        
        var cachedPackages = await resolver.DownloadPackagesAsync(_dir.CreateSubdirectory("cache"), packages, progress);
        
        progress.Report("Loading plugins");

        await LoadPlugins(cachedPackages, sourceMapping);
        
        progress.Report("Registering plugins");
        
        RegisterLifetime();
        
        RenderHandler.Current.RegisterComponent(new PluginListComponent(packagesConfig, sourceMapping, configPath, _plugins));
    }

    private void RegisterLifetime()
    {
        foreach (var instance in _plugins)
        {
            try
            {
                instance.Instantiate();
                instance.RegisterLifetime();
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to instantiate plugin {Plugin}", instance.Metadata);
            }
        }
    }

    private async Task LoadPlugins(IReadOnlySet<CachedPackage> packages, PackageSourceMapping sourceMapping)
    {
        var plugins = _plugins.ToBuilder();

        var packageVersions = BuiltInPackages.GetPackages(_runtimeFramework)
            .ToImmutableDictionary(b => b.Package.Id, b => b.Package.Version,
                StringComparer.OrdinalIgnoreCase);

        packageVersions = packageVersions.AddRange(packages.Select(b =>
            new KeyValuePair<string, NuGetVersion>(b.Package.Id, b.Package.Version)));

        var manifestBuilder = new DependencyManifestBuilder(_dir.CreateSubdirectory("cache"), sourceMapping,
            dependency => packageVersions.TryGetValue(dependency.Id, out var version) && version.Major != 99
                ? version
                : dependency.Range.MinVersion ?? dependency.Range.MaxVersion);
        
        foreach (var package in packages)
        {
            var dir = Path.Join(package.Directory.FullName, "lib", package.ResolvedFramework.GetShortFolderName());

            await using (var stream = File.Create(Path.Join(dir, $"{package.Package.Id}.deps.json")))
                await manifestBuilder.WriteDependencyManifestAsync(stream, package.Entry, _runtimeFramework);
            
            LoadComponent(plugins, Path.Join(dir, $"{package.Package.Id}.dll"));
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

    private static void LoadComponent(ImmutableArray<PluginInstance>.Builder plugins, string path)
    {
        try
        {
            plugins.Add(new PluginInstance(path));
        }
        catch (Exception e)
        { 
            Log.Error(e, "Failed to load plugin {PluginPath}", path);
        }
    }
}