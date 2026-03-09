using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Text;
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
        batch.AllowNamespaceOfTypes(MyWhitelistTarget.ModApi, typeof(ParallelQuery));
        batch.AllowTypes(MyWhitelistTarget.Both, typeof(BitArray), typeof(object), typeof(MemoryExtensions));
        batch.AllowNamespaceOfTypes(MyWhitelistTarget.Both, typeof(ImmutableArray), typeof(ASCIIEncoding), typeof(Queue),
            typeof(Queryable), typeof(IQueryable));//used for both, added separately normally

        return false;
    }
}
