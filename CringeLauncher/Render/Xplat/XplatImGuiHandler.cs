#if !WINDOWS
using System.Runtime.Versioning;
using ImGuiNET;
using SharpDX.Direct3D11;
using Silk.NET.SDL;

namespace CringeLauncher.Render.Xplat;

[SupportedOSPlatform("linux")]
internal sealed unsafe class XplatImGuiHandler(DirectoryInfo configDir) : ImGuiHandler(configDir)
{
    private int _blockKeysCounter;
    private WindowHandle _windowHandle;

    public override bool BlockKeys => _blockKeysCounter > 0;

    public void Init(WindowHandle windowHandle, Device device, DeviceContext deviceContext)
    {
        base.Init(device, deviceContext);

        _windowHandle = windowHandle;
        ImGui.ImGui_ImplSDL3_InitForD3D(windowHandle);
        GraphicsInitialized = true;
    }

    public void DispatchEvent(in Event @event)
    {
        fixed (Event* ptr = &@event)
            ImGui.ImGui_ImplSDL3_ProcessEvent(ptr);
    }

    protected override void SetupFrame(ImGuiIOPtr io)
    {
        base.SetupFrame(io);
        ImGui.ImGui_ImplSDL3_NewFrame();

        if (io.WantTextInput)
            _blockKeysCounter = 10; // WantTextInput can be false briefly after pressing enter in a textbox
        else
            _blockKeysCounter--;
    }

    protected override void AfterFrame()
    {
        if (_windowHandle == default || BlockKeys)
            return;

        if (!Sdl.TextInputActive(_windowHandle))
            Sdl.StartTextInput(_windowHandle);
    }
}

#endif
