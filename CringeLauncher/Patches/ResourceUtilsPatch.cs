#if !WINDOWS
using CringeLauncher.Platform.Xplat;
using HarmonyLib;
using VRage.FileSystem;
using VRage.Render11.Resources;
using VRageRender;

namespace CringeLauncher.Patches;

[HarmonyPatch]
internal static class ResourceUtilsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MyResourceUtils), nameof(MyResourceUtils.NormalizeFileTextureName),
        [typeof(string), typeof(Uri)], [ArgumentType.Ref, ArgumentType.Out])]
    private static bool NormalizeFileNamePrefix(ref string name, out Uri? uri, out bool __result)
    {
        if (MyRenderProxy.IsValidGeneratedTextureName(name))
        {
            uri = null;
            __result = false;
            return false;
        }

        if (!Path.IsPathRooted(name))
        {
            name = Path.Join(MyFileSystem.ContentPath, name);
        }

        uri = new(name, UriKind.Absolute);
        __result = true;

        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MyResourceUtils), nameof(MyResourceUtils.NormalizePath))]
    private static bool NormalizePathPrefix(string path, out string __result)
    {
        LauncherFileProvider.Instance.NormalizePath(ref path);
        __result = path;
        return false;
    }
}
#endif
