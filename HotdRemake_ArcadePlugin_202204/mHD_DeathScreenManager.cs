using System;
using System.Collections;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_DeathScreenManager
    {
        /// <summary>
        /// At start(), re-parenting some GUI items of the Players death Screen to make them visible, while deactivation the rest of the elements
        /// This should enable displaying a big "Continue ?" with Countdown, and remove all other initial useless stuff
        /// </summary>
        [HarmonyPatch(typeof(HD_DeathScreenManager), "Start")]
        class Start
        {
            static bool Prefix(HD_PlayerDeathScreen ___firstPlayerDeathScreen, HD_PlayerDeathScreen ___secondPlayerDeathScreen, TMP_Text ___timerText, GameObject ___timerMainGO)
            {
                Transform[] trs = ___firstPlayerDeathScreen.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in trs)
                {
                    if (t.name.Equals("txt_Continue"))
                    {
                        t.SetParent(___firstPlayerDeathScreen.transform);
                        TMPro.TextMeshProUGUI Text = t.gameObject.GetComponent<TMPro.TextMeshProUGUI>();
                        if (Text != null)
                        {
                            Text.autoSizeTextContainer = true;
                            Text.enableAutoSizing = true;
                            Text.fontSizeMax = 120.0f;
                            Text.fontSize = 120.0f;
                            Text.fontStyle = TMPro.FontStyles.Bold;
                            Text.color = Color.white;
                            Text.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                            Text.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                            Text.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
                            break;
                        }
                    }
                }

                trs = ___secondPlayerDeathScreen.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in trs)
                {
                    if (t.name.Equals("txt_Continue"))
                    {
                        t.SetParent(___secondPlayerDeathScreen.transform);
                        TMPro.TextMeshProUGUI Text = t.gameObject.GetComponent<TMPro.TextMeshProUGUI>();
                        if (Text != null)
                        {
                            Text.autoSizeTextContainer = true;
                            Text.enableAutoSizing = true;
                            Text.fontSizeMax = 120.0f;
                            Text.fontSize = 120.0f;
                            Text.fontStyle = TMPro.FontStyles.Bold;
                            Text.color = Color.white;
                            Text.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                            Text.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                            Text.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
                            break;
                        }                        
                    }
                }
                return true;
            }
        }

        //Update is called when the general timer is running, so we can look for Time Out and call for Game-Over action
        [HarmonyPatch(typeof(HD_DeathScreenManager), "Update")]
        class Update
        {
            static void Postfix(HD_DeathScreenManager __instance, HD_PlayerDeathScreen ___firstPlayerDeathScreen, HD_PlayerDeathScreen ___secondPlayerDeathScreen, GameObject ___timerMainGO, TMP_Text ___timerText)
            {
                if (___firstPlayerDeathScreen.IsActive && ___secondPlayerDeathScreen.IsActive && HotdRemake_ArcadePlugin.GeneralCountdownTimer.TimerValue <= 0f)
                {
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player1].IsWaitingToStartPlaying = true;
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player2].IsWaitingToStartPlaying = true;
                    HotdRemake_ArcadePlugin.GeneralCountdownTimer.StopTimer();
                    Action onGameOver = HD_Player.OnGameOver;
                    if (onGameOver != null)
                    {
                        HotdRemake_ArcadePlugin.GeneralCountdownTimer.StopTimer();
                        HotdRemake_ArcadePlugin.GeneralCountdownTimer.ResetTimerValue();
                        onGameOver();
                    }
                }
            }
        }

        /// <summary>
        /// Init and stop counter
        /// </summary>
        [HarmonyPatch(typeof(HD_DeathScreenManager), "updateElementsActiveStates")]
        class updateElementsActiveStates
        {
            static bool Prefix(HD_DeathScreenManager __instance, HD_PlayerDeathScreen ___firstPlayerDeathScreen, HD_PlayerDeathScreen ___secondPlayerDeathScreen, GameObject ___timerMainGO, TMP_Text ___timerText)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_DeathScreenManager.updateElementsActiveStates()");

                if (___firstPlayerDeathScreen.IsActive)
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player1].ContinueTimer.RestartTimer();
                else
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player1].ContinueTimer.StopTimer();

                if (___secondPlayerDeathScreen.IsActive)
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player2].ContinueTimer.RestartTimer();
                else
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player2].ContinueTimer.StopTimer();


                //If both players are dead, disaplay bloody background, run general Countdown instead oh both
                if (___firstPlayerDeathScreen.IsActive && ___secondPlayerDeathScreen.IsActive)
                {
                    msetBackgroundActive.setBackgroundActive(__instance, true);
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player1].ContinueTimer.StopTimer();
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player1].ContinueTimer.TimerValue = -4.0f;//Should Hide
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player2].ContinueTimer.StopTimer();
                    HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player2].ContinueTimer.TimerValue = -4.0f;//Should Hide
                    HotdRemake_ArcadePlugin.GeneralCountdownTimer.RestartTimer();
                    
                    ___timerMainGO.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, 0.0f);
                    ___timerMainGO.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 1.0f);
                    ___timerMainGO.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                    ___timerMainGO.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
                    ___timerMainGO.GetComponent<RectTransform>().sizeDelta = new Vector2(800.0f, 800.0f); // custom size*/
                    ___timerMainGO.SetActive(true);

                    ___timerText.color = Color.white;
                    ___timerText.alignment = TextAlignmentOptions.Center;
                    ___timerText.verticalAlignment = VerticalAlignmentOptions.Middle;
                    ___timerText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                    ___timerText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                    ___timerText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
                    ___timerText.autoSizeTextContainer = false;
                    ___timerText.enableAutoSizing = false;
                    ___timerText.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);                    
                    __instance.StartCoroutine(CountDown(___timerText));
                }
                else
                {
                    msetBackgroundActive.setBackgroundActive(__instance, false);
                    HotdRemake_ArcadePlugin.GeneralCountdownTimer.StopTimer();
                    ___timerMainGO.SetActive(false);
                }

                return false;
            }
        }

        /// <summary>
        /// Updating Timer text coroutine
        /// </summary>
        public static IEnumerator CountDown(TMP_Text CountDownTimerText)
        {
            string s = HotdRemake_ArcadePlugin.GetGeneralContinueCountdownString();
            while (s != string.Empty)
            {
                CountDownTimerText.text = s;
                s = HotdRemake_ArcadePlugin.GetGeneralContinueCountdownString();
                yield return null;
            }
            CountDownTimerText.text = "";
        }

        [HarmonyPatch(typeof(HD_DeathScreenManager), "setBackgroundActive")]
        class msetBackgroundActive
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void setBackgroundActive(object instance, bool _active)
            {
                //Used to call the private method    
            }
        }
    }
}
