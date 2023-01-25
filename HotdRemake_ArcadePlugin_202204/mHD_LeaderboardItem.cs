using HarmonyLib;
using TMPro;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_LeaderboardItem
    {
        //Disabling AgentName menu on the lower side
        [HarmonyPatch(typeof(HD_LeaderboardItem), "setPartsActive")]
        class setPartsActive
        {
            static bool Prefix(bool _active, GameObject ___rankFieldMainGO, MP_Button ___button)
            {
                if (___rankFieldMainGO.activeSelf != _active)
                {
                    ___rankFieldMainGO.SetActive(_active);
                }
                if (___button.interactable != _active)
                {
                    ___button.interactable = _active;

                }
                UnityEngine.UI.ColorBlock colors = ___button.colors;
                colors.highlightedColor = Color.white;
                colors.normalColor = Color.white;
                colors.pressedColor = Color.white;
                colors.selectedColor = Color.white;
                ___button.colors = colors;

                return false;
            }
        }
    }
}
