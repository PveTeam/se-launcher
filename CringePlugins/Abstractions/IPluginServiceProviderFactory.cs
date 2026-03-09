using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace CringePlugins.Abstractions;

internal interface IPluginServiceProviderFactory
{
    IServiceCollection CreateBuilder();
    IServiceProviderScope CreateServiceProviderScope(AssemblyLoadContext context, IServiceCollection services);
}

internal interface IServiceProviderScope : IDisposable
{
    IServiceProvider Provider { get; }
}