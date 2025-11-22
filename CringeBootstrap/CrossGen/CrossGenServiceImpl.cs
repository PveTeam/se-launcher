using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using CringeBootstrap.Transformers;
using NuGet;
using NuGet.Versioning;

namespace CringeBootstrap.CrossGen;

internal class CrossGenServiceImpl(string gameDirectoryPath, string cachePath, ITransformationService transformationService)
    : CrossGenService(gameDirectoryPath, cachePath, transformationService)
{
    protected override string CrossGenCachePath { get; } =
        Directory.CreateDirectory(Path.Join(cachePath, "R2R")).FullName;

    protected override async Task<string?> DownloadCrossGenAsync()
    {
        const string nugetUrl = "https://api.nuget.org/v3/index.json";
        const string toolName = "crossgen2.exe";
        const string packageId = "Microsoft.NETCore.App.Crossgen2.win-x64";
        var nugetCachePath = Path.Join(cachePath, "x64", $"net{Environment.Version.Major}.{Environment.Version.Minor}");

        var packagePath =
            Directory.CreateDirectory(Path.Join(nugetCachePath, packageId, Environment.Version.ToString()));
        var toolPath = Path.Join(packagePath.FullName, "tools", toolName);
        if (File.Exists(toolPath))
            return toolPath;

        using var httpClient = new HttpClient();
        try
        {
            var client = await NuGetClient.CreateFromIndexUrlAsync(nugetUrl, httpClient);

            if (!packagePath.Exists) packagePath.Create();

            await using var stream =
                await client.GetPackageContentStreamAsync(packageId, new NuGetVersion(Environment.Version));
            await using var archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Read, true, null);
            await archive.ExtractToDirectoryAsync(packagePath.FullName);

            if (!File.Exists(toolPath))
            {
                LogCrossGenException("Failed to find crossgen",
                    new FileNotFoundException("Failed to find crossgen", toolPath));
                return null;
            }
        }
        catch (IOException e)
        {
            LogCrossGenException("Failed to extract crossgen", e);
            return null;
        }
        catch (Exception e)
        {
            LogCrossGenException("Failed to download crossgen", e);
            return null;
        }

        return toolPath;
    }

    protected override async ValueTask<bool> RunCrossGenAsync(string crossGenPath, IEnumerable<string> inputReferences,
        string cacheDirectory,
        string inputAssembly)
    {
        var startInfo = new ProcessStartInfo(crossGenPath, [
            "--targetos:windows",
            "--targetarch:x64",
            "--Ot",
            ..inputReferences.SelectMany(x => new[] { "-r", x }),
            "--out", Path.Join(cacheDirectory, Path.GetFileName(inputAssembly)),
            inputAssembly
        ])
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var process = Process.Start(startInfo);

        var outputStringBuilder = new StringBuilder();
        var errorStringBuilder = new StringBuilder();
        if (process is not null)
        {
            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    outputStringBuilder.AppendLine(args.Data);
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    errorStringBuilder.AppendLine(args.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
        }

        if (process is not null && process.ExitCode == 0) return true;

        string? logFilePath = null;
        if (process is not null)
        {
            logFilePath = Path.Join(cachePath, $"{Path.GetFileName(inputAssembly)}.log");
            await File.WriteAllTextAsync(logFilePath, outputStringBuilder.ToString());
            await File.AppendAllTextAsync(logFilePath, errorStringBuilder.ToString());
        }

        LogCrossGenException(
            $"Crossgen failed! {(logFilePath is not null ? $"Log saved to: {logFilePath}" : string.Empty)} Skipping crossgen",
            new Exception($"Crossgen failed for {inputAssembly}"));
        return false;
    }
}