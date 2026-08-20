using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace CringePlugins.Abstractions.Loader;

public interface IPluginServiceProviderFactory
{
    IServiceCollection CreateBuilder();
    IServiceProviderScope CreateServiceProviderScope(AssemblyLoadContext context, IServiceCollection services);
}

public interface IServiceProviderScope : IDisposable
{
    IServiceProvider Provider { get; }
}
