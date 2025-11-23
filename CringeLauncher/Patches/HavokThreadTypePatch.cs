using CringeLauncher.CrashPad;
using HarmonyLib;
using Havok;
using Sandbox.Engine.Physics;
using Sandbox.Engine.Utils;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyPhysics), nameof(MyPhysics.LoadData))]
internal static class HavokThreadTypePatch
{
    private static void Postfix(HkJobThreadPool ___m_threadPool)
    {
        if (MyFakes.ENABLE_HAVOK_MULTITHREADING)
            ___m_threadPool.RunOnEachWorker(() => ThreadInformationTracker.MarkCurrentThreadType(ExceptionInformation.ThreadType.HavokPool));
    }
}