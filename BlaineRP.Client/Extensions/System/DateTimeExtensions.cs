using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Client.ExtensionsT.System
{
    internal static class DateTimeExtensions
    {
        public static long GetUnixTimestamp(this DateTime dt) => (long)(new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, TimeSpan.Zero)).Subtract(DateTimeOffset.UnixEpoch).TotalSeconds;

        public static long GetUnixTimestampMil(this DateTime dt) => (long)(new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, TimeSpan.Zero)).Subtract(DateTimeOffset.UnixEpoch).TotalMilliseconds;

        /// <summary>Проверка DateTime на спам</summary>
        /// <param name="dt">DateTime</param>
        /// <param name="timeout">Таймаут</param>
        /// <param name="updateTime">Обновить ли переданный DateTime на актуальный?</param>
        /// <param name="notify">Уведомить ли игрока о том, чтобы он подождал?</param>
        public static bool IsSpam(this ref DateTime dt, int timeout = 500, bool updateTime = false, bool notify = false) => CEF.Notification.SpamCheck(ref dt, timeout, updateTime, notify);
    }
}
