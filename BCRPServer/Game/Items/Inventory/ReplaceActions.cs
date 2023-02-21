using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPServer.Game.Items
{
    public partial class Inventory
    {
        private static Dictionary<Groups, Dictionary<Groups, Func<PlayerData, int, int, int, Results>>> ReplaceActions = new Dictionary<Groups, Dictionary<Groups, Func<PlayerData, int, int, int, Results>>>()
        {
            {
                Groups.Items,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                if (toItem.IsTemp || fromItem.IsTemp)
                                    return Results.TempItem;

                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount >= maxStack)
                                    return Results.Error;

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
                                    return Results.TempItem;

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            player.InventoryUpdate(Groups.Items, slotFrom, upd1, Groups.Items, slotTo, upd2);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (fromItem is Game.Items.Bag)
                                return Results.PlaceRestricted;

                            if (fromItem.IsTemp)
                                return Results.TempItem;

                            float curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            float maxWeight = pData.Bag.Data.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount >= maxStack)
                                    return Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Results.NoSpace;
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
                                        return Results.NoSpace;
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
                                    return Results.NoSpace;

                                pData.Items[slotFrom] = toItem;
                                pData.Bag.Items[slotTo] = fromItem;

                                if (fromItem is Game.Items.IUsable fromItemU && fromItemU.InUse)
                                    fromItemU.StopUse(pData, Groups.Bag, slotTo, false);

                                pData.Bag.Update();
                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag);

                            player.InventoryUpdate(Groups.Items, slotFrom, upd1, Groups.Bag, slotTo, upd2);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Weapons,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return Results.Error;

                            var toItem = pData.Weapons[slotTo];

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                    return Results.NoSpace;

                                pData.Weapons[slotTo] = fromWeapon;
                                pData.Items[slotFrom] = toItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                MySQL.CharacterWeaponsUpdate(pData.Info);
                            }
                            else if (fromItem is Game.Items.WeaponComponent wc)
                            {
                                if (toItem == null)
                                    return Results.Error;

                                var wcData = wc.Data;

                                if (!wcData.IsAllowedFor(toItem.Data.Hash))
                                {
                                    pData.Player.Notify("Inventory::WWC");

                                    return Results.Error;
                                }

                                var wcIdx = (int)wcData.Type;

                                if (toItem.Items[wcIdx] != null)
                                {
                                    pData.Player.Notify("Inventory::WHTC");

                                    return Results.Error;
                                }
                                else
                                {
                                    toItem.Items[wcIdx] = wc;

                                    pData.Items[slotFrom] = null;

                                    player.InventoryUpdate(Groups.Weapons, slotTo, toItem.ToClientJson(Groups.Weapons), Groups.Items, slotFrom, Game.Items.Item.ToClientJson(null, Groups.Items));

                                    toItem.UpdateWeaponComponents(pData);

                                    toItem.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);

                                    return Results.Success;
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
                                    return Results.Error;

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
                                return Results.Error;

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

                            player.InventoryUpdate(Groups.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items), Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Holster,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (pData.Holster == null)
                                return Results.Error;

                            var toItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem.IsTemp)
                                return Results.TempItem;

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (fromWeapon.Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                    return Results.PlaceRestricted;

                                if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                    return Results.NoSpace;

                                pData.Holster.Items[0] = fromWeapon;
                                pData.Items[slotFrom] = toItem;

                                pData.Holster.Update();
                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            else if (fromItem is Game.Items.WeaponComponent wc)
                            {
                                if (toItem == null)
                                    return Results.Error;

                                var wcData = wc.Data;

                                if (!wcData.IsAllowedFor(toItem.Data.Hash))
                                {
                                    pData.Player.Notify("Inventory::WWC");

                                    return Results.Error;
                                }

                                var wcIdx = (int)wcData.Type;

                                if (toItem.Items[wcIdx] != null)
                                {
                                    pData.Player.Notify("Inventory::WHTC");

                                    return Results.Error;
                                }
                                else
                                {
                                    toItem.Items[wcIdx] = wc;

                                    pData.Items[slotFrom] = null;

                                    player.InventoryUpdate(Groups.Holster, 2, toItem.ToClientJson(Groups.Holster), Groups.Items, slotFrom, Game.Items.Item.ToClientJson(null, Groups.Items));

                                    toItem.UpdateWeaponComponents(pData);

                                    toItem.Update();

                                    MySQL.CharacterItemsUpdate(pData.Info);

                                    return Results.Success;
                                }
                            }
                            #endregion
                            #region Load
                            else if (fromItem is Game.Items.Ammo fromAmmo && toItem != null && (fromItem.ID == toItem.Data.AmmoID))
                            {
                                var maxAmount = toItem.Data.MaxAmmo;

                                if (toItem.Ammo == maxAmount)
                                    return Results.Error;

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
                                return Results.Error;

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

                            player.InventoryUpdate(Groups.Items, slotFrom,  Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items), Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Clothes,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Clothes.Length)
                                return Results.Error;

                            var toItem = pData.Clothes[slotTo];

                            if (!(fromItem is Game.Items.Clothes fromClothes))
                                return Results.Error;

                            int actualSlot;

                            if (!ClothesSlots.TryGetValue(fromItem.Type, out actualSlot) || slotTo != actualSlot)
                                return Results.Error;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            pData.Clothes[slotTo] = fromClothes;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterClothesUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Clothes[slotTo], Groups.Clothes);

                            player.InventoryUpdate(Groups.Items, slotFrom, upd1, Groups.Clothes, slotTo, upd2);

                            (pData.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                            pData.Clothes[slotTo].Wear(pData);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Accessories,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Accessories.Length)
                                return Results.Error;

                            var toItem = pData.Accessories[slotTo];

                            if (!(fromItem is Game.Items.Clothes fromClothes))
                                return Results.Error;

                            int actualSlot;

                            if (!AccessoriesSlots.TryGetValue(fromItem.Type, out actualSlot) || slotTo != actualSlot)
                                return Results.Error;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            pData.Accessories[slotTo] = fromClothes;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterAccessoriesUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Accessories[slotTo], Groups.Accessories);

                            player.InventoryUpdate(Groups.Items, slotFrom, upd1, Groups.Accessories, slotTo, upd2);

                            (pData.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                            pData.Accessories[slotTo].Wear(pData);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.HolsterItem,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            var toItem = pData.Holster;

                            if (!(fromItem is Game.Items.Holster fromHolster))
                                return Results.Error;

                            if (fromItem.IsTemp || toItem?.Items[0]?.IsTemp == true)
                                return Results.TempItem;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

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

                            player.InventoryUpdate(Groups.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items), Groups.HolsterItem, 0, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.BagItem,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            var toItem = pData.Bag;

                            if (!(fromItem is Game.Items.Bag fromBag))
                                return Results.Error;

                            if (fromItem.IsTemp)
                                return Results.TempItem;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            pData.Bag = fromBag;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterBagUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag, Groups.BagItem);

                            player.InventoryUpdate(Groups.Items, slotFrom, upd1, Groups.BagItem, 0, upd2);

                            (pData.Items[slotFrom] as Game.Items.Bag)?.Unwear(pData);
                            pData.Bag.Wear(pData);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Armour,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            var toItem = pData.Armour;

                            if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                                return Results.Wounded;

                            if (!(fromItem is Game.Items.Armour fromArmour))
                                return Results.Error;

                            if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            pData.Armour = fromArmour;
                            pData.Items[slotFrom] = toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterArmourUpdate(pData.Info);

                            var upd2 = Game.Items.Item.ToClientJson(pData.Armour, Groups.Armour);

                            (pData.Items[slotFrom] as Game.Items.Armour)?.Unwear(pData);
                            pData.Armour.Wear(pData);

                            player.InventoryUpdate(Groups.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items), Groups.Armour, 0, upd2);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Bag,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Bag.Items.Length)
                                return Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Results.Error;

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag);

                            player.InventoryUpdate(Groups.Bag, slotFrom, upd1, Groups.Bag, slotTo, upd2);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem is Game.Items.Bag)
                                return Results.Error;

                            float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Results.NoSpace;
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
                                        return Results.NoSpace;
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
                                    return Results.NoSpace;

                                pData.Bag.Items[slotFrom] = toItem;
                                pData.Items[slotTo] = fromItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                pData.Bag.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            player.InventoryUpdate(Groups.Bag, slotFrom, upd1, Groups.Items, slotTo, upd2);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Weapons,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return Results.Error;

                            var toItem = pData.Weapons[slotTo];

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (toItem != null && pData.Bag.Weight - pData.Bag.BaseWeight + toItem.Weight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                    return Results.NoSpace;

                                pData.Weapons[slotTo] = fromWeapon;
                                pData.Bag.Items[slotFrom] = toItem;

                                MySQL.CharacterWeaponsUpdate(pData.Info);
                                pData.Bag.Update();
                            }
                            else if (fromItem is Game.Items.WeaponComponent wc)
                            {
                                if (toItem == null)
                                    return Results.Error;

                                var wcData = wc.Data;

                                if (!wcData.IsAllowedFor(toItem.Data.Hash))
                                {
                                    pData.Player.Notify("Inventory::WWC");

                                    return Results.Error;
                                }

                                var wcIdx = (int)wcData.Type;

                                if (toItem.Items[wcIdx] != null)
                                {
                                    pData.Player.Notify("Inventory::WHTC");

                                    return Results.Error;
                                }
                                else
                                {
                                    toItem.Items[wcIdx] = wc;

                                    pData.Bag.Items[slotFrom] = null;

                                    player.InventoryUpdate(Groups.Weapons, slotTo, toItem.ToClientJson(Groups.Weapons), Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(null, Groups.Bag));

                                    toItem.UpdateWeaponComponents(pData);

                                    toItem.Update();

                                    pData.Bag.Update();

                                    return Results.Success;
                                }
                            }
                            #endregion
                            #region Load
                            else if (fromItem is Game.Items.Ammo fromAmmo && toItem != null && fromItem.ID == toItem.Data.AmmoID)
                            {
                                var maxAmount = toItem.Data.MaxAmmo;

                                if (toItem.Ammo == maxAmount)
                                    return Results.Error;

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
                                return Results.Error;

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

                            player.InventoryUpdate(Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag), Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Holster,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (pData.Holster == null)
                                return Results.Error;

                            var toItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (fromWeapon.Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                    return Results.PlaceRestricted;

                                if (toItem != null && pData.Bag.Weight - pData.Bag.BaseWeight + toItem.Weight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                    return Results.NoSpace;

                                pData.Holster.Items[0] = fromWeapon;
                                pData.Bag.Items[slotFrom] = toItem;

                                pData.Holster.Update();
                                pData.Bag.Update();
                            }
                            else if (fromItem is Game.Items.WeaponComponent wc)
                            {
                                if (toItem == null)
                                    return Results.Error;

                                var wcData = wc.Data;

                                if (!wcData.IsAllowedFor(toItem.Data.Hash))
                                {
                                    pData.Player.Notify("Inventory::WWC");

                                    return Results.Error;
                                }

                                var wcIdx = (int)wcData.Type;

                                if (toItem.Items[wcIdx] != null)
                                {
                                    pData.Player.Notify("Inventory::WHTC");

                                    return Results.Error;
                                }
                                else
                                {
                                    toItem.Items[wcIdx] = wc;

                                    pData.Bag.Items[slotFrom] = null;

                                    player.InventoryUpdate(Groups.Holster, 2, toItem.ToClientJson(Groups.Holster), Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(null, Groups.Bag));

                                    toItem.UpdateWeaponComponents(pData);

                                    toItem.Update();

                                    pData.Bag.Update();

                                    return Results.Success;
                                }
                            }
                            #endregion
                            #region Load
                            else if (fromItem is Game.Items.Ammo fromAmmo && toItem != null && fromItem.ID == toItem.Data.AmmoID)
                            {
                                var maxAmount = toItem.Data.MaxAmmo;

                                if (toItem.Ammo == maxAmount)
                                    return Results.Error;

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
                                return Results.Error;

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

                            player.InventoryUpdate(Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag), Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Clothes,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Clothes.Length)
                                return Results.Error;

                            var toItem = pData.Clothes[slotTo];

                            if (!(fromItem is Game.Items.Clothes fromClothes))
                                return Results.Error;

                            int actualSlot;

                            if (!ClothesSlots.TryGetValue(fromItem.Type, out actualSlot) || slotTo != actualSlot)
                                return Results.Error;

                            if (toItem != null && toItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            pData.Clothes[slotTo] = fromClothes;
                            pData.Bag.Items[slotFrom] = toItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Clothes[slotTo], Groups.Clothes);

                            player.InventoryUpdate(Groups.Bag, slotFrom, upd1, Groups.Clothes, slotTo, upd2);

                            (pData.Bag.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                            pData.Clothes[slotTo].Wear(pData);

                            pData.Bag.Update();
                            MySQL.CharacterClothesUpdate(pData.Info);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Accessories,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Accessories.Length)
                                return Results.Error;

                            var toItem = pData.Accessories[slotTo];

                            if (!(fromItem is Game.Items.Clothes fromClothes))
                                return Results.Error;

                            int actualSlot;

                            if (!AccessoriesSlots.TryGetValue(fromItem.Type, out actualSlot) || slotTo != actualSlot)
                                return Results.Error;

                            if (toItem != null && toItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            pData.Accessories[slotTo] = fromClothes;
                            pData.Bag.Items[slotFrom] = toItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Accessories[slotTo], Groups.Accessories);

                            player.InventoryUpdate(Groups.Bag, slotFrom, upd1, Groups.Accessories, slotTo, upd2);

                            (pData.Bag.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                            pData.Accessories[slotTo].Wear(pData);

                            pData.Bag.Update();
                            MySQL.CharacterAccessoriesUpdate(pData.Info);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.HolsterItem,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            var toItem = pData.Holster;

                            if (!(fromItem is Game.Items.Holster fromHolster))
                                return Results.Error;

                            if (toItem != null && toItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            pData.Holster = fromHolster;
                            pData.Bag.Items[slotFrom] = toItem;

                            (pData.Bag.Items[slotFrom] as Game.Items.Holster)?.Unwear(pData);
                            pData.Holster.Wear(pData);

                            if (pData.Bag.Items[slotFrom] != null && ((pData.Bag.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Bag.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                            }

                            player.InventoryUpdate(Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag), Groups.HolsterItem, 0, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));

                            pData.Bag.Update();
                            MySQL.CharacterHolsterUpdate(pData.Info);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Armour,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                                return Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                                return Results.Wounded;

                            var toItem = pData.Armour;

                            if (!(fromItem is Game.Items.Armour fromArmour))
                                return Results.Error;

                            if (toItem != null && toItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            pData.Armour = fromArmour;
                            pData.Bag.Items[slotFrom] = toItem;

                            var upd2 = Game.Items.Item.ToClientJson(pData.Armour, Groups.Armour);

                            (pData.Bag.Items[slotFrom] as Game.Items.Armour)?.Unwear(pData);
                            pData.Armour.Wear(pData);

                            player.InventoryUpdate(Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag), Groups.Armour, 0, upd2);

                            pData.Bag.Update();
                            MySQL.CharacterArmourUpdate(pData.Info);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Weapons,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Weapons,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Weapons.Length)
                                return Results.Error;

                            var fromItem = pData.Weapons[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return Results.Error;

                            var toItem = pData.Weapons[slotTo];

                            pData.Weapons[slotTo] = fromItem;
                            pData.Weapons[slotFrom] = toItem;

                            if (pData.Weapons[slotFrom]?.Equiped == true)
                            {
                                pData.Weapons[slotFrom].Unequip(pData);

                                pData.Weapons[slotTo].Equip(pData);
                            }

                            player.InventoryUpdate(Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons), Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Holster,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Weapons.Length)
                                return Results.Error;

                            var fromItem = pData.Weapons[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (pData.Holster == null)
                                return Results.Error;

                            var toItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem.Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                return Results.PlaceRestricted;

                            if (fromItem.IsTemp)
                                return Results.TempItem;

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

                            player.InventoryUpdate(Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons), Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

                            pData.Holster.Update();

                            MySQL.CharacterWeaponsUpdate(pData.Info);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Weapons.Length)
                                return Results.Error;

                            var fromItem = pData.Weapons[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            bool extractToExisting = toItem is Game.Items.Ammo && toItem.ID == fromItem.Data.AmmoID;

                            #region Extract
                            if (amount != -1 || extractToExisting) // extract ammo from weapon
                            {
                                if (fromItem.IsTemp)
                                    return Results.TempItem;

                                if (fromItem.Ammo == 0 || fromItem.Data.AmmoID == null)
                                    return Results.Error;

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
                                            return Results.NoSpace;
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
                                            return Results.NoSpace;
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
                                    return Results.NoSpace;

                                pData.Items[slotTo] = fromItem;
                                pData.Weapons[slotFrom] = (Game.Items.Weapon)toItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                MySQL.CharacterWeaponsUpdate(pData.Info);
                            }
                            else
                                return Results.Error;
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

                            player.InventoryUpdate(Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items), Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Weapons.Length)
                                return Results.Error;

                            var fromItem = pData.Weapons[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (fromItem.IsTemp)
                                return Results.TempItem;

                            bool extractToExisting = toItem is Game.Items.Ammo && toItem.ID == fromItem.Data.AmmoID;

                            #region Extract
                            if (amount != -1 || extractToExisting)
                            {
                                if (fromItem.Ammo == 0 || fromItem.Data.AmmoID == null)
                                    return Results.Error;

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
                                            return Results.NoSpace;
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
                                            return Results.NoSpace;
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
                                    return Results.NoSpace;

                                pData.Bag.Items[slotTo] = fromItem;
                                pData.Weapons[slotFrom] = (Game.Items.Weapon)toItem;

                                pData.Bag.Update();
                                MySQL.CharacterWeaponsUpdate(pData.Info);
                            }
                            else
                                return Results.Error;
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


                            player.InventoryUpdate(Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag), Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons));

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Holster,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Weapons,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Holster == null)
                                return Results.Error;

                            var fromItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return Results.PlaceRestricted;

                            var toItem = pData.Weapons[slotTo];

                            if (toItem != null && toItem.Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                return Results.PlaceRestricted;

                            pData.Holster.Items[0] = toItem;
                            pData.Weapons[slotTo] = fromItem;

                            if (pData.Weapons[slotTo].Equiped)
                            {
                                pData.Weapons[slotTo].Unequip(pData);
                                (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                            }
                            else
                                pData.Weapons[slotTo].Unwear(pData);

                            player.InventoryUpdate(Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons), Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

                            pData.Holster.Update();

                            MySQL.CharacterWeaponsUpdate(pData.Info);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Holster == null)
                                return Results.Error;

                            var fromItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            bool extractToExisting = toItem is Game.Items.Ammo && toItem.ID == fromItem.Data.AmmoID;

                            #region Extract
                            if (amount != -1 || extractToExisting)
                            {
                                if (fromItem.Ammo == 0 || fromItem.Data.AmmoID == null)
                                    return Results.Error;

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
                                            return Results.NoSpace;
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
                                            return Results.NoSpace;
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
                                    return Results.NoSpace;

                                pData.Items[slotTo] = fromItem;
                                pData.Holster.Items[0] = (Game.Items.Weapon)toItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                pData.Holster.Update();
                            }
                            else
                                return Results.Error;
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

                            player.InventoryUpdate(Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items), Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Holster == null)
                                return Results.Error;

                            var fromItem = (Game.Items.Weapon)pData.Holster.Items[0];

                            if (fromItem == null)
                                return Results.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            bool extractToExisting = toItem is Game.Items.Ammo && toItem.ID == fromItem.Data.AmmoID;

                            #region Extract
                            if (amount != -1 || extractToExisting)
                            {
                                if (fromItem.Ammo == 0 || fromItem.Data.AmmoID == null)
                                    return Results.Error;

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
                                            return Results.NoSpace;
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
                                            return Results.NoSpace;
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
                                    return Results.NoSpace;

                                pData.Bag.Items[slotTo] = fromItem;
                                pData.Holster.Items[0] = (Game.Items.Weapon)toItem;

                                pData.Bag.Update();
                                pData.Holster.Update();
                            }
                            else
                                return Results.Error;
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


                            player.InventoryUpdate(Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag), Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Armour,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Armour;

                            if (fromItem == null)
                                return Results.Error;

                            if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                                return Results.Wounded;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && (!(toItem is Game.Items.Armour)))
                                return Results.Error;

                            if ((pData.Items.Sum(x => x?.Weight ?? 0f) + fromItem.Weight - (toItem?.Weight ?? 0)) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            pData.Armour = (Game.Items.Armour)toItem;
                            pData.Items[slotTo] = fromItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterArmourUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Armour, Groups.Armour);

                            (pData.Items[slotTo] as Game.Items.Armour).Unwear(pData);
                            pData.Armour?.Wear(pData);

                            player.InventoryUpdate(Groups.Armour, 0, upd1, Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Armour;

                            if (fromItem == null)
                                return Results.Error;

                            if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                                return Results.Wounded;

                            if (fromItem.IsTemp)
                                return Results.TempItem;

                            if (pData.Bag == null)
                                return Results.Error;

                            if (slotTo >= pData.Bag.Items.Length)
                                return Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (toItem != null && (!(toItem is Game.Items.Armour)))
                                return Results.Error;

                            if ((pData.Bag.Weight - pData.Bag.BaseWeight + fromItem.Weight - (toItem?.Weight ?? 0f)) > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            pData.Armour = (Game.Items.Armour)toItem;
                            pData.Bag.Items[slotTo] = fromItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Armour, Groups.Armour);

                            (pData.Bag.Items[slotTo] as Game.Items.Armour).Unwear(pData);
                            pData.Armour?.Wear(pData);

                            player.InventoryUpdate(Groups.Armour, 0, upd1, Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag));

                            pData.Bag.Update();
                            MySQL.CharacterArmourUpdate(pData.Info);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Clothes,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Clothes.Length)
                                return Results.Error;

                            var fromItem = pData.Clothes[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && toItem.Type != fromItem.Type)
                                return Results.Error;

                            if ((pData.Items.Sum(x => x?.Weight ?? 0f) + fromItem.Weight - (toItem?.Weight ?? 0f)) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            pData.Items[slotTo] = fromItem;
                            pData.Clothes[slotFrom] = (Game.Items.Clothes)toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterClothesUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Clothes[slotFrom], Groups.Clothes);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            player.InventoryUpdate(Groups.Clothes, slotFrom, upd1, Groups.Items, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                            pData.Clothes[slotFrom]?.Wear(pData);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Clothes.Length)
                                return Results.Error;

                            var fromItem = pData.Clothes[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (fromItem.IsTemp)
                                return Results.TempItem;

                            if (pData.Bag == null)
                                return Results.Error;

                            if (slotTo >= pData.Bag.Items.Length)
                                return Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (toItem != null && toItem.Type != fromItem.Type)
                                return Results.Error;

                            var curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            var maxWeight = pData.Bag.Data.MaxWeight;

                            if ((curWeight + fromItem.Weight - (toItem?.Weight ?? 0f)) > maxWeight)
                                return Results.NoSpace;

                            pData.Bag.Items[slotTo] = fromItem;
                            pData.Clothes[slotFrom] = (Game.Items.Clothes)toItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Clothes[slotFrom], Groups.Clothes);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag);

                            player.InventoryUpdate(Groups.Clothes, slotFrom, upd1, Groups.Bag, slotTo, upd2);

                            (pData.Bag.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                            pData.Clothes[slotFrom]?.Wear(pData);

                            pData.Bag.Update();
                            MySQL.CharacterClothesUpdate(pData.Info);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Accessories,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Accessories.Length)
                                return Results.Error;

                            var fromItem = pData.Accessories[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && toItem.Type != fromItem.Type)
                                return Results.Error;

                            if ((pData.Items.Sum(x => x?.Weight ?? 0f) + fromItem.Weight - (toItem?.Weight ?? 0f)) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            pData.Items[slotTo] = fromItem;
                            pData.Accessories[slotFrom] = (Game.Items.Clothes)toItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterAccessoriesUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Accessories[slotFrom], Groups.Accessories);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            player.InventoryUpdate(Groups.Accessories, slotFrom, upd1, Groups.Items, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                            (pData.Accessories[slotFrom])?.Wear(pData);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Accessories.Length)
                                return Results.Error;

                            var fromItem = pData.Accessories[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (fromItem.IsTemp)
                                return Results.TempItem;

                            if (pData.Bag == null)
                                return Results.Error;

                            if (slotTo >= pData.Bag.Items.Length)
                                return Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (toItem != null && toItem.Type != fromItem.Type)
                                return Results.Error;

                            var curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            var maxWeight = pData.Bag.Data.MaxWeight;

                            if ((curWeight + fromItem.Weight - (toItem?.Weight ?? 0f)) > maxWeight)
                                return Results.NoSpace;

                            pData.Bag.Items[slotTo] = fromItem;
                            pData.Accessories[slotFrom] = (Game.Items.Clothes)toItem;

                            var upd1 = Game.Items.Item.ToClientJson(pData.Accessories[slotFrom], Groups.Accessories);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag);

                            player.InventoryUpdate(Groups.Accessories, slotFrom, upd1, Groups.Bag, slotTo, upd2);

                            (pData.Bag.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                            pData.Accessories[slotFrom]?.Wear(pData);

                            pData.Bag.Update();
                            MySQL.CharacterAccessoriesUpdate(pData.Info);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.BagItem,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Bag;

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && !(toItem is Game.Items.Bag))
                                return Results.Error;

                            if ((fromItem.Weight + pData.Items.Sum(x => x?.Weight ?? 0f) - (toItem?.Weight ?? 0f)) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            pData.Bag = (Game.Items.Bag)toItem;
                            pData.Items[slotTo] = fromItem;

                            MySQL.CharacterItemsUpdate(pData.Info);
                            MySQL.CharacterBagUpdate(pData.Info);

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag, Groups.BagItem);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            player.InventoryUpdate(Groups.BagItem, 0, upd1, Groups.Items, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.Bag).Unwear(pData);
                            pData.Bag?.Wear(pData);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.HolsterItem,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Results>>()
                {
                    {
                        Groups.Items,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Holster;

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Results.Error;

                            var toItem = pData.Items[slotTo];

                            if (toItem != null && !(toItem is Game.Items.Holster))
                                return Results.Error;

                            if ((fromItem.Weight + pData.Items.Sum(x => x?.Weight ?? 0f) - (toItem?.Weight ?? 0f)) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

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

                            player.InventoryUpdate(Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items), Groups.HolsterItem, 0, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Holster;

                            if (fromItem == null)
                                return Results.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (toItem != null && !(toItem is Game.Items.Holster))
                                return Results.Error;

                            if ((fromItem.Weight + pData.Bag.Weight - pData.Bag.BaseWeight - (toItem?.Weight ?? 0f)) > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            pData.Holster = (Game.Items.Holster)toItem;
                            pData.Bag.Items[slotTo] = fromItem;

                            (pData.Bag.Items[slotTo] as Game.Items.Holster).Unwear(pData);
                            pData.Holster?.Wear(pData);

                            if (((pData.Bag.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Bag.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                (pData.Holster?.Items[0] as Game.Items.Weapon)?.Equip(pData);
                            }

                            player.InventoryUpdate(Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag), Groups.HolsterItem, 0, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));

                            pData.Bag.Update();
                            MySQL.CharacterHolsterUpdate(pData.Info);

                            return Results.Success;
                        }
                    },
                }
            },
        };
    }
}
