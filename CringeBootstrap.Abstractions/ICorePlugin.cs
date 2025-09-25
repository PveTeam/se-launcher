using Microsoft.Extensions.DependencyInjection;

namespace CringeBootstrap.Abstractions;

public interface ICorePlugin : IDisposable
{
    bool RestartRequested { get; }
    bool Initialize(string[] args, ServiceCollection services);
    bool Run();
    void Restart();
}