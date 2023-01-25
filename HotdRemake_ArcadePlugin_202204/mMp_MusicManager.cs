using HarmonyLib;

namespace HotdRemake_ArcadePlugin_202204
{
    /// <summary>
    /// Removing music and sound effect during attract mode
    /// Need also to patch the call to PlayMusic() from MainMovieController (because our mod is running different music, whereas initial game just ran main music from here and never stop)
    /// </summary>
    class mMP_MusicManager
    {

        [HarmonyPatch(typeof(MP_MusicManager), "PlayAdditionalMusic")]
        class PlayAdditionalMusic
        {
            static bool Prefix(ref AdditionalMusicType _type, bool _forceReset = false)
            {
                if (!HotdRemake_ArcadePlugin.Configurator.AdvertiseSound)
                    _type = AdditionalMusicType.None;
                return true;
            }
        }

        [HarmonyPatch(typeof(MP_MusicManager), "PlayMusic")]
        class PlayMusic
        {
            static bool Prefix(ref MusicType _musicType, bool _forceReset = false)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mMP_MusicManager.PlayMusic() => Musictype: " + _musicType.ToString());
                if (_musicType == MusicType.MainMenuMusic && !HotdRemake_ArcadePlugin.Configurator.AdvertiseSound)
                {
                    _musicType = MusicType.None;
                }
                return true;
            }
        }
    }
}
