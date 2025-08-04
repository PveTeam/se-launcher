using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CringeLauncher.Patches;

[HarmonyPatch(TypeName, "WriteStartAttribute")]
[HarmonyPatch([typeof(string), typeof(string), typeof(string)])]
public static class XmlRootWriterPatch
{
    private const string TypeName = "System.Xml.XmlWellFormedWriter, System.Private.Xml";
    
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var method = AccessTools.DeclaredMethod(original.DeclaringType, "CheckNCName");
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.LoadsArgument(), CodeMatch.Calls(method))
            .SetAndAdvance(OpCodes.Nop, null)
            .SetAndAdvance(OpCodes.Nop, null)
            .InstructionEnumeration();
    }
}