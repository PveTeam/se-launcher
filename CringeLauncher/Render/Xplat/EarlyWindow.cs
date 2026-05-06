#if !WINDOWS
using System.Collections.Concurrent;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
using System.Text;
using NLog;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Silk.NET.Core;
using Silk.NET.SDL;
using D3D11Device = SharpDX.Direct3D11.Device;
using Format = SharpDX.DXGI.Format;
using Point = System.Drawing.Point;
using Thread = System.Threading.Thread;

namespace CringeLauncher.Render.Xplat;

[SupportedOSPlatform("linux")]
internal unsafe class EarlyWindow : IEarlyWindow
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private WindowHandle _handle;
    public uint WindowId { get; private set; }
    private readonly Thread _ownerThread = Thread.CurrentThread;
    private readonly ConcurrentQueue<PendingInvocation> _invokeQueue = new();
    private readonly SdlRenderLoop _renderLoop;
    private Size? _newSize;
    private D3D11Device? _device;
    private SwapChain? _swapChain;
    private readonly XplatImGuiHandler _guiHandler;

    public EarlyWindow()
    {
        _renderLoop = new();
        _guiHandler = (XplatImGuiHandler)ImGuiHandler.Instance!;
    }

    private bool InvokeRequired => Thread.CurrentThread != _ownerThread;
    
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        IsDisposed = true;
        if (_handle != default) 
            Sdl.DestroyWindow(_handle);
        _handle = default;
        WindowId = 0;
    }

    public WindowState State { get; set; } = WindowState.Maximized;
    public FullScreenMode CurrentMode { get; private set; } = FullScreenMode.Borderless;
    public D3D11Device? DeviceInstance => _device;
    public SwapChain? SwapChainInstance => _swapChain;
    public VRageWindowSurrogate Surrogate => field ??= new(this, _renderLoop);
    public nint Handle => (nint)_handle.Handle;
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
                    Sdl.ShowWindow(_handle);
                else
                    Sdl.HideWindow(_handle);
            }
        }
    }

    public bool IsHandleCreated => _handle.Handle is not null;
    public bool IsDisposed { get; private set; }

    public string ClipboardText
    {
        get => Utf8StringMarshaller.ConvertToManaged((byte*)Sdl.GetClipboardText().Native) ?? string.Empty;
        set
        {
            if (InvokeRequired)
                Invoke(Do);
            else
                Do();

            void Do() => Sdl.SetClipboardText(value);
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

        void Do() => _renderLoop.Dispose();
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
            Sdl.ShowWindow(_handle);
        }
    }

    public void Hide()
    {
        if (InvokeRequired)
            Invoke(Do);
        else
            Do();

        void Do() => Sdl.HideWindow(_handle);
    }

    public void DoEvents()
    {
        Event @event = default;
        while (Sdl.PollEvent(&@event) != 0)
        {
            DispatchEvent(@event);
        }
        while (_invokeQueue.TryDequeue(out var pendingInvocation))
        {
            try
            {
                pendingInvocation.Callback();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Exception in dispatcher invocation");
                var closeEvent = new Event
                {
                    Window =
                    {
                        Type = EventType.WindowCloseRequested,
                        WindowID = WindowId
                    }
                };
                Sdl.PushEvent(&closeEvent);
                break;
            }
            pendingInvocation.ResetEvent.Set();
        }
    }

    private void DispatchEvent(in Event @event)
    {
        switch (@event.Type)
        {
            case (uint)EventType.MouseMotion when @event.Motion.WindowID == WindowId:
                float x;
                float y;
                if (Sdl.GetWindowRelativeMouseMode(_handle))
                {
                    float xDelta = 0;
                    float yDelta = 0;
                    Sdl.GetRelativeMouseState(&xDelta, &yDelta);
                    x = LastMousePosition.X + xDelta;
                    y = LastMousePosition.Y + yDelta;
                }
                else
                {
                    x = @event.Motion.X;
                    y = @event.Motion.Y;
                }
                LastMousePosition = new((int)Math.Floor(x), (int)Math.Floor(y));
                break;
            case (uint)EventType.WindowPixelSizeChanged when @event.Window.WindowID == WindowId:
            {
                var width = @event.Window.Data1;
                var height = @event.Window.Data2;
                ClientSize = new(width, height);
                ClientRectangle = ClientRectangle with
                {
                    Width = width,
                    Height = height
                };
                Resize?.Invoke(this, EventArgs.Empty);
                _newSize = new(width, height);
                break;
            }
            case (uint)EventType.WindowFocusGained when @event.Window.WindowID == WindowId:
                GotFocus?.Invoke(this, EventArgs.Empty);
                break;
            case (uint)EventType.WindowFocusLost when @event.Window.WindowID == WindowId:
                LostFocus?.Invoke(this, EventArgs.Empty);
                break;
            case (uint)EventType.WindowMoved when @event.Window.WindowID == WindowId:
                ClientRectangle = ClientRectangle with
                {
                    X = @event.Window.Data1,
                    Y = @event.Window.Data2
                };
                break;
            case (uint)EventType.TextInput when @event.Text.WindowID == WindowId:
            {
                var s = Utf8StringMarshaller.ConvertToManaged((byte*)@event.Text.Text);
                if (s is not null)
                    foreach (var c in s)
                        KeyPress?.Invoke(this, new(c));

                break;
            }
            case (uint)EventType.WindowCloseRequested when @event.Window.WindowID == WindowId:
            {
                var args = new ClosingEventArgs(true);
                Closing?.Invoke(this, args);
                if (!args.Cancel)
                    _renderLoop.Dispose();
                break;
            }
        }
        
        if (!_guiHandler.BlockKeys)
        {
            Event?.Invoke(this, @event);
        }
        
        _guiHandler.DispatchEvent(@event);
    }

    public void Invoke(Action action)
    {
        var resetEvent = new ManualResetEventSlim();
        _invokeQueue.Enqueue(new(action, resetEvent));
        resetEvent.Wait();
    }

    public Rectangle CursorClip
    {
        get => Sdl.GetWindowMouseRect(_handle).AsRectangle();
        set
        {
            // breaks relative mode
            /*var rect = new Rect
            {
                X = value.X,
                Y = value.Y,
                W = value.Width,
                H = value.Height
            };
            Sdl.SetWindowMouseRect(_handle, &rect);*/
        }
    }
    
    public void ShowCursor()
    {
        if (InvokeRequired)
            Invoke(Do);
        else
            Do();

        void Do() => Sdl.SetWindowRelativeMouseMode(_handle, false);
    }

    public void HideCursor()
    {
        if (InvokeRequired)
            Invoke(Do);
        else
            Do();

        void Do()
        {
            Sdl.SetWindowRelativeMouseMode(_handle, true);
        }
    }

    public void ConfigureComposition(bool transparent = true)
    {
        Invoke(() =>
        {
            Sdl.SetWindowOpacity(_handle, transparent ? 0 : 1);
            if (!Sdl.TextInputActive(_handle))
                Sdl.StartTextInput(_handle);
        });
    }

    public void DisableCrop()
    {
    }

    public Rectangle RectangleToScreen(Rectangle rectangle) => rectangle;

    public void ResizeFullScreen(FullScreenMode mode = FullScreenMode.Borderless, Rectangle? clientBounds = null,
        Size? windowedClientSize = null)
    {
        CurrentMode = mode;
        
        if (CurrentMode == FullScreenMode.Windowed)
        {
            Sdl.SetWindowAlwaysOnTop(_handle, false);
            Sdl.SetWindowBordered(_handle, true);
            Sdl.RestoreWindow(_handle);
            if (clientBounds.HasValue && windowedClientSize.HasValue)
            {
                var center = clientBounds.Value.Size / 2;
                ClientRectangle =
                    new(
                        new(center.Width - windowedClientSize.Value.Width / 2,
                            center.Height - windowedClientSize.Value.Height / 2), windowedClientSize.Value);
                ClientSize = windowedClientSize.Value;
                Sdl.SetWindowPosition(_handle, ClientRectangle.X, ClientRectangle.Y);
                Sdl.SetWindowSize(_handle, ClientSize.Width, ClientSize.Height);
            }
            State = WindowState.Normal;
            return;
        }
        
        State = WindowState.Maximized;
        
        Sdl.SetWindowBordered(_handle, false);
        if (!clientBounds.HasValue)
        {
            Sdl.MaximizeWindow(_handle);
            return;
        }
        
        var bounds = clientBounds.Value;
        Sdl.SetWindowPosition(_handle, bounds.X, bounds.Y);
        Sdl.SetWindowSize(_handle, bounds.Width, bounds.Height);
        
        Sdl.RestoreWindow(_handle);
        Sdl.MaximizeWindow(_handle);
    }

    private void CreateHandle()
    {
        Log.Debug("Sdl init");
        Sdl.SetAppMetadata("CringeLauncher", default, "com.selauncher.cringelauncher");
        Sdl.SetAppMetadataProperty(Sdl.PropAppMetadataCreatorString, "zznty");
        Sdl.Init(Sdl.InitAudio | Sdl.InitVideo | Sdl.InitGamepad);

        var monitor = Sdl.GetPrimaryDisplay();
        var videoMode = Sdl.GetCurrentDisplayMode(monitor);
        
        Log.Debug("Create window");
        _handle = Sdl.CreateWindow(Title, videoMode.Handle.W, videoMode.Handle.H,
            Sdl.WindowHidden | Sdl.WindowBorderless | Sdl.WindowMaximized | Sdl.WindowVulkan |
            Sdl.WindowHighPixelDensity | Sdl.WindowTransparent | Sdl.WindowAlwaysOnTop);
        WindowId = Sdl.GetWindowID(_handle);

        var width = 0;
        var height = 0;
        Sdl.GetWindowSizeInPixels(_handle, &width, &height);
        ClientSize = new(width, height);
        var x = 0;
        var y = 0;
        Sdl.GetWindowPosition(_handle, &x, &y);
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

    public event EarlyWindowEventHandler<ClosingEventArgs>? Closing;
    public event EarlyWindowEventHandler? GotFocus;
    public event EarlyWindowEventHandler? LostFocus;
    public event EarlyWindowEventHandler? Resize;
    public event EarlyWindowEventHandler<KeyPressEventArgs>? KeyPress;
    
    public event EarlyWindowEventHandler<Event>? Event;

    private class SdlRenderLoop : IRenderLoop
    {
        private bool _shouldClose;
        public void Dispose()
        {
            _shouldClose = true;
        }

        public bool NextFrame() => !_shouldClose;
    }

    private record PendingInvocation(Action Callback, ManualResetEventSlim ResetEvent);
}

internal static class SdlExtensions
{
    extension(Ptr<Rect> ptr)
    {
        public Rectangle AsRectangle() => new(ptr.Handle.X, ptr.Handle.Y, ptr.Handle.W, ptr.Handle.H);
    }
}
#endif
