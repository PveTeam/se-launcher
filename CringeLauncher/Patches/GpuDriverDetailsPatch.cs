#if !WINDOWS
using HarmonyLib;
using VRage.Platform.Windows.Render;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyPlatformRender), nameof(MyPlatformRender.FillDriverDetails))]
internal static class GpuDriverDetailsPatch
{
    private static bool Prefix()
    {
        return false;
    }
}
#endif
