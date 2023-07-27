using System;

namespace BlaineRP.Server.Extensions.System
{
    public static class TimeSpanExtensions
    {
        public static string GetBeautyString(this TimeSpan ts)
        {
            var days = ts.Days;

            if (days >= 1)
            {
                var hours = ts.Hours;

                if (hours >= 1)
                    return $"{days} дн. и {hours} ч.";

                return $"{days} дн.";
            }

            var hours1 = ts.Hours;

            if (hours1 >= 1)
            {
                var mins = ts.Minutes;

                if (mins >= 1)
                    return $"{hours1} ч. и {mins} мин.";

                return $"{hours1} ч.";
            }

            var mins1 = ts.Minutes;

            var secs = ts.Seconds;

            if (mins1 >= 1)
            {
                if (secs >= 1)
                    return $"{mins1} мин. и {secs} сек.";

                return $"{mins1} мин.";
            }

            return $"{secs} сек.";
        }
    }
}