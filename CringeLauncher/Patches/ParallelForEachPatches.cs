using HarmonyLib;
using ParallelTasks;
using Sandbox.Definitions;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyDefinitionManager), "GetDefinitionBuilders")]
public static class ParallelForEachPatches
{
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> GetDefinitionBuilders_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.Method(typeof(ParallelTasks.Parallel), nameof(ParallelTasks.Parallel.ForEach)).MakeGenericMethod(typeof(string));

        foreach (var instruction in instructions)
        {
            yield return instruction.Calls(method)
                ? CodeInstruction.CallClosure((IEnumerable<string> source, Action<string> body, WorkPriority _, WorkOptions? _, bool _) =>
                {
                    System.Threading.Tasks.Parallel.ForEach(source, body);
                })
                : instruction;
        }
    }
}