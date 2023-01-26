﻿using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        #region General
        public static partial class General
        {
            public static string PropertyHouseString = "Дом";
            public static string PropertyApartmentsString = "Квартира";
            public static string PropertyGarageString = "Гараж";
            public static string PropertyBusinessClass = "Business";

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
            }

            #region Players
            public static class Players
            {
                public static string MaleNameDefault = "Гражданин";
                public static string FemaleNameDefault = "Гражданка";

                public static string Id = "({0})";

                public static string AdminLabel = "Администратор";

                public static string PlayerQuitText = "Игрок вышел {0} в {1}\nCID: #{2} | ID: {3}";

                public static Dictionary<Sync.Players.FractionTypes, string> FractionNames = new Dictionary<Sync.Players.FractionTypes, string>()
                {
                    { Sync.Players.FractionTypes.None, "Отсутствует" },
                };

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
            public static string Money = "Наличные: {0}$ | Банк: {1}$";
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
        }

        public static class HudMenu
        {
            public static Dictionary<CEF.HUD.Menu.Types, string> Names = new Dictionary<CEF.HUD.Menu.Types, string>()
            {
                { CEF.HUD.Menu.Types.Menu, "Меню" },
                { CEF.HUD.Menu.Types.Documents, "Документы" },
                { CEF.HUD.Menu.Types.Menu_House, "Меню дома" },
                { CEF.HUD.Menu.Types.Menu_Apartments, "Меню квартиры" },

                { CEF.HUD.Menu.Types.Inventory, "Инвентарь" },
                { CEF.HUD.Menu.Types.Phone, "Телефон" },
                { CEF.HUD.Menu.Types.Animations, "Меню анимаций" },

                { CEF.HUD.Menu.Types.BlipsMenu, "Меню меток" },

                { CEF.HUD.Menu.Types.WeaponSkinsMenu, "Раскраски оружия" },
            };
        }
        #endregion
    }
}