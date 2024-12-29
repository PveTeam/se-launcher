using System.Diagnostics;
using System.Reflection.Emit;
using HarmonyLib;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptWhitelist.Batch), nameof(MyScriptWhitelist.Batch.ResolveTypeSymbol))]
public static class WhitelistTypeResolutionPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var call = CodeInstruction.CallClosure((MyWhitelistException ex) =>
        {
            Debug.WriteLine(ex);
        });

        return instructions.Manipulator(i => i.opcode == OpCodes.Throw,
            i =>
            {
                i.opcode = call.opcode;
                i.operand = call.operand;
            });
    }
}