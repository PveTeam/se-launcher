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
    public async Task<ImmutableHashSet<ResolvedPackage>> ResolveAsync()
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
            
            var package = new Package(order, reference.Id, version, []); // todo resolve dependencies
            
            if (packages.TryAdd(package, items[version].CatalogEntry))
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
            }, items[version].CatalogEntry);
        }

        var set = ImmutableHashSet<ResolvedPackage>.Empty.ToBuilder();
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

public abstract record ResolvedPackage(Package Package, NuGetFramework ResolvedFramework, CatalogEntry Entry);

public record Package(int Order, string Id, NuGetVersion Version, ImmutableArray<string> Dependencies) : IComparable<Package>, IComparable
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