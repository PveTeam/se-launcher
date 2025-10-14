using CringePlugins.Compatability;
using CringePlugins.Utils;
using NLog;
using NuGet;
using NuGet.Frameworks;
using NuGet.Models;
using NuGet.Versioning;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO.Compression;
using System.Runtime.Loader;
using System.Xml.Serialization;

namespace CringePlugins.Resolver;

public class PackageResolver(NuGetFramework runtimeFramework, ImmutableHashSet<PackageReference> references, PackageSourceMapping packageSources)
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    public async Task<ImmutableSortedSet<ResolvedPackage>> ResolveAsync(DirectoryInfo baseDir, bool disableUpdates, IReadOnlySet<string> builtinPackages, List<PackageReference> invalidPackages)
    {
        var order = 0;
        var packages = new Dictionary<Package, CatalogEntry>();

        foreach (var reference in references)
        {
            var (items, client, removed) = await ResolvePackageEntriesAsync(baseDir, packageSources, reference.Id);

            if (removed)
                invalidPackages.Add(reference);

            if (items == null || items.Count == 0)
            {
                Log.Warn("No valid package entries found for {Package}, skipping", reference.Id);
                continue;
            }

            var version = items.Values.Where(b => b.CatalogEntry.PackageTypes is ["CringePlugin"])
                .Select(b => b.CatalogEntry.Version).OrderDescending().First(reference.Range.Satisfies);

            if (client != null && disableUpdates)
            {
                if (GetLatestInstalledVersion(baseDir, reference.Id, reference.Range) is { } installedVersion && items.ContainsKey(installedVersion))
                {
                    if (installedVersion < version)
                    {
                        Log.Warn("Using outdated version of package {Package} {InstalledVersion} instead of {AvailableVersion} due to updates being disabled",
                            reference.Id, installedVersion, version);
                    }
                    version = installedVersion;
                }
                else
                {
                    Log.Warn("No valid installed version found for package {Package}. Updating to {Version}", reference.Id, version);
                }
            }

            var catalogEntry = items[version].CatalogEntry;

            var package = new Package(order, reference.Id, catalogEntry.Version);

            if (packages.TryAdd(package, catalogEntry))
                continue;

            if (!packages.TryGetValue(package, out var existingEntry))
                throw new InvalidOperationException($"Duplicate package error {package.Id}");

            if (package.Version < existingEntry.Version)
                throw new NotSupportedException($"Package reference {package.Id} has lower version {package.Version} than already resolved {existingEntry.Version}");

            if (package.Version == existingEntry.Version)
                continue;

            packages[package with { Order = ++order }] = catalogEntry;
        }

        var set = ImmutableSortedSet<ResolvedPackage>.Empty.ToBuilder();
        foreach (var (package, catalogEntry) in packages)
        {
            var client = await packageSources.GetClientAsync(package.Id);

            if (!catalogEntry.DependencyGroups.HasValue)
                continue;

            var nearestGroup = NuGetFrameworkUtility.GetNearest(catalogEntry.DependencyGroups.Value, runtimeFramework,
                g => g.TargetFramework);


            if (nearestGroup is null)
                throw new NotSupportedException($"Unable to find compatible dependency group for package {package.Id}");

            set.Add(new RemotePackage(package, nearestGroup.TargetFramework, client, catalogEntry));
        }

        var dependencyVersions = new Dictionary<Package, VersionRange>();
        var dependencyPackages = new HashSet<RemoteDependencyPackage>();
        for (var i = 0; i < set.Count; i++)
        {
            if (set[i] is not RemotePackage package) continue;

            var dependencies = package.Entry.DependencyGroups
                                   ?.Single(b => b.TargetFramework == package.ResolvedFramework)?.Dependencies ??
                               [];

            foreach (var (id, versionRange) in dependencies)
            {
                if (builtinPackages.Contains(id))
                    continue;

                (var items, var client, _) = await ResolvePackageEntriesAsync(baseDir, packageSources, id);

                if (items == null || items.Count == 0)
                    throw new NotSupportedException($"Missing required dependency {id} {versionRange} for {package.Package}");

                var version = items.Values.Select(b => b.CatalogEntry.Version).OrderDescending().FirstOrDefault(versionRange.Satisfies);

                if (version is null)
                    throw new NotSupportedException($"Unable to find version for package {id} as dependency of {package.Package}");

                if (client != null && disableUpdates)
                {
                    if (GetLatestInstalledVersion(baseDir, id, versionRange) is { } installedVersion && items.ContainsKey(installedVersion))
                    {
                        if (installedVersion < version)
                        {
                            Log.Warn("Using outdated version of dependency package {Package} {InstalledVersion} instead of {AvailableVersion} due to updates being disabled",
                                id, installedVersion, version);
                        }
                        version = installedVersion;
                    }
                    else
                    {
                        Log.Warn("No valid installed version found for dependency {Package}. Updating to {Version}",id, version);
                    }
                }

                var catalogEntry = items[version].CatalogEntry;

                var dependencyPackage = new Package(i, id, version);

                if (packages.TryGetValue(dependencyPackage, out var existingCatalog))
                {
                    if (dependencyPackage.Version == existingCatalog.Version)
                        continue; //a dependency with this version has already been resolved

                    //does the existing version support our package?
                    if (versionRange.Satisfies(existingCatalog.Version))
                        continue; //keep the old one

                    if (!dependencyVersions.TryGetValue(dependencyPackage, out var minimalVersionRange))
                        throw new InvalidOperationException("Missing minimal version range");

                    minimalVersionRange = VersionRange.CommonSubSet([minimalVersionRange, versionRange]);

                    if (!minimalVersionRange.Satisfies(version))
                    {
                        //do one last check for a matching version
                        version = items.Values.Select(b => b.CatalogEntry.Version).OrderDescending().FirstOrDefault(minimalVersionRange.Satisfies);

                        if (version is null)
                            throw new NotSupportedException($"Unable to find version for package {id} as dependency of {package.Package} (and others) that satisfies {minimalVersionRange}");

                        catalogEntry = items[version].CatalogEntry;

                        dependencyPackage = dependencyPackage with { Version = version };
                    }

                    //swap to this version
                    packages[dependencyPackage] = catalogEntry;
                    dependencyVersions[dependencyPackage] = minimalVersionRange;

                    var replacementGroup = NuGetFrameworkUtility.GetNearest(catalogEntry.DependencyGroups ?? [], runtimeFramework, g => g.TargetFramework)
                         ?? throw new NotSupportedException($"Unable to find compatible dependency group for {dependencyPackage} as dependency of {package.Package}");

                    var replacement = new RemoteDependencyPackage(dependencyPackage, replacementGroup.TargetFramework, client, package, catalogEntry);

                    if (!dependencyPackages.Remove(replacement))
                        throw new InvalidOperationException("Replaced dependency wasn't there");

                    dependencyPackages.Add(replacement);

                    continue;
                }

                if (!packages.TryAdd(dependencyPackage, catalogEntry) || !dependencyVersions.TryAdd(dependencyPackage, versionRange))
                    throw new InvalidOperationException($"Duplicate package {dependencyPackage.Id}");

                var nearestGroup = NuGetFrameworkUtility.GetNearest(catalogEntry.DependencyGroups ?? [], runtimeFramework,
                    g => g.TargetFramework) ?? throw new NotSupportedException($"Unable to find compatible dependency group for {dependencyPackage} as dependency of {package.Package}");

                dependencyPackages.Add(new RemoteDependencyPackage(dependencyPackage, nearestGroup.TargetFramework, client, package, catalogEntry));
            }
        }

        foreach (var item in dependencyPackages)
            set.Add(item);

        return set.ToImmutable();
    }

    private static async Task<(ImmutableDictionary<NuGetVersion, RegistrationEntry>? Items, NuGetClient? Client, bool Removed)> ResolvePackageEntriesAsync(DirectoryInfo baseDir, PackageSourceMapping packageSources, string id)
    {
        var client = await packageSources.GetClientAsync(id);

        if (client == null)
            return (await GetCachedVersionsAsync(baseDir, id), null, false);

        RegistrationRoot? registrationRoot;

        try
        {
            registrationRoot = await client.GetPackageRegistrationRootAsync(id);
        }
        catch (HttpRequestException ex)
        {
            Log.Warn("Failed to resolve remote package {Package}: {Message}", id, ex.Message);

            if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                //package isn't on this source, and should be removed from the config
                Log.Warn("Remote package {Package}: is not on {Client} and will be removed", id, client.ToString());
                return (null, client, true);
            }

            return (await GetCachedVersionsAsync(baseDir, id), null, false);
        }

        return (registrationRoot.Items.SelectMany(page => page.Items!)
                    .ToImmutableDictionary(b => b.CatalogEntry.Version), client, false);
    }


    private static async Task<ImmutableDictionary<NuGetVersion, RegistrationEntry>?> GetCachedVersionsAsync(DirectoryInfo baseDir, string id)
    {
        var dir = new DirectoryInfo(Path.Join(baseDir.FullName, id));

        if (!dir.Exists)
        {
            Log.Warn("No cached version of package {Package} found in {Directory}", id, dir.FullName);
            return null;
        }

        var serializer = new XmlSerializer(typeof(Nuspec));

        var bag = new ConcurrentBag<RegistrationEntry>();
        ValueTask LoadVersionAsync(DirectoryInfo info, CancellationToken _)
        {
            if (!NuGetVersion.TryParse(info.Name, out var version))
                return ValueTask.CompletedTask;

            var nuspecFile = Path.Join(info.FullName, $"{id}.nuspec");

            if (!File.Exists(nuspecFile))
                return ValueTask.CompletedTask;

            //dependency group, package type, package entry

            using var fileStream = File.OpenRead(nuspecFile);
            using var reader = new IgnoreNamespaceXmlReader(fileStream);
            Nuspec? nuspec;
            try
            {
                nuspec = (Nuspec?)serializer.Deserialize(reader);
            }
            catch (Exception ex)
            {
                Log.Warn(ex, "Failed to parse nuspec {File}: {Message}", nuspecFile, ex.Message);
                return ValueTask.CompletedTask;
            }

            if (nuspec == null)
            {
                Log.Warn("Failed to parse nuspec: {File}", nuspecFile);
                return ValueTask.CompletedTask;
            }

            var depsBuilder = ImmutableArray.CreateBuilder<DependencyGroup>();
            if (nuspec.Metadata.Dependencies is { } dependencies)
            {
                foreach (var dep in dependencies.Groups)
                {
                    var deps = dep.Dependencies?.Select(d => new Dependency(d.Id, new(new(d.Version)))).ToImmutableArray();
                    depsBuilder.Add(new(NuGetRuntimeFramework.Parse(dep.TargetFramework).Framework, deps));
                }
            }

            var packageTypes = nuspec.Metadata.PackageTypes?.Select(b => b.Name).ToImmutableArray() ?? [];

            bag.Add(new(new(id, version, depsBuilder.ToImmutable(), packageTypes, null)));

            return ValueTask.CompletedTask;
        }


        await Parallel.ForEachAsync(dir.GetDirectories(), LoadVersionAsync);

        return bag.ToImmutableDictionary(x => x.CatalogEntry.Version);
    }

    private static NuGetVersion? GetLatestInstalledVersion(DirectoryInfo baseDirectory, string id, VersionRange range)
    {
        var dir = new DirectoryInfo(Path.Join(baseDirectory.FullName, id));

        if (!dir.Exists)
            return null;

        NuGetVersion? maxVersion = null;
        foreach (var subdir in dir.GetDirectories())
        {
            if (NuGetVersion.TryParse(subdir.Name, out var version) && range.Satisfies(version) && (maxVersion == null || version > maxVersion))
            {
                maxVersion = version;
            }
        }

        return maxVersion;
    }

    public static async Task<ImmutableHashSet<CachedPackage>> DownloadPackagesAsync(DirectoryInfo baseDirectory,
        IReadOnlySet<ResolvedPackage> resolvedPackages, IReadOnlySet<string>? ignorePackages = null, IProgress<float>? progress = null)
    {
        var packages = ImmutableHashSet<CachedPackage>.Empty.ToBuilder();

        var i = 0f;
        foreach (var package in resolvedPackages)
        {
            if (ignorePackages?.Contains(package.Package.Id) == true)
                continue;

            switch (package)
            {
                case RemoteDependencyPackage:
                case RemotePackage:
                {
                    var dir = new DirectoryInfo(Path.Join(baseDirectory.FullName, package.Package.Id, package.Package.Version.ToString()));
                    if (!dir.Exists)
                    {
                        dir.Create();

                        var client = (package as RemoteDependencyPackage)?.Client ?? ((RemotePackage)package).Client
                                ?? throw new InvalidOperationException("Attempted to download a package with no client (no cached folder)");

                        await using var stream = await client.GetPackageContentStreamAsync(package.Package.Id, package.Package.Version);
                        await using var memStream = new MemoryStream();
                        await stream.CopyToAsync(memStream);
                        memStream.Position = 0;
                        using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);
                        archive.ExtractToDirectory(dir.FullName);
                    }

                    packages.Add(new CachedPackage(package.Package, package.ResolvedFramework, dir, package.Entry));
                    break;
                }
                case CachedPackage cachedPackage:
                    packages.Add(cachedPackage);
                    break;

                    
            }

            progress?.Report(i++ / resolvedPackages.Count);
        }

        return packages.ToImmutable();
    }
}

