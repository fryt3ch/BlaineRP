using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Roulette
    {
        public const byte MAX_BETS = 3;

        public const int BET_WAIT_TIME = 20_000;
        
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
    }
}