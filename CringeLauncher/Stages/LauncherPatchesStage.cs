using CringePlugins.Splash;
using HarmonyLib;

namespace CringeLauncher.Stages;

public class LauncherPatchesStage : ILoadingStage
{
    public string Name { get; } = "Launcher Patches";
    public ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(1);
        progress.Report("Applying launcher patches");
        
        try
        {
            new Harmony("CringeBootstrap").PatchAll(typeof(Launcher).Assembly);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return default;
    }
}