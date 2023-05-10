using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players
{
    internal class Casino : Script
    {
        [RemoteProc("Casino::RLTJ")]
        public static object CasinoRouletteJoin(Player player, int casinoId, int rouletteId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return null;

            var roulette = casino.GetRouletteById(rouletteId);

            if (roulette == null)
                return null;

            var betData = roulette.CurrentPlayers.GetValueOrDefault(pData.CID);

            if (betData == null)
            {
                if (roulette.CurrentPlayers.Count >= roulette.MaxPlayers)
                {
                    player.Notify("Casino::RLTMP", roulette.MaxPlayers);

                    return null;
                }

                return $"{pData.Info.CasinoChips}";
            }
            else
            {
                return $"{pData.Info.CasinoChips}";
            }
        }

        [RemoteProc("Casino::RLTSB")]
        public static bool CasinoRouletteSetBet(Player player, int casinoId, int rouletteId, byte betTypeNum, uint amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            if (!Enum.IsDefined(typeof(Game.Casino.Roulette.BetTypes), betTypeNum))
                return false;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return false;

            var roulette = casino.GetRouletteById(rouletteId);

            if (roulette == null)
                return false;

            if (!roulette.CanPlaceBet())
                return false;

            if (roulette.MinBet > amount)
                amount = roulette.MinBet;
            else if (roulette.MaxBet < amount)
                amount = roulette.MaxBet;

/*            if (roulette.MinBet > amount || roulette.MaxBet < amount)
                return false;*/

            uint newBalance;

            if (!Game.Casino.Casino.TryRemoveCasinoChips(pData.Info, amount, out newBalance, true, null))
                return false;

            var betType = (Game.Casino.Roulette.BetTypes)betTypeNum;

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
                for (int i = 0; i < betData.Bets.Length; i++)
                {
                    if (freeBetIdx < 0 && betData.Bets[i] == null)
                    {
                        freeBetIdx = i;
                    }
                    else if (betData.Bets[i]?.Type == betType)
                    {
                        return false;
                    }
                }

                if (freeBetIdx < 0)
                {
                    player.Notify("Casino::RLTMB");

                    return false;
                }
            }

            betData.Bets[freeBetIdx] = new Game.Casino.Roulette.BetData.Bet() { Type = betType, Amount = amount, };

            Game.Casino.Casino.SetCasinoChips(pData.Info, newBalance, null);

            var state = roulette.GetCurrentStateData();

            if (state != null && state[0] == 'I')
            {
                roulette.StartGame();
            }

            return true;
        }
    }
}
