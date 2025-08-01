using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NuGet;
using NuGet.Deps;
using NuGet.Versioning;

namespace CringeBootstrap.CrossGen;

internal class CrossGenService(string gameDirectoryPath, string cachePath)
{
    private readonly ImmutableHashSet<string> _excludedAssemblies = ImmutableHashSet.Create(
        StringComparer.OrdinalIgnoreCase,
        "VRage.NativeAftermath.dll" // managed C++ is not supported
    );
    
    // assembly with game version constant so hash always changes with game updates
    private const string CacheKeyFileName = "SpaceEngineers.Game.dll";
    
    private readonly string _crossGenCachePath = Directory.CreateDirectory(Path.Join(cachePath, "R2R")).FullName;

    public void CleanCache()
    {
        foreach (var directory in Directory.EnumerateDirectories(_crossGenCachePath))
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Failed to clean previous crossgen cache");
                Console.ResetColor();
                Console.WriteLine(e);
            }
        }
    }

    /// <summary>
    /// Run crossgen and return path to game assemblies directory
    /// </summary>
    /// <returns>The path to game assemblies directory either original or R2R</returns>
    public string RunCrossGen()
    {
        var cacheDirectory = Path.Join(_crossGenCachePath, GetCacheKey());
        if (Directory.Exists(cacheDirectory))
        {
            Console.WriteLine("Crossgen cache hit");
            return cacheDirectory;
        }
        
        Console.WriteLine("Starting coldstart crossgen");
        
        CleanCache();
        
        var crossGenPath = DownloadCrossGenAsync().GetAwaiter().GetResult();
        if (crossGenPath is null)
            return gameDirectoryPath;
        
        var inputAssemblies = CollectInputAssemblies();
        ImmutableHashSet<string> references = [..CollectFrameworkReferencesAsync().GetAwaiter().GetResult(), ..inputAssemblies];
        references = references.WithComparer(StringComparer.OrdinalIgnoreCase);
        
        Directory.CreateDirectory(cacheDirectory);

        for (var index = 0; index < inputAssemblies.Length; index++)
        {
            var inputAssembly = inputAssemblies[index];
            var inputReferences = references.Remove(inputAssembly);

            var startInfo = new ProcessStartInfo(crossGenPath, [
                "--targetos:windows",
                "--targetarch:x64",
                "--Ot",
                ..inputReferences.SelectMany(x => new [] { "-r", x }),
                "--out", Path.Join(cacheDirectory, Path.GetFileName(inputAssembly)),
                inputAssembly
            ])
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            
            Console.WriteLine($"Running crossgen... {index/(inputAssemblies.Length-1.0):P0}");
            
            using var process = Process.Start(startInfo);

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
                process.WaitForExit();
            }

            if (process is not null && process.ExitCode == 0) continue;
            
            string? logFilePath = null;
            if (process is not null)
            {
                logFilePath = Path.Join(cachePath, $"{Path.GetFileName(inputAssembly)}.log");
                File.WriteAllText(logFilePath, outputStringBuilder.ToString());
                File.AppendAllText(logFilePath, errorStringBuilder.ToString());
            }
                
            LogCrossGenException($"Crossgen failed! {(logFilePath is not null ? $"Log saved to: {logFilePath}" : string.Empty)} Skipping crossgen", new Exception($"Crossgen failed for {inputAssembly}"));
            CleanCache();
            return gameDirectoryPath;
        }
        
        foreach (var excludedAssembly in _excludedAssemblies)
        {
            var gameAssemblyPath = Path.Join(gameDirectoryPath, excludedAssembly);
            var cacheAssemblyPath = Path.Join(cacheDirectory, excludedAssembly);
            if (File.Exists(gameAssemblyPath))
                File.Copy(gameAssemblyPath, cacheAssemblyPath, true);
        }
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Crossgen finished");
        Console.ResetColor();
        return cacheDirectory;
    }

    private static async Task<ImmutableArray<string>> CollectFrameworkReferencesAsync()
    {
        var dotnetPacksPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared");
        if (!Directory.Exists(dotnetPacksPath))
            throw new Exception($"Dotnet shared packs not found in {dotnetPacksPath}");

        const string runtimePackId = "Microsoft.NETCore.App";
        const string desktopPackId = "Microsoft.WindowsDesktop.App";

        return
        [
            ..await CollectFrameworkReferencesFromPackAsync(dotnetPacksPath, runtimePackId),
            ..await CollectFrameworkReferencesFromPackAsync(dotnetPacksPath, desktopPackId)
        ];
    }

    private static async ValueTask<ImmutableArray<string>> CollectFrameworkReferencesFromPackAsync(string packsPath, string packId)
    {
        var packDirPath = Path.Join(packsPath, packId, Environment.Version.ToString());
        var packPath = Path.Join(packDirPath, $"{packId}.deps.json");
        if (!File.Exists(packPath))
            throw new Exception($"Dotnet shared pack {packId} not found in {packPath}");

        await using var stream = File.OpenRead(packPath);
        var ((runtimeFramework, _), _, targets, _) = await DependencyManifestSerializer.DeserializeAsync(stream);
        var (_, runtime, _) = targets[runtimeFramework].Values.First();
        
        return [..runtime!.Keys.Select(b => Path.Join(packDirPath, b))];
    }

    private ImmutableArray<string> CollectInputAssemblies()
    {
        return [
            ..Directory.EnumerateFiles(gameDirectoryPath, "*.dll")
                .Where(IsManagedAssembly)
        ];
    }

    private bool IsManagedAssembly(string path)
    {
        if (_excludedAssemblies.Contains(Path.GetFileName(path)))
            return false;
        
        try
        {
            AssemblyName.GetAssemblyName(path);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<string?> DownloadCrossGenAsync()
    {
        const string nugetUrl = "https://api.nuget.org/v3/index.json";
        const string toolName = "crossgen2.exe";
        const string packageId = "Microsoft.NETCore.App.Crossgen2.win-x64";
        var nugetCachePath = Path.Join(cachePath, "x64", $"net{Environment.Version.Major}.{Environment.Version.Minor}");

        var packagePath = Directory.CreateDirectory(Path.Join(nugetCachePath, packageId, Environment.Version.ToString()));
        var toolPath = Path.Join(packagePath.FullName, "tools", toolName);
        if (File.Exists(toolPath))
            return toolPath;
        
        using var httpClient = new HttpClient();
        try
        {
            var client = await NuGetClient.CreateFromIndexUrlAsync(nugetUrl, httpClient);

            if (!packagePath.Exists) packagePath.Create();

            await using var stream =
                await client!.GetPackageContentStreamAsync(packageId, new NuGetVersion(Environment.Version));
            await using var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream);
            memStream.Position = 0;
            using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);
            archive.ExtractToDirectory(packagePath.FullName);
            
            if (!File.Exists(toolPath))
            {
                LogCrossGenException("Failed to find crossgen", new FileNotFoundException("Failed to find crossgen", toolPath));
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

    private static void LogCrossGenException(string message, Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        Console.WriteLine(e);
    }

    private string GetCacheKey()
    {
        using var stream = File.OpenRead(Path.Join(gameDirectoryPath, CacheKeyFileName));
        return Convert.ToHexStringLower(SHA256.HashData(stream));
    }
}