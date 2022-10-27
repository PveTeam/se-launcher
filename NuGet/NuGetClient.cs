using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using NuGet.Converters;
using NuGet.Models;
using NuGet.Versioning;

namespace NuGet;

public class NuGetClient
{
    private readonly HttpClient _client;
    private readonly Uri _packageBaseAddress;
    private readonly Uri _registration;

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

    private NuGetClient(HttpClient client, Uri packageBaseAddress, Uri registration)
    {
        _client = client;
        _packageBaseAddress = packageBaseAddress;
        _registration = registration;
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

    public static async Task<NuGetClient> CreateFromIndexUrlAsync(string indexUrl)
    {
        var client = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        });

        var index = await client.GetFromJsonAsync<NuGetIndex>(indexUrl, SerializerOptions);
        
        var (packageBaseAddress, _, _) = index!.Resources.First(b => b.Type.Id == "PackageBaseAddress");
        var (registration, _, _) = index!.Resources.First(b => b.Type.Id == "RegistrationsBaseUrl");

        if (!packageBaseAddress.EndsWith('/'))
            packageBaseAddress += '/';
        if (!registration.EndsWith('/'))
            registration += '/';
        
        return new NuGetClient(client, new Uri(packageBaseAddress), new Uri(registration));
    }
}