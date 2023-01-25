using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_Health
    {
        /// <summary>
        /// Blocking Life increase after max value is reached
        /// </summary>
        [HarmonyPatch(typeof(HD_Health), "IncreaseHealth")]
        class IncreaseHealth
        {
            static bool Prefix(HD_Health __instance, ref int _value)
            {
                if (__instance is HD_PlayerHealth)
                {
                    if (__instance.HealthCurrent >= HotdRemake_ArcadePlugin.Configurator.MaxLife)
                        _value = 0;
                }
                return true;
            }
        }
        
    }
}
