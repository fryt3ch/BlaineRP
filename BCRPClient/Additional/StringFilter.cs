using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Additional
{
    class StringFilter : Events.Script
    {
        /*
           Swear Word Filter By frytech
         */

        private static List<string> ExceptionWords = new List<string>();
        private static List<string> FilterWords = new List<string>();

        public StringFilter()
        {
            Events.Add("StringFilter::LoadIn", (object[] args) =>
            {
                FilterWords = RAGE.Util.Json.Deserialize<List<string>>((string)args[0]);
                ExceptionWords = RAGE.Util.Json.Deserialize<List<string>>((string)args[1]);
            });
        }

        public static void Load() => Events.CallLocal("StringFilter::LoadOut");

        public static void Unload()
        {
            FilterWords.Clear();
            ExceptionWords.Clear();
        }

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

        public static string Process(string str, bool checkExceptions = true, bool replaceAllWord = true, char replaceChar = '♡')
        {
            char?[] removedChars = new char?[str.Length];
            bool[] upperChars = new bool[str.Length];

            for (int i = 0, k = 0; i < str.Length; i++, k++)
            {
                if (char.IsUpper(str[i]))
                    upperChars[k] = true;

                if (char.IsWhiteSpace(str[i]) || char.IsPunctuation(str[i]) || (i + 1 < str.Length && char.ToLower(str[i]) == char.ToLower(str[i + 1])))
                {
                    removedChars[k] = str[i];

                    str = str.Remove(i--, 1);
                }
            }

            str = str.ToLower();

            foreach (var word in FilterWords)
            {
                int idx = -1;

                while ((idx = str.IndexOf(word, idx + 1)) != -1)
                {
                    if (checkExceptions)
                    {
                        bool isExcept = false;

                        foreach (var exception in ExceptionWords)
                        {
                            if (!exception.Contains(word))
                                continue;

                            int idxEx = str.IndexOf(exception);

                            if (idxEx == -1)
                                continue;

                            if (idxEx + exception.IndexOf(word) == idx)
                            {
                                isExcept = true;

                                break;
                            }
                        }

                        if (isExcept)
                            continue;
                    }

                    str = str.Remove(idx, word.Length);
                    str = str.Insert(idx, new string(replaceChar, word.Length));
                }
            }

            StringBuilder strB = new StringBuilder();

            int j = 0;

            for (int i = 0; i < removedChars.Length; i++)
            {
                if (removedChars[i] != null)
                    strB.Append(removedChars[i]);
                else if (j < str.Length)
                    strB.Append(str[j++]);

                if (upperChars[i])
                    strB[i] = char.ToUpper(strB[i]);
            }

            if (replaceAllWord)
            {
                int nextPos = -1;

                for (int i = 0; i < strB.Length; i++)
                {
                    if (nextPos == -1 && ((!char.IsWhiteSpace(strB[i]) && !char.IsPunctuation(strB[i])) || strB[i] == replaceChar))
                    {
                        bool containsFilter = false;

                        for (int k = i; k < strB.Length; k++)
                        {
                            if ((char.IsWhiteSpace(strB[k]) || char.IsPunctuation(strB[k])) && strB[k] != replaceChar)
                                break;

                            if (strB[k] == replaceChar)
                                containsFilter = true;

                            nextPos = k;
                        }

                        if (!containsFilter)
                            nextPos = -1;
                    }

                    if (i <= nextPos)
                    {
                        strB[i] = replaceChar;
                    }
                    else
                        nextPos = -1;
                }
            }

            return strB.ToString();
        }
    }
}
