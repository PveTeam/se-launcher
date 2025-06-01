using HarmonyLib;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using CringeLauncher.SyntaxRewriters;
using VRage.Scripting.Rewriters;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(ProtoTagRewriter), "Rewrite")]
internal static class ModRewriterPatch
{
    public static bool Prefix(CSharpCompilation compilation, SyntaxTree syntaxTree, ref SyntaxTree __result)
    {
        __result = MissingUsingRewriter.Rewrite(compilation, syntaxTree);

        return false;
    }
}
