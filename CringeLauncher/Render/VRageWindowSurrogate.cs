using SharpDX.Windows;
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VRage;
using VRage.Platform.Windows.IME;
using VRageMath;
using Windows.Win32;
using Windows.Win32.System.DataExchange;
using CringeLauncher.Patches;
using VRage.Platform.Windows.Render;
using VRageRender;
using Rectangle = System.Drawing.Rectangle;

namespace CringeLauncher.Render;

internal class VRageWindowSurrogate : IVRageWindow, IVRageInput
{
    public IEarlyWindow Window { get; }
    private readonly IRenderLoop _renderLoop;

    private readonly Lock _lock = new();
    private readonly Dictionary<uint, ActionRef<MyMessage>> _messageHandlers = [];
    private readonly List<Action> _frameCallbacks = [];

    private MyGuiControlIme? _imeControl;
    private List<char> _textBuffer = [];
    private MyRenderDeviceSettings? _pendingSettings;

    private bool _shouldUpdateWindowFrame = true;

    public VRageWindowSurrogate(IEarlyWindow window, IRenderLoop renderLoop)
    {
        Window = window;
        _renderLoop = renderLoop;

        Window.Closing += WindowOnClosing;
        Window.GotFocus += WindowOnGotFocus;
        Window.LostFocus += WindowOnLostFocus;
        Window.KeyPress += WindowOnKeyPress;
        
        GameReadyHandlerPatch.GameReady += OnGameReady;
        GameReadyHandlerPatch.GameReadyTransitionStarted += OnGameReadyTransitionStarted;
    }

    public void AddFrameCallback(Action action)
    {
        _frameCallbacks.Add(action);
    }

    private void OnGameReady()
    {
        Window.ConfigureComposition(false);
    }

    private void OnGameReadyTransitionStarted()
    {
        _shouldUpdateWindowFrame = false;
        Window.DisableCrop();
    }

    private void WindowOnKeyPress(object? sender, KeyPressEventArgs e)
    {
        AddChar(e.KeyChar);
        e.Cancel = true;
    }

    private void WindowOnLostFocus(object? sender, EventArgs e)
    {
        IsActive = false;
        ActiveChanged?.Invoke();
        
        Window.CursorClip = Rectangle.Empty;
        if (!ShowCursor) CursorVisible = true;
    }

    private void WindowOnGotFocus(object? sender, EventArgs e)
    {
        IsActive = true;
        ActiveChanged?.Invoke();
        
        if (!ShowCursor) CursorVisible = false;
        
        if (Window.CurrentMode == FullScreenMode.Fullscreen)
            MyPlatformRender.RestoreFullscreenMode();
    }

    public void InitializeIme(Type imeCandidateType)
    {
#if WINDOWS
        MyImeProcessor.CreateInstance(imeCandidateType);
        var form = (Win.EarlyWindow)Window;
        form.ImeMode = ImeMode.On;
        _imeControl = new()
        {
            Size = new Size(0, 10),
            AutoFocusing = true
        };
        form.Controls.Add(_imeControl);
        _imeControl.ActivateInputListening();
#endif
    }

    private void UpdateClip()
    {
        if (Window.IsDisposed) return;

        if (IsActive && (MouseCapture || !ShowCursor))
        {
            Window.CursorClip = Window.RectangleToScreen(Window.ClientRectangle);
            return;
        }
        
        Window.CursorClip = Rectangle.Empty;
    }

    private void WindowOnClosing(IEarlyWindow window, ClosingEventArgs e)
    {
        if (e.UserClosing)
        {
            e.Cancel = true;
            if (OnManualWindowCloseRequest is not null && Window.Visible)
            {
                OnManualWindowCloseRequest.Invoke();
                return;
            }
            Hide();
        }

        OnExit?.Invoke();
    }

    public void CloseManually()
    {
        if (!Window.IsHandleCreated || Window.IsDisposed)
            return;

        Window.Close();
    }

    public void DoEvents() => Window.DoEvents();

    public void Exit()
    {
        if (!Window.IsDisposed)
            Window.Dispose();
    }

    public bool UpdateRenderThread()
    {
        UpdateClip();
        if (_pendingSettings.HasValue)
        {
            MyRenderProxy.UnloadContent();
            MyRenderProxy.ApplySettings(_pendingSettings.Value);
            _pendingSettings = null;
        }
        
        foreach (var callback in _frameCallbacks)
            callback();
        
        if (_shouldUpdateWindowFrame)
            Window.UpdateFrame();
        return _renderLoop.NextFrame();
    }
    
    public void ApplyRenderSettings(MyRenderDeviceSettings settings)
    {
        _pendingSettings = settings;
    }

    public void SetCursor(Stream stream)
    {
        /*if (!Window.IsHandleCreated || Window.IsDisposed)
            return;
        var bitmap = new Bitmap(stream);

        Window.Invoke(() =>
        {
            Window.Cursor = new Cursor(bitmap.GetHicon());
        });*/
    }

#if WINDOWS
    private readonly ConcurrentQueue<DataMessage> _messages = [];

