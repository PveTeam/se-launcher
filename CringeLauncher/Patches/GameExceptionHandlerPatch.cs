using HarmonyLib;
using Sandbox;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyInitializer), "UnhandledExceptionHandler")]
public static class GameExceptionHandlerPatch
{
    private static bool Prefix() => false;
}