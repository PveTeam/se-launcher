using VRage;

namespace CringeLauncher.Platform.Xplat;

internal class NullImeProcessor : IMyImeProcessor
{
    public void Activate(IMyImeActiveControl textElement)
    {
    }

    public void Deactivate()
    {
    }

    public void RecaptureTopScreen(IVRageGuiScreen screenWithFocus)
    {
    }

    public void RegisterActiveScreen(IVRageGuiScreen screen)
    {
    }

    public void UnregisterActiveScreen(IVRageGuiScreen screen)
    {
    }

    public void ProcessInvoke()
    {
    }

    public void CaretRepositionReaction()
    {
    }

    public bool IsComposing { get; }
}
