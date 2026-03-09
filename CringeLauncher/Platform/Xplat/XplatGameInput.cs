#if !WINDOWS
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using CringeLauncher.Render;
using CringeLauncher.Render.Xplat;
using Silk.NET.GLFW;
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
    private readonly GlfwCallbacks.MouseButtonCallback? _mouseCallback;
    private readonly GlfwCallbacks.ScrollCallback? _scrollCallback;
    private readonly GlfwCallbacks.JoystickCallback? _joystickCallback;
    private readonly GlfwCallbacks.KeyCallback? _keyCallback;

    private MyKeyboardBuffer _keyboardBuffer;

    private int? _gamepadId;
    private Vector2I _lastMousePos;

    public XplatGameInput(IEarlyWindow windowInstance)
    {
        _window = (EarlyWindow)windowInstance;
        var handle = (WindowHandle*)_window.Handle;

        var api = GlfwProvider.GLFW.Value;
        _mouseCallback = api.SetMouseButtonCallback(handle, WindowMouseButtonCallback);
        _scrollCallback = api.SetScrollCallback(handle, WindowScrollCallback);
        _joystickCallback = api.SetJoystickCallback(JoystickCallback);
        _keyCallback = api.SetKeyCallback(handle, WindowKeyCallback);
    }

    private void WindowKeyCallback(WindowHandle* window, Keys key, int scanCode, InputAction action, KeyModifiers mods)
    {
        using (_lock.EnterScope())
        {
            var value = action is InputAction.Press or InputAction.Repeat;
            _keyboardBuffer.SetBit((byte)Map(key), value);
            switch (key)
            {
                case Keys.AltRight or Keys.AltLeft:
                    _keyboardBuffer.SetBit((byte)MyKeys.Alt, value);
                    break;
                case Keys.ShiftRight or Keys.ShiftLeft:
                    _keyboardBuffer.SetBit((byte)MyKeys.Shift, value);
                    break;
                case Keys.ControlRight or Keys.ControlLeft:
                    _keyboardBuffer.SetBit((byte)MyKeys.Control, value);
                    break;
            }
        }
        
        _keyCallback?.Invoke(window, key, scanCode, action, mods);
    }

    private MyKeys Map(Keys key) =>
        key switch
        {
            >= Keys.A and <= Keys.Z or >= Keys.Number0 and <= Keys.Number9 => (MyKeys)key,
            >= Keys.F1 and <= Keys.F24 => MyKeys.F1 + (byte)(key - Keys.F1),
            Keys.Unknown => MyKeys.None,
            Keys.Space => MyKeys.Space,
            Keys.Apostrophe => MyKeys.OemPipe,
            Keys.Comma => MyKeys.OemComma,
            Keys.Minus => MyKeys.OemMinus,
            Keys.Period => MyKeys.OemPeriod,
            Keys.Slash => MyKeys.None,
            Keys.Semicolon => MyKeys.OemSemicolon,
            Keys.Equal => MyKeys.NEC_Equal,
            Keys.LeftBracket => MyKeys.OemOpenBrackets,
            Keys.BackSlash => MyKeys.OemBackslash,
            Keys.GraveAccent => MyKeys.OemTilde,
            Keys.Escape => MyKeys.Escape,
            Keys.Tab => MyKeys.Tab,
            Keys.Backspace => MyKeys.Back,
            Keys.Insert => MyKeys.Insert,
            Keys.Delete => MyKeys.Delete,
            Keys.Right => MyKeys.Right,
            Keys.Left => MyKeys.Left,
            Keys.Down => MyKeys.Down,
            Keys.Up => MyKeys.Up,
            Keys.PageUp => MyKeys.PageUp,
            Keys.PageDown => MyKeys.PageDown,
            Keys.Home => MyKeys.Home,
            Keys.End => MyKeys.End,
            Keys.CapsLock => MyKeys.CapsLock,
            Keys.ScrollLock => MyKeys.ScrollLock,
            Keys.NumLock => MyKeys.NumLock,
            Keys.PrintScreen => MyKeys.Print,
            Keys.Pause => MyKeys.Pause,
            Keys.Keypad0 => MyKeys.NumPad0,
            Keys.Keypad1 => MyKeys.NumPad1,
            Keys.Keypad2 => MyKeys.NumPad2,
            Keys.Keypad3 => MyKeys.NumPad3,
            Keys.Keypad4 => MyKeys.NumPad4,
            Keys.Keypad5 => MyKeys.NumPad5,
            Keys.Keypad6 => MyKeys.NumPad6,
            Keys.Keypad7 => MyKeys.NumPad7,
            Keys.Keypad8 => MyKeys.NumPad8,
            Keys.Keypad9 => MyKeys.NumPad9,
            Keys.KeypadDecimal => MyKeys.Decimal,
            Keys.KeypadDivide => MyKeys.Divide,
            Keys.KeypadMultiply => MyKeys.Multiply,
            Keys.KeypadSubtract => MyKeys.Subtract,
            Keys.KeypadAdd => MyKeys.Add,
            Keys.KeypadEnter => MyKeys.Enter,
            Keys.KeypadEqual => MyKeys.NEC_Equal,
            Keys.ShiftLeft => MyKeys.LeftShift,
            Keys.ControlLeft => MyKeys.LeftControl,
            Keys.AltLeft => MyKeys.LeftAlt,
            Keys.SuperLeft => MyKeys.LeftWindows,
            Keys.ShiftRight => MyKeys.RightShift,
            Keys.ControlRight => MyKeys.RightControl,
            Keys.AltRight => MyKeys.RightAlt,
            Keys.SuperRight => MyKeys.RightWindows,
            _ => MyKeys.None
        };

    private void JoystickCallback(int joystick, ConnectedState state)
    {
        if (!_disposed && GlfwProvider.GLFW.Value.JoystickIsGamepad(joystick))
            _gamepadId = state == ConnectedState.Connected ? joystick : null;
        
        _joystickCallback?.Invoke(joystick, state);
    }

    private void WindowScrollCallback(WindowHandle* window, double offsetX, double offsetY)
    {
        if (!_disposed)
            using (_lock.EnterScope())
            {
                _mouseState.ScrollWheelValue = (int)Math.Floor(offsetY * 120);
            }
        
        _scrollCallback?.Invoke(window, offsetX, offsetY);
    }

    private void WindowMouseButtonCallback(WindowHandle* window, MouseButton button, InputAction action, KeyModifiers mods)
    {
        if (!_disposed)
            using (_lock.EnterScope())
                switch (button)
                {
                    case MouseButton.Left:
                        _mouseState.LeftButton = action == InputAction.Press;
                        break;
                    case MouseButton.Right:
                        _mouseState.RightButton = action == InputAction.Press;
                        break;
                    case MouseButton.Middle:
                        _mouseState.MiddleButton = action == InputAction.Press;
                        break;
                    case MouseButton.Button4:
                        _mouseState.XButton1 = action == InputAction.Press;
                        break;
                    case MouseButton.Button5:
                        _mouseState.XButton2 = action == InputAction.Press;
                        break;
                }
        
        _mouseCallback?.Invoke(window, button, action, mods);
    }

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
