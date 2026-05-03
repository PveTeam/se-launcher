using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Xml.Serialization;
using CringeBootstrap.Abstractions;
using CringeLauncher.Loader;
using CringePlugins.Utils;
using HarmonyLib;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Game;
using Sandbox.Game.World;
using SharedCringe.Loader;
using VRage;
using VRage.FileSystem;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity.UseObject;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Plugins;
using CringePlugins.Loader;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class IntrospectionPatches
{
    [HarmonyPrefix, HarmonyPatch(typeof(Assembly), nameof(Assembly.GetTypes))]
    private static bool GetTypesPrefix(Assembly __instance, ref Type[] __result)
    {
        if (AssemblyLoadContext.GetLoadContext(__instance) is ICoreLoadContext or ModAssemblyLoadContext || __instance.FullName?.StartsWith("System.") == true)
            return true;

        if (__instance.IsDynamic)
        {
            __result = [];
            return false;
        }

        if (AssemblyLoadContext.GetLoadContext(__instance) is ModAssemblyLoadContext or DerivedAssemblyLoadContext)
        {
            //mods need to look for specific derived types
            Debug.WriteLine($"Getting special types for {__instance.FullName}");
            var module = __instance.GetMainModule();
            HashSet<Type> set =
            [
                .. IntrospectionContext.Global.CollectDerivedTypes<MyObjectBuilder_Base>(module),
                .. IntrospectionContext.Global.CollectDerivedTypes<MyStatLogic>(module),
                .. IntrospectionContext.Global.CollectAttributedTypes<MyObjectBuilderDefinitionAttribute>(module),
                .. IntrospectionContext.Global.CollectDerivedTypes<MyComponentBase>(module),
                .. IntrospectionContext.Global.CollectAttributedTypes<MyComponentBuilderAttribute>(module),
                .. IntrospectionContext.Global.CollectDerivedTypes<IMyTextSurfaceScript>(module),
                .. IntrospectionContext.Global.CollectDerivedTypes<IMyUseObject>(module),
                .. IntrospectionContext.Global.CollectDerivedTypes<IMyHudStat>(module),
                .. IntrospectionContext.Global.CollectAttributedTypes<MySessionComponentDescriptor>(module),
            ];

            __result = [.. set];

            return false;
        }

        Debug.WriteLine($"Blocking GetTypes for {__instance.FullName}");

        __result = [];
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AccessTools), nameof(AccessTools.GetTypesFromAssembly))]
    private static bool GetTypesHarmonyPrefix(Assembly assembly, ref Type[] __result)
    {
        if (AssemblyLoadContext.GetLoadContext(assembly) is ICoreLoadContext)
            return true;

        if (assembly.IsDynamic)
        {
            __result = [];
            return false;
        }

        // static classes are abstract
        __result = [.. IntrospectionContext.Global.CollectAttributedTypes<HarmonyAttribute>(assembly.GetMainModule(), true)];
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AccessTools), nameof(AccessTools.TypeByName))]
    private static bool TypeByNameHarmonyPrefix(ref Type __result, string name)
    {
        if (!PluginAssemblyLoadContext.TypeToAssembly.TryGetValue(name, out var assembly))
            return true;

        var module = assembly.GetMainModule();

        __result = module.GetType(name, true, false)!;

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(AccessTools), nameof(AccessTools.AllAssemblies))]
    private static bool AllAssembliesHarmonyPrefix(ref IEnumerable<Assembly> __result)
    {
        __result = AssemblyLoadContext.GetLoadContext(typeof(IntrospectionPatches).Assembly)?.Assemblies
            .Concat(PluginsLifetime.Contexts.SelectMany(x => x.Assemblies)) ?? [];
        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MySession), "PrepareBaseSession", typeof(MyObjectBuilder_Checkpoint), typeof(MyObjectBuilder_Sector))]
    private static void PrepareSessionPrefix()
    {
        // i hate keen for that in MyUseObjectFactory..cctor
        // MyUseObjectFactory.RegisterAssemblyTypes(Assembly.LoadFrom(Path.Combine(MyFileSystem.ExePath, "Sandbox.Game.dll")));

        MyUseObjectFactory.RegisterAssemblyTypes(MyPlugins.SandboxGameAssembly);
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MyPlugins), nameof(MyPlugins.LoadPlugins))]
    private static bool LoadPluginsPrefix(List<Assembly> assemblies, List<IPlugin> ___m_plugins, List<IHandleInputPlugin> ___m_handleInputPlugins)
    {
        foreach (var type in assemblies.SelectMany(b => IntrospectionContext.Global.CollectDerivedTypes<IPlugin>(b.GetMainModule())))
        {
            var instance = Activator.CreateInstance(type);

            if (instance is IPlugin plugin)
                ___m_plugins.Add(plugin);

            if (instance is IHandleInputPlugin handleInputPlugin)
                ___m_handleInputPlugins.Add(handleInputPlugin);
        }

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MyXmlSerializerManager), "TryLoadSerializerFrom")]
    private static bool LoadSerializerPrefix(string assemblyName, string typeName, ref XmlSerializer? __result)
    {
        if (AssemblyLoadContext.GetLoadContext(typeof(IntrospectionPatches).Assembly) is not ICoreLoadContext context)
            return false;

        var assembly = context.ResolveFromAssemblyName(new(assemblyName));

        if (assembly?.GetType($"Microsoft.Xml.Serialization.GeneratedAssembly.{typeName}Serializer") is not { } type)
            return false;

        __result = Activator.CreateInstance(type) as XmlSerializer;

        return false;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MyXmlSerializerManager), nameof(MyXmlSerializerManager.RegisterFromAssembly))]
    private static bool XmlManagerRegisterPrefix(Assembly assembly) => AssemblyLoadContext.GetLoadContext(assembly) is ICoreLoadContext;

    [HarmonyPatch]
    private static class GameAssembliesPatch
    {
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            return AccessTools.GetDeclaredMethods(typeof(MyPlugins))
                .Where(b => b.Name.StartsWith("Register") &&
                            b.GetParameters() is [var param] && param.ParameterType == typeof(string));
        }

        private static bool Prefix([HarmonyArgument(0)] string file)
        {
            var name = AssemblyName.GetAssemblyName(Path.Join(MyFileSystem.ExePath, file));

            ref var staticAssembly = ref AccessTools.StaticFieldRefAccess<Assembly>(typeof(MyPlugins), name.Name switch
            {
                "Sandbox.Common" => "m_sandboxAssembly",
                "Sandbox.Game" => "m_sandboxGameAssembly",
                "SpaceEngineers.Game" => "m_gamePluginAssembly",
                "SpaceEngineers.ObjectBuilders" => "m_gameObjBuildersPlugin",
                _ => throw new ArgumentOutOfRangeException(null, $"Unknown assembly name: {name}")
            });

            staticAssembly = AssemblyLoadContext.GetLoadContext(typeof(GameAssembliesPatch).Assembly)!
                .LoadFromAssemblyName(name);

            return false;
        }
    }
}
