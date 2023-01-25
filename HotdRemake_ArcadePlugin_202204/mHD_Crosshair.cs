using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_Crosshair
    {
        /// <summary>
        /// COG release doesn't have a setting for crosshair visibility, so we must deactivate the elements 
        /// </summary>
        [HarmonyPatch(typeof(HD_Crosshair), "Awake")]
        class Awake
        {
            static void Postfix(HD_Crosshair __instance)
            {
                if (MegaPixel.ApplicationManagement.MP_ApplicationManager.TargetPlatform == MegaPixel.ApplicationManagement.MP_ApplicationManager.GamePlatform.GOG)
                {
                    if (HotdRemake_ArcadePlugin.Configurator.HideCrosshairs)
                    {
                        Transform[] trs = __instance.GetComponentsInChildren<Transform>(false);
                        foreach (Transform t in trs)
                        {
                            //HotdRemake_ArcadePlugin.MyLogger.LogMessage(t.name);
                            t.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// No need to reset position to Center !!
        /// </summary>
        [HarmonyPatch(typeof(HD_Crosshair), "ResetToCenter")]
        class ResetToCenter
        {
            static bool Prefix(int _directionX = 0)
            {
                return false;
            }
        }
                

        /// <summary>
        /// With our input system, we need to force raw window position to the game, the translation itself is the good value
        /// </summary>
        [HarmonyPatch(typeof(HD_Crosshair), "MovePositionRaw")]
        class MovePositionRaw
        {
            static bool Prefix(HD_Crosshair __instance, Vector3 _translation)
            {
                __instance.transform.position = new Vector2(0.0f, 0.0f);
                return true;
            }
            static void Postfix(HD_Crosshair __instance, Vector3 _translation)
            {
                //HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_Crosshair.MovePositionRaw() => __instance.transform.position: "  +  __instance.transform.position.ToString() + ", translation= " + _translation.ToString());
            }
        }
    }
}
