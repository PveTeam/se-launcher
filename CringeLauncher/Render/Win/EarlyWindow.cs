#if WINDOWS
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.Graphics.Gdi;
using NLog;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;
using Message = System.Windows.Forms.Message;

namespace CringeLauncher.Render.Win;

internal sealed class EarlyWindow : Form, IEarlyWindow
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private Device? _device;
    private SwapChain? _swapChain;
    private VRageWindowSurrogate? _surrogate;
    private EarlyImGuiHandler? _guiHandler;
    private RenderLoop? _renderLoop;
    private Size? _newSize;
    private Point _lastMousePosition;
    private nint _handle;

    public WindowState State
    {
        get => (WindowState)WindowState;
        set
        {
            if (InvokeRequired)
                Invoke(() => WindowState = (FormWindowState)value);
            else
                WindowState = (FormWindowState)value;
        } 
    }
    public FullScreenMode CurrentMode { get; private set; }

    public Device? DeviceInstance => _device;

    public SwapChain? SwapChainInstance => _swapChain;

    public VRageWindowSurrogate Surrogate => _surrogate ??= new(this, new WinRenderLoop(_renderLoop ??= new(this)));

    public bool OwnsSwapChain { get; set; } = true;

    public Point LastMousePosition
    {
        get => _lastMousePosition;
        set
        {
            Invoke(() =>
            {
                Cursor.Position = PointToScreen(value);
            });
            _lastMousePosition = value;
        }
    }

    bool IEarlyWindow.Visible
    {
        get => Visible;
        set
        {
            if (InvokeRequired)
                Invoke(() => Visible = value);
            else
                Visible = value;
        }
    }

    public EarlyWindow()
    {
        ResizeFullScreen();
        SetStyle(
            ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor,
            true);
        BackColor = Color.Transparent;

        Icon? icon;
        try
        {
            icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }
        catch (Exception e)
        {
            Log.Warn(e, "Failed to extract icon from executable");
            icon = null;
        }
        if (icon is not null)
            Icon = icon;
    }

    public string ClipboardText
    {
        get => InvokeRequired ? Invoke(Clipboard.GetText) : Clipboard.GetText();
        set
        {
            if (InvokeRequired)
                Invoke(() => Clipboard.SetText(value));
            else
                Clipboard.SetText(value);
        }
    }

    public string Title
    {
        get => Text;
        set => Text = value;
    }

    public bool Frame()
    {
        if (!_renderLoop!.NextFrame())
            return false;

        if (_swapChain!.Present(0, PresentFlags.Test) == (int)DXGIStatus.Occluded)
            return true;

        if (_newSize.HasValue)
        {
            WinImGuiHandler.Instance!.CleanupRenderTarget();
            _swapChain.ResizeBuffers(0, _newSize.Value.Width, _newSize.Value.Height, Format.Unknown, SwapChainFlags.AllowModeSwitch);
            WinImGuiHandler.Instance.CreateRenderTarget(_device!, _swapChain);
            _newSize = null;
        }
        
        _device!.ImmediateContext.ClearRenderTargetView(_guiHandler!.RenderTarget, default);
        Draw();

        _swapChain.Present(0, PresentFlags.None);
        
        UpdateFrame();
        
        return true;
    }

    public void UpdateFrame()
    {
        if (CurrentMode != FullScreenMode.Borderless) return;
        var region = _guiHandler!.GetWindowRegions();
        if (region is null) return;
        Region = region;
    }

    void IEarlyWindow.Close()
    {
        if (InvokeRequired)
            Invoke(Close);
        else
            Close();
    }
    
    void IEarlyWindow.Hide()
    {
        if (InvokeRequired)
            Invoke(Hide);
        else
            Hide();
    }

    void IEarlyWindow.Activate()
    {
        if (InvokeRequired)
        {
            Invoke(() =>
            {
                Show();
                Activate();
            });
        }
        else
        {
            Show();
            Activate();
        }
    }

    public void DoEvents() => Application.DoEvents();

    Rectangle IEarlyWindow.CursorClip
    {
        get => Cursor.Clip;
        set => Cursor.Clip = value;
    }

    public void Draw() => _guiHandler?.Render();

    protected override void CreateHandle()
    {
        base.CreateHandle();
        _handle = base.Handle;
        CreateD3D11Device();
        CreateImGui();
        ConfigureCompositionInternal();
        Region = new Region(Rectangle.Empty);
    }

    void IEarlyWindow.ShowCursor()
    {
        if (InvokeRequired)
            Invoke(Cursor.Show);
        else
            Cursor.Show();
    }

    void IEarlyWindow.HideCursor()
    {
        if (InvokeRequired)
            Invoke(Cursor.Hide);
        else
            Cursor.Hide();
    }

    public void ConfigureComposition(bool enableBlurBehind = true)
    {
        if (InvokeRequired)
            Invoke(() => ConfigureCompositionInternal(enableBlurBehind));
        else
            ConfigureCompositionInternal(enableBlurBehind);
    }

    private void ConfigureCompositionInternal(bool enableBlurBehind = true)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(8) ||
            PInvoke.DwmIsCompositionEnabled(out var compositionEnabled).Failed || !compositionEnabled)
            return;

        HRESULT result;
        if (enableBlurBehind)
        {
            using var region = PInvoke.CreateRectRgn_SafeHandle(0, 0, -1, -1);

            var blurBehind = new DWM_BLURBEHIND
            {
                dwFlags = PInvoke.DWM_BB_ENABLE | PInvoke.DWM_BB_BLURREGION,
                hRgnBlur = (HRGN)region.DangerousGetHandle(),
                fEnable = true
            };

            result = PInvoke.DwmEnableBlurBehindWindow((HWND)Handle, in blurBehind);
        }
        else
        {
            var blurBehindDisable = new DWM_BLURBEHIND
            {
                dwFlags = PInvoke.DWM_BB_ENABLE,
                fEnable = false
            };
            result = PInvoke.DwmEnableBlurBehindWindow((HWND)Handle, in blurBehindDisable);
        }
        
        if (result.Succeeded) return;
        
        Log.Error(Marshal.GetExceptionForHR(result), "Failed to change Dwm blur behind state");
    }

    void IEarlyWindow.DisableCrop()
    {
        if (InvokeRequired)
            Invoke(() => Region = null);
        else
            Region = null;
    }

    private void CreateImGui()
    {
        _guiHandler = new();
        _guiHandler.CreateContext(Handle, _device!, _swapChain!);
    }

    private void CreateD3D11Device()
    {
        Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, new()
        {
            BufferCount = 2,
            Flags = SwapChainFlags.AllowModeSwitch,
            ModeDescription = new(ClientSize.Width, ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
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

        _renderLoop = new(this);
    }

    public void ResizeFullScreen(FullScreenMode mode = FullScreenMode.Borderless, Rectangle? clientBounds = null, Size? windowedClientSize = null)
    {
        CurrentMode = mode;
        SizeGripStyle = SizeGripStyle.Hide;
        
        // reset region if not borderless aka input transparent
        if (mode != FullScreenMode.Borderless) Region = null;
        
        if (mode == FullScreenMode.Windowed)
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            TopMost = false;
            WindowState = FormWindowState.Normal;
            if (clientBounds.HasValue && windowedClientSize.HasValue)
            {
                var center = clientBounds.Value.Size / 2;
                Location = new Point(center.Width - windowedClientSize.Value.Width / 2, center.Height - windowedClientSize.Value.Height / 2);
                ClientSize = windowedClientSize.Value;
            }
            return;
        }
        
        FormBorderStyle = FormBorderStyle.None;
        if (!clientBounds.HasValue)
        {
            WindowState = FormWindowState.Maximized;
            return;
        }
        
        var bounds = clientBounds.Value;
        SetDesktopBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        
        WindowState = FormWindowState.Normal;
        WindowState = FormWindowState.Maximized;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        var args = new ClosingEventArgs(e.CloseReason == CloseReason.UserClosing)
        {
            Cancel = e.Cancel
        };
        ClosingEvent?.Invoke(this, args);
        e.Cancel = args.Cancel;
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        GotFocusEvent?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);
        ResizeEvent?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnKeyPress(System.Windows.Forms.KeyPressEventArgs e)
    {
        base.OnKeyPress(e);
        var args = new KeyPressEventArgs(e.KeyChar)
        {
            Cancel = e.Handled
        };
        KeyPressEvent?.Invoke(this, args);
        e.Handled = args.Cancel;
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        LostFocusEvent?.Invoke(this, EventArgs.Empty);
    }

    private event EarlyWindowEventHandler<ClosingEventArgs>? ClosingEvent;
    event EarlyWindowEventHandler<ClosingEventArgs>? IEarlyWindow.Closing
    {
        add => ClosingEvent += value;
        remove => ClosingEvent -= value;
    }

    private event EarlyWindowEventHandler? GotFocusEvent;
    event EarlyWindowEventHandler? IEarlyWindow.GotFocus
    {
        add => GotFocusEvent += value;
        remove => GotFocusEvent -= value;
    }

    private event EarlyWindowEventHandler? LostFocusEvent;
    event EarlyWindowEventHandler? IEarlyWindow.LostFocus
    {
        add => LostFocusEvent += value;
        remove => LostFocusEvent -= value;
    }
    
    private event EarlyWindowEventHandler? ResizeEvent;
    event EarlyWindowEventHandler? IEarlyWindow.Resize
    {
        add => ResizeEvent += value;
        remove => ResizeEvent -= value;
    }

    private event EarlyWindowEventHandler<KeyPressEventArgs>? KeyPressEvent;
    event EarlyWindowEventHandler<KeyPressEventArgs>? IEarlyWindow.KeyPress
    {
        add => KeyPressEvent += value;
        remove => KeyPressEvent -= value;
    }

    nint IEarlyWindow.Handle => _handle;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _lastMousePosition = e.Location;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _lastMousePosition = new Point(-1, -1);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        _surrogate?.EnqueueMessage(ref m);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (OwnsSwapChain)
            _newSize = ClientSize;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void Dispose(bool disposing)
    {
        // render loop wants to listen to dispose event to dispose itself
        // _renderLoop?.Dispose();
        SwapChainInstance?.Dispose();
        DeviceInstance?.Dispose();
        _guiHandler = null;
        _surrogate = null;
        base.Dispose(disposing);
    }

    void IDisposable.Dispose()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        if (InvokeRequired)
            Invoke(Dispose);
        else
            Dispose();
    }
}

internal class WinRenderLoop(RenderLoop loop) : IRenderLoop
{
    public void Dispose() => loop.Dispose();

    public bool NextFrame() => loop.NextFrame();
}

#endif
