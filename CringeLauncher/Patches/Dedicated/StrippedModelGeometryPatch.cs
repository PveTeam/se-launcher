using HarmonyLib;
using VRage.Game.Models;
using VRageRender.Import;

namespace CringeLauncher.Patches.Dedicated;

[HarmonyPatch(typeof(MyModel), nameof(MyModel.LoadGeometryData))]
[HarmonyPatchCategory("Dedicated")]
internal static class StrippedModelGeometryPatch
{
    private static bool Prefix(MyModel __instance, MyModelImporter importer)
    {
        if (importer.GetTagData().ContainsKey("Vertices"))
            return true;

        __instance.m_vertices = [];
        __instance.m_verticesCount = 0;
        __instance.m_meshContainer.Clear();
        __instance.m_Indices = null;
        __instance.m_Indices_16bit = null;
        __instance.m_bonesIndicesWeights = null;
        __instance.Triangles = [];
        __instance.m_trianglesCount = 0;
        return false;
    }
}
