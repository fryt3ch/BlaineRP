using RAGE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BCRPClient.Additional
{
    public class Discord : Events.Script
    {
        public static Types CurrentType { get; private set; }
        private static string CurrentText { get; set; }

        public enum Types
        {
            Default = 0,
            Login,
            Registration,
            CharacterSelect,
        }

        public Discord()
        {
            SetDefault();

            (new AsyncTask(() => RAGE.Discord.Update(CurrentText, Locale.General.Discord.Header), Settings.DISCORD_STATUS_UPDATE_TIME, true, 0)).Run();
        }

        public static void SetStatus(Types type, params object[] args)
        {
            CurrentType = type;
            CurrentText = args.Length == 0 ? Locale.General.Discord.Statuses[type] : string.Format(Locale.General.Discord.Statuses[type], args);

            RAGE.Discord.Update(CurrentText, Locale.General.Discord.Header);
        }

        public static void SetDefault()
        {
            CurrentType = Types.Default;
            CurrentText = Locale.General.Discord.Statuses[Types.Default];

            RAGE.Discord.Update(CurrentText, Locale.General.Discord.Header);
        }
    }
}
