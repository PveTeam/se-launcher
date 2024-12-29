using CringePlugins.Loader;
using HarmonyLib;
using Sandbox.Game.World;
using System.Reflection;
using System.Reflection.Emit;
using VRage.Game;
using VRage.Game.ObjectBuilder;
using VRage.Plugins;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class PluginTypePatch
{
    [HarmonyPatch(typeof(MyGlobalTypeMetadata), nameof(MyGlobalTypeMetadata.Init))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MetadataTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        return new CodeMatcher(instructions, generator)
            .SearchForward(b => b.opcode == OpCodes.Ldloc_2)
            .Advance(-1)
            .CreateLabel(out var regularPluginLabel)
            .DeclareLocal(typeof(PluginWrapper), out var wrapper)
            .DefineLabel(out var continueLabel)
            .Insert(new(OpCodes.Ldloc_2), 
                new(OpCodes.Isinst, typeof(PluginWrapper)), new(OpCodes.Stloc, wrapper),
                new(OpCodes.Ldloc_S, wrapper), 
                new(OpCodes.Brfalse_S, regularPluginLabel), 
                new(OpCodes.Ldloc_0),
                new(OpCodes.Ldloc_S, wrapper),
                new(OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(PluginWrapper), nameof(PluginWrapper.InstanceType))),
                new(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(Type), nameof(Type.Assembly))),
                CodeInstruction.Call(typeof(MyGlobalTypeMetadata), nameof(MyGlobalTypeMetadata.RegisterAssembly),
                    [typeof(Assembly)]), 
                new(OpCodes.Br_S, continueLabel))
            .SearchForward(b => b.opcode == OpCodes.Ldloca_S)
            .AddLabels([continueLabel])
            .InstructionEnumeration();
    }

    [HarmonyPatch(typeof(MySession), "RegisterComponentsFromAssemblies")]
    [HarmonyPrefix]
    private static bool RegisterComponentsPrefix(MySession __instance)
    {
        __instance.m_componentsToLoad = [..__instance.GameDefinition.SessionComponents.Keys];
        __instance.m_componentsToLoad.ExceptWith(__instance.SessionComponentDisabled);
        __instance.m_componentsToLoad.UnionWith(__instance.SessionComponentEnabled);
        
        __instance.RegisterComponentsFromAssembly(MyPlugins.SandboxAssembly);
        __instance.RegisterComponentsFromAssembly(MyPlugins.SandboxGameAssembly);
        __instance.RegisterComponentsFromAssembly(MyPlugins.GameAssembly);
        
        foreach (var (context, ids) in __instance.ScriptManager.ScriptsPerMod)
        {
            foreach (var id in ids)
            {
                __instance.RegisterComponentsFromAssembly(__instance.ScriptManager.Scripts[id], true, context);
            }
        }
        
        foreach (var plugin in MyPlugins.Plugins)
        {
            var type = plugin is PluginWrapper wrapper ? wrapper.InstanceType : plugin.GetType();
            
            __instance.RegisterComponentsFromAssembly(type.Assembly, true);
        }
        
        foreach (var component in __instance.m_sessionComponents.Values)
        {
            if (component.ModContext is null or { IsBaseGame: true })
                __instance.m_sessionComponentForDrawAsync.Add(component);
            else
                __instance.m_sessionComponentForDraw.Add(component);
        }
        return false;
    }
}
