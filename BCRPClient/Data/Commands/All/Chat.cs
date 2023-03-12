namespace BCRPClient.Data
{
    partial class Commands
    {
        [Command("fontsize", false, "Задать размер шрифта в чате", "fsize")]
        public static void FontSize(int value)
        {
            if (value > 30 || value < 1)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Chat.Header, Locale.Notifications.Commands.Chat.WrongValue);

                return;
            }

            Settings.Chat.FontSize = value;
        }

        [Command("chatheight", false, "Задать высоту чата", "cheight")]
        public static void ChatHeight(int value)
        {
            if (value > 276 || value < 0)
            {
                CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.Commands.Chat.Header, Locale.Notifications.Commands.Chat.WrongValue);

                return;
            }

            Settings.Chat.Height = value;
        }

        [Command("chathide", false, "Скрыть/показать чат", "chide")]
        public static void ChatHide()
        {
            Settings.Chat.Height = Settings.Chat.Height == 0 ? Settings.Chat.Default.Height : 0;
        }
    }
}
