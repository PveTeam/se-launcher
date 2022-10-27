using System.Collections.Immutable;
using NuGet.Versioning;

namespace NuGet.Models;

public record CatalogEntry(string Id, NuGetVersion Version, ImmutableArray<DependencyGroup> DependencyGroups, ImmutableArray<string>? PackageTypes,
    ImmutableArray<CatalogPackageEntry>? PackageEntries);
    
public record CatalogPackageEntry(string Name, string FullName, long CompressedLength, long Length);