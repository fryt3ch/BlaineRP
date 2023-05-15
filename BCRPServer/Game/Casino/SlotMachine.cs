using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BCRPServer.Game.Casino
{
    public class SlotMachine
    {
        public enum ReelIconTypes : byte
        {
            Seven = 0,
            Grape = 1,
            Watermelon = 2,
            Microphone = 3,
            Bell = 5,
            Cherry = 6,
            Superstar = 13,

            Loose = 255,
        }

        private static Dictionary<ReelIconTypes, decimal> BetCoefs = new Dictionary<ReelIconTypes, decimal>()
        {
            { ReelIconTypes.Grape, 2m },
            { ReelIconTypes.Cherry, 3m },
            { ReelIconTypes.Watermelon, 5m },
            { ReelIconTypes.Microphone, 9m },
            { ReelIconTypes.Superstar, 15m },
            { ReelIconTypes.Bell, 25m },
        };

        private static Dictionary<decimal, ReelIconTypes> BetChances = new Dictionary<decimal, ReelIconTypes>()
        {
            { 0.001m, ReelIconTypes.Bell },
            { 0.004m, ReelIconTypes.Superstar },
            { 0.005m, ReelIconTypes.Microphone },

            { 0.01m, ReelIconTypes.Watermelon },

            { 0.03m, ReelIconTypes.Cherry },

            { 0.05m, ReelIconTypes.Grape },

            { 0.90m, ReelIconTypes.Loose },
        };

        public static uint MinBet { get; set; } = 5;
        public static uint MaxBet { get; set; } = 1000;

        public Vector3 Position { get; set; }

        public uint CurrentCID { get; set; }

        private Timer Timer { get; set; }

        public uint Jackpot { get; set; }

        public SlotMachine(int CasinoId, int Id, float PosX, float PosY, float PosZ)
        {
            this.Position = new Vector3(PosX, PosY, PosZ);
        }

        public bool CanSpinNow() => Timer == null;

        public bool IsAvailableNow() => CurrentCID <= 0;

        public void SetPlayerTo(PlayerData pData)
        {
            pData.PlayAnim(Sync.Animations.GeneralTypes.CasinoSlotMachineIdle0);

            CurrentCID = pData.CID;
        }

        public void SetPlayerFrom(PlayerData pData)
        {
            if (pData.GeneralAnim == Sync.Animations.GeneralTypes.CasinoSlotMachineIdle0)
            {
                pData.StopGeneralAnim();
            }

            CurrentCID = 0;
        }

        public ReelIconTypes Spin(PlayerData pData, uint bet)
        {
            Timer?.Dispose();

            var rProb = (decimal)SRandom.NextDoubleS();

            var rItems = BetChances.OrderBy(x => Math.Abs(rProb - x.Key)).ThenByDescending(x => x).First();

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (Timer != null)
                    {
                        Timer.Dispose();

                        Timer = null;
                    }

                    if (rItems.Value == ReelIconTypes.Loose)
                        return;

                    if (rItems.Value == ReelIconTypes.Seven)
                    {

                    }
                    else
                    {
                        var totalWin = (uint)Math.Floor(bet * BetCoefs.GetValueOrDefault(rItems.Value)) + bet;

                        uint newBalance;

                        if (totalWin > 0)
                        {
                            if (Casino.TryAddCasinoChips(pData.Info, totalWin, out newBalance, true, null))
                                Casino.SetCasinoChips(pData.Info, newBalance, null);
                        }
                    }
                });
            }, null, 5_500, Timeout.Infinite);

            return rItems.Value;
        }

        public ReelIconTypes GetResult()
        {
            var rProb = (decimal)SRandom.NextDoubleS();

            var rItems = BetChances.OrderBy(x => Math.Abs(rProb - x.Key)).ThenByDescending(x => x).First();

            return rItems.Value;
        }

        public void DoPayment(PlayerData.PlayerInfo pInfo, ReelIconTypes rType)
        {

        }
    }
}
