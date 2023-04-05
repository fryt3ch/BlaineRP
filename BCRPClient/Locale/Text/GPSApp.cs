using System.Collections.Generic;
using System.Linq;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static class GPSApp
        {
            public static Dictionary<string, string> Names = new Dictionary<string, string>()
            {
                { "closest0", "Ближайшее" }, { "closest1", "Ближайший" }, { "closest2", "Ближайшая" },

                { "money", "Деньги" }, { "banks", "Банковские отделения" }, { "atms", "Банкоматы" }, { "bank", "Отделение" }, { "atm", "Банкомат" },

                { "clothes", "Магазины одежды" }, { "clothes1", "Бюджетные" }, { "clothes2", "Премиум" }, { "clothes3", "Брендовые" }, { "clothesother", "Прочее" }, { "clothess", "Магазин одежды" },

                { "bizother", "Прочие бизнесы" }, { "gas", "Заправочные станции" }, { "tuning", "Тюнинг-салоны" }, { "market", "Магазины 24/7" }, { "weapon", "Магазины оружия" }, { "furn", "Магазины мебели" }, { "farm", "Фермы" },

                { "job", "Работы" }, { "jobfarm", "Фермы" },

                { "miscpl", "Прочие места" }

                //{ "veh", "Магазины транспорта" }, { "cars", "Автомобили" }, { "bikes", "Мотоциклы" }, { "boats", "Лодки" }, { "planes", "Воздушные" },
            };
        }
    }
}