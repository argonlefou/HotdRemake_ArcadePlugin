using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202204
{
    /// <summary>
    /// Waiting for players input after a CutScene
    /// Force auto-validation to start the level directly
    /// And remove unplaying character from the game (game is started as multiplayer forced to enable both players)
    /// </summary>
    class mHD_ReadyCheckPanel
    {
        [HarmonyPatch(typeof(HD_ReadyCheckPanel), "listenForConventionalInput")]
        class listenForConventionalInput
        {
            static bool Prefix(HD_ReadyCheckPanel __instance, ref bool _isPlayerReady, PlayerType _player, TMPro.TMP_Text _targetText = null)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_ReadyCheckPanel.listenForConventionalInput() " + _player.ToString());

                if (HotdRemake_ArcadePlugin.PluginPlayers[(int)_player].IsWaitingToStartPlaying)
                {
                    HD_Player.GetPlayer(_player).HealthScript.SetHealth(0);
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)_player].ContinueTimer.TimerValue = -4.0f; //-4.0f should hide time text
                    HD_Player.GetPlayer(_player).Crosshair.StateController.RegisterObjectThatForcesDisabledState(HD_Player.GetPlayer(_player));     //Deactivate crosshair, when dead
                }

                _isPlayerReady = true;
                __instance.HidePanel(); 
                return false;
            }
        }
    }
}
