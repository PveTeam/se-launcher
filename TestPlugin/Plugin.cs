using CringePlugins.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace TestPlugin;

public class Plugin : IPluginWithServices
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    public void Dispose()
    {
    }

    public void Init(IPluginContext context)
    {
        Log.Info("Test Plugin init");

        Log.Info("Dependency Test {Time}", NodaTime.SystemClock.Instance.GetCurrentInstant());
    }

    public static void RegisterServices(IServiceCollection services)
    {
    }

    public void Update()
    {
    }
}