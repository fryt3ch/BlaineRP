namespace BlaineRP.Client.Additional
{
    [Script(int.MaxValue)]
    public class Discord
    {
        public const int StatusUpdateTime = 5_000;

        private static string _currentHeader;
        private static string _currentContent;

        public Discord()
        {
            SetDefault();

            (new AsyncTask(() => RAGE.Discord.Update(_currentContent, _currentHeader), StatusUpdateTime, true, StatusUpdateTime)).Run();
        }

        public static void SetStatus(string content, string header = null)
        {
            _currentContent = content;

            if (header != null)
                _currentHeader = header;

            RAGE.Discord.Update(_currentContent, _currentHeader);
        }

        public static void SetDefault()
        {
            _currentHeader = Language.Strings.Get("DISCORD_L_HEADER_0");
            _currentContent = string.Empty;

            RAGE.Discord.Update(_currentContent, _currentHeader);
        }
    }
}
