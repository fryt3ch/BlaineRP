using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BlaineRP.Server.Extensions.System.Collections.Generic
{
    internal static class DictionaryExtensions
    {
        public static IReadOnlyDictionary<TKey, TValue> AsReadOnly<TKey, TValue>(this Dictionary<TKey, TValue> dictionry)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionry);
        }
    }
}
