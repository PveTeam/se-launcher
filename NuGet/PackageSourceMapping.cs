using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using NLog;

namespace NuGet;

public class PackageSourceMapping(ImmutableArray<PackageSource> sources, HttpClient client)
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private readonly ImmutableArray<(string pattern, Task<NuGetClient> client)> _clients = [
        ..sources.Select(b =>
            (b.Pattern,
                NuGetClient.CreateFromIndexUrlAsync(b.Url, client)))
    ];
    
    public bool SomeSourcesAreUnavailable { get; private set; }

    public ValueTask<NuGetClient?> GetClientAsync(string packageId)
    {
        var clientTask = _clients.FirstOrDefault(b => Regex.IsMatch(packageId, b.pattern)).client;
        return ResolveClientTask(clientTask);
    }

    private async ValueTask<NuGetClient?> ResolveClientTask(Task<NuGetClient> clientTask)
    {
        if (clientTask.Status is TaskStatus.Faulted or TaskStatus.Canceled)
        {
            SomeSourcesAreUnavailable = true;
            return null;
        }
        try
        {
            return await clientTask;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to get client");
            SomeSourcesAreUnavailable = true;
            return null;
        }
    }

    public ConfiguredCancelableAsyncEnumerable<NuGetClient?>.Enumerator GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return _clients.ToAsyncEnumerable()
            .SelectAwait(b => ResolveClientTask(b.client))
            .WithCancellation(cancellationToken)
            .GetAsyncEnumerator();
    }
}

public record PackageSource(string Name, [StringSyntax("Regex")] string Pattern, [StringSyntax("Uri")] string Url);