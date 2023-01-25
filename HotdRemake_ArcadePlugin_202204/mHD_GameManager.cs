using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_GameManager
    {
        /// <summary>
        /// Overriding default damage settings :
        /// On lower difficulty, game is only using "half lives" damage 
        /// Full life dammage must be 2 hit points
        /// </summary>
        [HarmonyPatch(typeof(HD_GameManager), "CurrentDifficultyLevel", MethodType.Setter)]
        class CurrentDifficultyLevel
        {
            static void Postfix(HD_DifficultyLevel value, HD_DifficultyLevel ___currentDifficultyLevel)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_GameManager.set_CurrentDifficultyLevel() => Overriding damage settings");
                if (___currentDifficultyLevel != null)
                {
                    ___currentDifficultyLevel.DefaultDamage = 2;
                    ___currentDifficultyLevel.BossDamage = 2;
                    ___currentDifficultyLevel.ScientistKillDamage = 2;
                    ___currentDifficultyLevel.WeaponDamage = 2;
                }
            }
        } 

        /// <summary>
        /// Disable game pausing when loosing focus on window
        /// Methods :
        /// Pause()
        /// PauseWithoutMenu()
        /// Are also calling the inner method "pause()". PauseWithoutMenu is used for Tutorial pause (so no touching).
        /// </summary>
        [HarmonyPatch(typeof(HD_GameManager), "OnApplicationFocus")]
        class OnApplicationFocus
        {
            static bool Prefix(ref bool _focus)
            {
                return false;
            }
        }
    }
}
