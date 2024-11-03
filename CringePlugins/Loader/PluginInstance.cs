using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Utils;
using VRage.Plugins;

namespace CringePlugins.Loader;

internal sealed class PluginInstance
{
    public bool HasConfig => _openConfigAction != null;

    private readonly string _entrypointPath;
    private PluginAssemblyLoadContext? _context;
    private IPlugin? _instance;
    private Action? _openConfigAction;
    private IHandleInputPlugin? _handleInputInstance;
    public PluginMetadata Metadata { get; }

    public PluginInstance(PluginMetadata metadata, string entrypointPath)
    {
        _entrypointPath = entrypointPath;
        Metadata = metadata;
    }
    
    public PluginInstance(string entrypointPath) : this(PluginMetadata.ReadFromEntrypoint(entrypointPath), entrypointPath)
    {
    }

    public void Instantiate()
    {
        if (AssemblyLoadContext.GetLoadContext(typeof(PluginInstance).Assembly) is not ICoreLoadContext parentContext)
            throw new NotSupportedException("Plugin instantiation is not supported in this context");
        
        _context = new PluginAssemblyLoadContext(parentContext, _entrypointPath);
        
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
            //todo: log this and continue without action instead of throwing exception
            if (openConfigMethod.ReturnType != typeof(void) || openConfigMethod.IsStatic || openConfigMethod.GetParameters().Length > 0)
                throw new InvalidOperationException("OpenConfigDialog method has an incorrect signature");

            _openConfigAction = openConfigMethod.CreateDelegate<Action>(_instance);
        }

        _handleInputInstance = _instance as IHandleInputPlugin;
    }

    public void RegisterLifetime()
    {
        if (_instance is null)
            throw new InvalidOperationException("Must call Instantiate first");
        
        MyPlugins.m_plugins.Add(_instance);
        if (_handleInputInstance is not null)
            MyPlugins.m_handleInputPlugins.Add(_handleInputInstance);
    }

    public void OpenConfig()
    {
        if (_openConfigAction is null)
            throw new InvalidOperationException("Plugin does not have OpenConfigDialog method");

        _openConfigAction();
    }
}