using System.Collections.Generic;
using BlaineRP.Client.Game.NPCs.Dialogues;

namespace BlaineRP.Client
{
    public static partial class Locale
    {
        public static partial class General
        {
            public static class NPC
            {
                public static Dictionary<Dialogue.TimeTypes, Dictionary<int, string>> TimeWords = new Dictionary<Dialogue.TimeTypes, Dictionary<int, string>>()
                {
                    {
                        Dialogue.TimeTypes.Morning, new Dictionary<int, string>()
                        {
                            { 0, "утро" },
                        }
                    },
                    {
                        Dialogue.TimeTypes.Day, new Dictionary<int, string>()
                        {
                            { 0, "день" },
                        }
                    },
                    {
                        Dialogue.TimeTypes.Evening, new Dictionary<int, string>()
                        {
                            { 0, "вечер" },
                        }
                    },
                    {
                        Dialogue.TimeTypes.Night, new Dictionary<int, string>()
                        {
                            { 0, "ночь" },
                        }
                    },
                };
            }
        }
    }
}