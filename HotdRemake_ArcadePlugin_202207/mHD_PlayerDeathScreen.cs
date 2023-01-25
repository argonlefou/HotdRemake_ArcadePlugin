using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_PlayerDeathScreen
    {
        /// <summary>
        /// Remove unwanted GUI items from the continue screen 
        /// </summary>
        [HarmonyPatch(typeof(HD_PlayerDeathScreen), "RedrawElement")]
        class RedrawElement
        {
            static void Postfix(HD_PlayerDeathScreen __instance, PlayerType ___owner)
            {
                TryToDisableElement("group_ScaledBottom");
                TryToDisableElement("group_ScaledTop");
            }
        }
        public static  void TryToDisableElement(string Name)
        {
            GameObject o = GameObject.Find(Name);
            if (o != null)
            {
                o.SetActive(false);
            }
        }

        /// <summary>
        /// When the Player's continue screen is changing state, we can activate again the coroutine to update the text untill
        /// the screen is deactivated again
        /// </summary>
        [HarmonyPatch(typeof(HD_PlayerDeathScreen), "SetActive")]
        class SetActive
        {
            static bool Prefix(HD_PlayerDeathScreen __instance, bool _active, out bool __state)
            {
                __state = false;
                if (__instance.IsActive != _active && _active)
                {
                    __state = true;
                }

                return true;
            }
            static void Postfix(HD_PlayerDeathScreen __instance, bool _active, PlayerType ___owner, bool __state)
            {
                if (__state)
                {
                    foreach (Transform t in __instance.transform)
                    {
                        if (t.name.Equals("txt_Continue"))
                        {
                            __instance.StartCoroutine(TimerUpdater(__instance, t.gameObject, ___owner));
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updating Timer text coroutine
        /// </summary>
        public static IEnumerator TimerUpdater(HD_PlayerDeathScreen PlayerScreen, GameObject CountDownTimer, PlayerType Player)
        {
            TMPro.TextMeshProUGUI Text = CountDownTimer.GetComponent<TMPro.TextMeshProUGUI>();
            Text.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
            while (PlayerScreen.IsActive)
            {
                Text.text = HotdRemake_ArcadePlugin.GetPlayerContinueCountdownString(Player);
                yield return null;
            }            
        }
    }
}
