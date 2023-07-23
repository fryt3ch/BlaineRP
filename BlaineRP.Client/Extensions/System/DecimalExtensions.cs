namespace BlaineRP.Client.Extensions.System
{
    internal static class DecimalExtensions
    {
        public static bool IsNumberValid<T>(this decimal number, decimal min, decimal max, out T converted, bool notify = true)
        {
            if (number < min)
            {
                converted = default;

                if (notify)
                    CEF.Notification.ShowError(string.Format(Locale.Notifications.General.LessThanMinValue, min));

                return false;
            }
            else if (number > max)
            {
                converted = converted = default;

                if (notify)
                    CEF.Notification.ShowError(string.Format(Locale.Notifications.General.BiggerThanMaxValue, max));

                return false;
            }

            converted = (T)global::System.Convert.ChangeType(number, typeof(T));

            return true;
        }
    }
}
