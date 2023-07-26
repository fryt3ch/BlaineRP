using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BlaineRP.Server.Game.Casino
{
    public class SlotMachine
    {
        [Properties.Settings.Static.ClientSync("casinoSlotMachineJackpotMinValue")]
        public const uint JackpotMinValue = 2_500;

        public const uint JackpotMaxValue = 10_000;

        [Properties.Settings.Static.ClientSync("casinoSlotMachineMinBet")]
        public const uint MinBet = 5;

        [Properties.Settings.Static.ClientSync("casinoSlotMachineMaxBet")]
        public const uint MaxBet = 1000;

        public const ReelIconTypes JackpotReplaceType = ReelIconTypes.Grape;

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
            { ReelIconTypes.Grape, 3m },
            { ReelIconTypes.Cherry, 4m },
            { ReelIconTypes.Watermelon, 6m },
            { ReelIconTypes.Microphone, 10m },
            { ReelIconTypes.Superstar, 16m },
            { ReelIconTypes.Bell, 26m },
        };

        private static ChancePicker<ReelIconTypes> ChancePicker = new ChancePicker<ReelIconTypes>
        (
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.001d, ReelIconTypes.Bell),
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.004d, ReelIconTypes.Superstar),
            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.005d, ReelIconTypes.Microphone),

            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.01d, ReelIconTypes.Watermelon),

            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.02d, ReelIconTypes.Cherry),

            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.11d, ReelIconTypes.Grape),

            new ChancePicker<ReelIconTypes>.Item<ReelIconTypes>(0.85d, ReelIconTypes.Loose)
        );

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

            double chance;

            var rItem = ChancePicker.GetNextItem(out chance);

            var cid = pData.CID;

            var jackpot = Jackpot;

            if (rItem == ReelIconTypes.Loose)
            {
                if (Jackpot < JackpotMaxValue)
                {
                    if (Jackpot + bet > JackpotMaxValue)
                        Jackpot = JackpotMaxValue;
                    else
                        Jackpot += bet;
                }
            }
            else if (rItem == JackpotReplaceType && jackpot >= JackpotMinValue)
            {
                rItem = ReelIconTypes.Seven;

                Jackpot = 0;
            }
            else
            {
                if (bet > Jackpot)
                    Jackpot = 0;
                else
                    Jackpot -= bet;
            }

            Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (Timer != null)
                    {
                        Timer.Dispose();

                        Timer = null;
                    }

                    var pInfo = PlayerData.PlayerInfo.Get(cid);

                    if (pInfo == null)
                    {

                    }
                    else
                    {
                        if (rItem == ReelIconTypes.Loose)
                        {
                            return;
                        }

                        uint totalWin = 0;

                        if (rItem == ReelIconTypes.Seven)
                        {
                            totalWin = jackpot + bet;
                        }
                        else
                        {
                            totalWin = (uint)Math.Floor(bet * BetCoefs.GetValueOrDefault(rItem));
                        }

                        if (totalWin > 0)
                        {
                            uint newBalance;

                            if (Casino.TryAddCasinoChips(pInfo, totalWin, out newBalance, true, null))
                                Casino.SetCasinoChips(pInfo, newBalance, null);
                        }
                    }
                });
            }, null, 5_500, Timeout.Infinite);

            return rItem;
        }

        public bool IsPlayerNear(Player player) => player.Dimension == Properties.Settings.Static.MainDimension && player.Position.DistanceTo(Position) <= 5f;
    }
}
