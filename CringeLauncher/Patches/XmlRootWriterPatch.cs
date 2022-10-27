using System.Reflection.Emit;
using System.Xml;
using HarmonyLib;
using VRage;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(CustomRootWriter), "Init")]
public class XmlRootWriterPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var ins = instructions.ToList();
        
        var index = ins.FindIndex(b =>
                                      b.opcode == OpCodes.Ldstr && b.operand is "xsi:type");
        ins[index].operand = "xsi";
        
        ins.InsertRange(index + 1, new[]
        {
            new CodeInstruction(OpCodes.Ldstr, "type"),
            new CodeInstruction(OpCodes.Ldstr, "http://www.w3.org/2001/XMLSchema-instance")
        });
        
        var instruction = ins[ins.FindIndex(b => b.opcode == OpCodes.Callvirt)];
        instruction.operand = AccessTools.Method(typeof(XmlWriter), "WriteAttributeString", new[]
        {
            typeof(string),
            typeof(string),
            typeof(string),
            typeof(string)
        });
        
        return ins;
    }
}