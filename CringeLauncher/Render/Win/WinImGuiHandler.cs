#if WINDOWS
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using ImGuiNET;
using SharpDX.Direct3D11;
using VRage;
using static ImGuiNET.ImGui;

namespace CringeLauncher.Render.Win;

internal sealed class WinImGuiHandler(DirectoryInfo configDir) : ImGuiHandler(configDir)
{
    private int _blockKeysCounter;
    private static nint _wndproc;

    public override bool BlockKeys => _blockKeysCounter > 0;

    public override bool Initialized => _init;

    private static bool _init;

    public new static WinImGuiHandler? Instance => (WinImGuiHandler?)ImGuiHandler.Instance;

    public unsafe void Init(nint windowHandle, Device device, DeviceContext deviceContext)
    {
        base.Init(device, deviceContext);

        ImGui_ImplWin32_Init(windowHandle);
        _init = true;
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
    private static int WndProcHook(HWND hWnd, int msg, nint wParam, nint lParam)
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

    protected override void SetupFrame(ImGuiIOPtr io)
    {
        base.SetupFrame(io);
        ImGui_ImplWin32_NewFrame();
            
        if (io.WantTextInput)
            _blockKeysCounter = 10; //WantTextInput can be false briefly after pressing enter in a textbox
        else
            _blockKeysCounter--;
    }
}
#endif
