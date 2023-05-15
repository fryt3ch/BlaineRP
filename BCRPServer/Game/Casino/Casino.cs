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

        public static List<Casino> All { get; private set; }

        public static Casino GetById(int id) => id < 0 || id >= All.Count ? null : All[id];

        public int Id => All.IndexOf(this);

        public Roulette[] Roulettes { get; set; }

        public LuckyWheel[] LuckyWheels { get; set; }

        public SlotMachine[] SlotMachines { get; set; }

        public Blackjack[] Blackjacks { get; set; }

        public ushort BuyChipPrice { get; set; }
        public ushort SellChipPrice { get; set; }

        public WallScreenTypes CurrentWallScreenType { get => (WallScreenTypes)Convert.ToByte(Sync.World.GetSharedData<object>($"CASINO_{Id}_WST")); set => Sync.World.SetSharedData($"CASINO_{Id}_WST", (byte)value); }

        public Casino()
        {
            All.Add(this);
        }

        public Roulette GetRouletteById(int id) => id < 0 || id >= Roulettes.Length ? null : Roulettes[id];

        public LuckyWheel GetLuckyWheelById(int id) => id < 0 || id >= LuckyWheels.Length ? null : LuckyWheels[id];

        public SlotMachine GetSlotMachineById(int id) => id < 0 || id >= SlotMachines.Length ? null : SlotMachines[id];

        public Blackjack GetBlackjackById(int id) => id < 0 || id >= Blackjacks.Length ? null : Blackjacks[id];

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
                    new Roulette(0, 0, 1024.319f, 62.52872f, 71.4761f)
                    {
                        MinBet = 250,
                        MaxBet = 15000,

                        MaxPlayers = 10,
                    },

                    new Roulette(0, 1, 1025.035f, 58.33623f, 71.4761f)
                    {
                        MinBet = 100,
                        MaxBet = 5000,

                        MaxPlayers = 10,
                    },

                    new Roulette(0, 2, 1031.199f, 64.15562f, 71.4761f)
                    {
                        MinBet = 5,
                        MaxBet = 1000,

                        MaxPlayers = 10,
                    },

                    new Roulette(0, 3, 1032.156f, 60.01989f, 71.4761f)
                    {
                        MinBet = 5,
                        MaxBet = 1000,

                        MaxPlayers = 10,
                    },
                },

                Blackjacks = new Blackjack[]
                {
                    new Blackjack(0, 0, 1010.524f, 67.42336f, 72.27832f, 4)
                    {
                        MinBet = 5,
                        MaxBet = 1000,
                    },

                    new Blackjack(0, 1, 1013.893f, 65.31736f, 72.27832f, 4)
                    {
                        MinBet = 5,
                        MaxBet = 1000,
                    },

                    new Blackjack(0, 2, 1013.794f, 72.33979f, 72.27832f, 4)
                    {
                        MinBet = 5,
                        MaxBet = 1000,
                    },

                    new Blackjack(0, 3, 1017.087f, 70.42752f, 72.27832f, 4)
                    {
                        MinBet = 5,
                        MaxBet = 1000,
                    },
                },

                LuckyWheels = new LuckyWheel[]
                {
                    new LuckyWheel(0, 0, 977.5012f, 49.64366f, 73.67611f)
                    {

                    },
                },

                SlotMachines = new SlotMachine[]
                {
                        new SlotMachine(0, 0, 1014.009f, 54.50948f, 72.2761f),
                        new SlotMachine(0, 1, 1013.83f, 55.30534f, 72.2761f),
                        new SlotMachine(0, 2, 1013.017f, 55.37876f, 72.2761f),
                        new SlotMachine(0, 3, 1012.695f, 54.62857f, 72.2761f),
                        new SlotMachine(0, 4, 1013.308f, 54.09158f, 72.2761f),

                        new SlotMachine(0, 5, 1012.662f, 60.35477f, 72.2761f),
                        new SlotMachine(0, 6, 1012.48f, 61.15062f, 72.2761f),
                        new SlotMachine(0, 7, 1011.667f, 61.22473f, 72.2761f),
                        new SlotMachine(0, 8, 1011.346f, 60.47492f, 72.2761f),
                        new SlotMachine(0, 9, 1011.961f, 59.93698f, 72.2761f),

                        new SlotMachine(0, 10, 973.071f, 52.94492f, 73.47611f),
                        new SlotMachine(0, 11, 972.3029f, 53.44027f, 73.47611f),
                        new SlotMachine(0, 12, 973.7795f, 53.52237f, 73.47611f),
                        new SlotMachine(0, 13, 972.5366f, 54.32385f, 73.47611f),
                        new SlotMachine(0, 14, 973.4492f, 54.3746f, 73.47611f),

                        new SlotMachine(0, 15, 978.3527f, 54.91812f, 73.47611f),
                        new SlotMachine(0, 16, 977.5847f, 55.41346f, 73.47611f),
                        new SlotMachine(0, 17, 977.8184f, 56.29704f, 73.47611f),
                        new SlotMachine(0, 18, 978.731f, 56.34779f, 73.47611f),
                        new SlotMachine(0, 19, 979.0612f, 55.49556f, 73.47611f),

                        new SlotMachine(0, 20, 982.2341f, 52.22181f, 73.47611f),
                        new SlotMachine(0, 21, 982.4678f, 53.1054f, 73.47611f),
                        new SlotMachine(0, 22, 983.3804f, 53.15614f, 73.47611f),
                        new SlotMachine(0, 23, 983.7107f, 52.30392f, 73.47611f),
                        new SlotMachine(0, 24, 983.0022f, 51.72647f, 73.47611f),

                        new SlotMachine(0, 25, 982.4944f, 47.44504f, 73.47611f),
                        new SlotMachine(0, 26, 983.407f, 47.49578f, 73.47611f),
                        new SlotMachine(0, 27, 983.7373f, 46.64355f, 73.47611f),
                        new SlotMachine(0, 28, 983.0288f, 46.06611f, 73.47611f),
                        new SlotMachine(0, 29, 982.2607f, 46.56145f, 73.47611f),
                },
            };

            mainCasino.CurrentWallScreenType = DEFAULT_WALLSCREEN_TYPE;

            var lines = new List<string>();

            lines.Add($"Casino.SlotMachine.MinBet = {SlotMachine.MinBet};");
            lines.Add($"Casino.SlotMachine.MaxBet = {SlotMachine.MaxBet};");

            for (int i = 0; i < All.Count; i++)
            {
                var x = All[i];

                lines.Add($"new Casino({i}, {x.BuyChipPrice}, {x.SellChipPrice}, \"{x.Roulettes.Select(x => new object[] { x.MinBet, x.MaxBet }).ToList().SerializeToJson().Replace('\"', '\'')}\", \"{x.Blackjacks.Select(x => new object[] { x.MinBet, x.MaxBet }).ToList().SerializeToJson().Replace('\"', '\'')}\");");

                for (int r = 0; r < x.Roulettes.Length; r++)
                {
                    var roulette = x.Roulettes[r];

                    roulette.SetCurrentStateData("I");
                }

                for (int r = 0; r < x.Blackjacks.Length; r++)
                {
                    var table = x.Blackjacks[r];

                    table.SetCurrentStateData("I");
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
