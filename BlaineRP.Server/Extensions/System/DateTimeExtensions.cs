using System;

namespace BlaineRP.Server.Extensions.System
{
    public static class DateTimeExtensions
    {
        public static long GetUnixTimestamp(this DateTime dt) => (long)(new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, TimeSpan.Zero)).Subtract(DateTimeOffset.UnixEpoch).TotalSeconds;

        public static long GetUnixTimestampMil(this DateTime dt) => (long)(new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, TimeSpan.Zero)).Subtract(DateTimeOffset.UnixEpoch).TotalMilliseconds;
    }
}