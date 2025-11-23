using System.Drawing;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace CringeLauncher.Render;

internal interface IEarlyWindow : IDisposable
{
    WindowState State { get; set; }
    FullScreenMode CurrentMode { get; }
    Device? DeviceInstance { get; }
    SwapChain? SwapChainInstance { get; }
    VRageWindowSurrogate Surrogate { get; }
    nint Handle { get; }
    bool OwnsSwapChain { get; set; }
    Point LastMousePosition { get; set; }
    Rectangle ClientRectangle { get; }
    Size ClientSize { get; }
    
    bool Visible { get; set; }
    bool IsHandleCreated { get; }
    bool IsDisposed { get; }
    
    string ClipboardText { get; set; }
    string Title { get; set; }

    bool Frame();
    void UpdateFrame();
    void Draw();

    void Close();
    void Activate();
    void Hide();

    void DoEvents();
    void Invoke(Action action);
    Rectangle CursorClip { get; set; }
    void ShowCursor();
    void HideCursor();
    
    void ConfigureComposition(bool transparent = true);
    void DisableCrop();

    Rectangle RectangleToScreen(Rectangle rectangle);

    void ResizeFullScreen(FullScreenMode mode = FullScreenMode.Borderless, Rectangle? clientBounds = null,
        Size? windowedClientSize = null);
    
    event EarlyWindowEventHandler<ClosingEventArgs> Closing;
    event EarlyWindowEventHandler GotFocus;
    event EarlyWindowEventHandler LostFocus;
    event EarlyWindowEventHandler Resize;
    event EarlyWindowEventHandler<KeyPressEventArgs> KeyPress;
}

internal interface IRenderLoop : IDisposable
{
    bool NextFrame();
}

internal delegate void EarlyWindowEventHandler<in TArgs>(IEarlyWindow window, TArgs e);
internal delegate void EarlyWindowEventHandler(IEarlyWindow window, EventArgs e);

internal abstract record CancelableEventArgs
{
    public bool Cancel { get; set; }
}

internal record ClosingEventArgs(bool UserClosing) : CancelableEventArgs;
internal record KeyPressEventArgs(char KeyChar) : CancelableEventArgs;
