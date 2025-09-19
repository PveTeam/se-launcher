using System.Reflection;
using HarmonyLib;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptWhitelist.MyWhitelistBatch), nameof(MyScriptWhitelist.MyWhitelistBatch.AllowMembers))]
public static class WhitelistAllowPatch
{
    private static void Prefix(ref MemberInfo?[] members)
    {
        members =
        [
            .. members.Where(b => b?.DeclaringType is { IsPublic: true } or { IsNestedPublic: true })
        ];
    }

    private static Exception? Finalizer(Exception __exception)
    {
        return __exception is MyWhitelistException ? null : __exception;
    }
}