using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Settings.User
{
    public static class Chat
    {
        public static class Default
        {
            public static bool UseFilter = true;
            public static bool ShowTime = false;
            public static int Height = 276;
            public static int FontSize = 14;
        }

        private static bool _UseFilter;
        private static bool _ShowTime;
        private static int _Height;
        private static int _FontSize;

        public static bool UseFilter { get => _UseFilter; set { if (value != _UseFilter) RageStorage.SetData("Settings::Chat::UseFilter", value); _UseFilter = value; Menu.UpdateToggle("sett-filter", value); } }
        public static bool ShowTime { get => _ShowTime; set { if (value != _ShowTime) RageStorage.SetData("Settings::Chat::ShowTime", value); _ShowTime = value; Menu.UpdateToggle("sett-timestamp", value); } }
        public static int Height { get => _Height; set { if (value != _Height) RageStorage.SetData("Settings::Chat::Height", value); _Height = value; Game.UI.CEF.Chat.SetHeight(value); } }
        public static int FontSize { get => _FontSize; set { if (value != _FontSize) RageStorage.SetData("Settings::Chat::FontSize", value); _FontSize = value; Game.UI.CEF.Chat.SetFontSize(value); } }
    }
}
