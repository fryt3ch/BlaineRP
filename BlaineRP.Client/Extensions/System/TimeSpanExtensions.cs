using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Client.ExtensionsT.System
{
    internal static class TimeSpanExtensions
    {
        public static string GetBeautyString(this TimeSpan timeSpan)
        {
            if (timeSpan.Days > 365)
            {
                var years = timeSpan.Days / 365;

                if (years >= 3)
                    return Locale.Get("GEN_TIMESPAN_YEARS_1");

                return Locale.Get("GEN_TIMESPAN_YEARS_0", years);
            }

            var days = timeSpan.Days;

            if (days >= 1)
            {
                var hours = timeSpan.Hours;

                if (hours >= 1)
                    return Locale.Get("GEN_TIMESPAN_DAYS_HOURS_0", days, hours);

                return Locale.Get("GEN_TIMESPAN_DAYS_0", days);
            }

            var hours1 = timeSpan.Hours;

            if (hours1 >= 1)
            {
                var mins = timeSpan.Minutes;

                if (mins >= 1)
                    return Locale.Get("GEN_TIMESPAN_HOURS_MINS_0", hours1, mins);

                return Locale.Get("GEN_TIMESPAN_HOURS_0", hours1);
            }

            var mins1 = timeSpan.Minutes;

            var secs = timeSpan.Seconds;

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
