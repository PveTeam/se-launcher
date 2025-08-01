using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;

namespace CringeBootstrap;

public class GameDirectoryAssemblyLoadContext : AssemblyLoadContext, ICoreLoadContext
{
    private readonly string _dir;
    private readonly string _unmanagedAssembliesDir;

    private static readonly ImmutableHashSet<string> ReferenceAssemblies = ["netstandard"];
    // Assembly simple names are case-insensitive per the runtime behavior
    // (see SimpleNameToFileNameMapTraits for the TPA lookup hash).
    private readonly Dictionary<string, string> _assemblyNames = new(StringComparer.OrdinalIgnoreCase);

    public GameDirectoryAssemblyLoadContext(string dir, string unmanagedAssembliesDir) : base("CringeBootstrap")
    {
        _dir = dir;
        _unmanagedAssembliesDir = unmanagedAssembliesDir;
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

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var key = assemblyName.Name ?? assemblyName.FullName[..','];

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

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        // if specified name is a path, skip to default logic 
        if (unmanagedDllName.AsSpan().ContainsAny(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
            return base.LoadUnmanagedDll(unmanagedDllName);

        // prefer System32 over ours
        // avoid using _dir because it may be a crossgen directory without unmanaged assemblies
        ReadOnlySpan<string> dirs = [Environment.SystemDirectory, _unmanagedAssembliesDir];
        foreach (var dir in dirs)
        {
            var path = Path.Join(dir, unmanagedDllName);
            if (!Path.HasExtension(path))
                path += ".dll";

            if (File.Exists(path))
                return LoadUnmanagedDllFromPath(path);
        }

        throw new DllNotFoundException($"Unable to load {unmanagedDllName}, module not found in valid locations");
    }

    public Assembly? ResolveFromAssemblyName(AssemblyName assemblyName) => Load(assemblyName);
    public nint ResolveUnmanagedDll(string unmanagedDllName) => LoadUnmanagedDll(unmanagedDllName);
}