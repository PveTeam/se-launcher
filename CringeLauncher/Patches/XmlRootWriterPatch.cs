using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using HarmonyLib;
using VRage.ObjectBuilders;
using VRage.ObjectBuilders.Private;

namespace CringeLauncher.Patches;

// doesn't work with crossgen enabled
/*[HarmonyPatch(typeof(CustomRootWriter), "Init")]
public static class XmlRootWriterPatch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var ins = instructions.ToList();

        var index = ins.FindIndex(b =>
                                      b.opcode == OpCodes.Ldstr && b.operand is "xsi:type");
        ins[index].operand = "xsi";

        ins.InsertRange(index + 1,
        [
            new CodeInstruction(OpCodes.Ldstr, "type"),
            new CodeInstruction(OpCodes.Ldstr, "http://www.w3.org/2001/XMLSchema-instance")
        ]);

        var instruction = ins[ins.FindIndex(b => b.opcode == OpCodes.Callvirt)];
        instruction.operand = AccessTools.Method(typeof(XmlWriter), "WriteAttributeString",
        [
            typeof(string),
            typeof(string),
            typeof(string),
            typeof(string)
        ]);

        return ins;
    }
}*/

[HarmonyPatch]
public static class XmlSerializerWriterPatch
{
    private static IEnumerable<MethodInfo> TargetMethods()
    {
        yield return AccessTools.DeclaredMethod(typeof(MyObjectBuilderSerializerKeen),
            nameof(MyObjectBuilderSerializerKeen.SerializeXML),
            [typeof(string), typeof(bool), typeof(MyObjectBuilder_Base), typeof(ulong).MakeByRefType(), typeof(Type)]);
        yield return AccessTools.DeclaredMethod(typeof(MyObjectBuilderSerializerKeen), "SerializeXMLInternal");
    }
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.DeclaredMethod(typeof(XmlSerializer), nameof(XmlSerializer.Serialize), [typeof(Stream), typeof(object)]);
        var serialize = AccessTools.DeclaredMethod(typeof(XmlSerializerWriterPatch), nameof(Serialize));
        return instructions.Manipulator(i => i.Calls(method), i =>
        {
            i.opcode = OpCodes.Call;
            i.operand = serialize;
        });
    }

    private static void Serialize(XmlSerializer serializer, Stream stream, object value)
    {
        var writer = new XmlTextWriter(stream, Encoding.UTF8);
        serializer.Serialize(writer, value);
    }
}