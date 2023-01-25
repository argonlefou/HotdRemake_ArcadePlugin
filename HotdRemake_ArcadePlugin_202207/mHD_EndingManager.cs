using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_EndingManager
    {
        /// <summary>
        /// Cleaning  both Player status after end game for the next one...
        /// That way, they can start again
        /// </summary>
        [HarmonyPatch(typeof(HD_EndingManager), "returnToMainMenu")]
        class returnToMainMenu
        {
            static bool Prefix()
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_EndingManager.returnToMainMenu()");
                //Game is over.....Cleaning for the next game
                HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player1].IsWaitingToStartPlaying = true;
                HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player2].IsWaitingToStartPlaying = true;
                return true;
            }
        }                           
    }
}
