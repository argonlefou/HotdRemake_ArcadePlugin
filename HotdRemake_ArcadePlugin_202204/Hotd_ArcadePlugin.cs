using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace HotdRemake_ArcadePlugin_202204
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class HotdRemake_ArcadePlugin : BaseUnityPlugin
    {
        public const String pluginGuid = "argonlefou.hotdr.arcade.v2022-04";
        public const String pluginName = "House Of The Dead Remake v2022-04 - Arcade Plugin";
        public const String pluginVersion = "3.0.0.0";

        public static HotdRemake_ArcadePlugin Instance = null;
        public static ManualLogSource MyLogger;

        //MApped Memory File
        private static String MAPPED_FILE_NAME = "DemulShooter_MMF_Hotdra";
        private static String MUTEX_NAME = "DemulShooter_Mutex_Hotdra";
        private static long MAPPED_FILE_CAPACITY = 2048;
        public static Hotdra_MemoryMappedFile_Controller Hotdra_Mmf;

        //Some config informations
        public static ConfManager Configurator;
        public static int CurrentCreditsCount = 0;
        public static Dictionary<LanguageStrings.StringName, String> TextLangs;

        // Handling inputs instead of letting the game do it  
        private const int INPUTBUTTONS_LENGTH = 3;
        public enum MyInputButtons
        {
            Start = 0,
            Trigger,
            Reload
        }        

        //Continue Screen countdown timers
        public static PluginPlayer[] PluginPlayers;
        public static ContinueCountDownTimer GeneralCountdownTimer;

        //Game's main Font details
        public static TMPro.TMP_FontAsset GameMainsFont;
        public static Material GameMainFontMaterial;

        public static TMPro.TMP_Text P1_GUIText = null;
        public static TMPro.TMP_Text P2_GUIText = null;
        private Color _BlinkingStringColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color _BlinkingStringColorVisible = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        private Color _BlinkingStringColorHidden = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        //Variable to be used between HD_GameOverMenu and HD_GameOverManager to auto close path-drawing game over screen
        public static bool GameOverAnimationFinished = false;

        //Variable to be used at various place to stop START LAMP to be on during loading screen, end level screens....basically everywhere a player can"t join
        public static bool DisableStartLampFlag = false;
        private byte _StartLampEnabled = 0;

        //MainScreen hack
        public static bool IsLeaderBoardIntro = false;
        public static bool IsMainScreenLogoCoroutineRunning = false;
        public static bool CanFadeOutMusicVideo = true;

        //Use to progress in the game to develop....
        public static bool EnableCustomDevelopmentHacks = false;

        public void Awake()
        {
            MyLogger = base.Logger;
            MyLogger.LogMessage("Plugin Loaded");
            Harmony harmony = new Harmony(pluginGuid);

            MyLogger.LogMessage("Detected Application Version : " + Application.version);

            //There seem to be no way to find any clue related to the version of the game (Original version ? July update ?)
            //So the workaround is to check if the field HD_GameSettingsSaveData.ChangeBloodColor is existing, as it has been added on the update only
            bool IsCorrectGameVersion = false;
            FieldInfo settingsData = typeof(HD_SaveDataHandler).GetField("settingsData", BindingFlags.NonPublic | BindingFlags.Static);
            if (settingsData != null)
            {
                if (settingsData.FieldType.GetField("ChangeBloodColor", BindingFlags.Public | BindingFlags.Instance) == null)
                    IsCorrectGameVersion = true;
            }

            if (IsCorrectGameVersion)
            {
                MyLogger.LogMessage("Detected game version corresponding to the plugin target, starting...");

                //Need to change BepInex.cfg
                //[Preloader.Entrypoint]
                //Type = MonoBehaviour
                //Else the plugin is loaded at the end of the splash
                MyLogger.LogMessage("Skip Splash....");
                UnityEngine.Rendering.SplashScreen.Stop(UnityEngine.Rendering.SplashScreen.StopBehavior.StopImmediate);

                //Preparing new save location
                if (!System.IO.Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/NVRAM"))
                {
                    System.IO.Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + "/NVRAM");
                }

                Configurator = new ConfManager();
                TextLangs = LanguageStrings.EnglishDictionnary;

                PluginPlayers = new PluginPlayer[] { new PluginPlayer(), new PluginPlayer() };
                GeneralCountdownTimer = new ContinueCountDownTimer();

                Instance = this;

                harmony.PatchAll();

                //Using DemulShooter MMF event for single player, to export Outputs
                Hotdra_Mmf = new Hotdra_MemoryMappedFile_Controller(MAPPED_FILE_NAME, MUTEX_NAME, MAPPED_FILE_CAPACITY);
                int r = Hotdra_Mmf.MMFOpen();
                if (r == 0)
                {
                    MyLogger.LogMessage("HotdRemake_ArcadePlugin.Awake() => DemulShooter MMF opened succesfully");
                }
                else
                {
                    MyLogger.LogError("HotdRemake_ArcadePlugin.Awake() => DemulShooter MMF open error : " + r.ToString());
                }

                StartCoroutine(PlayerTextBlinkerCoroutine());
                InvokeRepeating("BlinkStartLamps", 0.0f, 0.5f);
            }
            else
            {
                MyLogger.LogMessage("Detected game version not compatible with the plugin target, destroying....");
                DestroyImmediate(this);
            }
        }


        public void Update()
        {
            //Changing the In-Game Player Text fieds
            if (P1_GUIText != null)
            {
                P1_GUIText.text = GetPlayerString(PlayerType.Player1);
                P1_GUIText.color = _BlinkingStringColor;
            }
            if (P2_GUIText != null)
            {
                P2_GUIText.text = GetPlayerString(PlayerType.Player2);
                P2_GUIText.color = _BlinkingStringColor;
            }
            
            //P1 Start
            if (Input.GetKeyDown(KeyCode.Alpha1))
                SetButton(PlayerType.Player1, MyInputButtons.Start, 1);    
            if (Input.GetKeyUp(KeyCode.Alpha1))
                SetButton(PlayerType.Player1, MyInputButtons.Start, 0); 
           
            //P2 START
            if (Input.GetKeyDown(KeyCode.Alpha2))
                SetButton(PlayerType.Player2, MyInputButtons.Start, 1);  
            if (Input.GetKeyUp(KeyCode.Alpha2))
                SetButton(PlayerType.Player2, MyInputButtons.Start, 0);

            //Quit
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();

            //CREDITS HANDLING
            if (Input.GetKeyDown(KeyCode.Alpha5) && !MP_LevelLoader.Instance.IsLoadingScreenVisible)
            {
                if (CurrentCreditsCount < 99)
                {
                    CurrentCreditsCount++;
                    PluginPlayers[(int)PlayerType.Player1].ContinueTimer.ResetTimerValue();
                    PluginPlayers[(int)PlayerType.Player2].ContinueTimer.ResetTimerValue();
                    GeneralCountdownTimer.ResetTimerValue();                    
                    FMODUnity.RuntimeManager.PlayOneShot("event:/UI/UI_achievementUnlocked", default(Vector3));  //-> sound play                      
                }
            }

            //DemulShooter's Inputs data
            int iMmf_ReadState = Hotdra_Mmf.ReadAll();
            if (iMmf_ReadState != 0)
                MyLogger.LogError("HotdRemake_ArcadePlugin.Update() => DemulShooter MMF read error : " + iMmf_ReadState.ToString());

            //MOUSE P1
            if (Configurator.InputType == 0)
            {
                if (Input.GetMouseButtonDown(0))
                    SetButton(PlayerType.Player1, MyInputButtons.Trigger, 1);
                if (Input.GetMouseButtonUp(0))
                    SetButton(PlayerType.Player1, MyInputButtons.Trigger, 0);
                if (Input.GetMouseButtonDown(1))
                    SetButton(PlayerType.Player1, MyInputButtons.Reload, 1);
                if (Input.GetMouseButtonUp(1))
                    SetButton(PlayerType.Player1, MyInputButtons.Reload, 0);

                PluginPlayers[(int)PlayerType.Player1].SetAimingValues(Input.mousePosition);
            }
            else
            {
                //For multiplayer handling, we just need to fill the PluginPlayer axis values within range [0.0f, ScreenRes] range as float
                if (iMmf_ReadState == 0)
                {  
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 vAxis = new Vector2();
                        vAxis.x = BitConverter.ToSingle(Hotdra_Mmf.Payload, (int)Hotdra_MemoryMappedFile_Controller.Payload_Inputs_Index.P1_AxisX + (8 * i));
                        vAxis.y = BitConverter.ToSingle(Hotdra_Mmf.Payload, (int)Hotdra_MemoryMappedFile_Controller.Payload_Inputs_Index.P1_AxisY + (8 * i));
                        PluginPlayers[i].SetAimingValues(vAxis);
                        SetButton((PlayerType)i, MyInputButtons.Trigger, Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Inputs_Index.P1_Trigger + i]);
                        SetButton((PlayerType)i, MyInputButtons.Reload, Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Inputs_Index.P1_Reload + i]);
                    }                    
                }
            }

            //Timers CountDown
            //Paused during cut-scenes
            if (!HD_Cutscene.IsCutsceneActive)
            {
                for (int i = 0; i < PluginPlayers.Length; i++)
                {
                    if (PluginPlayers[i].ContinueTimer.Enabled)
                    {
                        PluginPlayers[i].ContinueTimer.TimerValue -= Time.deltaTime;
                        if (PluginPlayers[i].ContinueTimer.TimerValue <= -3.0f)
                        {
                            PluginPlayers[i].ContinueTimer.StopTimer();
                            PluginPlayers[i].IsWaitingToStartPlaying = true;
                        }
                    }
                }

                if (GeneralCountdownTimer.Enabled)
                    GeneralCountdownTimer.TimerValue -= Time.deltaTime;
            }


            //Outputs
            for (int i = 0; i < 2; i++)
            {
                Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_Life + i] = 0;
                Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_Ammo + i] = 0;
                Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.Credits] = 0;

                //Health
                if (HD_Player.GetPlayer((PlayerType)i) != null)
                {
                    try
                    {
                        int h = HD_Player.GetPlayer((PlayerType)i).HealthScript.HealthCurrent;
                        if (h > 0)
                        {
                            Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_Life + i] = (byte)(h >> 1);
                            int w = HD_Player.GetPlayer((PlayerType)i).WeaponHolder.CurrentWeapon.Ammo;
                            if (w > 0)
                                Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_Ammo + i] = (byte)w;
                        }
                    }
                    catch (Exception Ex)
                    {
                        HotdRemake_ArcadePlugin.MyLogger.LogWarning("HotdRemake_ArcadePlugin.Update() => Error setting " + ((PlayerType)i).ToString() + "Health/Ammo Output :");
                        HotdRemake_ArcadePlugin.MyLogger.LogWarning(Ex.Message.ToString());
                    }
                }

                //START Lamps
                Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_StartLmp + i] = _StartLampEnabled;

                if (DisableStartLampFlag)
                {
                    Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_StartLmp + i] = 0;
                }
                else
                {
                    if (!PluginPlayers[i].IsWaitingToStartPlaying)
                    {
                        if (HD_Player.GetPlayer((PlayerType)i) != null)
                        {
                            if (!HD_Player.GetPlayer((PlayerType)i).CanBeRevived())
                            {
                                Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_StartLmp + i] = 0;
                            }
                        }
                        else
                        {
                            //Main screen = player is ready to play but classes are not created yet
                            Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_StartLmp + i] = 0;
                        }
                    }
                    if (MP_LevelLoader.Instance != null)
                    {
                        if (MP_LevelLoader.Instance.IsLoadingScreenVisible)
                        {
                            //No blinking on Loading Screen
                            Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_StartLmp + i] = 0;
                        }
                        else if (MP_LevelLoader.Instance.CurrentLevel.name.Equals("levelToLoad_Credits"))
                        {
                            //No blinking on Credits Screen
                            Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.P1_StartLmp + i] = 0;
                        }
                    }
                }

                //CREDITS
                if (!Configurator.Freeplay)
                    Hotdra_Mmf.Payload[(int)Hotdra_MemoryMappedFile_Controller.Payload_Outputs_Index.Credits] = (byte)CurrentCreditsCount;
            }
            Hotdra_Mmf.Writeall();

            //Speeding up the game.....
            if (EnableCustomDevelopmentHacks)
            {
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    Time.timeScale = Time.timeScale * 2.0f;
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("Time.timeScale: " + Time.timeScale.ToString());
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    Time.timeScale = 1.0f;
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("Time.timeScale: " + Time.timeScale.ToString());
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    MP_LevelLoader.Instance.LoadCredits();
                }
            }
        }
        

        # region Controls Handling

        public static bool GetButtonDown(PlayerType Player, MyInputButtons ButtonId)
        {
            if (PluginPlayers[(int)Player].MyInputs[(int)ButtonId] == 1 && PluginPlayers[(int)Player].MyInputsBefore[(int)ButtonId] == 0)
            {
                PluginPlayers[(int)Player].MyInputsBefore[(int)ButtonId] = 1;
                return true;
            }
            return false;
        }

        private static void SetButton(PlayerType Player, MyInputButtons ButtonId, byte Value)
        {
            PluginPlayers[(int)Player].MyInputs[(int)ButtonId] = Value; 
            if (Value == 0)
                PluginPlayers[(int)Player].MyInputsBefore[(int)ButtonId] = Value; 
        }

        public static Vector2 GetAiming(PlayerType Player)
        {
            return new Vector2(PluginPlayers[(int)Player].Axis_X, PluginPlayers[(int)Player].Axis_Y);
        }

        #endregion

        #region Player Strings information

        /// <summary>
        /// Check the game state (Freeplay, coins, etc...) to generate the strings for Players
        /// </summary>
        public static String GetScreenPlayerString(int CreditReference)
        {
            if (Configurator.Freeplay)
            {
                return (TextLangs[LanguageStrings.StringName.PressStart] + "<br>" + TextLangs[LanguageStrings.StringName.Freeplay]);
            }
            else
            {
                if (CurrentCreditsCount <= 0)
                    return (TextLangs[LanguageStrings.StringName.InsertCoins] + "<br>" + TextLangs[LanguageStrings.StringName.Credits] + " : " + CurrentCreditsCount.ToString("D2"));
                else
                {
                    if (CreditReference > 1)
                    {
                        if (CurrentCreditsCount < CreditReference)
                            return (TextLangs[LanguageStrings.StringName.InsertMoreCoins] + "<br>" + TextLangs[LanguageStrings.StringName.Credits] + " : " + CurrentCreditsCount.ToString("D2") + " / " + CreditReference.ToString("D2"));
                        else
                            return (TextLangs[LanguageStrings.StringName.PressStart] + "<br>" + TextLangs[LanguageStrings.StringName.Credits] + " : " + CurrentCreditsCount.ToString("D2") + " / " + CreditReference.ToString("D2"));
                    }
                    else
                    {
                        return (TextLangs[LanguageStrings.StringName.PressStart] + "<br>" + TextLangs[LanguageStrings.StringName.Credits] + " : " + CurrentCreditsCount.ToString("D2"));
                    }
                }
            }
        }

        /// <summary>
        /// Same thing but player based, for In-Game display
        /// </summary>
        public static String GetPlayerString(PlayerType Player)
        {
            //MyLogger.LogMessage("GetPlayerString() =============> Player" + Player.ToString());
            if (PluginPlayers[(int)Player].IsWaitingToStartPlaying)
                return GetScreenPlayerString(Configurator.CreditsToStart);
            else
                return GetScreenPlayerString(Configurator.CreditsToContinue);            
        }

        #endregion

        #region Continue countdown Timers

        //Will return int Values from 9 to 0 -> Then "Game Over"
        public static String GetPlayerContinueCountdownString(PlayerType Player)
        {
            if (PluginPlayers[(int)Player].ContinueTimer.IsVisible && !PluginPlayers[(int)Player].IsWaitingToStartPlaying)
            {
                if (PluginPlayers[(int)Player].ContinueTimer.TimerValue >= 0)
                    return ("<size=120>" + HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.Continue] + "<br><size=300>" + Math.Truncate(PluginPlayers[(int)Player].ContinueTimer.TimerValue).ToString() + "</size>");
                else
                    return HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.GameOver];
            }
            return "";
        }       
                
        //Will return int Values from 9 to 0
        public static String GetGeneralContinueCountdownString()
        {
            if (GeneralCountdownTimer.IsVisible)
            {
                if (GeneralCountdownTimer.TimerValue >= 0)
                    return ("<size=120>" + HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.Continue] + "<br><size=300>" + Math.Truncate(GeneralCountdownTimer.TimerValue).ToString() + "</size>");
            }
            return "";
        }

        #endregion

        #region Start Lamp Blinking Coroutine

        //Called by InvokeRepeating()
        private void BlinkStartLamps()
        {
            _StartLampEnabled = (byte)(1 - (int)_StartLampEnabled);
        }

        #endregion


        /// <summary>
        /// Use our own config values to overrides the game's values
        /// </summary>
        public static void SetConfigValues(MegaPixel.SaveLoadSystem.MP_SaveData LoadedData)
        {
            //Quality Settings
            LoadedData.GameSettings.CurrentResolution = new MP_ScreenResolution(Configurator.ResolutionWidth, Configurator.ResolutionHeight);
            LoadedData.GameSettings.FullScreenMode = (FullScreenMode)Configurator.FullscreenMode;
            LoadedData.GameSettings.ShadowQuality = (SettingsShadowQuality)Configurator.ShadowQuality;
            LoadedData.GameSettings.AntiAliasing = (SettingsAntiAliasing)Configurator.AntialiasingEnabled;
            LoadedData.GameSettings.BloomEnabled = Configurator.BloomEnabled;
            LoadedData.GameSettings.AmbientOcclusionEnabled = Configurator.AmbiantOcclusionEnabled;

            //Display Settings
            LoadedData.GameSettings.HideHUD = false;
            LoadedData.GameSettings.HUDScale = Configurator.HudScale;
            LoadedData.GameSettings.VSyncEnabled = Configurator.VSyncEnabled;
            LoadedData.GameSettings.Brightness = Configurator.Gamma;
            LoadedData.GameSettings.ScoreNotificationsMode = ScoreNotificationsMode.Disabled;

            //Gameplay Settings
            LoadedData.GameSettings.PlayerOneGameplaySettings.AimAssist = HD_InputParameters.AimAssistMode.None;
            LoadedData.GameSettings.PlayerTwoGameplaySettings.AimAssist = HD_InputParameters.AimAssistMode.None;
            LoadedData.GameSettings.PlayerOneGameplaySettings.UseAutoReload = false;
            LoadedData.GameSettings.PlayerTwoGameplaySettings.UseAutoReload = false;
            LoadedData.GameSettings.PlayerOneGameplaySettings.DisplayAmmoOnCrosshair = false;
            LoadedData.GameSettings.PlayerTwoGameplaySettings.DisplayAmmoOnCrosshair = false;

            //Surprisingly, the following field is not exposed like it should. Using Relfexion to access the filed and change it
            //And ! COG version does not have these fields at all !!
            if (MegaPixel.ApplicationManagement.MP_ApplicationManager.TargetPlatform != MegaPixel.ApplicationManagement.MP_ApplicationManager.GamePlatform.GOG)
            {
                LoadedData.GameSettings.PlayerOneGameplaySettings.GetType().GetField("CrosshairDisabled", BindingFlags.Instance | BindingFlags.Public).SetValue(LoadedData.GameSettings.PlayerOneGameplaySettings, Configurator.HideCrosshairs);
                LoadedData.GameSettings.PlayerTwoGameplaySettings.GetType().GetField("CrosshairDisabled", BindingFlags.Instance | BindingFlags.Public).SetValue(LoadedData.GameSettings.PlayerTwoGameplaySettings, Configurator.HideCrosshairs);
            }

            //Sound Settings
            LoadedData.GameSettings.MasterVolume = Configurator.MasterVolume;
            LoadedData.GameSettings.MusicVolume = Configurator.MusicVolume;
            LoadedData.GameSettings.SFXVolume = Configurator.SFXVolume;
            LoadedData.GameSettings.UIVolume = Configurator.UIVolume;
            LoadedData.GameSettings.DialogsVolume = Configurator.DialogsVolume;
            LoadedData.GameSettings.ShotVolume = Configurator.ShotVolume;

            //Language Settings
            LoadedData.GameSettings.CurrentLanguage = (Language)Configurator.Language;
            switch (LoadedData.GameSettings.CurrentLanguage)
            {
                case Language.Chinese:
                    TextLangs = LanguageStrings.ChineseDictionnary;break;
                case Language.French:
                    TextLangs = LanguageStrings.FrenchDictionnary;break;
                case Language.German:
                    TextLangs = LanguageStrings.GermanDictionnary;break;
                case Language.Italian:
                    TextLangs = LanguageStrings.ItalianDictionnary; break;
                case Language.Japanese:
                    TextLangs = LanguageStrings.JapaneseDictionnary; break;
                case Language.Polish:
                    TextLangs = LanguageStrings.PolishDictionnary; break;
                case Language.Russian:
                    TextLangs = LanguageStrings.RussianDictionnary; break;
                case Language.Spanish:
                    TextLangs = LanguageStrings.SpainDictionnary; break;
                default:
                    TextLangs = LanguageStrings.EnglishDictionnary;break;
            }

            //GAME Settings
            LoadedData.PlaythroughStateData.Difficulty = (Difficulty)Configurator.GameDifficulty;
            LoadedData.PlaythroughStateData.GameMode = GameMode.Original;
            LoadedData.PlaythroughStateData.IsMultiplayer = true;
            LoadedData.PlaythroughStateData.MultiplayerMode = MultiplayerMode.Competitive;
            LoadedData.PlaythroughStateData.MultiplayerScoreMode = MultiplayerScoreMode.Competitive;
            LoadedData.PlaythroughStateData.ScoreMode = ScoreMode.Classic;
            LoadedData.PlaythroughStateData.WasMultiplayerEnabled = true;

            //Empty leaderboard ?? => Easter Egg
            HD_LocalLeaderboardData.LeaderboardType LdbType = HD_SaveDataHandler.PlaythroughState.GetCurrentLeaderboardType();
            LdbType.Difficulty = Difficulty.Arcade;
            LdbType.GameMode = GameMode.Original;
            LdbType.MultiplayerMode = MultiplayerMode.Competitive;
            LdbType.ScoreMode = ScoreMode.Classic;
            HD_LocalLeaderboardData.Leaderboard Board = HD_SaveDataHandler.LocalLeaderboardData.GetLeaderboard(LdbType);
            if (Board.BestScore == 0)
            {
                MyLogger.LogMessage("mHD_AgentNameMenu.savePlayerName() => Leaderboard is EMPTY, adding an easter egg");
                HD_SaveDataHandler.LocalLeaderboardData.TryToSavePlayerResults(LdbType, 100000, "ARGONLEFOU");
            } 
        }



        private void HarmonyPatch(Harmony hHarmony, Type OriginalClass, String OriginalMethod, Type ReplacementClass, String ReplacementMethod)
        {
            MethodInfo original = AccessTools.Method(OriginalClass, OriginalMethod);
            MethodInfo patch = AccessTools.Method(ReplacementClass, ReplacementMethod);
            hHarmony.Patch(original, new HarmonyMethod(patch));
        }


        public class PluginPlayer
        {
            public ContinueCountDownTimer ContinueTimer {get; private set;}
            public bool IsWaitingToStartPlaying {get; set;}
            public byte[] MyInputs { get; set; }
            public byte[] MyInputsBefore { get; set; }

            public float Axis_X { get; private set; }
            public float Axis_Y { get; private set; }

            public PluginPlayer()
            {
                ContinueTimer = new ContinueCountDownTimer();
                IsWaitingToStartPlaying = true;
                MyInputs = new byte[INPUTBUTTONS_LENGTH];
                MyInputsBefore = new byte[INPUTBUTTONS_LENGTH];
            }

            public void SetAimingValues(Vector3 Position)
            {
                Axis_X = Position.x;
                Axis_Y = Position.y;
            }
        }
        public class ContinueCountDownTimer
        {
            public float TimerValue { get; set; }
            public bool Enabled;
            public bool IsVisible { get; set; }

            private readonly float COUNTDOWN_INIT_VALUE = 9.9f;            

            public ContinueCountDownTimer()
            {
                TimerValue = COUNTDOWN_INIT_VALUE;
                Enabled = false;
                IsVisible = false;
            }

            public void RestartTimer()
            {
                if (!Enabled)
                {
                    ResetTimerValue();
                    StartTimer();
                }
            }

            public void ResetTimerValue()
            {
                TimerValue = COUNTDOWN_INIT_VALUE;
            }

            public void StartTimer()
            {
                Enabled = true;
                IsVisible = true;
            }

            public void StopTimer()
            {
                Enabled = false;
                IsVisible = false;
            }
        }

        /// <summary>
        /// Blinking Player Text objects coroutine
        /// Running it in the plugin thread make sure they will both have the same display alpha at the same time (sync blinking)
        /// </summary>
        private IEnumerator PlayerTextBlinkerCoroutine()
        {
            float HidingDuration = 0.5f;
            float ShowingDuration = 0.5f;
            float VisibleDuration = 0.5f;
            float ElapsedTime = 0f;
            while (true)
            {
                if (_BlinkingStringColor.a == 1.0f)
                {
                    ElapsedTime = 0f;
                    while (ElapsedTime < VisibleDuration)
                    {
                        ElapsedTime += Time.deltaTime;
                        yield return null;
                    }

                    ElapsedTime = 0f;
                    while (ElapsedTime < HidingDuration)
                    {
                        ElapsedTime += Time.deltaTime;
                        _BlinkingStringColor = Color.Lerp(_BlinkingStringColorVisible, _BlinkingStringColorHidden, ElapsedTime / HidingDuration);
                        yield return null;
                    }
                }
                if (_BlinkingStringColor.a == 0.0f)
                {
                    ElapsedTime = 0f;
                    while (ElapsedTime < ShowingDuration)
                    {
                        ElapsedTime += Time.deltaTime;
                        _BlinkingStringColor = Color.Lerp(_BlinkingStringColorHidden, _BlinkingStringColorVisible, ElapsedTime / ShowingDuration);
                        yield return null;
                    }
                }
            }
        }

        #region Debug Funtions

        public static void DisplayTMP_TextInfos(TMPro.TMP_Text Text)
        {
            MyLogger.LogMessage("--- TMP_Text details ---");
            MyLogger.LogMessage("Name: " + Text.name);
            MyLogger.LogMessage("Font: " + Text.font.ToString());
            MyLogger.LogMessage("FontMaterial: " + Text.fontMaterial.ToString());
            MyLogger.LogMessage("FontStyle: " + Text.fontStyle.ToString());
            MyLogger.LogMessage("FontWeight: " + Text.fontWeight.ToString());
            MyLogger.LogMessage("Fontsize: " + Text.fontSize.ToString());
            MyLogger.LogMessage("FontSizeMax: " + Text.fontSizeMax.ToString());
            MyLogger.LogMessage("FontSizeMin: " + Text.fontSizeMin.ToString());
            MyLogger.LogMessage("Alignment: " + Text.alignment.ToString());
            MyLogger.LogMessage("EnableAutoSizing: " + Text.enableAutoSizing.ToString());
            MyLogger.LogMessage("AutoSizeTextContainer: " + Text.autoSizeTextContainer.ToString());
            DisplayTransformInfos(Text.gameObject);
        }

        public static void DisplayRootTransformInfos(GameObject Object)
        {
            Transform Root = Object.GetComponent<RectTransform>().root;
            MyLogger.LogMessage("Root Transform: " + Root.name);
            //DisplayTransformInfos(Root);
        }

        public static void DisplayTransformInfos(GameObject Object)
        {
            MyLogger.LogMessage("RectTransform>.anchoredPosition: " + Object.GetComponent<RectTransform>().anchoredPosition.ToString());
            MyLogger.LogMessage("RectTransform>.anchorMin: " + Object.GetComponent<RectTransform>().anchorMin.ToString());
            MyLogger.LogMessage("RectTransform>.anchorMax: " + Object.GetComponent<RectTransform>().anchorMax.ToString());
            MyLogger.LogMessage("RectTransform>.pivot: " + Object.GetComponent<RectTransform>().pivot.ToString());            
            MyLogger.LogMessage("RectTransform>.sizeDelta: " + Object.GetComponent<RectTransform>().sizeDelta.ToString());
            MyLogger.LogMessage("RectTransform>.position: " + Object.GetComponent<RectTransform>().position.ToString());
            MyLogger.LogMessage("RectTransform>.localPosition: " + Object.GetComponent<RectTransform>().localPosition.ToString());
            MyLogger.LogMessage("RectTransform>.localPosition: " + Object.GetComponent<RectTransform>().root.ToString());
        }

        public static void DisplayStackTrace()
        {
            StackTrace st = new StackTrace(true);
            for (int i = 0; i < st.FrameCount; i++)
            {
                // Note that high up the call stack, there is only
                // one stack frame.
                StackFrame sf = st.GetFrame(i);
                MyLogger.LogMessage("--- Stacktrace ---");
                MyLogger.LogMessage("High up the call stack, Method: " + sf.GetMethod());
                MyLogger.LogMessage("High up the call stack, Line Number: " + sf.GetFileLineNumber());
            }
        }


        #endregion
    }

    
}
