using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using HarmonyLib;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class ScriptCompilerInitializationPatch
{
    private static MethodInfo TargetMethod()
    {
        return AccessTools.Method(Type.GetType("VRage.Scripting.MyVRageScriptingInternal, VRage.Scripting", true),
                                  "Initialize");
    }

    private static bool Prefix(Thread updateThread, Type[] referencedTypes, string[] symbols)
    {
        MyModWatchdog.Init(updateThread);
        MyScriptCompiler.Static.AddImplicitInGameNamespacesFromTypes(referencedTypes);
        MyScriptCompiler.Static.AddConditionalCompilationSymbols(symbols);

        using var batch = MyScriptCompiler.Static.Whitelist.OpenBatch();
        batch.AllowTypes(MyWhitelistTarget.ModApi, typeof(ConcurrentQueue<>));
        batch.AllowNamespaceOfTypes(MyWhitelistTarget.Both, typeof(ImmutableArray));

        return false;
    }
}