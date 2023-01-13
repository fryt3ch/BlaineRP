using BCRPServer.Game.Items;
using GTANetworkAPI;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Events.Players
{
    class Inventory : Script
    {
        [RemoteEvent("Inventory::Replace")]
        private static void InventoryReplace(Player player, int to, int slotTo, int from, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Groups), to) || !Enum.IsDefined(typeof(Groups), from))
                return;

            if (!pData.CanUseInventory(true))
                return;

            Replace(pData, (Groups)to, slotTo, (Groups)from, slotFrom, amount);
        }

        [RemoteEvent("Inventory::Action")]
        private static void InventoryAction(Player player, int group, int slot, int action, string data)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (data == null)
                return;

            if (!Enum.IsDefined(typeof(Groups), group) || slot < 0 || action < 5)
                return;

            if (pData.CurrentBusiness != null)
                return;

            if (!pData.CanUseInventory(true))
                return;

            Action(pData, (Groups)group, slot, action, data.Split('&'));
        }

        [RemoteEvent("Inventory::Drop")]
        private static void InventoryDrop(Player player, int slotStr, int slot, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Groups), slotStr) || slot < 0)
                return;

            if (!pData.CanUseInventory(true))
                return;

            Drop(pData, (Groups)slotStr, slot, amount);
        }

        [RemoteEvent("Inventory::Take")]
        private static void InventoryTake(Player player, uint UID, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true))
                return;

            var item = Sync.World.GetItemOnGround(UID);

            if (item?.Item == null)
                return;

            if (!player.AreEntitiesNearby(item.Object, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

            int freeIdx = -1, curAmount = 1;

            if (item.Item is Game.Items.IStackable stackable)
            {
                curAmount = stackable.Amount;

                if (amount > curAmount)
                    amount = curAmount;

                if (curWeight + amount * item.Item.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                {
                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / item.Item.BaseWeight);
                }

                if (amount > 0)
                {
                    for (int i = 0; i < pData.Items.Length; i++)
                    {
                        var curItem = pData.Items[i];

                        if (curItem == null)
                        {
                            if (freeIdx < 0)
                                freeIdx = i;

                            continue;
                        }

                        if (curItem.ID == item.Item.ID && curItem is Game.Items.IStackable curItemStackable && curItemStackable.Amount + amount <= curItemStackable.MaxAmount)
                        {
                            freeIdx = i;

                            break;
                        }
                    }
                }
            }
            else
            {
                if (amount != 1)
                    amount = 1;

                if (curWeight + item.Item.Weight <= Settings.MAX_INVENTORY_WEIGHT)
                    freeIdx = Array.IndexOf(pData.Items, null);
            }

            if (amount <= 0 || freeIdx < 0)
            {
                player.Notify("Inventory::NoSpace");

                return;
            }

            if (!pData.CanPlayAnim())
                pData.PlayAnim(Sync.Animations.FastTypes.Pickup);

            if (amount == curAmount)
            {
                if (pData.Items[freeIdx] != null)
                {
                    item.Delete(true);

                    ((Game.Items.IStackable)pData.Items[freeIdx]).Amount += amount;

                    pData.Items[freeIdx].Update();
                }
                else
                {
                    item.Delete(false);

                    pData.Items[freeIdx] = item.Item;
                }
            }
            else
            {
                ((Game.Items.IStackable)item.Item).Amount -= amount;

                item.UpdateAmount();

                if (pData.Items[freeIdx] != null)
                {
                    ((Game.Items.IStackable)pData.Items[freeIdx]).Amount += amount;

                    pData.Items[freeIdx].Update();
                }
                else
                {
                    pData.Items[freeIdx] = Game.Items.Items.CreateItem(item.Item.ID, 0, amount);
                }
            }

            player.InventoryUpdate(Groups.Items, freeIdx, pData.Items[freeIdx].ToClientJson(Groups.Items));

            MySQL.CharacterItemsUpdate(pData.Info);
        }

        #region Containers
        [RemoteEvent("Container::Show")]
        private static void ContainerShow(Player player, uint uid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentContainer != null)
                return;

            var cont = Game.Items.Container.Get(uid);

            if (cont == null)
                return;

            if (!(cont.IsNear(pData) && cont.IsAccessableFor(pData)))
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

            if (pData.CurrentContainer == null)
                return;

            if (!pData.CanUseInventory(true))
                return;

            if (!Enum.IsDefined(typeof(Game.Items.Inventory.Groups), toStr) || !Enum.IsDefined(typeof(Game.Items.Inventory.Groups), fromStr))
                return;

            if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                return;

            Game.Items.Inventory.Groups to = (Game.Items.Inventory.Groups)toStr;
            Game.Items.Inventory.Groups from = (Game.Items.Inventory.Groups)fromStr;

            var cont = Game.Items.Container.Get((uint)pData.CurrentContainer);

            if (cont == null)
                return;

            if (!(cont.IsNear(pData) && cont.IsAccessableFor(pData)))
            {
                player.TriggerEvent("Inventory::Close");

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

            if (pData.CurrentContainer == null || amount < 1 || slot < 0)
                return;

            var cont = Game.Items.Container.Get((uint)pData.CurrentContainer);

            if (cont == null)
                return;

            if (slot >= cont.Items.Length)
                return;

            var item = cont.Items[slot];

            if (item == null)
                return;

            if (!(cont.IsNear(pData) && cont.IsAccessableFor(pData)))
            {
                player.TriggerEvent("Inventory::Close");

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
                    item = Game.Items.Items.CreateItem(item.ID, 0, amount);
                }
                else
                    cont.Items[slot] = null;
            }
            else
                cont.Items[slot] = null;

            var upd = Game.Items.Item.ToClientJson(cont.Items[slot], Game.Items.Inventory.Groups.Container);

            foreach (var x in cont.PlayersObserving)
            {
                var target = x?.Player;

                if (target?.Exists != true)
                    return;

                target.InventoryUpdate(Groups.Container, slot, upd);
            }

            cont.Update();

            if (!pData.CanPlayAnim())
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

            if (pData.CurrentContainer == null)
                return;

            var cont = Game.Items.Container.Get((uint)pData.CurrentContainer);

            cont.RemovePlayerObserving(pData);
        }
        #endregion

        [RemoteEvent("Weapon::Reload")]
        public static void WeaponReload(Player player, int currentAmmo)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var weapon = pData.ActiveWeapon;

            if (weapon == null)
                return;

            if (!pData.CanUseInventory(true))
                return;

            if (currentAmmo > weapon.Value.WeaponItem.Ammo || currentAmmo < 0)
                currentAmmo = 0;

            if (currentAmmo == weapon.Value.WeaponItem.Data.MaxAmmo)
                return;

            weapon.Value.WeaponItem.Ammo = currentAmmo;

            pData.InventoryAction(weapon.Value.Group, weapon.Value.Slot, 6);
        }

        [RemoteEvent("Gift::Collect")]
        private static void GiftCollect(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true))
                return;

            var gift = pData.Gifts.Where(x => x.ID == id).FirstOrDefault();

            if (gift == null)
                return;

            if (gift.Collect(pData))
            {
                pData.Gifts.Remove(gift);

                gift.Delete();

                player.TriggerEvent("Menu::Gifts::Update", false, id);
            }
        }
    }
}
