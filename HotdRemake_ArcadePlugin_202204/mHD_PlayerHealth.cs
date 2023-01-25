using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    class mHD_PlayerHealth
    {
        /// <summary>
        /// Intercepting the event used for rumble controller to crate our own "damaged" output
        /// </summary>
        [HarmonyPatch(typeof(HD_PlayerHealth), "onBeforeDamageTaken")]
        class onBeforeDamageTaken
        {
            static bool Prefix(HD_Player ___player, HD_Player _instigator, int _receivedDamage)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_PlayerHealth.TakeDamage() => " + ___player.ToString() + " take damage : " + _receivedDamage.ToString());
                HotdRemake_ArcadePlugin.Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_Damaged + (int)___player.ThisPlayer] = 1;
                //HotdRemake_ArcadePlugin.Hotdra_Mmf.Writeall();
                HotdRemake_ArcadePlugin.Hotdra_Mmf.WriteDamageDirect((int)___player.ThisPlayer);
                return true;
            }
        }
        
    }
}
