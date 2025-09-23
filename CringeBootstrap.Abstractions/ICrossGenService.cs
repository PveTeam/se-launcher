namespace CringeBootstrap.Abstractions;
public interface ICrossGenService
{
    string CacheKey { get; }

    ValueTask<bool> RunCrossGenAsync(IEnumerable<string> inputReferences, string cacheDirectory, string inputAssembly);
}
