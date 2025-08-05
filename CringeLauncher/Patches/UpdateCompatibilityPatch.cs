using HarmonyLib;
using Sandbox.Game.World;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptManager), nameof(MyScriptManager.UpdateCompatibility))]
internal static class UpdateCompatibilityPatch
{
    public static bool Prefix(string filename, ref string __result)
    {
        __result = filename;
        return false;
    }
}
