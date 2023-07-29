using System;
using System.Collections.Generic;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Items;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Containers
{
    public partial class Container
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Container::Show")]
            private static void ContainerShow(Player player, uint uid)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.CurrentContainer != null || pData.CurrentWorkbench != null)
                    return;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                var cont = Get(uid);

                if (cont == null)
                    return;

                if (!cont.IsNear(pData) || !cont.IsAccessableFor(pData))
                    return;

                if (!cont.AddPlayerObserving(pData))
                {
                    player.Notify("Container::Wait", Properties.Settings.Static.CONTAINER_MAX_PLAYERS);

                    return;
                }

                string result = cont.ToClientJson();

                player.TriggerEvent("Inventory::Show", 1, result);
            }

            [RemoteEvent("Container::Replace")]
            private static void ContainerReplace(Player player, int toStr, int slotTo, int fromStr, int slotFrom, int amount)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Container cont = pData.CurrentContainer;

                if (cont == null)
                    return;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                if (!Enum.IsDefined(typeof(GroupTypes), toStr) || !Enum.IsDefined(typeof(GroupTypes), fromStr))
                    return;

                if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                    return;

                var to = (GroupTypes)toStr;
                var from = (GroupTypes)fromStr;

                if (!cont.IsNear(pData) || !cont.IsAccessableFor(pData))
                {
                    cont.RemovePlayerObserving(pData, true);

                    return;
                }

                Func<PlayerData, Container, int, int, int, Inventory.Service.ResultTypes> action = Container.GetReplaceAction(from, to);

                if (action != null)
                {
                    Inventory.Service.ResultTypes res = action.Invoke(pData, cont, slotTo, slotFrom, amount);

                    var notification = Inventory.Service.ResultsNotifications.GetValueOrDefault(res);

                    if (notification == null)
                        return;

                    player.Notify(notification);
                }
            }

            [RemoteEvent("Container::Drop")]
            private static void ContainerDrop(Player player, int slot, int amount)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Container cont = pData.CurrentContainer;

                if (cont == null || amount < 1 || slot < 0)
                    return;

                if (slot >= cont.Items.Length)
                    return;

                Item item = cont.Items[slot];

                if (item == null)
                    return;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                if (!cont.IsNear(pData) || !cont.IsAccessableFor(pData))
                {
                    cont.RemovePlayerObserving(pData, true);

                    return;
                }

                if (item is Items.IStackable itemStackable)
                {
                    int curAmount = itemStackable.Amount;

                    if (amount > curAmount)
                        amount = curAmount;

                    curAmount -= amount;

                    if (curAmount > 0)
                    {
                        itemStackable.Amount = curAmount;
                        cont.Items[slot] = item;

                        item.Update();
                        item = Game.Items.Stuff.CreateItem(item.ID, 0, amount);
                    }
                    else
                    {
                        cont.Items[slot] = null;
                    }
                }
                else
                {
                    cont.Items[slot] = null;
                }

                string upd = Game.Items.Item.ToClientJson(cont.Items[slot], GroupTypes.Container);

                Player[] players = cont.GetPlayersObservingArray();

                if (players.Length > 0)
                    Utils.InventoryUpdate(GroupTypes.Container, slot, upd, players);

                cont.Update();

                if (pData.CanPlayAnimNow())
                    pData.PlayAnim(FastType.Putdown, Properties.Settings.Static.InventoryPutdownAnimationTime);

                World.Service.AddItemOnGround(pData, item, player.GetFrontOf(0.6f), player.Rotation, player.Dimension);
            }

            [RemoteEvent("Container::Close")]
            private static void ContainerClose(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                Container cont = pData.CurrentContainer;

                if (cont == null)
                    return;

                cont.RemovePlayerObserving(pData, false);
            }
        }
    }
}