using CringePlugins.Render;
using NLog;
using VRage.Plugins;

namespace TestPlugin;

public class Plugin : IPlugin
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public void Dispose()
    {
    }

    public void Init(object gameInstance)
    {
        Log.Info("Test Plugin init");
        
        RenderHandler.Current.RegisterComponent(new TestRenderComponent());
    }

    public void Update()
    {
    }
}