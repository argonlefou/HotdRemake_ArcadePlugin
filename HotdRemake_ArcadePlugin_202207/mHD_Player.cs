using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_Player
    {
        /// <summary>
        /// Force true as return
        /// We do not use the game credits, we are using our own implementation
        /// Not forcing this would block the game to revive a player after a fixed amount of time (and we can't buy original credits anymore)
        /// </summary>
        [HarmonyPatch(typeof(HD_Player), "CanPlayerUseContinationToken")]
        class CanPlayerUseContinationToken
        {
            static bool Prefix(ref bool __result, HD_Player __instance)
            {
                __result = true;
                return false;
            }
        }

        /// <summary>
        /// Force initial life amount based on the configurator
        /// Force putting the player in a "Playing" state, if is was on a gameover state or not
        /// </summary>
        [HarmonyPatch(typeof(HD_Player), "revive")]
        class Revive
        {
            static void Postfix(HD_Player __instance)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_Player.revive() " + __instance.ThisPlayer.ToString());
                __instance.HealthScript.SetHealth(HotdRemake_ArcadePlugin.Configurator.InitialLife);
                HotdRemake_ArcadePlugin.PluginPlayers[(int)__instance.ThisPlayer].IsWaitingToStartPlaying = false;
                FMODUnity.RuntimeManager.PlayOneShot("event:/UI/UI_pistolShot", default(Vector3));  //-> sound play 
            }
        }
    }
}
