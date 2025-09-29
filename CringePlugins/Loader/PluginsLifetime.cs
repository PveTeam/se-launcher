using System.Collections.Immutable;
using System.Runtime.InteropServices;
using CringePlugins.Abstractions;
using CringePlugins.Config;
using CringePlugins.Render;
using CringePlugins.Resolver;
using CringePlugins.Splash;
using CringePlugins.Ui;
using CringePlugins.Utils;
using Microsoft.TemplateEngine.Utils;
using NLog;
using NuGet;
using NuGet.Deps;
using NuGet.Frameworks;
using NuGet.Models;
using SharedCringe.Loader;
using VRage.FileSystem;
using Dependency = NuGet.Models.Dependency;

namespace CringePlugins.Loader;

internal class PluginsLifetime(ConfigHandler configHandler, IPluginServiceProviderFactory serviceProviderFactory, HttpClient client, DirectoryInfo dir) : IPluginsLifetime
{
    public static ImmutableArray<DerivedAssemblyLoadContext> Contexts { get; private set; } = [];
    private static readonly Lock ContextsLock = new();

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public string Name => "Loading Plugins";

    internal ImmutableArray<PluginInstance> Plugins = [];
    internal bool SomeSourcesAreUnavailable { get; private set; }

    private readonly NuGetRuntimeFramework _runtimeFramework =
        new(NuGetFramework.ParseFolder("net9.0-windows10.0.19041.0"), RuntimeInformation.RuntimeIdentifier);

    private ConfigReference<PackagesConfig>? _configReference;
    private ConfigReference<LauncherConfig>? _launcherConfig;

    public async ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(6);

        progress.Report("Discovering local plugins");

#if DEBUG
        // await Task.Delay(10000);
#endif

        var (localPlugins, localRequestedReferences) = await DiscoverLocalPlugins(dir.CreateSubdirectory("plugins"));

        progress.Report("Loading config");

        _configReference = configHandler.RegisterConfig("packages", PackagesConfig.Default);
        _launcherConfig = configHandler.RegisterConfig("launcher", LauncherConfig.Default);
        var packagesConfig = _configReference.Value;
        var launcherConfig = _launcherConfig.Value;

        progress.Report("Resolving packages");

        var sourceMapping = new PackageSourceMapping(packagesConfig.Sources, client);
        // TODO take into account the target framework runtime identifier
        var resolver = new PackageResolver(_runtimeFramework.Framework, [..packagesConfig.Packages, ..localRequestedReferences], sourceMapping);

        var cacheDir = dir.CreateSubdirectory("cache");
        
        InitializeSharedStore(ref cacheDir);

        var invalidPackages = new List<PackageReference>();
        var builtInPackages = await BuiltInPackages.GetPackagesAsync(_runtimeFramework);
        var builtInPackageIds = builtInPackages.Keys.ToHashSet();
        var packages = await resolver.ResolveAsync(cacheDir, launcherConfig.DisablePluginUpdates, builtInPackageIds, invalidPackages);

        if (invalidPackages.Count > 0)
        {
            var builder = packagesConfig.Packages.ToBuilder();

            foreach (var package in invalidPackages)
            {
                builder.Remove(package);
            }

            _configReference.Value = packagesConfig with { Packages = builder.ToImmutable() };
            packagesConfig = _configReference.Value;
            Log.Warn("Removed {Count} invalid packages from the config", invalidPackages.Count);
        }

        progress.Report("Downloading packages");

        var cachedPackages =
            await PackageResolver.DownloadPackagesAsync(cacheDir, packages, builtInPackageIds, progress);

        progress.Report("Loading plugins");

        //we can move this, but it should be before plugin init
        RenderHandler.Current.RegisterComponent(new NotificationsComponent());

        var loadedPackages = cachedPackages.Concat(localPlugins)
            .ToDictionary(b => b.Package.Id, StringComparer.OrdinalIgnoreCase);
        await LoadPlugins(loadedPackages.Values, sourceMapping, packagesConfig, builtInPackages, cacheDir);

