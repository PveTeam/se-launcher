using System.Reflection;
using HarmonyLib;
using Pillar.Demystifier;

namespace CringeLauncher.CrashPad;

internal class HarmonyStackFrameMethodResolver : IStackFrameMethodResolver
{
    public MethodBase? ResolveMethod(MethodBase method)
    {
        if (method is MethodInfo methodInfo && Harmony.GetOriginalMethod(methodInfo) is { } originalMethod)
            return originalMethod;
        
        return null;
    }
}