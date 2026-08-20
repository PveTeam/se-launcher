using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;
using CringePlugins.Abstractions;
using CringePlugins.Abstractions.Loader;
using Microsoft.Extensions.DependencyInjection;
using Sandbox;
using Sandbox.Game.World;
using SharedCringe.Loader;
using VRage;
using VRage.Game;
using VRage.Game.ObjectBuilder;
using VRage.Plugins;

namespace CringePlugins.Loader.PluginProvider;

internal sealed class PluginInstance(PluginMetadata metadata, IPluginDependencyContextFactory contextFactory)
    : PluginInstanceBase(metadata, contextFactory)
{
    private static readonly MethodInfo RegisterServicesMethod =
        typeof(PluginInstance).GetMethod(nameof(RegisterPluginServices), BindingFlags.NonPublic | BindingFlags.Static)!;
    
    public override bool HasConfig => _openConfigAction != null;
    public override bool IsReloading => _disposeTcs?.Task.IsCompleted == false;

    private IPlugin? _instance;
    private TaskCompletionSource<bool>? _disposeTcs;

    private Action? _openConfigAction;
    public PluginWrapper? WrappedInstance { get; private set; }

    protected override void Instantiate(Type entrypointType, IServiceProvider serviceProvider)
    {
        _instance = serviceProvider.GetRequiredService<IPlugin>();

        var openConfigMethod = entrypointType.GetMethod("OpenConfigDialog");

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

        WrappedInstance =
            new PluginWrapper(
                new PluginContext(Metadata, serviceProvider,
                    serviceProvider.GetRequiredService<PluginsLifetime>()), _instance);
        
        var loadAssetsMethod = entrypointType.GetMethod("LoadAssets", [typeof(string)]);

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

    protected override void RegisterServices(IServiceCollection services, Type entrypointType)
    {
        services.AddSingleton(typeof(IPlugin), entrypointType);

        if (entrypointType.IsAssignableTo(typeof(IPluginWithServices)))
            RegisterServicesMethod.MakeGenericMethod(entrypointType).Invoke(null, [services]);
    }

    public override void OpenConfig()
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

    public override Task ReloadAsync()
    {
        if (!IsLocal)
            throw new NotSupportedException("Reload is only supported for local plugins");

        if (_disposeTcs != null)
            return _disposeTcs.Task;

        var tcs = new TaskCompletionSource<bool>();

        _disposeTcs = tcs;
        MySandboxGame.Static.Invoke(ReloadInternal, "PluginInstance.Reload");
        return tcs.Task;
    }
    private void ReloadInternal()
    {
        if (_disposeTcs is null)
            throw new InvalidOperationException("Must call Reload first");

        Log.Info("Reloading local plugin {Name}", Metadata.Name);

        if (Context is null)
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

        Context.Dispose();

        Instantiate();
        RegisterLifetime();
        WrappedInstance!.Init(MySandboxGame.Static);
        Log.Info("Plugin Init: {Metadata}", WrappedInstance.ToString());

        MyGlobalTypeMetadata.Static.RegisterAssembly(WrappedInstance!.InstanceType.Assembly);
        MySession.Static?.RegisterComponentsFromAssembly(WrappedInstance!.InstanceType.Assembly, true);

        _disposeTcs.SetResult(true);
        _disposeTcs = null;

        Log.Info("Reloaded local plugin {Name}", Metadata.Name);
    }

    private static void RegisterPluginServices<T>(IServiceCollection services) where T : IPluginWithServices
    {
        T.RegisterServices(services);
    }

    public override void RegisterLifetime()
    {
        if (_instance is null)
            throw new InvalidOperationException("Must call Instantiate first");

        MyPlugins.m_plugins.Add(WrappedInstance);
        if (_instance is IHandleInputPlugin)
            MyPlugins.m_handleInputPlugins.Add(WrappedInstance);
    }

    private record PluginContext(PluginMetadata Metadata, IServiceProvider Provider, PluginsLifetime Lifetime) : IPluginContext
    {
        public object? GetService(Type serviceType) => Provider.GetService(serviceType);

        public ImmutableDictionary<string, PluginMetadata> Plugins => Lifetime.Plugins;
    }
}
