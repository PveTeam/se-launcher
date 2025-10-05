using HarmonyLib;
using VRageRender.Import;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyModelImporter), "ImportData")]
internal static class ModelImporterPatch
{
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
        while (readTotal < count)
        {
            var readBytes = stream.Read(array, readTotal, count - readTotal);

            if (readBytes == 0)
                break;

            readTotal += readBytes;
        }

        return readTotal;
    }
}
