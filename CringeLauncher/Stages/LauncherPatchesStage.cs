using CringePlugins.Splash;
using HarmonyLib;

namespace CringeLauncher.Stages;

public class LauncherPatchesStage(bool isDedicated) : ILoadingStage
{
    public string Name { get; } = "Launcher Patches";
    public ValueTask Load(ISplashProgress progress)
    {
        progress.DefineStepsCount(1);
        progress.Report("Applying launcher patches");
        
        try
        {
            var harmony = new Harmony("CringeBootstrap");
            harmony.PatchAllUncategorized(typeof(Launcher).Assembly);
            if (isDedicated)
                harmony.PatchCategory(typeof(Launcher).Assembly, "Dedicated");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return default;
    }
}
