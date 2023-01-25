using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202207
{
    class mMP_SaveData
    {
        /// <summary>
        /// Overwriting SaveData constructor at the end, to replace default parameters with our own parameter from the config tool
        /// </summary>
        [HarmonyPatch(typeof(MegaPixel.SaveLoadSystem.MP_SaveData), MethodType.Constructor)]
        class MP_SaveData
        {
            static bool PostFix(MegaPixel.SaveLoadSystem.MP_SaveData __instance)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("MegaPixel.SaveLoadSystem.MP_SaveData.Ctor()");
                HotdRemake_ArcadePlugin.SetConfigValues(__instance);
                return false;
            }
        }
    }
}
