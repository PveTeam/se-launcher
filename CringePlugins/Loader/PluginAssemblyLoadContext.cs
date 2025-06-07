using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Utils;
using SharedCringe.Loader;

namespace CringePlugins.Loader;

internal class PluginAssemblyLoadContext : DerivedAssemblyLoadContext
{
    //todo: refactor?
    public static readonly ConcurrentDictionary<string, Assembly> TypeToAssembly = [];

    private readonly string _entrypointPath;
    private readonly AssemblyDependencyResolver _dependencyResolver;
    private readonly HashSet<string> _loadedTypes = [];
    private Assembly? _assembly;

    internal PluginAssemblyLoadContext(ICoreLoadContext parentContext, string entrypointPath) : base(parentContext, $"Plugin Context {Path.GetFileNameWithoutExtension(entrypointPath)}")
    {
        _entrypointPath = entrypointPath;
        _dependencyResolver = new(entrypointPath);

        Unloading += OnUnload;
    }

    public Assembly LoadEntrypoint()
    {
        if (_assembly is not null)
            return _assembly;

        _assembly = LoadAssemblyFile(_entrypointPath);

        var module = _assembly.GetMainModule();

        foreach (var type in module.GetTypes())
        {
            var name = type.FullName?.Replace('/', '+');

            if (string.IsNullOrEmpty(name) || !_loadedTypes.Add(name))
                continue;

            TypeToAssembly[name] = _assembly;
        }

        return _assembly;
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (_dependencyResolver.ResolveAssemblyToPath(assemblyName) is { } path)
            return LoadAssemblyFile(path);

        return base.Load(assemblyName);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        if (_dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName) is { } path)
            return LoadUnmanagedDllFromPath(path);

        return base.LoadUnmanagedDll(unmanagedDllName);
    }

    protected virtual Assembly LoadAssemblyFile(string path) => LoadFromAssemblyPath(path);

    private static void OnUnload(AssemblyLoadContext context)
    {
        if (context is not PluginAssemblyLoadContext pluginContext)
            return;

        foreach (var typeStr in pluginContext._loadedTypes)
        {
            TypeToAssembly.Remove(typeStr);
        }
        pluginContext._loadedTypes.Clear();
    }
}