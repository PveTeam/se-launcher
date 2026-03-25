using System.Runtime.InteropServices;
using CringeLauncher.Platform.Xplat;
using CringeLauncher.Render;
using VRage;
using VRage.Analytics;
using VRage.Audio;
using VRage.Http;
using VRage.Input;
using VRage.Platform.Windows;
using VRage.Platform.Windows.Audio;
using VRage.Platform.Windows.DShow;
using VRage.Platform.Windows.Forms;
using VRage.Platform.Windows.Http;
using VRage.Platform.Windows.IME;
using VRage.Platform.Windows.Input;
using VRage.Platform.Windows.Render;
using VRage.Platform.Windows.Serialization;
using VRage.Scripting;
using VRage.Serialization;
using VRage.Utils;

namespace CringeLauncher.Platform;

internal class VRageLauncherPlatform(string applicationName, string? appdataPath, VRageWindowSurrogate? surrogate) : IVRagePlatform
{
    private readonly VRageSystem _system = new(applicationName, surrogate, appdataPath);
    private readonly IProtoTypeModel _typeModel = new DynamicTypeModel();
    
    public VRageWindowSurrogate? Surrogate => surrogate;

    public void Init()
    {
        if (surrogate is not null)
            Render = new PlatformRender(surrogate);
        else Render = new MyWindowsRender(MyLog.Default, null);
    }

    public void Update()
    {
    }

    public void Done()
    {
    }

    public bool CreateInput2()
    {
#if WINDOWS
        Input2 = new MyDirectInput(new MyWindowsWindows(null)
        {
            WindowHandle = surrogate?.Window.Handle ?? 0
        });
#else
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            throw new PlatformNotSupportedException("Platforms other than linux are not supported");
        var input = new XplatGameInput(surrogate!.Window);
        Input2 = input;
        
        surrogate?.AddFrameCallback(input.Update);
#endif
        return Input2.IsCorrectlyInitialized;
    }

    public IVideoPlayer CreateVideoPlayer() => new MyVideoPlayer();

    public IMyAnalytics? InitAnalytics(string projectId, string version)
    {
        return null;
    }

    public IMyAnalytics? InitAnalytics(string projectId, string version, bool idSynced)
    {
        return null;
    }

    public IProtoTypeModel GetTypeModel() => _typeModel;

    public bool SessionReady { get; set; }
    public IVRageWindows Windows { get; } = new WindowHelper(surrogate);
    public IVRageHttp Http { get; } = new MyWindowsHttpClient(null);

    public IVRageSystem System => _system;

    public IVRageRender? Render { get; private set; }

    public IAnsel? Ansel { get; } =
#if WINDOWS
        (IAnsel)Activator.CreateInstance(Type.GetType("VRage.Ansel.MyAnsel, VRage.Ansel", true)!)!;
#else
        new NullAnsel();
#endif
    public IAfterMath AfterMath { get; } =
#if WINDOWS
        new MyAfterMath();
#else
        new NullAfterMath();
#endif
    public IVRageInput? Input => surrogate;
    public IVRageInput2? Input2 { get; private set; }

    public IMyAudio? Audio => field ??=
#if WINDOWS
        new MyXAudio2(new MyPlatformAudio());
#else
        new MyXAudio2(new MyPlatformAudio());
#endif
    public IMyImeProcessor ImeProcessor { get; } =
#if WINDOWS
        MyImeProcessor.Instance;
#else
        new NullImeProcessor();
#endif
    public IMyCrashReporting CrashReporting { get; } = new CrashReportingSurrogate();
    public IVRageScripting Scripting { get; } = MyVRageScripting.Create();
}
