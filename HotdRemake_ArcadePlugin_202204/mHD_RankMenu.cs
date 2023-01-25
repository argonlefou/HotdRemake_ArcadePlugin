using HarmonyLib;
using UnityEngine;
using System.Collections;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_RankMenu
    {
        /// <summary>
        /// Remove Menu at the bottom of the page, and auto validate to the next screen after a short period of time
        /// </summary>
        [HarmonyPatch(typeof(HD_RankMenu), "Awake")]
        class Awake
        {
            static void Postfix(HD_RankMenu __instance)
            {
                //Stopping Start Lamp during the end game screens
                HotdRemake_ArcadePlugin.DisableStartLampFlag = true;

                Transform[] trs = __instance.GetComponentsInChildren<Transform>(false);
                foreach (Transform t in trs)
                {
                    if (t.name.Equals("img_MenuBackground"))
                    {
                        t.gameObject.SetActive(false);
                    }
                    else if (t.name.Equals("btn_Continue"))
                    {
                        MP_Button b = t.gameObject.GetComponent<MP_Button>();                        
                        TMPro.TMP_Text myTest = b.GetComponentInChildren<TMPro.TMP_Text>();
                        myTest.text = HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStartToSkip];
                        myTest.SetColorAlpha(0.0f);
                        __instance.StartCoroutine(RankMenu_Continue(__instance, b));
                    }
                }
                
            }
        }

        /// <summary>
        /// Close RankMenu screen and go to loeaderboard
        /// </summary>
        public static IEnumerator RankMenu_Continue(HD_RankMenu Menu, MP_Button Button)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_RankMenu.RankMenu_Continue() => Start coroutine");
            float Chrono = 6.0f;
            while (true)
            {
                if (Chrono <= 0)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_RankMenu.RankMenu_Continue() => Finished");
                    Button.onClick.Invoke();
                    break;
                }
                Chrono -= Time.deltaTime;
                yield return null;
            }
        }        
    }
}
