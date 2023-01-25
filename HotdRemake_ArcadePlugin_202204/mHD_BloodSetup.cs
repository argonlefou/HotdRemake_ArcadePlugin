using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_BloodSetup
    {
        /// <summary>
        /// Changing blood color
        /// </summary>
        [HarmonyPatch(typeof(HD_BloodSetup), "GetBloodColor", new[] {typeof(BloodColor)})]
        class GetBloodColor
        {
            static bool Prefix(ref UnityEngine.Color __result, ref BloodColor _color)
            {
                if (HotdRemake_ArcadePlugin.Configurator.BloodColor == 1)
                    _color = BloodColor.WithoutRed;
                else if (HotdRemake_ArcadePlugin.Configurator.BloodColor == 0)
                    _color = BloodColor.Default;
                return true;
            }
        }
    }
}
