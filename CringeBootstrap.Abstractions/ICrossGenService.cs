namespace CringeBootstrap.Abstractions;
public interface ICrossGenService
{
    string CacheKey { get; }

    ValueTask<bool> RunCrossGenAsync(string crossGenPath, IEnumerable<string> inputReferences, string cacheDirectory,
        string inputAssembly);
}
