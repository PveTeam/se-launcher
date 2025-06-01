using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace NuGet;

public class PackageSourceMapping(ImmutableArray<PackageSource> sources)
{
    private readonly ImmutableArray<(string pattern, Task<NuGetClient?> client)> _clients = [
        ..sources.Select(b =>
            (b.Pattern,
                NuGetClient.CreateFromIndexUrlAsync(b.Url)))
    ];

    public Task<NuGetClient?> GetClientAsync(string packageId) =>
        _clients.FirstOrDefault(b => Regex.IsMatch(packageId, b.pattern)).client;

    public ConfiguredCancelableAsyncEnumerable<NuGetClient?>.Enumerator GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return _clients.ToAsyncEnumerable()
            .SelectAwait(b => new ValueTask<NuGetClient?>(b.client))
            .WithCancellation(cancellationToken)
            .GetAsyncEnumerator();
    }
}

public record PackageSource(string Name, [StringSyntax("Regex")] string Pattern, [StringSyntax("Uri")] string Url);