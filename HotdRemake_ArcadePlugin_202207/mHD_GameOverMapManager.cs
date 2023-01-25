using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_GameOverMapManager
    {
        /// <summary>
        /// Replacing the display of continues left (not used with arcade coin system) with continues Spent
        /// </summary>
        [HarmonyPatch(typeof(HD_GameOverMapManager), "StopUpdatingPlayerPath")]
        class StopUpdatingPlayerPath
        {
            static void Postfix(HD_GameOverMapManager __instance)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_GameOverMapManager.StopUpdatingPlayerPath()");
                __instance.StartCoroutine(GameOverMenu_Close());
            }
        }

        /// <summary>
        /// Close GameOver screen automatically
        /// </summary>
        public static IEnumerator GameOverMenu_Close()
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_GameOverMapManager.GameOverMenu_Close() => Start coroutine");
            float Chrono = 3.0f;
            while (true)
            {
                if (Chrono <= 0)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_GameOverMapManager.GameOverMenu_Close() => Close !");
                    HotdRemake_ArcadePlugin.GameOverAnimationFinished = true;
                    break;
                }
                Chrono -= Time.deltaTime;
                yield return null;
            }
        }  
    }
}
