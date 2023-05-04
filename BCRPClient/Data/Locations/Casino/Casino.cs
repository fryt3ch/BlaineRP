using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data
{
    public partial class Locations
    {
        public partial class Casino
        {
            public static List<Casino> All { get; set; } = new List<Casino>();

            public static Casino GetById(int id) => id < 0 || id >= All.Count ? null : All[id];

            public int Id => All.IndexOf(this);

            public List<Roulette> Roulettes { get; set; }

            public Roulette GetRouletteById(int id) => id < 0 || id >= Roulettes.Count ? null : Roulettes[id];

            public Casino(int Id, string RoulettesDataJs)
            {
                All.Add(this);

                var roulettesData = RAGE.Util.Json.Deserialize<object[]>(RoulettesDataJs);

                if (Id == 0)
                {
                    var mainCs = new Additional.Circle(new Vector3(63.4196f, 47.85423f, 74.31705f), 40f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                    {

                    };

                    Roulettes = new List<Roulette>()
                    {
                        new Roulette(Id, 0, "vw_prop_casino_roulette_01b", 1031.199f, 64.15562f, 71.4761f, 134.69f - 32.01f + 90f),
                        new Roulette(Id, 1, "vw_prop_casino_roulette_01b", 1024.319f, 62.52872f, 71.4761f, 134.69f - 32.01f + 90f),

                        new Roulette(Id, 2, "vw_prop_casino_roulette_01", 1032.156f, 60.01989f, 71.4761f, -45.31f - 32.01f + 90f),
                        new Roulette(Id, 3, "vw_prop_casino_roulette_01", 1025.035f, 58.33623f, 71.4761f, -45.31f - 32.01f + 90f),
                    };
                }

                for (int i = 0; i < roulettesData.Length; i++)
                {
                    var data = ((JArray)roulettesData[i]).ToObject<object[]>();

                    Roulettes[i].MinBet = Convert.ToUInt32(data[0]);
                    Roulettes[i].MaxBet = Convert.ToUInt32(data[1]);
                }

                KeyBinds.Bind(RAGE.Ui.VirtualKeys.Z, true, () =>
                {
                    Roulette.ActiveBets = new HashSet<Roulette.BetData>()
                    {
                        new Roulette.BetData() { BetType = Roulette.BetTypes._00, Amount = 5_124, },
                    };

                    Roulettes[0].StartGame();
                });
            }
        }

        public class CasinoEvents : Events.Script
        {
            public CasinoEvents()
            {
                Events.Add("Casino::RTSP", (args) =>
                {
                    var casinoId = (int)args[0];
                    var rouletteId = (int)args[1];

                    var casino = Casino.GetById(casinoId);

                    var roulette = casino.GetRouletteById(rouletteId);

                    if (roulette.TableObject?.Exists != true || roulette.NPC?.Ped?.Exists != true)
                        return;

                    var targetNumber = Convert.ToByte(args[2]);

                    roulette.Spin(targetNumber);
                });
            }
        }
    }
}