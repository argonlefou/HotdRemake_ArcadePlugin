using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_Cheats
    {
        [HarmonyPatch(typeof(HD_Cheats), "IsCheatActive")]
        class IsCheatActive
        {
            static bool Prefix(ref bool __result, CheatType _type)
            {
                if (HotdRemake_ArcadePlugin.EnableCustomDevelopmentHacks)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(HD_BossHealth), "SetDamageLimit")]
        class SetDamageLimit
        {
            static bool Prefix(ref float _damageLimit)
            {
                if (HotdRemake_ArcadePlugin.EnableCustomDevelopmentHacks)
                {
                    _damageLimit = 0f;
                }
                return true;
            }
        }
    }
}
