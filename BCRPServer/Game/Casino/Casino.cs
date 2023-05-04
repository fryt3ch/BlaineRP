using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;

namespace BCRPServer.Game.Casino
{
    public class Casino
    {
        private static List<Casino> All { get; set; }

        public static Casino GetById(int id) => id < 0 || id >= All.Count ? null : All[id];

        public int Id => All.IndexOf(this);

        public Roulette[] Roulettes { get; set; }

        public LuckyWheel[] LuckyWheels { get; set; }

        public Casino()
        {
            All.Add(this);
        }

        public Roulette GetRouletteById(int id) => id < 0 || id >= Roulettes.Length ? null : Roulettes[id];

        public static void InitializeAll()
        {
            if (All != null)
                return;

            All = new List<Casino>();

            var mainCasino = new Casino()
            {
                Roulettes = new Roulette[]
                {
                    new Roulette(1031.199f, 64.15562f, 71.4761f)
                    {
                        MinBet = 250,
                        MaxBet = 15000,
                    },

                    new Roulette(1024.319f, 62.52872f, 71.4761f)
                    {
                        MinBet = 100,
                        MaxBet = 5000,
                    },

                    new Roulette(1032.156f, 60.01989f, 71.4761f)
                    {
                        MinBet = 5,
                        MaxBet = 1000,
                    },

                    new Roulette(1025.035f, 58.33623f, 71.4761f)
                    {
                        MinBet = 5,
                        MaxBet = 1000,
                    },
                },
            };

            var lines = new List<string>();

            for (int i = 0; i < All.Count; i++)
            {
                var x = All[i];

                lines.Add($"new Casino({i}, \"{x.Roulettes.Select(x => new object[] { x.MinBet, x.MaxBet }).ToList().SerializeToJson().Replace('\"', '\'')}\");");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "CASINOS_TO_REPLACE", lines);
        }
    }
}
