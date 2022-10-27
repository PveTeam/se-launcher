using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CringeLauncher.Patches;

// eos is disabled
// [HarmonyPatch]
public class EosInitPatch
{
    private static MethodInfo TargetMethod() =>
        AccessTools.Method(Type.GetType("VRage.EOS.MyEOSNetworking, VRage.EOS", true), "Init");

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var ins = instructions.ToList();

        var stIndex = ins.FindIndex(b => b.opcode == OpCodes.Stloc_1);
        
        ins.InsertRange(stIndex, new []
        {
            new CodeInstruction(OpCodes.Dup),
            new(OpCodes.Ldc_I4_2), // PlatformFlags.DisableOverlay
            new(OpCodes.Conv_I8),
            CodeInstruction.Call("Epic.OnlineServices.Platform.Options:set_Flags"), 
        });
        
        return ins;
    }
}