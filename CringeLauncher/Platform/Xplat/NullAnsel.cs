using System.Diagnostics.CodeAnalysis;
using VRage;

namespace CringeLauncher.Platform.Xplat;

internal class NullAnsel : IAnsel
{
    public int Init(bool settingsEnableAnselWithSprites) => 0;

    public void SetCamera(ref MyCameraSetup cameraSetup)
    {
    }

    public void GetCamera([UnscopedRef] out MyCameraSetup cameraSetup)
    {
        cameraSetup = default;
    }

    public void Enable()
    {
    }

    public void StopSession()
    {
    }

    public void MarkHdrBufferBind()
    {
    }

    public void MarkHdrBufferFinished()
    {
    }

    public bool IsSessionEnabled { get; set; }
    public bool IsGamePausable { get; set; }
    public bool IsCaptureRunning => false;
    public bool IsSessionRunning => false;
    public bool Is360Capturing => false;
    public bool IsMultiresCapturing => false;
    public bool IsOverlayEnabled => false;
    public bool IsInitializedSuccessfuly => false;
    public event Action<int>? StartCaptureDelegate;
    public event Action? StopCaptureDelegate;
    public event Action<bool, bool>? WarningMessageDelegate;
    public event Func<bool>? IsSpectatorEnabledDelegate;
}
