using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using VRage.Scripting;
using CringeLauncher.SyntaxRewriters;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptCompiler), "InjectMod")]
internal static class ModRewriterPatch
{
    public static void Prefix(ref CSharpCompilation compilation, ref SyntaxTree syntaxTree)
    {
        var fixedSyntaxTree = MissingUsingRewriter.Rewrite(compilation, syntaxTree);
        compilation = compilation.ReplaceSyntaxTree(syntaxTree, fixedSyntaxTree);
        syntaxTree = fixedSyntaxTree;
    }
}
