using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class Inventory
    {
        #region Settings
        /// <summary>Слоты одежды</summary>
        public static Dictionary<Type, int> ClothesSlots = new Dictionary<Type, int>()
        {
            { typeof(Game.Items.Hat), 0 },
            { typeof(Game.Items.Top), 1 },
            { typeof(Game.Items.Under), 2 },
            { typeof(Game.Items.Pants), 3 },
            { typeof(Game.Items.Shoes), 4 },
        };

        /// <summary>Слоты аксессуаров</summary>
        public static Dictionary<Type, int> AccessoriesSlots = new Dictionary<Type, int>()
        {
            { typeof(Game.Items.Glasses), 0 },
            //{ typeof(Game.Items.Mask), 1 },
            { typeof(Game.Items.Earrings), 2 },
            { typeof(Game.Items.Accessory), 3 },
            { typeof(Game.Items.Watches), 4 },
            { typeof(Game.Items.Bracelet), 5 },
            //{ typeof(Game.Items.Ring), 6 },
            { typeof(Game.Items.Gloves), 7 },
        };

        /// <summary>Секции инвентаря</summary>
        public enum Groups
        {
            /// <summary>Карманы</summary>
            Items = 0,
            /// <summary>Сумка</summary>
            Bag = 1,
            /// <summary>Оружие</summary>
            Weapons = 2,
            /// <summary>Кобура (оружие)</summary>
            Holster = 3,
            /// <summary>Одежда</summary>
            Clothes = 4,
            /// <summary>Аксессуары</summary>
            Accessories = 5,
            /// <summary>Предмет сумки</summary>
            BagItem = 6,
            /// <summary>Предмет кобуры</summary>
            HolsterItem = 7,
            /// <summary>Бронежилет</summary>
            Armour = 8,
            /// <summary>Контейнер</summary>
            Container = 9
        }

        /// <summary>Типы результатов</summary>
        public enum Results
        {
            /// <summary>Успешно</summary>
            Success = 0,
            /// <summary>Ошибка</summary>
            Error,
            /// <summary>Действие запрещено в данный момент</summary>
            ActionRestricted,
            /// <summary>Нет места</summary>
            NoSpace,
            /// <summary>В данный слот класть данный предмет нельзя</summary>
            PlaceRestricted,
            /// <summary>Предмет временный</summary>
            TempItem,
            /// <summary>Недавно получено ранение</summary>
            Wounded,
            /// <summary>Недостаточно денег (для обмена)</summary>
            NotEnoughMoney,

            NotEnoughVehicleSlots,
            NotEnoughHouseSlots,
            NotEnoughApartmentsSlots,
            NotEnoughBusinessSlots,
        }

        public static Dictionary<Results, string> ResultsNotifications = new Dictionary<Results, string>()
        {
            { Results.NoSpace, "Inventory::NoSpace" },
            { Results.PlaceRestricted, "Inventory::PlaceRestricted" },
            { Results.Wounded, "Inventory::Wounded" },
            { Results.TempItem, "Inventory::ItemIsTemp" },
        };
        #endregion

        private static Dictionary<Groups, Func<PlayerData, int, Game.Items.Item>> PlayerDataGroups = new Dictionary<Groups, Func<PlayerData, int, Game.Items.Item>>
        {
            { Groups.Items, (pData, slot) => pData.Items.Length <= slot ? null : pData.Items[slot] },

            { Groups.Bag, (pData, slot) => pData.Bag == null ? null : pData.Bag.Items.Length <= slot ? null : pData.Bag.Items[slot] },

            { Groups.Weapons, (pData, slot) =>  pData.Weapons.Length <= slot ? null : pData.Weapons[slot] },

            { Groups.Holster, (pData, slot) =>  pData.Holster == null ? null : pData.Holster.Items[0] },

            { Groups.Armour, (pData, slot) =>  pData.Armour },

            { Groups.BagItem, (pData, slot) =>  pData.Bag },

            { Groups.HolsterItem, (pData, slot) =>  pData.Holster },

            { Groups.Clothes, (pData, slot) =>  pData.Clothes.Length <= slot ? null : pData.Clothes[slot] },

            { Groups.Accessories, (pData, slot) =>  pData.Accessories.Length <= slot ? null : pData.Accessories[slot] },
        };

        public static Game.Items.Item GetPlayerItem(PlayerData pData, Groups group, int slot = 0)
        {
            var res = PlayerDataGroups.GetValueOrDefault(group);

            return res == null ? null : res.Invoke(pData, slot);
        }

        private static Dictionary<Type, Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Results>>> Actions { get; set; } = new Dictionary<Type, Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Results>>>()
        {
            {
                typeof(Game.Items.Weapon),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Results>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;
                            var weapons = pData.Weapons;
                            var holster = pData.Holster;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                int newSlot = 0;

                                if (weapons[0] != null)
                                {
                                    newSlot = 1;

                                    if (weapons[1] != null)
                                    {
                                        newSlot = -1;

                                        if (pData.Holster?.Items[0] != null)
                                            newSlot = 2;
                                    }
                                }

                                if (newSlot == -1)
                                    return Results.Error;

                                return Replace(pData, newSlot != 2 ? Groups.Weapons : Groups.Holster, newSlot, group, slot, -1);
                            }
                            else if (group == Groups.Weapons)
                            {
                                if (weapons[slot].Equiped)
                                {
                                    weapons[slot].Unequip(pData);

                                    player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slot, Game.Items.Item.ToClientJson(weapons[slot], Groups.Weapons));
                                }
                                else
                                {
                                    if (player.Vehicle != null && !weapons[slot].Data.CanUseInVehicle)
                                    {
                                        player.Notify("Weapon::InVehicleRestricted");
                                    }
                                    else
                                    {
                                        int idxToCheck = slot == 0 ? 1 : 0;

                                        if (weapons[idxToCheck]?.Equiped == true)
                                        {
                                            weapons[idxToCheck].Unequip(pData);

                                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, idxToCheck, Game.Items.Item.ToClientJson(weapons[idxToCheck], Groups.Weapons));
                                        }
                                        else if (holster != null && (holster.Items[0] as Game.Items.Weapon)?.Equiped == true)
                                        {
                                            ((Game.Items.Weapon)holster.Items[0]).Unequip(pData);

                                            player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(holster.Items[0], Groups.Holster));
                                        }

                                        weapons[slot].Equip(pData);

                                        player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slot, Game.Items.Item.ToClientJson(weapons[slot], Groups.Weapons));
                                    }
                                }

                                return Results.Success;
                            }
                            else if (group == Groups.Holster)
                            {
                                if (((Game.Items.Weapon)holster.Items[0]).Equiped)
                                {
                                    ((Game.Items.Weapon)holster.Items[0]).Unequip(pData);

                                    player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(holster.Items[0], Groups.Holster));
                                }
                                else
                                {
                                    if (weapons[0]?.Equiped == true)
                                    {
                                        weapons[0].Unequip(pData);

                                        player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, 0, Game.Items.Item.ToClientJson(weapons[0], Groups.Weapons));
                                    }
                                    else if (weapons[1]?.Equiped == true)
                                    {
                                        weapons[1].Unequip(pData);

                                        player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, 1, Game.Items.Item.ToClientJson(weapons[1], Groups.Weapons));
                                    }

                                    ((Game.Items.Weapon)holster.Items[0]).Equip(pData);

                                    player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(holster.Items[0], Groups.Holster));
                                }

                                return Results.Success;
                            }

                            return Results.Error;
                        }
                    },

                    {
                        6,

                        (pData, item, group, slot, args) =>
                        {
                            var items = pData.Items;
                            var player = pData.Player;
                            var weapons = pData.Weapons;
                            var holster = pData.Holster;

                            if (group == Groups.Weapons)
                            {
                                int ammoToFill = weapons[slot].Data.MaxAmmo - weapons[slot].Ammo;

                                if (ammoToFill == 0)
                                    return Results.Success;

                                int ammoIdx = -1;
                                int maxAmmo = 0;

                                for (int i = 0; i < items.Length; i++)
                                {
                                    if (items[i] != null && items[i].ID == weapons[slot].Data.AmmoID && maxAmmo < (items[i] as Game.Items.Ammo).Amount)
                                    {
                                        ammoIdx = i;
                                        maxAmmo = (items[i] as Game.Items.Ammo).Amount;
                                    }
                                }

                                if (ammoIdx == -1)
                                    return Results.Error;

                                return Replace(pData, group, slot, Groups.Items, ammoIdx, ammoToFill);
                            }
                            else if (group == Groups.Holster)
                            {
                                int ammoToFill = ((Game.Items.Weapon)holster.Items[0]).Data.MaxAmmo - ((Game.Items.Weapon)holster.Items[0]).Ammo;

                                if (ammoToFill == 0)
                                    return Results.Success;

                                int ammoIdx = -1;
                                int maxAmmo = 0;

                                for (int i = 0; i < items.Length; i++)
                                {
                                    if (items[i] != null && items[i].ID == ((Game.Items.Weapon)holster.Items[0]).Data.AmmoID && maxAmmo < (items[i] as Game.Items.Ammo).Amount)
                                    {
                                        ammoIdx = i;
                                        maxAmmo = (items[i] as Game.Items.Ammo).Amount;
                                    }
                                }

                                if (ammoIdx == -1)
                                    return Results.Error;

                                return Replace(pData, group, slot, Groups.Items, ammoIdx, ammoToFill);
                            }

                            return Results.Error;
                        }
                    },
                }
            },

            {
                typeof(Game.Items.Clothes),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Results>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                int slotTo;

                                if (AccessoriesSlots.TryGetValue(item.Type, out slotTo))
                                {
                                    return Replace(pData, Groups.Accessories, slotTo, group, slot, -1);
                                }
                                else
                                {
                                    return Replace(pData, Groups.Clothes, ClothesSlots[item.Type], group, slot, -1);
                                }
                            }
                            else if (group == Groups.Clothes || group == Groups.Accessories)
                            {
                                if (item is Game.Items.Clothes.IToggleable tItem)
                                {
                                    tItem.Action(pData);
                                }

                                return Results.Success;
                            }

                            return Results.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Bag),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Results>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items)
                            {
                                return Replace(pData, Groups.BagItem, 0, group, slot, -1);
                            }

                            return Results.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Holster),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Results>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                return Replace(pData, Groups.HolsterItem, 0, group, slot, -1);
                            }

                            return Results.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Armour),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Results>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                return Replace(pData, Groups.Armour, 0, group, slot, -1);
                            }

                            return Results.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.StatusChanger),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Results>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                ((Game.Items.StatusChanger)item).Apply(pData);

                                if (item is Game.Items.IStackable itemStackable)
                                {
                                    if (itemStackable.Amount == 1)
                                    {
                                        item.Delete();

                                        item = null;
                                    }
                                    else
                                        itemStackable.Amount -= 1;
                                }
                                else if (item is Game.Items.IConsumable itemConsumable)
                                {
                                    if (itemConsumable.Amount == 1)
                                    {
                                        item.Delete();

                                        item = null;
                                    }
                                    else
                                        itemConsumable.Amount -= 1;
                                }

                                if (group == Groups.Bag)
                                {
                                    if (item == null)
                                    {
                                        pData.Bag.Items[slot] = null;

                                        pData.Bag.Update();
                                    }
                                    else
                                    {
                                        item.Update();
                                    }
                                }
                                else
                                {
                                    if (item == null)
                                    {
                                        pData.Items[slot] = null;

                                        MySQL.CharacterItemsUpdate(pData.Info);
                                    }
                                    else
                                    {
                                        item.Update();
                                    }
                                }

                                var upd = Game.Items.Item.ToClientJson(item, group);

                                player.TriggerEvent("Inventory::Update", (int)group, slot, upd);

                                return Results.Success;
                            }

                            return Results.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.VehicleKey),

                new Dictionary<int, Func<PlayerData, Item, Groups, int, object[], Results>>()
                {
                    {
                        5,

                        (pData, item, group, slot, args) =>
                        {
                            var vk = (Game.Items.VehicleKey)item;

                            var vInfo = vk.VehicleInfo;

                            if (vInfo == null)
                                return Results.Error;

                            if (vInfo.VehicleData?.Vehicle?.Exists != true)
                                return Results.Error;

                            pData.Player.CreateGPSBlip(vInfo.VehicleData.Vehicle.Position, pData.Player.Dimension, true);

                            return Results.Success;
                        }
                    }
                }
            }
        };

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

                                pData.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.ID, 0, amount);

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);

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

                                pData.Bag.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.ID, 0, amount);

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

                                pData.Bag.Update();
                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, upd2);

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
                                    weapon.Unequip(pData);
                                    pData.Weapons[slotTo].Equip(pData);
                                }
                                else
                                    weapon.Unwear(pData);
                            }
                            else
                            {
                                pData.Weapons[slotTo].UpdateAmmo(pData);

                                pData.Weapons[slotTo].Wear(pData);
                            }

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));

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
                                    weapon.Unequip(pData);
                                    ((Game.Items.Weapon)pData.Holster.Items[0]).Equip(pData);
                                }
                                else
                                    weapon.Unwear(pData);
                            }
                            else
                            {
                                ((Game.Items.Weapon)pData.Holster.Items[0]).UpdateAmmo(pData);

                                ((Game.Items.Weapon)pData.Holster.Items[0]).Wear(pData);
                            }

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom,  Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Clothes[slotTo], Groups.Clothes);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slotTo, upd2);

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Accessories[slotTo], Groups.Accessories);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slotTo, upd2);

                            (pData.Accessories[slotTo] as Game.Items.Clothes)?.Unwear(pData);
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

                            (pData.Items[slotFrom] as Game.Items.Holster)?.Unwear(pData);

                            pData.Holster.Wear(pData);

                            if (pData.Items[slotFrom] is Game.Items.Holster holster && ((Game.Items.Weapon)holster.Items[0])?.Equiped == true)
                            {
                                ((Game.Items.Weapon)holster.Items[0]).Unequip(pData);
                                ((Game.Items.Weapon)pData.Holster.Items[0])?.Equip(pData);
                            }

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items));
                            player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag, Groups.BagItem);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.BagItem, upd2);

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

                            var upd2 = Game.Items.Item.ToClientJson(pData.Armour, Groups.Armour);

                            (pData.Items[slotFrom] as Game.Items.Armour)?.Unwear(pData);
                            pData.Armour.Wear(pData);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Armour, upd2);

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

                                pData.Bag.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.ID, 0, amount);

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, upd2);

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

                                pData.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);

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
                                    weapon.Unequip(pData);
                                    pData.Weapons[slotTo].Equip(pData);
                                }
                                else
                                    weapon.Unwear(pData);
                            }
                            else
                            {
                                pData.Weapons[slotTo].UpdateAmmo(pData);

                                pData.Weapons[slotTo].Wear(pData);
                            }

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));

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
                                    weapon.Unequip(pData);
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Equip(pData);
                                }
                                else
                                    weapon.Unwear(pData);
                            }
                            else
                            {
                                (pData.Holster.Items[0] as Game.Items.Weapon).UpdateAmmo(pData);

                                (pData.Holster.Items[0] as Game.Items.Weapon).Wear(pData);
                            }

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slotTo, upd2);

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slotTo, upd2);

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag));
                            player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Armour, upd2);

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));

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
                                (pData.Holster.Items[0] as Game.Items.Weapon).Equip(pData);
                            }
                            else
                                (pData.Holster.Items[0] as Game.Items.Weapon).Wear(pData);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

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

                                if (fromItem.Equiped)
                                    Sync.WeaponSystem.UpdateAmmo(pData, fromItem, false);

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
                                    pData.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.Data.AmmoID, 0, amount);

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
                            }
                            else
                                return Results.Error;
                            #endregion

                            if (pData.Items[slotTo] is Game.Items.Weapon weapon)
                            {
                                if (weapon.Equiped)
                                {
                                    weapon.Unequip(pData);
                                    pData.Weapons[slotFrom]?.Equip(pData);
                                }
                                else
                                    weapon.Unwear(pData);
                            }
                            else
                            {
                                if (pData.Weapons[slotFrom] != null)
                                {
                                    pData.Weapons[slotFrom].UpdateAmmo(pData);

                                    pData.Weapons[slotFrom].Wear(pData);
                                }
                            }

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons));

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

                                if (fromItem.Equiped)
                                    Sync.WeaponSystem.UpdateAmmo(pData, fromItem, false);

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
                                    pData.Bag.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.Data.AmmoID, 0, amount);

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
                                    weapon.Unequip(pData);
                                    pData.Weapons[slotFrom]?.Equip(pData);
                                }
                                else
                                    weapon.Unwear(pData);
                            }
                            else
                            {
                                if (pData.Weapons[slotFrom] != null)
                                {
                                    pData.Weapons[slotFrom].UpdateAmmo(pData);

                                    pData.Weapons[slotFrom].Wear(pData);
                                }
                            }


                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons));

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

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

                                if (fromItem.Equiped)
                                    Sync.WeaponSystem.UpdateAmmo(pData, fromItem, false);

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
                                    pData.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.Data.AmmoID, 0, amount);

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
                                    weapon.Unequip(pData);
                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                                }
                                else
                                    weapon.Unwear(pData);
                            }
                            else
                            {
                                if ((pData.Holster.Items[0] as Game.Items.Weapon) != null)
                                {
                                    (pData.Holster.Items[0] as Game.Items.Weapon).UpdateAmmo(pData);

                                    (pData.Holster.Items[0] as Game.Items.Weapon).Wear(pData);
                                }
                            }

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

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

                                if (fromItem.Equiped)
                                    Sync.WeaponSystem.UpdateAmmo(pData, fromItem, false);

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
                                    pData.Bag.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.Data.AmmoID, 0, amount);

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
                                    weapon.Unequip(pData);
                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                                }
                                else
                                    weapon.Unwear(pData);
                            }
                            else
                            {
                                if ((pData.Holster.Items[0] as Game.Items.Weapon) != null)
                                {
                                    (pData.Holster.Items[0] as Game.Items.Weapon).UpdateAmmo(pData);

                                    (pData.Holster.Items[0] as Game.Items.Weapon).Wear(pData);
                                }
                            }


                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag));
                            player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Armour, Groups.Armour);

                            (pData.Items[slotTo] as Game.Items.Armour).Unwear(pData);
                            pData.Armour?.Wear(pData);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Armour, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items));

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Armour, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag));

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Clothes[slotFrom], Groups.Clothes);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, upd2);

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Accessories[slotFrom], Groups.Accessories);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, upd2);

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

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag, Groups.BagItem);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            player.TriggerEvent("Inventory::Update", (int)Groups.BagItem, upd1);
                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);

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

                            (pData.Items[slotTo] as Game.Items.Holster).Unwear(pData);
                            pData.Holster?.Wear(pData);

                            if (((pData.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                (pData.Holster?.Items[0] as Game.Items.Weapon)?.Equip(pData);
                            }

                            player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items));
                            player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));

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

                            player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag));
                            player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));

                            pData.Bag.Update();
                            MySQL.CharacterHolsterUpdate(pData.Info);

                            return Results.Success;
                        }
                    },
                }
            },
        };

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
                            item = Game.Items.Items.CreateItem(item.ID, 0, amount);
                        }
                        else
                            pData.Items[slot] = null;
                    }
                    else
                        pData.Items[slot] = null;

                    var upd = Game.Items.Item.ToClientJson(pData.Items[slot], Groups.Items);

                    player.TriggerEvent("Inventory::Update", (int)Groups.Items, slot, upd);

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
                            item = Game.Items.Items.CreateItem(item.ID, 0, amount);
                        }
                        else
                            pData.Bag.Items[slot] = null;
                    }
                    else
                        pData.Bag.Items[slot] = null;

                    var upd = Game.Items.Item.ToClientJson(pData.Bag.Items[slot], Groups.Bag);

                    player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slot, upd);

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

                    if (item.Equiped)
                        Sync.WeaponSystem.UpdateAmmo(pData, item, false);

                    player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slot, Game.Items.Item.ToClientJson(null, Groups.Weapons));

                    if (item.Equiped)
                        item.Unequip(pData, false, false);
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

                    if (item.Equiped)
                        Sync.WeaponSystem.UpdateAmmo(pData, item, false);

                    player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(null, Groups.Holster));

                    if (item.Equiped)
                        item.Unequip(pData, false, false);
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

                    player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slot, Game.Items.Item.ToClientJson(null, Groups.Clothes));

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

                    player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slot, Game.Items.Item.ToClientJson(null, Groups.Accessories));

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

                    player.TriggerEvent("Inventory::Update", (int)Groups.BagItem, Game.Items.Item.ToClientJson(null, Groups.BagItem));

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

                    if ((pData.Holster.Items[0] as Game.Items.Weapon)?.Equiped == true)
                        Sync.WeaponSystem.UpdateAmmo(pData, pData.Holster.Items[0] as Game.Items.Weapon, false);

                    player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(null, Groups.HolsterItem));

                    item.Unwear(pData);

                    (item.Items[0] as Game.Items.Weapon)?.Unequip(pData, false, false);

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

                    player.TriggerEvent("Inventory::Update", (int)Groups.Armour, Game.Items.Item.ToClientJson(null, Groups.Armour));

                    item.Unwear(pData);

                    pData.Armour = null;

                    MySQL.CharacterArmourUpdate(pData.Info);

                    return item;
                }
            },
        };

        #region Replace
        /// <summary>Метод для перемещения/замены предметов в инвентаре игрока</summary>
        /// <param name="pData">PlayerData</param>
        /// <param name="to">Группа, куда переместить</param>
        /// <param name="slotTo">Слот, в который переместить</param>
        /// <param name="from">Группа, откуда переместить</param>
        /// <param name="slotFrom">Слот, откуда переместить</param>
        /// <param name="amount">Кол-во для перемещения (-1 - предмет целиком)</param>
        public static Results Replace(PlayerData pData, Groups to, int slotTo, Groups from, int slotFrom, int amount)
        {
            var player = pData.Player;

            if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                return Results.Error;

            var action = ReplaceActions.GetValueOrDefault(from)?.GetValueOrDefault(to);

            var res = Results.Error;

            if (action != null)
                res = action.Invoke(pData, slotTo, slotFrom, amount);

            var notification = ResultsNotifications.GetValueOrDefault(res);

            if (notification == null)
                return res;

            player.Notify(notification);

            return res;
        }
        #endregion

        #region Action
        /// <summary>Метод для выполнения действия предмета</summary>
        /// <param name="pData">PlayerData</param>
        /// <param name="group">Группа</param>
        /// <param name="slot">Слот</param>
        /// <param name="action">Действие (минимум - 5)</param>
        public static Results Action(PlayerData pData, Groups group, int slot, int action = 5, params object[] args)
        {
            var player = pData.Player;

            var item = GetPlayerItem(pData, group, slot);

            if (item == null)
                return Results.Error;

            var a1 = Actions.GetValueOrDefault(item.Type);

            if (a1 == null)
            {
                a1 = Actions.GetValueOrDefault(item.Type.BaseType);

                if (a1 == null)
                    return Results.Error;
            }

            var a2 = a1.GetValueOrDefault(action);

            if (a2 == null)
                return Results.Error;

            return a2.Invoke(pData, item, group, slot, args);
        }
        #endregion

        #region Drop
        /// <summary>Метод для выбрасывания предмета</summary>
        /// <param name="pData">PlayerData</param>
        /// <param name="group">Группа</param>
        /// <param name="slot">Слот</param>
        /// <param name="amount">Кол-во (минимум - 1, -1 - предмет целиком)</param>
        public static void Drop(PlayerData pData, Groups group, int slot, int amount)
        {
            if (pData == null)
                return;

            var player = pData.Player;

            if (amount < 1 || slot < 0)
                return;

            var action = DropActions.GetValueOrDefault(group);

            if (action == null)
                return;

            Game.Items.Item item = action.Invoke(pData, slot, amount);

            if (item == null)
                return;

            if (item.IsTemp)
            {
                item = null;

                player.Notify("Inventory::TempItemDeleted");

                return;
            }

            if (!pData.CanPlayAnim())
                pData.PlayAnim(Sync.Animations.FastTypes.Putdown);

            Sync.World.AddItemOnGround(pData, item, player.GetFrontOf(0.6f), player.Rotation, player.Dimension);
        }
        #endregion

        public static bool GiveExisting(PlayerData pData, Item item, int amount, bool notifyOnFail = true, bool notifyOnSuccess = true)
        {
            var curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

            int freeIdx = -1, curAmount = 1;

            if (item is Game.Items.IStackable stackable)
            {
                curAmount = stackable.Amount;

                if (amount > curAmount)
                    amount = curAmount;

                if (curWeight + amount * item.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                {
                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / item.BaseWeight);
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

                        if (curItem.ID == item.ID && curItem is Game.Items.IStackable curItemStackable && curItemStackable.Amount + amount <= curItemStackable.MaxAmount)
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

                if (curWeight + item.Weight <= Settings.MAX_INVENTORY_WEIGHT)
                    freeIdx = Array.IndexOf(pData.Items, null);
            }

            if (amount <= 0 || freeIdx < 0)
            {
                if (notifyOnFail)
                    pData.Player.Notify("Inventory::NoSpace");

                return false;
            }

            if (amount == curAmount)
            {
                if (pData.Items[freeIdx] != null)
                {
                    item.Delete();

                    ((Game.Items.IStackable)pData.Items[freeIdx]).Amount += amount;

                    pData.Items[freeIdx].Update();
                }
                else
                {
                    pData.Items[freeIdx] = item;
                }
            }
            else
            {
                ((Game.Items.IStackable)item).Amount -= amount;

                item.Update();

                if (pData.Items[freeIdx] != null)
                {
                    ((Game.Items.IStackable)pData.Items[freeIdx]).Amount += amount;

                    pData.Items[freeIdx].Update();
                }
                else
                {
                    pData.Items[freeIdx] = Game.Items.Items.CreateItem(item.ID, 0, amount, false);
                }
            }

            var upd = Game.Items.Item.ToClientJson(pData.Items[freeIdx], Groups.Items);

            if (notifyOnSuccess)
                pData.Player.TriggerEvent("Item::Added", item.ID, amount);

            pData.Player.TriggerEvent("Inventory::Update", (int)Groups.Items, freeIdx, upd);

            MySQL.CharacterItemsUpdate(pData.Info);

            return true;
        }
    }
}
