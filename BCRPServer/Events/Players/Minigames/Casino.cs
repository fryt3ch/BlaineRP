using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players.Minigames
{
    internal class Casino : Script
    {
        [RemoteProc("Casino::RLTJ")]
        public static bool CasinoRouletteJoin(Player player, int casinoId, int rouletteId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return false;

            var roulette = casino.GetRouletteById(rouletteId);

            if (roulette == null)
                return false;

            if (roulette.CurrentPlayers.Count >= roulette.MaxPlayers)
            {
                return false;
            }

            var betData = roulette.CurrentPlayers.GetValueOrDefault(pData.CID);

            if (betData == null)
            {
                roulette.AddPlayer(pData);
            }
            else
            {

            }

            return true;
        }

        [RemoteProc("Casino::RLTL")]
        public static object CasinoRouletteLeave(Player player, int casinoId, int rouletteId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var casino = Game.Casino.Casino.GetById(casinoId);

            if (casino == null)
                return false;

            var roulette = casino.GetRouletteById(rouletteId);

            if (roulette == null)
                return false;

            var betData = roulette.CurrentPlayers.GetValueOrDefault(pData.CID);

            if (betData == null)
                return false;

            return true;
        }
    }
}
