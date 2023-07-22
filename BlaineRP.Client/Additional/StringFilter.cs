using RAGE;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BlaineRP.Client.Additional
{
    class StringFilter
    {
        /*
           Swear Word Filter By frytech
         */

        private static List<string> ExceptionWords = new List<string>()
        {

        };

        private static Regex Pattern = new Regex(@"(?<=^|[^а-я])(([уyu]|[нзnz3][аa]|(хитро|не)?[вvwb][зz3]?[ыьъi]|[сsc][ьъ']|(и|[рpr][аa4])[зсzs]ъ?|([оo0][тбtb6]|[пp][оo0][дd9])[ьъ']?|(.\\B)+?[оаеиeo])?-?([еёe][бb6](?!о[рй])|и[пб][ае][тц]).*?|([нn][иеаaie]|([дпdp]|[вv][еe3][рpr][тt])[оo0]|[рpr][аa][зсzc3]|[з3z]?[аa]|с(ме)?|[оo0]([тt]|дно)?|апч)?-?[хxh][уuy]([яйиеёюuie]|ли(?!ган)).*?|([вvw][зы3z]|(три|два|четыре)жды|(н|[сc][уuy][кk])[аa])?-?[бb6][лl]([яy](?!(х|ш[кн]|мб)[ауеыио]).*?|[еэe][дтdt][ь']?)|([рp][аa][сзc3z]|[знzn][аa]|[соsc]|[вv][ыi]?|[пp]([еe][рpr][еe]|[рrp][оиioеe]|[оo0][дd])|и[зс]ъ?|[аоao][тt])?[пpn][иеёieu][зz3][дd9].*?|([зz3][аa])?[пp][иеieu][дd][аоеaoe]?[рrp](ну.*?|[оаoa][мm]|([аa][сcs])?([иiu]([лl][иiu])?[нщктлtlsn]ь?)?|([оo](ч[еиei])?|[аa][сcs])?[кk]([оo]й)?|[юu][гg])[ауеыauyei]?|[мm][аa][нnh][дd]([ауеыayueiи]([лl]([иi][сзc3щ])?[ауеыauyei])?|[оo][йi]|[аоao][вvwb][оo](ш|sh)[ь']?([e]?[кk][ауеayue])?|юк(ов|[ауи])?)|[мm][уuy][дd6]([яyаиоaiuo0].*?|[еe]?[нhn]([ьюия'uiya]|ей))|мля([тд]ь)?|лять|([нз]а|по)х|м[ао]л[ао]фь([яию]|[её]й))(?=($|[^а-я]))", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.IgnoreCase);

        public static string Beatify(string str, bool trim = true, bool removeExtraSpaces = true, bool removeExtraSymbols = false)
        {
            if (trim)
                str = str.Trim(' ');

            if (removeExtraSpaces)
                for (int i = 0; i < str.Length - 1; i++)
                    if (str[i] == ' ' && str[i + 1] == ' ')
                        str = str.Remove(i--, 1);

            if (removeExtraSymbols)
                for (int i = 0; i < str.Length - 1; i++)
                {
                    var lChar = char.ToLower(str[i]);

                    if (i + 3 < str.Length && lChar == char.ToLower(str[i + 1]) && lChar == char.ToLower(str[i + 2]) && lChar == char.ToLower(str[i + 3]))
                        str = str.Remove(i--, 1);
                }

            return str;
        }

        public static string Process(string str, bool checkExceptions, char replaceChar)
        {
            return Pattern.Replace(str, (match) =>
            {
                var value = match.Value;

                if (checkExceptions && ExceptionWords.Contains(value))
                    return value;

                return new string(replaceChar, value.Length);
            });
        }
    }
}
