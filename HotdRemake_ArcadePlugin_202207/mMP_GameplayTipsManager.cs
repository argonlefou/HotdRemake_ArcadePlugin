using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202207
{
    class mMP_GameplayTipsManager
    {
        /// <summary>
        /// Simply removing the differents hints that are displayed during the loading screen
        /// Some of them have nothing to do with arcade game mode
        /// </summary>
        [HarmonyPatch(typeof(MP_GameplayTipsManager), "updateVisualizersAndText")]
        class updateVisualizersAndText
        {
            static void Postfix(TMPro.TMP_Text ___textField)
            {
                ___textField.text = HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.Loading];
            }
        }
    }
}
