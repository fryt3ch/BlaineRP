using System;

namespace BlaineRP.Client.Extensions.System
{
    internal static class TimeSpanExtensions
    {
        public static string GetBeautyString(this TimeSpan timeSpan)
        {
            if (timeSpan.Days > 365)
            {
                int years = timeSpan.Days / 365;

                if (years >= 3)
                    return Locale.Get("GEN_TIMESPAN_YEARS_1");

                return Locale.Get("GEN_TIMESPAN_YEARS_0", years);
            }

            int days = timeSpan.Days;

            if (days >= 1)
            {
                int hours = timeSpan.Hours;

                if (hours >= 1)
                    return Locale.Get("GEN_TIMESPAN_DAYS_HOURS_0", days, hours);

                return Locale.Get("GEN_TIMESPAN_DAYS_0", days);
            }

            int hours1 = timeSpan.Hours;

            if (hours1 >= 1)
            {
                int mins = timeSpan.Minutes;

                if (mins >= 1)
                    return Locale.Get("GEN_TIMESPAN_HOURS_MINS_0", hours1, mins);

                return Locale.Get("GEN_TIMESPAN_HOURS_0", hours1);
            }

            int mins1 = timeSpan.Minutes;

            int secs = timeSpan.Seconds;

            if (mins1 >= 1)
            {
                if (secs >= 1)
                    return Locale.Get("GEN_TIMESPAN_MINS_SECS_0", mins1, secs);

                return Locale.Get("GEN_TIMESPAN_MINS_0", mins1);
            }

            return Locale.Get("GEN_TIMESPAN_SECS_0", secs);
        }
    }
}