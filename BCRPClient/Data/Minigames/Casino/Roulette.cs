using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BCRPClient.Data.Minigames.Casino
{
    public class Roulette : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(CEF.Browser.IntTypes.CasinoRoulette);

        public static uint CurrentBet { get; set; }

        private static int EscBindIdx { get; set; } = -1;

        public Roulette()
        {
            Events.Add("CasinoRoulette::SetBet", (args) =>
            {
                CurrentBet = Convert.ToUInt32(args[0]);
            });
        }

        public static async void Show(Data.Locations.Casino casino, Data.Locations.Casino.Roulette roulette, decimal chipsBalance)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(CEF.Browser.IntTypes.CasinoRoulette, true, true);

            CEF.Browser.Window.ExecuteJs($"Casino.draw", 0, roulette.GetCurrestStateString() ?? "null", chipsBalance, roulette.MaxBet, CurrentBet < roulette.MinBet || CurrentBet > roulette.MaxBet ? roulette.MinBet : CurrentBet, new object[] { });

            //CEF.Notification.ClearAll();

            CEF.Notification.SetOnTop(true);

            if (!Settings.Interface.HideHUD)
                CEF.HUD.ShowHUD(false);

            CEF.Chat.Show(false);

            CEF.Cursor.Show(true, true);

            GameEvents.DisableAllControls(true);

            KeyBinds.DisableAll(KeyBinds.Types.MicrophoneOff, KeyBinds.Types.MicrophoneOn, KeyBinds.Types.Cursor);

            roulette.StartGame();

            if (roulette.LastBets != null)
            {
                foreach (var x in roulette.LastBets)
                    AddLastBet(x);
            }

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            CurrentBet = roulette.MinBet;
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(CEF.Browser.IntTypes.CasinoRoulette, false);

            //CEF.Notification.ClearAll();

            CEF.Notification.SetOnTop(false);

            CEF.Cursor.Show(false, false);

            Data.Locations.Casino.Roulette.CurrentRoulette?.StopGame();

            if (!Settings.Interface.HideHUD)
                CEF.HUD.ShowHUD(true);

            CEF.Chat.Show(true);

            GameEvents.DisableAllControls(false);

            KeyBinds.EnableAll();

            KeyBinds.Unbind(EscBindIdx);

            EscBindIdx = -1;
        }

        public static void AddLastBet(Data.Locations.Casino.Roulette.BetTypes betType)
        {
            if (!IsActive)
                return;

            if (Data.Locations.Casino.Roulette.HoverDatas == null)
                return;

            var colourNum = 0;

            if (betType == Locations.Casino.Roulette.BetTypes._0 || betType == Locations.Casino.Roulette.BetTypes._00)
            {
                colourNum = 2;
            }
            else
            {
                var blackNumbers = Data.Locations.Casino.Roulette.HoverDatas.GetValueOrDefault(Locations.Casino.Roulette.BetTypes.Black)?.HoverNumbers;

                if (blackNumbers != null)
                {
                    if (Array.IndexOf(blackNumbers, (byte)betType) >= 0)
                        colourNum = 1;
                }
            }

            CEF.Browser.Window.ExecuteJs("Casino.addLastNum", new List<object> { colourNum, betType.ToString().Replace("_", "") });
        }

        public static void UpdateStatus(string status)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("Casino.updateGameStatus", status);
        }

        public static void UpdateBalance(decimal balance)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("Casino.updateCurBal", balance);
        }
    }
}
