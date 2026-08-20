using Microsoft.Extensions.DependencyInjection;

namespace CringeBootstrap.Abstractions;

public interface ICorePlugin : IDisposable
{
    DirectoryInfo DataDirectory { get; }
    DirectoryInfo ConfigDirectory { get; }
    
    bool RestartRequested { get; }
    bool Initialize(string[] args, ServiceCollection services);
    bool Run();
    void Restart();
    void Stop();
}
