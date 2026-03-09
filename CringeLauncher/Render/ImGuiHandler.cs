using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using CringePlugins.Abstractions;
using CringePlugins.Render;
using CringePlugins.Services;
using CringePlugins.Ui;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using Sandbox.Graphics.GUI;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using static ImGuiNET.ImGui;
using Device = SharpDX.Direct3D11.Device;

namespace CringeLauncher.Render;

[SupportedOSPlatform("windows")]
[SupportedOSPlatform("linux")]
internal abstract class ImGuiHandler : IGuiHandler, IDisposable
{
    public static ImGuiHandler? Instance;
    protected readonly DirectoryInfo ConfigDir;
    protected readonly IRootRenderComponent RenderHandler;
    protected readonly ImGuiImageService ImageService;
    private bool _gameRendererInitialized;
    private DeviceContext? _deviceContext;

    protected ImGuiHandler(DirectoryInfo configDir)
    {
        ConfigDir = configDir;
        RenderHandler = new RenderHandler(this);
        ImageService = (ImGuiImageService)GameServicesExtension.GameServices.GetRequiredService<IImGuiImageService>();
    }

    public bool BlockMouse { get; set; }
    public abstract bool BlockKeys { get; }
    public bool DrawMouse { get; set; }
    public bool MouseToggle { get; set; }
    public bool MouseKey { get; set; }
    public abstract bool Initialized { get; }
    
    public RenderTargetView? Rtv { get; private set; }
    
    public void CreateRenderTarget(Device device, SwapChain swapChain)
    {
        using var resource = swapChain.GetBackBuffer<Texture2D>(0);
        CreateRenderTarget(device, resource);
    }

    public void CreateRenderTarget(Device device, SharpDX.Direct3D11.Resource swapChainBackBuffer)
    {
        Rtv = new(device, swapChainBackBuffer, new()
        {
            Format = Format.R8G8B8A8_UNorm,
            Dimension = RenderTargetViewDimension.Texture2D,
        });
    }

    public void CleanupRenderTarget()
    {
        Rtv?.Dispose();
        Rtv = null;
    }

    private static unsafe void BuildFonts(ImGuiIOPtr io)
    {
        ImFontVariants.LoadFonts(io, Path.Join(AppContext.BaseDirectory, "Resources", "Fonts"), "SourceCodePro",
            0, Enum.GetValues<FontVariant>());
        /*ImFontGlyphRangesBuilderPtr builder = ImGuiNative.ImFontGlyphRangesBuilder_ImFontGlyphRangesBuilder();

            try
            {
                builder.AddRanges(io.Fonts.GetGlyphRangesDefault());
                builder.AddRanges(io.Fonts.GetGlyphRangesCyrillic());
                builder.BuildRanges(out var ranges);
                try
                {
                    ImFontVariants.LoadFonts(io, Path.Join(AppContext.BaseDirectory, "Resources", "Fonts"), "SourceCodePro",
                        ranges.Data,
                        Enum.GetValues<FontVariant>());
                }
                finally
                {
                    MemFree(ranges.Data);
                }
            }
            finally
            {
                builder.Destroy();
            }*/
    }

    public void NotifyGameRendererInitialized()
    {
        _gameRendererInitialized = true;
    }

    public void DoRender()
    {
        if (!Initialized)
            return;
        
        var io = GetIO();
        SetupFrame(io);
        
        NewFrame();
        
        BlockMouse = io.WantCaptureMouse;

        DrawMouse = io.MouseDrawCursor || MouseToggle || MouseKey;

        if (_gameRendererInitialized) UpdateMouse();

        RenderHandler.OnFrame();

        ImGui.Render();
        
        ImGui_ImplDX11_RenderDrawData(GetDrawData());

        UpdatePlatformWindows();
        RenderPlatformWindowsDefault();

        ImageService.Update();
    }

    protected virtual void SetupFrame(ImGuiIOPtr io)
    {
        ImGui_ImplDX11_NewFrame();
        _deviceContext!.ClearState();
        _deviceContext.OutputMerger.SetRenderTargets(Rtv);
    }

    private void UpdateMouse()
    {
        var focusedScreen = MyScreenManager.GetScreenWithFocus(); //migrated logic from MyDX9Gui.Draw

        if (DrawMouse || focusedScreen?.GetDrawMouseCursor() == true)
        {
            MyGuiSandbox.SetMouseCursorVisibility(true, false);
        }
        else if (focusedScreen != null)
        {
            MyGuiSandbox.SetMouseCursorVisibility(focusedScreen.GetDrawMouseCursor());
        }
    }

    public virtual void Dispose()
    {
        RenderHandler.Dispose();
    }

    protected unsafe void Init(Device device, DeviceContext deviceContext)
    {
        _deviceContext = deviceContext;
        CreateContext();

        var io = GetIO();

        var path = Path.Join(ConfigDir.FullName, "imgui.ini");

        io.NativePtr->IniFilename = Utf8StringMarshaller.ConvertToUnmanaged(path);

        io.ConfigErrorRecoveryEnableAssert = false;
        io.ConfigWindowsMoveFromTitleBarOnly = true;
        io.ConfigDpiScaleViewports = true;
        io.ConfigDpiScaleFonts = true;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;
        
        ImGui_ImplDX11_Init(device.NativePointer, deviceContext.NativePointer);
        ImageService.Initialize(device);
        BuildFonts(io);
    }
}
