using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202207
{
    class mMP_Button
    {
        /// <summary>
        /// Making Buttons (in LEaderboard, or in screens, appear like simple text
        /// </summary>
        [HarmonyPatch(typeof(MP_Button), "OnSelect")]
        class OnSelect
        {
            static bool Prefix(UnityEngine.EventSystems.BaseEventData _eventData)
            {
                return false;
            }
        }
    }
}
