using System;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Casino.Games
{
    public partial class Roulette
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Casino::RLTJ")]
            public static object CasinoRouletteJoin(Player player, int casinoId, int rouletteId)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return null;

                Roulette roulette = casino.GetRouletteById(rouletteId);

                if (roulette == null)
                    return null;

                if (player.Dimension != Properties.Settings.Static.MainDimension || roulette.Position.DistanceTo(player.Position) > 5f)
                    return null;

                var betData = roulette.CurrentPlayers.GetValueOrDefault(pData.CID);

                if (betData == null)
                {
                    if (roulette.CurrentPlayers.Count >= roulette.MaxPlayers)
                    {
                        player.Notify("Casino::RLTMP", roulette.MaxPlayers);

                        return null;
                    }

                    return $"{pData.Info.CasinoChips}^{roulette.GetCurrentStateData()}";
                }
                else
                {
                    return $"{pData.Info.CasinoChips}^{roulette.GetCurrentStateData()}";
                }
            }

            [RemoteProc("Casino::RLTSB")]
            public static bool CasinoRouletteSetBet(Player player, int casinoId, int rouletteId, byte betTypeNum, uint amount)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                if (!Enum.IsDefined(typeof(BetTypes), betTypeNum))
                    return false;

                var casino = CasinoEntity.GetById(casinoId);

                if (casino == null)
                    return false;

                Roulette roulette = casino.GetRouletteById(rouletteId);

                if (roulette == null)
                    return false;

                if (player.Dimension != Properties.Settings.Static.MainDimension || roulette.Position.DistanceTo(player.Position) > 5f)
                    return false;

                if (!roulette.CanPlaceBet())
                    return false;

                if (roulette.MinBet > amount)
                    amount = roulette.MinBet;
                else if (roulette.MaxBet < amount)
                    amount = roulette.MaxBet;

                uint newBalance;

                if (!CasinoEntity.TryRemoveCasinoChips(pData.Info, amount, out newBalance, true, null))
                    return false;

                var betType = (BetTypes)betTypeNum;

                int freeBetIdx = -1;

                var betData = roulette.CurrentPlayers.GetValueOrDefault(pData.CID);

                if (betData == null)
                {
                    if (roulette.CurrentPlayers.Count >= roulette.MaxPlayers)
                    {
                        player.Notify("Casino::RLTMP", roulette.MaxPlayers);

                        return false;
                    }

                    betData = roulette.AddPlayer(pData);

                    freeBetIdx = 0;
                }
                else
                {
                    for (var i = 0; i < betData.Bets.Length; i++)
                    {
                        if (freeBetIdx < 0 && betData.Bets[i] == null)
                            freeBetIdx = i;
                        else if (betData.Bets[i]?.Type == betType)
                            return false;
                    }

                    if (freeBetIdx < 0)
                    {
                        player.Notify("Casino::RLTMB");

                        return false;
                    }
                }

                betData.Bets[freeBetIdx] = new BetData.Bet()
                {
                    Type = betType,
                    Amount = amount,
                };

                CasinoEntity.SetCasinoChips(pData.Info, newBalance, null);

                string state = roulette.GetCurrentStateData();

                if (state != null && state[0] == 'I')
                    roulette.StartGame();

                return true;
            }
        }
    }
}