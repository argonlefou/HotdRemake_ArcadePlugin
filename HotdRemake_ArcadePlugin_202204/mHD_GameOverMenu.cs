using HarmonyLib;
using Rewired;
using System.Runtime.CompilerServices;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_GameOverMenu
    {
        /// <summary>
        /// After game over, replacing the displayed text
        /// </summary>
        [HarmonyPatch(typeof(HD_GameOverMenu), "Update")]
        class Update
        {
            static bool Prefix(ref TMPro.TMP_Text ___textField)
            {
                ___textField.text = HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStartToSkip]; 
                return true;
            }
        } 

        /// <summary>
        /// Forcing our input handling : skip the scene with start button only        /// 
        /// We could just check for any start button without filtering the call, as the game is forced to multiplayer so the calling procedure will call for P2 start button even if he is dead
        /// </summary>
        [HarmonyPatch(typeof(HD_GameOverMenu), "handlePlayerInputs")]
        class handlePlayerInputs
        {
            static bool Prefix(HD_GameOverMenu __instance, Player _player, Player ___player1, Player ___player2, ref bool ___anyButtonPressed)
            {
                PlayerType pType = PlayerType.Player1;
                if (_player == ___player2)
                    pType = PlayerType.Player2;
                if (HotdRemake_ArcadePlugin.GetButtonDown(pType, HotdRemake_ArcadePlugin.MyInputButtons.Start) || HotdRemake_ArcadePlugin.GameOverAnimationFinished)
                {
                    HotdRemake_ArcadePlugin.GameOverAnimationFinished = false;
                    ___anyButtonPressed = true;
                    mhandleGameOver.handleGameOver(__instance);
                }
                return false;
            }
        }


        [HarmonyPatch(typeof(HD_GameOverMenu), "handleGameOver")]
        class mhandleGameOver
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void handleGameOver(object instance)
            {
                //Used to call the private method    
            }
        }
    }
}
