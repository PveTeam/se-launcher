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
    
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var field = AccessTools.Field(typeof(MyScriptCompiler), nameof(MyScriptCompiler.m_conditionalParseOptions));
        return new CodeMatcher(instructions)
            .Start()
            .MatchStartForward(CodeMatch.IsLdarg(0), CodeMatch.LoadsField(field))
            .SetAndAdvance(OpCodes.Nop, null)
            .SetInstructionAndAdvance(CodeInstruction.LoadField(typeof(ScriptCompilationSettingsPatch), nameof(Options)))
            .Instructions();
    }
}