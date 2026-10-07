#if !WINDOWS
using System.Reflection;
using System.Reflection.Emit;
using CringeLauncher.Platform;
using HarmonyLib;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using VRage;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class ClipboardPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.DeclaredMethod(
            AccessTools.Inner(typeof(MyGuiControlTextbox), "MyGuiControlTextboxSelection"), "PasteText");
        yield return AccessTools.DeclaredMethod(typeof(MyClipboardService), nameof(MyClipboardService.GetText));
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

[HarmonyPatch]
internal static class ClipboardPastePatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.DeclaredMethod(
            AccessTools.Inner(typeof(MyGuiControlTextbox), "MyGuiControlTextboxSelection"), "PasteFromClipboard");
        yield return AccessTools.DeclaredMethod(typeof(MyClipboardService), "PasteFromClipboard");
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.DeclaredPropertyGetter(typeof(IVRageSystem), nameof(IVRageSystem.Clipboard));
        return instructions.MethodReplacer(method,
            AccessTools.DeclaredMethod(typeof(ClipboardPastePatch), nameof(GetClipboardText)));
    }

    private static string GetClipboardText(IVRageSystem vrSystem)
    {
        var system = (VRageSystem)vrSystem;

        if (system.Surrogate is null) return string.Empty;

        var text = new Holder();
        
        // blocking
        system.Surrogate.Window.Invoke(() => text.Value = system.Clipboard);
        
        return text.Value;
    }
    
    private class Holder
    {
        public string Value = string.Empty;
    }
}

#endif
