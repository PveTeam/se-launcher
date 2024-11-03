using HarmonyLib;
using Sandbox.Game.World;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyScriptManager), nameof(MyScriptManager.Init))]
public static class DarkTardMissingNamespacePatch
{
    private static void Prefix(Dictionary<string, string> ___m_compatibilityChanges)
    {
        ___m_compatibilityChanges["using System.Runtime.Remoting.Metadata.W3cXsd2001;"] = "";
        ___m_compatibilityChanges["using System.IO.Ports;"] = "";
        ___m_compatibilityChanges["using System.Runtime.Remoting;"] = "";
        ___m_compatibilityChanges["using System.Runtime.Remoting.Messaging;"] = "";
        ___m_compatibilityChanges["using System.Numerics;"] = "";
        ___m_compatibilityChanges["using System.Runtime.Remoting.Lifetime;"] = "";
        ___m_compatibilityChanges["using System.Net.Configuration;"] = "";
        ___m_compatibilityChanges["using System.Reflection.Metadata.Ecma335;"] = "";
    }
}