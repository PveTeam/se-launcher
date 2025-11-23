using System.Collections.Immutable;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Utils;
using NLog;
using Sandbox;
using Sandbox.Game.World;
using SharedCringe.Loader;
using VRage;
using VRage.Game;
using VRage.Game.ObjectBuilder;
using VRage.Plugins;

namespace CringePlugins.Loader;

internal sealed class PluginInstance(PluginMetadata metadata, string entrypointPath, bool local)
{
    public bool HasConfig => _openConfigAction != null;
    public bool IsReloading => _disposeTcs?.Task.IsCompleted == false;

    public bool IsLocal => local;

    private PluginAssemblyLoadContext? _context;
    private IPlugin? _instance;
    private TaskCompletionSource<(DerivedAssemblyLoadContext OldContext, DerivedAssemblyLoadContext NewContext)>? _disposeTcs;

    private Action? _openConfigAction;
    public PluginWrapper? WrappedInstance { get; private set; }

    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    public PluginMetadata Metadata { get; } = metadata;

    public PluginInstance(string entrypointPath, bool local) : this(PluginMetadata.ReadFromEntrypoint(entrypointPath), entrypointPath, local)
    {
    }

    public void Instantiate(ImmutableArray<DerivedAssemblyLoadContext>.Builder contextBuilder)
    {
        if (AssemblyLoadContext.GetLoadContext(typeof(PluginInstance).Assembly) is not ICoreLoadContext parentContext)
            throw new NotSupportedException("Plugin instantiation is not supported in this context");

        _context = local ? new LocalLoadContext(parentContext, entrypointPath) : new PluginAssemblyLoadContext(parentContext, entrypointPath);
        contextBuilder.Add(_context);

        var entrypoint = _context.LoadEntrypoint();

        var plugins = IntrospectionContext.Global.CollectDerivedTypes<IPlugin>(entrypoint.GetMainModule()).ToArray();

        if (plugins.Length == 0)
            throw new InvalidOperationException("Entrypoint does not contain any plugins");
        if (plugins.Length > 1)
            throw new InvalidOperationException("Entrypoint contains multiple plugins");

        _instance = (IPlugin) Activator.CreateInstance(plugins[0])!;

        var openConfigMethod = plugins[0].GetMethod("OpenConfigDialog");

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

        WrappedInstance = new PluginWrapper(Metadata, _instance);
        
        var loadAssetsMethod = plugins[0].GetMethod("LoadAssets", [typeof(string)]);

        if (loadAssetsMethod is null) return;
        
        var assetsDir = Path.Join(new DirectoryInfo(Path.GetDirectoryName(entrypointPath)!).Parent?.Parent?.FullName,
            "assets" + Path.DirectorySeparatorChar);
        
        if (Directory.Exists(assetsDir))
        {
            loadAssetsMethod.Invoke(_instance, [assetsDir]);
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
}
