using RAGE;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace BCRPClient.Data.Minigames.Casino
{
    public class Roulette : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(CEF.Browser.IntTypes.CasinoRoulette);

        public static uint CurrentBet { get; set; }

        public Roulette()
        {
            Events.Add("CasinoRoulette::SetBet", (args) =>
            {
                CurrentBet = Convert.ToUInt32(args[0]);
            });
        }

        public static async void Show(Data.Locations.Casino.Roulette roulette)
        {
            if (IsActive)
                return;

            await CEF.Browser.Render(CEF.Browser.IntTypes.CasinoRoulette, true, true);

            CEF.Browser.Window.ExecuteJs($"Casino.draw", new object[] { }, "Ожидание ставок", 0, roulette.MaxBet);

            if (!Settings.Interface.HideHUD)
                CEF.HUD.ShowHUD(false);

            CEF.Chat.Show(false);

            CEF.Cursor.Show(true, true);

            GameEvents.DisableAllControls(true);

            KeyBinds.DisableAll(KeyBinds.Types.MicrophoneOff, KeyBinds.Types.MicrophoneOn);

            roulette.StartGame();

            if (roulette.LastBets != null)
            {
                foreach (var x in roulette.LastBets)
                    AddLastBet(x);
            }
        }

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(CEF.Browser.IntTypes.CasinoRoulette, false);

            Data.Locations.Casino.Roulette.CurrentRoulette?.StopGame();

            if (!Settings.Interface.HideHUD)
                CEF.HUD.ShowHUD(true);

            CEF.Chat.Show(true);

            GameEvents.DisableAllControls(false);

            KeyBinds.EnableAll();
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
    }
}
