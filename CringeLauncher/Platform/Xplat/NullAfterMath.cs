using VRage;

namespace CringeLauncher.Platform.Xplat;

internal class NullAfterMath : IAfterMath
{
    public int Init(IntPtr device) => 1;

    public void Shutdown()
    {
    }

    public string GetInfo(IntPtr context)
    {
        return "";
    }

    public void SetEventMarker(IntPtr nativePointer, string tag)
    {
    }
}
