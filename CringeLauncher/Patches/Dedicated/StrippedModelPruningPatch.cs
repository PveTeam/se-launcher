using HarmonyLib;
using VRage.Game.Models;
using NLog;

namespace CringeLauncher.Patches.Dedicated;

[HarmonyPatch(typeof(MyModel), nameof(MyModel.LoadData))]
[HarmonyPatchCategory("Dedicated")]
internal static class StrippedModelPruningPatch
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private static void Postfix(MyModel __instance)
    {
        var structure = new HullPruningStructure();
        
        try
        {
            structure.Collect(__instance.HavokCollisionShapes);
        }
        catch (Exception e)
        {
            Log.Error(e, "Model {AssetName} unable to build hull pruning structure", __instance.AssetName);
        }

        __instance.m_bvh = structure;
    }
}
