using HarmonyLib;
using ParallelTasks;
using Sandbox;
using Sandbox.Game.World;
using System.Reflection;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class ParallelForPatches
{
    [HarmonyTargetMethods]
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(MySandboxGame), "PerformPreloading");

        var foundType = false;
        foreach (var type in typeof(MySession).GetNestedTypes(BindingFlags.NonPublic))
        {
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (method.Name.Contains("PreloadModels") && method.GetParameters().Length == 0 && method.ReturnType == typeof(void))
                {
                    foundType = true;
                    yield return method;
                }
            }

            if (foundType)
                yield break;
        }

        if (!foundType)
            throw new InvalidOperationException("Could not find MySession.PreloadModels method");
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PreloadModels_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.Method(typeof(ParallelTasks.Parallel), nameof(ParallelTasks.Parallel.For),
            [typeof(int), typeof(int), typeof(Action<int>), typeof(WorkPriority), typeof(WorkOptions?)]);

        foreach (var instruction in instructions)
        {
            yield return instruction.Calls(method)
                ? CodeInstruction.CallClosure((int start, int end, Action<int> body, WorkPriority _, WorkOptions? _) =>
                {
                    System.Threading.Tasks.Parallel.For(start, end, body);
                })
                : instruction;
        }
    }

}
