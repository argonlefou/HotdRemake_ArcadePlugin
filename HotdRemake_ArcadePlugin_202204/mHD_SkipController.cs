using HarmonyLib;
using System.Runtime.CompilerServices;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_SkipController
    {
        /// <summary>
        /// Skip as soon as the player press the Start button
        /// </summary>
        [HarmonyPatch(typeof(HD_SkipController), "Update")]
        class Update
        {
            static bool Prefix(HD_SkipController __instance, HD_SkipView ___view, ref float ___skipTimer, float ___skipDuration)
            {
                for (int i = 0; i < 2; i++)
                {
                    if (HotdRemake_ArcadePlugin.GetButtonDown((PlayerType)i, HotdRemake_ArcadePlugin.MyInputButtons.Start))
                    {
                        if (HD_PlayerCombatManager.Instance != null)
                        {
                            if (HD_PlayerCombatManager.Instance.GetPlayerState((PlayerType)i) == HD_PlayerCombatManager.PlayerState.Up)
                            {
                                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_SkipController.Update() => " + (PlayerType)i + " skipping");
                                ___skipTimer = ___skipDuration;
                                mchangeSkipTimer.changeSkipTimer(__instance, 0);
                                ___view.HideHint(___skipTimer);
                            }
                        }
                        else
                        {
                            ___skipTimer = ___skipDuration;
                            mchangeSkipTimer.changeSkipTimer(__instance, 0);
                            ___view.HideHint(___skipTimer);  
                        }
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(HD_SkipController), "changeSkipTimer")]
        class mchangeSkipTimer
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void changeSkipTimer(object instance, float time)
            {
                //Used to call the private method    
            }
        }
    }
}
