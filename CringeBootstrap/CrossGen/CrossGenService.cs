using CringeBootstrap.Abstractions;
using CringeBootstrap.Transformers;
using NuGet.Deps;
using System.Collections.Immutable;
using NLog;

namespace CringeBootstrap.CrossGen;

internal abstract class CrossGenService(string gameDirectoryPath, string cacheKey, ITransformationService transformationService) : ICrossGenService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    public string CacheKey { get; } = $"{FormatVersion}_{cacheKey}_{Environment.Version}";

    private string? _crossGenPath;

    private const string FormatVersion = "1";
    
    private readonly ImmutableHashSet<string> _excludedAssemblies =
    [
        "VRage.NativeAftermath.dll", // managed C++ is not supported
        "Sandbox.Common.dll", // game assemblies are kept excluded from crossgen to avoid harmony patches failing from JIT inlining
        "Sandbox.Game.dll", // TODO move patches to be ahead of time (long-term)
        "Sandbox.Graphics.dll",
        "Sandbox.RenderDirect.dll",
        "SpaceEngineers.Game.dll",
        "SpaceEngineers.ObjectBuilders.dll",
        "VRage.Ansel.dll",
        "VRage.Audio.dll",
        "VRage.dll",
        "VRage.EOS.dll",
        "VRage.Game.dll",
        "VRage.Input.dll",
        "VRage.Library.dll",
        "VRage.Math.dll",
        "VRage.Mod.Io.dll",
        "VRage.NativeWrapper.dll",
        "VRage.Network.dll",
        "VRage.Platform.Windows.dll",
        "VRage.Render.dll",
        "VRage.Render11.dll",
        "VRage.Scripting.dll",
        "VRage.Steam.dll",
        "VRage.UserInterface.dll",
    ];

    private readonly ImmutableHashSet<string> _includedAssemblies =
    [
        "CppNet.dll",
        "DirectShowLib.dll",
        "EmptyKeys.UserInterface.Core.dll",
        "EmptyKeys.UserInterface.dll",
        "GameAnalytics.Mono.dll",
        "HavokWrapper.dll",
        "netstandard.dll",
        "ProtoBuf.Net.Core.dll",
        "ProtoBuf.Net.dll",
        "RecastDetourWrapper.dll",
        "RestSharp.dll",
        "Sandbox.Game.XmlSerializers.dll",
        "SharpDX.D3DCompiler.dll",
        "SharpDX.Desktop.dll",
        "SharpDX.Direct3D11.dll",
        "SharpDX.DirectInput.dll",
        "SharpDX.dll",
        "SharpDX.DXGI.dll",
        "SharpDX.XAudio2.dll",
        "SharpDX.XInput.dll",
        "SpaceEngineers.ObjectBuilders.XmlSerializers.dll",
        "System.Data.SQLite.dll",
        "VRage.EOS.XmlSerializers.dll",
        "VRage.Game.XmlSerializers.dll",
        "VRage.Math.XmlSerializers.dll",
        "VRage.XmlSerializers.dll",
    ];

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private volatile ImmutableHashSet<string>? _defaultReferences;

    protected abstract string CrossGenCachePath { get; }

    public void CleanCache()
    {
        foreach (var directory in Directory.EnumerateDirectories(CrossGenCachePath))
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                const string message = "Failed to clean previous crossgen cache";
                Console.WriteLine(message);
                Console.ResetColor();
                
                Log.Warn(e, message);
            }
        }
    }

    /// <summary>
    /// Run crossgen and return path to game assemblies directory
    /// </summary>
    /// <returns>The path to game assemblies directory either original or R2R</returns>
    public async ValueTask<CrossGenResult> RunCrossGenAsync()
    {
        _crossGenPath = await DownloadCrossGenAsync();
        var cacheDirectory = Path.Join(CrossGenCachePath, CacheKey);
        var finishedMarkerPath = Path.Join(cacheDirectory, ".finished");
        if (Directory.Exists(cacheDirectory) && File.Exists(finishedMarkerPath))
        {
            Log.Info("Crossgen cache hit");
            return new(cacheDirectory, CacheHit: true);
        }

        Log.Info("Starting coldstart crossgen");

        CleanCache();

        if (_crossGenPath is null)
            return new(gameDirectoryPath, Failed: true);

        var inputAssemblies = CollectInputAssemblies();
        ImmutableHashSet<string> references =
            [..await CollectFrameworkReferencesAsync(), ..inputAssemblies];
        references = references.WithComparer(StringComparer.OrdinalIgnoreCase);

        _defaultReferences = references;

        Directory.CreateDirectory(cacheDirectory);

        for (var index = 0; index < inputAssemblies.Length; index++)
        {
            var inputAssembly = inputAssemblies[index];
            var inputReferences = references.Remove(inputAssembly);
            
            TransformInputAssembly(ref inputAssembly);

            Console.WriteLine($"Running crossgen... {index / (inputAssemblies.Length - 1.0):P0}");
            var success = await RunCrossGenAsync(_crossGenPath, inputReferences, cacheDirectory, inputAssembly);

            if (success) continue;
            
            CleanCache();
            return new(gameDirectoryPath, Failed: true);
        }
        
        Console.WriteLine("Completing leftover transformations...");

        foreach (var excludedAssembly in _excludedAssemblies)
        {
            var inputAssemblyPath = Path.Join(gameDirectoryPath, excludedAssembly);
            if (!File.Exists(inputAssemblyPath)) continue;

            TransformInputAssembly(ref inputAssemblyPath);
            
            File.Copy(inputAssemblyPath, Path.Join(cacheDirectory, excludedAssembly), true);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        const string crossgenFinishedMessage = "Crossgen finished";
        Console.WriteLine(crossgenFinishedMessage);
        Console.ResetColor();
        Log.Info(crossgenFinishedMessage);
        await File.WriteAllTextAsync(finishedMarkerPath, "OK");
        return new(cacheDirectory);
    }

    protected abstract Task<string?> DownloadCrossGenAsync();

    private void TransformInputAssembly(ref string inputAssemblyPath)
    {
        var token = transformationService.PrepareTransformation(inputAssemblyPath);
        if (token is null) return;

        inputAssemblyPath = Path.Join(Path.GetTempPath(), Path.GetRandomFileName() + ".dll");
        transformationService.Transform(token, inputAssemblyPath);
    }
    
    private static async Task<ImmutableArray<string>> CollectFrameworkReferencesAsync()
    {
        var dotnetRoot = Environment.GetEnvironmentVariable("DOTNET_ROOT") ??
                         Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet");
        var dotnetPacksPath = Path.Join(dotnetRoot, "shared");
        if (!Directory.Exists(dotnetPacksPath))
            throw new Exception($"Dotnet shared packs not found in {dotnetPacksPath}");

        const string runtimePackId = "Microsoft.NETCore.App";
        const string desktopPackId = "Microsoft.WindowsDesktop.App";

        return
        [
            ..await CollectFrameworkReferencesFromPackAsync(dotnetPacksPath, runtimePackId),
#if WINDOWS
            ..await CollectFrameworkReferencesFromPackAsync(dotnetPacksPath, desktopPackId)
#endif
        ];
    }

    private static async ValueTask<ImmutableArray<string>> CollectFrameworkReferencesFromPackAsync(string packsPath,
        string packId)
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

    private ImmutableArray<string> CollectInputAssemblies() => [
            .._includedAssemblies.Except(_excludedAssemblies)
                .Select(s => Path.Join(gameDirectoryPath, s))
                .Where(File.Exists)
        ];

    protected static void LogCrossGenException(string message, Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();
        
        Log.Error(e, message);
    }

    private async Task<ImmutableHashSet<string>> GetDefaultReferences()
    {
        ImmutableHashSet<string> references = [.. await CollectFrameworkReferencesAsync(), .. CollectInputAssemblies()];
        return references.WithComparer(StringComparer.OrdinalIgnoreCase);
    }

    protected abstract ValueTask<bool> RunCrossGenAsync(string crossGenPath, IEnumerable<string> inputReferences, string cacheDirectory,
        string inputAssembly);

    async ValueTask<bool> ICrossGenService.RunCrossGenAsync(IEnumerable<string> inputReferences, string cacheDirectory, string inputAssembly)
    {
        if (_defaultReferences is null)
        {
            await _semaphore.WaitAsync();
            try
            {
                _defaultReferences ??= await GetDefaultReferences();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        return await RunCrossGenAsync(_crossGenPath ?? throw new InvalidOperationException("Using crossgen before bootstrap"),
            inputReferences.Concat(_defaultReferences), cacheDirectory, inputAssembly);
    }
}
