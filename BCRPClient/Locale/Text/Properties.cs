using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static class TestDrive
        {
            public static string CloseText = "Нажмите ESC, чтобы закончить тест-драйв";
            public static string TuningText = "Нажмите F4, чтобы открыть меню тюнинга";
        }

        public static class Property
        {
            public static Dictionary<Data.Vehicles.Vehicle.Types, string> VehicleTypesNames = new Dictionary<Data.Vehicles.Vehicle.Types, string>()
            {
                { Data.Vehicles.Vehicle.Types.Car, "Автомобиль" },
                { Data.Vehicles.Vehicle.Types.Boat, "Лодка" },
                { Data.Vehicles.Vehicle.Types.Motorcycle, "Мотоцикл" },
                { Data.Vehicles.Vehicle.Types.Cycle, "Велосипед" },
                { Data.Vehicles.Vehicle.Types.Helicopter, "Вертолет" },
                { Data.Vehicles.Vehicle.Types.Plane, "Самолет" },
            };

            public static string VehicleTradeInfoStr = "{0} | {1} #{2}";
            public static string VehicleTradeInfoStr1 = "{0} #{1}";
            public static string HouseTradeInfoStr = "Дом #{0}";
            public static string ApartmentsTradeInfoStr = "{0}, кв. {1}";
            public static string GarageTradeInfoStr = "{0}, #{1}";
            public static string BusinessTradeInfoStr = "{0} #{1}";

            public static Dictionary<Data.Locations.Business.Types, string> BusinessNames = new Dictionary<Data.Locations.Business.Types, string>()
            {
                { Data.Locations.Business.Types.ClothesShop1, "Магазин спортивной одежды" },
                { Data.Locations.Business.Types.ClothesShop2, "Магазин премиальной одежды" },
                { Data.Locations.Business.Types.ClothesShop3, "Магазин брендовой одежды" },

                { Data.Locations.Business.Types.JewelleryShop, "Ювелирный салон" },

                { Data.Locations.Business.Types.Market, "Магазин 24/7" },

                { Data.Locations.Business.Types.GasStation, "АЗС" },

                { Data.Locations.Business.Types.CarShop1, "Автосалон бюджетного сегмента" },

                { Data.Locations.Business.Types.BoatShop, "Лодочный салон" },

                { Data.Locations.Business.Types.AeroShop, "Салон воздушного транспорта" },

                { Data.Locations.Business.Types.TuningShop, "Тюнинг" },

                { Data.Locations.Business.Types.WeaponShop, "Оружейный магазин" },

                { Data.Locations.Business.Types.BarberShop, "Салон красоты" },

                { Data.Locations.Business.Types.TattooShop, "Тату-салон" },

                { Data.Locations.Business.Types.BagShop, "Торговец сумок и рюкзаков" },
                { Data.Locations.Business.Types.MaskShop, "Торговец масок" },

                { Data.Locations.Business.Types.FurnitureShop, "Мебельный магазин" },
            };

            public static Dictionary<CEF.Shop.FurnitureSubTypes, string> FurnitureSubTypeNames { get; private set; } = new Dictionary<CEF.Shop.FurnitureSubTypes, string>()
            {
                { CEF.Shop.FurnitureSubTypes.Chairs, "Кресла и стулья" },
                { CEF.Shop.FurnitureSubTypes.Tables, "Столы" },
                { CEF.Shop.FurnitureSubTypes.Beds, "Кровати и диваны" },
                { CEF.Shop.FurnitureSubTypes.Closets, "Шкафы и тумбы" },
                { CEF.Shop.FurnitureSubTypes.Plants, "Растения" },
                { CEF.Shop.FurnitureSubTypes.Lamps, "Светильники" },
                { CEF.Shop.FurnitureSubTypes.Electronics, "Электроника" },
                { CEF.Shop.FurnitureSubTypes.Kitchen, "Все для кухни" },
                { CEF.Shop.FurnitureSubTypes.Bath, "Все для ванной" },
                { CEF.Shop.FurnitureSubTypes.Pictures, "Картины" },
                { CEF.Shop.FurnitureSubTypes.Decores, "Декор" },
            };

            public static Dictionary<Data.Locations.ApartmentsRoot.Types, string> ApartmentsRootNames = new Dictionary<Data.Locations.ApartmentsRoot.Types, string>()
            {
                { Data.Locations.ApartmentsRoot.Types.Cheap1, "ЖК Paleto" },
            };

            public static string NoOwner = "Государство";

            public static string BankNameDef = "Банковское отделение";
            public static string AtmNameDef = "Банковское отделение";

            public static string GarageRootNameDef = "Гаражный комплекс";
            public static string GarageRootName = "Гаражный комплекс #{0}";

            public static string ApartmentsRootTextLabel = "{0}\nЭтажей: {1}\nКвартир свободно: {2}/{3}";
            public static string ApartmentsTextLabel = "Квартира #{0}\nВладелец: {1}";
            public static string HouseTextLabel = "Дом #{0}\nВладелец: {1}";

            public static string ApartmentsRootElevatorTextLabel = "Лифт [{0} этаж]";

            public static string ApartmentsRootExitTextLabel = "Выход на улицу";
            public static string HouseExitTextLabel = "Выход";
        }

        public static class Shop
        {
            public static string ModDeletionTitle = "Удаление модификации";

            public static string ModDeletionText = "Вы собираетесь удалить {0} со своего транспорта.\n\nДанное действие необратимо!\nВыберите способ оплаты, чтобы продолжить.";

            public static string BarberShopLipstickLabel = "Помада";
            public static string BarberShopBlushLabel = "Румяна";
            public static string BarberShopMakeupLabel = "Макияж";

            public static Dictionary<string, string> BarberShopNames = new Dictionary<string, string>()
            {
                { "lipstick_0", "Нет" },
                { "lipstick_1", "Средняя матовая" },
                { "lipstick_2", "Средняя глянцевая" },
                { "lipstick_3", "Матовая" },
                { "lipstick_4", "Глянцевая" },
                { "lipstick_5", "Насыщенная матовая" },
                { "lipstick_6", "Насыщенная глянцевая" },
                { "lipstick_7", "Стёртая матовая" },
                { "lipstick_8", "Стёртая глянцевая" },
                { "lipstick_9", "Размазанная" },
                { "lipstick_10", "Гейша" },

                { "blush_0", "Нет" },
                { "blush_1", "Полностью" },
                { "blush_2", "По углам" },
                { "blush_3", "Вокруг" },
                { "blush_4", "Горизонтально" },
                { "blush_5", "От глаз" },
                { "blush_6", "Сердечки" },
                { "blush_7", "Восьмидесятые" },

                { "makeup_0", "Нет" },
                { "makeup_1", "Дымчатый чёрный" },
                { "makeup_2", "Бронзовый" },
                { "makeup_3", "Мягкий серый" },
                { "makeup_4", "Ретро" },
                { "makeup_5", "Естественный" },
                { "makeup_6", "Кошачьи глазки" },
                { "makeup_7", "Чола" },
                { "makeup_8", "Вамп" },
                { "makeup_9", "Вайнвудский гламур" },
                { "makeup_10", "Бабблгам" },
                { "makeup_11", "Аква дрим" },
                { "makeup_12", "Пин ап" },
                { "makeup_13", "Фиолетовая страсть" },
                { "makeup_14", "Дымчатый кошачий глаз" },
                { "makeup_15", "Тлеющий рубин" },
                { "makeup_16", "Поп-принцесса" },
                { "makeup_17", "Гайлайнер" },
                { "makeup_18", "Кровавые слезы" },
                { "makeup_19", "Хэви метал" },
                { "makeup_20", "Скорбь" },
                { "makeup_21", "Принц тьмы" },
                { "makeup_22", "Легкая тушь" },
                { "makeup_23", "Гот" },
                { "makeup_24", "Панк" },
                { "makeup_25", "Потекшая тушь" },
            };
        }
    }
}
