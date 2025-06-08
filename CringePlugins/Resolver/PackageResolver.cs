using NLog;
using NuGet;
using NuGet.Frameworks;
using NuGet.Models;
using NuGet.Versioning;
using System.Collections.Immutable;
using System.IO.Compression;

namespace CringePlugins.Resolver;

public class PackageResolver(NuGetFramework runtimeFramework, ImmutableArray<PackageReference> references, PackageSourceMapping packageSources)
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    public async Task<ImmutableSortedSet<ResolvedPackage>> ResolveAsync(DirectoryInfo baseDir, bool disableUpdates, List<PackageReference> invalidPackages)
    {
        var order = 0;
        var packages = new Dictionary<Package, CatalogEntry>();

        foreach (var reference in references)
        {
            var client = await packageSources.GetClientAsync(reference.Id);

            if (client == null)
                continue; //todo: check cached files. test with Internet disconnected

            RegistrationRoot? registrationRoot;

            try
            {
                registrationRoot = await client.GetPackageRegistrationRootAsync(reference.Id);
            }
            catch (HttpRequestException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    //package isn't on this source, and should be removed from the config
                    invalidPackages.Add(reference);
                }

                Log.Warn("Failed to resolve package {Package}: {Message}", reference.Id, ex.Message);
                continue;
            }

            var items = registrationRoot.Items.SelectMany(page =>
                page.Items!.Where(b => b.CatalogEntry.PackageTypes is ["CringePlugin"]))
                    .ToImmutableDictionary(b => b.CatalogEntry.Version);

            var version = items.Values.Select(b => b.CatalogEntry.Version).OrderDescending().First(reference.Range.Satisfies);

            if (disableUpdates)
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
                    Log.Warn("No valid installed version found for package {Package}", reference.Id);
                }
            }

            if (version is null)
                throw new NotSupportedException($"Unable to find version for package {reference.Id}");

            var catalogEntry = items[version].CatalogEntry;

            var package = new Package(order, reference.Id, version);

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

            if (client == null || !catalogEntry.DependencyGroups.HasValue)
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
                var client = await packageSources.GetClientAsync(id);

                if (client == null)
                    continue;

                RegistrationRoot? registrationRoot;

                try
                {
                    registrationRoot = await client.GetPackageRegistrationRootAsync(id);
                }
                catch (HttpRequestException ex)
                {
                    throw new InvalidOperationException($"Failed to resolve dependency {id} for {package.Package}", ex);
                }

                var items = registrationRoot.Items.SelectMany(page => page.Items!)
                    .ToImmutableDictionary(b => b.CatalogEntry.Version);

                var version = items.Values.Select(b => b.CatalogEntry.Version).OrderDescending().FirstOrDefault(versionRange.Satisfies);

                if (version is null)
                    throw new NotSupportedException($"Unable to find version for package {id} as dependency of {package.Package}");

                if (disableUpdates)
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
                    //todo: warnings here? we'd need to check against builtin packages
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

                        var client = (package as RemoteDependencyPackage)?.Client ?? ((RemotePackage)package).Client;

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

public record CachedPackage(Package Package, NuGetFramework ResolvedFramework, DirectoryInfo Directory, CatalogEntry Entry) : ResolvedPackage(Package, ResolvedFramework, Entry);
public record RemotePackage(Package Package, NuGetFramework ResolvedFramework, NuGetClient Client, CatalogEntry Entry) : ResolvedPackage(Package, ResolvedFramework, Entry);

// should not inherit from RemotePackage
public record RemoteDependencyPackage(
    Package Package,
    NuGetFramework ResolvedFramework,
    NuGetClient Client,
    RemotePackage Parent,
    CatalogEntry Entry) : ResolvedPackage(Package, ResolvedFramework, Entry);

public abstract record ResolvedPackage(Package Package, NuGetFramework ResolvedFramework, CatalogEntry Entry) : IComparable<ResolvedPackage>, IComparable
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

public record PackageReference(string Id, VersionRange Range);