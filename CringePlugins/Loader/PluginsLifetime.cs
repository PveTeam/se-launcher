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

internal class PluginsLifetime(ConfigHandler configHandler, HttpClient client, DirectoryInfo dir) : IPluginsLifetime
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
        await Task.Delay(10000);
#endif

        DiscoverLocalPlugins(dir.CreateSubdirectory("plugins"));

        progress.Report("Loading config");

        _configReference = configHandler.RegisterConfig("packages", PackagesConfig.Default);
        _launcherConfig = configHandler.RegisterConfig("launcher", LauncherConfig.Default);
        var packagesConfig = _configReference.Value;
        var launcherConfig = _launcherConfig.Value;

        progress.Report("Resolving packages");

        var sourceMapping = new PackageSourceMapping(packagesConfig.Sources, client);
        // TODO take into account the target framework runtime identifier
        var resolver = new PackageResolver(_runtimeFramework.Framework, packagesConfig.Packages, sourceMapping);

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

        await LoadPlugins(cachedPackages, sourceMapping, packagesConfig, builtInPackages);

        RenderHandler.Current.RegisterComponent(new PluginListComponent(_configReference, _launcherConfig,
            sourceMapping, MyFileSystem.ExePath, Plugins, dir, cacheDir));

        SomeSourcesAreUnavailable = sourceMapping.SomeSourcesAreUnavailable;
    }

    public static async Task ReloadPlugin(PluginInstance instance)
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
        PackagesConfig packagesConfig, ImmutableDictionary<string, ResolvedPackage> builtInPackages)
    {
        var plugins = Plugins.ToBuilder();

        var resolvedPackages = builtInPackages.ToDictionary();
        foreach (var package in packages)
        {
            resolvedPackages.TryAdd(package.Package.Id, package);
        }

        var manifestBuilder = new DependencyManifestBuilder(dir.CreateSubdirectory("cache"), sourceMapping,
            dependency =>
            {
                resolvedPackages.TryGetValue(dependency.Id, out var package);
                return package?.Entry;
            });

        foreach (var package in packages)
        {
            if (builtInPackages.ContainsKey(package.Package.Id) || package.Entry.PackageTypes is not ["CringePlugin"]) continue;

            var packageClient = await sourceMapping.GetClientAsync(package.Package.Id);


            var packageDir = Path.Join(package.Directory.FullName, "lib", package.ResolvedFramework.GetShortFolderName());

            var path = Path.Join(packageDir, $"{package.Package.Id}.deps.json");
            if (!File.Exists(path))
            {
                if (packageClient == null)
                {
                    Log.Warn("No package source found for {Package}, cannot generate dependency manifest", package.Package.Id);
                    continue;
                }

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

            var sourceName = packageClient == null ? "Local Cache" : packagesConfig.Sources.First(b => b.Url == packageClient.ToString()).Name;
            LoadComponent(plugins, Path.Join(packageDir, $"{package.Package.Id}.dll"),
                new(package.Entry.Title ?? package.Package.Id, package.Package.Version, sourceName));
        }

        Plugins = plugins.ToImmutable();
    }

    private void DiscoverLocalPlugins(DirectoryInfo dir)
    {
        var plugins = ImmutableArray<PluginInstance>.Empty.ToBuilder();

        foreach (var directory in Environment.GetEnvironmentVariable("DOTNET_USERDEV_PLUGINDIR") is { } userDevPlugin
                     ? [new(userDevPlugin), ..dir.GetDirectories()]
                     : dir.EnumerateDirectories())
        {
            var files = directory.GetFiles("*.deps.json");

            if (files.Length != 1) continue;

            var path = files[0].FullName[..^".deps.json".Length] + ".dll";

            LoadComponent(plugins, path, null, true);
        }

        Plugins = plugins.ToImmutable();
    }

    private static void LoadComponent(ImmutableArray<PluginInstance>.Builder plugins, string path,
        PluginMetadata? metadata = null, bool local = false)
    {
        try
        {
            plugins.Add(metadata is null ? new PluginInstance(path, local) : new(metadata, path, local));
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load plugin {PluginPath}", path);
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