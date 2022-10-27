using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;

namespace CringeBootstrap;

public class GameDirectoryAssemblyLoadContext : AssemblyLoadContext, ICoreLoadContext
{
    private static readonly ImmutableHashSet<string> ReferenceAssemblies = ["netstandard"];
    private readonly Dictionary<string, string> _assemblyNames = [];

    public GameDirectoryAssemblyLoadContext(string dir) : base("CringeBootstrap")
    {
        var files = Directory.GetFiles(dir, "*.dll");
        foreach (var file in files)
        {
            if (File.Exists(Path.Join(AppContext.BaseDirectory, Path.GetFileName(file))))
                continue;
            
            try
            {
                var name = AssemblyName.GetAssemblyName(file);
                
                AddOverride(name, file);
            }
            catch (BadImageFormatException)
            {
                // if we are trying to load native image
            }
        }
    }

    public void AddOverride(AssemblyName name, string file)
    {
        var key = name.Name ?? name.FullName[..','];

        if (key.StartsWith("System.") || ReferenceAssemblies.Contains(key))
            return;
        
        _assemblyNames.TryAdd(key, file);
    }

    public void AddDependencyOverride(string name)
    {
        AddOverride(new(name), Path.Join(AppContext.BaseDirectory, name + ".dll"));
    }

    protected override Assembly? Load(AssemblyName name)
    {
        var key = name.Name ?? name.FullName[..','];

        try
        {
            return _assemblyNames.TryGetValue(key, out var value) ? LoadFromAssemblyPath(value) : null;
        }
        catch (BadImageFormatException e)
        {
            Debug.WriteLine(e);
            return null;
        }
    }

    public Assembly? ResolveFromAssemblyName(AssemblyName assemblyName) => Load(assemblyName);
    public nint ResolveUnmanagedDll(string unmanagedDllName) => LoadUnmanagedDll(unmanagedDllName);
}