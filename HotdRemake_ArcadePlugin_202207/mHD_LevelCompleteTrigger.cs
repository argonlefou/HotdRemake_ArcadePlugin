using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_LevelCompleteTrigger
    { 
        /// <summary>
        /// Adding a coroutine and timer to automatically pass the level complete menu after a few seconds or with START button
        /// For the last level finished, "Next Level" button is not displayed, but only "Continue" button
        /// </summary>
        [HarmonyPatch(typeof(HD_LevelCompleteTrigger), "Show")]
        class Show
        {
            static bool Prefix(HD_LevelCompleteTrigger __instance, bool ___triggered, HD_LevelCompleteMenu ___menu)
            {
                if (!___triggered)
                {
                    //Stopping Start Lamp during the end level screen
                    HotdRemake_ArcadePlugin.DisableStartLampFlag = true;

                    Transform[] trs = ___menu.GetComponentsInChildren<Transform>(false);
                    foreach (Transform t in trs)
                    {
                        if (t.name.Equals("btn_MainMenu"))
                        {
                            t.gameObject.SetActive(false);
                        }
                        else if (t.name.Equals("btn_NextLevel") || t.name.Equals("btn_Continue"))
                        {
                            MP_Button b = t.gameObject.GetComponent<MP_Button>();
                            UnityEngine.UI.ColorBlock colors = b.colors;
                            colors.highlightedColor = Color.white;
                            colors.normalColor = Color.white;
                            colors.pressedColor = Color.white;
                            colors.selectedColor = Color.white;
                            b.colors = colors;
                            __instance.StartCoroutine(LevelCompleteMenu_LoadNextLevel(__instance, b));
                        }                        
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Load next level
        /// </summary>
        public static IEnumerator LevelCompleteMenu_LoadNextLevel(HD_LevelCompleteTrigger Menu, MP_Button Button)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_LevelCompleteTrigger.LevelCompleteMenu_LoadNextLevel() => Start coroutine");
            float Chrono = 10.0f;
            while (true)
            {
                bool bFlagP1 = HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player1, HotdRemake_ArcadePlugin.MyInputButtons.Start) && HD_PlayerCombatManager.Instance.GetPlayerState(PlayerType.Player1) == HD_PlayerCombatManager.PlayerState.Up;
                bool bFlagP2 = HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player2, HotdRemake_ArcadePlugin.MyInputButtons.Start) && HD_PlayerCombatManager.Instance.GetPlayerState(PlayerType.Player2) == HD_PlayerCombatManager.PlayerState.Up;
                if (bFlagP1 || bFlagP2 || Chrono <= 0)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_LevelCompleteTrigger.LevelCompleteMenu_LoadNextLevel() => Done !");
                    //Reactivating Start Lamp
                    HotdRemake_ArcadePlugin.DisableStartLampFlag = false;
                    Button.onClick.Invoke();
                    break;
                }                
                Chrono -= Time.deltaTime;
                yield return null;
            }
        }        
    }
}
