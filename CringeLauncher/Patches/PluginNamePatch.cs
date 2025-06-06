using System.Reflection.Emit;
using HarmonyLib;
using Sandbox;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MySandboxGame), nameof(MySandboxGame.Run))]
public static class PluginNamePatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // replaces $"Plugin Init: {plugin.GetType()}"
        // to be just $"Plugin Init: {plugin}"
        // so you could override .ToString
        // doesn't change default behavior since base .ToString is .GetType().ToString()

        return new CodeMatcher(instructions)
            .SearchForward(b => b.Is(OpCodes.Ldstr, "Plugin Init: "))
            .Advance(2)
            .Set(OpCodes.Callvirt, AccessTools.DeclaredMethod(typeof(object), nameof(ToString)))
            .InstructionEnumeration();
    }
}