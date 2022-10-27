using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using SharedCringe.Loader;

namespace CringePlugins.Loader;

internal class PluginAssemblyLoadContext : DerivedAssemblyLoadContext
{
    private readonly string _entrypointPath;
    private readonly AssemblyDependencyResolver _dependencyResolver;
    private Assembly? _assembly;

    internal PluginAssemblyLoadContext(ICoreLoadContext parentContext, string entrypointPath) : base(parentContext, $"Plugin Context {Path.GetFileNameWithoutExtension(entrypointPath)}")
    {
        _entrypointPath = entrypointPath;
        _dependencyResolver = new(entrypointPath);
    }

    public Assembly LoadEntrypoint() => _assembly ??= LoadFromAssemblyPath(_entrypointPath);

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (_dependencyResolver.ResolveAssemblyToPath(assemblyName) is { } path)
            return LoadFromAssemblyPath(path);
        
        return base.Load(assemblyName);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        if (_dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName) is { } path)
            return LoadUnmanagedDllFromPath(path);
        
        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}