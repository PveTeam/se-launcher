using Windows.Win32.Foundation;
using HarmonyLib;
using SharpDX.DXGI;
using VRage.Platform.Windows.Forms;
using SharpDX.Windows;
using System.Reflection;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class RenderHookPatch
{
    [HarmonyPrefix, HarmonyPatch(typeof(SwapChain), nameof(SwapChain.Present))]
    private static void PresentPrefix()
    {
        ImGuiHandler.Instance?.DoRender();
    }

    [HarmonyPostfix, HarmonyPatch(typeof(MyGameForm), "OnLoad")]
    private static void LoadPostfix(MyGameForm __instance)
    {
        ImGuiHandler.HookWindow((HWND)__instance.Handle);
    }
}