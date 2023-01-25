using HarmonyLib;


namespace HotdRemake_ArcadePlugin_202204
{
    class mMP_ApplicationManager
    {
        [HarmonyPatch(typeof(MegaPixel.ApplicationManagement.MP_ApplicationManager), "initialize")]
        class initialise
        {
            static void Postfix(MegaPixel.ApplicationManagement.MP_ApplicationManager.GamePlatform ___targetPlatform)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("MP_ApplicationManager.initialize() => TargetPlatform = " + ___targetPlatform.ToString());
            }
        }  

        /// <summary>
        /// Called after the coroutine to load the data is called at the start of the game
        /// We will insert our data here
        /// </summary>
        [HarmonyPatch(typeof(MegaPixel.ApplicationManagement.MP_ApplicationManager), "onLoadingDataFinished")]
        class onLoadingDataFinished
        {
            static bool Prefix()
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mMP_ApplicationManager.onLoadingDataFinished()");
                MegaPixel.SaveLoadSystem.MP_PCSaveLoadSystem SaveLoadSystem = MegaPixel.SaveLoadSystem.MP_PCSaveLoadSystem.Instance;
                if (SaveLoadSystem != null)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("mMP_ApplicationManager.onLoadingDataFinished() => replacing values.....");
                    HotdRemake_ArcadePlugin.SetConfigValues(SaveLoadSystem.LoadedData);
                }
                return true;
            }
        }
        
        /// <summary>
        /// Forcing Keyboard for P2
        /// </summary>
        [HarmonyPatch(typeof(MegaPixel.ApplicationManagement.MP_ApplicationManager), "SetCurrentInputDevice")]
        class SetCurrentInputDevice
        {
            static bool Prefix(int _playerID, ref InputDevice _newDevice)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mMP_ApplicationManager.SetCurrentInputDevice() => _playerID = " + _playerID + ", new dveice = " + _newDevice.ToString());

                if (_playerID == 1)
                    _newDevice = InputDevice.Keyboard;

                return true;
            }
        }
    }
}
