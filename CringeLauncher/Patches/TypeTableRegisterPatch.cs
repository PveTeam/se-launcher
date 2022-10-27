using HarmonyLib;
using VRage.Network;

namespace CringeLauncher.Patches;

[HarmonyPatch(typeof(MyTypeTable), "IsSerializableClass")]
public static class TypeTableRegisterPatch
{
    private static void Postfix(Type type, ref bool __result)
    {
        if (type == typeof(Delegate) || type == typeof(MulticastDelegate))
            __result = true;
    }
}