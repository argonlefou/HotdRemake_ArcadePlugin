using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_PlayerCombatManager
    {
        /// <summary>
        /// Adding another "condition" to set a status down : not beeing in the game
        /// This is read by the CutSceneUI_Manager to display "PRESS START TO SKIP" text
        /// On this game, we have to start the game as multiplayer, so we need to deactivate one of the player at start not to display it
        /// </summary>
        [HarmonyPatch(typeof(HD_PlayerCombatManager), "handlePlayerStateChange")]
        class handlePlayerStateChange
        {
            static bool Prefix(PlayerType _type, ref HD_PlayerCombatManager.PlayerState _state, HD_PlayerHealth _health)
            {
                if (HotdRemake_ArcadePlugin.PluginPlayers[(int)_type].IsWaitingToStartPlaying)
                {
                    _state = HD_PlayerCombatManager.PlayerState.Down;
                    _health.HealthCurrent = 0;
                }

                return true;
            }
        }
    }
}
