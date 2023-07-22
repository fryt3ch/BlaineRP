using System.Collections.Generic;

namespace BlaineRP.Client
{
    public static partial class Locale
    {
        public static partial class General
        {
            public static class NPC
            {
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
