using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Server.Extensions.System.Collections.Generic
{
    internal static class ISetExtensions
    {
        public static ReadOnlySet<T> AsReadOnly<T>(this ISet<T> set)
        {
            return new ReadOnlySet<T>(set);
        }
    }
}
