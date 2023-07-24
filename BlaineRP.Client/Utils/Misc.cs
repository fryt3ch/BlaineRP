using BlaineRP.Client.Utils.Game;
using RAGE;
using System;
using System.Text.RegularExpressions;

namespace BlaineRP.Client.Utils
{
    internal static class Misc
    {
        public static Colour RedColor = new Colour(255, 0, 0);

        public static RGBA WhiteColourRGBA = new RGBA(255, 255, 255, 255);

        private static Regex MailPattern = new Regex(@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$", RegexOptions.Compiled);
        private static Regex LoginPattern = new Regex(@"^(?=.*[a-zA-Z0-9])[0-9a-zA-Z!@#$%^&*]{6,12}$", RegexOptions.Compiled);
        private static Regex NamePattern = new Regex(@"^[A-Z]{1}[a-zA-Z]{1,9}$", RegexOptions.Compiled);
        private static Regex PasswordPattern = new Regex(@"^(?=.*[a-zA-Z0-9])[0-9a-zA-Z!@#$%^&*]{6,64}$", RegexOptions.Compiled);
        public static bool IsGameWindowFocused => RAGE.Ui.Windows.Focused;

        public static Random Random { get; } = new Random();

        public static bool CanShowCEF(bool checkCursor = true, bool checkPause = true) => (checkCursor ? !CEF.Cursor.IsVisible : true) && (checkPause ? !RAGE.Game.Ui.IsPauseMenuActive() : true);
        public static void DebugServerSaveText(string text) => Events.CallRemote("debug_save", text);

        public static float GetFpsCoef() => Settings.App.Static.BaseFps / (Main.CurrentFps > Settings.App.Static.BaseFps ? Settings.App.Static.BaseFps : Main.CurrentFps);

        public static decimal GetGovSellPrice(decimal price) => System.Math.Floor(price / 2m);

        public static bool IsAnyCefActive(bool checkChatInput = true) => checkChatInput && CEF.Chat.InputVisible || CEF.Browser.IsAnyCEFActive;
        public static bool IsLoginValid(string str) => LoginPattern.IsMatch(str);

        public static bool IsMailValid(string str) => MailPattern.IsMatch(str);
        public static bool IsNameValid(string str) => NamePattern.IsMatch(str);
        public static bool IsPasswordValid(string str) => PasswordPattern.IsMatch(str);

        /// <summary>Метод для перезагрузки голосового чата со стороны RAGE</summary>
        public static void ReloadVoiceChat() => Invoker.JsEval("try { mp.voiceChat.cleanupAndReload(true, false, false) } catch {} try { mp.voiceChat.cleanupAndReload(false, false, true) } catch {} try { mp.voiceChat.cleanupAndReload(true, true, true) } catch {}");

        /// <summary>Метод для замены символа \n в строке на тег /br</summary>
        /// <param name="text"></param>
        public static string ReplaceNewLineHtml(string text) => text.Replace("\n", "</br>");
    }
}
