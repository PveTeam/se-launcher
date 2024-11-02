using System.Collections.Immutable;
using System.Text.Json.Serialization;
using NuGet.Converters;
using NuGet.Versioning;

namespace NuGet.Models;

public record SearchResult(int TotalHits, [property: JsonPropertyName("data")] ImmutableArray<SearchResultEntry> Entries);

public record SearchResultEntry(
    string Id,
    NuGetVersion Version,
    string? Description,
    ImmutableArray<SearchResultPackageVersion> Versions,
    PackageAuthors? Authors,
    string? IconUrl,
    string? LicenseUrl,
    [property: JsonConverter(typeof(StringOrStringArrayConverter))]
    ImmutableArray<string>? Owners,
    string? ProjectUrl,
    Uri Registration,
    string? Summary,
    [property: JsonConverter(typeof(StringOrStringArrayConverter))]
    ImmutableArray<string>? Tags,
    string? Title,
    int? TotalDownloads,
    ImmutableArray<PackageType> PackageTypes,
    bool Verified = false);

public record SearchResultPackageVersion(
    NuGetVersion Version,
    int Downloads,
    [property: JsonPropertyName("@id")] Uri Registration);
