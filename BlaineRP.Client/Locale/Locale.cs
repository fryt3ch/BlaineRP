using System.Collections.Generic;

namespace BlaineRP.Client
{
    public static partial class Locale
    {
        // restricted chars for regex _|&^

        public static string Get(string key, params object[] formatArgs)
        {
            return Language.Strings.Get(key, formatArgs);
        }

        public static string? GetNullOtherwise(string key, params object[] formatArgs)
        {
            return Language.Strings.GetNullOtherwise(key);
        }

        #region General

        public static partial class General
        {
            public static Dictionary<uint, string> DefaultNumbersNames = new Dictionary<uint, string>()
            {
                { 900, "Банк" },
                { 873, "Сервис доставки" },
            };

            public static class Blip
            {
                public static string ApartmentsOwnedBlip = "{0}, кв. {1}";
                public static string GarageOwnedBlip = "{0}, #{1}";

                public static string JobTruckerPointAText = "Склад материалов";

                public static string JobTaxiTargetPlayer = "Заказчик такси";
            }
        }

        #endregion
    }
}