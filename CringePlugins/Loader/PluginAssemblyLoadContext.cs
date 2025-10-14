using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Utils;
using dnlib.DotNet;
using SharedCringe.Loader;

namespace CringePlugins.Loader;

internal class PluginAssemblyLoadContext : DerivedAssemblyLoadContext, ICoreLoadContext
{
    //todo: refactor?
    public static readonly ConcurrentDictionary<string, Assembly> TypeToAssembly = [];

    private readonly string _entrypointPath;
    private readonly AssemblyDependencyResolver _dependencyResolver;
    private readonly HashSet<string> _loadedTypes = [];
    private Assembly? _assembly;
    private readonly AssemblyName _entrypointName;

    internal PluginAssemblyLoadContext(ICoreLoadContext parentContext, string entrypointPath, AssemblyDependencyResolver dependencyResolver) : base(parentContext, $"Plugin Context {Path.GetFileNameWithoutExtension(entrypointPath)}")
    {
        _entrypointPath = entrypointPath;
        _dependencyResolver = dependencyResolver;
        _entrypointName = AssemblyName.GetAssemblyName(entrypointPath);

        Unloading += OnUnload;
    }

    public Assembly LoadEntrypoint()
    {
        if (_assembly is not null)
            return _assembly;

        _assembly = LoadAssemblyFile(_entrypointPath);
        
        var moduleDef = ModuleDefMD.Load(_assembly.GetMainModule(), IntrospectionContext.Global.Context);

        foreach (var type in moduleDef.GetTypes())
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
        return ResolveFromAssemblyName(assemblyName) ?? base.Load(assemblyName);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        if (_dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName) is { } path)
            return LoadUnmanagedDllFromPath(path);
        
        var handle = ResolveUnmanagedDll(unmanagedDllName);
        return handle != nint.Zero ? handle : base.LoadUnmanagedDll(unmanagedDllName);
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

    public Assembly? ResolveFromAssemblyName(AssemblyName assemblyName)
    {
        return AssemblyName.ReferenceMatchesDefinition(assemblyName, _entrypointName) ? LoadEntrypoint() : base.Load(assemblyName);
    }

    public nint ResolveUnmanagedDll(string unmanagedDllName)
    {
        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}