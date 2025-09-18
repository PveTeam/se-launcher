using HarmonyLib;
using Sandbox.Game.Entities;

namespace CringeLauncher.Patches;


[HarmonyPatch(typeof(MyBlueprintIdTracker), nameof(MyBlueprintIdTracker.OnAdded))]
internal static class BpIdTrackerPatch
{
    [HarmonyPrefix]
    public static bool Prefix() => false;
}