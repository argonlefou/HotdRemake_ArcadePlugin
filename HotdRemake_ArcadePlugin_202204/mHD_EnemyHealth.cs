using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_EnemyHealth
    {
        /// <summary>
        ///The games is multiplying enemy's healt by 1.2 when playeing in multiplayer
        ///Multiplayer is forced to use the mode, so we will change the rule so that the multiplier is applied only when both players are alive
        /// </summary>
        [HarmonyPatch(typeof(HD_EnemyHealth), "getNewHealthPoints")]
        class getNewHealthPoints
        {
            static bool Prefix(HD_EnemyHealth __instance, int ___healthMax, ref int __result)
            {
                float enemyLifeMultiplier = HD_GameManager.Instance.CurrentDifficultyLevel.EnemyLifeMultiplier;
                float num = 1f;
                if (HD_SaveDataHandler.PlaythroughState.IsMultiplayer && !(__instance is HD_BossHealth))
                {
                    if (!HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player1].IsWaitingToStartPlaying && !HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player2].IsWaitingToStartPlaying)
                    {
                        num = HD_Data.MULTIPLAYER_HEALTH_MULTIPLIER;
                        HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_EnemyHealth.getNewHealthPoints() => Applying 1.2 multiplier for ennemy");
                    }
                }

                int b = (int)((float)___healthMax * enemyLifeMultiplier * num);
                __result = Mathf.Max(1, b);

                return false;
            }
        }
    }
}
