using System.Collections;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_AgentNameMenu
    {
        //Disabling original keyboard keys to validate
        [HarmonyPatch(typeof(HD_AgentNameMenu), "addActionsToPlayers")]
        class addActionsToPlayers
        {
            static bool Prefix()
            {
                return false;
            }
        }

        //Disabling AgentName menu on the lower side
        [HarmonyPatch(typeof(HD_AgentNameMenu), "OnEnable")]
        class OnEnable
        {
            static bool Prefix(HD_AgentNameMenu __instance)
            {
                //Creating the "Press Start To Save" image/texture
                GameObject TypeTextObject = new GameObject("TypeText");
                RectTransform TypeTextTransform = TypeTextObject.AddComponent<RectTransform>();
                TypeTextTransform.transform.SetParent(__instance.transform); // setting parent
                TypeTextTransform.localScale = Vector3.one;
                TypeTextTransform.pivot = new Vector2(0.5f, 0.0f);      //
                TypeTextTransform.anchorMin = new Vector2(0.5f, 0.0f);  //Upper Middle
                TypeTextTransform.anchorMax = new Vector2(0.5f, 0.0f);  //
                TypeTextTransform.anchoredPosition = new Vector2(0.0f, 0.0f); // setting position, will be a little bit below upper border window        

                Texture2D tDrawTexture = new Texture2D(2, 2);
                byte[] bBuffer = Properties.Resources.typewriter_text_2;
                tDrawTexture.LoadImage(bBuffer);
                tDrawTexture.name = "Texture_TypeText";

                Image imgTypeText = TypeTextObject.AddComponent<Image>();
                imgTypeText.sprite = Sprite.Create(tDrawTexture, new Rect(0, 0, tDrawTexture.width, tDrawTexture.height), new Vector2(0.5f, 0.5f));
                //img.color = Color.yellow;
                TypeTextObject.transform.SetParent(__instance.transform);
                imgTypeText.SetColorAlpha(0.0f);
                TypeTextTransform.sizeDelta = new Vector2(tDrawTexture.width, tDrawTexture.height); // custom size
                TypeTextObject.SetActive(true);
                
                /*
                Transform[] trs = __instance.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in trs)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("Transform : " + t.name);
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage(t.name + ", enabled: " + t.gameObject.activeSelf);
                    if (t.name.Equals("canvas_AgentNameMenu"))
                    {
                        HotdRemake_ArcadePlugin.MyLogger.LogMessage("canvas_AgentNameMenu activated");
                        t.gameObject.SetActive(false);
                    }
                }
                //To hide the menu we have to completely disable the only Canva object available, thus
                //disabling the possibility to run the coroutine in this Object
                //That's why we are running it on the main plugin class instead....
                HotdRemake_ArcadePlugin.RunAgentNameMenu_Coroutine(__instance);*/

                __instance.StartCoroutine(WaitForQuit_Coroutine(__instance, TypeTextObject));
                return true;
            }
        }

        public static void SetTmpText(string Text, TMPro.TMP_Text Target)
        {
            Target.text = Text;

            Target.alignment = TMPro.TextAlignmentOptions.Center;
            Target.verticalAlignment = TMPro.VerticalAlignmentOptions.Bottom;
            Target.autoSizeTextContainer = false;
            Target.enableAutoSizing = false;

            Target.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            Target.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
            Target.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f);
            Target.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);

            Target.color = Color.white;
            Target.fontSize = 45;
            Target.fontSizeMin = 20;
            Target.fontSizeMax = 60;
            Target.font = HotdRemake_ArcadePlugin.GameMainsFont;
            Target.fontMaterial = HotdRemake_ArcadePlugin.GameMainFontMaterial;
        }

        //Disabling Bad Words check and auto completing ampty strings
        [HarmonyPatch(typeof(HD_AgentNameMenu), "OnContinue")]
        class OnContinue
        {
            static bool Prefix(HD_AgentNameMenu __instance, MP_Typewriter ___typewriter)
            {
                string text = ___typewriter.GetInputTextReceived().Trim();
                if (text == null || text.Length == 0)
                {
                    text = "AAA";
                }

                mhandleContinue.handleContinue(__instance, text);
                return false;
            }
        }


        /// <summary>
        /// Adding a small easter Egg :-)
        /// </summary>
        [HarmonyPatch(typeof(HD_AgentNameMenu), "savePlayerName")]
        class savePlayerName
        {
            static bool Prefix(HD_AgentNameMenu __instance, string _name)
            {
                HD_LocalLeaderboardData.LeaderboardType LdbType = HD_SaveDataHandler.PlaythroughState.GetCurrentLeaderboardType();
                LdbType.Difficulty = Difficulty.Arcade;
                LdbType.GameMode = GameMode.Original;
                LdbType.MultiplayerMode = MultiplayerMode.Competitive;
                LdbType.ScoreMode = ScoreMode.Classic;
                
                int num = HD_ScoreManager.Instance.GetPlayerFinalScore(PlayerType.Player1);
                if (HD_SaveDataHandler.PlaythroughState.IsMultiplayer)
                {
                    int playerFinalScore = HD_ScoreManager.Instance.GetPlayerFinalScore(PlayerType.Player2);
                    if (playerFinalScore > num)
                    {
                        num = playerFinalScore;
                    }
                }
                HD_SaveDataHandler.LocalLeaderboardData.TryToSavePlayerResults(LdbType, num, _name);
                MegaPixel.SaveLoadSystem.MP_SaveLoad.Save(__instance, null, true, 1f);

                return false;
            }
        }

        [HarmonyPatch(typeof(HD_AgentNameMenu), "handleContinue")]
        class mhandleContinue
        {
            [HarmonyReversePatch]
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static void handleContinue(object instance, string _playerName)
            {
                //Used to call the private method    
            }
        }

        /// <summary>
        /// Save Name
        /// </summary>
        public static IEnumerator WaitForQuit_Coroutine(HD_AgentNameMenu Menu, GameObject TypeTextSaveObject)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_AgentNameMenu.WaitForQuit_Coroutine() => Start coroutine");
            Image Img = TypeTextSaveObject.GetComponent<Image>();
            Color InitialColor = Img.color;
            Color TargetColor = new Color(InitialColor.r, InitialColor.g, InitialColor.b, 1.0f);
            float ElapsedTime = 0f;
            float FadeDuration = 2.0f;
            while (ElapsedTime < FadeDuration)
            {
                ElapsedTime += Time.deltaTime;
                Img.color = Color.Lerp(InitialColor, TargetColor, ElapsedTime / FadeDuration);
                yield return null;
            }
            HotdRemake_ArcadePlugin.IsMainScreenLogoCoroutineRunning = false;
            while (true)
            {
                if (ElapsedTime < FadeDuration)
                {
                    ElapsedTime += Time.deltaTime;
                    Img.color = Color.Lerp(InitialColor, TargetColor, ElapsedTime / FadeDuration);
                }
                bool bFlagP1 = HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player1, HotdRemake_ArcadePlugin.MyInputButtons.Start) && HD_PlayerCombatManager.Instance.GetPlayerState(PlayerType.Player1) == HD_PlayerCombatManager.PlayerState.Up;
                bool bFlagP2 = HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player2, HotdRemake_ArcadePlugin.MyInputButtons.Start) && HD_PlayerCombatManager.Instance.GetPlayerState(PlayerType.Player2) == HD_PlayerCombatManager.PlayerState.Up;
                if (bFlagP1 || bFlagP2)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("mHD_AgentNameMenu.WaitForQuit_Coroutine() => Saving !");
                    Menu.OnContinue();
                    break;
                }
                yield return null;
            }
        } 
    }
}
