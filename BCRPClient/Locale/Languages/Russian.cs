using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static Language RussianLanguage => new Language
        (
            ("HOUSE_STYLE_0@Name", "Вар. 1, Стиль: 1"),

            ("HOUSE_STYLE_OVERVIEW_T1", "{0} - завершить просмотр"),
            ("HOUSE_STYLE_OVERVIEW_T2", "{0} - смотреть другие")
        );
    }
}
