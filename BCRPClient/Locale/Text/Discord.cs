using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static partial class General
        {
            public static class Discord
            {
                public static string Header = "Играет на Blaine RP";

                public static Dictionary<Additional.Discord.Types, string> Statuses = new Dictionary<Additional.Discord.Types, string>()
                {
                    { Additional.Discord.Types.Default, "" },
                    { Additional.Discord.Types.Login, "Входит в аккаунт" },
                    { Additional.Discord.Types.Registration, "Проходит регистрацию" },
                    { Additional.Discord.Types.CharacterSelect, "Выбирает персонажа" },
                };
            }
        }
    }
}
