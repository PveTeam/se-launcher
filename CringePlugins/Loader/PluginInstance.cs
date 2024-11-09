using System.Collections.Immutable;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Utils;
using NLog;
using SharedCringe.Loader;
using VRage.Plugins;

namespace CringePlugins.Loader;

internal sealed class PluginInstance(PluginMetadata metadata, string entrypointPath)
{
    public bool HasConfig => _openConfigAction != null;

    private PluginAssemblyLoadContext? _context;
    private IPlugin? _instance;

    private Action? _openConfigAction;
    public PluginWrapper? WrappedInstance { get; private set; }

    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    public PluginMetadata Metadata { get; } = metadata;

    public PluginInstance(string entrypointPath) : this(PluginMetadata.ReadFromEntrypoint(entrypointPath), entrypointPath)
    {
    }

    public void Instantiate(ImmutableArray<DerivedAssemblyLoadContext>.Builder contextBuilder)
    {
        if (AssemblyLoadContext.GetLoadContext(typeof(PluginInstance).Assembly) is not ICoreLoadContext parentContext)
            throw new NotSupportedException("Plugin instantiation is not supported in this context");
        
        _context = new PluginAssemblyLoadContext(parentContext, entrypointPath);
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
}