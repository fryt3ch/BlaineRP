using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace BCRPServer.Game.Casino
{
    public class Roulette
    {
        public const byte MAX_BETS = 3;

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

        public enum StateTypes : byte
        {
            /// <summary>В простое, ожидание первой ставки</summary>
            Idle = 0,
            /// <summary>Игроки делают следующие ставки, идет обратный отсчёт</summary>
            Bets,
            /// <summary>Результат уже известен, идет проигрывание анимаций на клиенте и ожидание на сервере (вращение колеса)</summary>
            Spinning,
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

        private static Dictionary<BetTypes, byte[]> Numbers = new Dictionary<BetTypes, byte[]>()
        {
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
        };

        public static decimal GetWinCoefByBetType(BetTypes betType) => WinCoefs.GetValueOrDefault(betType);

        public static byte[] GetNumbersForBetType(BetTypes betType) => betType <= BetTypes._00 ? new byte[] { (byte)betType } : Numbers.GetValueOrDefault(betType);

        public class BetData
        {
            public class Bet
            {
                public BetTypes Type { get; set; }

                public uint Amount { get; set; }
            }

            public Bet[] Bets { get; set; } = new Bet[MAX_BETS];
        }

        public StateTypes CurrentStateType { get; set; }

        public Timer Timer { get; set; }

        public Vector3 Position { get; set; }

        public ushort MaxPlayers { get; set; }

        public uint MinBet { get; set; }

        public uint MaxBet { get; set; }

        public Dictionary<uint, BetData> CurrentPlayers { get; set; } = new Dictionary<uint, BetData>();

        public Roulette(float PosX, float PosY, float PosZ)
        {
            this.Position = new Vector3(PosX, PosY, PosZ);
        }

        public void AddPlayer(PlayerData pData)
        {
            CurrentPlayers.Add(pData.CID, new BetData() { });
        }

        public bool RemovePlayer(uint cid)
        {
            if (CurrentPlayers.Remove(cid))
            {
                return true;
            }

            return false;
        }

        public void SetSpinningState()
        {
            CurrentStateType = StateTypes.Spinning;

            Timer?.Dispose();

            Timer = new Timer((obj) =>
            {

            }, null, 17_100, Timeout.Infinite);
        }

        public void CalculateAll(byte resultNum)
        {
            foreach (var x in CurrentPlayers)
            {
                for (int i = 0; i < x.Value.Bets.Length; i++)
                {
                    var bet = x.Value.Bets[i];

                    if (bet == null)
                        continue;
                }
            }
        }
    }
}
