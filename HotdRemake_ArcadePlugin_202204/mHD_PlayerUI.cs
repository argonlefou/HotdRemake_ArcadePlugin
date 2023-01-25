using HarmonyLib;
using UnityEngine;
using TMPro;
using System.Reflection;
using System.Collections;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_PlayerUI
    {
        /// <summary>
        /// Creating our TMP_Text object to display player-related info (credits, continue, press start....)
        /// </summary>
        [HarmonyPatch(typeof(HD_PlayerUI), "Awake")]
        class spawnHealthTorches
        {
            static bool Prefix(HD_PlayerUI __instance, TMP_Text ___noAmmoText)
            { 
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_PlayerUI.Awake() => ");

                TMP_Text myText = UnityEngine.Object.Instantiate(___noAmmoText, __instance.transform, false);
                RectTransform TextTransform = myText.GetComponent<RectTransform>();

                Transform[] trs = __instance.GetComponentsInChildren<Transform>(true);    
                foreach (Transform t in trs)
                {
                    if (t.name.Equals("PlayerUI_Player1"))
                    {                        
                        myText.name = "P1_InGameInfoText";
                        TextTransform.anchorMin = new Vector2(0.0f, 0.0f);
                        TextTransform.anchorMax = new Vector2(0.0f, 0.0f);
                        TextTransform.pivot = new Vector2(0.0f, 0.0f);
                        TextTransform.anchoredPosition = new Vector2(0f, 0f);
                        HotdRemake_ArcadePlugin.P1_GUIText = myText;    //--> Dirty method to update it in the plugin Update() loop    
                        break;
                    }
                    else if (t.name.Equals("PlayerUI_Player2"))
                    {
                        myText.name = "P2_InGameInfoText";
                        TextTransform.anchorMin = new Vector2(1.0f, 0f);
                        TextTransform.anchorMax = new Vector2(1.0f, 0f);
                        TextTransform.pivot = new Vector2(1.0f, 0f);
                        TextTransform.anchoredPosition = new Vector2(0f, 0f);
                        HotdRemake_ArcadePlugin.P2_GUIText = myText;    //--> Dirty method to update it in the plugin Update() loop 
                        break;
                    }
                }
                myText.color = Color.white;
                myText.fontSize = 45;
                myText.fontSizeMin = 20;
                myText.fontSizeMax = 60;
                myText.enableAutoSizing = false;
                myText.autoSizeTextContainer = false;
                myText.font = HotdRemake_ArcadePlugin.GameMainsFont;
                myText.fontMaterial = HotdRemake_ArcadePlugin.GameMainFontMaterial;
                myText.horizontalAlignment = HorizontalAlignmentOptions.Center;

                return true;
            }
        }


        /// <summary>
        /// Blinking Player Text objects
        /// </summary>
        public static IEnumerator PlayerTextBlinkerCoroutine(TMP_Text PlayerText)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("PlayerTextBlinkerCoroutine()=> Starting coroutine for " + PlayerText.name);
            while (true)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("PlayerTextBlinkerCoroutine()=> " + PlayerText.name + " Alpha: " + PlayerText.alpha);
                PlayerText.alpha = 1.0f - PlayerText.alpha;
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("PlayerTextBlinkerCoroutine()=> " + PlayerText.name + " Alpha: " + PlayerText.alpha);
                yield return new WaitForSeconds(1);
            }
        }

        /// <summary>
        /// Updating Player Text objects
        /// </summary>
        public static IEnumerator Player1TextUpdaterCoroutine(TMP_Text PlayerText)
        {
            while (true)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_PlayerUI.Player1TextUpdaterCoroutine()");            
                PlayerText.text = HotdRemake_ArcadePlugin.GetPlayerString(PlayerType.Player1);
                yield return 0; //wait for 1 Frame
            }
        }
        public static IEnumerator Player2TextUpdaterCoroutine(TMP_Text PlayerText)
        {
            while (true)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_PlayerUI.Player2TextUpdaterCoroutine()");
                PlayerText.text = HotdRemake_ArcadePlugin.GetPlayerString(PlayerType.Player2);
                yield return 0; //wait for 1 Frame
            }
        }

        /// <summary>
        /// Force hiding score panel, like original arcade
        /// </summary>
        [HarmonyPatch(typeof(HD_PlayerUI), "setScoreDisplayUIActive")]
        class setScoreDisplayUIActive
        {
            static bool Prefix(ref bool _active)
            {
                _active = false;
                return true; ;
            }
        }

        /// <summary>
        /// Force hiding score panel, like original arcade
        /// </summary>
        [HarmonyPatch(typeof(HD_PlayerUI), "updateWeaponText")]
        class updateWeaponText
        {
            static bool Prefix(ref string _name)
            {
                _name = "";
                return true; ;
            }
        }        

        /// <summary>
        /// Activating / Deactivating the display of players String 
        /// to show them only when the player is dead
        /// </summary>
        [HarmonyPatch(typeof(HD_PlayerUI), "onHealthChange")]
        class RedrawElement
        {
            static bool Prefix(HD_PlayerUI __instance, HD_Player ___owner, HD_Health _health, int _diff)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_PlayerUI.onHealthChange() => Player " + ___owner.ThisPlayer);
                if (___owner != null )
                {
                   
                    if (___owner.ThisPlayer == PlayerType.Player1)
                    {
                        foreach (Transform t in __instance.transform)
                        {
                            if (t.name.Equals("P1_InGameInfoText"))
                            {
                                if (_health.HealthCurrent <= 0)
                                    t.gameObject.SetActive(true);
                                else
                                    t.gameObject.SetActive(false);
                                break;
                            }
                        }
                    }
                    else if (___owner.ThisPlayer == PlayerType.Player2)
                    {
                        foreach (Transform t in __instance.transform)
                        {
                            if (t.name.Equals("P2_InGameInfoText"))
                            {
                                if (_health.HealthCurrent <= 0)
                                    t.gameObject.SetActive(true);
                                else
                                    t.gameObject.SetActive(false);
                                break;
                            }
                        }
                    }
                }
                return true;
            }
        }
    }
}
