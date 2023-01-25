using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    /// <summary>
    /// This object is the "Press Any Button" Text at Title screen, and has button event attach to display the main menu (!)
    /// So....no more event, no more menu
    /// </summary>
    class mHD_PressAnyKeyDisplay
    {
        [HarmonyPatch(typeof(HD_PressAnyKeyDisplay), "OnEnable")]
        class OnEnable
        {
            static bool Prefix()
            {
                if (HotdRemake_ArcadePlugin.Configurator.DisableMainMenu)
                    return false;
                return true;
            }
        }

        [HarmonyPatch(typeof(HD_PressAnyKeyDisplay), "OnDisable")]
        class OnDisable
        {
            static bool Prefix()
            {
                if (HotdRemake_ArcadePlugin.Configurator.DisableMainMenu)
                    return false;
                return true;
            }
        }
    }
}
