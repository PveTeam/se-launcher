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
using VRage.Platform.Windows.Serialization;
using VRage.Platform.Windows.Sys;
using VRage.Scripting;
using VRage.Serialization;
using VRage.Utils;

namespace CringeLauncher.Platform;

internal class VRageLauncherPlatform(string applicationName, string? appdataPath, VRageWindowSurrogate surrogate) : IVRagePlatform
{
    private readonly MyWindowsSystem _system = new(applicationName, appdataPath, MyLog.Default);
    private readonly IProtoTypeModel _typeModel = new DynamicTypeModel();
    
    public VRageWindowSurrogate Surrogate => surrogate;

    public void Init()
    {
        _system.Init();
        Render = new PlatformRender(surrogate);
    }

    public void Update()
    {
    }

    public void Done()
    {
    }

    public bool CreateInput2()
    {
        Input2 = new MyDirectInput(new MyWindowsWindows(null)
        {
            WindowHandle = surrogate.Window.Handle
        });
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

    public IAnsel Ansel { get; } =
        (IAnsel)Activator.CreateInstance(Type.GetType("VRage.Ansel.MyAnsel, VRage.Ansel", true)!)!;
    public IAfterMath AfterMath { get; } = new MyAfterMath();
    public IVRageInput Input => surrogate;
    public IVRageInput2? Input2 { get; private set; }
    public IMyAudio? Audio => field ??= new MyXAudio2(new MyPlatformAudio());
    public IMyImeProcessor ImeProcessor => MyImeProcessor.Instance;
    public IMyCrashReporting CrashReporting { get; } = new CrashReportingSurrogate();
    public IVRageScripting Scripting { get; } = MyVRageScripting.Create();
}