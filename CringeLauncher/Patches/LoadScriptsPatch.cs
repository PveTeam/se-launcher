using HarmonyLib;
using Sandbox.Game.World;
using System.Reflection;
using System.Reflection.Emit;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptManager), "LoadScripts")]
public static class LoadScriptsPatch
{
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .SearchForward(i => i.opcode == OpCodes.Ldloc_1)
            .SearchForward(i => i.opcode == OpCodes.Ret)
            .Advance(1);

        var call = matcher.Operand as MethodInfo ?? throw new InvalidOperationException("Changes to LoadScripts");

        return matcher.SetAndAdvance(OpCodes.Ldloc_1, null)
            .InsertAndAdvance(CodeInstruction.CallClosure((string[] s) => Array.Sort(s)))
            .Insert(new CodeInstruction(OpCodes.Call, call))
            .InstructionEnumeration();
    }
}
