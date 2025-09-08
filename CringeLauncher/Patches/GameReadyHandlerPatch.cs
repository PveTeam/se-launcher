using HarmonyLib;
using Sandbox;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class GameReadyHandlerPatch
{
    private static bool _ready;
    public static event Action? GameReady;
    public static event Action? GameReadyTransitionStarted;
    
    [HarmonyPrefix, HarmonyPatch(typeof(MyGuiScreenBase), "UpdateTransition")]
    private static void TransitionPrefix(MyGuiScreenBase __instance)
    {
        if (_ready || __instance is not MyGuiScreenMainMenu { State: MyGuiScreenState.OPENED }) return;
        _ready = true;
        GameReady?.Invoke();
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MySandboxGame), nameof(MySandboxGame.ShowIntroMessages))]
    private static void ShowMainMenuPrefix()
    {
        if (_ready) return;
        GameReadyTransitionStarted?.Invoke();
    }
}