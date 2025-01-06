using System.Collections.Immutable;
using System.IO.Compression;
using NLog;
using NuGet;
using NuGet.Frameworks;
using NuGet.Models;
using NuGet.Versioning;

namespace CringePlugins.Resolver;

public class PackageResolver(NuGetFramework runtimeFramework, ImmutableArray<PackageReference> references, PackageSourceMapping packageSources)
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    public async Task<ImmutableSortedSet<ResolvedPackage>> ResolveAsync()
    {
        var order = 0;
        var packages = new SortedDictionary<Package, CatalogEntry>();

        foreach (var reference in references)
        {
            var client = await packageSources.GetClientAsync(reference.Id);

            RegistrationRoot? registrationRoot;

            try
            {
                registrationRoot = await client.GetPackageRegistrationRootAsync(reference.Id);
            }
            catch (HttpRequestException ex)
            {
                Log.Warn("Failed to resolve package {Package}: {Message}", reference.Id, ex.Message);
                continue;
            }

            var items = registrationRoot.Items.SelectMany(page =>
                page.Items!.Where(b => b.CatalogEntry.PackageTypes is ["CringePlugin"]))
                    .ToImmutableDictionary(b => b.CatalogEntry.Version);

            var version = items.Values.Select(b => b.CatalogEntry.Version).OrderDescending().First(b => reference.Range.Satisfies(b));
            
            if (version is null)
                throw new Exception($"Unable to find version for package {reference.Id}");

            var catalogEntry = items[version].CatalogEntry;
            
            var package = new Package(order, reference.Id, version);

            if (packages.TryAdd(package, catalogEntry))
                continue;
            
            if (!packages.TryGetValue(package, out _))
                throw new Exception($"Duplicate package {package.Id}");
            
            var existingPackage = packages.Keys.First(b => b.Version == package.Version && b.Id == package.Id);
            
            if (package.Version < existingPackage.Version)
                throw new Exception($"Package reference {package.Id} has lower version {package.Version} than already resolved {existingPackage.Version}");
            
            if (package.Version == existingPackage.Version)
                continue;

            packages.Remove(existingPackage);
            packages.Add(package with
            {
                Order = ++order
            }, catalogEntry);
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
                throw new Exception($"Unable to find compatible dependency group for package {package.Id}");

            set.Add(new RemotePackage(package, nearestGroup.TargetFramework, client, catalogEntry));
        }
        
        for (var i = 0; i < set.Count; i++)
        {
            if (set[i] is not RemotePackage package) continue;

            var dependencies = package.Entry.DependencyGroups
                                   ?.Single(b => b.TargetFramework == package.ResolvedFramework)?.Dependencies ??
                               [];
            
            foreach (var (id, versionRange) in dependencies)
            {
                var client = await packageSources.GetClientAsync(id);
                
                RegistrationRoot? registrationRoot;

                try
                {
                    registrationRoot = await client.GetPackageRegistrationRootAsync(id);
                }
                catch (HttpRequestException ex)
                {
                    throw new Exception($"Failed to resolve dependency {id} for {package.Package}", ex);
                }
                
                var items = registrationRoot.Items.SelectMany(page => page.Items!)
                    .ToImmutableDictionary(b => b.CatalogEntry.Version);

                var version = items.Values.Select(b => b.CatalogEntry.Version).OrderDescending().FirstOrDefault(b => versionRange.Satisfies(b));
            
                if (version is null)
                    throw new Exception($"Unable to find version for package {id} as dependency of {package.Package}");
                
                var catalogEntry = items[version].CatalogEntry;

                var dependencyPackage = new Package(i, id, version);

                if (packages.TryGetValue(dependencyPackage, out var existingPackage))
                {
                    if (dependencyPackage.Version < existingPackage.Version)
                    {
                        // dependency has lower version than already resolved
                        // need to check if existing fits the version range
                        // and reorder existing to ensure it's ordered before requesting package

                        if (!versionRange.Satisfies(existingPackage.Version))
                            throw new Exception(
                                $"Incompatible package version {dependencyPackage} (required by {package.Package}) from {existingPackage}");
                        
                        if (dependencyPackage.CompareTo(existingPackage) < 0)
                        {
                            packages.Remove(dependencyPackage);
                            packages.Add(dependencyPackage, existingPackage);
                        }
                        
                        continue;
                    }
                    
                    throw new Exception($"Detected package downgrade from {existingPackage} to {dependencyPackage} as dependency of {package.Package}");
                }
                
                if (!packages.TryAdd(dependencyPackage, catalogEntry))
                    throw new Exception($"Duplicate package {dependencyPackage.Id}");
                
                var nearestGroup = NuGetFrameworkUtility.GetNearest(catalogEntry.DependencyGroups ?? [], runtimeFramework,
                    g => g.TargetFramework);
            
                if (nearestGroup is null)
                    throw new Exception($"Unable to find compatible dependency group for {dependencyPackage} as dependency of {package.Package}");
                
                set.Add(new RemoteDependencyPackage(dependencyPackage, nearestGroup.TargetFramework, client, package, catalogEntry));
            }
        }

        return set.ToImmutable();
    }

    public async Task<ImmutableHashSet<CachedPackage>> DownloadPackagesAsync(DirectoryInfo baseDirectory,
        IReadOnlySet<ResolvedPackage> resolvedPackages, IProgress<float>? progress = null)
    {
        var packages = ImmutableHashSet<CachedPackage>.Empty.ToBuilder();
        
        var i = 0f;
        foreach (var package in resolvedPackages)
        {
            switch (package)
            {
                case RemotePackage remotePackage:
                {
                    var dir = new DirectoryInfo(Path.Join(baseDirectory.FullName, package.Package.Id, package.Package.Version.ToString()));
                    if (!dir.Exists)
                    {
                        dir.Create();

                        await using var stream = await remotePackage.Client.GetPackageContentStreamAsync(remotePackage.Package.Id, remotePackage.Package.Version);
                        using var memStream = new MemoryStream();
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