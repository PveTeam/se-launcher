using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using CringeLauncher.Utils;
using HarmonyLib;
using Sandbox.Game.Entities;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class ThreadSchedulerPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.GetDeclaredConstructors(typeof(ParallelTasks.PrioritizedScheduler.Worker))[0];
        yield return AccessTools.DeclaredConstructor(typeof(MyEntityCreationThread));
    }
    
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return instructions;

        var codeMatcher = new CodeMatcher(instructions);
        return codeMatcher
            .MatchStartForward(CodeMatch.WithOpcodes([OpCodes.Newobj], AccessTools.DeclaredConstructor(typeof(Thread), [typeof(ThreadStart)])))
            .SetInstructionAndAdvance(original.DeclaringType == typeof(MyEntityCreationThread)
                ? new CodeInstruction(OpCodes.Ldstr, "Entity creation thread")
                : CodeInstruction.LoadArgument(2))
            .SetAndAdvance(OpCodes.Call, AccessTools.DeclaredMethod(typeof(PlatformApi), nameof(PlatformApi.CreateThread)))
            .MatchStartForward(CodeMatch.IsLdarg(0),
                CodeMatch.LoadsField(AccessTools.DeclaredField(original.DeclaringType, "m_thread")))
            .RemoveInstructions(codeMatcher.Remaining)
            .End()
            .InsertAfter(new CodeInstruction(OpCodes.Pop), new CodeInstruction(OpCodes.Ret))
            .InstructionEnumeration();
    }
}

[HarmonyPatch]
internal static class ThreadProcPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.DeclaredMethod(typeof(ParallelTasks.PrioritizedScheduler.Worker), "WorkerLoop");
        yield return AccessTools.DeclaredMethod(typeof(MyEntityCreationThread), "ThreadProc");
    }
    
    private static void Prefix()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return;
        
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
    }
}
