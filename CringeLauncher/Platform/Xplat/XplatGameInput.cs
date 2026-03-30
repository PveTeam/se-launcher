#if !WINDOWS
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using CringeLauncher.Render;
using CringeLauncher.Render.Xplat;
using Silk.NET.SDL;
using VRage.Input;
using VRage.Input.Keyboard;
using VRageMath;

namespace CringeLauncher.Platform.Xplat;

[SupportedOSPlatform("linux")]
internal unsafe class XplatGameInput : IVRageInput2
{
    private readonly Lock _lock = new();
    private readonly EarlyWindow _window;
    private bool _disposed;

    private MyMouseState _mouseState;

    private MyKeyboardBuffer _keyboardBuffer;

    private int? _gamepadId;
    private Vector2I _lastMousePos;

    public XplatGameInput(IEarlyWindow windowInstance)
    {
        _window = (EarlyWindow)windowInstance;
        _window.Event += DispatchEvent;
    }

    private void DispatchEvent(IEarlyWindow window, Event e)
    {
        if (e.Key.Type is EventType.KeyDown or EventType.KeyUp && e.Key.WindowID == _window.WindowId)
        {
            using (_lock.EnterScope())
            {
                var value = e.Key.Type == EventType.KeyDown;
                var key = e.Key.Scancode;
                _keyboardBuffer.SetBit((byte)Map(key), value);
                switch (key)
                {
                    case Scancode.ScancodeLalt or Scancode.ScancodeRalt:
                        _keyboardBuffer.SetBit((byte)MyKeys.Alt, value);
                        break;
                    case Scancode.ScancodeLshift or Scancode.ScancodeRshift:
                        _keyboardBuffer.SetBit((byte)MyKeys.Shift, value);
                        break;
                    case Scancode.ScancodeLctrl or Scancode.ScancodeRctrl:
                        _keyboardBuffer.SetBit((byte)MyKeys.Control, value);
                        break;
                }
            }
        }
        else if (e.Wheel.Type == EventType.MouseWheel && e.Wheel.WindowID == _window.WindowId)
        {
            using (_lock.EnterScope())
            {
                _mouseState.ScrollWheelValue = (int)Math.Floor(e.Wheel.Y * 120);
            }
        }
        else if (e.Button.Type is EventType.MouseButtonDown or EventType.MouseButtonUp &&
                 e.Button.WindowID == _window.WindowId)
        {
            using (_lock.EnterScope())
            {
                var press = e.Button.Down != 0;
                switch (e.Button.Button)
                {
                    case Sdl.ButtonLeft:
                        _mouseState.LeftButton = press;
                        break;
                    case Sdl.ButtonRight:
                        _mouseState.RightButton = press;
                        break;
                    case Sdl.ButtonMiddle:
                        _mouseState.MiddleButton = press;
                        break;
                    case Sdl.ButtonX1:
                        _mouseState.XButton1 = press;
                        break;
                    case Sdl.ButtonX2:
                        _mouseState.XButton2 = press;
                        break;
                }
            }
        }
    }

