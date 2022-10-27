using CringePlugins.Abstractions;
using ImGuiNET;

namespace TestPlugin;

public class TestRenderComponent : IRenderComponent
{
    public void OnFrame()
    {
        if (ImGui.Begin("Test Window"))
        {
            ImGui.Button("Test");
            
            ImGui.End();
        }
    }
}