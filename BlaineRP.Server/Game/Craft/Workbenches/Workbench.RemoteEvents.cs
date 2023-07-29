using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Items;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Craft.Workbenches
{
    public abstract partial class Workbench
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Workbench::Show")]
            private static void WorkbenchShow(Player player, byte wTypeNum, uint uid)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Types), wTypeNum))
                    return;

                var wType = (Types)wTypeNum;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                if (pData.CurrentContainer != null || pData.CurrentWorkbench != null)
                    return;

                Workbench bench = Get(wType, uid);

                if (bench == null)
                    return;

                if (!bench.IsNear(pData) || !bench.IsAccessableFor(pData))
                    return;

                if (!bench.AddPlayerObserving(pData))
                {
                    player.Notify("Container::Wait", Properties.Settings.Static.WORKBENCH_MAX_PLAYERS);

                    return;
                }

                string result = bench.ToClientJson();

                player.TriggerEvent("Inventory::Show", 4, result);
            }

            [RemoteEvent("Workbench::Close")]
            private static void WorkbenchClose(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Workbench wb = pData.CurrentWorkbench;

                if (wb == null)
                    return;

                wb.RemovePlayerObserving(pData, false);
            }

            [RemoteEvent("Workbench::Craft")]
            private static void WorkbenchCraft(Player player, int receiptIdx)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                if (pData.CurrentWorkbench is Workbench curBench)
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

                        var receipt = Receipt.GetByIndex(receiptIdx);

                        if (receipt == null)
                            return;

                        List<Item> craftItems = curBench.GetOrderedItems();

                        int expAmount = receipt.GetExpectedAmountByIngredients(craftItems);

                        if (expAmount <= 0)
                            return;

                        int timeout = receipt.CraftResultData.CraftTime * expAmount;

                        curBench.ProceedCraft(craftItems, receipt, expAmount, timeout);
                    }
                }
            }

            [RemoteEvent("Workbench::Replace")]
            private static void WorkbenchReplace(Player player, int toStr, int slotTo, int fromStr, int slotFrom, int amount)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Workbench wb = pData.CurrentWorkbench;

                if (wb == null)
                    return;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                if (!Enum.IsDefined(typeof(GroupTypes), toStr) || !Enum.IsDefined(typeof(GroupTypes), fromStr))
                    return;

                if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                    return;

                var to = (GroupTypes)toStr;
                var from = (GroupTypes)fromStr;

                if (!wb.IsNear(pData) || !wb.IsAccessableFor(pData))
                {
                    wb.RemovePlayerObserving(pData, true);

                    return;
                }

                if (wb.CurrentPendingCraftData != null)
                    return;

                Func<PlayerData, Workbench, int, int, int, Inventory.Service.ResultTypes> action = GetReplaceAction(from, to);

                if (action != null)
                {
                    Inventory.Service.ResultTypes res = action.Invoke(pData, wb, slotTo, slotFrom, amount);

                    string notification = Game.Inventory.Service.ResultsNotifications.GetValueOrDefault(res);

                    if (notification == null)
                        return;

                    player.Notify(notification);
                }
            }

            [RemoteEvent("Workbench::Drop")]
            private static void WorkbenchDrop(Player player, int groupNum, int slot, int amount)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                if (amount < 1 || slot < 0)
                    return;

                if (!Enum.IsDefined(typeof(GroupTypes), groupNum))
                    return;

                var group = (GroupTypes)groupNum;

                if (group != GroupTypes.CraftItems && group != GroupTypes.CraftResult)
                    return;

                Workbench wb = pData.CurrentWorkbench;

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

                Item item = group == GroupTypes.CraftItems ? wb.Items[slot] : wb.ResultItem;

                if (item == null || item is Items.WorkbenchTool)
                    return;

                if (item is Items.IStackable itemStackable)
                {
                    int curAmount = itemStackable.Amount;

                    if (amount > curAmount)
                        amount = curAmount;

                    curAmount -= amount;

                    if (curAmount > 0)
                    {
                        itemStackable.Amount = curAmount;

                        if (group == GroupTypes.CraftItems)
                            wb.Items[slot] = item;
                        else
                            wb.ResultItem = item;

                        item.Update();
                        item = Game.Items.Stuff.CreateItem(item.ID, 0, amount);
                    }
                    else
                    {
                        if (group == GroupTypes.CraftItems)
                            wb.Items[slot] = null;
                        else
                            wb.ResultItem = null;
                    }
                }
                else
                {
                    if (group == GroupTypes.CraftItems)
                        wb.Items[slot] = null;
                    else
                        wb.ResultItem = null;
                }

                string upd = Game.Items.Item.ToClientJson(group == GroupTypes.CraftItems ? wb.Items[slot] : wb.ResultItem, group);

                Player[] players = wb.GetPlayersObservingArray();

                if (players.Length > 0)
                    Utils.InventoryUpdate(group, slot, upd, players);

                if (pData.CanPlayAnimNow())
                    pData.PlayAnim(FastType.Putdown, Properties.Settings.Static.InventoryPutdownAnimationTime);

                World.Service.AddItemOnGround(pData, item, player.GetFrontOf(0.6f), player.Rotation, player.Dimension);
            }
        }
    }
}