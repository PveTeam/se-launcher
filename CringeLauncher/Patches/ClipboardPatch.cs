#if !WINDOWS
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Sandbox.Graphics.GUI;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class ClipboardPatch
{
    private static MethodBase TargetMethod()
    {
        return AccessTools.DeclaredMethod(
            AccessTools.Inner(typeof(MyGuiControlTextbox), "MyGuiControlTextboxSelection"), "PasteText");
    } 
    
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.LoadsConstant(0),
                CodeMatch.Calls(AccessTools.DeclaredMethod(typeof(Thread), nameof(Thread.SetApartmentState), [typeof(ApartmentState)])),
                CodeMatch.WithOpcodes([OpCodes.Dup]))
            .RemoveInstructions(3)
            .InstructionEnumeration();
    }
}

#endif
