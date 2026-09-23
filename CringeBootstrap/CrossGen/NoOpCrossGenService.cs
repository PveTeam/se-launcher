using System.Collections.Immutable;
using System.Reflection;
using CringeBootstrap.Transformers;
using SharedCringe.Abstractions.Transformers;

namespace CringeBootstrap.CrossGen;

internal class NoOpCrossGenService(
    string gameDirectoryPath,
    string cachePath,
    string cacheKey,
    ITransformationService transformationService,
    ImmutableArray<ITransformer> transformers)
    : CrossGenService(gameDirectoryPath, cacheKey, transformationService, transformers)
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
        var assemblyName = AssemblyName.GetAssemblyName(inputAssembly);
        File.Copy(inputAssembly, Path.Join(cacheDirectory, $"{assemblyName.Name}.dll"), true);
        return ValueTask.FromResult(true);
    }
}
