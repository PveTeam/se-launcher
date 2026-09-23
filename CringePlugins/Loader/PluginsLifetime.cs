using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using CringePlugins.Abstractions.Loader;
using CringePlugins.Config;
using CringePlugins.Loader.ProviderProvider;
using CringePlugins.Render;
using CringePlugins.Resolver;
using CringePlugins.Splash;
using CringePlugins.Ui;
using CringePlugins.Utils;
using dnlib.DotNet;
using dnlib.PE;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TemplateEngine.Utils;
using NLog;
using NuGet;
using NuGet.Deps;
using NuGet.Frameworks;
using NuGet.Models;
using SharedCringe.Abstractions.Transformers;
using SharedCringe.Utils;
using VRage.FileSystem;
using Dependency = NuGet.Models.Dependency;

namespace CringePlugins.Loader;

internal class PluginsLifetime(
    IServiceProvider serviceProvider,
    ConfigHandler configHandler,
    IPluginServiceProviderFactory serviceProviderFactory,
    HttpClient client,
    DirectoryInfo dir,
    string packageType) : IPluginsLifetime
{
    internal static PluginsLifetime? Instance;

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public string Name => "Loading Plugins";

    internal ImmutableDictionary<IPluginInstance, PluginInstanceData> LoadedPlugins = [];
    internal ImmutableDictionary<string, PluginMetadata> Plugins = [];
    internal bool SomeSourcesAreUnavailable { get; private set; }

    private readonly NuGetRuntimeFramework _runtimeFramework =
        new(NuGetFramework.ParseFolder("net10.0-windows10.0.19041.0"), RuntimeInformation.RuntimeIdentifier);

    private ConfigReference<PackagesConfig>? _configReference;
    private ConfigReference<LauncherConfig>? _launcherConfig;

    public async ValueTask Load(ISplashProgress progress)
    {
        Instance = this;
        progress.DefineStepsCount(6);
        
        progress.Report("Loading config");

        _configReference = configHandler.RegisterConfig("packages", PackagesConfig.Default);
        _launcherConfig = configHandler.RegisterConfig("launcher", LauncherConfig.Default);
        var packagesConfig = _configReference.Value;
        var launcherConfig = _launcherConfig.Value;
        
        var cacheDir = dir.CreateSubdirectory("cache");
        InitializeSharedStore(ref cacheDir);

        progress.Report("Discovering local plugins");

#if DEBUG
        // await Task.Delay(10000);
#endif

        var (localPlugins, localRequestedReferences) = await DiscoverLocalPlugins(dir.CreateSubdirectory("plugins"));

        progress.Report("Resolving packages");

        var sourceMapping = new PackageSourceMapping(packagesConfig.Sources, client);

        var builtInPackages = await BuiltInPackages.GetPackagesAsync(_runtimeFramework);
        var builtInPackageIds = builtInPackages.Keys.ToHashSet();
        
        // TODO take into account the target framework runtime identifier
        var resolver = new PackageResolver(_runtimeFramework.Framework, [..packagesConfig.Packages, ..localRequestedReferences], sourceMapping);
        
        var invalidPackages = new List<PackageReference>();
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
            .Order()
            .DistinctBy(b => b.Package.Id)
            .ToDictionary(b => b.Package.Id, StringComparer.OrdinalIgnoreCase);

        var rootProvider = new PluginProviderPluginProvider();

        var providerInstances = await LoadComponentsAsync(loadedPackages.Values, [rootProvider], sourceMapping,
            packagesConfig, builtInPackages, cacheDir, []);
        
        AttachComponents(providerInstances);

        var providers = providerInstances.Keys.OfType<PluginProviderInstance>().Select(b => b.Provider)
            .Append(new PluginProvider.PluginProvider(packageType))
            .ToImmutableArray();

        var loadedComponents =
            providerInstances.ToImmutableDictionary(b => b.Key.Metadata.Id, b => b.Value,
                StringComparer.OrdinalIgnoreCase);

        LoadedPlugins = await LoadComponentsAsync(loadedPackages.Values, providers, sourceMapping,
            packagesConfig, builtInPackages, cacheDir, loadedComponents);
        
        Plugins = LoadedPlugins.Keys.ToImmutableDictionary(b => b.Metadata.Id, b => b.Metadata, StringComparer.OrdinalIgnoreCase);

        RenderHandler.Current.RegisterComponent(new PluginListComponent(_configReference, _launcherConfig,
            sourceMapping, MyFileSystem.ExePath, LoadedPlugins.Keys, dir, cacheDir, loadedPackages));

        SomeSourcesAreUnavailable = sourceMapping.SomeSourcesAreUnavailable;
    }

    private async Task<ImmutableDictionary<IPluginInstance, PluginInstanceData>> LoadComponentsAsync(IReadOnlyCollection<CachedPackage> packages,
        ImmutableArray<IPluginProvider> providers, PackageSourceMapping sourceMapping, PackagesConfig packagesConfig,
        ImmutableDictionary<string, ResolvedPackage> builtInPackages, DirectoryInfo cacheDir, ImmutableDictionary<string, PluginInstanceData> loadedComponents)
    {
        var packageTypes = providers
            .SelectMany(b => b.PackageTypes.Select(c => new KeyValuePair<string, IPluginProvider>(c, b)))
            .ToImmutableDictionary();
        
        var resolvedPackages = builtInPackages.ToDictionary();
        foreach (var package in packages)
        {
            resolvedPackages.TryAdd(package.Package.Id, package);
        }

        var manifestBuilder = new DependencyManifestBuilder(cacheDir, sourceMapping,
            dependency =>
            {
                if (builtInPackages.ContainsKey(dependency.Id))
                    return null;
                resolvedPackages.TryGetValue(dependency.Id, out var package);
                return package?.Entry;
            },
            isHostProvided: id => builtInPackages.ContainsKey(id));

        var componentPackages = packages.Where(package =>
                !builtInPackages.ContainsKey(package.Package.Id) && package.Entry.PackageTypes is [..] &&
                packageTypes.Keys.Intersect(package.Entry.PackageTypes).Any())
            .ToImmutableArray();

        var dependenciesMap = componentPackages.ToDictionary(ResolvedPackage (b) => b, b =>
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
                }).Where(p =>
                    p.Entry.PackageTypes is { IsEmpty: false } types &&
                    packageTypes.Keys.Intersect(types).Any())
                .ToHashSet();
        });

        var plugins = ImmutableDictionary.CreateBuilder<IPluginInstance, PluginInstanceData>();
        
        foreach (var subGraph in DependenciesUtils.SplitIntoSubGraphs(dependenciesMap!))
        {
            DirectedGraph<ResolvedPackage> graph = subGraph;
            if (!graph.TryGetTopologicalSort(out var sortedElements))
                throw new Exception("Plugin dependency cycle detected");

            AlcFactory? parent = null;
            var anyLoaded = false;
            var order = 0;
            foreach (var package in sortedElements.OfType<CachedPackage>())
            {
                if (package.Entry.PackageTypes is not [..] ||
                    packageTypes.IntersectBy(package.Entry.PackageTypes.Value, b => b.Key).FirstOrDefault() is not
                        { Value: { } provider })
                    continue;
                
                anyLoaded = true;
                var packageClient = await sourceMapping.GetClientAsync(package.Package.Id);

                string packageDir;
                if (package is LocalPluginPackage)
                    packageDir = package.Directory.FullName;
                else
                {
                    packageDir = Path.Join(package.Directory.FullName, "lib",
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
                                dependency => !dependency.PackageTypes.GetValueOrDefault([]).Any(packageTypes.ContainsKey));
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex, $"Failed to write dependency manifest for {path}");
                            File.Delete(path); //delete file to avoid breaking cache
                            throw;
                        }
                    }
                }
                
                var assetsDirectory = new DirectoryInfo(Path.Join(package.Directory.FullName, "assets"));
                if (!assetsDirectory.Exists) assetsDirectory = null;

                var sourceName = package is LocalPluginPackage
                    ? "Local"
                    : packageClient == null
                        ? "Local Cache"
                        : packagesConfig.Sources.First(b => b.Url == packageClient.ToString()).Name;

                var entryTitle = string.IsNullOrEmpty(package.Entry.Title) ? package.Package.Id : package.Entry.Title;
                var metadata = new PluginMetadata(package.Package.Id, entryTitle, package.Package.Version,
                    sourceName);

                var entrypointPath = Path.Join(packageDir, $"{package.Package.Id}.dll");
                var entrypoint = new LazyPluginEntrypoint(metadata, provider);

                var factory = loadedComponents.TryGetValue(package.Package.Id, out var instanceData)
                    ? new AlcFactory(instanceData.ContextFactory.Context!, entrypoint, serviceProviderFactory)
                    : new AlcFactory(entrypointPath, entrypoint, package is LocalPluginPackage pluginPackage
                            ? pluginPackage.DependencyResolver
                            : new(entrypointPath), serviceProviderFactory, parent, package is LocalPluginPackage,
                        serviceProvider.GetRequiredService<ITransformationService>(), provider, metadata);

                var topoIndex = order;
                order++;

                try
                {
                    var pluginInstance = provider.LoadComponent(metadata, factory);

                    if (pluginInstance is not null && !plugins.TryAdd(pluginInstance, new(factory, topoIndex)))
                    {

                        plugins.TryGetKey(pluginInstance, out var actualInstance);
                        plugins.TryGetValue(actualInstance, out var actualData);
                        Log.Warn(
                            "Plugin Id {PluginId} is already occupied, using previously loaded {PreviousMetadata} instead of {NewMetadata}",
                            metadata.Id, actualInstance.Metadata, metadata);
                        factory = actualData!.ContextFactory;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e, "Failed to load plugin {PluginPath}", entrypointPath);
                }
                
                parent = factory;
            }
            
            if (anyLoaded)
                Log.Info("Topological Sorted Leaf: {Leaf}",
                    string.Join(", ", sortedElements.Select(b =>
                        string.IsNullOrEmpty(b.Entry.Title) ? b.Entry.Id : b.Entry.Title)));
        }

        return plugins.ToImmutable();
    }

    public static async Task ReloadPluginAsync(IPluginInstance instance)
    {
        try
        {
            await instance.ReloadAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to reload plugin {Plugin}", instance.Metadata);
        }
    }

    public void RegisterLifetime() => AttachComponents(LoadedPlugins);

    private static void AttachComponents(ImmutableDictionary<IPluginInstance, PluginInstanceData> instances)
    {
        // Dependency-first instantiation: a package's ALC (and its host role, e.g. a combined
        // provider+plugin package) must exist before anything that depends on it instantiates.
        foreach (var (instance, data) in instances.OrderBy(b => b.Value.Order))
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

    private async ValueTask<(ImmutableArray<CachedPackage>localPlugins, ImmutableArray<PackageReference> references)>
        DiscoverLocalPlugins(DirectoryInfo dir)
    {
        var localPlugins = ImmutableArray.CreateBuilder<CachedPackage>();
        var references = ImmutableArray.CreateBuilder<PackageReference>();
        
        foreach (var directory in Environment.GetEnvironmentVariable("DOTNET_USERDEV_PLUGINDIR") is { } userDevPlugin
                     ? [new(userDevPlugin), ..dir.GetDirectories()]
                     : dir.EnumerateDirectories())
        {
            const string depsExtension = ".deps.json";
            var files = directory.GetFiles($"*{depsExtension}");

            if (files.Length != 1) continue;

            DependenciesManifest manifest;
            await using (var stream = files[0].OpenRead())
                manifest = await DependencyManifestSerializer.DeserializeAsync(stream);
            
            var packageReferences = manifest.Libraries.Where(b => b.Value.Serviceable)
                .Select(b =>
                {
                    var ((id, version), _) = b;
                    return new PackageReference(id, new(version));
                }).ToArray();
            references.AddRange(packageReferences);

            var resolver = new AssemblyDependencyResolver(files[0].FullName[..^depsExtension.Length] + ".dll");

            var targetLibraries = manifest.Targets[manifest.RuntimeTarget.RuntimeFramework];
            foreach (var (packageKey, _) in manifest.Libraries.Where(b => b.Value is
                         { Serviceable: false, Type: LibraryType.Project }))
            {
                if (resolver.ResolveAssemblyToPath(new(packageKey.Id)) is not { } path)
                {
                    Log.Warn("Failed to resolve {PackageKey} from {DepsPath}. This dependency would be skipped.",
                        packageKey, files[0].Name);
                    continue;
                }
                
                var metadata = PluginMetadata.ReadFromEntrypoint(path);
                
                if (metadata is null) continue;

                var dependencies = targetLibraries[packageKey].Dependencies
                    ?.Select(b => new ManifestPackageKey(b.Key, b.Value))
                    .Where(b => manifest.Libraries[b] is { Type: LibraryType.Package, Serviceable: true } or
                        { Type: LibraryType.Project })
                    .Select(b => new Dependency(b.Id, new(b.Version)))
                    .ToImmutableArray() ?? [];
                
                var package = new Package(int.MinValue, metadata.Id, metadata.Version);
                var entry = new CatalogEntry(metadata.Id, metadata.Version, [
                    new DependencyGroup(_runtimeFramework.Framework,
                        dependencies)
                ], ["CringePlugin"], [], metadata.Name);

                localPlugins.Add(new LocalPluginPackage(package, _runtimeFramework.Framework, directory, entry, resolver));
            }
        }
        
        return (localPlugins.ToImmutable(), references.ToImmutable());
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

        var storeValue = string.Join(Path.PathSeparator, paths);
        Environment.SetEnvironmentVariableNoCap(envVar, storeValue);

        cacheDir = cacheDir.CreateSubdirectory("x64"); // todo change this to automatic if we ever get to aarch64
        cacheDir = cacheDir.CreateSubdirectory(new NuGetFramework(_runtimeFramework.Framework.Framework, _runtimeFramework.Framework.Version).GetShortFolderName());
    }

    internal record PluginInstanceData(AlcFactory ContextFactory, int Order = 0);
}
