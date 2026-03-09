using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Abstractions;
using CringePlugins.Utils;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Sandbox;
using Sandbox.Game.World;
using SharedCringe.Loader;
using VRage;
using VRage.Game;
using VRage.Game.ObjectBuilder;
using VRage.Plugins;

namespace CringePlugins.Loader;

internal sealed class PluginInstance(
    PluginMetadata metadata,
    string entrypointPath,
    bool local,
    IPluginServiceProviderFactory serviceProviderFactory,
    AssemblyDependencyResolver? dependencyResolver,
    PluginsLifetime pluginsLifetime,
    PluginInstance? parent = null) : IEquatable<PluginInstance>
{
    private static readonly MethodInfo RegisterServicesMethod =
        typeof(PluginInstance).GetMethod(nameof(RegisterServices), BindingFlags.NonPublic | BindingFlags.Static)!;
    
    public bool HasConfig => _openConfigAction != null;
    public bool IsReloading => _disposeTcs?.Task.IsCompleted == false;

    public bool IsLocal => local;

    private PluginAssemblyLoadContext? _context;
    private IPlugin? _instance;
    private TaskCompletionSource<(DerivedAssemblyLoadContext OldContext, DerivedAssemblyLoadContext NewContext)>? _disposeTcs;

    private Action? _openConfigAction;
    private IServiceProviderScope? _serviceProviderScope;
    private AssemblyDependencyResolver? _dependencyResolver = dependencyResolver;
    public PluginWrapper? WrappedInstance { get; private set; }

    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    public PluginMetadata Metadata { get; } = metadata;

    public void Instantiate(ImmutableArray<DerivedAssemblyLoadContext>.Builder contextBuilder)
    {
        if (AssemblyLoadContext.GetLoadContext(typeof(PluginInstance).Assembly) is not ICoreLoadContext parentContext)
            throw new NotSupportedException("Plugin instantiation is not supported in this context");

        _dependencyResolver ??= new(entrypointPath);

        _context = local
            ? new LocalLoadContext(parentContext, entrypointPath, _dependencyResolver)
            : new PluginAssemblyLoadContext(parent?._context ?? parentContext, entrypointPath, _dependencyResolver);
        contextBuilder.Add(_context);

        var entrypoint = _context.LoadEntrypoint();

        var implementationType = entrypoint.GetMainModule().GetType(Metadata.EntrypointTypeName, true, false)!;

        var services = serviceProviderFactory.CreateBuilder();

        services.AddSingleton(typeof(IPlugin), implementationType);

        if (implementationType.IsAssignableTo(typeof(IPluginWithServices)))
            RegisterServicesMethod.MakeGenericMethod(implementationType).Invoke(null, [services]);

        _serviceProviderScope = serviceProviderFactory.CreateServiceProviderScope(_context, services);

        _instance = _serviceProviderScope.Provider.GetRequiredService<IPlugin>();

        var openConfigMethod = implementationType.GetMethod("OpenConfigDialog");

        if (openConfigMethod is not null)
        {
            if (openConfigMethod.ReturnType != typeof(void) || openConfigMethod.IsStatic || openConfigMethod.GetParameters().Length > 0)
            {
                Log.Error("Plugin has OpenConfigDialog method with incorrect signature: {Name}, v{Version} - {Source}",
                    Metadata.Name, Metadata.Version, Metadata.Source);
            }
            else
            {
                _openConfigAction = openConfigMethod.CreateDelegate<Action>(_instance);
            }
        }

        WrappedInstance = new PluginWrapper(new PluginContext(Metadata, _serviceProviderScope.Provider, pluginsLifetime), _instance);
        
        var loadAssetsMethod = implementationType.GetMethod("LoadAssets", [typeof(string)]);

        if (loadAssetsMethod is null) return;
        
        if (Metadata.AssetsDirectory?.Exists == true)
        {
            loadAssetsMethod.Invoke(_instance, [Metadata.AssetsDirectory.FullName]);
        }
        else
        {
            Log.Error("Plugin is missing an assets folder: {Name}, v{Version} - {Source}", Metadata.Name,
                Metadata.Version, Metadata.Source);
        }
    }

    public void RegisterLifetime()
    {
        if (_instance is null)
            throw new InvalidOperationException("Must call Instantiate first");

        MyPlugins.m_plugins.Add(WrappedInstance);
        if (_instance is IHandleInputPlugin)
            MyPlugins.m_handleInputPlugins.Add(WrappedInstance);
    }

    public void OpenConfig()
    {
        if (_openConfigAction is null)
            throw new InvalidOperationException("Plugin does not have OpenConfigDialog method");

        try
        {
            _openConfigAction();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error opening config");
        }
    }

    public Task<(DerivedAssemblyLoadContext OldContext, DerivedAssemblyLoadContext NewContext)> ReloadAsync()
    {
        if (!local)
            throw new NotSupportedException("Reload is only supported for local plugins");

        if (_disposeTcs != null)
            return _disposeTcs.Task;

        var tcs = new TaskCompletionSource<(DerivedAssemblyLoadContext OldContext, DerivedAssemblyLoadContext NewContext)>();

        _disposeTcs = tcs;
        MySandboxGame.Static.Invoke(ReloadInternal, "PluginInstance.Reload");
        return tcs.Task;
    }
    private void ReloadInternal()
    {
        if (_disposeTcs is null)
            throw new InvalidOperationException("Must call Reload first");

        Log.Info("Reloading local plugin {Name}", Metadata.Name);

        if (_context is null)
            throw new InvalidOperationException("Must call Instantiate first");

        MyPlugins.m_plugins.Remove(WrappedInstance);
        if (_instance is IHandleInputPlugin)
            MyPlugins.m_handleInputPlugins.Remove(WrappedInstance);

        if (MySession.Static is { } session)
        {
            foreach (var kvp in session.m_sessionComponents)
            {
                if (kvp.Key.Assembly == WrappedInstance!.InstanceType.Assembly)
                {
                    session.UnregisterComponent(kvp.Value);
                }
            }
        }
        MyGlobalTypeMetadata.Static.m_assemblies.Remove(WrappedInstance!.InstanceType.Assembly);
        MyDefinitionManagerBase.m_registered.Remove(WrappedInstance!.InstanceType.Assembly);
        MyDefinitionManagerBase.m_registeredAssemblies.Remove(WrappedInstance!.InstanceType.Assembly);
        MyXmlSerializerManager.m_registeredAssemblies.Remove(WrappedInstance!.InstanceType.Assembly);

        _openConfigAction = null;
        WrappedInstance?.Dispose();
        WrappedInstance = null;
        _instance = null;

        _serviceProviderScope?.Dispose();
        _context.Unload();
        var oldContext = _context;

        var builder = ImmutableArray.CreateBuilder<DerivedAssemblyLoadContext>();
        Instantiate(builder);
        RegisterLifetime();
        WrappedInstance!.Init(MySandboxGame.Static);
        Log.Info("Plugin Init: {Metadata}", WrappedInstance.ToString());

        MyGlobalTypeMetadata.Static.RegisterAssembly(WrappedInstance!.InstanceType.Assembly);
        MySession.Static?.RegisterComponentsFromAssembly(WrappedInstance!.InstanceType.Assembly, true);

        _disposeTcs.SetResult((oldContext, builder[0]));
        _disposeTcs = null;

        Log.Info("Reloaded local plugin {Name}", Metadata.Name);
    }

    private static void RegisterServices<T>(IServiceCollection services) where T : IPluginWithServices
    {
        T.RegisterServices(services);
    }

    private record PluginContext(PluginMetadata Metadata, IServiceProvider Provider, PluginsLifetime Lifetime) : IPluginContext
    {
        public object? GetService(Type serviceType) => Provider.GetService(serviceType);

        public ImmutableDictionary<string, PluginMetadata> Plugins => Lifetime.Plugins;
    }

    public bool Equals(PluginInstance? other) => 
        Metadata.Id.Equals(other?.Metadata.Id, StringComparison.OrdinalIgnoreCase);
    
    public override bool Equals(object? obj) => 
        obj is PluginInstance other && Equals(other);
    
    public override int GetHashCode() => Metadata.Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
}
