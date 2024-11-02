using HarmonyLib;
using System.Reflection;
using System.Xml.Serialization;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class XmlSerializerPatch
{
    [HarmonyTargetMethod]
    private static MethodInfo TargetMethod() => AccessTools.Method(
        typeof(XmlSerializer).Assembly.GetType("System.Xml.Serialization.TempAssembly"),
        "GenerateRefEmitAssembly");

    [HarmonyTranspiler]
    private static List<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var list = instructions.ToList();
        var method = AccessTools.PropertyGetter(typeof(Type), nameof(Type.Assembly));
        var index = list.FindIndex(x => x.Calls(method));
        list[index] = CodeInstruction.Call(typeof(XmlSerializerPatch), nameof(GetCorrectAssembly));
        return list;
    }

    private static Assembly GetCorrectAssembly(Type type)
    {
        if (type.Assembly.IsCollectible || type.GenericTypeArguments is not { } genericArgs || genericArgs.Length == 0)
            return type.Assembly;

        foreach (var assembly in genericArgs.Select(x => x.Assembly))
        {
            if (assembly.IsCollectible)
                return assembly;
        }
        return type.Assembly;
    }
}
