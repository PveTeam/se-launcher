using System.Reflection.Emit;
using HarmonyLib;
using VRage.Game.GUI;
using VRageRender;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyGuiTextures), nameof(MyGuiTextures.Reload))]
internal static class RenderTexturesPreloadPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var renderThreadGetter = AccessTools.PropertyGetter(typeof(MyRenderProxy), nameof(MyRenderProxy.RenderThread));
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.Calls(renderThreadGetter))
            .Set(OpCodes.Ldc_I4_1, null)
            .InstructionEnumeration();
    }
}