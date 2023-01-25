using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace HotdRemake_ArcadePlugin_202207
{
    public class ConfManager
    {
        //Quality Settings
        private int _ResolutionWidth = 1920;
        private int _ResolutionHeight = 1080;
        private int _FullscreenMode = 0;       //0=Exclusive fullscreen, 1=Fullscreen window, 2=Maximized window, 3=Windowed
        private int _ShadowQuality = 0;        //0=Low, 1=MEdium, 2=High
        private int _AntialiasingEnabled = 1;  //0=Off, 1=FXAA
        private int _BloomEnabled = 0;
        private int _MotionBlurEnabled = 0;
        private int _AmbiantOcclusionEnabled = 0;

        //Graphics Settings
        private int _VSyncEnabled = 0;
        private int _HudScale = 100;
        private int _Gamma = 100;
        private int _BloodColor = 0;    //0 = RED, Green = 1

        //Gameplay Settings (Works for both players)
        private int _HideCrosshairs = 0;
        private int _GameDifficulty = 3;

        //Sound Settings (0-100%)
        private int _MasterVolume = 100;
        private int _MusicVolume = 100;
        private int _SFXVolume = 100;
        private int _UIVolume = 100;
        private int _DialogsVolume = 100;
        private int _ShotVolume = 100;
        private int _AdvertiseSound = 1;

        //Language Settings(0=Unknown, 1=English, 2=Polish, 3=Japanese, 4=German, 5=Spanish, 6=Italian, 7=French, 8=Russian, 9=Chinese)
        private int _Language = 1;

        //These ones are custom        
        private int _HideCursor = 1;        
        private int _DisableMainMenu = 1;
        private int _Freeplay = 0;
        private int _CreditsToStart = 1;
        private int _CreditsToContinue = 1;
        private int _InitialLife = 4;
        private int _MaxLife = 5;
        private int _InputType = 0;    //0=Mouse (1P), 1=DemulShooter(2P)

        #region Accessors

        //Quality Settings
        public int ResolutionWidth
        { get { return _ResolutionWidth; } }
        public int ResolutionHeight
        { get { return _ResolutionHeight; } }
        public byte FullscreenMode
        { get { return (byte)_FullscreenMode; } }
        public byte ShadowQuality
        { get { return (byte)_ShadowQuality; } }
        public byte AntialiasingEnabled
        { get { return (byte)_AntialiasingEnabled; } }
        public bool BloomEnabled
        { get { return _BloomEnabled == 1 ? true : false; } }
        public bool MotionBlurEnabled
        { get { return _MotionBlurEnabled == 1 ? true : false; } }
        public bool AmbiantOcclusionEnabled
        { get { return _AmbiantOcclusionEnabled == 1 ? true : false; } }

        //Graphics Settings
        public bool VSyncEnabled
        { get { return _VSyncEnabled == 1 ? true : false; } }
        public float HudScale
        { get { return (float)_HudScale / 100.0f ; } }
        public float Gamma
        { get { return (float)_Gamma; } }

        //Gameplay Settings
        public bool HideCrosshairs
        { get { return _HideCrosshairs == 1 ? true : false; } }
        public byte GameDifficulty
        { get { return (byte)_GameDifficulty; } }
        public byte BloodColor
        { get { return (byte)_BloodColor; } }

        //Sound Settings
        public float MasterVolume
        { get { return (float)_MasterVolume / 100.0f; } }
        public float MusicVolume
        { get { return (float)_MusicVolume / 100.0f; } }
        public float SFXVolume
        { get { return (float)_SFXVolume / 100.0f; } }
        public float UIVolume
        { get { return (float)_UIVolume / 100.0f; } }
        public float DialogsVolume
        { get { return (float)_DialogsVolume / 100.0f; } }
        public float ShotVolume
        { get { return (float)_ShotVolume / 100.0f; } }
        public bool AdvertiseSound
        { get { return _AdvertiseSound == 1 ? true : false; } }

        //Language Settings
        public byte Language
        { get { return (byte)_Language; } }

        //Custom Arcade Settings
        public bool HideCursor
        { get { return _HideCursor == 1 ? true : false; } }   
        public bool DisableMainMenu
        { get { return _DisableMainMenu == 1 ? true : false; } }        
        public bool Freeplay
        { get { return _Freeplay == 1 ? true : false; } }        
        public byte CreditsToStart
        { get { return (byte)_CreditsToStart; } }
        public byte CreditsToContinue
        { get { return (byte)_CreditsToContinue; } }
        public int InitialLife
        { get { return _InitialLife * 2; } }
        public int MaxLife
        { get { return _MaxLife * 2; } }
        public int InputType
        { get { return _InputType; } }
        
        #endregion

        public ConfManager()
        {
            ReadConf(AppDomain.CurrentDomain.BaseDirectory + "\\ArcadeMod_Config.ini");
        }

        public void ReadConf(string ConfigFilePath)
        {
            try
            {
                using (StreamReader sr = new StreamReader(ConfigFilePath))
                {
                    String line = sr.ReadLine();
                    String[] buffer;
                    int i = 0;
                                
                    while (line != null)
                    {
                        if (!line.StartsWith(";"))
                        {
                            buffer = line.Split('=');
                            if (buffer.Length == 2)
                            {
                                String StrKey = buffer[0].Trim();
                                String StrValue = buffer[1].Trim();

                                if (this.GetType().GetField("_" + StrKey, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic) != null)
                                {
                                    if (int.TryParse(StrValue, out i))
                                    {
                                        this.GetType().GetField("_" + StrKey, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(this, i);
                                        HotdRemake_ArcadePlugin.MyLogger.LogMessage("ConfManager.Readconf() => _" + StrKey + "=" + i.ToString());
                                    }
                                    else
                                        HotdRemake_ArcadePlugin.MyLogger.LogError("ConfManager.Readconf() => Error parsing " + StrKey + " value in INI file : " + StrValue + " is not valid");
                                }
                                else
                                    HotdRemake_ArcadePlugin.MyLogger.LogError("ConfManager.Readconf() => Field not found : _" + StrKey);
                            }
                        }
                        line = sr.ReadLine();
                    }
                    sr.Close();
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("ConfManager.Readconf() => Configuration file succesfuly loaded");
                }
            }
            catch (Exception ex)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogError("ConfManager.Readconf() => Error reading " + ConfigFilePath + " : " + ex.Message);
            }
        }
       
    }
}
