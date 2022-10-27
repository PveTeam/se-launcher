using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Utils;
using VRage.Plugins;

namespace CringePlugins.Loader;

internal sealed class PluginInstance
{
    private readonly string _entrypointPath;
    private PluginAssemblyLoadContext? _context;
    private IPlugin? _instance;
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
}