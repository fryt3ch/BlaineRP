using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Settings.User
{
    public static class Special
    {
        public static class Default
        {
            public static bool DisabledPerson = false;
        }

        private static bool _DisabledPerson;

        public static bool DisabledPerson { get => _DisabledPerson; set { if (value != _DisabledPerson) RageStorage.SetData("Settings::Special::DisabledPerson", value); _DisabledPerson = value; Menu.UpdateToggle("sett-special", value); } }
    }
}
