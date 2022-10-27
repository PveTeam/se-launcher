namespace CringePlugins.Abstractions;

public interface IRenderComponent
{
    /// <summary>
    /// Gets called while the dear ImGui frame is being composed
    /// </summary>
    void OnFrame();
}

internal interface IRootRenderComponent : IRenderComponent, IDisposable;