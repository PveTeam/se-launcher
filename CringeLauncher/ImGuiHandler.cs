using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using CringePlugins.Abstractions;
using CringePlugins.Render;
using CringePlugins.Services;
using CringePlugins.Ui;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;
using SharpDX.Direct3D11;
using static ImGuiNET.ImGui;
using VRage;
using Sandbox.Graphics.GUI;
using VRageRender;

namespace CringeLauncher;

internal sealed class ImGuiHandler : IGuiHandler, IDisposable
{
    private readonly DirectoryInfo _configDir;
    private DeviceContext? _deviceContext;
    private int _blockKeysCounter;
    private static nint _wndproc;

    public bool BlockMouse { get; private set; }
    public bool BlockKeys => _blockKeysCounter > 0;
    public bool DrawMouse { get; private set; }

    public bool MouseToggle { get; set; }
    public bool MouseKey { get; set; }

    public bool Initialized => _init;

    public static ImGuiHandler? Instance;

    public static RenderTargetView? Rtv;

    private readonly IRootRenderComponent _renderHandler;
    private readonly ImGuiImageService _imageService;
    private bool _gameRendererInitialized;
    private static bool _init;

    public ImGuiHandler(DirectoryInfo configDir)
    {
        _configDir = configDir;
        _renderHandler = new RenderHandler(this);
        _imageService = (ImGuiImageService)GameServicesExtension.GameServices.GetRequiredService<IImGuiImageService>();
    }

    public unsafe void Init(nint windowHandle, Device device, DeviceContext deviceContext)
    {
        _deviceContext = deviceContext;

        CreateContext();

        var io = GetIO();

        var path = Path.Join(_configDir.FullName, "imgui.ini");

        io.NativePtr->IniFilename = Utf8StringMarshaller.ConvertToUnmanaged(path);

        io.ConfigErrorRecoveryEnableAssert = false;
        io.ConfigWindowsMoveFromTitleBarOnly = true;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;

        ImGui_ImplWin32_Init(windowHandle);
        ImGui_ImplDX11_Init(device.NativePointer, deviceContext.NativePointer);
        _init = true;

        _imageService.Initialize(device);

        BuildFonts(io);
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

    public static void HookWindow(HWND windowHandle)
    {
        _wndproc = PInvoke.GetWindowLongPtr(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC);

        unsafe
        {
            delegate* unmanaged[Stdcall]<HWND, int, nint, nint, int> wndProcHook = &WndProcHook;

            PInvoke.SetWindowLongPtr(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, (nint)wndProcHook);
        }
    }

    public void NotifyGameRendererInitialized()
    {
        _gameRendererInitialized = true;
    }

    public void DoRender()
    {
        if (Rtv is null)
            return;

        ImGui_ImplDX11_NewFrame();
        ImGui_ImplWin32_NewFrame();
        NewFrame();

        var io = GetIO();
        BlockMouse = io.WantCaptureMouse;

        if (io.WantTextInput)
            _blockKeysCounter = 10; //WantTextInput can be false briefly after pressing enter in a textbox
        else
            _blockKeysCounter--;

        DrawMouse = io.MouseDrawCursor || MouseToggle || MouseKey;
        
        if (_gameRendererInitialized) UpdateMouse();

        _renderHandler.OnFrame();

        ImGui.Render();

        _deviceContext!.ClearState();
        _deviceContext.OutputMerger.SetRenderTargets(Rtv);

        ImGui_ImplDX11_RenderDrawData(GetDrawData());

        UpdatePlatformWindows();
        RenderPlatformWindowsDefault();

        _imageService.Update();
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe int WndProcHook(HWND hWnd, int msg, nint wParam, nint lParam)
    {
        //special handling for the mouse free key

        if ((uint)msg is PInvoke.WM_KEYDOWN or PInvoke.WM_SYSKEYDOWN && (int)wParam == (int)Keys.Oemtilde && Instance != null)
        {
            Instance.MouseKey = true;

            return 0;
        }

        if ((uint)msg is PInvoke.WM_KEYUP or PInvoke.WM_SYSKEYUP && (int)wParam == (int)Keys.Oemtilde && Instance != null)
        {
            Instance.MouseKey = false;

            return 0;
        }

        if ((uint)msg == PInvoke.WM_CHAR && (char)(int)wParam == '`')
            return 0;

        //ignore input if mouse is hidden
        if (Instance?.BlockKeys != true && MyVRage.Platform?.Input?.ShowCursor == false && Instance?.DrawMouse != true)
            return CallWindowProc(_wndproc, hWnd, msg, wParam, lParam);

        var hookResult = ImGui_ImplWin32_WndProcHandler(hWnd, msg, wParam, lParam);

        if (hookResult != 0)
            return hookResult;

        if (!_init)
            return CallWindowProc(_wndproc, hWnd, msg, wParam, lParam);

        var io = GetIO();

        var blockMessage = (msg is >= 256 and <= 265 && io.WantTextInput)
            || (msg is >= 512 and <= 526 && io.WantCaptureMouse);

        return blockMessage ? hookResult : CallWindowProc(_wndproc, hWnd, msg, wParam, lParam);
    }

    [DllImport("USER32.dll", ExactSpelling = true, EntryPoint = "CallWindowProcW")]
    [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
    [SupportedOSPlatform("windows5.0")]
    private static extern int CallWindowProc(nint lpPrevWndFunc, HWND hWnd, int msg, nint wParam, nint lParam);

    public void Dispose()
    {
        _deviceContext?.Dispose();
        _renderHandler.Dispose();
    }
}