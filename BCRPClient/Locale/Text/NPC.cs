using System.Collections.Generic;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static partial class General
        {
            public static class NPC
            {
                public static string NotFamiliarMale = "Незнакомец";
                public static string NotFamiliarFemale = "Незнакомка";

                public static Dictionary<string, string> TypeNames = new Dictionary<string, string>()
                {
                    { "quest", "Квестодатель" },

                    { "seller", "Продавец" },
                    { "vendor", "Торговец" },
                    { "tatseller", "Тату-мастер" },

                    { "fishbuyer", "Скупщик рыбы" },

                    { "bank", "Работник банка" },
                    { "vpound", "Работник штрафстоянки" },
                    { "vrent", "Арендодатель транспорта" },

                    { "job_trucker", "Глава дальнобойщиков" },

                    { "farmer", "Фермер" },

                    { "cop0", "Дежурный полиции" },
                    { "drivingschool", "Менеджер автошколы" },

                    { "Casino_Roulette", "Крупье (рулетка)" },
                };

                public static Dictionary<Data.Dialogue.TimeTypes, Dictionary<int, string>> TimeWords = new Dictionary<Data.Dialogue.TimeTypes, Dictionary<int, string>>()
                {
                    {
                        Data.Dialogue.TimeTypes.Morning,

                        new Dictionary<int, string>()
                        {
                            { 0, "утро" },
                        }
                    },

                    {
                        Data.Dialogue.TimeTypes.Day,

                        new Dictionary<int, string>()
                        {
                            { 0, "день" },
                        }
                    },

                    {
                        Data.Dialogue.TimeTypes.Evening,

                        new Dictionary<int, string>()
                        {
                            { 0, "вечер" },
                        }
                    },

                    {
                        Data.Dialogue.TimeTypes.Night,

                        new Dictionary<int, string>()
                        {
                            { 0, "ночь" },
                        }
                    },
                };
            }
        }
    }
}
