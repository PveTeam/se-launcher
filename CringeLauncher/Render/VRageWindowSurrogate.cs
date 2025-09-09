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
using Message = System.Windows.Forms.Message;
using Rectangle = System.Drawing.Rectangle;

namespace CringeLauncher.Render;

internal class VRageWindowSurrogate : IVRageWindow, IVRageInput
{
    public EarlyWindow Window { get; }
    private readonly RenderLoop _renderLoop;

    private readonly Lock _lock = new();
    private readonly Dictionary<uint, ActionRef<MyMessage>> _messageHandlers = [];
    private readonly ConcurrentQueue<DataMessage> _messages = [];

    private MyGuiControlIme? _imeControl;
    private List<char> _textBuffer = [];
    private MyRenderDeviceSettings? _pendingSettings;

    private bool _shouldUpdateWindowFrame = true;

    public VRageWindowSurrogate(EarlyWindow window, RenderLoop renderLoop)
    {
        Window = window;
        _renderLoop = renderLoop;

        Window.FormClosing += WindowOnFormClosing;
        Window.GotFocus += WindowOnGotFocus;
        Window.LostFocus += WindowOnLostFocus;
        Window.KeyPress += WindowOnKeyPress;
        
        GameReadyHandlerPatch.GameReady += OnGameReady;
        GameReadyHandlerPatch.GameReadyTransitionStarted += OnGameReadyTransitionStarted;
    }

    private void OnGameReady()
    {
        Window.Invoke(() => Window.ConfigureComposition(false));
    }

    private void OnGameReadyTransitionStarted()
    {
        _shouldUpdateWindowFrame = false;
        Window.Invoke(() => Window.Region = null);
    }

    private void WindowOnKeyPress(object? sender, KeyPressEventArgs e)
    {
        AddChar(e.KeyChar);
        e.Handled = true;
    }

    private void WindowOnLostFocus(object? sender, EventArgs e)
    {
        IsActive = false;
        ActiveChanged?.Invoke();
        
        Cursor.Clip = Rectangle.Empty;
        if (!ShowCursor) Cursor.Show();
    }

    private void WindowOnGotFocus(object? sender, EventArgs e)
    {
        IsActive = true;
        ActiveChanged?.Invoke();
        
        if (!ShowCursor) Cursor.Hide();
        
        if (Window.CurrentMode == FullScreenMode.Fullscreen)
            MyPlatformRender.RestoreFullscreenMode();
    }

    public void InitializeIme(Type imeCandidateType)
    {
        MyImeProcessor.CreateInstance(imeCandidateType);
        Window.ImeMode = ImeMode.On;
        _imeControl = new()
        {
            Size = new Size(0, 10),
            AutoFocusing = true
        };
        Window.Controls.Add(_imeControl);
        _imeControl.ActivateInputListening();
    }

    private void UpdateClip()
    {
        if (Window.IsDisposed) return;

        if (IsActive && (MouseCapture || !ShowCursor))
        {
            Cursor.Clip = Window.RectangleToScreen(Window.ClientRectangle);
            return;
        }
        
        Cursor.Clip = Rectangle.Empty;
    }

    private void WindowOnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
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

        Window.Invoke(Window.Close);
    }

    public void DoEvents() => Application.DoEvents();

    public void Exit()
    {
        if (!Window.IsDisposed)
            Window.Invoke(Window.Dispose);
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
        if (_shouldUpdateWindowFrame)
            Window.UpdateFrame();
        return _renderLoop.NextFrame();
    }
    
    public void ApplyRenderSettings(MyRenderDeviceSettings settings)
    {
        _pendingSettings = settings;
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

        Window.Invoke(() =>
        {
            Window.Show();
            Window.Activate();
        });
        IsActive = true;
        ActiveChanged?.Invoke();
    }

    public void Hide()
    {
        if (!Window.IsHandleCreated || Window.IsDisposed)
            return;

        Window.Invoke(Window.Hide);
    }

    public bool DrawEnabled => Window.WindowState != FormWindowState.Minimized;
    public bool IsActive { get; set; }
    public event Action? ActiveChanged;
    public Vector2I ClientSize => new(Window.ClientSize.Width, Window.ClientSize.Height);
    public event Action? OnExit;
    public event Action? OnManualWindowCloseRequest;

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
        get;
        set
        {
            var previousValue = Interlocked.Exchange(ref field, value);
            if (previousValue != value)
            {
                Window.Invoke(value ? Cursor.Show : Cursor.Hide);
            }
        }
    } = true;

    public int KeyboardDelay { get; } = SystemInformation.KeyboardDelay;
    public int KeyboardSpeed { get; } = SystemInformation.KeyboardSpeed;
}