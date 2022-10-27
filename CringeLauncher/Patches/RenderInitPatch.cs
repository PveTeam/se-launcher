using HarmonyLib;
using SpaceEngineers.Game;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(SpaceEngineersGame), "InitializeRender")]
public static class RenderInitPatch
{
    private static bool Prefix() => false;
}