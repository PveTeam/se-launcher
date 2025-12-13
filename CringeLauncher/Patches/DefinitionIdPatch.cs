using HarmonyLib;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using VRage.Game;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class DefinitionIdPatch
{
    private static readonly ConcurrentDictionary<MyDefinitionId, string> StringCache = [];

    [HarmonyPostfix, HarmonyPatch(typeof(MyDefinitionId), nameof(MyDefinitionId.DropToStringCache))]
    private static void DropStringPostfix() => StringCache.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    [HarmonyPrefix, HarmonyPatch(typeof(MyDefinitionId), nameof(MyDefinitionId.ToString))]
    private static bool ToStringPrefix(ref MyDefinitionId __instance, ref string __result)
    {
        __result = StringCache.GetOrAdd(__instance, CreateString);
        return false;
    }

    private static string CreateString(MyDefinitionId id) => $"{(string.IsNullOrEmpty(id.TypeId.ToString()) ? "(null)" : id.TypeId.ToString())}/{(string.IsNullOrEmpty(id.SubtypeName) ? "(null)" : id.SubtypeName)}";
}
