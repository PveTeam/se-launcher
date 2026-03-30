using System.Reflection.Emit;
using CringeLauncher.Platform.Xplat;
using HarmonyLib;
using Sandbox.Game.Gui;
using VRage.FileSystem;
using VRage.Render11.Resources;

#if !WINDOWS
namespace CringeLauncher.Patches;

[HarmonyPatch]
[HarmonyPatchCategory("EarlyRender")]
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

    [HarmonyPatch(typeof(MyTextureAtlas), nameof(MyTextureAtlas.ParseAtlasDescription))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ParseAtlasDescriptionTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.Calls(AccessTools.DeclaredMethod(typeof(Path), nameof(Path.GetFileName), [typeof(string)])))
            .SetInstruction(CodeInstruction.Call(typeof(LauncherFileProvider), nameof(LauncherFileProvider.GetFileName)))
            .InstructionEnumeration();
    }
}
#endif
