using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Abstractions.Loader;
using CringePlugins.Utils;
using dnlib.DotNet;
using SharedCringe.Abstractions.Transformers;
using SharedCringe.Loader;

namespace CringePlugins.Loader;

internal class PluginAssemblyLoadContext : DerivedAssemblyLoadContext, ICoreLoadContext
{
    //todo: refactor?
    public static readonly ConcurrentDictionary<string, Assembly> TypeToAssembly = [];

    private readonly string _entrypointPath;
    private readonly AssemblyDependencyResolver _dependencyResolver;
    private readonly ITransformationService _transformationService;
    private readonly IPluginProvider _provider;
    private readonly PluginMetadata _metadata;
    private readonly HashSet<string> _loadedTypes = [];
    private Assembly? _assembly;
    private readonly AssemblyName _entrypointName;

    internal PluginAssemblyLoadContext(ICoreLoadContext parentContext,
        string entrypointPath,
        AssemblyDependencyResolver dependencyResolver,
        ITransformationService transformationService,
        IPluginProvider provider, PluginMetadata metadata) : base(parentContext,
        $"Plugin Context {Path.GetFileNameWithoutExtension(entrypointPath)}")
    {
        _entrypointPath = entrypointPath;
        _dependencyResolver = dependencyResolver;
        _transformationService = transformationService;
        _provider = provider;
        _metadata = metadata;
        _entrypointName = AssemblyName.GetAssemblyName(entrypointPath);

        Unloading += OnUnload;
    }

    public Assembly LoadEntrypoint()
    {
        if (_assembly is not null)
            return _assembly;

        var transformers = _provider.PrepareTransformers(_metadata);
        using var transaction = _transformationService.PrepareTransaction(transformers, this);

        var token = _transformationService.PrepareTransformation(transaction, _entrypointPath);

        var entrypointPath = _entrypointPath;
        if (token is not null)
        {
            if (transformers is not [])
                Transform(token, ref entrypointPath);
                
            foreach (var type in token.ModuleDef.GetTypes())
            {
                var name = type.FullName?.Replace('/', '+');

                if (string.IsNullOrEmpty(name) || !_loadedTypes.Add(name))
                    continue;
            }
        }

        _assembly = LoadAssemblyFile(entrypointPath);
        
        foreach (var loadedType in _loadedTypes)
        {
            TypeToAssembly[loadedType] = _assembly;
        }

        return _assembly;
    }

    private void Transform(ITransformationToken token, ref string entrypointPath)
    {
        // todo ask transformers for their version token, hash them and lookup assembly cache

        entrypointPath = Path.Join(Path.GetTempPath(),
            $"{Path.GetFileNameWithoutExtension(entrypointPath.AsSpan())}.{Path.GetRandomFileName()}.dll");
        
        _transformationService.Transform(token, entrypointPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (AssemblyName.ReferenceMatchesDefinition(assemblyName, _entrypointName))
            return LoadEntrypoint();
        if (_dependencyResolver.ResolveAssemblyToPath(assemblyName) is { } path)
            return LoadAssemblyFile(path);
        return base.Load(assemblyName);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        return _dependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName) is { } path
            ? LoadUnmanagedDllFromPath(path)
            : base.LoadUnmanagedDll(unmanagedDllName);
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

    public Assembly? ResolveFromAssemblyName(AssemblyName assemblyName) => Load(assemblyName);

    public nint ResolveUnmanagedDll(string unmanagedDllName) => LoadUnmanagedDll(unmanagedDllName);
}
