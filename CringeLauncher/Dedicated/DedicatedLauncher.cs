using System.Runtime.InteropServices;
using CringeLauncher.Render;
using CringeLauncher.Stages;
using CringePlugins.Render;
using CringePlugins.Splash;

namespace CringeLauncher.Dedicated;

public class DedicatedLauncher() : Launcher(Environment.GetEnvironmentVariable("DOTNET_USERDEV_RUNDIR"))
{
    private PosixSignalRegistration? _reg;
    protected override bool IsDedicated => true;

    protected override void Initialize(Splash splash)
    {
        base.Initialize(splash);
        
        splash.DefineStage(new DedicatedPlatformInitializationStep());

        if (!OperatingSystem.IsWindows())
            _reg = PosixSignalRegistration.Create(PosixSignal.SIGTERM, c =>
            {
                c.Cancel = true;
                Stop();
            }); 
    }

    protected override void InitializeEarlyWindow(string[] args)
    {
        RenderHandler.InitializeNoop();
        ImGuiHandler.Instance = new Render.NoopImGuiHandler(ConfigDirectory);
    }
}
