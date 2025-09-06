using HarmonyLib;
using Sandbox;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MySandboxGame), nameof(MySandboxGame.ShowIntroMessages))]
internal static class GameReadyHandlerPatch
{
    private static bool _ready;
    public static event Action? GameReady;
    private static void Prefix()
    {
        if (_ready) return;
        _ready = true;
        GameReady?.Invoke();
    }
}