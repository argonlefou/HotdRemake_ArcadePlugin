using HarmonyLib;
using UnityEngine;
using System.Collections;

namespace HotdRemake_ArcadePlugin_202207
{
    class mHD_LeaderboardMenu
    {
        /// <summary>
        /// Forcing Local Leaderboard
        /// </summary>
        [HarmonyPatch(typeof(HD_LeaderboardMenu), "Open")]
        class Open
        {
            static bool Prefix()
            {
                HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_LeaderboardMenu.Open()");
                return true;
            }
        }


        /// <summary>
        /// Forcing Local Leaderboard
        /// </summary>
        [HarmonyPatch(typeof(HD_LeaderboardMenu), "displayLeaderboard")]
        class displayLeaderboard
        {
            static bool Prefix(ref LeaderboardDataType _dataType, HD_LocalLeaderboardData.LeaderboardType _leaderboardType)
            {
                _dataType = LeaderboardDataType.Local;
                return true;
            }
        }

        /// <summary>
        /// Forcing multiplayer, competitive and always same difficulty leaderboard
        /// </summary>
        [HarmonyPatch(typeof(HD_LeaderboardMenu), "displayLocalLeaderboard")]
        class displayLocalLeaderboard
        {
            static bool Prefix(ref HD_LocalLeaderboardData.LeaderboardType _leaderboardType)
            {
                _leaderboardType.ScoreMode = ScoreMode.Classic;
                _leaderboardType.MultiplayerMode = MultiplayerMode.Competitive;
                _leaderboardType.GameMode = GameMode.Original;
                _leaderboardType.Difficulty = Difficulty.Arcade;
                return true;
            }
        }

        /// <summary>
        /// Removing unwanted graphical objects
        /// </summary>
        [HarmonyPatch(typeof(HD_LeaderboardMenu), "displayLeaderboardData")]
        class displayLeaderboardData
        {
            static bool Prefix(HD_LeaderboardMenu __instance, ScoreMode _scoreMode, ref HD_LeaderboardItem.LeaderboardItemData[] _leaderboardDatas)
            {
                Transform[] trs = __instance.GetComponentsInChildren<Transform>();

                //At the end of teh game, it is needed to press the "CONTINUE" button to go on
                if (!HotdRemake_ArcadePlugin.IsLeaderBoardIntro)
                {
                    foreach (Transform t in trs)
                    {
                        if (t.name.Equals("canvas_LeaderboardMenu"))
                        {
                            t.gameObject.SetActive(true);
                        }
                        if (t.name.Equals("img_MenuBackground") || t.name.Equals("canvas_LeaderboardTop"))
                        {
                            t.gameObject.SetActive(false);
                        }
                        else if (t.name.Equals("btn_Continue"))
                        {
                            MP_Button b = t.gameObject.GetComponent<MP_Button>();
                            TMPro.TMP_Text myTest = b.GetComponentInChildren<TMPro.TMP_Text>();
                            myTest.text = HotdRemake_ArcadePlugin.TextLangs[LanguageStrings.StringName.PressStartToSkip];
                            myTest.SetColorAlpha(0.0f);
                            __instance.StartCoroutine(EndGameLeaderboardMenu_Close(__instance, b));
                        }
                    }
                }
                //On attract screen, no need to push button, just closing the display is enough
                else
                {
                    foreach (Transform t in trs)
                    {
                        if (t.name.Equals("canvas_LeaderboardMenu"))
                        {
                            t.gameObject.SetActive(true);
                        }
                        if (t.name.Equals("img_MenuBackground") || t.name.Equals("canvas_LeaderboardTop"))
                        {
                            t.gameObject.SetActive(false);
                        }
                    }
                    __instance.StartCoroutine(IntroLeaderboardMenu_Close(__instance));
                }
                return true;
            }
        }

        /// <summary>
        /// Close Leaderboard by pressing a button
        /// </summary>
        public static IEnumerator EndGameLeaderboardMenu_Close(HD_LeaderboardMenu Menu, MP_Button Button)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_LeaderboardMenu.EndGameLeaderboardMenu_Close() => Start coroutine");
            float Chrono = 6.0f;
            while (true)
            {
                bool bFlagP1 = HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player1, HotdRemake_ArcadePlugin.MyInputButtons.Start) && HD_PlayerCombatManager.Instance.GetPlayerState(PlayerType.Player1) == HD_PlayerCombatManager.PlayerState.Up;
                bool bFlagP2 = HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player2, HotdRemake_ArcadePlugin.MyInputButtons.Start) && HD_PlayerCombatManager.Instance.GetPlayerState(PlayerType.Player2) == HD_PlayerCombatManager.PlayerState.Up;
                if (bFlagP1 || bFlagP2 || Chrono <= 0)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_LeaderboardMenu.EndGameLeaderboardMenu_Close() => Close !");
                    Menu.Close();
                    Button.onClick.Invoke();
                    HotdRemake_ArcadePlugin.IsLeaderBoardIntro = false;
                    break;
                }
                Chrono -= Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Close Leaderboardby closing the menu
        /// </summary>
        public static IEnumerator IntroLeaderboardMenu_Close(HD_LeaderboardMenu Menu)
        {
            HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_LeaderboardMenu.IntroLeaderboardMenu_Close() => Start coroutine");
            float Chrono = 10.0f;
            while (true)
            {
                bool bFlagP1 = HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player1, HotdRemake_ArcadePlugin.MyInputButtons.Start);
                bool bFlagP2 = HotdRemake_ArcadePlugin.GetButtonDown(PlayerType.Player2, HotdRemake_ArcadePlugin.MyInputButtons.Start);
                if (bFlagP1 || bFlagP2 || Chrono <= 0)
                {
                    HotdRemake_ArcadePlugin.MyLogger.LogMessage("HD_LeaderboardMenu.IntroLeaderboardMenu_Close() => Close !");
                    Menu.Close();
                    HotdRemake_ArcadePlugin.IsLeaderBoardIntro = false;
                    break;
                }
                Chrono -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
