using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer
{
    class Locale
    {
        #region Chat
        public class Chat
        {
            public class Vehicle
            {
                public static string BeltOn = "пристегнул(а) ремень безопасности";
                public static string BeltOff = "отстегнул(а) ремень безопасности";

                public static string EngineOn = "запустил(а) двигатель";
                public static string EngineOff = "заглушил(а) двигатель";
                public static string OutOfFuel = "Закончилось топливо";

                public static string Locked = "заблокировал(а) двери";
                public static string Unlocked = "разблокировал(а) двери";

                public static string TrunkOn = "открыл(а) багажник";
                public static string TrunkOff = "закрыл(а) багажник";
                public static string HoodOn = "открыл(а) капот";
                public static string HoodOff = "закрыл(а) капот";

                public static string Kick = "вышвырнул(а)";

                public static string NoNumberplate = "без номерного знака";
            }

            public class Player
            {
                public static string PhoneOn = "достал(а) телефон";
                public static string PhoneOff = "убрал(а) телефон";

                public static string PointsAt = "показал(а) пальцем на";
                public static string PointsAtPerson = "показал(а) пальцем на человека";

                public static string Take = "подобрал(а)";
            }

            public class Server
            {
                public static string ClearItemsSoon = "Через {0} секунд будет произведена очистка предметов, лежащих на земле!";
                public static string ClearItems = "Было удалено {0} предметов!";
                public static string ClearItemsCancelled = "Удаление предметов было отменено!";
            }

            public static class Admin
            {
                public static string SilentKick = "[SKick] {0} выгнал игрока {1} с сервера. Причина: {2}";

                public static string TeleportWarning = "[BAC] Игрок {0} подозревается в телепорте! Дистанция: {1}";
            }
        }
        #endregion

        #region General
        public class General
        {
            public static string WrongData = "Wrong Data";

            public class Auth
            {
                public static string AttemptsOver = "Попытки входа закончились!";

                public static string NoFraction = "нет фракции";
            }

            public class GlobalBan
            {
                public const string Header = "Глобальная блокировка";

                public static Dictionary<AccountData.GlobalBan.Types, string> TypesNames = new Dictionary<AccountData.GlobalBan.Types, string>()
                {
                    { AccountData.GlobalBan.Types.IP, "IP-адрес" },
                    { AccountData.GlobalBan.Types.HWID, "Серийный номер" },
                    { AccountData.GlobalBan.Types.SCID, "Social Club" },
                    { AccountData.GlobalBan.Types.Blacklist, "ЧС проекта" },
                };

                public static string NotificationText = "ID блокировки: {0}\nТип: {1}\nДата: {2}\nПричина: {3}\nID администратора: #{4}";
            }
        }
        #endregion

        public class Businesses
        {
            public static string Information = "{0} #{1}\nВладелец: {2}";

            public static string Government = "Государство";

            public static string ClothesShop1 = "Магазин спортивной одежды";
            public static string ClothesShop2 = "Магазин премиальной одежды";
            public static string ClothesShop3 = "Магазин брендовой одежды";

            public static string Header = "Магазин";
        }

        public class Houses
        {
            public static string HouseInfo = "Дом #{0}";
            public static string ApartmentsInfo = "Квартира #{0}";
            public static string ApartmentsRootInfo = "Многоквартирный жилой дом\n{0}";
            public static string ApartmentsRootBlip = "МКД \n{0}";

            public static string Exit = "Выход";
        }

        #region Blips
        public class Blips
        {
            public static string Custom = "Таможня";
            public static string Blockpost = "Блок-пост";
            public static string Pierce = "Пирс";
            public static string Beach = "Пляж";
            public static string Prison = "Федеральная тюрьма";
            public static string Milbase = "Военная база";
            public static string Church = "Церковь";
            public static string Firestation = "Пожарная станция";
            public static string Hospital = "Больница";
            public static string Police = "Полиция";
            public static string StrangePlace = "Загадочное место";
            public static string FurnitureShop = "Магазин мебели";
        }
        #endregion
    }
}
