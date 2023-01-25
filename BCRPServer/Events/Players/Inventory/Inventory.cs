using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Events.Players
{
    partial class Inventory : Script
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

            if (item.Type == Sync.World.ItemOnGround.Types.PlacedItem && !item.PlayerHasAccess(pData, false, true))
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
                    if (item.Type == Sync.World.ItemOnGround.Types.PlacedItem && item.Item is Game.Items.PlaceableItem placeableItem)
                    {
                        placeableItem.Remove(pData);
                    }
                    else
                    {
                        item.Delete(false);
                    }

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
                    pData.Items[freeIdx] = Game.Items.Stuff.CreateItem(item.Item.ID, 0, amount);
                }
            }

            player.InventoryUpdate(Groups.Items, freeIdx, pData.Items[freeIdx].ToClientJson(Groups.Items));

            MySQL.CharacterItemsUpdate(pData.Info);
        }

        [RemoteEvent("Item::IOGL")]
        private static void ItemOnGroundLock(Player player, uint uid, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var iog = Sync.World.GetItemOnGround(uid);

            if (iog == null || iog.Type != Sync.World.ItemOnGround.Types.PlacedItem)
                return;

            if (!iog.PlayerHasAccess(pData, false, true))
                return;

            if (iog.IsLocked == state)
                return;

            iog.IsLocked = state;
        }

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