        RenderHandler.Current.RegisterComponent(new PluginListComponent(_configReference, _launcherConfig,
            sourceMapping, MyFileSystem.ExePath, Plugins, dir, cacheDir, loadedPackages));

        SomeSourcesAreUnavailable = sourceMapping.SomeSourcesAreUnavailable;
    }

    public static async Task ReloadPluginAsync(PluginInstance instance)
    {
        try
        {
            var (oldContext, newContext) = await instance.ReloadAsync();

            using (ContextsLock.EnterScope())
            {
                Contexts = Contexts.Replace(oldContext, newContext);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to reload plugin {Plugin}", instance.Metadata);
        }
    }

    public void RegisterLifetime()
    {
        var contextBuilder = Contexts.ToBuilder();
        foreach (var instance in Plugins)
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
        PackagesConfig packagesConfig, ImmutableDictionary<string, ResolvedPackage> builtInPackages, DirectoryInfo cacheDir)
    {
        var plugins = Plugins.ToBuilder();

        var resolvedPackages = builtInPackages.ToDictionary();
        foreach (var package in packages)
        {
            resolvedPackages.TryAdd(package.Package.Id, package);
        }

        var manifestBuilder = new DependencyManifestBuilder(cacheDir, sourceMapping,
            dependency =>
            {
                resolvedPackages.TryGetValue(dependency.Id, out var package);
                return package?.Entry;
            });

        var pluginPackages = packages.Where(package =>
                !builtInPackages.ContainsKey(package.Package.Id) && package.Entry.PackageTypes is ["CringePlugin"])
            .ToImmutableArray();

        var dependenciesMap = pluginPackages.OfType<ResolvedPackage>().ToDictionary(b => b, b =>
        {
            if (b.Entry.DependencyGroups is null or []) return [];

            var nearest = NuGetFrameworkUtility.GetNearest(b.Entry.DependencyGroups.Value,
                _runtimeFramework.Framework,
                g => g.TargetFramework);

            if (nearest?.Dependencies is null or [])
                return [];

            return nearest.Dependencies.Value.Select(p =>
            {
                resolvedPackages.TryGetValue(p.Id, out var package);
                return package;
            }).Where(p => p is { Entry.PackageTypes: ["CringePlugin"] }).ToHashSet();
        });

        foreach (var subGraph in DependenciesUtils.SplitIntoSubGraphs(dependenciesMap!))
        {
            DirectedGraph<ResolvedPackage> graph = subGraph;
            if (!graph.TryGetTopologicalSort(out var sortedElements))
                throw new Exception("Plugin dependency cycle detected");

            PluginInstance? parent = null;
            var anyLoaded = false;
            foreach (var package in sortedElements.OfType<CachedPackage>().Where(b => b.Entry.PackageTypes is ["CringePlugin"]))
            {
                anyLoaded = true;
                var packageClient = await sourceMapping.GetClientAsync(package.Package.Id);

                var packageDir = package is LocalPackage
                    ? package.Directory.FullName
                    : Path.Join(package.Directory.FullName, "lib",
                        package.ResolvedFramework.GetShortFolderName());

                var path = Path.Join(packageDir, $"{package.Package.Id}.deps.json");
                if (!File.Exists(path))
                {
                    if (packageClient == null)
                    {
                        Log.Warn("No package source found for {Package}, cannot generate dependency manifest",
                            package.Package.Id);
                        continue;
                    }

                    try
                    {
                        await using var stream = File.Create(path);

                        //client should not be null for calls to this
                        //filter out plugins from the dependency tree so they're loaded as port of their own trees
                        await manifestBuilder.WriteDependencyManifestAsync(stream, package.Entry, _runtimeFramework,
                            entry => entry.PackageTypes is not ["CringePlugin"]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Failed to write dependency manifest for {path}");
                        File.Delete(path); //delete file to avoid breaking cache
                        throw;
                    }
                }

                var sourceName = package is LocalPackage
                    ? "Local"
                    : packageClient == null
                        ? "Local Cache"
                        : packagesConfig.Sources.First(b => b.Url == packageClient.ToString()).Name;
                parent = LoadComponent(plugins, Path.Join(packageDir, $"{package.Package.Id}.dll"),
                    new(package.Package.Id, package.Entry.Title ?? package.Package.Id, package.Package.Version, sourceName),
                    parent: parent);
            }
            
            if (anyLoaded)
                Log.Info("Topological Sorted Leaf: {Leaf}",
                    string.Join(", ", sortedElements.Select(b => b.Entry.Title ?? b.Entry.Id)));
        }
        
        Plugins = plugins.ToImmutable();
    }

    private async ValueTask<(ImmutableArray<CachedPackage> localPlugins, ImmutableArray<PackageReference> references)> DiscoverLocalPlugins(DirectoryInfo dir)
    {
        var localPlugins = ImmutableArray.CreateBuilder<CachedPackage>();
        var references = ImmutableArray.CreateBuilder<PackageReference>();
        
        foreach (var directory in Environment.GetEnvironmentVariable("DOTNET_USERDEV_PLUGINDIR") is { } userDevPlugin
                     ? [new(userDevPlugin), ..dir.GetDirectories()]
                     : dir.EnumerateDirectories())
        {
            var files = directory.GetFiles("*.deps.json");

            if (files.Length != 1) continue;

            var path = files[0].FullName[..^".deps.json".Length] + ".dll";

            var metadata = PluginMetadata.ReadFromEntrypoint(path);
            
            await using var stream = files[0].OpenRead();
            var manifest = await DependencyManifestSerializer.DeserializeAsync(stream);

            var packageReferences = manifest.Libraries.Where(b => b.Value.Serviceable)
                .Select(b =>
                {
                    var ((id, version), _) = b;
                    return new PackageReference(id, new(version));
                }).ToArray();
            references.AddRange(packageReferences);

            var package = new Package(0, metadata.Id, metadata.Version);
            var entry = new CatalogEntry(metadata.Id, metadata.Version, [
                new DependencyGroup(_runtimeFramework.Framework,
                    [..packageReferences.Select(b => new Dependency(b.Id, b.Range))])
            ], ["CringePlugin"], [], metadata.Name);
            
            localPlugins.Add(new LocalPackage(package, _runtimeFramework.Framework, directory, entry));
        }
        
        return (localPlugins.ToImmutable(), references.ToImmutable());
    }

    private PluginInstance? LoadComponent(ImmutableArray<PluginInstance>.Builder plugins, string path,
        PluginMetadata metadata, bool local = false, PluginInstance? parent = null)
    {
        try
        {
            var instance = new PluginInstance(metadata, path, local, serviceProviderFactory, parent);
            plugins.Add(instance);
            return instance;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load plugin {PluginPath}", path);
            return null;
        }
    }

    // initializes dotnet shared store for plugin resolver to look for dependencies
    private void InitializeSharedStore(ref DirectoryInfo cacheDir)
    {
        const string envVar = "DOTNET_SHARED_STORE";
        
        string[] paths = [];
        if (Environment.GetEnvironmentVariable(envVar) is { } value)
        {
            paths = value.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        }

        paths = [cacheDir.FullName, ..paths];
        
        Environment.SetEnvironmentVariable(envVar, string.Join(Path.PathSeparator, paths));

        cacheDir = cacheDir.CreateSubdirectory("x64"); // todo change this to automatic if we ever get to aarch64
        cacheDir = cacheDir.CreateSubdirectory(new NuGetFramework(_runtimeFramework.Framework.Framework, _runtimeFramework.Framework.Version).GetShortFolderName());
    }
}