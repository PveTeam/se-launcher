#if !WINDOWS
using System.Runtime.Versioning;
using ImGuiNET;
using SharpDX.Direct3D11;
using Silk.NET.GLFW;

namespace CringeLauncher.Render.Xplat;

[SupportedOSPlatform("linux")]
internal sealed unsafe class XplatImGuiHandler(DirectoryInfo configDir) : ImGuiHandler(configDir)
{
    private bool _init;
    public override bool BlockKeys { get; }
    public override bool Initialized => _init;

    public void Init(WindowHandle* windowHandle, Device device, DeviceContext deviceContext)
    {
        base.Init(device, deviceContext);
        
        ImGui.ImGui_ImplGlfw_InitForOther(windowHandle, true);
        _init = true;
    }
    
    protected override void SetupFrame(ImGuiIOPtr io)
    {
        base.SetupFrame(io);
        ImGui.ImGui_ImplGlfw_NewFrame();
    }
}

#endif
