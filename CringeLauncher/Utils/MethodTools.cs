using System.Reflection;
using HarmonyLib;

namespace CringeLauncher.Utils;

public static class MethodTools
{
    public static MethodInfo AsyncMethodBody(MethodInfo method)
    {
        var (_, operand) = PatchProcessor.ReadMethodBody(method).First();
        
        if (operand is not LocalVariableInfo localVar)
            throw new InvalidOperationException($"Method {method.FullDescription()} does not contain a valid async state machine");

        return AccessTools.Method(localVar.LocalType, "MoveNext") ??
               throw new InvalidOperationException(
                   $"Async State machine of method {method.FullDescription()} does not contain a valid MoveNext method");
    }
}