using System.Linq;
using System.Reflection;

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
        
        public static string? GetKeyFromTypeByMemberName(System.Type type, string memberName, string localKey = null)
        {
            MemberInfo member = type.GetMember(memberName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault();

            return member?.GetCustomAttributes<LocalizedAttribute>()?.Where(x => x.LocalKey == localKey).Select(x => x.GlobalKey).FirstOrDefault();
        }
    }
}
