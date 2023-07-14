using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BCRPServer.Game.Casino
{
    public class Roulette
    {
        public const byte MAX_BETS = 3;

        public const int BET_WAIT_TIME = 20_000;

        public enum BetTypes : byte
        {
            None = 0,

            _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15, _16, _17, _18, _19, _20, _21, _22, _23, _24, _25, _26, _27, _28, _29, _30, _31, _32, _33, _34, _35, _36,
            _0, _00,

            Red = 100,
            Black = 101,

            Even = 110,
            Odd = 111,

            _1to18 = 120,
            _19to36 = 121,


            First_12 = 130,
            Second_12 = 131,
            Third_12 = 132,

            _2to1_1 = 140,
            _2to1_2 = 141,
            _2to1_3 = 142,
        }

        private static Dictionary<BetTypes, decimal> WinCoefs = new Dictionary<BetTypes, decimal>()
        {
            { BetTypes._1, 36m },
            { BetTypes._2, 36m },
            { BetTypes._3, 36m },
            { BetTypes._4, 36m },
            { BetTypes._5, 36m },
            { BetTypes._6, 36m },
            { BetTypes._7, 36m },
            { BetTypes._8, 36m },
            { BetTypes._9, 36m },
            { BetTypes._10, 36m },
            { BetTypes._11, 36m },
            { BetTypes._12, 36m },
            { BetTypes._13, 36m },
            { BetTypes._14, 36m },
            { BetTypes._15, 36m },
            { BetTypes._16, 36m },
            { BetTypes._17, 36m },
            { BetTypes._18, 36m },
            { BetTypes._19, 36m },
            { BetTypes._20, 36m },
            { BetTypes._21, 36m },
            { BetTypes._22, 36m },
            { BetTypes._23, 36m },
            { BetTypes._24, 36m },
            { BetTypes._25, 36m },
            { BetTypes._26, 36m },
            { BetTypes._27, 36m },
            { BetTypes._28, 36m },
            { BetTypes._29, 36m },
            { BetTypes._30, 36m },
            { BetTypes._31, 36m },
            { BetTypes._32, 36m },
            { BetTypes._33, 36m },
            { BetTypes._34, 36m },
            { BetTypes._35, 36m },
            { BetTypes._36, 36m },

            { BetTypes._0, 36m },
            { BetTypes._00, 36m },

            { BetTypes.Red, 2m },
            { BetTypes.Black, 2m },
            { BetTypes.Even, 2m },
            { BetTypes.Odd, 2m },
            { BetTypes._1to18, 2m },
            { BetTypes._19to36, 2m },

            { BetTypes.First_12, 3m },
            { BetTypes.Second_12, 3m },
            { BetTypes.Third_12, 3m },
            { BetTypes._2to1_1, 3m },
            { BetTypes._2to1_2, 3m },
            { BetTypes._2to1_3, 3m },
        };

        private static Dictionary<byte, BetTypes[]> Numbers = new Dictionary<BetTypes, byte[]>()
        {
            { BetTypes._1, new byte[] { 1, } },
            { BetTypes._2, new byte[] { 2, } },
            { BetTypes._3, new byte[] { 3, } },
            { BetTypes._4, new byte[] { 4, } },
            { BetTypes._5, new byte[] { 5, } },
            { BetTypes._6, new byte[] { 6, } },
            { BetTypes._7, new byte[] { 7, } },
            { BetTypes._8, new byte[] { 8, } },
            { BetTypes._9, new byte[] { 9, } },
            { BetTypes._10, new byte[] { 10, } },
            { BetTypes._11, new byte[] { 11, } },
            { BetTypes._12, new byte[] { 12, } },
            { BetTypes._13, new byte[] { 13, } },
            { BetTypes._14, new byte[] { 14, } },
            { BetTypes._15, new byte[] { 15, } },
            { BetTypes._16, new byte[] { 16, } },
            { BetTypes._17, new byte[] { 17, } },
            { BetTypes._18, new byte[] { 18, } },
            { BetTypes._19, new byte[] { 19, } },
            { BetTypes._20, new byte[] { 20, } },
            { BetTypes._21, new byte[] { 21, } },
            { BetTypes._22, new byte[] { 22, } },
            { BetTypes._23, new byte[] { 23, } },
            { BetTypes._24, new byte[] { 24, } },
            { BetTypes._25, new byte[] { 25, } },
            { BetTypes._26, new byte[] { 26, } },
            { BetTypes._27, new byte[] { 27, } },
            { BetTypes._28, new byte[] { 28, } },
            { BetTypes._29, new byte[] { 29, } },
            { BetTypes._30, new byte[] { 30, } },
            { BetTypes._31, new byte[] { 31, } },
            { BetTypes._32, new byte[] { 32, } },
            { BetTypes._33, new byte[] { 33, } },
            { BetTypes._34, new byte[] { 34, } },
            { BetTypes._35, new byte[] { 35, } },
            { BetTypes._36, new byte[] { 36, } },

            { BetTypes._0, new byte[] { 37, } },
            { BetTypes._00, new byte[] { 38, } },

            { BetTypes.Red, new byte[] { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36, } },
            { BetTypes.Black, new byte[] { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35, } },
            { BetTypes.Even, new byte[] { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 26, 28, 30, 32, 34, 36, } },
            { BetTypes.Odd, new byte[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, } },
            { BetTypes._1to18, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, } },
            { BetTypes._19to36, new byte[] { 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, } },
            { BetTypes.First_12, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, } },
            { BetTypes.Second_12, new byte[] { 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, } },
            { BetTypes.Third_12, new byte[] { 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, } },
            { BetTypes._2to1_1, new byte[] { 1, 4, 7, 10, 13, 16, 19, 22, 25, 28, 31, 34, } },
            { BetTypes._2to1_2, new byte[] { 2, 5, 8, 11, 14, 17, 20, 23, 26, 29, 32, 35, } },
            { BetTypes._2to1_3, new byte[] { 3, 6, 9, 12, 15, 18, 21, 24, 27, 30, 33, 36, } },
        }.SelectMany(x => x.Value.Select(y => new { Num = y, BetType = x.Key })).GroupBy(x => x.Num).ToDictionary(x => x.Key, x => x.Select(y => y.BetType).ToArray());

        private static Dictionary<byte, BetTypes[]> Numbers1 = new Dictionary<byte, BetTypes[]>()
        {
            { 1, new BetTypes[] { BetTypes._1, BetTypes.Red, BetTypes.Odd,  } },
        };

        public static decimal GetWinCoefByBetType(BetTypes betType) => WinCoefs.GetValueOrDefault(betType);

        public static BetTypes[] GetBetTypesForNumber(byte number) => Numbers.GetValueOrDefault(number);

        public class BetData
        {
            public class Bet
            {
                public BetTypes Type { get; set; }

                public uint Amount { get; set; }
            }

            public Bet[] Bets { get; set; } = new Bet[MAX_BETS];
        }

        public Timer Timer { get; set; }

        public Vector3 Position { get; set; }

        public ushort MaxPlayers { get; set; }

        public uint MinBet { get; set; }

        public uint MaxBet { get; set; }

        public readonly byte CasinoId;

        public readonly byte Id;

        private string CurrentStateData { get; set; }

        public Dictionary<uint, BetData> CurrentPlayers { get; set; } = new Dictionary<uint, BetData>();

        public Roulette(byte CasinoId, byte Id, float PosX, float PosY, float PosZ)
        {
            this.Position = new Vector3(PosX, PosY, PosZ);

            this.CasinoId = CasinoId;
            this.Id = Id;
        }

        public void SetCurrentStateData(string value)
        {
            CurrentStateData = value;

            Utils.TriggerEventInDistance(Position, Settings.CurrentProfile.Game.MainDimension, 50f, "Casino::RLTS", CasinoId, Id, value);
        }

        public string GetCurrentStateData()
        {
            return CurrentStateData;
        }

        public BetData AddPlayer(PlayerData pData)
        {
            var betData = new BetData() { };

            CurrentPlayers.Add(pData.CID, betData);

            return betData;
        }

        public bool RemovePlayer(uint cid)
        {
            if (CurrentPlayers.Remove(cid))
            {
                return true;
            }

            return false;
        }

        public bool CanPlaceBet()
        {
            var state = GetCurrentStateData();

            if (state == null || !(state[0] == 'I' || state[0] == 'S'))
                return false;

            return true;
        }

        public void StartGame()
        {
            Timer?.Dispose();

            var startDate = Utils.GetCurrentTime().AddMilliseconds(BET_WAIT_TIME);

            SetCurrentStateData($"S{startDate.GetUnixTimestamp()}");

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    Timer?.Dispose();

                    var resultNumber = GetNextResultNumber();

                    SetCurrentStateData($"R{resultNumber}");

                    Timer = new Timer((obj) =>
                    {
                        NAPI.Task.Run(() =>
                        {
                            if (Timer != null)
                            {
                                Timer.Dispose();

                                Timer = null;
                            }

                            SetCurrentStateData($"I{resultNumber}");

                            OnFinishGame(resultNumber);

                            CurrentPlayers.Clear();
                        });
                    }, null, 15_000, Timeout.Infinite);
                });
            }, null, BET_WAIT_TIME, Timeout.Infinite);
        }

        public byte GetNextResultNumber()
        {
            return (byte)SRandom.NextInt32S(1, 38 + 1);
        }

        private void OnFinishGame(byte resultNum)
        {
            var betTypes = GetBetTypesForNumber(resultNum);

            if (betTypes == null)
                return;

            foreach (var x in CurrentPlayers)
            {
                if (x.Value.Bets == null)
                    continue;

                var pInfo = PlayerData.PlayerInfo.Get(x.Key);

                if (pInfo == null)
                    continue;

                uint totalWin = 0;

                for (int i = 0; i < x.Value.Bets.Length; i++)
                {
                    var bet = x.Value.Bets[i];

                    if (bet == null)
                        continue;

                    if (Array.IndexOf(betTypes, bet.Type) >= 0)
                    {
                        var coef = GetWinCoefByBetType(bet.Type);

                        try
                        {
                            totalWin += (uint)Math.Floor(bet.Amount * coef);
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }

                if (totalWin > 0)
                {
                    uint newBalance;

                    if (Casino.TryAddCasinoChips(pInfo, totalWin, out newBalance, true, null))
                    {
                        Casino.SetCasinoChips(pInfo, newBalance, null);
                    }
                }
            }
        }
    }
}
