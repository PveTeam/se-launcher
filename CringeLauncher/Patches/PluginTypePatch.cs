using CringePlugins.Loader;
using HarmonyLib;
using Sandbox.Game.World;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using VRage.Game.ObjectBuilder;
using VRage.Plugins;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class PluginTypePatch
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodInfo> TargetMethods()
    {
        yield return AccessTools.Method(typeof(MySession), "RegisterComponentsFromAssemblies");
        yield return AccessTools.Method(typeof(MyGlobalTypeMetadata), nameof(MyGlobalTypeMetadata.Init));
    }

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        //replaces plugin.GetType() with GetPluginType(plugin)
        var pve = AccessTools.Method(typeof(Assembly), nameof(Assembly.Load), [typeof(AssemblyName)]);
        var method = AccessTools.Method(typeof(object), nameof(GetType));
        var getter = AccessTools.PropertyGetter(typeof(MyPlugins), nameof(MyPlugins.Plugins));


        var matcher = new CodeMatcher(instructions);

        if (original.DeclaringType == typeof(MySession))
        {
            matcher = matcher
                .SearchForward(b => b.Calls(pve))
                .Set(OpCodes.Call, AccessTools.Method(typeof(PluginTypePatch), nameof(LoadAssembly)));
        }

        return matcher
            .SearchForward(b => b.Calls(getter))
            .SearchForward(b => b.Calls(method))
            .Set(OpCodes.Call, AccessTools.Method(typeof(PluginTypePatch), nameof(GetPluginType)))
            .InstructionEnumeration();
    }

    //Assembly.Load is called in MySession.RegisterComponentsFromAssemblies. When patching, it uses the wrong context
    //todo: maybe there's a better way to do this?
    private static Assembly LoadAssembly(AssemblyName name) => AssemblyLoadContext.GetLoadContext(typeof(Launcher).Assembly)!.LoadFromAssemblyName(name);
    private static Type GetPluginType(IPlugin plugin) => plugin is PluginWrapper wrapper ? wrapper.InstanceType : plugin.GetType();
}
