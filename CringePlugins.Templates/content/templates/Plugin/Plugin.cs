using CringePlugins.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace $projectName$;

public class Plugin : IPluginWithServices
{
    public void Init(IPluginContext context)
    {
    }

    public void Update()
    {
    }

    public void Dispose()
    {
    }
    
    public static void RegisterServices(IServiceCollection services)
    {
    }
}