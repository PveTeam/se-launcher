#if WINDOWS
using Windows.Win32.Foundation;
using ImGuiNET;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace CringeLauncher.Render.Win;

internal class EarlyImGuiHandler
{
    public void CreateContext(nint windowHandle, Device device, SwapChain swapChain)
    {
        WinImGuiHandler.Instance!.Init(windowHandle, device, device.ImmediateContext);

        WinImGuiHandler.Instance.CleanupRenderTarget();
        WinImGuiHandler.Instance.CreateRenderTarget(device, swapChain);

        WinImGuiHandler.HookWindow((HWND)windowHandle);
    }

    public void Render()
    {
        ImGuiHandler.Instance!.DoRender();
    }

    public Region? GetWindowRegions()
    {
        var contextPtr = ImGui.GetCurrentContext();
        
        var hash = new HashCode();
        
        foreach (var windowPtr in contextPtr.Windows)
        {
            var windowRegion = new RectangleF(windowPtr.Pos.X, windowPtr.Pos.Y, windowPtr.Size.X, windowPtr.Size.Y);
            hash.Add(windowRegion);
        }
        
        var regionHash = hash.ToHashCode();
        if (_previousWindowRegionHash == regionHash) return null;
        _previousWindowRegionHash = regionHash;
        
        _windowRegion ??= new();
        _windowRegion.MakeEmpty();
        
        foreach (var windowPtr in contextPtr.Windows)
        {
            var windowRegion = new RectangleF(windowPtr.Pos.X, windowPtr.Pos.Y, windowPtr.Size.X, windowPtr.Size.Y);
            _windowRegion.Union(windowRegion);
        }

        return _windowRegion.Clone();
    }
    
    private int _previousWindowRegionHash;
    private Region? _windowRegion;

    public RenderTargetView RenderTarget => WinImGuiHandler.Instance!.Rtv!;
}
#endif
