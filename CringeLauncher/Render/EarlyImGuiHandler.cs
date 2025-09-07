using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Windows.Win32.Foundation;
using ImGuiNET;
using Device = SharpDX.Direct3D11.Device;

namespace CringeLauncher.Render;

internal class EarlyImGuiHandler
{
    public void CreateContext(nint windowHandle, Device device, SwapChain swapChain)
    {
        ImGuiHandler.Instance!.Init(windowHandle, device, device.ImmediateContext);

        CleanupRenderTarget();
        CreateRenderTarget(device, swapChain);

        ImGuiHandler.HookWindow((HWND)windowHandle);
    }

    public void CreateRenderTarget(Device device, SwapChain swapChain)
    {
        using var resource = swapChain.GetBackBuffer<Texture2D>(0);
        ImGuiHandler.Rtv = new(device, resource, new()
        {
            Format = Format.R8G8B8A8_UNorm,
            Dimension = RenderTargetViewDimension.Texture2D,
        });
    }

    public void CleanupRenderTarget()
    {
        ImGuiHandler.Rtv?.Dispose();
        ImGuiHandler.Rtv = null;
    }

    public void Render()
    {
        ImGuiHandler.Instance!.DoRender();
    }

    public unsafe Region? GetWindowRegions()
    {
        // todo fix ImGuiContext layout to include all visible windows in the region
        var windowPtr = ImGui.FindWindowByName("Splash");
        if (windowPtr.NativePtr == null) return null;
        var windowRegion = new RectangleF(windowPtr.Pos.X, windowPtr.Pos.Y, windowPtr.Size.X, windowPtr.Size.Y);
        if (_previousWindowRegion == windowRegion) return null;
        return new(_previousWindowRegion = windowRegion);
    }
    
    private RectangleF _previousWindowRegion;

    public RenderTargetView RenderTarget => ImGuiHandler.Rtv!;
}