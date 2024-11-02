using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Havok;
using ParallelTasks;
using CringeTask = ParallelTasks.Task;

namespace CringeLauncher.Utils;

/*
public class ThreadPoolScheduler : IWorkScheduler
{
    public void Schedule(CringeTask item)
    {
        ThreadPool.UnsafeQueueUserWorkItem(new ThreadPoolWorkItemTask(item), false);
    }

    public bool WaitForTasksToFinish(TimeSpan waitTimeout) => true;

    public void ScheduleOnEachWorker(Action action)
    {
    }

    public int ReadAndClearExecutionTime() => 0;

    public void SuspendThreads(TimeSpan waitTimeout)
    {
    }

    public void ResumeThreads()
    {
    }

    public int ThreadCount { get; } = ThreadPool.ThreadCount;
}

[HarmonyPatch]
internal class KeenThreadingPatch
{
    private static MethodBase TargetMethod()
    {
        var type = Type.GetType(
            "VRage.Render11.Resources.Textures.MyTextureStreamingManager+MyStreamedTextureBase, VRage.Render11");

        return AccessTools.FirstConstructor(type, _ => true);
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var property = AccessTools.Property(typeof(Environment), nameof(Environment.ProcessorCount));

        return instructions.Manipulator(i => i.Calls(property.GetMethod),
            i =>
            {
                i.opcode = OpCodes.Ldc_I4;
                i.operand = 1024;
            });
    }
}

[HarmonyPatch]
internal class ThreadPoolWorkItemTask(CringeTask task) : IThreadPoolWorkItem
{
    public void Execute()
    {
        if (HkBaseSystem.IsInitialized && !HkBaseSystem.IsThreadInitialized) 
            HkBaseSystem.InitThread(Thread.CurrentThread.Name);
            
        task.Execute();
    }

    private static MethodBase TargetMethod()
    {
        var type = Type.GetType("System.Threading.PortableThreadPool+WorkerThread, System.Private.CoreLib", true);
        return AccessTools.DeclaredMethod(type, "WorkerThreadStart");
    }

    private static void Postfix()
    {
        if (!HkBaseSystem.IsInitialized || !HkBaseSystem.IsThreadInitialized) return;
        
        HkBaseSystem.QuitThread();
        Debug.WriteLine($"Hk Shutdown for {Thread.CurrentThread.Name}");
    }
}*/