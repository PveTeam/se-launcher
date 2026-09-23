using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Abstractions.Loader;
using Microsoft.Extensions.DependencyInjection;
using SharedCringe.Abstractions.Transformers;
using SharedCringe.Loader;

namespace CringePlugins.Loader;

internal class AlcFactory : IPluginDependencyContextFactory
{
    private readonly IPluginServiceProviderFactory _serviceProviderFactory;
    private readonly IPluginEntrypoint _pluginEntrypoint;
    private readonly ContextParams? _contextParams;
    public PluginAssemblyLoadContext? Context { get; private set; }

    public AlcFactory(PluginAssemblyLoadContext context, IPluginEntrypoint pluginEntrypoint,
        IPluginServiceProviderFactory serviceProviderFactory)
    {
        _serviceProviderFactory = serviceProviderFactory;
        _pluginEntrypoint = pluginEntrypoint;
        Context = context;
    }
    
    public AlcFactory(string entrypointPath,
        IPluginEntrypoint pluginEntrypoint,
        AssemblyDependencyResolver resolver,
        IPluginServiceProviderFactory serviceProviderFactory,
        AlcFactory? parentInstance,
        bool local,
        ITransformationService transformationService,
        IPluginProvider provider,
        PluginMetadata metadata)
    {
        _serviceProviderFactory = serviceProviderFactory;
        _pluginEntrypoint = pluginEntrypoint;
        _contextParams = new(entrypointPath, resolver, parentInstance, local, transformationService, provider, metadata);
    }

    public bool Local => _contextParams?.Local is true || Context is LocalLoadContext;

    public IPluginDependencyContext Create(
        Action<IServiceCollection, Type> registerServices)
    {
        var context = CreateContext();

        var entrypointAssembly = context.LoadEntrypoint();

        var entrypoint = _pluginEntrypoint.GetEntrypointType(entrypointAssembly);

        var serviceCollection = _serviceProviderFactory.CreateBuilder();
        
        registerServices(serviceCollection, entrypoint);
        
        var scope = _serviceProviderFactory.CreateServiceProviderScope(context, serviceCollection);
        
        return new AlcContext(context, entrypoint, scope);
    }

    private PluginAssemblyLoadContext CreateContext()
    {
        if (Context is not null) return Context;
        if (_contextParams is null)
            throw new InvalidOperationException("AlcFactory requires a context parameter");
        if (AssemblyLoadContext.GetLoadContext(typeof(AlcFactory).Assembly) is not ICoreLoadContext parentContext)
            throw new NotSupportedException("Plugin instantiation is not supported in this context");

        // The parent ALC is created lazily at instantiation; a child that instantiates before its
        // parent (dictionary enumeration is not insertion-ordered) must materialize the parent
        // chain so shared host assemblies resolve through it instead of falling back to the game
        // context.
        var chainParent = _contextParams.ParentInstance?.Context
            ?? _contextParams.ParentInstance?.CreateContext()
            ?? parentContext;

        return Context = _contextParams.Local
            ? new LocalLoadContext(chainParent, _contextParams.EntrypointPath, _contextParams.Resolver,
                _contextParams.TransformationService, _contextParams.Provider, _contextParams.Metadata)
            : new PluginAssemblyLoadContext(chainParent, _contextParams.EntrypointPath, _contextParams.Resolver,
                _contextParams.TransformationService, _contextParams.Provider, _contextParams.Metadata);
    }

    private record ContextParams(
        string EntrypointPath,
        AssemblyDependencyResolver Resolver,
        AlcFactory? ParentInstance,
        bool Local,
        ITransformationService TransformationService,
        IPluginProvider Provider,
        PluginMetadata Metadata);
}

internal record AlcContext(DerivedAssemblyLoadContext Context, Type EntrypointType, IServiceProviderScope ProviderScope) : IPluginDependencyContext
{
    public void Dispose()
    {
        ProviderScope.Dispose();
        Context.Unload();
    }

    public IServiceProvider ServiceProvider { get; } = ProviderScope.Provider;
}
