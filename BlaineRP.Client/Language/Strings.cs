using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BlaineRP.Client.Language
{
    public static class Strings
    {
        private static Dictionary<string, string> Texts = new Dictionary<string, string>()
        {
            #region TEXTS_TO_REPLACE

            #endregion
        };

        public static string? Get(string key, string otherwise, object[] formatArgs)
        {
            var str = Texts.GetValueOrDefault(key) ?? otherwise;

            if (str == null || formatArgs.Length == 0)
                return str;

            return string.Format(str, formatArgs);
        }

        public static string Get(string key, params object[] formatArgs) => Get(key, key, formatArgs);

        public static string? GetNullOtherwise(string key, params object[] formatArgs) => Get(key, null, formatArgs);

        public static string GetKeyFromTypeByMemberName(System.Type type, string memberName, string localKey = null)
        {
            var member = type.GetMember(memberName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();

            return member?.GetCustomAttributes<LocalizedAttribute>()?.Where(x => x.LocalKey == localKey).Select(x => x.GlobalKey).FirstOrDefault();
        }
    }
}
