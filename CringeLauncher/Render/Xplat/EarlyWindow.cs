#if !WINDOWS
using System.Collections.Concurrent;
using System.Runtime.Versioning;
using NLog;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Silk.NET.GLFW;
using D3D11Device = SharpDX.Direct3D11.Device;
using Format = SharpDX.DXGI.Format;

namespace CringeLauncher.Render.Xplat;

[SupportedOSPlatform("linux")]
internal unsafe class EarlyWindow : IEarlyWindow
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private WindowHandle* _handle;
    private readonly Thread _ownerThread = Thread.CurrentThread;
    private readonly ConcurrentQueue<PendingInvocation> _invokeQueue = new();
    private readonly IRenderLoop _renderLoop;
    private Size? _newSize;
    private D3D11Device? _device;
    private SwapChain? _swapChain;
    private readonly XplatImGuiHandler _guiHandler;

    public EarlyWindow()
    {
        _renderLoop = new GlfwRenderLoop(this);
        _guiHandler = (XplatImGuiHandler)ImGuiHandler.Instance!;
    }

    private bool InvokeRequired => Thread.CurrentThread != _ownerThread;
    
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        IsDisposed = true;
        if (_handle != null) 
            GlfwProvider.GLFW.Value.DestroyWindow(_handle);
        _handle = null;
    }

    public WindowState State { get; set; } = WindowState.Maximized;
    public FullScreenMode CurrentMode { get; private set; } = FullScreenMode.Borderless;
    public D3D11Device? DeviceInstance => _device;
    public SwapChain? SwapChainInstance => _swapChain;
    public VRageWindowSurrogate Surrogate => field ??= new(this, _renderLoop);
    public nint Handle => (nint)_handle;
    public bool OwnsSwapChain { get; set; } = true;
    public Point LastMousePosition { get; set; }
    public Rectangle ClientRectangle { get; private set; }
    public Size ClientSize { get; private set; }

    public bool Visible
    {
        get;
        set
        {
            field = value;
            
            if (InvokeRequired)
                Invoke(Do);
            else
                Do();

            void Do()
            {
                if (field)
                    GlfwProvider.GLFW.Value.ShowWindow(_handle);
                else
                    GlfwProvider.GLFW.Value.HideWindow(_handle);
            }
        }
    }

    public bool IsHandleCreated => _handle is not null;
    public bool IsDisposed { get; private set; }

    public string ClipboardText
    {
        get => GlfwProvider.GLFW.Value.GetClipboardString(_handle);
        set
        {
            if (InvokeRequired)
                Invoke(Do);
            else
                Do();

            void Do() => GlfwProvider.GLFW.Value.SetClipboardString(_handle, value);
        }
    }

    public string Title { get; set; } = "";

    public bool Frame()
    {
        if (!_renderLoop.NextFrame())
            return false;
        
        if (_swapChain!.Present(0, PresentFlags.Test) == (int)DXGIStatus.Occluded)
        {
            DoEvents();
            return true;
        }

        if (_newSize.HasValue)
        {
            _guiHandler.CleanupRenderTarget();
            _swapChain!.ResizeBuffers(0, _newSize.Value.Width, _newSize.Value.Height, Format.Unknown, SwapChainFlags.AllowModeSwitch);
            _guiHandler.CreateRenderTarget(_device!, _swapChain);
            _newSize = null;
        }

        _device!.ImmediateContext.ClearRenderTargetView(_guiHandler.Rtv, default);
        Draw();
        
        _swapChain.Present(0, PresentFlags.None);
        
        UpdateFrame();
        DoEvents();
        
        return true;
    }

    public void UpdateFrame()
    {
    }

    public void Draw() => _guiHandler.DoRender();

    public void Close()
    {
        if (InvokeRequired)
            Invoke(Do);
        else
            Do();

        void Do() => GlfwProvider.GLFW.Value.SetWindowShouldClose(_handle, true);
    }

    public void Activate()
    {
        if (!IsHandleCreated)
        {
            if (InvokeRequired)
                throw new InvalidOperationException(
                    "Window should be created before activation from non-owning threads");
            
            CreateHandle();
        }
        
        if (InvokeRequired)
            Invoke(Do);
        else
            Do();

        void Do()
        {
            GlfwProvider.GLFW.Value.ShowWindow(_handle);
            GlfwProvider.GLFW.Value.FocusWindow(_handle);
        }
    }

    public void Hide()
    {
        if (InvokeRequired)
            Invoke(Do);
        else
            Do();

        void Do() => GlfwProvider.GLFW.Value.HideWindow(_handle);
    }

    public void DoEvents()
    {
        var api = GlfwProvider.GLFW.Value;
        api.PollEvents();
        while (_invokeQueue.TryDequeue(out var pendingInvocation))
        {
            try
            {
                pendingInvocation.Callback();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Exception in dispatcher invocation");
                api.SetWindowShouldClose(_handle, true);
                break;
            }
            pendingInvocation.ResetEvent.Set();
        }

        api.GetCursorPos(_handle, out var xPos, out var yPos);
        LastMousePosition = new((int)Math.Floor(xPos), (int)Math.Floor(yPos));
    }

    public void Invoke(Action action)
    {
        var resetEvent = new ManualResetEventSlim();
        _invokeQueue.Enqueue(new(action, resetEvent));
        resetEvent.Wait();
    }

    public Rectangle CursorClip
    {
        get =>
            (CursorModeValue)GlfwProvider.GLFW.Value.GetInputMode(_handle, CursorStateAttribute.Cursor) ==
            CursorModeValue.CursorDisabled
                ? Rectangle.Empty
                : ClientRectangle;
        set
        {
            // todo cursor captured?
        }
    }
    
    public void ShowCursor()
    {
        if (InvokeRequired)
            Invoke(Do);
        else
            Do();

        void Do() => GlfwProvider.GLFW.Value.SetInputMode(_handle, CursorStateAttribute.Cursor, CursorModeValue.CursorNormal);
    }

    public void HideCursor()
    {
        if (InvokeRequired)
            Invoke(Do);
        else
            Do();

        void Do() => GlfwProvider.GLFW.Value.SetInputMode(_handle, CursorStateAttribute.Cursor, CursorModeValue.CursorDisabled);
    }

    public void ConfigureComposition(bool transparent = true)
    {
        Invoke(() => GlfwProvider.GLFW.Value.SetWindowOpacity(_handle, transparent ? 0 : 1));
    }

    public void DisableCrop()
    {
    }

    public Rectangle RectangleToScreen(Rectangle rectangle) => rectangle;

    public void ResizeFullScreen(FullScreenMode mode = FullScreenMode.Borderless, Rectangle? clientBounds = null,
        Size? windowedClientSize = null)
    {
        CurrentMode = mode;
        var api = GlfwProvider.GLFW.Value;
        
        if (CurrentMode == FullScreenMode.Windowed)
        {
            api.SetWindowAttrib(_handle, WindowAttributeSetter.Floating, false);
            api.SetWindowAttrib(_handle, WindowAttributeSetter.Decorated, true);
            api.RestoreWindow(_handle);
            if (clientBounds.HasValue && windowedClientSize.HasValue)
            {
                var center = clientBounds.Value.Size / 2;
                ClientRectangle =
                    new(
                        new(center.Width - windowedClientSize.Value.Width / 2,
                            center.Height - windowedClientSize.Value.Height / 2), windowedClientSize.Value);
                ClientSize = windowedClientSize.Value;
                api.SetWindowPos(_handle, ClientRectangle.X, ClientRectangle.Y);
                api.SetWindowSize(_handle, ClientSize.Width, ClientSize.Height);
            }
            State = WindowState.Normal;
            return;
        }
        
        State = WindowState.Maximized;
        
        api.SetWindowAttrib(_handle, WindowAttributeSetter.Decorated, false);
        if (!clientBounds.HasValue)
        {
            api.MaximizeWindow(_handle);
            return;
        }
        
        var bounds = clientBounds.Value;
        api.SetWindowPos(_handle, bounds.X, bounds.Y);
        api.SetWindowSize(_handle, bounds.Width, bounds.Height);
        
        api.RestoreWindow(_handle);
        api.MaximizeWindow(_handle);
    }

    private void CreateHandle()
    {
        Log.Debug("Glfw init");
        var api = GlfwProvider.GLFW.Value;
        
        api.WindowHint(WindowHintBool.Resizable, false);
        api.WindowHint(WindowHintBool.CenterCursor, false);
        api.WindowHint(WindowHintBool.FocusOnShow, false);
        api.WindowHint(WindowHintBool.Focused, false);
        api.WindowHint(WindowHintBool.Floating, true);
        api.WindowHint(WindowHintBool.Decorated, false);
        api.WindowHint(WindowHintBool.Maximized, true);
        api.WindowHint(WindowHintBool.TransparentFramebuffer, true);
        api.WindowHint(WindowHintClientApi.ClientApi, ClientApi.NoApi);

        var monitor = api.GetPrimaryMonitor();
        var videoMode = api.GetVideoMode(monitor);
        
        api.WindowHint(WindowHintInt.RefreshRate, videoMode->RefreshRate);
        
        Log.Debug("Create window");
        _handle = api.CreateWindow(videoMode->Width, videoMode->Height, Title, null, null);

        api.SetFramebufferSizeCallback(_handle, FramebufferSizeCallback);
        api.SetWindowPosCallback(_handle, WindowPosCallback);
        api.SetWindowCloseCallback(_handle, WindowCloseCallback);
        api.SetWindowFocusCallback(_handle, WindowFocusCallback);
        api.SetCharCallback(_handle, WindowCharCallback);
        
        if (api.RawMouseMotionSupported())
            api.SetInputMode(_handle, CursorStateAttribute.RawMouseMotion, true);
        
        api.GetFramebufferSize(_handle, out var width, out var height);
        ClientSize = new(width, height);
        api.GetWindowPos(_handle, out var x, out var y);
        ClientRectangle = new(x, y, width, height);
        
        CreateD3D11Device();
        Log.Debug("ImGui init");
        InitImGui();
    }

    private void CreateD3D11Device()
    {
        D3D11Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, new()
        {
            BufferCount = 2,
            Flags = SwapChainFlags.AllowModeSwitch,
            ModeDescription = new(ClientSize.Width, ClientSize.Height, new(60, 1), Format.R8G8B8A8_UNorm),
            SampleDescription =
            {
                Count = 1,
                Quality = 0
            },
            OutputHandle = Handle,
            Usage = Usage.ShaderInput | Usage.RenderTargetOutput,
            IsWindowed = true,
            SwapEffect = SwapEffect.Discard
        }, out _device, out _swapChain);

        using var factory = _swapChain.GetParent<Factory>();
        factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);
    }

    private void InitImGui()
    {
        _guiHandler.CleanupRenderTarget();
        _guiHandler.CreateRenderTarget(_device!, _swapChain!);
        _guiHandler.Init(_handle, _device!, _device!.ImmediateContext);
    }

    private void WindowCharCallback(WindowHandle* window, uint codepoint)
    {
        KeyPress?.Invoke(this, new((char)codepoint));
    }

    private void WindowFocusCallback(WindowHandle* window, bool focused)
    {
        if (focused)
            GotFocus?.Invoke(this, EventArgs.Empty);
        else
            LostFocus?.Invoke(this, EventArgs.Empty);
    }

    private void WindowCloseCallback(WindowHandle* window)
    {
        var args = new ClosingEventArgs(true);
        Closing?.Invoke(this, args);
        if (args.Cancel)
            GlfwProvider.GLFW.Value.SetWindowShouldClose(window, false);
    }

    private void WindowPosCallback(WindowHandle* window, int x, int y)
    {
        ClientRectangle = ClientRectangle with
        {
            X = x,
            Y = y
        };
    }

    private void FramebufferSizeCallback(WindowHandle* window, int width, int height)
    {
        ClientSize = new(width, height);
        ClientRectangle = ClientRectangle with
        {
            Width = width,
            Height = height
        };
        Resize?.Invoke(this, EventArgs.Empty);
        _newSize = new(width, height);
    }

    public event EarlyWindowEventHandler<ClosingEventArgs>? Closing;
    public event EarlyWindowEventHandler? GotFocus;
    public event EarlyWindowEventHandler? LostFocus;
    public event EarlyWindowEventHandler? Resize;
    public event EarlyWindowEventHandler<KeyPressEventArgs>? KeyPress;

    private class GlfwRenderLoop(EarlyWindow window) : IRenderLoop
    {
        public void Dispose()
        {
        }

        public bool NextFrame() => !GlfwProvider.GLFW.Value.WindowShouldClose(window._handle);
    }

    private record PendingInvocation(Action Callback, ManualResetEventSlim ResetEvent);
}
#endif
