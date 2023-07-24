using System.Collections.Generic;
using BlaineRP.Client.EntitiesData.Enums;
using BlaineRP.Client.Sync;

namespace BlaineRP.Client
{
    public static partial class Locale
    {
        // restricted chars for regex _|&^

        public static string Get(string key, params object[] formatArgs) => Language.Strings.Get(key, formatArgs);
        public static string? GetNullOtherwise(string key, params object[] formatArgs) => Language.Strings.GetNullOtherwise(key);

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

            public static Dictionary<uint, string> DefaultNumbersNames = new Dictionary<uint, string>()
            {
                { 900, "Банк" },
                { 873, "Сервис доставки" },
            };

            public static class Blip
            {
                public static Dictionary<Additional.ExtraBlip.Types, string> TypesNames = new Dictionary<Additional.ExtraBlip.Types, string>()
                {
                    { Additional.ExtraBlip.Types.GPS, "GPS-отметка" },
                    { Additional.ExtraBlip.Types.Furniture, "Мебель" },
                    { Additional.ExtraBlip.Types.AutoPilot, "Цель автопилота" },
                };

                public static string ApartmentsOwnedBlip = "{0}, кв. {1}";
                public static string GarageOwnedBlip = "{0}, #{1}";

                public static string JobTruckerPointAText = "Склад материалов";

                public static string JobTaxiTargetPlayer = "Заказчик такси";
            }

            #region Players
            public static class Players
            {
                public static Dictionary<SkillTypes, string> SkillNames = new Dictionary<SkillTypes, string>()
                {
                    { SkillTypes.Shooting, "Стрельба" },
                    { SkillTypes.Fishing, "Рыболовство" },
                    { SkillTypes.Cooking, "Кулинария" },
                    { SkillTypes.Strength, "Сила" },
                };

                public static Dictionary<SkillTypes, string> SkillNamesGenitive = new Dictionary<SkillTypes, string>()
                {
                    { SkillTypes.Shooting, "стрельбы" },
                    { SkillTypes.Fishing, "рыболовства" },
                    { SkillTypes.Cooking, "кулинарии" },
                    { SkillTypes.Strength, "силы" },
                };

                public static Dictionary<LicenseTypes, string> LicenseNames = new Dictionary<LicenseTypes, string>()
                {
                    { LicenseTypes.B, "B (легковой транспорт)" },
                    { LicenseTypes.C, "C (грузовой транспорт)" },
                    { LicenseTypes.D, "D (маршрутный транспорт)" },
                    { LicenseTypes.A, "D (мотоциклы)" },
                    { LicenseTypes.M, "M (мопеды)" },
                    { LicenseTypes.Sea, "Sea (водный транспорт)" },
                    { LicenseTypes.Fly, "Fly (воздушный транспорт)" },
                };

                public static Dictionary<AchievementTypes, (string Title, string Desc)> AchievementTexts = new Dictionary<AchievementTypes, (string, string)>()
                {
                    { AchievementTypes.SR1, ("В яблочко!", "Получите навык стрельбы 80 в тире") },
                    { AchievementTypes.SR2, ("Концентрация", "Продержите точность 100% в тире при навыке стрельбы 100") }
                };
            }
            #endregion
        }
        #endregion

        #region Other
        public static class HudMenu
        {
            public static Dictionary<CEF.HUD.Menu.Types, string> Names = new Dictionary<CEF.HUD.Menu.Types, string>()
            {
                { CEF.HUD.Menu.Types.Menu, "Меню" },
                { CEF.HUD.Menu.Types.Documents, "Документы" },
                { CEF.HUD.Menu.Types.Menu_House, "Меню дома" },
                { CEF.HUD.Menu.Types.Menu_Apartments, "Меню квартиры" },
                { CEF.HUD.Menu.Types.Job_Menu, "Меню работы" },
                { CEF.HUD.Menu.Types.Fraction_Menu, "Меню фракции" },

                { CEF.HUD.Menu.Types.Inventory, "Инвентарь" },
                { CEF.HUD.Menu.Types.Phone, "Телефон" },
                { CEF.HUD.Menu.Types.Animations, "Меню анимаций" },

                { CEF.HUD.Menu.Types.BlipsMenu, "Меню меток" },

                { CEF.HUD.Menu.Types.WeaponSkinsMenu, "Раскраски оружия" },

                { CEF.HUD.Menu.Types.Fraction_Police_TabletPC, "Служебный планшет" },
            };
        }
        #endregion
    }
}
