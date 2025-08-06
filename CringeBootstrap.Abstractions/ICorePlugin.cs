namespace CringeBootstrap.Abstractions;

public interface ICorePlugin : IDisposable
{
    bool Initialize(string[] args);
    bool Run();
}