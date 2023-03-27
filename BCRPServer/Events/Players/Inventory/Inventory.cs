using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
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

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
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

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
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

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
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

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || player.Vehicle != null)
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

            if (pData.CanPlayAnimNow())
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

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var iog = Sync.World.GetItemOnGround(uid);

            if (iog == null || iog.Type != Sync.World.ItemOnGround.Types.PlacedItem)
                return;

            if (!iog.PlayerHasAccess(pData, false, true))
                return;

            if (iog.IsLocked == state)
                return;

            iog.IsLocked = state;
        }

        [RemoteEvent("opws")]
        public static void OnPlayerWeaponShot(Player player)
        {
            if (player?.Exists != true)
                return;

            var pData = PlayerData.Get(player);

            if (pData == null)
                return;

            if (pData.Weapons[0]?.Equiped == true)
            {
                if (pData.Weapons[0].Ammo <= 0)
                    return;

                pData.Weapons[0].Ammo--;

                return;
            }
            else if (pData.Weapons[1]?.Equiped == true)
            {
                if (pData.Weapons[1].Ammo <= 0)
                    return;

                pData.Weapons[1].Ammo--;

                return;
            }

            var hWeapon = pData.Holster?.Weapon;

            if (hWeapon?.Equiped == true)
            {
                hWeapon.Ammo--;
            }
        }

        [RemoteEvent("Weapon::Reload")]
        public static void WeaponReload(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var weapon = pData.ActiveWeapon;

            if (weapon == null)
                return;

            pData.InventoryAction(weapon.Value.Group, weapon.Value.Slot, 6);
        }

        [RemoteEvent("Gift::Collect")]
        private static void GiftCollect(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
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

        [RemoteProc("WSkins::Rm")]
        private static bool WeaponSkinsRemove(Player player, int wSkinTypeNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            if (!Enum.IsDefined(typeof(Game.Items.WeaponSkin.ItemData.Types), wSkinTypeNum))
                return false;

            var wSkinType = (Game.Items.WeaponSkin.ItemData.Types)wSkinTypeNum;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var ws = pData.Info.WeaponSkins.GetValueOrDefault(wSkinType);

            if (ws == null)
                return false;

            var freeIdx = -1;

            for (int i = 0; i < pData.Items.Length; i++)
            {
                if (pData.Items[i] == null)
                {
                    freeIdx = i;

                    break;
                }
            }

            if (freeIdx < 0)
            {
                player.Notify("Inventory::NoSpace");

                return false;
            }

            pData.Info.WeaponSkins.Remove(wSkinType);

            pData.Items[freeIdx] = ws;

            player.InventoryUpdate(Groups.Items, freeIdx, ws.ToClientJson(Groups.Items));

            player.TriggerEvent("Player::WSkins::Update", false, ws.ID);

            MySQL.CharacterWeaponSkinsUpdate(pData.Info);
            MySQL.CharacterItemsUpdate(pData.Info);

            for (int i = 0; i < pData.Weapons.Length; i++)
            {
                if (pData.Weapons[i] is Game.Items.Weapon weapon)
                {
                    weapon.UpdateWeaponComponents(pData);
                }
            }

            if (pData.Holster?.Items[0] is Game.Items.Weapon hWeapon)
            {
                hWeapon.UpdateWeaponComponents(pData);
            }

            return true;
        }

        [RemoteEvent("Player::SUCI")]
        private static void StopUseCurrentItem(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var item = pData.CurrentItemInUse;

            if (item == null)
                return;

            item.Value.Item.StopUse(pData, Groups.Items, item.Value.Slot, true);
        }

        [RemoteEvent("Player::ParachuteS")]
        private static void ParachuteState(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (state)
            {
                for (int slot = 0; slot < pData.Items.Length; slot++)
                {
                    if (pData.Items[slot] is Game.Items.Parachute parachute && parachute.InUse)
                    {
                        if (parachute.StopUse(pData, Groups.Items, slot, false, false))
                        {
                            parachute.Delete();

                            pData.Items[slot] = null;

                            player.InventoryUpdate(Groups.Items, slot, Game.Items.Item.ToClientJson(pData.Items[slot], Groups.Items));

                            MySQL.CharacterItemsUpdate(pData.Info);

                            player.AttachObject(Sync.AttachSystem.Models.ParachuteSync, Sync.AttachSystem.Types.ParachuteSync, -1, null);

                            return;
                        }
                    }
                }
            }
            else
            {
                player.DetachObject(Sync.AttachSystem.Types.ParachuteSync);
            }
        }
    }
}
