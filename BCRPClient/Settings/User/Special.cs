using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.Settings.User
{
    public static class Special
    {
        public static class Default
        {
            public static bool DisabledPerson = false;
        }

        private static bool _DisabledPerson;

        public static bool DisabledPerson { get => _DisabledPerson; set { if (value != _DisabledPerson) Additional.Storage.SetData("Settings::Special::DisabledPerson", value); _DisabledPerson = value; CEF.Menu.UpdateToggle("sett-special", value); } }
    }
}
