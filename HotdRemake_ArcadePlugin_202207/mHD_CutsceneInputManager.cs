using System.Runtime.CompilerServices;
using HarmonyLib;
using Rewired;
using System;

namespace HotdRemake_ArcadePlugin_202207
{
    /// <summary>
    /// Handle the cutscene skipping text and events
    /// So that both players can skip cut scenes by pressing START
    /// </summary>
    class mHD_CutsceneInputManager
    {
        [HarmonyPatch(typeof(HD_CutsceneInputManager), "handleInputs", new Type[] { typeof(PlayerType), typeof(Player) })]
        class handleInputs
        {
            static bool Prefix(PlayerType _playerType, Player _playerInput, HD_CutsceneUI ___cutsceneUI, ref bool ___playerIsSkippingCutscene, ref PlayerType ___playerCurrentlySkippingCutscene, HD_CutsceneInputManager __instance)
            {
                //HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_CutsceneInputManager.handleInputs() => _playerType = " + _playerType.ToString());
                if (HD_Cutscene.IsSkippingDelayed() && !HD_Cutscene.IsReadyToSkipAfterDelay())
                {
                    return false;
                }
                if (!HD_Cutscene.CanSkip())
                {
                    return false;
                }

                //NEw way : Skip directly
                if (!___playerIsSkippingCutscene)
                {
                    if (HotdRemake_ArcadePlugin.GetButtonDown(_playerType, HotdRemake_ArcadePlugin.MyInputButtons.Start))
                    {
                        if (HD_PlayerCombatManager.Instance.GetPlayerState(_playerType) == HD_PlayerCombatManager.PlayerState.Up)
                        {
                            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_CutsceneInputManager.handleInputs() => " + _playerType.ToString() + " skipped Cut-Scene");
                            ___playerIsSkippingCutscene = true;
                            ___playerCurrentlySkippingCutscene = _playerType;
                            HD_Cutscene.Skip();
                        }
                        else
                        {
                            if (HotdRemake_ArcadePlugin.Configurator.Freeplay)
                            {
                                HD_Player.GetPlayer(_playerType).RevivePlayer();
                            }
                            else
                            {
                                if (HotdRemake_ArcadePlugin.PluginPlayers[(int)_playerType].IsWaitingToStartPlaying && HotdRemake_ArcadePlugin.CurrentCreditsCount >= HotdRemake_ArcadePlugin.Configurator.CreditsToStart)
                                {
                                    HotdRemake_ArcadePlugin.CurrentCreditsCount -= HotdRemake_ArcadePlugin.Configurator.CreditsToStart;
                                    HD_Player.GetPlayer(_playerType).RevivePlayer();
                                }
                                else if (!HotdRemake_ArcadePlugin.PluginPlayers[(int)_playerType].IsWaitingToStartPlaying && HotdRemake_ArcadePlugin.CurrentCreditsCount >= HotdRemake_ArcadePlugin.Configurator.CreditsToContinue)
                                {
                                    HotdRemake_ArcadePlugin.CurrentCreditsCount -= HotdRemake_ArcadePlugin.Configurator.CreditsToContinue;
                                    HD_Player.GetPlayer(_playerType).RevivePlayer();
                                }
                            }

                            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_CutsceneInputManager.handleInputs() => Adding " + _playerType.ToString() + " to the game");
                            HD_Player.GetPlayer(_playerType).RevivePlayer();
                            HotdRemake_ArcadePlugin.PluginPlayers[(int)_playerType].IsWaitingToStartPlaying = false;
                        }
                    }
                }
                return false;
            }
        }

        //LateUpdate is calling a new procedure just for P1 -> garbage
        [HarmonyPatch(typeof(HD_CutsceneInputManager), "LateUpdate")]
        class LateUpdate
        {
            static bool Prefix()
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(HD_CutsceneInputManager), "changeCutsceneSkipTimer")]
        class mChangeCutsceneSkipTimer
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void changeCutsceneSkipTimer(object instance, float time)
            {
                //Used to call the private method    
            }
        }
    }
}
