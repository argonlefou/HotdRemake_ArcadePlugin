using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_PlaythroughStateData
    {
        /// <summary>
        /// Continues are useless but values may be displayed in score mode, so we keep it to Zero (non negative...)
        /// </summary>
        [HarmonyPatch(typeof(HD_PlaythroughStateData), "DecreasePlayerContinues")]
        class DecreasePlayerContinues
        {
            static bool Prefix(PlayerType _player)
            {
                return false;
            }
        }

        /// <summary>
        /// Continues are useless but values may be displayed in score mode, so we keep it to Zero
        /// </summary>
        [HarmonyPatch(typeof(HD_PlaythroughStateData), "IncreasePlayerContinues")]
        class IncreasePlayerContinues
        {
            static bool Prefix(PlayerType _player)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(HD_PlaythroughStateData), "ResetDataWhenStartingNewGame")]
        class ResetDataWhenStartingNewGame
        {
            static void Postfix(GameMode _gameMode, ScoreMode _scoreMode, MultiplayerMode _multiplayerMode, Difficulty _difficulty, HD_PlaythroughStateData __instance)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_PlaythroughStateData.ResetDataWhenStartingNewGame() Postfix");
                __instance.IsMultiplayer = true;
                __instance.WasMultiplayerEnabled = true;                
                MegaPixel.SaveLoadSystem.MP_SaveLoad.Save(__instance, null, true, 1f);
            }
        }

        [HarmonyPatch(typeof(HD_PlaythroughStateData.PlayerData), "SetDefault")]
        class SetDefault
        {
            static bool Prefix(PlayerCharacterType _character, ref int _lives, ref int _continues)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_PlaythroughStateData.SetDefault()");
                _lives = HotdRemake_ArcadePlugin.Configurator.InitialLife;
                _continues = 0;
                return true;
            }
        }
    }
}
