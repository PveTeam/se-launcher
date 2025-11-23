using System.Reflection.Emit;
using HarmonyLib;
using Sandbox.Engine.Multiplayer;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyDedicatedServerBase), "Initialize")]
internal static class GameServerInitPatch
{
    // TODO figure out why this is required
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.LoadsConstant(100),
                CodeMatch.Calls(AccessTools.DeclaredMethod(typeof(Thread), nameof(Thread.Sleep), [typeof(int)])))
            .SetAndAdvance(OpCodes.Ldc_I4_1, null)
            .InstructionEnumeration();
    }
}