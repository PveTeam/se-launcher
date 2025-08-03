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

        Log.Info("Dependency Test {Time}", NodaTime.SystemClock.Instance.GetCurrentInstant());
    }

    public void Update()
    {
    }
}