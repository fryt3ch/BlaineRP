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

        public static bool UseFilter { get => _UseFilter; set { if (value != _UseFilter) Additional.Storage.SetData("Settings::Chat::UseFilter", value); _UseFilter = value; CEF.Menu.UpdateToggle("sett-filter", value); } }
        public static bool ShowTime { get => _ShowTime; set { if (value != _ShowTime) Additional.Storage.SetData("Settings::Chat::ShowTime", value); _ShowTime = value; CEF.Menu.UpdateToggle("sett-timestamp", value); } }
        public static int Height { get => _Height; set { if (value != _Height) Additional.Storage.SetData("Settings::Chat::Height", value); _Height = value; CEF.Chat.SetHeight(value); } }
        public static int FontSize { get => _FontSize; set { if (value != _FontSize) Additional.Storage.SetData("Settings::Chat::FontSize", value); _FontSize = value; CEF.Chat.SetFontSize(value); } }
    }
}
