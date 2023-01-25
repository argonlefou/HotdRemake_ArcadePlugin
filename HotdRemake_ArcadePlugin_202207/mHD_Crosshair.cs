using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_Crosshair
    {
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
