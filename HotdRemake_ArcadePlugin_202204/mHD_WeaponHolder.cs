﻿using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_WeaponHolder
    {
        /// <summary>
        /// Using the FireWeapon call to generate our own "Recoil" output
        /// </summary>
        [HarmonyPatch(typeof(HD_WeaponHolder), "FireWeapon")]
        class FireWeapon
        {
            static bool Prefix(Vector3 _aimPosition, HD_WeaponHolder __instance)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_WeaponHolder.FireWeapon() = > " + __instance.Owner.ThisPlayer.ToString());
                HotdRemake_ArcadePlugin.Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_Recoil + (int)__instance.Owner.ThisPlayer] = 1;
                //HotdRemake_ArcadePlugin.Hotdra_Mmf.Writeall();
                HotdRemake_ArcadePlugin.Hotdra_Mmf.WriteRecoilDirect((int)(int)__instance.Owner.ThisPlayer);
                return true;
            }
        }
    }
}
