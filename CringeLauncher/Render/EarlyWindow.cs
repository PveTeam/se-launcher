using NLog;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;
using Message = System.Windows.Forms.Message;

namespace CringeLauncher.Render;

internal sealed class EarlyWindow : Form
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private Device? _device;
    private SwapChain? _swapChain;
    private VRageWindowSurrogate? _surrogate;
    private EarlyImGuiHandler? _guiHandler;
    private RenderLoop? _renderLoop;
    private Size? _newSize;
    private Point _lastMousePosition;
    
    public FullScreenMode CurrentMode { get; private set; }

    public Device? DeviceInstance => _device;

    public SwapChain? SwapChainInstance => _swapChain;

    public VRageWindowSurrogate Surrogate => _surrogate ??= new(this, _renderLoop ??= new(this));

    public new nint Handle { get; private set; }

    public bool ReflectResize { get; set; } = true;

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

    public void Frame()
    {
        if (!_renderLoop!.NextFrame())
            return;

        if (_swapChain!.Present(0, PresentFlags.Test) == (int)DXGIStatus.Occluded)
            return;

        if (_newSize.HasValue)
        {
            _guiHandler!.CleanupRenderTarget();
            _swapChain.ResizeBuffers(0, _newSize.Value.Width, _newSize.Value.Height, Format.Unknown, SwapChainFlags.AllowModeSwitch);
            _guiHandler.CreateRenderTarget(_device!, _swapChain);
            _newSize = null;
        }
        
        _device!.ImmediateContext.ClearRenderTargetView(_guiHandler!.RenderTarget, default);
        Draw();

        _swapChain.Present(0, PresentFlags.None);
    }

    public void Draw() => _guiHandler!.Render();

    protected override void CreateHandle()
    {
        base.CreateHandle();
        Handle = base.Handle;
        CreateD3D11Device();
        CreateImGui();
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

    public void ResizeFullScreen(FullScreenMode mode = FullScreenMode.Borderless)
    {
        CurrentMode = mode;
        SizeGripStyle = SizeGripStyle.Hide;
        
        if (mode == FullScreenMode.Windowed)
        {
            FormBorderStyle = FormBorderStyle.FixedSingle;
            TopMost = false;
            WindowState = FormWindowState.Normal;
            return;
        }
        
        FormBorderStyle = FormBorderStyle.None;
        WindowState = FormWindowState.Maximized;
        Location = new Point(0, 0);
    }

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
        if (ReflectResize)
            _newSize = ClientSize;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void Dispose(bool disposing)
    {
        _renderLoop?.Dispose();
        SwapChainInstance?.Dispose();
        DeviceInstance?.Dispose();
        _guiHandler = null;
        _surrogate = null;
        base.Dispose(disposing);
    }
}

public enum FullScreenMode
{
    Windowed,
    Borderless,
    Fullscreen
}