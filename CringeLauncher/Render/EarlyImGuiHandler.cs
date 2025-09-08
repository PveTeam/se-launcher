using System.Drawing.Drawing2D;
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

    public RenderTargetView RenderTarget => ImGuiHandler.Rtv!;
}