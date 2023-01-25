using System.Runtime.CompilerServices;
using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    /// <summary>
    /// Removing the Controller Disconnected popup before start of a level, forcing players to play
    /// </summary>
    class mMP_ControllerdisconnectedPopup
    {
        [HarmonyPatch(typeof(MP_ControllerDisconnectedPopup), "UpdatePopupState")]
        class UpdatePopupState
        {
            static bool Prefix(MP_ControllerDisconnectedPopup __instance)
            {
                msetDisabled.setDisabled(__instance);
                return false;
            }
        }

        [HarmonyPatch(typeof(MP_ControllerDisconnectedPopup), "setDisabled")]
        class msetDisabled
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void setDisabled(object instance)
            {
                //Used to call the private method GameStart.EnterGame()      
            }
        }      
    }
}
