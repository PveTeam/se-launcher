using System.Collections.Immutable;

namespace NuGet;

public class PackageSourceMapping(ImmutableArray<PackageSource> sources)
{
    private readonly ImmutableArray<(string pattern, Task<NuGetClient> client)> _clients = [
        ..sources.Select(b =>
            (b.Pattern,
                NuGetClient.CreateFromIndexUrlAsync(b.Url)))
    ];

    public Task<NuGetClient> GetClientAsync(string packageId) =>
        _clients.FirstOrDefault(b => packageId.StartsWith(b.pattern)).client;
}

public record PackageSource(string Pattern, string Url);