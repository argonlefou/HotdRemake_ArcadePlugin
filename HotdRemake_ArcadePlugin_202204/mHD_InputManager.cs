using System.Runtime.CompilerServices;
using HarmonyLib;
using Rewired;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_InputManager
    {
        /// <summary>
        /// Disabling In-Game Pause
        /// Forcing enabling P2 on skip cut scenes
        /// </summary>
        [HarmonyPatch(typeof(HD_InputManager), "handleControlInputsOfPlayer")]
        class handleControlInputsOfPlayer
        {
            static bool Prefix(HD_InputManager __instance, Player[] ___playerInputs, int _playerIndex, bool _canSkipCutscene = true)
            {
                Rewired.Player player = ___playerInputs[_playerIndex];
                if (mIsPauseToggled.isPauseToggled(__instance, player))
                {
                    if (MegaPixel.ApplicationManagement.MP_ApplicationManager.IsPaused)
                    {
                        HD_GameManager.Resume();
                    }
                    else
                    {
                        //Disabling Pause in game !
                        //HD_GameManager.Pause();
                    }
                }
                else if (MegaPixel.ApplicationManagement.MP_ApplicationManager.IsPaused && (player.GetButtonDown(10) || player.GetButtonDown(36)))
                {
                    HD_GameManager.Resume();
                }

                if (HD_Cutscene.IsCutsceneActive && HD_Cutscene.CanSkip())
                {
                    if (_playerIndex == 0)
                        HD_CutsceneInputManager.HandleInputs(PlayerType.Player1, player);
                    else
                        HD_CutsceneInputManager.HandleInputs(PlayerType.Player2, player);
                }
                return false;
            }
        }

        /// <summary>
        /// Disabling In-Game Pause
        /// Forcing enabling P2 on skip cut scenes
        /// </summary>
        [HarmonyPatch(typeof(HD_InputManager), "handleSendingInput")]
        class handleSendingInput
        {
            static bool Prefix(HD_InputManager __instance, Player[] ___playerInputs, HD_Player _player, PlayerType _playerType)
            {
                if (HD_Cutscene.IsCutsceneActive || !HD_ReadyCheckPanel.PlayersReady)
                {
                    return false;
                }

                Player player = ___playerInputs[(int)_player.ThisPlayer];
                if (player == null)
                {
                    return false;
                }

                //"Reviving" a player will be different if it's from a GameOver state (=start new game) or Continue State (= continue)
                if (_player.CanBeRevived())
                {
                    if (HotdRemake_ArcadePlugin.GetButtonDown(_player.ThisPlayer, HotdRemake_ArcadePlugin.MyInputButtons.Start))
                    {
                        if (HotdRemake_ArcadePlugin.Configurator.Freeplay)
                        {
                            _player.RevivePlayer();
                        }
                        else
                        {
                            if (HotdRemake_ArcadePlugin.PluginPlayers[(int)_playerType].IsWaitingToStartPlaying && HotdRemake_ArcadePlugin.CurrentCreditsCount >= HotdRemake_ArcadePlugin.Configurator.CreditsToStart)
                            {
                                HotdRemake_ArcadePlugin.CurrentCreditsCount -= HotdRemake_ArcadePlugin.Configurator.CreditsToStart;
                                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_InputManager.handleSendingInput() => Ok to revive (START)" + _player.ThisPlayer.ToString());
                                _player.RevivePlayer();
                            }
                            else if (!HotdRemake_ArcadePlugin.PluginPlayers[(int)_playerType].IsWaitingToStartPlaying && HotdRemake_ArcadePlugin.CurrentCreditsCount >= HotdRemake_ArcadePlugin.Configurator.CreditsToContinue)
                            {
                                HotdRemake_ArcadePlugin.CurrentCreditsCount -= HotdRemake_ArcadePlugin.Configurator.CreditsToContinue;
                                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_InputManager.handleSendingInput() => Ok to revive (CONTINUE)" + _player.ThisPlayer.ToString());                                
                                _player.RevivePlayer();
                            }
                        }
                    }
                }

                if (_player.HealthScript.HasDied)
                {
                    return false;
                }

                if (HotdRemake_ArcadePlugin.GetButtonDown(_player.ThisPlayer, HotdRemake_ArcadePlugin.MyInputButtons.Trigger))
                    _player.HandleShooting();

                if (HotdRemake_ArcadePlugin.GetButtonDown(_player.ThisPlayer, HotdRemake_ArcadePlugin.MyInputButtons.Reload))
                    _player.HandleReloading();

                Vector2 vAim = HotdRemake_ArcadePlugin.GetAiming(_player.ThisPlayer);
                _player.HandleAiming(ref vAim);
                
                return false;
            }
        }

        [HarmonyPatch(typeof(HD_InputManager), "isPauseToggled")]
        class mIsPauseToggled
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static bool isPauseToggled(object instance, Rewired.Player p)
            {
                //Used to call the private method GameStart.EnterGame()     
                return false;
            }
        } 
    }
}
