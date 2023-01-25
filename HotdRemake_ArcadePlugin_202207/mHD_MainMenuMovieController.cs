using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HotdRemake_ArcadePlugin_202207
{
    /// <summary>
    /// This class handles the intro movie/screen
    /// Lots of thing to mod here:
    /// - Add a Title Logo (and show/hide it when necessary)
    /// - Add Player text info (credits, etc...)
    /// - Block Menu (done in mPressAnyKeyDisplay class)
    /// - Start game directly when P1 or P2 start is pressed
    /// </summary>
    class mHD_MainMenuMovieController
    {
        [HarmonyPatch(typeof(HD_MainMenuMovieController), "Awake")]
        class Awake
        {
            static bool Prefix(HD_MainMenuMovieController __instance, HD_PressAnyKeyDisplay ___pressAnyKeyDisplay)
            {
                //Creating the Ranking background image/texture
                //This will be displayed between the 2 videos, with the Ranking
                GameObject RankingBackgroundObject = new GameObject("RankingBackground");
                RectTransform RankingBackgroundTransform = RankingBackgroundObject.AddComponent<RectTransform>();
                RankingBackgroundTransform.transform.SetParent(__instance.transform); // setting parent
                RankingBackgroundTransform.localScale = Vector3.one;
                RankingBackgroundTransform.pivot = new Vector2(0.5f, 0.5f);      //
                RankingBackgroundTransform.anchorMin = new Vector2(0.5f, 0.5f);  //Upper Middle
                RankingBackgroundTransform.anchorMax = new Vector2(0.5f, 0.5f);  //
                RankingBackgroundTransform.anchoredPosition = new Vector2(0.0f, 0.0f); // setting position, will be a little bit below upper border window        

                Texture2D tDrawTexture = new Texture2D(2, 2);
                byte[] bBuffer = Properties.Resources.ranking_backgound;
                tDrawTexture.LoadImage(bBuffer);
                tDrawTexture.name = "Texture_RankingBackground";

                Image imgTypeText = RankingBackgroundObject.AddComponent<Image>();
                imgTypeText.sprite = Sprite.Create(tDrawTexture, new Rect(0, 0, tDrawTexture.width, tDrawTexture.height), new Vector2(0.5f, 0.5f));
                //imgTypeText.color = Color.yellow;
                imgTypeText.SetColorAlpha(0.0f);
                RankingBackgroundTransform.sizeDelta = new Vector2(tDrawTexture.width, tDrawTexture.height); // custom size
                RankingBackgroundObject.SetActive(true);


                //Creating the Title Game Logo Image/Texture
                GameObject GameLogo = new GameObject("GameLogo");
                RectTransform GameLogoTransform = GameLogo.AddComponent<RectTransform>();
                GameLogoTransform.transform.SetParent(__instance.transform); // setting parent
                GameLogoTransform.localScale = Vector3.one;
                GameLogoTransform.pivot = new Vector2(0.5f, 1.0f);      //
                GameLogoTransform.anchorMin = new Vector2(0.5f, 1.0f);  //Upper Middle
                GameLogoTransform.anchorMax = new Vector2(0.5f, 1.0f);  //
                GameLogoTransform.anchoredPosition = new Vector2(0f, -25.0f); // setting position, will be a little bit below upper border window                

                Texture2D DrawTexture = new Texture2D(2, 2);
                byte[] Buffer = Properties.Resources.hod_remake_logo_1;
                DrawTexture.LoadImage(Buffer);
                DrawTexture.name = "Texture_GameLogo";

                Image img = GameLogo.AddComponent<Image>();
                img.sprite = Sprite.Create(DrawTexture, new Rect(0, 0, DrawTexture.width, DrawTexture.height), new Vector2(0.5f, 0.5f));
                //img.color = Color.yellow;
                GameLogo.transform.SetParent(__instance.transform);
                img.SetColorAlpha(0.0f);
                GameLogoTransform.sizeDelta = new Vector2(DrawTexture.width, DrawTexture.height); // custom size
                

                //Small changes to the BlinkingText object reused for our Player strings
                TMP_Text myText = (TMP_Text)___pressAnyKeyDisplay.GetType().GetField("textField", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(___pressAnyKeyDisplay);
                myText.text = HotdRemake_ArcadePlugin.GetScreenPlayerString(HotdRemake_ArcadePlugin.Configurator.CreditsToStart);
                myText.alignment = TextAlignmentOptions.Center;
                myText.verticalAlignment = TMPro.VerticalAlignmentOptions.Bottom;
                myText.autoSizeTextContainer = false;
                myText.enableAutoSizing = false;
                HotdRemake_ArcadePlugin.GameMainsFont = myText.font;  //Save font material for later use on other text
                HotdRemake_ArcadePlugin.GameMainFontMaterial = myText.fontMaterial;

                //Getting full canvas size to place objects
                RectTransform TextObjectTransform = ___pressAnyKeyDisplay.GetComponent<RectTransform>();
                if (TextObjectTransform.root != null)
                {
                    RectTransform RootCanvasRectTransform = TextObjectTransform.root.GetComponent<RectTransform>();
                    if (RootCanvasRectTransform != null)
                    {
                        TextObjectTransform.anchorMin = new Vector2(0.5f, 0.0f);    //
                        TextObjectTransform.anchorMax = new Vector2(0.5f, 0.0f);    //Lower center
                        TextObjectTransform.pivot = new Vector2(0.5f, 0.0f);        //
                        TextObjectTransform.anchoredPosition = new Vector2(0.0f, 100.0f);   //Putting it a litlle bit higher than bottom window border
                        TextObjectTransform.sizeDelta = RootCanvasRectTransform.sizeDelta;
                        myText.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
                        myText.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
                        myText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
                        myText.GetComponent<RectTransform>().sizeDelta = RootCanvasRectTransform.sizeDelta;

                        //Sizing the logo a little smaller than the screen
                        float Xscale = 0.7f;
                        float Ratio = img.sprite.rect.width / img.sprite.rect.height;
                        float NewWidth = RootCanvasRectTransform.sizeDelta.x * Xscale;
                        float NewHeight = NewWidth / Ratio;
                        GameLogoTransform.sizeDelta = new Vector2(NewWidth, NewHeight);

                        RankingBackgroundTransform.sizeDelta = new Vector2(RootCanvasRectTransform.sizeDelta.x, RootCanvasRectTransform.sizeDelta.y);
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Force play the intro video at start, instead of Tutorial (replace setVideo() arg from "false" to "true")
        /// </summary>
        [HarmonyPatch(typeof(HD_MainMenuMovieController), "Start")]
        class Start
        {
            static bool Prefix(HD_MainMenuMovieController __instance, ref UnityEngine.UI.Image ___blackScreenImage, ref bool ___unfadedOnStart, ref MP_VideoPlayerController ___mainMenuVideoController, UnityEngine.Video.VideoClip ___mainVideoClip)
            {
                ___blackScreenImage.SetColorAlpha(1f);
                ___unfadedOnStart = false;
                ___mainMenuVideoController.PlayClip(___mainVideoClip, true, 0.5f);
                mSetVideo.setVideo(__instance, true);
                return false;
            }
        }

        /// <summary>
        /// Displaying the Ranking screen between videos clips
        /// </summary>
        [HarmonyPatch(typeof(HD_MainMenuMovieController), "OnVideoPlayerLoopPointReached")]
        class OnVideoPlayerLoopPointReached
        {
            static bool Prefix(HD_MainMenuMovieController __instance, UnityEngine.Video.VideoPlayer ___videoPlayer, HD_PressAnyKeyDisplay ___pressAnyKeyDisplay, MP_VideoPlayerController ___mainMenuVideoController, UnityEngine.Video.VideoClip ___mainVideoClip)
            {
                MP_MusicManager.StopAdditionalMusic();
                MP_MusicManager.StopMusic();
                ___videoPlayer.Pause();
                HotdRemake_ArcadePlugin.IsLeaderBoardIntro = true;
                __instance.StartCoroutine(WaitForLeaderboardClosed_Coroutine(__instance, ___mainMenuVideoController, ___mainVideoClip));

                MP_Menu Menu = (MP_Menu)___pressAnyKeyDisplay.GetType().GetField("menuToOpen", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(___pressAnyKeyDisplay);
                if (Menu != null)
                {
                    foreach (Component c in Menu.GetComponentsInChildren<Component>())
                    {
                        if (c.name == "btn_Leaderboard")
                        {
                            MP_Button b = c.gameObject.GetComponent<MP_Button>();
                            __instance.StartCoroutine(FadeImage(GameObject.Find("RankingBackground"), true, 2.0f));
                            b.onClick.Invoke();
                            break;
                        }
                    }
                }
                return false;
            }
        }
        

        /// <summary>
        /// Check here which video is playing, to add "game title" sprite at start
        /// (Need to find, add sprite and make it diseappear after a while)
        /// </summary>
        [HarmonyPatch(typeof(HD_MainMenuMovieController), "OnVideoPlayerRestart")]
        class OnVideoPlayerRestart
        {
            static bool Prefix(HD_MainMenuMovieController __instance, MP_VideoPlayerController ___mainMenuVideoController, UnityEngine.Video.VideoClip ___mainVideoClip, UnityEngine.Video.VideoPlayer ___videoPlayer, HD_PressAnyKeyDisplay ___pressAnyKeyDisplay)
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.OnVideoPlayerRestart()");
                HotdRemake_ArcadePlugin.DisableStartLampFlag = false;
                HotdRemake_ArcadePlugin.CanFadeOutMusicVideo = true;

                //Displaying the logo when needed
                GameObject Logo = GameObject.Find("GameLogo");                
                if (___mainMenuVideoController.CurrentVideoClip != null && ___mainMenuVideoController.CurrentVideoClip.name == ___mainVideoClip.name)
                {
                    __instance.StartCoroutine(FadeImage(Logo, true, 0.5f));
                }
                return true;
            }
        }
        

        /// <summary>
        /// Removing Title logo when it's time to,
        /// Start a new game directly from the title screen
        /// Doing the same calls than NewGameMenu.StartNewGame()
        /// </summary>
        [HarmonyPatch(typeof(HD_MainMenuMovieController), "Update")]
        class Update
        {
            static bool Prefix(HD_MainMenuMovieController __instance, HD_PressAnyKeyDisplay ___pressAnyKeyDisplay, MP_VideoPlayerController ___mainMenuVideoController, UnityEngine.Video.VideoClip ___mainVideoClip, MP_Menu ___mainMenu)
            {
                //Disabling MainMenu object, if enabled (if not, it can pop-up with a right click and I don't know where this is triggered....
                GameObject MainMenu = GameObject.Find("canvas_MainMenu");
                if (MainMenu != null)
                    MainMenu.SetActive(false);

                //Hiding Logo Sprite and handling music/fading
                if (___mainMenuVideoController.CurrentVideoClip != null)
                {
                    if (___mainMenuVideoController.CurrentVideoClip.name == ___mainVideoClip.name)
                    {
                        if (___mainMenuVideoController.CurrentVideoTime > 5.0)
                        {
                            if (!HotdRemake_ArcadePlugin.IsMainScreenLogoCoroutineRunning)
                            {
                                GameObject Logo = GameObject.Find("GameLogo");
                                __instance.StartCoroutine(FadeImage(Logo, false, 4.0f));
                            }

                            if (___mainMenuVideoController.CurrentVideoTime > ___mainMenuVideoController.CurrentVideoClip.length - 4.0 && HotdRemake_ArcadePlugin.CanFadeOutMusicVideo)
                            {
                                Transform[] trs = __instance.GetComponentsInChildren<Transform>(true);
                                foreach (Transform t in trs)
                                {
                                    if (t.name == "img_Movie")
                                    {
                                        RawImage image = t.gameObject.GetComponent<RawImage>();
                                        HotdRemake_ArcadePlugin.CanFadeOutMusicVideo = false;
                                        __instance.StartCoroutine(FadeOutMusicAndVideo(false, 3.0f, image));
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (___mainMenuVideoController.CurrentVideoTime > ___mainMenuVideoController.CurrentVideoClip.length - 3.0 && HotdRemake_ArcadePlugin.CanFadeOutMusicVideo)
                        {
                            Transform[] trs = __instance.GetComponentsInChildren<Transform>(true);
                            foreach (Transform t in trs)
                            {
                                if (t.name == "img_Movie")
                                {
                                    RawImage image = t.gameObject.GetComponent<RawImage>();
                                    HotdRemake_ArcadePlugin.CanFadeOutMusicVideo = false;
                                    __instance.StartCoroutine(FadeOutMusicAndVideo(false, 2.0f, image));
                                    break;
                                }
                            }
                        }
                    }
                }

                //Refreshing Player Text String
                TMP_Text myText = (TMP_Text)___pressAnyKeyDisplay.GetType().GetField("textField", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(___pressAnyKeyDisplay);
                myText.text = HotdRemake_ArcadePlugin.GetScreenPlayerString(HotdRemake_ArcadePlugin.Configurator.CreditsToStart); 

                //Starting Game if start is pressed
                if (HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player1, HotdRemake_ArcadePlugin.MyInputButtons.Start) && HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player1].IsWaitingToStartPlaying)
                {
                    StartGame(PlayerType.Player1);
                    return false;
                }
                else if (HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player2, HotdRemake_ArcadePlugin.MyInputButtons.Start) && HotdRemake_ArcadePlugin.PluginPlayers[(int)PlayerType.Player2].IsWaitingToStartPlaying)
                {
                    StartGame(PlayerType.Player2);
                    return false;
                }

                return false;
            }
        }
        public static void StartGame(PlayerType Player)
        {
            if (HotdRemake_ArcadePlugin.CurrentCreditsCount >= HotdRemake_ArcadePlugin.Configurator.CreditsToStart || HotdRemake_ArcadePlugin.Configurator.Freeplay)
            {
                HotdRemake_ArcadePlugin.PluginPlayers[(int)Player].IsWaitingToStartPlaying = false;

                FMODUnity.RuntimeManager.PlayOneShot("event:/UI/UI_pistolShot", default(Vector3));  //-> sound play         

                HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.Update() => Starting new game for Player " + Player.ToString());
                if (!HotdRemake_ArcadePlugin.Configurator.Freeplay)
                    HotdRemake_ArcadePlugin.CurrentCreditsCount -= HotdRemake_ArcadePlugin.Configurator.CreditsToStart;

                //Only starting a new game if another player isn't alreday starting it....
                int OtherPlayer = 1 - (int)Player;
                if (HotdRemake_ArcadePlugin.PluginPlayers[OtherPlayer].IsWaitingToStartPlaying)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.Update() => Starting game level loading.....");
                    HD_SaveDataHandler.PlaythroughState.ResetDataWhenStartingNewGame(GameMode.Original, ScoreMode.Classic, MultiplayerMode.Competitive, (Difficulty)HotdRemake_ArcadePlugin.Configurator.GameDifficulty);
                    HD_GameManager.ChangeDifficulty((Difficulty)HotdRemake_ArcadePlugin.Configurator.GameDifficulty);
                    HD_ScoreManager.Instance.SetScoreMode(ScoreMode.Classic);
                    HD_ScoreManager.Instance.SetMultiplayerScoreMode(MultiplayerMode.Competitive);
                    MP_HintsManager.ForceCleanState();
                    MP_LevelLoader.Instance.LoadGameplayLevel(0);
                }
                else
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.Update() => Loading procedure already started by "  + (PlayerType)OtherPlayer);
                    
            }
        }

        #region Coroutines

        /// <summary>
        /// Fading procedure for our title logo, will be called as a Coroutine to fade in and out the Image
        /// </summary>
        public static IEnumerator FadeImage(GameObject ObjectToFade, bool FadeToShow, float FadeDuration)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.FadeImage_Coroutine() => Start coroutine");
            HotdRemake_ArcadePlugin.IsMainScreenLogoCoroutineRunning = true;
            Image Img = ObjectToFade.GetComponent<Image>();
            Color InitialColor = Img.color;
            Color TargetColor = new Color(InitialColor.r, InitialColor.g, InitialColor.b, 0.0f);
            if (FadeToShow)
                TargetColor.a = 1.0f;

            float ElapsedTime = 0f;

            while (ElapsedTime < FadeDuration)
            {
                ElapsedTime += Time.deltaTime;
                Img.color = Color.Lerp(InitialColor, TargetColor, ElapsedTime / FadeDuration);
                yield return null;
            }
            HotdRemake_ArcadePlugin.IsMainScreenLogoCoroutineRunning = false;
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.FadeImage_Coroutine() => Coroutine Ended");
        }

        /// <summary>
        /// Fading procedure for the video clips played in attract mode
        /// </summary>
        public static IEnumerator FadeOutMusicAndVideo(bool FadeToShow, float FadeDuration, RawImage ImageToFade)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.FadeMusicAndVideo() => Coroutine Started");
            MP_MusicManager Manager = (MP_MusicManager)typeof(MP_MusicManager).GetField("instance", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            if (Manager != null)
            {
                FMODUnity.StudioEventEmitter Emitter = (FMODUnity.StudioEventEmitter)Manager.GetType().GetField("eventEmitter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Manager);
                if (Emitter != null)
                {
                    FMOD.Studio.EventInstance Event = (FMOD.Studio.EventInstance)Emitter.GetType().GetField("instance", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Emitter);
                    float InitialVolume = 0.0f;
                    Event.getVolume(out InitialVolume);
                    float InitialColor = ImageToFade.color.a;

                    float TargetVolume = 0.0f;
                    float TargetColor = 0.0f;

                    float ElapsedTime = 0f;
                    while (ElapsedTime < FadeDuration)
                    {
                        ElapsedTime += Time.deltaTime;
                        Event.setVolume(Mathf.Lerp(InitialVolume, TargetVolume, ElapsedTime / FadeDuration));
                        ImageToFade.SetColorAlpha(Mathf.Lerp(InitialColor, TargetColor, ElapsedTime / FadeDuration));
                        float f;
                        Event.getVolume(out f);
                        yield return null;
                    }
                }
            }
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.FadeMusicAndVideo() => Coroutine Ended");
        }

        /// <summary>
        /// Switch video after leaderboard is closed
        /// </summary>
        public static IEnumerator WaitForLeaderboardClosed_Coroutine(HD_MainMenuMovieController __instance, MP_VideoPlayerController ___mainMenuVideoController, UnityEngine.Video.VideoClip ___mainVideoClip)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.WaitForLeaderboardClosed_Coroutine() => Start coroutine");
            GameObject oBackground = GameObject.Find("RankingBackground");
            float Chrono = 11.0f;
            while (Chrono > 2.0f)
            {
                Chrono -= Time.deltaTime;
                yield return null;
            }
            __instance.StartCoroutine(FadeImage(oBackground, false, 2.0f));

            while (Chrono > 0.0f)
            {
                Chrono -= Time.deltaTime;
                yield return null;
            }
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_MainMenuMovieController.WaitForLeaderboardClosed_Coroutine() => Ended !");
            mswapVideo.swapVideo(__instance);
            if (!HotdRemake_ArcadePlugin.Configurator.AdvertiseSound)
                MP_MusicManager.PlayMusic(MusicType.None);
            else
            {
                if (___mainMenuVideoController.CurrentVideoClip.name == ___mainVideoClip.name)
                    MP_MusicManager.PlayMusic(MusicType.Level2Music);
                else
                    MP_MusicManager.PlayMusic(MusicType.MainMenuMusic);
            }
        }

        #endregion
        
        [HarmonyPatch(typeof(HD_MainMenuMovieController), "setVideo")]
        class mSetVideo
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void setVideo(object instance, bool _mainVideo)
            {
                //Used to call the private method    
            }
        }

        [HarmonyPatch(typeof(HD_MainMenuMovieController), "swapVideo")]
        class mswapVideo
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void swapVideo(object instance)
            {
                //Used to call the private method    
            }
        }


        
    }
}
