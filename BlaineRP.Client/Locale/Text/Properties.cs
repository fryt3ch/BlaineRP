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
            public static string VehicleTradeInfoStr = "{0} | {1} #{2}";
            public static string VehicleTradeInfoStr1 = "{0} #{1}";
            public static string HouseTradeInfoStr = "Дом #{0}";
            public static string ApartmentsTradeInfoStr = "{0}, кв. {1}";
            public static string GarageTradeInfoStr = "{0}, #{1}";
            public static string BusinessTradeInfoStr = "{0} #{1}";

            public static Dictionary<BusinessType, string> BusinessNames = new Dictionary<BusinessType, string>()
            {
                { BusinessType.ClothesShop1, "Магазин спортивной одежды" },
                { BusinessType.ClothesShop2, "Магазин премиальной одежды" },
                { BusinessType.ClothesShop3, "Магазин брендовой одежды" },
                { BusinessType.JewelleryShop, "Ювелирный салон" },
                { BusinessType.Market, "Магазин 24/7" },
                { BusinessType.GasStation, "АЗС" },
                { BusinessType.CarShop1, "Автосалон бюджетного сегмента" },
                { BusinessType.BoatShop, "Лодочный салон" },
                { BusinessType.AeroShop, "Салон воздушного транспорта" },
                { BusinessType.TuningShop, "Тюнинг" },
                { BusinessType.WeaponShop, "Оружейный магазин" },
                { BusinessType.BarberShop, "Салон красоты" },
                { BusinessType.TattooShop, "Тату-салон" },
                { BusinessType.BagShop, "Торговец сумок и рюкзаков" },
                { BusinessType.MaskShop, "Торговец масок" },
                { BusinessType.FurnitureShop, "Мебельный магазин" },
                { BusinessType.Farm, "Ферма" },
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