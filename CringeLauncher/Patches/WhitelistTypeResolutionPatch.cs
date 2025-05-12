using HarmonyLib;
using Microsoft.CodeAnalysis;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptWhitelist.Batch), nameof(MyScriptWhitelist.Batch.ResolveTypeSymbol))]
public static class WhitelistTypeResolutionPatch
{
    [HarmonyReversePatch]
    private static INamedTypeSymbol ResolveTypeSymbol(MyScriptWhitelist.Batch batch, Type type) => throw null!;
    
    // cant be assed to write a transpiler so heres a prefix
    private static bool Prefix(MyScriptWhitelist.Batch __instance, Type type, ref INamedTypeSymbol __result)
    {
        // fast path
        if (!type.IsGenericType || !type.IsConstructedGenericType)
            return true;

        __result = ResolveGenericTypeSymbol(__instance, type);
        return false;
    }

    private static INamedTypeSymbol ResolveGenericTypeSymbol(MyScriptWhitelist.Batch batch, Type type)
    {
        // if type is not generic or constructed generic, run regular lookup
        if (!type.IsGenericType || !type.IsConstructedGenericType)
            return ResolveTypeSymbol(batch, type);
        
        var unconstructedSymbol = ResolveTypeSymbol(batch, type.GetGenericTypeDefinition());
        
        var typeArguments = type.GetGenericArguments();
        
        var typeSymbolArguments = new ITypeSymbol[typeArguments.Length];
        for (var i = 0; i < typeArguments.Length; i++)
        {
            // recursively resolve (possibly) generic arguments
            typeSymbolArguments[i] = ResolveGenericTypeSymbol(batch, typeArguments[i]);
        }
        
        return unconstructedSymbol.Construct(typeSymbolArguments);
    }
}