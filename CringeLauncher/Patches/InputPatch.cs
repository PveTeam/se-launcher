using HarmonyLib;
using VRage;
using VRage.Input;
using VRage.Input.Keyboard;
using VRage.Platform.Windows.Input;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class InputPatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(MyDirectInput), nameof(MyDirectInput.GetMouseState))]
    private static bool GetMouseStatePrefix(ref MyMouseState state)
    {
        if (ImGuiHandler.Instance?.BlockMouse == true && (MyVRage.Platform.Input.ShowCursor || ImGuiHandler.Instance.DrawMouse))
        {
            state = default;
            return false;
        }
        return true;
    }

    [HarmonyPrefix, HarmonyPatch("VRage.Input.Keyboard.MyGuiLocalizedKeyboardState", "GetCurrentState")]
    private static bool GetKeyboardStatePrefix(ref MyKeyboardState __result)
    {
        if (ImGuiHandler.Instance?.BlockKeys == true)
        {
            __result = default;
            return false;
        }
        return true;
    }
}
