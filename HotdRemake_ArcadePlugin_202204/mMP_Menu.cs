using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    /// <summary>
    /// Disabling HInts diplayed when menus are opened
    /// This was showing when LEaderboard is displayed on Attractmode, by simulating a menu button click
    /// </summary>
    class mMP_Menu
    {
        [HarmonyPatch(typeof(MP_Menu), "setHintsEnabled")]
        class setHintsEnabled
        {
            static bool Prefix(ref bool _enabled, MP_Hint.HintType[] ___hintsToEnable)
            {
                for (int i = 0; i < ___hintsToEnable.Length; i++)
                {
                    if (___hintsToEnable[i] == MP_Hint.HintType.Tutorial)
                        return true;
                }
                _enabled = false;
                return true;
            }
        }
    }
}
