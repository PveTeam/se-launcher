using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;
using CringeLauncher.Loader;
using HarmonyLib;
using Sandbox.Game.World;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class ModAssemblyLoadContextPatches
{
    private static ModAssemblyLoadContext? _currentSessionContext;
    
    [HarmonyPatch(typeof(MyScriptCompiler), nameof(MyScriptCompiler.Compile), MethodType.Async)]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CompilerTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        var matcher = new CodeMatcher(instructions);

        var load1Method = AccessTools.DeclaredMethod(typeof(Assembly), nameof(Assembly.Load), [typeof(byte[]), typeof(byte[])]);
        var load2Method = AccessTools.DeclaredMethod(typeof(Assembly), nameof(Assembly.Load), [typeof(byte[])]);

        matcher.SearchForward(i => i.Calls(load1Method))
            .InsertAndAdvance(new(OpCodes.Ldarg_0), CodeInstruction.LoadField(original.DeclaringType, "target"))
            .SetInstruction(CodeInstruction.CallClosure((byte[] assembly, byte[] symbols, MyApiTarget target) =>
            {
                if (target is not MyApiTarget.Mod) return Assembly.Load(assembly, symbols);
                ArgumentNullException.ThrowIfNull(_currentSessionContext, "No session context");
                return _currentSessionContext.LoadFromStream(new MemoryStream(assembly), new MemoryStream(symbols));
            }));
        
        matcher.SearchForward(i => i.Calls(load2Method))
            .InsertAndAdvance(new(OpCodes.Ldarg_0), CodeInstruction.LoadField(original.DeclaringType, "target"))
            .SetInstruction(CodeInstruction.CallClosure((byte[] assembly, MyApiTarget target) =>
            {
                if (target is not MyApiTarget.Mod) return Assembly.Load(assembly);
                ArgumentNullException.ThrowIfNull(_currentSessionContext, "No session context");
                return _currentSessionContext.LoadFromStream(new MemoryStream(assembly));
            }));

        return matcher.Instructions();
    }


    [HarmonyPatch(typeof(MySession), nameof(MySession.Unload))]
    [HarmonyPostfix]
    private static void UnloadPostfix()
    {
        if (_currentSessionContext is null) return;
        
        _currentSessionContext.Unload();
        _currentSessionContext = null;
    }

    [HarmonyPatch]
    private static class LoadPrefixes
    {
        [HarmonyTargetMethods]
        private static IEnumerable<MethodInfo> TargetMethods()
        {
            yield return AccessTools.Method(typeof(MySession), nameof(MySession.Load));
            yield return AccessTools.Method(typeof(MySession), "LoadMultiplayer");
        }

        [HarmonyPrefix]
        private static void Prefix()
        {
            if (_currentSessionContext is not null)
                throw new InvalidOperationException("Previous session context was not disposed");

            if (AssemblyLoadContext.GetLoadContext(typeof(MySession).Assembly) is not ICoreLoadContext coreContext)
                throw new NotSupportedException("Mod loading is not supported in this context");

            _currentSessionContext = new ModAssemblyLoadContext(coreContext);
        }
    }
}