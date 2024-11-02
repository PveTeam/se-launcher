using System.Collections.Concurrent;
using CringePlugins.Abstractions;
using ImGuiNET;
using NLog;

namespace CringePlugins.Render;

public sealed class RenderHandler : IRootRenderComponent
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    private static RenderHandler? _current;
    public static RenderHandler Current => _current ?? throw new InvalidOperationException("Render is not yet initialized");
    
    private readonly ConcurrentBag<ComponentRegistration> _components = [];

    internal RenderHandler()
    {
        _current = this;
    }

    public void RegisterComponent<TComponent>(TComponent instance) where TComponent : IRenderComponent
    {
        _components.Add(new ComponentRegistration(typeof(TComponent), instance));
    }

    void IRenderComponent.OnFrame()
    {
#if DEBUG
        ImGui.ShowDemoWindow();
#endif
        
        foreach (var (instanceType, renderComponent) in _components)
        {
            try
            {
                renderComponent.OnFrame();
            }
            catch (Exception e)
            {
                Log.Error(e, "Component {TypeName} failed to render a new frame", instanceType);
            }
        }
    }

    private record ComponentRegistration(Type InstanceType, IRenderComponent Instance);

    public void Dispose()
    {
        _current = null;
        _components.Clear();
    }
}