using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using CringeBootstrap;
using CringeBootstrap.Abstractions;
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
    return;
}

#if DEBUG
AssemblyLoadContext.Default.Resolving += (loadContext, name) =>
{
    Debug.WriteLine($"resolving {name} in {loadContext}");
    return null;
};
#endif

var dir = Path.GetDirectoryName(args[0])!;
var context = new GameDirectoryAssemblyLoadContext(dir);

// a list of assemblies which are not in the game binaries but reference them
context.AddDependencyOverride("CringeLauncher");
context.AddDependencyOverride("CringePlugins");
context.AddDependencyOverride("EOSSDK");

var entrypoint = Environment.GetEnvironmentVariable("DOTNET_BOOTSTRAP_ENTRYPOINT") ??
                                           "CringeLauncher.Launcher, CringeLauncher";
if (!TypeName.TryParse(entrypoint, out var entrypointName) || 
    entrypointName.AssemblyName is null)
{
    Console.Error.WriteLine($"Invalid entrypoint name: {entrypoint}");
    Console.Error.WriteLine("Bootstrap encountered a fatal error and will shutdown.");
    Console.Read();
    return;
}

var launcher = context.LoadFromAssemblyName(entrypointName.AssemblyName.ToAssemblyName());

using var corePlugin = (ICorePlugin) launcher.CreateInstance(entrypointName.FullName)!;

corePlugin.Initialize(args);
corePlugin.Run();