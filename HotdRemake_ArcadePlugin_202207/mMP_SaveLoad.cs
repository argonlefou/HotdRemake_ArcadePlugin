using System;
using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202207
{
    /// <summary>
    /// Replace Folder path from original game (%appdata%) or COG (MyDocuments) by NVRAM local folder
    /// </summary>
    class mMP_SaveLoad
    {
        [HarmonyPatch(typeof(MegaPixel.SaveLoadSystem.MP_SaveLoad), "PathToSaveDataDirectoryInMyDocs", MethodType.Getter)]
        class PathToSaveDataDirectoryInMyDocs
        {
            static bool Prefix(ref string __result)
            {
                __result = AppDomain.CurrentDomain.BaseDirectory + "/NVRAM";
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("MegaPixel.SaveLoadSystem.MP_SaveLoad.PathToSaveDataDirectoryInMyDocs()=> Replaced by " + __result);
                
                return false;
            }
        }

        [HarmonyPatch(typeof(MegaPixel.SaveLoadSystem.MP_SaveLoad), "FullPathToPersistentSaveData", MethodType.Getter)]
        class FullPathToPersistentSaveData
        {
            static bool Prefix(ref string __result)
            {
                __result = AppDomain.CurrentDomain.BaseDirectory + "/NVRAM/saveData.sav";
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("MegaPixel.SaveLoadSystem.MP_SaveLoadSystem.FullPathToPersistentSaveData()=> Replaced by " + __result);
                return false;
            }
        }
    }
}
