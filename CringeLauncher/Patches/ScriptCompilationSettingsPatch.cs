using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptCompiler), nameof(MyScriptCompiler.CreateCompilation))]
public static class ScriptCompilationSettingsPatch
{
    private static readonly CSharpParseOptions Options = new(LanguageVersion.Latest, DocumentationMode.None);

    internal static readonly HashSet<MetadataReference> CompilerMetadataReferences = [];

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var parseOptionsField = AccessTools.Field(typeof(MyScriptCompiler), nameof(MyScriptCompiler.m_conditionalParseOptions));
        var metadataReferencesField = AccessTools.Field(typeof(MyScriptCompiler), nameof(MyScriptCompiler.m_metadataReferences));
        return new CodeMatcher(instructions)
            .Start()
            .MatchStartForward(CodeMatch.IsLdarg(0), CodeMatch.LoadsField(parseOptionsField))
            .SetAndAdvance(OpCodes.Nop, null)
            .SetInstructionAndAdvance(
                CodeInstruction.LoadField(typeof(ScriptCompilationSettingsPatch), nameof(Options)))
            .MatchEndForward(CodeMatch.IsLdarg(0), CodeMatch.LoadsField(metadataReferencesField))
            .SetInstructionAndAdvance(CodeInstruction.LoadArgument(1))
            .InsertAndAdvance(CodeInstruction.CallClosure((MyScriptCompiler compiler, string? assemblyName) =>
                assemblyName is null
                    ? compiler.m_metadataReferences.AsEnumerable()
                    : CompilerMetadataReferences))
            .Instructions();
    }
}