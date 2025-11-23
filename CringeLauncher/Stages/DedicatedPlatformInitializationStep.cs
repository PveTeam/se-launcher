using CringePlugins.Splash;
using Sandbox;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using VRage.Game;

namespace CringeLauncher.Stages;

public class DedicatedPlatformInitializationStep : ILoadingStage
{
    public string Name => "Dedicated Platform Initialization";

    public ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(1);
        progress.Report("Initializing dedicated platform");
        
        MySandboxGame.ConfigDedicated = new MyConfigDedicated<MyObjectBuilder_SessionSettings>(
            $"{MyPerGameSettings.GameNameSafe}-Dedicated.cfg");
        MySandboxGame.ConfigDedicated.Load();
        if (!File.Exists(MySandboxGame.ConfigDedicated.GetFilePath()))
            MySandboxGame.ConfigDedicated.Save();
        
        return ValueTask.CompletedTask;
    }
}