using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BCRPServer.Events.Players.Misc
{
    internal class MarketStall : Script
    {
        [RemoteProc("MarketStall::Rent")]
        private static bool Rent(Player player, int stallIdx, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var stall = Game.Misc.MarketStall.GetByIdx(stallIdx);

            if (stall == null)
                return false;

            if (!stall.IsPlayerNear(player))
                return false;

            var rentPrice = Game.Misc.MarketStall.RentPrice;

            ulong newBalance;

            if (useCash)
            {
                if (!pData.TryRemoveCash(rentPrice, out newBalance, true, null))
                    return false;
            }
            else
            {
                if (!pData.HasBankAccount(true))
                    return false;

                if (!pData.BankAccount.TryRemoveMoneyDebit(rentPrice, out newBalance, true, null))
                    return false;
            }

            var rid = player.Id;

            var currentStall = Game.Misc.MarketStall.GetByRenter(rid, out _);

            if (currentStall != null)
            {
                return false;
            }

            if (useCash)
            {
                pData.SetCash(newBalance);
            }
            else
            {
                pData.BankAccount.SetDebitBalance(newBalance, null);
            }

            Game.Misc.MarketStall.SetCurrentRenterRID(stallIdx, rid);

            return true;
        }

        [RemoteProc("MarketStall::Lock")]
        private static byte Lock(Player player, int stallIdx, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 255;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return 255;

            var stall = Game.Misc.MarketStall.GetByIdx(stallIdx);

            if (stall == null)
                return 255;

            if (!stall.IsPlayerNear(player))
                return 255;

            if (!stall.IsPlayerRenter(stallIdx, player, true))
                return 255;

            if (stall.IsLocked == state)
                return 2;

            stall.IsLocked = state;

            return 1;
        }

        [RemoteProc("MarketStall::Close")]
        private static bool Close(Player player, int stallIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            var stall = Game.Misc.MarketStall.GetByIdx(stallIdx);

            if (stall == null)
                return false;

            if (!stall.IsPlayerRenter(stallIdx, player, true))
                return false;

            Game.Misc.MarketStall.SetCurrentRenterRID(stallIdx, ushort.MaxValue);

            return true;
        }

        [RemoteProc("MarketStall::Buy")]
        private static bool Buy(Player player, int stallIdx, uint itemUid, int amount, bool useCash)
        {
            return false;
        }

        [RemoteProc("MarketStall::Show")]
        private static object Show(Player player, int stallIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var stall = Game.Misc.MarketStall.GetByIdx(stallIdx);

            if (stall == null)
                return null;

            if (!stall.IsPlayerNear(player))
                return null;

            return null;
        }

        [RemoteProc("MarketStall::GMD")]
        private static object GetManageData(Player player, int stallIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var stall = Game.Misc.MarketStall.GetByIdx(stallIdx);

            if (stall == null)
                return null;

            if (Game.Misc.MarketStall.GetCurrentRenterRID(stallIdx) != player.Id)
                return null;

            return $"{(stall.IsLocked ? 1 : 0)}";
        }
    }
}
