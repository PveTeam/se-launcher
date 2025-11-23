using CringeLauncher.Render;
using VRage;
using VRageMath;
using MessageBoxOptions = VRage.MessageBoxOptions;

namespace CringeLauncher.Platform;

internal class WindowHelper(VRageWindowSurrogate? surrogate) : IVRageWindows
{
    public void CreateWindow(string gameName, string gameIcon, Type imeCandidateType)
    {
        throw new NotImplementedException();
    }

    public void CreateToolWindow(nint windowHandle)
    {
        throw new NotImplementedException();
    }

    public MessageBoxResult MessageBox(string text, string caption, MessageBoxOptions options)
    {
        throw new NotImplementedException();
    }

    public void ShowSplashScreen(string image, Vector2 scale)
    {
        throw new NotImplementedException();
    }

    public void HideSplashScreen()
    {
        throw new NotImplementedException();
    }

    public nint FindWindowInParent(string parent, string child)
    {
        throw new NotImplementedException();
    }

    public void PostMessage(nint handle, uint wm, nint wParam, nint lParam)
    {
        throw new NotImplementedException();
    }

    public IVRageWindow? Window => surrogate;
}