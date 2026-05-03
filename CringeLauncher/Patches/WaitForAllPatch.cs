#if !WINDOWS
using System.Reflection.Emit;
using HarmonyLib;
using Parallel = ParallelTasks.Parallel;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(Parallel), nameof(Parallel.WaitForAll))]
internal static class WaitForAllPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.Calls(AccessTools.DeclaredPropertyGetter(typeof(Thread), nameof(Thread.CurrentThread))))
            .SetAndAdvance(OpCodes.Nop, null)
            .SetAndAdvance(OpCodes.Ldc_I4_1, null)
            .InstructionEnumeration();
    }
}
#endif
