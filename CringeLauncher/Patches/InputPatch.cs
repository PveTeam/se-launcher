using HarmonyLib;
using Sandbox.Graphics;
using VRage;
using VRage.Input;
using VRage.Input.Keyboard;
using VRage.Platform.Windows.Input;
using VRageMath;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class InputPatch
{
    [HarmonyPostfix, HarmonyPatch(typeof(MyDirectInput), nameof(MyDirectInput.GetMouseState))]
    private static void GetMouseStatePostfix(ref MyMouseState state)
    {
        if (ImGuiHandler.Instance?.BlockMouse == true && (MyVRage.Platform.Input.ShowCursor || ImGuiHandler.Instance.DrawMouse))
        {
            state.LeftButton = false;
            state.RightButton = false;
            state.MiddleButton = false;
            state.XButton1 = false;
            state.XButton2 = false;
            state.ScrollWheelValue = 0;
        }
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MyGuiLocalizedKeyboardState), nameof(MyGuiLocalizedKeyboardState.GetCurrentState))]
    private static bool GetKeyboardStatePrefix(ref MyKeyboardState __result)
    {
        if (ImGuiHandler.Instance?.BlockKeys == true)
        {
            __result = default;
            return false;
        }
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MyGuiManager), nameof(MyGuiManager.MouseCursorPosition), MethodType.Getter)]
    private static bool GetMouseCursorPositionPrefix(ref Vector2 __result)
    {
        if (ImGuiHandler.Instance?.BlockMouse == true && (MyVRage.Platform.Input.ShowCursor || ImGuiHandler.Instance.DrawMouse))
        {
            __result = Vector2.Zero;
            return false;
        }
        return true;
    }
}
