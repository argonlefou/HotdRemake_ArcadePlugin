using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace HotdRemake_ArcadePlugin_202207
{
    /// <summary>
    /// Remove the "Skip Text Mouse icon and loading bar", as the skip is direct on START press
    /// Adding a "Press Start To Skip" text for each player
    /// </summary>
    class mHD_CutsceneUI
    {
        [HarmonyPatch(typeof(HD_CutsceneUI), "Awake")]
        class Awake
        {
            static void Postfix(HD_CutsceneUI __instance, MP_BlinkingTextField ___skipBlinkingText, TMPro.TMP_Text ___skipHint)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_CutsceneUI.Awake()");

                ___skipHint.text = "<size=30>" + HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStartToSkip] + "</size>";
                ___skipHint.color = Color.white;
                ___skipHint.verticalAlignment = TMPro.VerticalAlignmentOptions.Top;
                //___skipHint.autoSizeTextContainer = true;
                //___skipHint.enableAutoSizing = true;
                MP_BlinkingTextField t1 = UnityEngine.Object.Instantiate(___skipBlinkingText, ___skipBlinkingText.transform.parent, false);
                t1.name = "P1_PressStart";
                t1.transform.GetComponent<TMPro.TMP_Text>().alignment = TMPro.TextAlignmentOptions.Left;
                t1.GetComponent<RectTransform>().anchorMin = new Vector2(0.0f, 1.0f);  //Top Left
                t1.GetComponent<RectTransform>().anchorMax = new Vector2(0.0f, 1.0f);
                t1.GetComponent<RectTransform>().position = new Vector3(30.0f, Screen.height + __instance.transform.Find("panel_Top").GetComponent<RectTransform>().sizeDelta.y - 30.0f, t1.GetComponent<RectTransform>().position.z);
                t1.GetComponent<RectTransform>().pivot = new Vector2(0, 1.0f);
                t1.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ___skipHint.GetPreferredValues().x);
                t1.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ___skipHint.GetPreferredValues().y);
                t1.transform.parent = __instance.transform.Find("panel_Top");

                MP_BlinkingTextField t2 = UnityEngine.Object.Instantiate(___skipBlinkingText, ___skipBlinkingText.transform.parent, false);
                t2.name = "P2_PressStart";
                t2.transform.GetComponent<TMPro.TMP_Text>().alignment = TMPro.TextAlignmentOptions.Right;
                t2.GetComponent<RectTransform>().anchorMin = new Vector2(1.0f, 1.0f);  //Top Left
                t2.GetComponent<RectTransform>().anchorMax = new Vector2(1.0f, 1.0f);
                t2.GetComponent<RectTransform>().position = new Vector3(Screen.width - ___skipHint.preferredWidth - 30.0f, Screen.height + __instance.transform.Find("panel_Top").GetComponent<RectTransform>().sizeDelta.y - 30.0f, t1.GetComponent<RectTransform>().position.z);
                t2.GetComponent<RectTransform>().pivot = new Vector2(0, 1.0f);
                t2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ___skipHint.GetPreferredValues().x);
                t2.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ___skipHint.GetPreferredValues().y);
                t2.transform.parent = __instance.transform.Find("panel_Top");                
            }
        }

        //Removing old Sprite / Text (unused now)
        [HarmonyPatch(typeof(HD_CutsceneUI), "updateSkipHintText")]
        class updateSkipHintText
        {
            static bool Prefix(TMPro.TMP_Text ___skipHint)
            {
                ___skipHint.text = ""; //PRESS START TO SKIP
                return false;
            }
        }        

        //Disabling also newer UI elements we created
        [HarmonyPatch(typeof(HD_CutsceneUI), "disableAllUIElements")]
        class disableAllUIElements
        {
            static bool Prefix(HD_CutsceneUI __instance)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_CutsceneUI.disableAllUIElements()");
                Transform[] trs = __instance.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in trs)
                {                       
                    if (t.name.Equals("P2_PressStart") || t.name.Equals("P1_PressStart"))
                    {
                        t.gameObject.SetActive(false);                        
                    }
                }
                return true;
            }
        }

        //Enabling newer UI elements we created
        //Force display of all elements on UI cutscene screen : visibility will be settled on the Update loop to allow player joining during cut scene
        [HarmonyPatch(typeof(HD_CutsceneUI), "displayHoldTexts")]
        class displayHoldTexts
        {
            static bool Prefix(HD_CutsceneUI __instance, MP_BlinkingTextField ___skipBlinkingText, TMPro.TMP_Text ___skipHint, TMPro.TMP_Text ___holdTextP1, TMPro.TMP_Text ___holdTextP2)
            { 
                //Enabling HOLD TRIGGER text always
                ___holdTextP1.SetColorAlpha(0f);
		        ___holdTextP1.gameObject.SetActive(true);
                ___holdTextP2.SetColorAlpha(0f);
                ___holdTextP2.gameObject.SetActive(true);

                //Adding our custom text fields
                Transform[] trs = __instance.GetComponentsInChildren<Transform>(true);
                int nFound = 0;
                foreach (Transform t in trs)
                {
                    if (t.name.Equals("P1_PressStart") || t.name.Equals("P2_PressStart"))
                    {
                        t.gameObject.SetActive(true);
                        TMPro.TMP_Text myText = t.gameObject.GetComponentInChildren<TMPro.TMP_Text>();
                        myText.canvasRenderer.SetAlpha(0.0f);
                        nFound++;
                        if (nFound == 2)
                            break;
                    }
                }

                return false;
            }
        }

        //Enabling newer UI elements we created
        //And select whether "SKIP" text will be displayed according to the player state and the skipping state of the Cutscene
        [HarmonyPatch(typeof(HD_CutsceneUI), "Update")]
        class Update
        {
            static bool Prefix(HD_CutsceneUI __instance, MP_BlinkingTextField ___skipBlinkingText, TMPro.TMP_Text ___skipHint, TMPro.TMP_Text ___holdTextP1, TMPro.TMP_Text ___holdTextP2)
            {
                GameObject oP1_Text = GameObject.Find("P1_PressStart");
                if (oP1_Text != null)
                {
                    TMPro.TMP_Text myText = oP1_Text.GetComponentInChildren<TMPro.TMP_Text>();
                    if (HD_PlayerCombatManager.Instance.GetPlayerState(PlayerType.Player1) == HD_PlayerCombatManager.PlayerState.Up)
                    {
                        myText.canvasRenderer.SetAlpha(HD_Cutscene.CanSkip() ? 1.0f : 0.0f);
                        ___holdTextP1.canvasRenderer.SetAlpha(1.0f);
                    }
                    else
                    {
                        myText.canvasRenderer.SetAlpha(0.0f);
                        ___holdTextP1.canvasRenderer.SetAlpha(0.0f);
                    }
                }

                GameObject oP2_Text = GameObject.Find("P2_PressStart");
                if (oP2_Text != null)
                {
                    TMPro.TMP_Text myText = oP2_Text.GetComponentInChildren<TMPro.TMP_Text>();
                    if (HD_PlayerCombatManager.Instance.GetPlayerState(PlayerType.Player2) == HD_PlayerCombatManager.PlayerState.Up)
                    {
                        myText.canvasRenderer.SetAlpha(HD_Cutscene.CanSkip() ? 1.0f : 0.0f);
                        ___holdTextP2.canvasRenderer.SetAlpha(1.0f);
                    }
                    else
                    {
                        myText.canvasRenderer.SetAlpha(0.0f);
                        ___holdTextP2.canvasRenderer.SetAlpha(0.0f);
                    }
                }
                return true;
            }
        }
    }       
}
