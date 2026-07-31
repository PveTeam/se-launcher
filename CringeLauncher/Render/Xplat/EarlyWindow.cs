#if !WINDOWS
using System.Collections.Concurrent;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;
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
    private readonly SdlRenderLoop _renderLoop = new();
    private Size? _newSize;
    private D3D11Device? _device;
    private SwapChain? _swapChain;
    private readonly XplatImGuiHandler _guiHandler = (XplatImGuiHandler)ImGuiHandler.Instance!;

    private Rectangle _logicalBounds;
    private float _pixelDensity = 1f;

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

    public Rectangle ClientRectangle => new(0, 0, ClientSize.Width, ClientSize.Height);

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
        // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
        switch ((EventType)@event.Type)
        {
            case EventType.MouseMotion when @event.Motion.WindowID == WindowId:
            {
                float x;
                float y;
                if (Sdl.GetWindowRelativeMouseMode(_handle))
                {
                    float xDelta = 0;
                    float yDelta = 0;
                    Sdl.GetRelativeMouseState(&xDelta, &yDelta);
                    x = LastMousePosition.X + xDelta * _pixelDensity;
                    y = LastMousePosition.Y + yDelta * _pixelDensity;
                }
                else
                {
                    x = @event.Motion.X * _pixelDensity;
                    y = @event.Motion.Y * _pixelDensity;
                }

                LastMousePosition = new((int)Math.Floor(x), (int)Math.Floor(y));
                break;
            }
            case EventType.WindowPixelSizeChanged when @event.Window.WindowID == WindowId:
            {
                RefreshPixelSize(@event.Window.Data1, @event.Window.Data2);
                break;
            }
            case EventType.WindowResized when @event.Window.WindowID == WindowId:
            {
                _logicalBounds = _logicalBounds with
                {
                    Width = @event.Window.Data1,
                    Height = @event.Window.Data2
                };
                RefreshPixelSize();
                break;
            }
            case EventType.WindowDisplayScaleChanged when @event.Window.WindowID == WindowId:
            {
                RefreshPixelDensity();
                RefreshPixelSize();
                break;
            }
            case EventType.WindowFocusGained when @event.Window.WindowID == WindowId:
                GotFocus?.Invoke(this, EventArgs.Empty);
                break;
            case EventType.WindowFocusLost when @event.Window.WindowID == WindowId:
                LostFocus?.Invoke(this, EventArgs.Empty);
                break;
            case EventType.WindowMoved when @event.Window.WindowID == WindowId:
                _logicalBounds = _logicalBounds with
                {
                    X = @event.Window.Data1,
                    Y = @event.Window.Data2
                };
                break;
            case EventType.WindowDisplayChanged when @event.Window.WindowID == WindowId:
            {
                if (!OwnsSwapChain || CurrentMode is FullScreenMode.Windowed) break;

                Rect bounds = default;
                Sdl.GetDisplayBounds((uint)@event.Window.Data1, &bounds);
                _logicalBounds = new(bounds.X, bounds.Y, bounds.W, bounds.H);
                Sdl.SetWindowPosition(_handle, bounds.X, bounds.Y);
                Sdl.SetWindowSize(_handle, bounds.W, bounds.H);
                RefreshPixelSize();
                break;
            }
            case EventType.TextInput when @event.Text.WindowID == WindowId:
            {
                var s = Utf8StringMarshaller.ConvertToManaged((byte*)@event.Text.Text);
                if (s is not null)
                    foreach (var c in s)
                        KeyPress?.Invoke(this, new(c));

                break;
            }
            case EventType.WindowCloseRequested when @event.Window.WindowID == WindowId:
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

    private void RefreshPixelDensity()
    {
        if (_handle == default)
        {
            _pixelDensity = 1f;
            return;
        }

        var density = Sdl.GetWindowPixelDensity(_handle);
        _pixelDensity = density > 0f ? density : 1f;
    }

    private void RefreshPixelSize(int? pixelWidth = null, int? pixelHeight = null)
    {
        RefreshPixelDensity();

        int width;
        int height;
        if (pixelWidth is > 0 && pixelHeight is > 0)
        {
            width = pixelWidth.Value;
            height = pixelHeight.Value;
        }
        else
        {
            width = 0;
            height = 0;
            Sdl.GetWindowSizeInPixels(_handle, &width, &height);
        }

        if (width <= 0 || height <= 0)
            return;

        var logicalW = 0;
        var logicalH = 0;
        Sdl.GetWindowSize(_handle, &logicalW, &logicalH);
        if (logicalW > 0 && logicalH > 0)
            _logicalBounds = _logicalBounds with { Width = logicalW, Height = logicalH };

        var previous = ClientSize;
        ClientSize = new(width, height);

        if (previous == ClientSize)
            return;

        Resize?.Invoke(this, EventArgs.Empty);
        _newSize = ClientSize;
    }

    private Size PixelsToLogicalSize(Size pixels)
    {
        RefreshPixelDensity();
        var density = _pixelDensity;
        return new(
            Math.Max(1, (int)MathF.Round(pixels.Width / density)),
            Math.Max(1, (int)MathF.Round(pixels.Height / density)));
    }

    private Rectangle PixelsToLogicalBounds(Rectangle pixelBounds)
    {
        RefreshPixelDensity();
        var density = _pixelDensity;
        return new(
            (int)MathF.Round(pixelBounds.X / density),
            (int)MathF.Round(pixelBounds.Y / density),
            Math.Max(1, (int)MathF.Round(pixelBounds.Width / density)),
            Math.Max(1, (int)MathF.Round(pixelBounds.Height / density)));
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

    public Rectangle RectangleToScreen(Rectangle rectangle)
    {
        RefreshPixelDensity();
        var density = Math.Max(_pixelDensity, 0.0001f);
        return new(
            (int)MathF.Floor(_logicalBounds.X + rectangle.X / density),
            (int)MathF.Floor(_logicalBounds.Y + rectangle.Y / density),
            (int)MathF.Floor(rectangle.Width / density),
            (int)MathF.Floor(rectangle.Height / density));
    }

    public void ResizeFullScreen(FullScreenMode mode = FullScreenMode.Borderless, Rectangle? clientBounds = null,
        Size? windowedClientSize = null)
    {
        CurrentMode = mode;

        if (CurrentMode == FullScreenMode.Windowed)
        {
            Sdl.SetWindowFullscreen(_handle, false);
            Sdl.SetWindowAlwaysOnTop(_handle, false);
            Sdl.SetWindowBordered(_handle, true);
            Sdl.RestoreWindow(_handle);
            if (clientBounds.HasValue && windowedClientSize.HasValue)
            {
                var logicalSize = PixelsToLogicalSize(windowedClientSize.Value);
                var logicalBounds = PixelsToLogicalBounds(clientBounds.Value);
                var x = logicalBounds.X + (logicalBounds.Width - logicalSize.Width) / 2;
                var y = logicalBounds.Y + (logicalBounds.Height - logicalSize.Height) / 2;
                Sdl.SetWindowPosition(_handle, x, y);
                Sdl.SetWindowSize(_handle, logicalSize.Width, logicalSize.Height);
                _logicalBounds = new(x, y, logicalSize.Width, logicalSize.Height);
            }
            State = WindowState.Normal;
            RefreshPixelSize();
            return;
        }

        State = WindowState.Maximized;

        Sdl.SetWindowBordered(_handle, false);
        if (!clientBounds.HasValue)
        {
            Sdl.MaximizeWindow(_handle);
            RefreshPixelSize();
            return;
        }

        Sdl.SetWindowFullscreen(_handle, mode is FullScreenMode.Fullscreen);

        var bounds = PixelsToLogicalBounds(clientBounds.Value);
        _logicalBounds = bounds;
        Sdl.SetWindowPosition(_handle, bounds.X, bounds.Y);
        Sdl.SetWindowSize(_handle, bounds.Width, bounds.Height);

        Sdl.RestoreWindow(_handle);
        Sdl.MaximizeWindow(_handle);
        RefreshPixelSize();
    }

    private void CreateHandle()
    {
        Log.Debug("Sdl init");
        Sdl.SetAppMetadata("CringeLauncher", default, "com.selauncher.cringelauncher");
        Sdl.SetAppMetadataProperty(Sdl.PropAppMetadataCreatorString, "zznty");
        byte hintValue = 1;
        fixed (byte* hintName = &Sdl.HintVideoSyncWindowOperations.Bytes.GetPinnableReference())
            Sdl.SetHint(hintName, &hintValue);
        Sdl.Init(Sdl.InitAudio | Sdl.InitVideo | Sdl.InitGamepad);

        var monitor = Sdl.GetPrimaryDisplay();
        Rect displayBounds = default;
        Sdl.GetDisplayBounds(monitor, &displayBounds);
        var logicalWidth = Math.Max(1, displayBounds.W);
        var logicalHeight = Math.Max(1, displayBounds.H);

        Log.Debug("Create window {0}x{1} (window coords)", logicalWidth, logicalHeight);
        _handle = Sdl.CreateWindow(Title, logicalWidth, logicalHeight,
            Sdl.WindowHidden | Sdl.WindowBorderless | Sdl.WindowMaximized | Sdl.WindowVulkan |
            Sdl.WindowHighPixelDensity | Sdl.WindowTransparent | Sdl.WindowAlwaysOnTop | Sdl.WindowFullscreen);
        WindowId = Sdl.GetWindowID(_handle);

        var x = 0;
        var y = 0;
        var logicalW = 0;
        var logicalH = 0;
        Sdl.GetWindowPosition(_handle, &x, &y);
        Sdl.GetWindowSize(_handle, &logicalW, &logicalH);
        _logicalBounds = new(x, y, Math.Max(1, logicalW), Math.Max(1, logicalH));

        RefreshPixelSize();

        Log.Debug("Window pixels {0}x{1}, density {2}, display scale {3}",
            ClientSize.Width, ClientSize.Height, _pixelDensity,
            Math.Max(Sdl.GetWindowDisplayScale(_handle), 0.0001f));

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
