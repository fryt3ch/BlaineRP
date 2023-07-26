using System.Collections.Generic;

using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Data.Vehicles;
using BlaineRP.Client.Game.Jobs;

namespace BlaineRP.Client
{
    public static partial class Locale
    {
        public static class Property
        {
            public static Dictionary<VehicleTypes, string> VehicleTypesNames = new Dictionary<VehicleTypes, string>()
            {
                { VehicleTypes.Car, "Автомобиль" },
                { VehicleTypes.Boat, "Лодка" },
                { VehicleTypes.Motorcycle, "Мотоцикл" },
                { VehicleTypes.Cycle, "Велосипед" },
                { VehicleTypes.Helicopter, "Вертолет" },
                { VehicleTypes.Plane, "Самолет" },
            };

            public static string VehicleTradeInfoStr = "{0} | {1} #{2}";
            public static string VehicleTradeInfoStr1 = "{0} #{1}";
            public static string HouseTradeInfoStr = "Дом #{0}";
            public static string ApartmentsTradeInfoStr = "{0}, кв. {1}";
            public static string GarageTradeInfoStr = "{0}, #{1}";
            public static string BusinessTradeInfoStr = "{0} #{1}";

            public static Dictionary<JobTypes, string> JobNames = new Dictionary<JobTypes, string>()
            {
                { JobTypes.Trucker, "Доставка грузов" },
            };

            public static Dictionary<BusinessTypes, string> BusinessNames = new Dictionary<BusinessTypes, string>()
            {
                { BusinessTypes.ClothesShop1, "Магазин спортивной одежды" },
                { BusinessTypes.ClothesShop2, "Магазин премиальной одежды" },
                { BusinessTypes.ClothesShop3, "Магазин брендовой одежды" },

                { BusinessTypes.JewelleryShop, "Ювелирный салон" },

                { BusinessTypes.Market, "Магазин 24/7" },

                { BusinessTypes.GasStation, "АЗС" },

                { BusinessTypes.CarShop1, "Автосалон бюджетного сегмента" },

                { BusinessTypes.BoatShop, "Лодочный салон" },

                { BusinessTypes.AeroShop, "Салон воздушного транспорта" },

                { BusinessTypes.TuningShop, "Тюнинг" },

                { BusinessTypes.WeaponShop, "Оружейный магазин" },

                { BusinessTypes.BarberShop, "Салон красоты" },

                { BusinessTypes.TattooShop, "Тату-салон" },

                { BusinessTypes.BagShop, "Торговец сумок и рюкзаков" },
                { BusinessTypes.MaskShop, "Торговец масок" },

                { BusinessTypes.FurnitureShop, "Мебельный магазин" },

                { BusinessTypes.Farm, "Ферма" },
            };

            public static Dictionary<Game.UI.CEF.Shop.FurnitureSubTypes, string> FurnitureSubTypeNames { get; private set; } = new Dictionary<Game.UI.CEF.Shop.FurnitureSubTypes, string>()
            {
                { Game.UI.CEF.Shop.FurnitureSubTypes.Chairs, "Кресла и стулья" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Tables, "Столы" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Beds, "Кровати и диваны" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Closets, "Шкафы и тумбы" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Plants, "Растения" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Lamps, "Светильники" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Electronics, "Электроника" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Kitchen, "Все для кухни" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Bath, "Все для ванной" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Pictures, "Картины" },
                { Game.UI.CEF.Shop.FurnitureSubTypes.Decores, "Декор" },
            };

            public static string NoOwner = "Государство";

            public static string BankNameDef = "Банковское отделение";
            public static string AtmNameDef = "Банкомат";

            public static string GarageRootNameDef = "Гаражный комплекс";
            public static string GarageRootName = "Гаражный комплекс #{0}";

            public static string ApartmentsRootTextLabel = "{0}\nЭтажей: {1}\nКвартир свободно: {2}/{3}";
            public static string ApartmentsTextLabel = "Квартира #{0}\nВладелец: {1}";
            public static string HouseTextLabel = "Дом #{0}\nВладелец: {1}";

            public static string ApartmentsRootElevatorTextLabel = "Этаж: {0}\nКвартиры: {1} - {2}";

            public static string ApartmentsRootExitTextLabel = "Выход на улицу";
            public static string HouseExitTextLabel = "Выход";
        }

        public static class Shop
        {
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
