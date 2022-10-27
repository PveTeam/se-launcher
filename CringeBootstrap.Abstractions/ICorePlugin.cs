namespace CringeBootstrap.Abstractions;

public interface ICorePlugin : IDisposable
{
    void Initialize(string[] args);
    void Run();
}