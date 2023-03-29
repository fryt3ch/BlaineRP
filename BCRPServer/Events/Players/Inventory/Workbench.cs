using BCRPServer.Game.Items.Craft;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Events.Players
{
    partial class Inventory : Script
    {
        [RemoteEvent("Workbench::Show")]
        private static void WorkbenchShow(Player player, byte wTypeNum, uint uid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Items.Craft.Workbench.Types), wTypeNum))
                return;

            var wType = (Game.Items.Craft.Workbench.Types)wTypeNum;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (pData.CurrentContainer != null || pData.CurrentWorkbench != null)
                return;

            var bench = Game.Items.Craft.Workbench.Get(wType, uid);

            if (bench == null)
                return;

            if (!bench.IsNear(pData) || !bench.IsAccessableFor(pData))
                return;

            if (!bench.AddPlayerObserving(pData))
            {
                player.Notify("Container::Wait", Settings.WORKBENCH_MAX_PLAYERS);

                return;
            }

            string result = bench.ToClientJson();

            player.TriggerEvent("Inventory::Show", 4, result);
        }

        [RemoteEvent("Workbench::Close")]
        private static void WorkbenchClose(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var wb = pData.CurrentWorkbench;

            if (wb == null)
                return;

            wb.RemovePlayerObserving(pData, false);
        }

        [RemoteEvent("Workbench::Craft")]
        private static void WorkbenchCraft(Player player, int receiptIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (pData.CurrentWorkbench is Game.Items.Craft.Workbench curBench)
            {
                if (!curBench.IsNear(pData) || !curBench.IsAccessableFor(pData))
                {
                    curBench.RemovePlayerObserving(pData, true);

                    return;
                }

                if (receiptIdx < 0)
                {
                    if (curBench.CurrentPendingCraftData?.IsInProcess != true)
                        return;

                    curBench.CancelCraft();
                }
                else
                {
                    if (curBench.ResultItem != null)
                        return;

                    if (curBench.CurrentPendingCraftData?.IsInProcess == true)
                        return;

                    var receipt = Craft.Receipt.GetByIndex(receiptIdx);

                    if (receipt == null)
                        return;

                    var craftItems = curBench.GetOrderedItems();

                    var expAmount = receipt.GetExpectedAmountByIngredients(craftItems);

                    if (expAmount <= 0)
                        return;

                    var timeout = receipt.CraftResultData.CraftTime * expAmount;

                    curBench.ProceedCraft(craftItems, receipt, expAmount, timeout);
                }
            }
        }

        [RemoteEvent("Workbench::Replace")]
        private static void WorkbenchReplace(Player player, int toStr, int slotTo, int fromStr, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var wb = pData.CurrentWorkbench;

            if (wb == null)
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (!Enum.IsDefined(typeof(Game.Items.Inventory.Groups), toStr) || !Enum.IsDefined(typeof(Game.Items.Inventory.Groups), fromStr))
                return;

            if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                return;

            var to = (Game.Items.Inventory.Groups)toStr;
            var from = (Game.Items.Inventory.Groups)fromStr;

            if (!wb.IsNear(pData) || !wb.IsAccessableFor(pData))
            {
                wb.RemovePlayerObserving(pData, true);

                return;
            }

            if (wb.CurrentPendingCraftData != null)
                return;

            var action = Game.Items.Craft.Workbench.GetReplaceAction(from, to);

            if (action != null)
            {
                var res = action.Invoke(pData, wb, slotTo, slotFrom, amount);

                var notification = Game.Items.Inventory.ResultsNotifications.GetValueOrDefault(res);

                if (notification == null)
                    return;

                player.Notify(notification);
            }
        }

        [RemoteEvent("Workbench::Drop")]
        private static void WorkbenchDrop(Player player, int groupNum, int slot, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (amount < 1 || slot < 0)
                return;

            if (!Enum.IsDefined(typeof(Game.Items.Inventory.Groups), groupNum))
                return;

            var group = (Game.Items.Inventory.Groups)groupNum;

            if (group != Groups.CraftItems && group != Groups.CraftResult)
                return;

            var wb = pData.CurrentWorkbench;

            if (wb == null)
                return;

            if (slot >= wb.Items.Length)
                return;

            if (!pData.CanUseInventory(true))
                return;

            if (!wb.IsNear(pData) || !wb.IsAccessableFor(pData))
            {
                wb.RemovePlayerObserving(pData, true);

                return;
            }

            if (wb.CurrentPendingCraftData != null)
                return;

            var item = group == Groups.CraftItems ? wb.Items[slot] : wb.ResultItem;

            if (item == null || item is Game.Items.WorkbenchTool)
                return;

            if (item is Game.Items.IStackable itemStackable)
            {
                int curAmount = itemStackable.Amount;

                if (amount > curAmount)
                    amount = curAmount;

                curAmount -= amount;

                if (curAmount > 0)
                {
                    itemStackable.Amount = curAmount;

                    if (group == Groups.CraftItems)
                        wb.Items[slot] = item;
                    else
                        wb.ResultItem = item;

                    item.Update();
                    item = Game.Items.Stuff.CreateItem(item.ID, 0, amount);
                }
                else
                {
                    if (group == Groups.CraftItems)
                        wb.Items[slot] = null;
                    else
                        wb.ResultItem = null;
                }
            }
            else
            {
                if (group == Groups.CraftItems)
                    wb.Items[slot] = null;
                else
                    wb.ResultItem = null;
            }

            var upd = Game.Items.Item.ToClientJson(group == Groups.CraftItems ? wb.Items[slot] : wb.ResultItem, group);

            var players = wb.GetPlayersObservingArray();

            if (players.Length > 0)
                Utils.InventoryUpdate(group, slot, upd, players);

            if (pData.CanPlayAnimNow())
                pData.PlayAnim(Sync.Animations.FastTypes.Putdown);

            Sync.World.AddItemOnGround(pData, item, player.GetFrontOf(0.6f), player.Rotation, player.Dimension);
        }
    }
}
