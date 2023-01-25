using HarmonyLib;
using TMPro;
using UnityEngine.UI;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_SkipView
    {

        /// <summary>
        /// Setting our text at start
        /// </summary>
        [HarmonyPatch(typeof(HD_SkipView), "Awake")]
        class Awake
        {
            static void Postfix(TMP_Text ___skipHint, MP_BlinkingTextField ___skipBlinkingText, Image ___skipIndicatorImage)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_SkipView.Awake()");
                ___skipHint.text = HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStartToSkip];
                ___skipHint.color = UnityEngine.Color.white;
                ___skipHint.alpha = 1.0f;
                ___skipBlinkingText.Graphic.SetColorAlpha(1.0f);
                ___skipBlinkingText.enabled = true;
                ___skipIndicatorImage.SetColorAlpha(0.0f);
            }
        }

        /// <summary>
        /// Blocking initial disabling of GUI elements
        /// Setting Progressbar to trnapsarent color
        /// </summary>
        [HarmonyPatch(typeof(HD_SkipView), "OnEnable")]
        class OnEnable
        {
            static bool Prefix()
            {
                return false;
            }
        }

        /// <summary>
        /// Setting our text instead
        /// </summary>
        [HarmonyPatch(typeof(HD_SkipView), "updateSkipHintText")]
        class updateSkipHintText
        {
            static bool Prefix(TMP_Text ___skipHint)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_SkipView.updateSkipHintText()");
                ___skipHint.text = HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStartToSkip];
                return false;
            }
        }

        /// <summary>
        /// On that version of the game, this is called in a loop even if the HD_SkipController update loop is patched (??)
        /// So we need to patch it here too, overwise the text is always hidden
        /// </summary>
        [HarmonyPatch(typeof(HD_SkipView), "HideHint")]
        class HideHint
        {
            static bool Prefix(TMP_Text ___skipHint)
            {
                //HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_SkipView.HideHint()");
                return false;
            }
        }
    }
}
