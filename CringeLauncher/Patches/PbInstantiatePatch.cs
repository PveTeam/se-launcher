using HarmonyLib;
using Sandbox.Game.Entities.Blocks;
using System.Reflection.Emit;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyProgrammableBlock), "UpdateBeforeSimulation")]
internal static class PbInstantiatePatch
{
    [HarmonyTranspiler]
    public static List<CodeInstruction> UpdateBeforeSimulationTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = instructions.ToList();

        var index = list.FindIndex(b => b.opcode == OpCodes.Brfalse);

        //get second brfase
        index = list.FindIndex(index, b => b.opcode == OpCodes.Brfalse);

        var jump = new CodeInstruction(list[index])
        {
            opcode = OpCodes.Brtrue
        };

        index++;

        list.Insert(index++, new CodeInstruction(OpCodes.Ldarg_0));
        list.Insert(index++, CodeInstruction.CallClosure(PveMethod));
        list.Insert(index, jump);

        return list;
    }

    //IMPORTANT: if you reference CompilingPbs in the transpiler, ModScriptCompilerPatch will cause static init of MyScriptWhitelist, crashing the game
    private static bool PveMethod(MyProgrammableBlock block) => ModScriptCompilerPatch.CompilingPbs.Contains(block);
}