    private MyKeys Map(Scancode key) =>
        key switch
        {
            // Letters A-Z
            Scancode.ScancodeA => MyKeys.A,
            Scancode.ScancodeB => MyKeys.B,
            Scancode.ScancodeC => MyKeys.C,
            Scancode.ScancodeD => MyKeys.D,
            Scancode.ScancodeE => MyKeys.E,
            Scancode.ScancodeF => MyKeys.F,
            Scancode.ScancodeG => MyKeys.G,
            Scancode.ScancodeH => MyKeys.H,
            Scancode.ScancodeI => MyKeys.I,
            Scancode.ScancodeJ => MyKeys.J,
            Scancode.ScancodeK => MyKeys.K,
            Scancode.ScancodeL => MyKeys.L,
            Scancode.ScancodeM => MyKeys.M,
            Scancode.ScancodeN => MyKeys.N,
            Scancode.ScancodeO => MyKeys.O,
            Scancode.ScancodeP => MyKeys.P,
            Scancode.ScancodeQ => MyKeys.Q,
            Scancode.ScancodeR => MyKeys.R,
            Scancode.ScancodeS => MyKeys.S,
            Scancode.ScancodeT => MyKeys.T,
            Scancode.ScancodeU => MyKeys.U,
            Scancode.ScancodeV => MyKeys.V,
            Scancode.ScancodeW => MyKeys.W,
            Scancode.ScancodeX => MyKeys.X,
            Scancode.ScancodeY => MyKeys.Y,
            Scancode.ScancodeZ => MyKeys.Z,

            // Numbers 0-9 (top row)
            Scancode.Scancode1 => MyKeys.D1,
            Scancode.Scancode2 => MyKeys.D2,
            Scancode.Scancode3 => MyKeys.D3,
            Scancode.Scancode4 => MyKeys.D4,
            Scancode.Scancode5 => MyKeys.D5,
            Scancode.Scancode6 => MyKeys.D6,
            Scancode.Scancode7 => MyKeys.D7,
            Scancode.Scancode8 => MyKeys.D8,
            Scancode.Scancode9 => MyKeys.D9,
            Scancode.Scancode0 => MyKeys.D0,

            // Function keys F1-F24
            Scancode.ScancodeF1 => MyKeys.F1,
            Scancode.ScancodeF2 => MyKeys.F2,
            Scancode.ScancodeF3 => MyKeys.F3,
            Scancode.ScancodeF4 => MyKeys.F4,
            Scancode.ScancodeF5 => MyKeys.F5,
            Scancode.ScancodeF6 => MyKeys.F6,
            Scancode.ScancodeF7 => MyKeys.F7,
            Scancode.ScancodeF8 => MyKeys.F8,
            Scancode.ScancodeF9 => MyKeys.F9,
            Scancode.ScancodeF10 => MyKeys.F10,
            Scancode.ScancodeF11 => MyKeys.F11,
            Scancode.ScancodeF12 => MyKeys.F12,
            Scancode.ScancodeF13 => MyKeys.F13,
            Scancode.ScancodeF14 => MyKeys.F14,
            Scancode.ScancodeF15 => MyKeys.F15,
            Scancode.ScancodeF16 => MyKeys.F16,
            Scancode.ScancodeF17 => MyKeys.F17,
            Scancode.ScancodeF18 => MyKeys.F18,
            Scancode.ScancodeF19 => MyKeys.F19,
            Scancode.ScancodeF20 => MyKeys.F20,
            Scancode.ScancodeF21 => MyKeys.F21,
            Scancode.ScancodeF22 => MyKeys.F22,
            Scancode.ScancodeF23 => MyKeys.F23,
            Scancode.ScancodeF24 => MyKeys.F24,

            // Navigation & editing
            Scancode.ScancodeReturn => MyKeys.Enter,
            Scancode.ScancodeEscape => MyKeys.Escape,
            Scancode.ScancodeBackspace => MyKeys.Back,
            Scancode.ScancodeTab => MyKeys.Tab,
            Scancode.ScancodeSpace => MyKeys.Space,
            Scancode.ScancodeCapslock => MyKeys.CapsLock,

            // Arrow keys
            Scancode.ScancodeLeft => MyKeys.Left,
            Scancode.ScancodeRight => MyKeys.Right,
            Scancode.ScancodeUp => MyKeys.Up,
            Scancode.ScancodeDown => MyKeys.Down,

            // Navigation cluster
            Scancode.ScancodeInsert => MyKeys.Insert,
            Scancode.ScancodeHome => MyKeys.Home,
            Scancode.ScancodePageup => MyKeys.PageUp,
            Scancode.ScancodeDelete => MyKeys.Delete,
            Scancode.ScancodeEnd => MyKeys.End,
            Scancode.ScancodePagedown => MyKeys.PageDown,

            // Lock keys
            Scancode.ScancodeNumlockclear => MyKeys.NumLock,
            Scancode.ScancodeScrolllock => MyKeys.ScrollLock,
            Scancode.ScancodePrintscreen => MyKeys.Snapshot,
            Scancode.ScancodePause => MyKeys.Pause,

            // Modifiers - left
            Scancode.ScancodeLctrl => MyKeys.LeftControl,
            Scancode.ScancodeLshift => MyKeys.LeftShift,
            Scancode.ScancodeLalt => MyKeys.LeftAlt,
            Scancode.ScancodeLgui => MyKeys.LeftWindows,

            // Modifiers - right
            Scancode.ScancodeRctrl => MyKeys.RightControl,
            Scancode.ScancodeRshift => MyKeys.RightShift,
            Scancode.ScancodeRalt => MyKeys.RightAlt,
            Scancode.ScancodeRgui => MyKeys.RightWindows,

            // Numpad numbers
            Scancode.ScancodeKp0 => MyKeys.NumPad0,
            Scancode.ScancodeKp1 => MyKeys.NumPad1,
            Scancode.ScancodeKp2 => MyKeys.NumPad2,
            Scancode.ScancodeKp3 => MyKeys.NumPad3,
            Scancode.ScancodeKp4 => MyKeys.NumPad4,
            Scancode.ScancodeKp5 => MyKeys.NumPad5,
            Scancode.ScancodeKp6 => MyKeys.NumPad6,
            Scancode.ScancodeKp7 => MyKeys.NumPad7,
            Scancode.ScancodeKp8 => MyKeys.NumPad8,
            Scancode.ScancodeKp9 => MyKeys.NumPad9,

            // Numpad operators
            Scancode.ScancodeKpDivide => MyKeys.Divide,
            Scancode.ScancodeKpMultiply => MyKeys.Multiply,
            Scancode.ScancodeKpMinus => MyKeys.Subtract,
            Scancode.ScancodeKpPlus => MyKeys.Add,
            Scancode.ScancodeKpEnter => MyKeys.Enter,
            Scancode.ScancodeKpPeriod => MyKeys.Decimal,

            // Punctuation / OEM keys
            Scancode.ScancodeMinus => MyKeys.OemMinus,
            Scancode.ScancodeEquals => MyKeys.OemPlus,
            Scancode.ScancodeLeftbracket => MyKeys.OemOpenBrackets,
            Scancode.ScancodeRightbracket => MyKeys.OemCloseBrackets,
            Scancode.ScancodeBackslash => MyKeys.OemBackslash,
            Scancode.ScancodeSemicolon => MyKeys.OemSemicolon,
            Scancode.ScancodeApostrophe => MyKeys.OemQuotes,
            Scancode.ScancodeGrave => MyKeys.OemTilde,
            Scancode.ScancodeComma => MyKeys.OemComma,
            Scancode.ScancodePeriod => MyKeys.OemPeriod,
            Scancode.ScancodeSlash => MyKeys.OemQuestion,

            // Media keys
            Scancode.ScancodeMute => MyKeys.VolumeMute,
            Scancode.ScancodeVolumeup => MyKeys.VolumeUp,
            Scancode.ScancodeVolumedown => MyKeys.VolumeDown,
            Scancode.ScancodeMediaPlayPause => MyKeys.MediaPlayPause,
            Scancode.ScancodeMediaNextTrack => MyKeys.MediaNextTrack,
            Scancode.ScancodeMediaPreviousTrack => MyKeys.MediaPrevTrack,
            Scancode.ScancodeMediaStop => MyKeys.MediaStop,
            Scancode.ScancodeMediaEject => MyKeys.MediaStop,

            // Browser keys
            Scancode.ScancodeAcBack => MyKeys.BrowserBack,
            Scancode.ScancodeAcForward => MyKeys.BrowserForward,
            Scancode.ScancodeAcRefresh => MyKeys.BrowserRefresh,
            Scancode.ScancodeAcStop => MyKeys.BrowserStop,
            Scancode.ScancodeAcSearch => MyKeys.BrowserSearch,
            Scancode.ScancodeAcHome => MyKeys.BrowserHome,
            Scancode.ScancodeAcBookmarks => MyKeys.BrowserFavorites,

            // Application / Menu
            Scancode.ScancodeApplication => MyKeys.Apps,
            Scancode.ScancodeMenu => MyKeys.Apps,

            // Default fallback
            _ => MyKeys.None
        };