    public unsafe void EnqueueMessage(ref Message message)
    {
        // love keen
        if (message.Msg == PInvoke.WM_INPUTLANGCHANGE && MyImeProcessor.Instance is not null)
        {
            MyImeProcessor.Instance.LanguageChanged();
            return;
        }

        using (_lock.EnterScope())
        {
            if (!_messageHandlers.ContainsKey((uint)message.Msg))
                return;
        }

        ArraySegment<byte>? dataSegment = null;
        if (message.Msg == PInvoke.WM_COPYDATA)
        {
            var copyData = (COPYDATASTRUCT*)message.LParam;
            if (copyData->cbData > int.MaxValue)
                throw new ArgumentOutOfRangeException(null, $"Copy data size is too big: {copyData->cbData}");

            var dataSpan = new ReadOnlySpan<byte>(copyData->lpData, (int)copyData->cbData);

            var array = ArrayPool<byte>.Shared.Rent((int)copyData->cbData);
            dataSegment = new ArraySegment<byte>(array, 0, (int)copyData->cbData);
            dataSpan.CopyTo(dataSegment.Value);
        }

        _messages.Enqueue(new DataMessage(message, dataSegment));
    }
    
    private readonly record struct DataMessage(Message Message, ArraySegment<byte>? Data) : IDisposable
    {
        public void Dispose()
        {
            if (Data is { Array: { } array })
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }
    }
    
    public void UpdateMainThread()
    {
        using var scope = _lock.EnterScope();
        while (_messages.TryDequeue(out var dataMessage))
            try
            {
                var (message, dataSegment) = dataMessage;

                if (!_messageHandlers.TryGetValue((uint)message.Msg, out var handler))
                    continue;

                var msg = new MyMessage
                {
                    Msg = message.Msg,
                    WParam = message.WParam,
                    LParam = message.LParam
                };

                if (message.Msg == PInvoke.WM_COPYDATA && dataSegment.HasValue)
                {
                    unsafe
                    {
                        fixed (byte* ptr = &dataSegment.Value.AsSpan().GetPinnableReference())
                        {
                            msg.LParam = (nint)ptr;
                            handler(ref msg);
                        }
                    }
                }
                else
                    handler(ref msg);
            }
            finally
            {
                dataMessage.Dispose();
            }
    }
    
    public int KeyboardDelay { get; } = SystemInformation.KeyboardDelay;
    public int KeyboardSpeed { get; } = SystemInformation.KeyboardSpeed;
#else
    public void UpdateMainThread()
    {
        
    }
    
    public int KeyboardDelay { get; } = 0;
    public int KeyboardSpeed { get; } = 0;
#endif

    public void AddMessageHandler(uint wm, ActionRef<MyMessage> action)
    {
        using var scope = _lock.EnterScope();
        ref var delegateRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_messageHandlers, wm, out _);
        delegateRef += action;
    }

    public void RemoveMessageHandler(uint wm, ActionRef<MyMessage> action)
    {
        using var scope = _lock.EnterScope();
        ref var delegateRef = ref CollectionsMarshal.GetValueRefOrNullRef(_messageHandlers, wm);
        if (Unsafe.IsNullRef(ref delegateRef))
            return;
#pragma warning disable CS8601 // Possible null reference assignment. Stupid roslyn.
        delegateRef -= action;
#pragma warning restore CS8601 // Possible null reference assignment.
    }

    public void ShowAndFocus()
    {
        if (!Window.IsHandleCreated || Window.IsDisposed)
            return;

        Window.Activate();
        IsActive = true;
        ActiveChanged?.Invoke();
    }

    public void Hide()
    {
        if (!Window.IsHandleCreated || Window.IsDisposed)
            return;

        Window.Hide();
    }

    public bool DrawEnabled => Window.State != WindowState.Minimized;
    public bool IsActive { get; set; }
    public event Action? ActiveChanged;
    public Vector2I ClientSize => new(Window.ClientSize.Width, Window.ClientSize.Height);
    public event Action? OnExit;
    public event Action? OnManualWindowCloseRequest;

    public void AddChar(char ch) => _textBuffer.Add(ch);

    public void GetBufferedTextInput(ref List<char> currentTextInput)
    {
        currentTextInput.Clear();
        currentTextInput = Interlocked.Exchange(ref _textBuffer, currentTextInput);
    }

    public Vector2 MousePosition
    {
        get => new(Window.LastMousePosition.X, Window.LastMousePosition.Y);
        set => Window.LastMousePosition = new((int)value.X, (int)value.Y);
    }
    public Vector2 MouseAreaSize => new(Window.ClientSize.Width, Window.ClientSize.Height);

    public bool MouseCapture
    {
        get;
        set
        {
            field = value;
            Window.Invoke(UpdateClip);
        }
    }

    public bool ShowCursor
    {
        get => CursorVisible;
        set => CursorVisible = value;
    }

    public bool CursorVisible
    {
        get;
        set
        {
            if (Interlocked.Exchange(ref field, value) == value) return;
            
            if (value)
                Window.ShowCursor();
            else
                Window.HideCursor();
        }
    } = true;
}
