using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items
{
    public partial class Inventory
    {
        private static Dictionary<Groups, Func<PlayerData, int, int, Game.Items.Item>> DropActions = new Dictionary<Groups, Func<PlayerData, int, int, Game.Items.Item>>()
        {
            {
                Groups.Items,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (slot >= pData.Items.Length)
                        return null;

                    var item = pData.Items[slot];

                    if (item == null)
                        return null;

                    if (item is Game.Items.IStackable itemStackable)
                    {
                        int curAmount = itemStackable.Amount;

                        if (amount > curAmount)
                            amount = curAmount;

                        curAmount -= amount;

                        if (curAmount > 0)
                        {
                            itemStackable.Amount = curAmount;
                            pData.Items[slot] = item;

                            item.Update();
                            item = Game.Items.Stuff.CreateItem(item.ID, 0, amount);
                        }
                        else
                            pData.Items[slot] = null;
                    }
                    else
                    {
                        pData.Items[slot] = null;

                        if (item is Game.Items.IUsable itemU && itemU.InUse)
                            itemU.StopUse(pData, Groups.Items, -1, false);
                    }

                    var upd = Game.Items.Item.ToClientJson(pData.Items[slot], Groups.Items);

                    player.InventoryUpdate(Groups.Items, slot, upd);

                    MySQL.CharacterItemsUpdate(pData.Info);

                    return item;
                }
            },

            {
                Groups.Bag,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (pData.Bag == null || slot >= pData.Bag.Items.Length)
                        return null;

                    var item = pData.Bag.Items[slot];

                    if (item == null)
                        return null;

                    if (item is Game.Items.IStackable itemStackable)
                    {
                        int curAmount = itemStackable.Amount;

                        if (amount > curAmount)
                            amount = curAmount;

                        curAmount -= amount;

                        if (curAmount > 0)
                        {
                            itemStackable.Amount = curAmount;
                            pData.Bag.Items[slot] = item;

                            item.Update();
                            item = Game.Items.Stuff.CreateItem(item.ID, 0, amount);
                        }
                        else
                            pData.Bag.Items[slot] = null;
                    }
                    else
                        pData.Bag.Items[slot] = null;

                    var upd = Game.Items.Item.ToClientJson(pData.Bag.Items[slot], Groups.Bag);

                    player.InventoryUpdate(Groups.Bag, slot, upd);

                    pData.Bag.Update();

                    return item;
                }
            },

            {
                Groups.Weapons,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (slot >= pData.Weapons.Length)
                        return null;

                    var item = pData.Weapons[slot];

                    if (item == null)
                        return null;

                    player.InventoryUpdate(Groups.Weapons, slot, Game.Items.Item.ToClientJson(null, Groups.Weapons));

                    if (item.Equiped)
                        item.Unequip(pData, false);
                    else
                        item.Unwear(pData);

                    pData.Weapons[slot] = null;

                    MySQL.CharacterWeaponsUpdate(pData.Info);

                    return item;
                }
            },

            {
                Groups.Holster,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (pData.Holster == null)
                        return null;

                    var item = (Game.Items.Weapon)pData.Holster.Items[0];

                    if (item == null)
                        return null;

                    player.InventoryUpdate(Groups.Holster, 2, Game.Items.Item.ToClientJson(null, Groups.Holster));

                    if (item.Equiped)
                        item.Unequip(pData, false);
                    else
                        item.Unwear(pData);

                    pData.Holster.Items[0] = null;

                    pData.Holster.Update();

                    return item;
                }
            },

            {
                Groups.Clothes,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (slot >= pData.Clothes.Length)
                        return null;

                    var item = pData.Clothes[slot];

                    if (item == null)
                        return null;

                    player.InventoryUpdate(Groups.Clothes, slot, Game.Items.Item.ToClientJson(null, Groups.Clothes));

                    item.Unwear(pData);

                    pData.Clothes[slot] = null;

                    MySQL.CharacterClothesUpdate(pData.Info);

                    return item;
                }
            },

            {
                Groups.Accessories,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (slot >= pData.Accessories.Length)
                        return null;

                    var item = pData.Accessories[slot];

                    if (item == null)
                        return null;

                    player.InventoryUpdate(Groups.Accessories, slot, Game.Items.Item.ToClientJson(null, Groups.Accessories));

                    item.Unwear(pData);

                    pData.Accessories[slot] = null;

                    MySQL.CharacterAccessoriesUpdate(pData.Info);

                    return item;
                }
            },

            {
                Groups.BagItem,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    var item = pData.Bag;

                    if (item == null)
                        return null;

                    player.InventoryUpdate(Groups.BagItem, Game.Items.Item.ToClientJson(null, Groups.BagItem));

                    item.Unwear(pData);

                    pData.Bag = null;

                    MySQL.CharacterBagUpdate(pData.Info);

                    return item;
                }
            },

            {
                Groups.HolsterItem,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    var item = pData.Holster;

                    if (item == null)
                        return null;

                    player.InventoryUpdate(Groups.HolsterItem, Game.Items.Item.ToClientJson(null, Groups.HolsterItem));

                    item.Unwear(pData);

                    item.Weapon?.Unequip(pData, false);

                    pData.Holster = null;

                    MySQL.CharacterHolsterUpdate(pData.Info);

                    return item;
                }
            },

            {
                Groups.Armour,

                (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    var item = pData.Armour;

                    if (item == null)
                        return null;

                    player.InventoryUpdate(Groups.Armour, Game.Items.Item.ToClientJson(null, Groups.Armour));

                    item.Unwear(pData);

                    pData.Armour = null;

                    MySQL.CharacterArmourUpdate(pData.Info);

                    return item;
                }
            },
        };
    }
}
