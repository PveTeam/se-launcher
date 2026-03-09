using System.Reflection.Emit;
using HarmonyLib;
using Sandbox.Game.Gui;
using VRage.FileSystem;

#if !WINDOWS
namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class FsPathsPatch
{
    [HarmonyPatch(typeof(MyGuiScreenOptionsGame), "InitCrosshairIndicators")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CrosshairIndicatorsTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.Calls(AccessTools.DeclaredMethod(typeof(Directory),
                nameof(Directory.EnumerateFiles), [typeof(string)])))
            .Set(OpCodes.Call,
                AccessTools.DeclaredMethod(typeof(MyFileSystem), nameof(MyFileSystem.GetFiles), [typeof(string)]))
            .InstructionEnumeration();
    }
}
#endif
