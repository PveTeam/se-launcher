using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using VRage.Scripting;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class WhitelistRegistrationPatch
{
    private static IEnumerable<MethodInfo> TargetMethods()
    {
        yield return AccessTools.Method(typeof(MyScriptWhitelist), "Register",
                                        new[] { typeof(MyWhitelistTarget), typeof(INamespaceSymbol), typeof(Type) });
        yield return AccessTools.Method(typeof(MyScriptWhitelist), "Register",
                                        new[] { typeof(MyWhitelistTarget), typeof(ITypeSymbol), typeof(Type) });
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var ins = instructions.ToList();

        var label = generator.DefineLabel();
        
        ins[0].labels.Add(label);

        ins.InsertRange(0, [
            CodeInstruction.LoadArgument(3),
            new(OpCodes.Callvirt, AccessTools.DeclaredPropertyGetter(typeof(Type), nameof(Type.IsPublic))),
            new(OpCodes.Brtrue, label),
            CodeInstruction.LoadArgument(3),
            new(OpCodes.Callvirt, AccessTools.DeclaredPropertyGetter(typeof(Type), nameof(Type.IsNestedPublic))),
            new(OpCodes.Brtrue, label),
            new(OpCodes.Ret)
        ]);
        
        var throwIns = ins.FindAll(b => b.opcode == OpCodes.Throw).Select(b => ins.IndexOf(b));
        foreach (var index in throwIns)
        {
            var i = index;
            do
            {
                ins[i] = new(OpCodes.Nop);
            } while (ins[--i].opcode.FlowControl != FlowControl.Cond_Branch);

            ins[index] = new(OpCodes.Ret);
        }

        return ins;
    }
}