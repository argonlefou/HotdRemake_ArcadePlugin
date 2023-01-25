using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    /// <summary>
    /// Force disabling Level-select on a new game
    /// </summary>
    class mHD_NewGameMenu
    {
        [HarmonyPatch(typeof(HD_NewGameMenu), "startNewGame")]
        class startNewGame
        {
            static bool Prefix(ScoreMode _scoreMode, MultiplayerMode _multiplayerMode)
            {
                HD_Data.LOAD_LEVEL_SELECTION = false;
                return true;
            }
        }        
    }
}
