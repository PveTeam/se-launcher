using System.Reflection;
using CringePlugins.Abstractions.Loader;
using Microsoft.Extensions.DependencyInjection;
using SharedCringe.Loader;

namespace CringePlugins.Loader.ProviderProvider;

internal class PluginProviderInstance(PluginMetadata metadata, IPluginDependencyContextFactory contextFactory)
    : PluginInstanceBase(metadata, contextFactory)
{
    private static readonly MethodInfo RegisterServicesMethod =
        typeof(PluginProviderInstance).GetMethod(nameof(RegisterPluginServices), BindingFlags.NonPublic | BindingFlags.Static)!;
    
    public override bool HasConfig => false;
    public override bool IsReloading => false;
    
    public IPluginProvider Provider
    {
        get => field ?? throw new InvalidOperationException("Call Instantiate first");
        private set;
    }
    
    protected override void Instantiate(Type entrypointType, IServiceProvider serviceProvider)
    {
        Provider = serviceProvider.GetRequiredService<IPluginProvider>();
    }

    protected override void RegisterServices(IServiceCollection services, Type entrypointType)
    {
        services.AddSingleton(typeof(IPluginProvider), entrypointType);
        RegisterServicesMethod.MakeGenericMethod(entrypointType).Invoke(null, [services]);
    }

    public override void RegisterLifetime()
    {
    }

    public override void OpenConfig()
    {
    }

    public override Task<(DerivedAssemblyLoadContext OldContext, DerivedAssemblyLoadContext NewContext)> ReloadAsync()
    {
        throw new NotSupportedException();
    }
    
    private static void RegisterPluginServices<T>(IServiceCollection services) where T : IPluginProvider
    {
        T.RegisterServices(services);
    }
}
