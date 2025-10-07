using HarmonyLib;
using SharpDX.Toolkit.Graphics;
using System.Reflection;
using VRageRender.Import;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class ModelImporterPatch
{
    [HarmonyTargetMethods]
    public static IEnumerable<MethodBase> TargetMethods() =>
    [
        AccessTools.Method(typeof(MyModelImporter), "ImportData"),
        AccessTools.Method(typeof(DDSHelper), "TryReadDDSHeader"),
        AccessTools.Method(typeof(DDSHelper), "CreateCompressedImageFromStream"),
    ];

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> ImportData_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var method = AccessTools.Method(typeof(Stream), nameof(Stream.Read), [typeof(byte[]), typeof(int), typeof(int)]);
        foreach (var instruction in instructions)
        {
            yield return instruction.Calls(method)
                ? CodeInstruction.CallClosure(ReadFully)
                : instruction;
        }
    }

    private static int ReadFully(Stream stream, byte[] array, int offset, int count)
    {
        var readTotal = offset;
        var finalPosition = count + offset;
        while (readTotal < finalPosition)
        {
            var readBytes = stream.Read(array, readTotal, count - readTotal);

            if (readBytes == 0)
                break;

            readTotal += readBytes;
        }

        return readTotal;
    }
}