public record CachedPackage(
    Package Package,
    NuGetFramework ResolvedFramework,
    DirectoryInfo Directory,
    CatalogEntry Entry) : ResolvedPackage(Package, ResolvedFramework, Entry);

public record LocalPluginPackage(
    Package Package,
    NuGetFramework ResolvedFramework,
    DirectoryInfo Directory,
    CatalogEntry Entry,
    AssemblyDependencyResolver DependencyResolver) : CachedPackage(Package, ResolvedFramework, Directory, Entry);

public record RemotePackage(Package Package, NuGetFramework ResolvedFramework, NuGetClient? Client, CatalogEntry Entry)
    : ResolvedPackage(Package, ResolvedFramework, Entry);

// should not inherit from RemotePackage
public record RemoteDependencyPackage(
    Package Package,
    NuGetFramework ResolvedFramework,
    NuGetClient? Client,
    RemotePackage Parent,
    CatalogEntry Entry) : ResolvedPackage(Package, ResolvedFramework, Entry);

public abstract record ResolvedPackage(Package Package, NuGetFramework ResolvedFramework, CatalogEntry Entry)
    : IComparable<ResolvedPackage>, IComparable
{
    public int CompareTo(ResolvedPackage? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Package.CompareTo(other.Package);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is ResolvedPackage other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(ResolvedPackage)}");
    }

    public override int GetHashCode() => Package.GetHashCode();

    public virtual bool Equals(Package? other)
    {
        if (other is null) return false;
        return Package.Equals(other);
    }
}

public record Package(int Order, string Id, NuGetVersion Version) : IComparable<Package>, IComparable
{
    public int CompareTo(Package? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var orderComparison = Order.CompareTo(other.Order);
        if (orderComparison != 0) return orderComparison;
        return string.Compare(Id, other.Id, StringComparison.OrdinalIgnoreCase);
    }

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (ReferenceEquals(this, obj)) return 0;
        return obj is Package other ? CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Package)}");
    }

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Id);

    public virtual bool Equals(Package? other)
    {
        if (other is null) return false;
        return Id.Equals(other.Id, StringComparison.OrdinalIgnoreCase);
    }
}

public record PackageReference(string Id, VersionRange Range)
{
    public override int GetHashCode() => Id.GetHashCode(StringComparison.OrdinalIgnoreCase);

    public virtual bool Equals(PackageReference? other) => Id.Equals(other?.Id, StringComparison.OrdinalIgnoreCase);
}