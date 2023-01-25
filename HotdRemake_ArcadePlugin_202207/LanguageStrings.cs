using System;
using System.Collections.Generic;

namespace HotdRemake_ArcadePlugin_202207
{
    public class LanguageStrings
    {
        public enum StringName
        {
            Credits,
            Continue,
            Freeplay,
            GameOver,
            InsertCoins,
            InsertMoreCoins,
            Loading,
            PressStart,
            PressStartToSkip
        }

        public static Dictionary<StringName, String> ChineseDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "学分"},
            { StringName.Continue, "继续 ？"},
            { StringName.Freeplay, "免费玩"},
            { StringName.GameOver, "游戏结束"},      
            { StringName.InsertCoins, "投币"},
            { StringName.InsertMoreCoins, "插入更多硬币"},
            { StringName.Loading, "加载中。 . ."},
            { StringName.PressStart, "按开始"},
            { StringName.PressStartToSkip, "按开始跳过"}
        };

        public static Dictionary<StringName, String> EnglishDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CREDITS"},
            { StringName.Continue, "CONTINUE ?"},
            { StringName.Freeplay, "FREEPLAY"},
            { StringName.GameOver, "GAME OVER"},
            { StringName.InsertCoins, "INSERT COIN(S)"},
            { StringName.InsertMoreCoins, "INSERT MORE COIN(S)"},
            { StringName.Loading, "LOADING. . ."},
            { StringName.PressStart, "PRESS START"},
            { StringName.PressStartToSkip, "PRESS START TO SKIP"}
        };

        public static Dictionary<StringName, String> FrenchDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CRÉDITS"},
            { StringName.Continue, "CONTINUER ?"},
            { StringName.Freeplay, "JEU LIBRE"},
            { StringName.GameOver, "GAME OVER"},      
            { StringName.InsertCoins, "INSÉRER CRÉDIT(S)"},
            { StringName.InsertMoreCoins, "INSÉRER PLUS DE CRÉDIT(S)"},
            { StringName.Loading, "CHARGEMENT EN COURS. . ."},
            { StringName.PressStart, "APPUYER SUR START"},
            { StringName.PressStartToSkip, "APPUYER SUR START POUR PASSER"}
        };

        public static Dictionary<StringName, String> GermanDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "KREDITE"},
            { StringName.Continue, "FORTSETZEN ?"},
            { StringName.Freeplay, "FREIES SPIEL"},
            { StringName.GameOver, "SPIEL IST AUS"},      
            { StringName.InsertCoins, "MÜNZEN EINWERFEN)"},
            { StringName.InsertMoreCoins, "WEITERE MÜNZE(N) EINWERFEN"},
            { StringName.Loading, "WIRD GELADEN. . ."},
            { StringName.PressStart, "DRÜCKE START"},
            { StringName.PressStartToSkip, "DRÜCKE START ZUM ÜBERSPRINGEN"}
        };

        public static Dictionary<StringName, String> ItalianDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CREDITI"},
            { StringName.Continue, "CONTINUA ?"},
            { StringName.Freeplay, "GIOCO GRATUITO"},
            { StringName.GameOver, "GAME OVER"},      
            { StringName.InsertCoins, "INSERIRE MONETA(E)"},
            { StringName.InsertMoreCoins, "INSERIRE ALTRE MONETE"},
            { StringName.Loading, "CARICAMENTO IN CORSO. . ."},
            { StringName.PressStart, "PREMI START"},
            { StringName.PressStartToSkip, "PREMI START PER SALTARE"}
        };

        public static Dictionary<StringName, String> JapaneseDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "クレジット"},
            { StringName.Continue, "続く ？"},
            { StringName.Freeplay, "無料プレイ"},
            { StringName.GameOver, "ゲームオーバー"},      
            { StringName.InsertCoins, "コインを挿入"},
            { StringName.InsertMoreCoins, "より多くのコインを挿入してください"},
            { StringName.Loading, "読み込み中。 . ."},
            { StringName.PressStart, "スタートを押す"},
            { StringName.PressStartToSkip, "スキップするにはスタートを押してください"}
        };

        public static Dictionary<StringName, String> PolishDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "KREDYTY"},
            { StringName.Continue, "DALEJ ?"},
            { StringName.Freeplay, "DARMOWA GRA"},
            { StringName.GameOver, "KONIEC GRY"},      
            { StringName.InsertCoins, "WŁÓŻ MONETY"},
            { StringName.InsertMoreCoins, "WŁÓŻ WIĘCEJ MONETY"},
            { StringName.Loading, "ŁADOWANIE. . ."},
            { StringName.PressStart, "NACIŚNIJ START"},
            { StringName.PressStartToSkip, "NACIŚNIJ START, ABY POMINĄĆ"}
        };

        public static Dictionary<StringName, String> RussianDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "КРЕДИТЫ"},
            { StringName.Continue, "ПРОДОЛЖИТЬ ?"},
            { StringName.Freeplay, "БЕСПЛАТНАЯ ИГРА"},
            { StringName.GameOver, "ИГРА ЗАКОНЧЕНА"},      
            { StringName.InsertCoins, "ВСТАВЬТЕ МОНЕТЫ"},
            { StringName.InsertMoreCoins, "ВСТАВЬТЕ БОЛЬШЕ МОНЕТ(Ы)"},
            { StringName.Loading, "ЗАГРУЗКА. . ."},
            { StringName.PressStart, "НАЖМИТЕ СТАРТ"},
            { StringName.PressStartToSkip, "НАЖМИТЕ СТАРТ, ЧТОБЫ ПРОПУСТИТЬ"}
        };

        public static Dictionary<StringName, String> SpainDictionnary = new Dictionary<StringName, String>()
        {
            { StringName.Credits, "CRÉDITOS"},
            { StringName.Continue, "SEGUIR ?"},
            { StringName.Freeplay, "JUEGO GRATIS"},
            { StringName.GameOver, "JUEGO TERMINADO"},      
            { StringName.InsertCoins, "INSERTAR MONEDA(S)"},
            { StringName.InsertMoreCoins, "INSERTAR MÁS MONEDA(S)"},
            { StringName.Loading, "CARGANDO. . ."},
            { StringName.PressStart, "PRESIONA INICIO"},
            { StringName.PressStartToSkip, "PULSA INICIO PARA SALTAR"}
        };
    }
}









