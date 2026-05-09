using CringeLauncher.Stages;
using CringePlugins.Config;
using CringePlugins.Splash;
using NLog;

namespace CringeLauncher.Dedicated;

public class DedicatedLauncher() : Launcher(Environment.GetEnvironmentVariable("DOTNET_USERDEV_RUNDIR"))
{
    protected override bool IsDedicated => true;

    protected override void Initialize(Splash splash)
    {
        base.Initialize(splash);
        
        splash.DefineStage(new DedicatedPlatformInitializationStep());
    }
}
