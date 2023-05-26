using System.Collections.Generic;

namespace BCRPClient
{
    public static partial class Locale
    {
        public class Language
        {
            private Dictionary<string, string> Texts { get; set; }

            public Language(params (string Key, string Value)[] Texts)
            {
                this.Texts = new Dictionary<string, string>();

                for (int i = 0; i < Texts.Length; i++)
                {
                    var x = Texts[i];

                    Add(x.Key, x.Value);
                }
            }

            public string Get(string key, string otherwise) => Texts.GetValueOrDefault(key, otherwise);

            public void Add(string key, string value)
            {
                if (!this.Texts.TryAdd(key, value))
                    this.Texts[key] = value;
            }
        }

        private static Language CurrentLanguage { get; } = Settings.LANGUAGE == "ru" ? RussianLanguage : RussianLanguage;

        public static string Get(string key, string otherwise = null) => CurrentLanguage.Get(key, otherwise);

        public static void Add(string key, string value) => CurrentLanguage.Add(key, value);

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

            public static Dictionary<byte, string> PoliceTabletCallTypes = new Dictionary<byte, string>()
            {
                { 0, "КОД-0" },
                { 1, "КОД-1" },
                { 2, "КОД-2" },

                { 255, "Вызов" },
            };

            public static Dictionary<byte, string> PoliceTabletCallMessages = new Dictionary<byte, string>()
            {
                { 0, "Требуется немедленная поддержка! Всем департаментам!" },
                { 1, "Требуется небольшая помощь!" },
                { 2, "Требуется помощь!" },
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
                public static string MaleNameDefault = "Гражданин";
                public static string FemaleNameDefault = "Гражданка";

                public static string Id = "({0})";

                public static string AdminLabel = "Администратор";

                public static string PlayerQuitText = "Игрок вышел {0} в {1}\nCID: #{2} | ID: {3}";

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

            public static class Business
            {
                public static string InfoColshape = "{0} #{1}";

                public static string NothingItem = "Ничего";

                public static string TuningNeon = "Неон";
                public static string TuningColours = "Цвета покраски";
                public static string TuningPearl = "Перламутр";
                public static string TuningWheelColour = "Цвет покрышек";
                public static string TuningTyreSmokeColour = "Цвет дыма от колес";

                public static string ShootingRangeTitle = "Тир";
            }

            public static class Documents
            {
                public static string SexMale = "мужской";
                public static string SexFemale = "женский";

                public static string NotMarriedMale = "не женат";
                public static string NotMarriedFemale = "не замужем";

                public static string VehiclePassportNoPlate = "отсутствует";
            }
        }
        #endregion

        #region Other
        public static class PauseMenu
        {
            public static string Money = "Наличные: {0} | Банк: {1}";
        }

        public static class Scaleform
        {
            public static class Wasted
            {
                public static string Header = "Вы при смерти";

                public static string TextAttacker = "Атакующий: {0} | CID: #{1}";
                public static string TextSelf = "Несчастный случай";
            }

            public static string ShootingRangeCountdownTitle = "~g~Приготовьтесь!";
            public static string ShootingRangeCountdownText = "Начало через: {0}";

            public static string ShootingRangeScoreText = "Счёт: {0} / {1}";
            public static string ShootingRangeAccuracyText = "Точность: {0}%";

            public const string JobBusDriverWaitTitle = "~g~Ожидание пассажиров";
            public const string JobCollectorWaitTitle = "~g~Загрузка денег";
            public const string JobCollectorWaitTitle1 = "~g~Выгрузка денег";
            public const string JobTruckerLoadMaterialsTitle = "~g~Загрузка материалов";
            public const string JobTruckerUnloadMaterialsTitle = "~g~Выгрузка материалов";
            public const string JobTruckerLoadMaterialsText = "Подождите еще {0} сек.";
        }

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
