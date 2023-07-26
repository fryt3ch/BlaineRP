using BlaineRP.Client.Game.UI.CEF;

namespace BlaineRP.Client.Extensions.System
{
    internal static class StringExtensions
    {
        public static bool IsTextLengthValid(this string text, int minLength, int maxLength, bool notify)
        {
            if (text == null || text.Length < minLength)
            {
                Notification.ShowError(string.Format(Locale.Notifications.General.MinimalCharactersCount, minLength));

                return false;
            }

            if (text.Length > maxLength)
            {
                Notification.ShowError(string.Format(Locale.Notifications.General.MaximalCharactersCount, maxLength));

                return false;
            }

            return true;
        }
    }
}