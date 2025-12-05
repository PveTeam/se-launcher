using HarmonyLib;
using Sandbox;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MySandboxGame), nameof(MySandboxGame.CheckGraphicsCard))]
internal static class GpuCheckPatch
{
    private static bool Prefix() => false;
}