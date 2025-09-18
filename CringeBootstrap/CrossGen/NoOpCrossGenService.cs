using System.Collections.Immutable;
using CringeBootstrap.Transformers;

namespace CringeBootstrap.CrossGen;

internal class NoOpCrossGenService(string gameDirectoryPath, string cachePath, ITransformationService transformationService)
    : CrossGenService(gameDirectoryPath, cachePath, transformationService)
{
    protected override string CrossGenCachePath { get; } =
        Directory.CreateDirectory(Path.Join(cachePath, "NOOP")).FullName;

    protected override Task<string?> DownloadCrossGenAsync()
    {
        return Task.FromResult<string?>("dummy");
    }

    protected override ValueTask<bool> RunCrossGenAsync(string crossGenPath, IEnumerable<string> inputReferences, string cacheDirectory,
        string inputAssembly)
    {
        File.Copy(inputAssembly, Path.Join(cacheDirectory, Path.GetFileName(inputAssembly)), true);
        return ValueTask.FromResult(true);
    }
}