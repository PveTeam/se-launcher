using System.Collections.Immutable;
using System.Reflection;
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
        var assemblyName = AssemblyName.GetAssemblyName(inputAssembly);
        File.Copy(inputAssembly, Path.Join(cacheDirectory, $"{assemblyName.Name}.dll"), true);
        return ValueTask.FromResult(true);
    }
}