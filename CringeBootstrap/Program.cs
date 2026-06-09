using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using CringeBootstrap;
using CringeBootstrap.Abstractions;
using CringeBootstrap.CrossGen;
using CringeBootstrap.Transformers;
using CringeBootstrap.Transformers.Impl;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Velopack;

// #if DEBUG
// while (!Debugger.IsAttached)
//     Thread.Sleep(100);
// #endif

VelopackApp.Build().Run();

if (args.Length == 0)
{
#if WINDOWS
    var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CringeLauncher",
        "current", "CringeBootstrap.exe");

    Console.Write("Set your Launch Options under ");
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("Space Engineers -> Properties -> Launch Options");
    Console.ResetColor();
    Console.WriteLine(" in steam and launch the game as usual");
    Console.WriteLine();
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Gray;
    Console.WriteLine($"\"{path}\" %command%");
    Console.ResetColor();
    Console.Read();
    return 0;
#else
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("not a command line tool");
    Console.ResetColor();
    return 1;
#endif
}

#if DEBUG
AssemblyLoadContext.Default.Resolving += (loadContext, name) =>
{
    Debug.WriteLine($"resolving {name} in {loadContext}");
    return null;
};
#endif

SharedCringe.Utils.NLogLogging.Init();

var logger = LogManager.GetLogger("CringeBootstrap");
logger.Info("Bootstrapping {DotnetVersion} {RuntimeIdentifier} OS {OsDescription}", 
    RuntimeInformation.FrameworkDescription, 
    RuntimeInformation.RuntimeIdentifier, 
    RuntimeInformation.OSDescription);

var dirIndex = Array.FindIndex(args, b => b.EndsWith("SpaceEngineers.exe"));
var dir = Path.GetDirectoryName(args[dirIndex])!;
args = args[dirIndex..];
var gameDir = dir;

var customEntrypoint = Environment.GetEnvironmentVariable("DOTNET_BOOTSTRAP_ENTRYPOINT");

var cacheKey = GameCacheKey.FromDirectory(gameDir).Value;

var transformationService = new TransformationService(gameDir, [
    new ImageSharpTransformer(), 
    new DebugSymbolsTransformer(cacheKey),
#if !WINDOWS
    new DllImportTransformer(),
    new SharpDxTransformer(),
#endif
]);
var cacheDir = Directory.CreateDirectory(Path.Join(
    Environment.GetEnvironmentVariable("DOTNET_USERDEV_RUNDIR") ??
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "CringeLauncher", "cache"));

CrossGenResult? result = null;
CrossGenService crossGenService = new CrossGenServiceImpl(gameDir, cacheDir.FullName, cacheKey, transformationService);
if (!args.Contains("--skip-crossgen", StringComparer.OrdinalIgnoreCase))
{
    result = RunCrossGen(crossGenService);
}
if (result is null or { Failed: true })
{
    if (result is null) logger.Info("Running without crossgen as it has been skipped");
    else if (result.Failed) logger.Info("Running without crossgen as it has failed");
    
    crossGenService = new NoOpCrossGenService(gameDir, cacheDir.FullName, cacheKey, transformationService);
        
    result = RunCrossGen(crossGenService);
}
dir = result.CacheDirectory;

CrossGenResult RunCrossGen(CrossGenService crossGen)
{
    CrossGenResult crossGenResult;
    try
    {
        var crossGenTask = crossGen.RunCrossGenAsync();
        crossGenResult = crossGenTask.IsCompletedSuccessfully
            ? crossGenTask.Result
            : crossGenTask.AsTask().GetAwaiter().GetResult();
    }
    catch (Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("Crossgen encountered a fatal error and will be skipped for this session.");
        Console.ResetColor();
        
        logger.Error(e, "Crossgen has failed");

        crossGenResult = new(gameDir, Failed: true);
    }

    return crossGenResult;
}

var context = new GameDirectoryAssemblyLoadContext(dir, gameDir, customEntrypoint is not null);

// a list of assemblies which are not in the game binaries but reference them
context.AddDependencyOverride("CringeLauncher");
context.AddDependencyOverride("CringePlugins");
context.AddDependencyOverride("EOSSDK");

const string crashPadEntrypoint = "CringeLauncher.CrashPad.CrashPadLauncher, CringeLauncher";

var entrypoint = customEntrypoint ?? crashPadEntrypoint;

var isCrashPad = entrypoint.Equals(crashPadEntrypoint);

if (!TypeName.TryParse(entrypoint, out var entrypointName) || 
    entrypointName.AssemblyName is null)
{
    if (!Console.IsInputRedirected)
    {
        Console.Error.WriteLine($"Invalid entrypoint name: {entrypoint}");
        Console.Error.WriteLine("Bootstrap encountered a fatal error and will shutdown.");
        Console.Read();
    }
    return 1;
}

logger.Info("Selected entrypoint {EntrypointName}", entrypoint);

var launcher = context.LoadFromAssemblyName(entrypointName.AssemblyName.ToAssemblyName());

using var corePlugin = (ICorePlugin) launcher.CreateInstance(entrypointName.FullName)!;

var services = new ServiceCollection();
services.AddSingleton<ICrossGenService>(crossGenService);
services.AddSingleton(corePlugin);

do
{
    if (!corePlugin.Initialize(args, services) || !corePlugin.Run())
    {
        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }
        return 1;
    }
}
while (isCrashPad && corePlugin.RestartRequested);

return corePlugin.RestartRequested ? -2 : 0;
