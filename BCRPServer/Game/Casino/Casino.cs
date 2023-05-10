using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Casino
{
    public enum WallScreenTypes : byte
    {
        None = 0,

        CASINO_DIA_PL,
        CASINO_DIA_SLW_PL,
        CASINO_HLW_PL,
        CASINO_SNWFLK_PL,
        CASINO_WIN_PL,
        CASINO_WIN_SLW_PL,
    }

    public class Casino
    {
        public const WallScreenTypes DEFAULT_WALLSCREEN_TYPE = WallScreenTypes.CASINO_DIA_PL;

        private static List<Casino> All { get; set; }

        public static Casino GetById(int id) => id < 0 || id >= All.Count ? null : All[id];

        public int Id => All.IndexOf(this);

        public Roulette[] Roulettes { get; set; }

        public LuckyWheel[] LuckyWheels { get; set; }

        public ushort BuyChipPrice { get; set; }
        public ushort SellChipPrice { get; set; }

        public WallScreenTypes CurrentWallScreenType { get => (WallScreenTypes)Convert.ToByte(Sync.World.GetSharedData<object>($"CASINO_{Id}_WST")); set => Sync.World.SetSharedData($"CASINO_{Id}_WST", (byte)value); }

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
                BuyChipPrice = 100,

                SellChipPrice = 95,

                Roulettes = new Roulette[]
                {
                    new Roulette(0, 0, 1031.199f, 64.15562f, 71.4761f)
                    {
                        MinBet = 250,
                        MaxBet = 15000,

                        MaxPlayers = 10,
                    },

                    new Roulette(0, 1, 1024.319f, 62.52872f, 71.4761f)
                    {
                        MinBet = 100,
                        MaxBet = 5000,

                        MaxPlayers = 10,
                    },

                    new Roulette(0, 2, 1032.156f, 60.01989f, 71.4761f)
                    {
                        MinBet = 5,
                        MaxBet = 1000,

                        MaxPlayers = 10,
                    },

                    new Roulette(0, 3, 1025.035f, 58.33623f, 71.4761f)
                    {
                        MinBet = 5,
                        MaxBet = 1000,

                        MaxPlayers = 10,
                    },
                },

                LuckyWheels = new LuckyWheel[]
                {
                    new LuckyWheel(977.5012f, 49.64366f, 73.67611f)
                    {

                    },
                },
            };

            mainCasino.CurrentWallScreenType = DEFAULT_WALLSCREEN_TYPE;

            var lines = new List<string>();

            for (int i = 0; i < All.Count; i++)
            {
                var x = All[i];

                lines.Add($"new Casino({i}, {x.BuyChipPrice}, {x.SellChipPrice}, \"{x.Roulettes.Select(x => new object[] { x.MinBet, x.MaxBet }).ToList().SerializeToJson().Replace('\"', '\'')}\");");

                for (int r = 0; i < x.Roulettes.Length; i++)
                {
                    var roulette = x.Roulettes[i];

                    roulette.SetCurrentStateData("I");
                }
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_LOCATIONS_DATA_PATH, "CASINOS_TO_REPLACE", lines);
        }

        public static bool TryAddCasinoChips(PlayerData.PlayerInfo pInfo, uint amount, out uint newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!pInfo.CasinoChips.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public static bool TryRemoveCasinoChips(PlayerData.PlayerInfo pInfo, uint amount, out uint newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!pInfo.CasinoChips.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (pInfo.PlayerData != null)
                    {
                        pInfo.PlayerData.Player.Notify("Casino::NEC", pInfo.CasinoChips);
                    }
                }

                return false;
            }

            return true;
        }

        public static void SetCasinoChips(PlayerData.PlayerInfo pInfo, uint value, string reason)
        {
            pInfo.CasinoChips = value;

            if (pInfo.PlayerData != null)
            {
                pInfo.PlayerData.Player.TriggerEvent("Casino::CB", value);
            }

            MySQL.CharacterCasinoChipsUpdate(pInfo);
        }
    }
}
