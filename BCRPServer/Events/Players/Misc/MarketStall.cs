using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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

            stall.SetCurrentRenter(stallIdx, pData);

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

            if (!stall.IsPlayerRenter(stallIdx, player, true, out _))
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

            if (!stall.IsPlayerRenter(stallIdx, player, true, out _))
                return false;

            stall.SetCurrentRenter(stallIdx, null);

            return true;
        }

        [RemoteProc("MarketStall::Buy")]
        private static bool Buy(Player player, int stallIdx, uint itemUid, int amount, bool useCash)
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

            ushort renterRid;

            if (stall.IsPlayerRenter(stallIdx, player, false, out renterRid))
                return false;

            if (renterRid == ushort.MaxValue)
                return false;

            int itemIdx = -1;

            if (stall.Items != null)
            {
                for (int i = 0; i < stall.Items.Count; i++)
                {
                    var x = stall.Items[i];

                    if (x.ItemRoot?.UID == itemUid)
                    {
                        itemIdx = i;

                        break;
                    }
                }
            }

            if (itemIdx < 0 || stall.Items[itemIdx].Amount <= 0)
            {
                player.Notify("MarketStall::BSE3");

                return false;
            }

            var rData = PlayerData.All.Values.Where(x => x.Player.Id == renterRid).FirstOrDefault();

            if (rData == null)
            {
                player.Notify("MarketStall::BSE0");

                return false;
            }

            if (!stall.IsPlayerNear(rData.Player, 50f))
            {
                stall.SetCurrentRenter(stallIdx, null);

                player.Notify("MarketStall::BSE2");

                return false;
            }

            var sItem = stall.Items[itemIdx];

            var rItemIdx = Array.IndexOf(rData.Items, sItem.ItemRoot);

            if (rItemIdx < 0)
            {
                player.Notify("MarketStall::BSE1");

                return false;
            }

            if (rData.IsKnocked || rData.IsFrozen || rData.IsCuffed || !rData.CanUseInventory())
            {
                player.Notify("MarketStall::BSE0");

                return false;
            }

            return false;
        }

        [RemoteProc("MarketStall::Try")]
        private static object Try(Player player, int stallIdx, uint itemUid)
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

/*            if (stall.IsPlayerRenter(stallIdx, player, false))
                return null;*/

            var item = stall?.Items.Where(x => x.ItemRoot?.UID == itemUid).FirstOrDefault();

            if (item == null)
            {
                return null;
            }

            var clothes = item.ItemRoot as Game.Items.Clothes;

            if (clothes == null)
                return null;

            return $"{clothes.Var}";
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

            if (Game.Misc.MarketStall.GetCurrentRenterRID(stallIdx) == ushort.MaxValue)
                return null;

/*            if (stall.IsPlayerRenter(stallIdx, player, false))
                return null;*/

            if (stall.IsLockedNow(stallIdx, player, true))
                return null;

            if (stall.Items == null || stall.Items.Count == 0)
                return string.Empty;

            var items = new List<string>();

            for (int i = 0; i < stall.Items.Count; i++)
            {
                var item = stall.Items[i];

                if (item.ItemRoot == null)
                    continue;

                var actualAmount = item.ItemRoot is Game.Items.IStackable stackable ? stackable.Amount : 1;

                if (item.Amount < actualAmount)
                    actualAmount = item.Amount;

                items.Add($"{item.ItemRoot.UID}&{item.ItemRoot.ID}&{(item.ItemRoot is Game.Items.IStackable ? item.ItemRoot.BaseWeight : item.ItemRoot.Weight)}&{Game.Items.Stuff.GetItemTag(item.ItemRoot)}&{item.Price}&{actualAmount}");
            }

            return items;
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

            if (!stall.IsPlayerNear(player))
                return null;

            if (!stall.IsPlayerRenter(stallIdx, player, true, out _))
                return null;

            return $"{(stall.IsLocked ? 1 : 0)}";
        }

        [RemoteProc("MarketStall::OSIM")]
        private static object OpenSetItemsMenu(Player player, int stallIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            if (!pData.CanUseInventory(true))
                return null;

            var stall = Game.Misc.MarketStall.GetByIdx(stallIdx);

            if (stall == null)
                return null;

            if (!stall.IsPlayerNear(player))
                return null;

            if (!stall.IsPlayerRenter(stallIdx, player, true, out _))
                return null;

            if (stall.Items == null)
                return string.Empty;

            var cItems = new List<string>();

            for  (int i = 0; i < stall.Items.Count; i++)
            {
                var sItem = stall.Items[i];

                var rIdx = sItem.ItemRoot == null ? -1 : Array.IndexOf(pData.Items, sItem.ItemRoot);

                if (rIdx < 0)
                {
                    stall.Items.RemoveAt(i--);

                    continue;
                }

                cItems.Add($"{rIdx}_{sItem.Amount}_{sItem.Price}");
            }

            return cItems;
        }

        [RemoteProc("MarketStall::SI")]
        private static byte SetItems(Player player, int stallIdx, string dataJson)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return 0;

            if (!pData.CanUseInventory(true))
                return 0;

            var stall = Game.Misc.MarketStall.GetByIdx(stallIdx);

            if (stall == null)
                return 0;

            if (!stall.IsPlayerNear(player))
                return 0;

            if (!stall.IsPlayerRenter(stallIdx, player, true, out _))
                return 0;

            var rItems = new List<Game.Misc.MarketStall.Item>();

            try
            {
                var items = dataJson.DeserializeFromJson<string[]>();

                if (items.Length == 0 || items.Length > pData.Items.Length)
                    return 255;

                for (int i = 0; i < items.Length; i++)
                {
                    var d = items[i].Split('_');

                    var idx = uint.Parse(d[0]);

                    var amount = int.Parse(d[1]);

                    if (amount <= 0)
                        return 255;

                    var price = int.Parse(d[2]);

                    if (price <= 0)
                        return 255;

                    var item = pData.Items[idx];

                    if (item == null)
                        return 255;

                    if (item is Game.Items.IStackable stackable)
                    {
                        if (stackable.Amount < amount)
                            return 255;
                    }
                    else
                    {
                        if (amount != 1)
                            amount = 1;
                    }

                    rItems.Add(new Game.Misc.MarketStall.Item(item, amount, (uint)price));
                }
            }
            catch (Exception ex)
            {
                return 255;
            }

            if (rItems.Count == 0)
                return 255;

            stall.SetItems(rItems);

            return 1;
        }
    }
}
