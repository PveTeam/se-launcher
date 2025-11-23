using System.Reflection.Emit;
using System.Runtime.InteropServices;
using HarmonyLib;
using Sandbox.Game.Entities;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyEntityCreationThread), MethodType.Constructor)]
internal static class EntityCreationThreadPriorityPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return instructions;
        var field = AccessTools.DeclaredField(typeof(MyEntityCreationThread), "m_thread");
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.StoresField(field))
            .Insert(new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Ldc_I4_1),
                new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.PropertySetter(typeof(Thread), nameof(Thread.IsBackground)))
            )
            .InstructionEnumeration();
    }
}