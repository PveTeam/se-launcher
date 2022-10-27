using System.Reflection;
using HarmonyLib;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class WhitelistAllowPatch
{
    private static MethodInfo TargetMethod()
    {
        return AccessTools.Method(AccessTools.Inner(typeof(MyScriptWhitelist), "MyWhitelistBatch"), "AllowMembers");
    }
    
    private static void Prefix(ref MemberInfo[] members)
    {
        if (members.Any(b => b is null))
            members = members.Where(b => b is { }).ToArray();
    }
}