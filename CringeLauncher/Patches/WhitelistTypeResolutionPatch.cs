using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptWhitelist.Batch), nameof(MyScriptWhitelist.Batch.ResolveTypeSymbol))]
public static class WhitelistTypeResolutionPatch
{
    private static readonly ConditionalWeakTable<MyScriptWhitelist.Batch, CSharpCompilation> CompilationTable = new();
    
    private static INamedTypeSymbol ResolveTypeSymbol(MyScriptWhitelist.Batch batch, Type type)
    {
        var name = GetCompilation(batch).GetTypeByMetadataName(type.FullName!);
        return name ?? throw new MyWhitelistException(
            $"Cannot add {type.FullName}, {type.Assembly.FullName} to the batch because its symbol variant could not be found.");
    }

    private static CSharpCompilation GetCompilation(MyScriptWhitelist.Batch batch) =>
        CompilationTable.GetValue(batch, static b => b.Whitelist.CreateCompilation());

    // cant be assed to write a transpiler so heres a prefix
    private static bool Prefix(MyScriptWhitelist.Batch __instance, Type type, ref INamedTypeSymbol __result)
    {
        if (type.IsPublic || type.IsNestedPublic)
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

[HarmonyPatch(typeof(MyScriptWhitelist.Batch), MethodType.Constructor, typeof(MyScriptWhitelist))]
public static class WhitelistCompilationPatch
{
    private static bool Prefix(MyScriptWhitelist.Batch __instance, MyScriptWhitelist whitelist)
    {
        __instance.Whitelist = whitelist;
        return false;
    }
}