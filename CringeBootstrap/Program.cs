using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using CringeBootstrap;
using CringeBootstrap.Abstractions;
using CringeBootstrap.CrossGen;
using CringeBootstrap.Transformers;
using CringeBootstrap.Transformers.Impl;
using Microsoft.Extensions.DependencyInjection;
using Velopack;

#if DEBUG
while (!Debugger.IsAttached)
    Thread.Sleep(100);
#endif

VelopackApp.Build().Run();

if (args.Length == 0)
{
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
}

#if DEBUG
AssemblyLoadContext.Default.Resolving += (loadContext, name) =>
{
    Debug.WriteLine($"resolving {name} in {loadContext}");
    return null;
};
#endif

var dir = Path.GetDirectoryName(args[0])!;
var gameDir = dir;

var customEntrypoint = Environment.GetEnvironmentVariable("DOTNET_BOOTSTRAP_ENTRYPOINT");

var transformationService = new TransformationService(gameDir, [new ImageSharpTransformer()]);
var cacheDir = Directory.CreateDirectory(Path.Join(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "CringeLauncher", "cache"));

CrossGenResult? result = null;
CrossGenService crossGenService = new CrossGenServiceImpl(gameDir, cacheDir.FullName, transformationService);
if (!args.Contains("--skip-crossgen", StringComparer.OrdinalIgnoreCase))
{
    result = RunCrossGen(crossGenService);
}
if (result is null or { Failed: true })
{
    if (result is null) Console.WriteLine("Running without crossgen as it has been skipped");
    else if (result.Failed) Console.WriteLine("Running without crossgen as it has failed");
    
    crossGenService = new NoOpCrossGenService(gameDir, cacheDir.FullName, transformationService);
        
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
        Console.WriteLine(e);

        crossGenResult = new(gameDir, Failed: true);
    }

    return crossGenResult;
}

var context = new GameDirectoryAssemblyLoadContext(dir, gameDir);

// a list of assemblies which are not in the game binaries but reference them
context.AddDependencyOverride("CringeLauncher");
context.AddDependencyOverride("CringePlugins");
context.AddDependencyOverride("EOSSDK");

const string CrashPadEntrypoint = "CringeLauncher.CrashPad.CrashPadLauncher, CringeLauncher";

var entrypoint = customEntrypoint ?? CrashPadEntrypoint;

var isCrashPad = entrypoint.Equals(CrashPadEntrypoint);

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