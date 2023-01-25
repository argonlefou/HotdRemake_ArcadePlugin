using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    /// <summary>
    /// Block the Ahievments popups from displaying during the game
    /// </summary>
    class mHD_AchievementHandler
    {
        [HarmonyPatch(typeof(HD_AchievementHandler), "unlockAchievement")]
        class unlockAchievement
        {
            static bool Prefix(MegaPixel.Achievements.AchievementType _achievementType)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_AchievementHandler.unlockAchievement() => AchievementType: " + _achievementType.ToString());
                return false;
            }
        }
    }
}
