using System.Net.Http.Json;
using System.Text.Json;
using System.Web;
using NuGet.Converters;
using NuGet.Models;
using NuGet.Versioning;

namespace NuGet;

public sealed class NuGetClient
{
    private readonly Uri _index;
    private readonly HttpClient _client;
    private readonly Uri _packageBaseAddress;
    private readonly Uri _registration;
    private readonly Uri _search;

    public static JsonSerializerOptions SerializerOptions { get; }  = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new VersionJsonConverter(),
            new VersionRangeJsonConverter(),
            new FrameworkJsonConverter(FrameworkNameFormat.ShortFolderName),
        },
        WriteIndented = true
    };

    private NuGetClient(Uri index, HttpClient client, Uri packageBaseAddress, Uri registration, Uri search)
    {
        _index = index;
        _client = client;
        _packageBaseAddress = packageBaseAddress;
        _registration = registration;
        _search = search;
    }

    public Task<Stream> GetPackageContentStreamAsync(string id, NuGetVersion version)
    {
        id = id.ToLower();
        return _client.GetStreamAsync(new Uri(_packageBaseAddress,
            new Uri($"{id}/{version}/{id}.{version}.nupkg", UriKind.Relative)));
    }

    public Task<Registration> GetPackageRegistrationAsync(string id, NuGetVersion version)
    {
        return _client.GetFromJsonAsync<Registration>(
            new Uri(_registration,
                new Uri($"{id.ToLower()}/{version}.json", UriKind.Relative)),
            SerializerOptions
        )!;
    }

    public Task<RegistrationRoot> GetPackageRegistrationRootAsync(string id)
    {
        return _client.GetFromJsonAsync<RegistrationRoot>(
            new Uri(_registration,
                new Uri($"{id.ToLower()}/index.json", UriKind.Relative)),
            SerializerOptions
        )!;
    }

    public Task<CatalogEntry> GetPackageCatalogEntryAsync(string url)
    {
        return _client.GetFromJsonAsync<CatalogEntry>(url, SerializerOptions)!;
    }

    public Task<SearchResult> SearchPackagesAsync(string? query = null, int? skip = null, int? take = null,
        bool? includePrerelease = null, NuGetVersion? minVersion = null, string? packageType = null)
    {
        var queryParameters = HttpUtility.ParseQueryString(string.Empty);

        if (!string.IsNullOrEmpty(query))
            queryParameters.Add("q", query);

        if (skip.HasValue)
            queryParameters.Add("skip", skip.Value.ToString());

        if (take.HasValue)
            queryParameters.Add("take", take.Value.ToString());

        if (includePrerelease.HasValue)
            queryParameters.Add("prerelease", includePrerelease.Value.ToString());

        if (minVersion is not null)
            queryParameters.Add("semVerLevel", minVersion.ToString());

        if (!string.IsNullOrEmpty(packageType))
            queryParameters.Add("packageType", packageType);

        var builder = new UriBuilder(_search)
        {
            Query = queryParameters.ToString()
        };

        return _client.GetFromJsonAsync<SearchResult>(builder.Uri, SerializerOptions)!;
    }

    public static async Task<NuGetClient?> CreateFromIndexUrlAsync(string indexUrl, HttpClient client)
    {
        var index = await client.GetFromJsonAsync<NuGetIndex>(indexUrl, SerializerOptions);

        var (packageBaseAddress, _, _) = index!.Resources.First(b => b.Type.Id == "PackageBaseAddress");
        var (registration, _, _) = index.Resources.First(b => b.Type.Id == "RegistrationsBaseUrl");
        var (search, _, _) = index.Resources.First(b => b.Type.Id == "SearchQueryService");

        if (!packageBaseAddress.EndsWith('/'))
            packageBaseAddress += '/';
        if (!registration.EndsWith('/'))
            registration += '/';

        return new NuGetClient(new Uri(indexUrl), client, new Uri(packageBaseAddress), new Uri(registration), new Uri(search));
    }

    public override string ToString() => _index.ToString();

    public override bool Equals(object? obj) => obj is NuGetClient { _index: { } index } && index == _index;

    public override int GetHashCode() => _index.GetHashCode();
}