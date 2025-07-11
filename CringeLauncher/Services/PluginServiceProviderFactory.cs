using System.Runtime.Loader;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CringePlugins.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace CringeLauncher.Services;

internal class PluginServiceProviderFactory(IServiceProviderFactory<ContainerBuilder> providerFactory, ILifetimeScope lifetimeScope) : IPluginServiceProviderFactory
{
    public IServiceCollection CreateBuilder() => new ServiceCollection();

    public IServiceProviderScope CreateServiceProviderScope(AssemblyLoadContext context, IServiceCollection services)
    {
        var pluginScope = lifetimeScope.BeginLoadContextLifetimeScope(context, builder => builder.Populate(services));
        
        return new ProviderScope(pluginScope);
    }

    private record ProviderScope(ILifetimeScope LifetimeScope) : IServiceProviderScope
    {
        private bool _disposed;

        public void Dispose()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _disposed = true;
            LifetimeScope.Dispose();
        }

        public IServiceProvider Provider
        {
            get
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return field;
            }
        } = new AutofacServiceProvider(LifetimeScope);
    }
}