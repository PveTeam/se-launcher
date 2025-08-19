using System.Reflection.Emit;
using Windows.Win32.Foundation;
using CringeLauncher.Platform;
using HarmonyLib;
using SharpDX.DXGI;
using VRage;
using VRage.Platform.Windows.Forms;
using VRageRender;
using VRageRender.ExternalApp;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class RenderHookPatch
{
    [HarmonyPostfix, HarmonyPatch(typeof(MyGameForm), "OnLoad")]
    private static void LoadPostfix(MyGameForm __instance)
    {
        ImGuiHandler.HookWindow((HWND)__instance.Handle);
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(MyRender11), nameof(MyRender11.ProcessStateChanges))]
    private static IEnumerable<CodeInstruction> ProcessStateChangesTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var renderThreadGetter = AccessTools.DeclaredPropertyGetter(typeof(MyRenderProxy), nameof(MyRenderProxy.RenderThread));
        var threadGetter = AccessTools.DeclaredPropertyGetter(typeof(Thread), nameof(Thread.CurrentThread));
        var stateThreadField = AccessTools.Field(typeof(MyRenderThread), nameof(MyRenderThread.ProcessStateChangesThread));
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.Calls(renderThreadGetter), CodeMatch.Calls(threadGetter),
                CodeMatch.StoresField(stateThreadField))
            .RemoveInstructions(3)
            .MatchStartForward(CodeMatch.Calls(renderThreadGetter), CodeMatch.WithOpcodes([OpCodes.Ldnull]),
                CodeMatch.StoresField(stateThreadField))
            .RemoveInstructions(3)
            .InstructionEnumeration();
    }

    [HarmonyTranspiler, HarmonyPatch(typeof(MyRender11), nameof(MyRender11.ProcessMessageInternal))]
    private static IEnumerable<CodeInstruction> ProcessMessageTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.Calls(AccessTools.DeclaredMethod(typeof(MyRenderThread), nameof(MyRenderThread.SwitchSettings))))
            .Set(OpCodes.Call, AccessTools.DeclaredMethod(typeof(RenderHookPatch), nameof(SwitchSettings)))
            .InstructionEnumeration();
    }

    private static void SwitchSettings(object? renderThread, MyRenderDeviceSettings settings)
    {
        ((VRageLauncherPlatform)MyVRage.Platform).Surrogate.ApplyRenderSettings(settings);
    }
}