using System.Collections.Generic;

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
                public static Dictionary<Sync.Players.SkillTypes, string> SkillNames = new Dictionary<Sync.Players.SkillTypes, string>()
                {
                    { Sync.Players.SkillTypes.Shooting, "Стрельба" },
                    { Sync.Players.SkillTypes.Fishing, "Рыболовство" },
                    { Sync.Players.SkillTypes.Cooking, "Кулинария" },
                    { Sync.Players.SkillTypes.Strength, "Сила" },
                };

                public static Dictionary<Sync.Players.SkillTypes, string> SkillNamesGenitive = new Dictionary<Sync.Players.SkillTypes, string>()
                {
                    { Sync.Players.SkillTypes.Shooting, "стрельбы" },
                    { Sync.Players.SkillTypes.Fishing, "рыболовства" },
                    { Sync.Players.SkillTypes.Cooking, "кулинарии" },
                    { Sync.Players.SkillTypes.Strength, "силы" },
                };

                public static Dictionary<Sync.Players.LicenseTypes, string> LicenseNames = new Dictionary<Sync.Players.LicenseTypes, string>()
                {
                    { Sync.Players.LicenseTypes.B, "B (легковой транспорт)" },
                    { Sync.Players.LicenseTypes.C, "C (грузовой транспорт)" },
                    { Sync.Players.LicenseTypes.D, "D (маршрутный транспорт)" },
                    { Sync.Players.LicenseTypes.A, "D (мотоциклы)" },
                    { Sync.Players.LicenseTypes.M, "M (мопеды)" },
                    { Sync.Players.LicenseTypes.Sea, "Sea (водный транспорт)" },
                    { Sync.Players.LicenseTypes.Fly, "Fly (воздушный транспорт)" },
                };

                public static Dictionary<Sync.Players.AchievementTypes, (string Title, string Desc)> AchievementTexts = new Dictionary<Sync.Players.AchievementTypes, (string, string)>()
                {
                    { Sync.Players.AchievementTypes.SR1, ("В яблочко!", "Получите навык стрельбы 80 в тире") },
                    { Sync.Players.AchievementTypes.SR2, ("Концентрация", "Продержите точность 100% в тире при навыке стрельбы 100") }
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
