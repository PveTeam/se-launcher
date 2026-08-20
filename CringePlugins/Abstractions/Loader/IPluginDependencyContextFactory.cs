using Microsoft.Extensions.DependencyInjection;
using SharedCringe.Loader;

namespace CringePlugins.Abstractions.Loader;

public interface IPluginDependencyContextFactory
{
    bool Local { get; }
    IPluginDependencyContext Create(Action<IServiceCollection, Type> registerServices);
}

public interface IPluginDependencyContext : IDisposable
{
    DerivedAssemblyLoadContext Context { get; }
    Type EntrypointType { get; }
    IServiceProvider ServiceProvider { get; }
}
