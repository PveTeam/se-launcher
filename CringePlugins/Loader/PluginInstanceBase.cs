using System.Collections.Immutable;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringePlugins.Abstractions;
using CringePlugins.Abstractions.Loader;
using CringePlugins.Loader.PluginProvider;
using CringePlugins.Utils;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using SharedCringe.Loader;
using VRage.Plugins;

namespace CringePlugins.Loader;

public abstract class PluginInstanceBase(
    PluginMetadata metadata,
    IPluginDependencyContextFactory contextFactory) : IPluginInstance
{
    protected static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    public abstract bool HasConfig { get; }
    public abstract bool IsReloading { get; }
    public bool IsLocal => contextFactory.Local;
    public PluginMetadata Metadata { get; } = metadata;
    public IPluginDependencyContext Context { get => field ?? throw new InvalidOperationException("Call Instantiate first"); private set; }

    public virtual void Instantiate()
    {
        Context = contextFactory.Create(RegisterServices);
        Instantiate(Context.EntrypointType, Context.ServiceProvider);
    }
    
    protected abstract void Instantiate(Type entrypointType, IServiceProvider serviceProvider);

    protected abstract void RegisterServices(IServiceCollection services, Type entrypointType);

    public abstract void RegisterLifetime();

    public abstract void OpenConfig();
    public abstract Task ReloadAsync();

    public bool Equals(IPluginInstance? other) => other is not null && Metadata.Id.Equals(other.Metadata.Id, StringComparison.OrdinalIgnoreCase);

    public override bool Equals(object? obj) =>  obj is IPluginInstance other && Equals(other);

    public override int GetHashCode() => Metadata.Id.GetHashCode(StringComparison.OrdinalIgnoreCase);
}
