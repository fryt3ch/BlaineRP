using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Server.Language
{
    public static class Strings
    {
        public static string? Get(string key, System.Globalization.CultureInfo culture, string otherwise, object[] formatArgs)
        {
            var str = Properties.Language.ResourceManager.GetString(key, culture) ?? otherwise;

            if (str == null || formatArgs.Length == 0)
                return str;

            return string.Format(str, formatArgs);
        }

        public static string Get(string key, params object[] formatArgs) => Get(key, Properties.Language.Culture, key, formatArgs);

        public static string? GetNullOtherwise(string key) => Get(key);
    }
}
