using NLog;
using VRage.Plugins;

namespace CringePlugins.Loader;

//todo: we should patch Mysanboxgame.Run to log init of actual plugin names instead of pluginwrapper
//also, maybe we could unload the plugin if there's an error?
internal sealed class PluginWrapper(string name, IPlugin plugin) : IHandleInputPlugin
{
    public bool HasError => LastException != null;
    public Exception? LastException { get; private set; } //todo: show exception when hovered in plugin menu?

    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

    public void Dispose()
    {
        try
        {
            plugin.Dispose();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error Disposing {Name}", name);
            LastException = e;
        }
    }

    public void HandleInput()
    {
        if (HasError || plugin is not IHandleInputPlugin input)
            return;

        try
        {
            input.HandleInput();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error Updating {Name}", name);
            LastException = e;
        }
    }

    public void Init(object gameInstance)
    {
        try
        {
            plugin.Init(gameInstance);
        }
        catch (Exception e)
        {
            Log.Error(e, "Error Initializing {Name}", name);
            LastException = e;
        }
    }

    public void Update()
    {
        if (HasError)
            return;

        try
        {
            plugin.Update();
        }
        catch (Exception e)
        {
            Log.Error(e, "Error Updating {Name}", name);
            LastException = e;
        }
    }
}
