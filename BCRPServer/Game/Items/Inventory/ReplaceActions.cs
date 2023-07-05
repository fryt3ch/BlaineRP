using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Items
{
    public partial class Inventory
    {
        private static Dictionary<GroupTypes, Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>> ReplaceActions = new Dictionary<GroupTypes, Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>>()
        {
            {
                GroupTypes.Items,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                if (toItem.IsTemp || fromItem.IsTemp)
                                    return ResultTypes.TempItem;

                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount >= maxStack)
                                    return ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (toStackable.Amount + amount > maxStack)
                                {
                                    fromStackable.Amount -= maxStack - toStackable.Amount;
                                    toStackable.Amount = maxStack;
                                }
                                else
                                {
                                    toStackable.Amount += amount;
                                    fromStackable.Amount -= amount;

                                    if (fromStackable.Amount == 0)
                                    {
                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Items[slotFrom] = null;

                                        MySQL.CharacterItemsUpdate(pData.Info);
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount) // split to new item
                            {
                                if (fromItem.IsTemp)
                                    return ResultTypes.TempItem;

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                pData.Items[slotFrom] = toItem;
                                pData.Items[slotTo] = fromItem;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items);

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, upd1, GroupTypes.Items, slotTo, upd2);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (fromItem is Game.Items.Bag)
                                return ResultTypes.PlaceRestricted;

                            if (fromItem.IsTemp)
                                return ResultTypes.TempItem;

                            float curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            float maxWeight = pData.Bag.Data.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount >= maxStack)
                                    return ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return ResultTypes.NoSpace;
                                }

                                if (toStackable.Amount + amount > maxStack)
                                {
                                    fromStackable.Amount -= maxStack - toStackable.Amount;
                                    toStackable.Amount = maxStack;
                                }
                                else
                                {
                                    toStackable.Amount += amount;
                                    fromStackable.Amount -= amount;

                                    if (fromStackable.Amount == 0)
                                    {
                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Items[slotFrom] = null;

                                        MySQL.CharacterItemsUpdate(pData.Info);
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount)
                            {
                                if (fromItem.BaseWeight * amount + curWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return ResultTypes.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Bag.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);

                                pData.Bag.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if (addWeightBag - addWeightItems + curWeight > maxWeight || addWeightItems - addWeightBag + pData.Items.Sum(x => x?.Weight ?? 0f) > Settings.MAX_INVENTORY_WEIGHT)
                                    return ResultTypes.NoSpace;

                                pData.Items[slotFrom] = toItem;
                                pData.Bag.Items[slotTo] = fromItem;

                                if (fromItem is Game.Items.IUsable fromItemU && fromItemU.InUse)
                                    fromItemU.StopUse(pData, GroupTypes.Bag, slotTo, false);

                                pData.Bag.Update();
                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag);

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, upd1, GroupTypes.Bag, slotTo, upd2);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Weapons,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Weapons[slotTo];

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                    return ResultTypes.NoSpace;

                                pData.Weapons[slotTo] = fromWeapon;
                                pData.Items[slotFrom] = toItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                MySQL.CharacterWeaponsUpdate(pData.Info);
                            }
                            else if (fromItem is Game.Items.WeaponComponent wc)
                            {
                                if (toItem == null)
                                    return ResultTypes.Error;

                                var wcData = wc.Data;

                                if (!wcData.IsAllowedFor(toItem.Data.Hash))
                                {
                                    pData.Player.Notify("Inventory::WWC");

                                    return ResultTypes.Error;
                                }

                                var wcIdx = (int)wcData.Type;

                                if (toItem.Items[wcIdx] != null)
                                {
                                    pData.Player.Notify("Inventory::WHTC");

                                    return ResultTypes.Error;
                                }
                                else
                                {
                                    toItem.Items[wcIdx] = wc;

                                    pData.Items[slotFrom] = null;

                                    player.InventoryUpdate(GroupTypes.Weapons, slotTo, toItem.ToClientJson(GroupTypes.Weapons), GroupTypes.Items, slotFrom, Game.Items.Item.ToClientJson(null, GroupTypes.Items));

                                    toItem.UpdateWeaponComponents(pData);

                                    toItem.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);

                                    return ResultTypes.Success;
                                }
                            }
                            #endregion
                            #region Load
                            else if (fromItem is Game.Items.Ammo fromAmmo && toItem != null && fromItem.ID == toItem.Data.AmmoID)
                            {
                                var maxAmount = toItem.Data.MaxAmmo;

                                if (amount == -1 || amount > fromAmmo.Amount)
                                    amount = fromAmmo.Amount;

                                if (toItem.Ammo == maxAmount)
                                    return ResultTypes.Error;

                                if (toItem.Ammo + amount > maxAmount)
                                {
                                    fromAmmo.Amount -= maxAmount - toItem.Ammo;
                                    toItem.Ammo = maxAmount;
                                }
                                else
                                {
                                    toItem.Ammo += amount;
                                    fromAmmo.Amount -= amount;

                                    if (fromAmmo.Amount == 0)
                                    {
                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Items[slotFrom] = null;

                                        MySQL.CharacterItemsUpdate(pData.Info);
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            else
                                return ResultTypes.Error;

                            if (pData.Items[slotFrom] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData, false);

                                    pData.Weapons[slotTo].Equip(pData);
                                }
                                else
                                {
                                    weapon.Unwear(pData);

                                    pData.Weapons[slotTo].Wear(pData);
                                }
                            }
                            else
                            {
                                pData.Weapons[slotTo].UpdateAmmo(pData);

                                pData.Weapons[slotTo].Wear(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items), GroupTypes.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], GroupTypes.Weapons));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Holster,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (pData.Holster == null)
                                return ResultTypes.Error;

                            var toItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem.IsTemp)
                                return ResultTypes.TempItem;

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (fromWeapon.Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                    return ResultTypes.PlaceRestricted;

                                if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                    return ResultTypes.NoSpace;

                                pData.Holster.Items[0] = fromWeapon;
                                pData.Items[slotFrom] = toItem;

                                pData.Holster.Update();
                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            else if (fromItem is Game.Items.WeaponComponent wc)
                            {
                                if (toItem == null)
                                    return ResultTypes.Error;

                                var wcData = wc.Data;

                                if (!wcData.IsAllowedFor(toItem.Data.Hash))
                                {
                                    pData.Player.Notify("Inventory::WWC");

                                    return ResultTypes.Error;
                                }

                                var wcIdx = (int)wcData.Type;

                                if (toItem.Items[wcIdx] != null)
                                {
                                    pData.Player.Notify("Inventory::WHTC");

                                    return ResultTypes.Error;
                                }
                                else
                                {
                                    toItem.Items[wcIdx] = wc;

                                    pData.Items[slotFrom] = null;

                                    player.InventoryUpdate(GroupTypes.Holster, 2, toItem.ToClientJson(GroupTypes.Holster), GroupTypes.Items, slotFrom, Game.Items.Item.ToClientJson(null, GroupTypes.Items));

                                    toItem.UpdateWeaponComponents(pData);

                                    toItem.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);

                                    return ResultTypes.Success;
                                }
                            }
                            #endregion
                            #region Load
                            else if (fromItem is Game.Items.Ammo fromAmmo && toItem != null && (fromItem.ID == toItem.Data.AmmoID))
                            {
                                var maxAmount = toItem.Data.MaxAmmo;

                                if (toItem.Ammo == maxAmount)
                                    return ResultTypes.Error;

                                if (amount == -1 || amount > fromAmmo.Amount)
                                    amount = fromAmmo.Amount;

                                if (toItem.Ammo + amount > maxAmount)
                                {
                                    fromAmmo.Amount -= maxAmount - toItem.Ammo;
                                    toItem.Ammo = maxAmount;
                                }
                                else
                                {
                                    toItem.Ammo += amount;
                                    fromAmmo.Amount -= amount;

                                    if (fromAmmo.Amount == 0)
                                    {
                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Items[slotFrom] = null;

                                        MySQL.CharacterItemsUpdate(pData.Info);
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            else
                                return ResultTypes.Error;

                            if (pData.Items[slotFrom] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData, false);

                                    ((Game.Items.Weapon)pData.Holster.Items[0]).Equip(pData);
                                }
                                else
                                {
                                    weapon.Unwear(pData);

                                    ((Game.Items.Weapon)pData.Holster.Items[0]).Wear(pData);
                                }
                            }
                            else
                            {
                                ((Game.Items.Weapon)pData.Holster.Items[0]).UpdateAmmo(pData);

                                ((Game.Items.Weapon)pData.Holster.Items[0]).Wear(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Items, slotFrom,  Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items), GroupTypes.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], GroupTypes.Holster));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Clothes,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Clothes.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Clothes[slotTo];

                            if (!(fromItem is Game.Items.Clothes fromClothes))
                                return ResultTypes.Error;

                            int actualSlot;

                            if (!ClothesSlots.TryGetValue(fromItem.Type, out actualSlot) || slotTo != actualSlot)
                                return ResultTypes.Error;

                            if (Game.Data.Customization.IsUniformElementActive(pData, fromClothes is Items.Clothes.IProp ? fromClothes.Slot + 1000 : fromClothes.Slot, true))
                                return ResultTypes.Error;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Clothes[slotTo] = fromClothes;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterClothesUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Clothes[slotTo], GroupTypes.Clothes);

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, upd1, GroupTypes.Clothes, slotTo, upd2);

                            (pData.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                            pData.Clothes[slotTo].Wear(pData);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Accessories,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Accessories.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Accessories[slotTo];

                            if (!(fromItem is Game.Items.Clothes fromClothes))
                                return ResultTypes.Error;

                            int actualSlot;

                            if (!AccessoriesSlots.TryGetValue(fromItem.Type, out actualSlot) || slotTo != actualSlot)
                                return ResultTypes.Error;

                            if (Game.Data.Customization.IsUniformElementActive(pData, fromClothes is Items.Clothes.IProp ? fromClothes.Slot + 1000 : fromClothes.Slot, true))
                                return ResultTypes.Error;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Accessories[slotTo] = fromClothes;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterAccessoriesUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Accessories[slotTo], GroupTypes.Accessories);

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, upd1, GroupTypes.Accessories, slotTo, upd2);

                            (pData.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                            pData.Accessories[slotTo].Wear(pData);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.HolsterItem,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            var toItem = pData.Holster;

                            if (!(fromItem is Game.Items.Holster fromHolster))
                                return ResultTypes.Error;

                            if (fromItem.IsTemp || toItem?.Items[0]?.IsTemp == true)
                                return ResultTypes.TempItem;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Holster = fromHolster;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterHolsterUpdate(pData.Info);

                            (pData.Items[slotFrom] as Game.Items.Holster)?.Unwear(pData);

                            pData.Holster.Wear(pData);

                            if (pData.Items[slotFrom] is Game.Items.Holster holster && ((Game.Items.Weapon)holster.Items[0])?.Equiped == true)
                            {
                                ((Game.Items.Weapon)holster.Items[0]).Unequip(pData);
                                ((Game.Items.Weapon)pData.Holster.Items[0])?.Equip(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items), GroupTypes.HolsterItem, 0, Game.Items.Item.ToClientJson(pData.Holster, GroupTypes.HolsterItem));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.BagItem,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            var toItem = pData.Bag;

                            if (!(fromItem is Game.Items.Bag fromBag))
                                return ResultTypes.Error;

                            if (fromItem.IsTemp)
                                return ResultTypes.TempItem;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Bag = fromBag;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterBagUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag, GroupTypes.BagItem);

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, upd1, GroupTypes.BagItem, 0, upd2);

                            (pData.Items[slotFrom] as Game.Items.Bag)?.Unwear(pData);
                            pData.Bag.Wear(pData);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Armour,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            var toItem = pData.Armour;

                            if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                                return ResultTypes.Wounded;

                            if (!(fromItem is Game.Items.Armour fromArmour))
                                return ResultTypes.Error;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Armour = fromArmour;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterArmourUpdate(pData.Info);

                            var upd2 = Game.Items.Item.ToClientJson(pData.Armour, GroupTypes.Armour);

                            (pData.Items[slotFrom] as Game.Items.Armour)?.Unwear(pData);
                            pData.Armour.Wear(pData);

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items), GroupTypes.Armour, 0, upd2);

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.Bag,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return ResultTypes.Error;

                                if (amount == -1 || amount > toStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (toStackable.Amount + amount > maxStack)
                                {
                                    fromStackable.Amount -= maxStack - toStackable.Amount;
                                    toStackable.Amount = maxStack;
                                }
                                else
                                {
                                    toStackable.Amount += amount;
                                    fromStackable.Amount -= amount;

                                    if (fromStackable.Amount == 0)
                                    {
                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Bag.Items[slotFrom] = null;

                                        pData.Bag.Update();
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount) // split to new item
                            {
                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Bag.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);

                                pData.Bag.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                pData.Bag.Items[slotFrom] = toItem;
                                pData.Bag.Items[slotTo] = fromItem;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag);

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, upd1, GroupTypes.Bag, slotTo, upd2);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem is Game.Items.Bag)
                                return ResultTypes.Error;

                            float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return ResultTypes.NoSpace;
                                }

                                if (toStackable.Amount + amount > maxStack)
                                {
                                    fromStackable.Amount -= maxStack - toStackable.Amount;
                                    toStackable.Amount = maxStack;
                                }
                                else
                                {
                                    toStackable.Amount += amount;
                                    fromStackable.Amount -= amount;

                                    if (fromStackable.Amount == 0)
                                    {
                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Bag.Items[slotFrom] = null;

                                        pData.Bag.Update();
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount)
                            {
                                if (fromItem.BaseWeight * amount + curWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return ResultTypes.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if (addWeightBag - addWeightItems + curWeight > Settings.MAX_INVENTORY_WEIGHT || addWeightItems - addWeightBag + pData.Bag.Weight - pData.Bag.BaseWeight > pData.Bag.Data.MaxWeight)
                                    return ResultTypes.NoSpace;

                                pData.Bag.Items[slotFrom] = toItem;
                                pData.Items[slotTo] = fromItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                pData.Bag.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items);

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, upd1, GroupTypes.Items, slotTo, upd2);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Weapons,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Weapons[slotTo];

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (toItem != null && pData.Bag.Weight - pData.Bag.BaseWeight + toItem.Weight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                    return ResultTypes.NoSpace;

                                pData.Weapons[slotTo] = fromWeapon;
                                pData.Bag.Items[slotFrom] = toItem;

                                MySQL.CharacterWeaponsUpdate(pData.Info);
                                pData.Bag.Update();
                            }
                            else if (fromItem is Game.Items.WeaponComponent wc)
                            {
                                if (toItem == null)
                                    return ResultTypes.Error;

                                var wcData = wc.Data;

                                if (!wcData.IsAllowedFor(toItem.Data.Hash))
                                {
                                    pData.Player.Notify("Inventory::WWC");

                                    return ResultTypes.Error;
                                }

                                var wcIdx = (int)wcData.Type;

                                if (toItem.Items[wcIdx] != null)
                                {
                                    pData.Player.Notify("Inventory::WHTC");

                                    return ResultTypes.Error;
                                }
                                else
                                {
                                    toItem.Items[wcIdx] = wc;

                                    pData.Bag.Items[slotFrom] = null;

                                    player.InventoryUpdate(GroupTypes.Weapons, slotTo, toItem.ToClientJson(GroupTypes.Weapons), GroupTypes.Bag, slotFrom, Game.Items.Item.ToClientJson(null, GroupTypes.Bag));

                                    toItem.UpdateWeaponComponents(pData);

                                    toItem.Update();

                                    pData.Bag.Update();

                                    return ResultTypes.Success;
                                }
                            }
                            #endregion
                            #region Load
                            else if (fromItem is Game.Items.Ammo fromAmmo && toItem != null && fromItem.ID == toItem.Data.AmmoID)
                            {
                                var maxAmount = toItem.Data.MaxAmmo;

                                if (toItem.Ammo == maxAmount)
                                    return ResultTypes.Error;

                                if (amount == -1 || amount > fromAmmo.Amount)
                                    amount = fromAmmo.Amount;

                                if (toItem.Ammo + amount > maxAmount)
                                {
                                    fromAmmo.Amount -= maxAmount - toItem.Ammo;
                                    toItem.Ammo = maxAmount;
                                }
                                else
                                {
                                    toItem.Ammo += amount;
                                    fromAmmo.Amount -= amount;

                                    if (fromAmmo.Amount == 0)
                                    {
                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Bag.Items[slotFrom] = null;

                                        pData.Bag.Update();
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            else
                                return ResultTypes.Error;

                            if (pData.Bag.Items[slotFrom] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData, false);

                                    pData.Weapons[slotTo].Equip(pData);
                                }
                                else
                                {
                                    weapon.Unwear(pData);

                                    pData.Weapons[slotTo].Wear(pData);
                                }
                            }
                            else
                            {
                                pData.Weapons[slotTo].UpdateAmmo(pData);

                                pData.Weapons[slotTo].Wear(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag), GroupTypes.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], GroupTypes.Weapons));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Holster,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (pData.Holster == null)
                                return ResultTypes.Error;

                            var toItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (fromWeapon.Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                    return ResultTypes.PlaceRestricted;

                                if (toItem != null && pData.Bag.Weight - pData.Bag.BaseWeight + toItem.Weight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                    return ResultTypes.NoSpace;

                                pData.Holster.Items[0] = fromWeapon;
                                pData.Bag.Items[slotFrom] = toItem;

                                pData.Holster.Update();
                                pData.Bag.Update();
                            }
                            else if (fromItem is Game.Items.WeaponComponent wc)
                            {
                                if (toItem == null)
                                    return ResultTypes.Error;

                                var wcData = wc.Data;

                                if (!wcData.IsAllowedFor(toItem.Data.Hash))
                                {
                                    pData.Player.Notify("Inventory::WWC");

                                    return ResultTypes.Error;
                                }

                                var wcIdx = (int)wcData.Type;

                                if (toItem.Items[wcIdx] != null)
                                {
                                    pData.Player.Notify("Inventory::WHTC");

                                    return ResultTypes.Error;
                                }
                                else
                                {
                                    toItem.Items[wcIdx] = wc;

                                    pData.Bag.Items[slotFrom] = null;

                                    player.InventoryUpdate(GroupTypes.Holster, 2, toItem.ToClientJson(GroupTypes.Holster), GroupTypes.Bag, slotFrom, Game.Items.Item.ToClientJson(null, GroupTypes.Bag));

                                    toItem.UpdateWeaponComponents(pData);

                                    toItem.Update();

                                    pData.Bag.Update();

                                    return ResultTypes.Success;
                                }
                            }
                            #endregion
                            #region Load
                            else if (fromItem is Game.Items.Ammo fromAmmo && toItem != null && fromItem.ID == toItem.Data.AmmoID)
                            {
                                var maxAmount = toItem.Data.MaxAmmo;

                                if (toItem.Ammo == maxAmount)
                                    return ResultTypes.Error;

                                if (amount == -1 || amount > fromAmmo.Amount)
                                    amount = fromAmmo.Amount;

                                if (toItem.Ammo + amount > maxAmount)
                                {
                                    fromAmmo.Amount -= maxAmount - toItem.Ammo;
                                    toItem.Ammo = maxAmount;
                                }
                                else
                                {
                                    toItem.Ammo += amount;
                                    fromAmmo.Amount -= amount;

                                    if (fromAmmo.Amount == 0)
                                    {
                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Bag.Items[slotFrom] = null;

                                        pData.Bag.Update();
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            else
                                return ResultTypes.Error;

                            if (pData.Bag.Items[slotFrom] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData, false);

                                    (pData.Holster.Items[0] as Game.Items.Weapon).Equip(pData);
                                }
                                else
                                {
                                    weapon.Unwear(pData);

                                    (pData.Holster.Items[0] as Game.Items.Weapon).Wear(pData);
                                }
                            }
                            else
                            {
                                (pData.Holster.Items[0] as Game.Items.Weapon).UpdateAmmo(pData);

                                (pData.Holster.Items[0] as Game.Items.Weapon).Wear(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag), GroupTypes.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], GroupTypes.Holster));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Clothes,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Clothes.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Clothes[slotTo];

                            if (!(fromItem is Game.Items.Clothes fromClothes))
                                return ResultTypes.Error;

                            int actualSlot;

                            if (!ClothesSlots.TryGetValue(fromItem.Type, out actualSlot) || slotTo != actualSlot)
                                return ResultTypes.Error;

                            if (Game.Data.Customization.IsUniformElementActive(pData, fromClothes is Items.Clothes.IProp ? fromClothes.Slot + 1000 : fromClothes.Slot, true))
                                return ResultTypes.Error;

                            if (toItem != null && toItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                return ResultTypes.NoSpace;

                            pData.Clothes[slotTo] = fromClothes;
                            pData.Bag.Items[slotFrom] = toItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Clothes[slotTo], GroupTypes.Clothes);

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, upd1, GroupTypes.Clothes, slotTo, upd2);

                            (pData.Bag.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                            pData.Clothes[slotTo].Wear(pData);

                            pData.Bag.Update();
                            MySQL.CharacterClothesUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Accessories,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Accessories.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Accessories[slotTo];

                            if (!(fromItem is Game.Items.Clothes fromClothes))
                                return ResultTypes.Error;

                            int actualSlot;

                            if (!AccessoriesSlots.TryGetValue(fromItem.Type, out actualSlot) || slotTo != actualSlot)
                                return ResultTypes.Error;

                            if (Game.Data.Customization.IsUniformElementActive(pData, fromClothes is Items.Clothes.IProp ? fromClothes.Slot + 1000 : fromClothes.Slot, true))
                                return ResultTypes.Error;

                            if (toItem != null && toItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                return ResultTypes.NoSpace;

                            pData.Accessories[slotTo] = fromClothes;
                            pData.Bag.Items[slotFrom] = toItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Accessories[slotTo], GroupTypes.Accessories);

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, upd1, GroupTypes.Accessories, slotTo, upd2);

                            (pData.Bag.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                            pData.Accessories[slotTo].Wear(pData);

                            pData.Bag.Update();
                            MySQL.CharacterAccessoriesUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.HolsterItem,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            var toItem = pData.Holster;

                            if (!(fromItem is Game.Items.Holster fromHolster))
                                return ResultTypes.Error;

                            if (toItem != null && toItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                return ResultTypes.NoSpace;

                            pData.Holster = fromHolster;
                            pData.Bag.Items[slotFrom] = toItem;

                            (pData.Bag.Items[slotFrom] as Game.Items.Holster)?.Unwear(pData);
                            pData.Holster.Wear(pData);

                            if (pData.Bag.Items[slotFrom] != null && ((pData.Bag.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Bag.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag), GroupTypes.HolsterItem, 0, Game.Items.Item.ToClientJson(pData.Holster, GroupTypes.HolsterItem));

                            pData.Bag.Update();
                            MySQL.CharacterHolsterUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Armour,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                                return ResultTypes.Wounded;

                            var toItem = pData.Armour;

                            if (!(fromItem is Game.Items.Armour fromArmour))
                                return ResultTypes.Error;

                            if (toItem != null && toItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                return ResultTypes.NoSpace;

                            pData.Armour = fromArmour;
                            pData.Bag.Items[slotFrom] = toItem;

                            var upd2 = Game.Items.Item.ToClientJson(pData.Armour, GroupTypes.Armour);

                            (pData.Bag.Items[slotFrom] as Game.Items.Armour)?.Unwear(pData);
                            pData.Armour.Wear(pData);

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag), GroupTypes.Armour, 0, upd2);

                            pData.Bag.Update();
                            MySQL.CharacterArmourUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.Weapons,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Weapons,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Weapons.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Weapons[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Weapons[slotTo];

                            pData.Weapons[slotTo] = fromItem;
                            pData.Weapons[slotFrom] = toItem;

                            if (pData.Weapons[slotFrom]?.Equiped == true)
                            {
                                pData.Weapons[slotFrom].Unequip(pData);

                                pData.Weapons[slotTo].Equip(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], GroupTypes.Weapons), GroupTypes.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], GroupTypes.Weapons));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Holster,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Weapons.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Weapons[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (pData.Holster == null)
                                return ResultTypes.Error;

                            var toItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem.Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                return ResultTypes.PlaceRestricted;

                            if (fromItem.IsTemp)
                                return ResultTypes.TempItem;

                            pData.Holster.Items[0] = fromItem;
                            pData.Weapons[slotFrom] = toItem;

                            if (pData.Weapons[slotFrom]?.Equiped == true)
                            {
                                pData.Weapons[slotFrom].Unequip(pData);
                                fromItem.Equip(pData);
                            }
                            else
                            {
                                fromItem.Wear(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], GroupTypes.Weapons), GroupTypes.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], GroupTypes.Holster));

                            pData.Holster.Update();

                            MySQL.CharacterWeaponsUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Weapons.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Weapons[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            bool extractToExisting = toItem is Game.Items.Ammo && toItem.ID == fromItem.Data.AmmoID;

                            #region Extract
                            if (amount != -1 || extractToExisting) // extract ammo from weapon
                            {
                                if (fromItem.IsTemp)
                                    return ResultTypes.TempItem;

                                if (fromItem.Ammo == 0 || fromItem.Data.AmmoID == null)
                                    return ResultTypes.Error;

                                if (amount == -1)
                                    amount = fromItem.Ammo;

                                var curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);
                                var ammoWeight = Game.Items.Ammo.GetData(fromItem.Data.AmmoID).Weight;

                                if (extractToExisting)
                                {
                                    var toAmmo = (Game.Items.Ammo)toItem;

                                    int maxStack = toAmmo.MaxAmount;

                                    if (curWeight + amount * ammoWeight > Settings.MAX_INVENTORY_WEIGHT)
                                    {
                                        amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / ammoWeight);

                                        if (amount <= 0)
                                            return ResultTypes.NoSpace;
                                    }

                                    if (toAmmo.Amount + amount > maxStack)
                                    {
                                        fromItem.Ammo -= maxStack - toAmmo.Amount;
                                        toAmmo.Amount = maxStack;
                                    }
                                    else
                                    {
                                        toAmmo.Amount += amount;
                                        fromItem.Ammo -= amount;
                                    }

                                    fromItem.Update();
                                    toItem.Update();
                                }
                                else if (toItem == null)
                                {
                                    if (curWeight + amount * ammoWeight > Settings.MAX_INVENTORY_WEIGHT)
                                    {
                                        amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / ammoWeight);

                                        if (amount <= 0)
                                            return ResultTypes.NoSpace;
                                    }

                                    fromItem.Ammo -= amount;
                                    pData.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.Data.AmmoID, 0, amount);

                                    fromItem.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);
                                }
                            }
                            #endregion
                            #region Replace
                            else if (toItem == null || toItem is Game.Items.Weapon)
                            {
                                if (pData.Items.Sum(x => x?.Weight ?? 0f) + fromItem.Weight - (toItem?.Weight ?? 0f) > Settings.MAX_INVENTORY_WEIGHT)
                                    return ResultTypes.NoSpace;

                                pData.Items[slotTo] = fromItem;
                                pData.Weapons[slotFrom] = (Game.Items.Weapon)toItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                MySQL.CharacterWeaponsUpdate(pData.Info);
                            }
                            else
                                return ResultTypes.Error;
                            #endregion

                            if (pData.Items[slotTo] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData, false);

                                    pData.Weapons[slotFrom]?.Equip(pData);
                                }
                                else
                                {
                                    weapon.Unwear(pData);

                                    pData.Weapons[slotFrom]?.Wear(pData);
                                }
                            }
                            else
                            {
                                if (pData.Weapons[slotFrom] != null)
                                {
                                    pData.Weapons[slotFrom].UpdateAmmo(pData);

                                    pData.Weapons[slotFrom].Wear(pData);
                                }
                            }

                            player.InventoryUpdate(GroupTypes.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items), GroupTypes.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], GroupTypes.Weapons));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Weapons.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Weapons[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (fromItem.IsTemp)
                                return ResultTypes.TempItem;

                            bool extractToExisting = toItem is Game.Items.Ammo && toItem.ID == fromItem.Data.AmmoID;

                            #region Extract
                            if (amount != -1 || extractToExisting)
                            {
                                if (fromItem.Ammo == 0 || fromItem.Data.AmmoID == null)
                                    return ResultTypes.Error;

                                if (amount == -1)
                                    amount = fromItem.Ammo;

                                var curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                                var maxWeight = pData.Bag.Data.MaxWeight;

                                var ammoWeight = Game.Items.Ammo.GetData(fromItem.Data.AmmoID).Weight;

                                if (extractToExisting)
                                {
                                    var toAmmo = (Game.Items.Ammo)toItem;

                                    int maxStack = toAmmo.MaxAmount;

                                    if (curWeight + amount * ammoWeight > maxWeight)
                                    {
                                        amount = (int)Math.Floor((maxWeight - curWeight) / ammoWeight);

                                        if (amount <= 0)
                                            return ResultTypes.NoSpace;
                                    }

                                    if (toAmmo.Amount + amount > maxStack)
                                    {
                                        fromItem.Ammo -= maxStack - toAmmo.Amount;
                                        toAmmo.Amount = maxStack;
                                    }
                                    else
                                    {
                                        toAmmo.Amount += amount;
                                        fromItem.Ammo -= amount;
                                    }

                                    fromItem.Update();
                                    toItem.Update();
                                }
                                else if (toItem == null)
                                {
                                    if (curWeight + amount * ammoWeight > maxWeight)
                                    {
                                        amount = (int)Math.Floor((maxWeight - curWeight) / ammoWeight);

                                        if (amount <= 0)
                                            return ResultTypes.NoSpace;
                                    }

                                    fromItem.Ammo -= amount;
                                    pData.Bag.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.Data.AmmoID, 0, amount);

                                    fromItem.Update();

                                    pData.Bag.Update();
                                }
                            }
                            #endregion
                            #region Replace
                            else if (toItem == null || toItem is Game.Items.Weapon)
                            {
                                if ((pData.Bag.Weight - pData.Bag.BaseWeight + fromItem.Weight - (toItem?.Weight ?? 0) > pData.Bag.Data.MaxWeight))
                                    return ResultTypes.NoSpace;

                                pData.Bag.Items[slotTo] = fromItem;
                                pData.Weapons[slotFrom] = (Game.Items.Weapon)toItem;

                                pData.Bag.Update();
                                MySQL.CharacterWeaponsUpdate(pData.Info);
                            }
                            else
                                return ResultTypes.Error;
                            #endregion

                            if (pData.Bag.Items[slotTo] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData, false);

                                    pData.Weapons[slotFrom]?.Equip(pData);
                                }
                                else
                                {
                                    weapon.Unwear(pData);

                                    pData.Weapons[slotFrom]?.Wear(pData);
                                }
                            }
                            else
                            {
                                if (pData.Weapons[slotFrom] != null)
                                {
                                    pData.Weapons[slotFrom].UpdateAmmo(pData);

                                    pData.Weapons[slotFrom].Wear(pData);
                                }
                            }


                            player.InventoryUpdate(GroupTypes.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag), GroupTypes.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], GroupTypes.Weapons));

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.Holster,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Weapons,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Holster == null)
                                return ResultTypes.Error;

                            var fromItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return ResultTypes.PlaceRestricted;

                            var toItem = pData.Weapons[slotTo];

                            if (toItem != null && toItem.Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                return ResultTypes.PlaceRestricted;

                            pData.Holster.Items[0] = toItem;
                            pData.Weapons[slotTo] = fromItem;

                            if (pData.Weapons[slotTo].Equiped)
                            {
                                pData.Weapons[slotTo].Unequip(pData);
                                (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                            }
                            else
                                pData.Weapons[slotTo].Unwear(pData);

                            player.InventoryUpdate(GroupTypes.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], GroupTypes.Weapons), GroupTypes.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], GroupTypes.Holster));

                            pData.Holster.Update();

                            MySQL.CharacterWeaponsUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Holster == null)
                                return ResultTypes.Error;

                            var fromItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            bool extractToExisting = toItem is Game.Items.Ammo && toItem.ID == fromItem.Data.AmmoID;

                            #region Extract
                            if (amount != -1 || extractToExisting)
                            {
                                if (fromItem.Ammo == 0 || fromItem.Data.AmmoID == null)
                                    return ResultTypes.Error;

                                if (amount == -1)
                                    amount = fromItem.Ammo;

                                var curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);
                                var ammoWeight = Game.Items.Ammo.GetData(fromItem.Data.AmmoID).Weight;

                                if (extractToExisting)
                                {
                                    var toAmmo = (Game.Items.Ammo)toItem;

                                    int maxStack = toAmmo.MaxAmount;

                                    if (curWeight + amount * ammoWeight > Settings.MAX_INVENTORY_WEIGHT)
                                    {
                                        amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / ammoWeight);

                                        if (amount <= 0)
                                            return ResultTypes.NoSpace;
                                    }

                                    if (toAmmo.Amount + amount > maxStack)
                                    {
                                        fromItem.Ammo -= maxStack - toAmmo.Amount;
                                        toAmmo.Amount = maxStack;
                                    }
                                    else
                                    {
                                        toAmmo.Amount += amount;
                                        fromItem.Ammo -= amount;
                                    }

                                    fromItem.Update();
                                    toItem.Update();
                                }
                                else if (toItem == null)
                                {
                                    if (curWeight + amount * ammoWeight > Settings.MAX_INVENTORY_WEIGHT)
                                    {
                                        amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / ammoWeight);

                                        if (amount <= 0)
                                            return ResultTypes.NoSpace;
                                    }

                                    fromItem.Ammo -= amount;
                                    pData.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.Data.AmmoID, 0, amount);

                                    fromItem.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);
                                }
                            }
                            #endregion
                            #region Replace
                            else if (toItem == null || toItem is Game.Items.Weapon)
                            {
                                if (pData.Items.Sum(x => x?.Weight ?? 0f) + fromItem.Weight - (toItem?.Weight ?? 0) > Settings.MAX_INVENTORY_WEIGHT)
                                    return ResultTypes.NoSpace;

                                pData.Items[slotTo] = fromItem;
                                pData.Holster.Items[0] = (Game.Items.Weapon)toItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                pData.Holster.Update();
                            }
                            else
                                return ResultTypes.Error;
                            #endregion

                            if (pData.Items[slotTo] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData, false);

                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                                }
                                else
                                {
                                    weapon.Unwear(pData);

                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Wear(pData);
                                }
                            }
                            else
                            {
                                if (pData.Holster.Items[0] is Game.Items.Weapon nWeapon)
                                {
                                    nWeapon.UpdateAmmo(pData);

                                    nWeapon.Wear(pData);
                                }
                            }

                            player.InventoryUpdate(GroupTypes.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items), GroupTypes.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], GroupTypes.Holster));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Holster == null)
                                return ResultTypes.Error;

                            var fromItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            bool extractToExisting = toItem is Game.Items.Ammo && toItem.ID == fromItem.Data.AmmoID;

                            #region Extract
                            if (amount != -1 || extractToExisting)
                            {
                                if (fromItem.Ammo == 0 || fromItem.Data.AmmoID == null)
                                    return ResultTypes.Error;

                                if (amount == -1)
                                    amount = fromItem.Ammo;

                                var curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                                var maxWeight = pData.Bag.Data.MaxWeight;

                                var ammoWeight = Game.Items.Ammo.GetData(fromItem.Data.AmmoID).Weight;

                                if (extractToExisting)
                                {
                                    var toAmmo = (Game.Items.Ammo)toItem;

                                    int maxStack = toAmmo.MaxAmount;

                                    if (curWeight + amount * ammoWeight > maxWeight)
                                    {
                                        amount = (int)Math.Floor((maxWeight - curWeight) / ammoWeight);

                                        if (amount <= 0)
                                            return ResultTypes.NoSpace;
                                    }

                                    if (toAmmo.Amount + amount > maxStack)
                                    {
                                        fromItem.Ammo -= maxStack - toAmmo.Amount;
                                        toAmmo.Amount = maxStack;
                                    }
                                    else
                                    {
                                        toAmmo.Amount += amount;
                                        fromItem.Ammo -= amount;
                                    }

                                    fromItem.Update();
                                    toItem.Update();
                                }
                                else if (toItem == null)
                                {
                                    if (curWeight + amount * ammoWeight > pData.Bag.Data.MaxWeight)
                                    {
                                        amount = (int)Math.Floor((maxWeight - curWeight) / ammoWeight);

                                        if (amount <= 0)
                                            return ResultTypes.NoSpace;
                                    }

                                    fromItem.Ammo -= amount;
                                    pData.Bag.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.Data.AmmoID, 0, amount);

                                    fromItem.Update();

                                    pData.Bag.Update();
                                }
                            }
                            #endregion
                            #region Replace
                            else if (toItem == null || toItem is Game.Items.Weapon)
                            {
                                if ((pData.Bag.Weight - pData.Bag.BaseWeight + fromItem.Weight - (toItem?.Weight ?? 0)) > pData.Bag.Data.MaxWeight)
                                    return ResultTypes.NoSpace;

                                pData.Bag.Items[slotTo] = fromItem;
                                pData.Holster.Items[0] = (Game.Items.Weapon)toItem;

                                pData.Bag.Update();
                                pData.Holster.Update();
                            }
                            else
                                return ResultTypes.Error;
                            #endregion

                            if (pData.Bag.Items[slotTo] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData, false);

                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                                }
                                else
                                {
                                    weapon.Unwear(pData);

                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Wear(pData);
                                }
                            }
                            else
                            {
                                if (pData.Holster.Items[0] is Game.Items.Weapon nWeapon)
                                {
                                    nWeapon.UpdateAmmo(pData);

                                    nWeapon.Wear(pData);
                                }
                            }


                            player.InventoryUpdate(GroupTypes.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag), GroupTypes.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], GroupTypes.Holster));

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.Armour,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Armour;

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                                return ResultTypes.Wounded;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && (!(toItem is Game.Items.Armour)))
                                return ResultTypes.Error;

                            if ((pData.Items.Sum(x => x?.Weight ?? 0f) + fromItem.Weight - (toItem?.Weight ?? 0)) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Armour = (Game.Items.Armour)toItem;
                            pData.Items[slotTo] = fromItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterArmourUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Armour, GroupTypes.Armour);

                            (pData.Items[slotTo] as Game.Items.Armour).Unwear(pData);
                            pData.Armour?.Wear(pData);

                            player.InventoryUpdate(GroupTypes.Armour, 0, upd1, GroupTypes.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Armour;

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                                return ResultTypes.Wounded;

                            if (fromItem.IsTemp)
                                return ResultTypes.TempItem;

                            if (pData.Bag == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (toItem != null && (!(toItem is Game.Items.Armour)))
                                return ResultTypes.Error;

                            if ((pData.Bag.Weight - pData.Bag.BaseWeight + fromItem.Weight - (toItem?.Weight ?? 0f)) > pData.Bag.Data.MaxWeight)
                                return ResultTypes.NoSpace;

                            pData.Armour = (Game.Items.Armour)toItem;
                            pData.Bag.Items[slotTo] = fromItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Armour, GroupTypes.Armour);

                            (pData.Bag.Items[slotTo] as Game.Items.Armour).Unwear(pData);
                            pData.Armour?.Wear(pData);

                            player.InventoryUpdate(GroupTypes.Armour, 0, upd1, GroupTypes.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag));

                            pData.Bag.Update();
                            MySQL.CharacterArmourUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.Clothes,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Clothes.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Clothes[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && toItem.Type != fromItem.Type)
                                return ResultTypes.Error;

                            if (Game.Data.Customization.IsUniformElementActive(pData, fromItem is Items.Clothes.IProp ? fromItem.Slot + 1000 : fromItem.Slot, true))
                                return ResultTypes.Error;

                            if ((pData.Items.Sum(x => x?.Weight ?? 0f) + fromItem.Weight - (toItem?.Weight ?? 0f)) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Items[slotTo] = fromItem;
                            pData.Clothes[slotFrom] = (Game.Items.Clothes)toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterClothesUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Clothes[slotFrom], GroupTypes.Clothes);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items);

                            player.InventoryUpdate(GroupTypes.Clothes, slotFrom, upd1, GroupTypes.Items, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                            pData.Clothes[slotFrom]?.Wear(pData);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Clothes.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Clothes[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (fromItem.IsTemp)
                                return ResultTypes.TempItem;

                            if (pData.Bag == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (toItem != null && toItem.Type != fromItem.Type)
                                return ResultTypes.Error;

                            if (Game.Data.Customization.IsUniformElementActive(pData, fromItem is Items.Clothes.IProp ? fromItem.Slot + 1000 : fromItem.Slot, true))
                                return ResultTypes.Error;

                            var curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            var maxWeight = pData.Bag.Data.MaxWeight;

                            if ((curWeight + fromItem.Weight - (toItem?.Weight ?? 0f)) > maxWeight)
                                return ResultTypes.NoSpace;

                            pData.Bag.Items[slotTo] = fromItem;
                            pData.Clothes[slotFrom] = (Game.Items.Clothes)toItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Clothes[slotFrom], GroupTypes.Clothes);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag);

                            player.InventoryUpdate(GroupTypes.Clothes, slotFrom, upd1, GroupTypes.Bag, slotTo, upd2);

                            (pData.Bag.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                            pData.Clothes[slotFrom]?.Wear(pData);

                            pData.Bag.Update();
                            MySQL.CharacterClothesUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.Accessories,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Accessories.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Accessories[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && toItem.Type != fromItem.Type)
                                return ResultTypes.Error;

                            if (Game.Data.Customization.IsUniformElementActive(pData, fromItem is Items.Clothes.IProp ? fromItem.Slot + 1000 : fromItem.Slot, true))
                                return ResultTypes.Error;

                            if ((pData.Items.Sum(x => x?.Weight ?? 0f) + fromItem.Weight - (toItem?.Weight ?? 0f)) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Items[slotTo] = fromItem;
                            pData.Accessories[slotFrom] = (Game.Items.Clothes)toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterAccessoriesUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Accessories[slotFrom], GroupTypes.Accessories);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items);

                            player.InventoryUpdate(GroupTypes.Accessories, slotFrom, upd1, GroupTypes.Items, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                            (pData.Accessories[slotFrom])?.Wear(pData);

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Accessories.Length)
                                return ResultTypes.Error;

                            var fromItem = pData.Accessories[slotFrom];

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (fromItem.IsTemp)
                                return ResultTypes.TempItem;

                            if (pData.Bag == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (toItem != null && toItem.Type != fromItem.Type)
                                return ResultTypes.Error;

                            if (Game.Data.Customization.IsUniformElementActive(pData, fromItem is Items.Clothes.IProp ? fromItem.Slot + 1000 : fromItem.Slot, true))
                                return ResultTypes.Error;

                            var curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            var maxWeight = pData.Bag.Data.MaxWeight;

                            if ((curWeight + fromItem.Weight - (toItem?.Weight ?? 0f)) > maxWeight)
                                return ResultTypes.NoSpace;

                            pData.Bag.Items[slotTo] = fromItem;
                            pData.Accessories[slotFrom] = (Game.Items.Clothes)toItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Accessories[slotFrom], GroupTypes.Accessories);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag);

                            player.InventoryUpdate(GroupTypes.Accessories, slotFrom, upd1, GroupTypes.Bag, slotTo, upd2);

                            (pData.Bag.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                            pData.Accessories[slotFrom]?.Wear(pData);

                            pData.Bag.Update();
                            MySQL.CharacterAccessoriesUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.BagItem,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Bag;

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && !(toItem is Game.Items.Bag))
                                return ResultTypes.Error;

                            if ((fromItem.Weight + pData.Items.Sum(x => x?.Weight ?? 0f) - (toItem?.Weight ?? 0f)) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Bag = (Game.Items.Bag)toItem;
                            pData.Items[slotTo] = fromItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterBagUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag, GroupTypes.BagItem);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items);

                            player.InventoryUpdate(GroupTypes.BagItem, 0, upd1, GroupTypes.Items, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.Bag).Unwear(pData);
                            pData.Bag?.Wear(pData);

                            return ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.HolsterItem,

                new Dictionary<GroupTypes, Func<PlayerData, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Holster;

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && !(toItem is Game.Items.Holster))
                                return ResultTypes.Error;

                            if ((fromItem.Weight + pData.Items.Sum(x => x?.Weight ?? 0f) - (toItem?.Weight ?? 0f)) > Settings.MAX_INVENTORY_WEIGHT)
                                return ResultTypes.NoSpace;

                            pData.Holster = (Game.Items.Holster)toItem;
                            pData.Items[slotTo] = fromItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterHolsterUpdate(pData.Info);

                            (pData.Items[slotTo] as Game.Items.Holster).Unwear(pData);
                            pData.Holster?.Wear(pData);

                            if (((pData.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                (pData.Holster?.Items[0] as Game.Items.Weapon)?.Equip(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items), GroupTypes.HolsterItem, 0, Game.Items.Item.ToClientJson(pData.Holster, GroupTypes.HolsterItem));

                            return ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Holster;

                            if (fromItem == null)
                                return ResultTypes.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (toItem != null && !(toItem is Game.Items.Holster))
                                return ResultTypes.Error;

                            if ((fromItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - (toItem?.Weight ?? 0f)) > pData.Bag.Data.MaxWeight)
                                return ResultTypes.NoSpace;

                            pData.Holster = (Game.Items.Holster)toItem;
                            pData.Bag.Items[slotTo] = fromItem;

                            (pData.Bag.Items[slotTo] as Game.Items.Holster).Unwear(pData);
                            pData.Holster?.Wear(pData);

                            if (((pData.Bag.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Bag.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                (pData.Holster?.Items[0] as Game.Items.Weapon)?.Equip(pData);
                            }

                            player.InventoryUpdate(GroupTypes.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag), GroupTypes.HolsterItem, 0, Game.Items.Item.ToClientJson(pData.Holster, GroupTypes.HolsterItem));

                            pData.Bag.Update();
                            MySQL.CharacterHolsterUpdate(pData.Info);

                            return ResultTypes.Success;
                        }
                    },
                }
            },
        };
    }
}
