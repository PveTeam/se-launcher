using HarmonyLib;
using SharpDX.Multimedia;
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
        AccessTools.Method(typeof(SoundStream), nameof(SoundStream.ToDataStream)),
        AccessTools.Method(typeof(System.StreamExtensions), nameof(System.StreamExtensions.ReadNoAlloc)),
        AccessTools.Method(typeof(System.StreamExtensions), nameof(System.StreamExtensions.ReadString)),
        AccessTools.Method(typeof(System.StreamExtensions), nameof(System.StreamExtensions.SkipBytes)),
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
