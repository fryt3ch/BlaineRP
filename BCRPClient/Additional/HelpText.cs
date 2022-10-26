using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Additional
{
    class HelpText : Events
    {
        public static bool IsActive { get => RAGE.Game.Ui.IsHelpMessageOnScreen(); }

        public static void Show(string text, int duration = -1, bool sound = true)
        {
            RAGE.Game.Ui.BeginTextCommandDisplayHelp("STRING");
            RAGE.Game.Ui.AddTextComponentSubstringPlayerName(text);
            RAGE.Game.Ui.EndTextCommandDisplayHelp(0, duration == -1, sound, duration);
        }

        public static void Hide()
        {
            RAGE.Game.Ui.ClearAllHelpMessages();
        }

        public HelpText()
        {

        }
    }
}
