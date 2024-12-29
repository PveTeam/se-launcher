using System.Reflection;
using HarmonyLib;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptWhitelist.MyWhitelistBatch), nameof(MyScriptWhitelist.MyWhitelistBatch.AllowMembers))]
public static class WhitelistAllowPatch
{
    private static void Prefix(ref MemberInfo[] members)
    {
        if (members.Any(b => b is null))
            members = members.Where(b => b is { }).ToArray();
    }

    private static Exception? Finalizer(Exception __exception)
    {
        return __exception is MyWhitelistException ? null : __exception;
    }
}