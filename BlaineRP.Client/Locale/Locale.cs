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
            public static string PropertyHouseString = "Дом";
            public static string PropertyApartmentsString = "Квартира";
            public static string PropertyGarageString = "Гараж";
            public static string PropertyBusinessClass = "Business";

            public static string FiveNotificationDefSubj = "Новое сообщение";
            public static string FiveNotificationIncCallSubj = "Входящий вызов";
            public static string FiveNotificationEndCallSubj0 = "Звонок завершен";
            public static string FiveNotificationEndCallSubj1 = "Звонок завершен вами";
            public static string FiveNotificationEndCallSubj2 = "Звонок завершен собеседником";
            public static string FiveNotificationEndCallSubj10 = "Звонок завершен (у Вас закончились средства)";
            public static string FiveNotificationEndCallSubj20 = "Звонок завершен (у собеседника закончились средства)";

            public static string FiveNotificationIncCallText = "Откройте телефон ({0}) и примите/отклоните вызов";

            public static string FiveNotificationEndedCallTextT = "Продолжительность звонка: {0}";

            public static string PhoneOutgoingCall = "Исходящий вызов";
            public static string PhoneIncomingCall = "Входящий вызов";

            public static Dictionary<uint, string> DefaultNumbersNames = new Dictionary<uint, string>()
            {
                { 900, "Банк" },
                { 873, "Сервис доставки" },
            };

            public static class PhoneCamera
            {
                public const string On = "вкл.";
                public const string Off = "выкл.";

                public const string Bokeh = "Боке";
                public const string CamOffset = "Смещение камеры";
                public const string HeadOffset = "Наклон головы";
                public const string Animation = "Анимация";
                public const string Emotion = "Эмоция";
                public const string Zoom = "Зум";
                public const string Filter = "Фильтр";
                public const string FrontCam = "Передняя камера";
                public const string BackCam = "Задняя камера";
                public const string Exit = "Выход";
                public const string Photo = "Фото";
            }

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