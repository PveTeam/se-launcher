using System.Reflection;
using System.Reflection.Emit;
using CringeLauncher.Platform.Xplat;
using HarmonyLib;
using Sandbox.Definitions;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using VRage.FileSystem;
using VRage.Game;
using VRage.Private;
using VRage.Render11.Resources;

#if !WINDOWS
namespace CringeLauncher.Patches;

[HarmonyPatch]
[HarmonyPatchCategory("EarlyRender")]
internal static class FsPathsPatch
{
    private static void Prepare(MethodBase? original)
    {
        if (original is not null) return;

        MyKeenUtils.FixedInvalidFileNameChars = Path.GetInvalidFileNameChars();
        MyKeenUtils.FixedInvalidPathChars = Path.GetInvalidPathChars();
    }
    
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

    [HarmonyPatch(typeof(MyDefinitionManager), "ProcessContentFilePath")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> ProcessContentFilePathTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        /*var getFileName =
            AccessTools.DeclaredMethod(typeof(LauncherFileProvider), nameof(LauncherFileProvider.GetFileName));
        var getDirectoryName =
            AccessTools.DeclaredMethod(typeof(LauncherFileProvider), nameof(LauncherFileProvider.GetDirectoryName));
        return instructions
            .MethodReplacer(AccessTools.DeclaredMethod(typeof(Path), nameof(Path.GetFileName), [typeof(string)]),
                getFileName)
            .MethodReplacer(AccessTools.DeclaredMethod(typeof(Path), nameof(Path.GetDirectoryName), [typeof(string)]),
                getDirectoryName);*/

        return new CodeMatcher(instructions)
            .MatchEndForward(
                CodeMatch.Calls(AccessTools.DeclaredMethod(typeof(Path), nameof(Path.Combine),
                    [typeof(string), typeof(string)])), CodeMatch.StoresLocal())
            .InsertAfter(
                CodeInstruction.LoadField(typeof(LauncherFileProvider), nameof(LauncherFileProvider.Instance)),
                new(OpCodes.Ldloca_S, 1),
                CodeInstruction.Call(typeof(LauncherFileProvider), nameof(LauncherFileProvider.NormalizePath))
            )
            .InstructionEnumeration();
    }

    [HarmonyPatch(typeof(MyScriptManager), "LoadScripts")]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> LoadScriptsTranspiler(
        IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchEndForward(
                CodeMatch.Calls(AccessTools.DeclaredMethod(typeof(Path), nameof(Path.Combine),
                    [typeof(string), typeof(string), typeof(string)])), CodeMatch.IsStloc())
            .InsertAfter(
                new CodeInstruction(OpCodes.Ldloc_0),
                new(OpCodes.Ldc_I4_S, (int)Path.DirectorySeparatorChar),
                new(OpCodes.Ldc_I4_S, (int)'\\'),
                CodeInstruction.Call(typeof(string), nameof(string.Replace), [typeof(char), typeof(char)]),
                new(OpCodes.Stloc_0)
            )
            .MatchEndForward(CodeMatch.Calls(AccessTools.DeclaredMethod(typeof(Enumerable),
                nameof(Enumerable.ToArray), [typeof(IEnumerable<>).MakeGenericType(Type.MakeGenericMethodParameter(0))],
                [typeof(string)])), CodeMatch.IsStloc())
            .InsertAfter(new CodeInstruction(OpCodes.Ldloc_1),
                CodeInstruction.CallClosure((string[] files) =>
                {
                    for (var i = 0; i < files.Length; i++)
                    {
                        files[i] = files[i].Replace(Path.DirectorySeparatorChar, '\\');
                    }
                }))
            .InstructionEnumeration();
    }

    [HarmonyPatch(typeof(MyZipFileProvider), nameof(MyZipFileProvider.IsZipFile))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> IsZipFileTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return
        [
            new(OpCodes.Ldsfld, AccessTools.DeclaredField(typeof(LauncherFileProvider), nameof(LauncherFileProvider.Instance))),
            new(OpCodes.Ldarg_0),
            CodeInstruction.Call(typeof(LauncherFileProvider), nameof(LauncherFileProvider.DirectoryExists)),
            new(OpCodes.Ldc_I4_0),
            new(OpCodes.Ceq),
            new(OpCodes.Ret)
        ];
    }

    [HarmonyPatch(typeof(MyDefinitionManager), nameof(MyDefinitionManager.LoadData))]
    [HarmonyPrefix]
    private static void LoadModsPrefix(List<MyObjectBuilder_Checkpoint.ModItem> mods)
    {
        LauncherFileProvider.Instance.CacheMods(mods.Select(b => b.GetModContext().ModPath));
    }
}
#endif
