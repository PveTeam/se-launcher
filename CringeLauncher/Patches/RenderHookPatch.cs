using Windows.Win32.Foundation;
using HarmonyLib;
using SharpDX.DXGI;
using VRage.Platform.Windows.Forms;
using SharpDX.Windows;
using System.Reflection;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public class RenderHookPatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(SwapChain), nameof(SwapChain.Present))]
    private static void PresentPrefix()
    {
        ImGuiHandler.Instance?.DoRender();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MyGameForm), "OnLoad")]
    private static void LoadPostfix(MyGameForm __instance)
    {
        ImGuiHandler.Instance?.HookWindow((HWND)__instance.Handle);
    }

    

    [HarmonyPatch]
    public static class RenderMessagePatches
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            yield return AccessTools.Method(typeof(MyGameForm), nameof(MyGameWindow.WndProc));
            yield return AccessTools.Method(typeof(RenderForm), "WndProc");
        }

        [HarmonyPrefix]
        private static bool WndProcPrefix(MyGameForm __instance, ref Message m)
        {
            if (ImGuiHandler.Instance is not { } handler)
                return true;

            if (m.Msg is >= 256 and <= 265)
                return !handler.Io.WantTextInput;

            if (__instance.ShowCursor && m.Msg is >= 512 and <= 526)
                return !handler.Io.WantCaptureMouse;

            return true;
        }
    }
}