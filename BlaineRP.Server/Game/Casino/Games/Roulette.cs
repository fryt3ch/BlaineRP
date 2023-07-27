using System;
using System.Collections.Generic;
using System.Threading;
using BlaineRP.Server.EntityData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.CasinoSystem.Games
{
    public partial class Roulette
    {
        public static decimal GetWinCoefByBetType(BetTypes betType) => WinCoefs.GetValueOrDefault(betType);

        public static BetTypes[] GetBetTypesForNumber(byte number) => Numbers.GetValueOrDefault(number);

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

            Utils.TriggerEventInDistance(Position, Properties.Settings.Static.MainDimension, 50f, "Casino::RLTS", CasinoId, Id, value);
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

                var pInfo = PlayerInfo.Get(x.Key);

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
