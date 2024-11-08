using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using CringePlugins.Abstractions;
using CringePlugins.Render;
using ImGuiNET;
using SharpDX.Direct3D11;
using static ImGuiNET.ImGui;

namespace CringeLauncher;

internal class ImGuiHandler : IDisposable
{
    private DeviceContext? _deviceContext;
    private static nint _wndproc;

    public static ImGuiHandler? Instance;
    
    public static RenderTargetView? Rtv;
    
    public ImGuiIOPtr Io { get; private set; }
    private readonly IRootRenderComponent _renderHandler = new RenderHandler();

    public unsafe void Init(nint windowHandle, Device1 device, DeviceContext deviceContext)
    {
        _deviceContext = deviceContext;

        CreateContext();
        
        Io = GetIO();
        
        var path = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CringeLauncher", "imgui.ini");

        Io.NativePtr->IniFilename = AnsiStringMarshaller.ConvertToUnmanaged(path);

        Io.ConfigWindowsMoveFromTitleBarOnly = true;
        Io.ConfigFlags |= ImGuiConfigFlags.DockingEnable | ImGuiConfigFlags.ViewportsEnable;
        
        ImGui_ImplWin32_Init(windowHandle);
        ImGui_ImplDX11_Init(device.NativePointer, deviceContext.NativePointer);
    }

    public void HookWindow(HWND windowHandle)
    {
        _wndproc = PInvoke.GetWindowLongPtr(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC);

        unsafe
        {
            delegate* unmanaged[Stdcall]<HWND, int, nint, nint, int> wndProcHook = &WndProcHook;
            
            PInvoke.SetWindowLongPtr(windowHandle, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, (nint)wndProcHook);
        }
    }

    public void DoRender()
    {
        if (Rtv is null)
            return;
        
        ImGui_ImplDX11_NewFrame();
        ImGui_ImplWin32_NewFrame();
        NewFrame();
        
        _renderHandler.OnFrame();
        
        Render();
        
        _deviceContext!.ClearState();
        _deviceContext.OutputMerger.SetRenderTargets(Rtv);
        
        ImGui_ImplDX11_RenderDrawData(GetDrawData());
        
        UpdatePlatformWindows();
        RenderPlatformWindowsDefault();
    }
    
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int WndProcHook(HWND hWnd, int msg, nint wParam, nint lParam)
    {
        var hookResult = ImGui_ImplWin32_WndProcHandler(hWnd, msg, wParam, lParam);
        return hookResult == 0 ? CallWindowProc(_wndproc, hWnd, msg, wParam, lParam) : hookResult;
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