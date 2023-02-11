using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Events.Players
{
    partial class Inventory : Script
    {
        [RemoteEvent("Container::Show")]
        private static void ContainerShow(Player player, uint uid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentContainer != null || pData.CurrentWorkbench != null)
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var cont = Game.Items.Container.Get(uid);

            if (cont == null)
                return;

            if (!cont.IsNear(pData) || !cont.IsAccessableFor(pData))
            {
                if (cont.Entity?.Type == EntityType.Vehicle)
                    player.Notify("Vehicle::NotAllowed");

                return;
            }

            if (!cont.AddPlayerObserving(pData))
            {
                player.Notify("Container::Wait");

                return;
            }

            string result = cont.ToClientJson();

            player.TriggerEvent("Inventory::Show", 1, result);
        }

        [RemoteEvent("Container::Replace")]
        private static void ContainerReplace(Player player, int toStr, int slotTo, int fromStr, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var cont = pData.CurrentContainer;

            if (cont == null)
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (!Enum.IsDefined(typeof(Game.Items.Inventory.Groups), toStr) || !Enum.IsDefined(typeof(Game.Items.Inventory.Groups), fromStr))
                return;

            if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                return;

            var to = (Game.Items.Inventory.Groups)toStr;
            var from = (Game.Items.Inventory.Groups)fromStr;

            if (!cont.IsNear(pData) && !cont.IsAccessableFor(pData))
            {
                cont.RemovePlayerObserving(pData, true);

                return;
            }

            var action = Game.Items.Container.GetReplaceAction(from, to);

            if (action != null)
            {
                var res = action.Invoke(pData, cont, slotTo, slotFrom, amount);

                var notification = Game.Items.Inventory.ResultsNotifications.GetValueOrDefault(res);

                if (notification == null)
                    return;

                player.Notify(notification);
            }
        }

        [RemoteEvent("Container::Drop")]
        private static void ContainerDrop(Player player, int slot, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var cont = pData.CurrentContainer;

            if (cont == null || amount < 1 || slot < 0)
                return;

            if (slot >= cont.Items.Length)
                return;

            var item = cont.Items[slot];

            if (item == null)
                return;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (!cont.IsNear(pData) && !cont.IsAccessableFor(pData))
            {
                cont.RemovePlayerObserving(pData, true);

                return;
            }

            if (item is Game.Items.IStackable itemStackable)
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
                    cont.Items[slot] = null;
            }
            else
                cont.Items[slot] = null;

            var upd = Game.Items.Item.ToClientJson(cont.Items[slot], Game.Items.Inventory.Groups.Container);

            var players = cont.GetPlayersObservingArray();

            if (players.Length > 0)
                Utils.InventoryUpdate(Groups.Container, slot, upd, players);

            cont.Update();

            if (pData.CanPlayAnimNow())
                pData.PlayAnim(Sync.Animations.FastTypes.Putdown);

            Sync.World.AddItemOnGround(pData, item, player.GetFrontOf(0.6f), player.Rotation, player.Dimension);
        }

        [RemoteEvent("Container::Close")]
        private static void ContainerClose(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var cont = pData.CurrentContainer;

            if (cont == null)
                return;

            cont.RemovePlayerObserving(pData, false);
        }
    }
}
