using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_LevelCompleteMenu
    {
        /// <summary>
        /// Deactivate unwanted buttons and set text as we want
        /// Use postfix to do it after menu is displayed and text initialized
        /// </summary>
        [HarmonyPatch(typeof(HD_LevelCompleteMenu), "Open")]
        class Open
        {
            static void Postfix(HD_LevelCompleteMenu __instance)
            {
                Transform[] trs = __instance.GetComponentsInChildren<Transform>(false);

                foreach (Transform t in trs)
                {
                    if (t.name.Equals("btn_MainMenu"))
                    {
                        t.gameObject.SetActive(false);
                    }
                    else if (t.name.Equals("btn_NextLevel"))
                    {
                        MP_Button b = t.gameObject.GetComponent<MP_Button>();
                        TMPro.TMP_Text myTest = b.GetComponentInChildren<TMPro.TMP_Text>();
                        myTest.text = HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStartToSkip];
                    }
                    else if (t.name.Equals("btn_Continue"))
                    {
                        MP_Button b = t.gameObject.GetComponent<MP_Button>();
                        TMPro.TMP_Text myTest = b.GetComponentInChildren<TMPro.TMP_Text>();
                        myTest.text = HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStartToSkip];
                    }
                }
            }
        }

        /// <summary>
        /// Replacing the display of continues left (not used with arcade coin system) with continues Spent
        /// </summary>
        [HarmonyPatch(typeof(HD_LevelCompleteMenu), "setPlayerValues")]
        class setPlayerValues
        {
            static bool Prefix(HD_LevelCompleteMenu __instance)
            {
                HD_SaveDataHandler.PlaythroughState.PlayerOne.Continues = HD_SaveDataHandler.PlaythroughState.PlayerOne.ContinuesSpent;
                HD_SaveDataHandler.PlaythroughState.PlayerTwo.Continues = HD_SaveDataHandler.PlaythroughState.PlayerTwo.ContinuesSpent;
                return true;
            }
        }
    }
}
