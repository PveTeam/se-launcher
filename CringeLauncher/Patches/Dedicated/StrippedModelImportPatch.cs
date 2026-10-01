using HarmonyLib;
using VRage.FileSystem;
using VRageRender.Import;

namespace CringeLauncher.Patches.Dedicated;

[HarmonyPatch(typeof(MyModelImporter), nameof(MyModelImporter.ImportData))]
[HarmonyPatchCategory("Dedicated")]
internal static class StrippedModelImportPatch
{
    private static bool Prefix(string assetFileName)
    {
        if (string.IsNullOrWhiteSpace(assetFileName))
            return false;

        var path = Path.IsPathRooted(assetFileName)
            ? assetFileName
            : Path.Combine(MyFileSystem.ContentPath, assetFileName);

        return MyFileSystem.FileExists(path);
    }
}