    public void Update()
    {
        if (_disposed) return;
        using var scope = _lock.EnterScope();
        var curPos = new Vector2I(_window.LastMousePosition.X, _window.LastMousePosition.Y);
        var deltaPos = curPos - _lastMousePos;
        _mouseState.X = deltaPos.X;
        _mouseState.Y = deltaPos.Y;
        _lastMousePos = curPos;
    }
    
    public void Dispose()
    {
        _disposed = true;
    }

    public void GetMouseState([UnscopedRef] out MyMouseState state)
    {
        using (_lock.EnterScope())
        {
            state = _mouseState;
            _mouseState.ScrollWheelValue = 0;
        }
    }

    public List<string> EnumerateJoystickNames() => [];

    public string? InitializeJoystickIfPossible(string joystickInstanceName)
    {
        return null;
    }

    public bool IsJoystickAxisSupported(MyJoystickAxesEnum axis)
    {
        return false;
    }

    public bool IsJoystickConnected() => _gamepadId.HasValue;

    public void GetJoystickState(ref MyJoystickState state)
    {
    }

    public void ShowVirtualKeyboardIfNeeded(Action<string> onSuccess, Action? onCancel = null, string? defaultText = null,
        string? title = null, int maxLength = 0)
    {
    }

    public void GetAsyncKeyStates(byte* data)
    {
        using var scope = _lock.EnterScope();

        fixed (MyKeyboardBuffer* dataPtr = &_keyboardBuffer)
        {
            var sourceData = new Span<byte>(dataPtr, sizeof(MyKeyboardBuffer));
            var destData = new Span<byte>(data, sizeof(MyKeyboardBuffer));
            
            sourceData.CopyTo(destData);
        }
    }

    public uint[] DeveloperKeys { get; } =
    [
        2726635697U,
        644003104U,
        3810731010U,
        2191594058U
    ];
    public bool IsCorrectlyInitialized { get; } = true;
}
#endif
