using HarmonyLib;
using MegaPixel.ApplicationManagement;

namespace HotdRemake_ArcadePlugin_202207
{
    class mMP_CursorSetterModule
    {
        [HarmonyPatch(typeof(MP_CursorSetterModule), "changeCursorLockMode")]
        class changeCursorLockMode
        {
            static bool Prefix(ref UnityEngine.CursorLockMode _cursorLockMode)
            {
                //HotdRemake_ArcadePlugin.MyLogger.LogMessage("MP_CursorSetterModule.changeCursorLockMode(" + _cursorLockMode + ")");
                _cursorLockMode = UnityEngine.CursorLockMode.None;
                return true;
            }
        }

        /// <summary>
        /// Force hiding cursor
        /// </summary>
        [HarmonyPatch(typeof(MP_CursorSetterModule), "changeCursorVisibility")]
        class changeCursorVisibility
        {
            static bool Prefix(ref bool _shouldBeVisible)
            {
                //HotdRemake_ArcadePlugin.MyLogger.LogMessage("MP_CursorSetterModule.changeCursorVisibility(" + _shouldBeVisible + ")");
                if (HotdRemake_ArcadePlugin.Configurator.HideCursor)
                    _shouldBeVisible = false;
                return true;
            }
        }
    }
}
