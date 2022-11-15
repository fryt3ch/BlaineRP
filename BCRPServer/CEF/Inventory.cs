﻿using BCRPServer.Game.Bank;
using BCRPServer.Game.Items;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.CEF
{
    /*
        4.000 LINES! BE CAREFUL! IT SLOWS IDE
    */

    public class Inventory : Script
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

        private static Dictionary<Type, Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Task<Results>>>> Actions { get; set; } = new Dictionary<Type, Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Task<Results>>>>()
        {
            {
                typeof(Game.Items.Weapon),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Task<Results>>>()
                {
                    {
                        5,

                        async (pData, item, group, slot, args) =>
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

                                return await Replace(pData, newSlot != 2 ? Groups.Weapons : Groups.Holster, newSlot, group, slot, -1);
                            }
                            else if (group == Groups.Weapons)
                            {
                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (player?.Exists != true)
                                        return;

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

                                            return;
                                        }

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
                                });

                                return Results.Success;
                            }
                            else if (group == Groups.Holster)
                            {
                                await NAPI.Task.RunAsync(() =>
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
                                });

                                return Results.Success;
                            }

                            return Results.Error;
                        }
                    },

                    {
                        6,

                        async (pData, item, group, slot, args) =>
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

                                return await Replace(pData, group, slot, Groups.Items, ammoIdx, ammoToFill);
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

                                return await Replace(pData, group, slot, Groups.Items, ammoIdx, ammoToFill);
                            }

                            return Results.Error;
                        }
                    },
                }
            },

            {
                typeof(Game.Items.Clothes),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Task<Results>>>()
                {
                    {
                        5,

                        async (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                int slotTo;

                                if (AccessoriesSlots.TryGetValue(item.Type, out slotTo))
                                {
                                    return await Replace(pData, Groups.Accessories, slotTo, group, slot, -1);
                                }
                                else
                                {
                                    return await Replace(pData, Groups.Clothes, ClothesSlots[item.Type], group, slot, -1);
                                }
                            }
                            else if (group == Groups.Clothes || group == Groups.Accessories)
                            {
                                if ((item is Game.Items.Clothes.IToggleable tItem))
                                {
                                    NAPI.Task.Run(() =>
                                    {
                                        if (player?.Exists != true)
                                            return;

                                        tItem?.Action(pData);
                                    });
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

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Task<Results>>>()
                {
                    {
                        5,

                        async (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items)
                            {
                                return await Replace(pData, Groups.BagItem, 0, group, slot, -1);
                            }

                            return Results.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Holster),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Task<Results>>>()
                {
                    {
                        5,

                        async (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                return  await Replace(pData, Groups.HolsterItem, 0, group, slot, -1);
                            }

                            return Results.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.Armour),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Task<Results>>>()
                {
                    {
                        5,

                        async (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                return  await Replace(pData, Groups.Armour, 0, group, slot, -1);
                            }

                            return Results.Error;
                        }
                    }
                }
            },

            {
                typeof(Game.Items.StatusChanger),

                new Dictionary<int, Func<PlayerData, Game.Items.Item, Groups, int, object[], Task<Results>>>()
                {
                    {
                        5,

                        async (pData, item, group, slot, args) =>
                        {
                            var player = pData.Player;

                            if (group == Groups.Items || group == Groups.Bag)
                            {
                                await NAPI.Task.RunAsync(() =>
                                {
                                    if (player?.Exists != true)
                                        return;

                                    ((Game.Items.StatusChanger)item).Apply(pData);
                                });

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

                                        MySQL.UpdatePlayerInventory(pData, true);
                                    }
                                    else
                                    {
                                        item.Update();
                                    }
                                }

                                var upd = Game.Items.Item.ToClientJson(item, group);

                                NAPI.Task.RunSafe(() =>
                                {
                                    if (player?.Exists != true)
                                        return;

                                    player.TriggerEvent("Inventory::Update", (int)group, slot, upd);
                                });

                                return Results.Success;
                            }

                            return Results.Error;
                        }
                    }
                }
            },
        };

        private static Dictionary<Groups, Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>> ReplaceActions = new Dictionary<Groups, Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>>()
        {
            {
                Groups.Items,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                                pData.Items[slotTo] = await Game.Items.Items.CreateItem(fromItem.ID, 0, amount);
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);
                            });

                            MySQL.UpdatePlayerInventory(pData, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasDeleted = false;
                            bool wasCreated = false;

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

                                        wasDeleted = true;
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

                                pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(fromItem.ID, 0, amount);

                                wasCreated = true;
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

                                wasDeleted = true; wasCreated = true;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag);

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, upd2);
                            });

                            if (wasDeleted)
                                MySQL.UpdatePlayerInventory(pData, true);

                            if (wasCreated)
                                pData.Bag.Update();

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Weapons,

                        async (pData, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var fromItem = pData.Items.Length <= slotFrom ? null : pData.Items[slotFrom];

                            if (fromItem == null)
                                return Results.Error;

                            if (slotTo >= pData.Weapons.Length)
                                return Results.Error;

                            var toItem = pData.Weapons[slotTo];

                            bool wasDeleted = false;
                            bool wasReplaced = false;

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (toItem != null && (pData.Items.Sum(x => x?.Weight ?? 0f) + toItem.Weight - fromItem.Weight) > Settings.MAX_INVENTORY_WEIGHT)
                                    return Results.NoSpace;

                                pData.Weapons[slotTo] = fromWeapon;
                                pData.Items[slotFrom] = toItem;

                                wasReplaced = true;
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
                                        wasDeleted = true;

                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Items[slotFrom] = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            else
                                return Results.Error;

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

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
                            });

                            if (wasDeleted)
                                MySQL.UpdatePlayerInventory(pData, true);
                            else if (wasReplaced)
                                MySQL.UpdatePlayerInventory(pData, true, false, false, false, false, true, false);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Holster,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasDeleted = false;
                            bool wasReplaced = false;

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

                                wasReplaced = true;
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
                                        wasDeleted = true;

                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Items[slotFrom] = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            else
                                return Results.Error;

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

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
                            });

                            if (wasDeleted || wasReplaced)
                                MySQL.UpdatePlayerInventory(pData, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Clothes,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slotTo, upd2);

                                (pData.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                                pData.Clothes[slotTo].Wear(pData);
                            });

                            MySQL.UpdatePlayerInventory(pData, true, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Accessories,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slotTo, upd2);

                                (pData.Accessories[slotTo] as Game.Items.Clothes)?.Unwear(pData);
                                pData.Accessories[slotTo].Wear(pData);
                            });

                            MySQL.UpdatePlayerInventory(pData, true, false, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.HolsterItem,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                (pData.Items[slotFrom] as Game.Items.Holster)?.Unwear(pData);

                                pData.Holster.Wear(pData);

                                if (pData.Items[slotFrom] is Game.Items.Holster holster && ((Game.Items.Weapon)holster.Items[0])?.Equiped == true)
                                {
                                    ((Game.Items.Weapon)holster.Items[0]).Unequip(pData);
                                    ((Game.Items.Weapon)pData.Holster.Items[0])?.Equip(pData);
                                }

                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items));
                                player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));
                            });

                            MySQL.UpdatePlayerInventory(pData, true, false, false, false, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.BagItem,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.BagItem, upd2);

                                (pData.Items[slotFrom] as Game.Items.Bag)?.Unwear(pData);
                                pData.Bag.Wear(pData);
                            });

                            MySQL.UpdatePlayerInventory(pData, true, false, false, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Armour,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                (pData.Items[slotFrom] as Game.Items.Armour)?.Unwear(pData);
                                pData.Armour.Wear(pData);

                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotFrom, Game.Items.Item.ToClientJson(pData.Items[slotFrom], Groups.Items));
                                player.TriggerEvent("Inventory::Update", (int)Groups.Armour, upd2);
                            });

                            MySQL.UpdatePlayerInventory(pData, true, false, false, false, false, false, true);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Bag,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Bag,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasCreated = false;
                            bool wasDeleted = false;

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
                                        wasDeleted = true;

                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Bag.Items[slotFrom] = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount) // split to new item
                            {
                                wasCreated = true;

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(fromItem.ID, 0, amount);
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                pData.Bag.Items[slotFrom] = toItem;
                                pData.Bag.Items[slotTo] = fromItem;

                                wasCreated = true;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag);

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, upd2);
                            });

                            if (wasCreated || wasDeleted)
                                pData.Bag.Update();

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasDeleted = false, wasCreated = false;

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

                                        wasDeleted = true;
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

                                pData.Items[slotTo] = await Game.Items.Items.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                wasCreated = true;
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

                                wasDeleted = true; wasCreated = true;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items);

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);
                            });

                            if (wasCreated)
                                MySQL.UpdatePlayerInventory(pData, true);

                            if (wasDeleted)
                                pData.Bag.Update();

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Weapons,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasDeleted = false;
                            bool wasReplaced = false;

                            #region Replace
                            if (fromItem is Game.Items.Weapon fromWeapon)
                            {
                                if (toItem != null && pData.Bag.Weight - pData.Bag.BaseWeight + toItem.Weight - fromItem.Weight > pData.Bag.Data.MaxWeight)
                                    return Results.NoSpace;

                                pData.Weapons[slotTo] = fromWeapon;
                                pData.Bag.Items[slotFrom] = toItem;

                                wasReplaced = true;
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
                                        wasDeleted = true;

                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Bag.Items[slotFrom] = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            else
                                return Results.Error;

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

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
                            });

                            if (wasDeleted)
                                pData.Bag.Update();
                            else if (wasReplaced)
                            {
                                pData.Bag.Update();
                                MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);
                            }

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Holster,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasDeleted = false;
                            bool wasReplaced = false;

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

                                wasReplaced = true;
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
                                        wasDeleted = true;

                                        fromItem.Delete();

                                        fromItem = null;

                                        pData.Bag.Items[slotFrom] = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            else
                                return Results.Error;

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

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
                            });

                            if (wasDeleted || wasReplaced)
                                pData.Bag.Update();

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Clothes,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slotTo, upd2);

                                (pData.Bag.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                                pData.Clothes[slotTo].Wear(pData);
                            });

                            pData.Bag.Update();
                            MySQL.UpdatePlayerInventory(pData, false, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Accessories,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slotTo, upd2);

                                (pData.Bag.Items[slotFrom] as Game.Items.Clothes)?.Unwear(pData);
                                pData.Accessories[slotTo].Wear(pData);
                            });

                            pData.Bag.Update();
                            MySQL.UpdatePlayerInventory(pData, false, false, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.HolsterItem,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                (pData.Bag.Items[slotFrom] as Game.Items.Holster)?.Unwear(pData);
                                pData.Holster.Wear(pData);

                                if (pData.Bag.Items[slotFrom] != null && ((pData.Bag.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                                {
                                    ((pData.Bag.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                                }

                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag));
                                player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));
                            });

                            pData.Bag.Update();
                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Armour,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                (pData.Bag.Items[slotFrom] as Game.Items.Armour)?.Unwear(pData);
                                pData.Armour.Wear(pData);

                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotFrom, Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Groups.Bag));
                                player.TriggerEvent("Inventory::Update", (int)Groups.Armour, upd2);
                            });

                            pData.Bag.Update();
                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Weapons,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Weapons,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                if (pData.Weapons[slotFrom]?.Equiped == true)
                                {
                                    pData.Weapons[slotFrom].Unequip(pData);
                                    pData.Weapons[slotTo].Equip(pData);
                                }

                                player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons));
                                player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));
                            });

                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Holster,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                if (pData.Weapons[slotFrom]?.Equiped == true)
                                {
                                    pData.Weapons[slotFrom].Unequip(pData);
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Equip(pData);
                                }
                                else
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Wear(pData);

                                player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotFrom, Game.Items.Item.ToClientJson(pData.Weapons[slotFrom], Groups.Weapons));
                                player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));
                            });

                            pData.Holster.Update();

                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasCreated = false;
                            bool wasReplaced = false;

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
                                    pData.Items[slotTo] = await Game.Items.Items.CreateItem(fromItem.Data.AmmoID, 0, amount);

                                    fromItem.Update();

                                    wasCreated = true;
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

                                wasReplaced = true;
                            }
                            #endregion

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

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
                            });

                            if (wasCreated)
                                MySQL.UpdatePlayerInventory(pData, true);
                            else if (wasReplaced)
                                MySQL.UpdatePlayerInventory(pData, true, false, false, false, false, true, false);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasCreated = false;
                            bool wasReplaced = false;

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
                                    pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(fromItem.Data.AmmoID, 0, amount);

                                    fromItem.Update();

                                    wasCreated = true;
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

                                wasReplaced = true;
                                wasCreated = true;
                            }
                            #endregion

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

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
                            });

                            if (wasCreated)
                                pData.Bag.Update();

                            if (wasReplaced)
                                MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Holster,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Weapons,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                if (pData.Weapons[slotTo].Equiped)
                                {
                                    pData.Weapons[slotTo].Unequip(pData);
                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(pData);
                                }
                                else
                                    pData.Weapons[slotTo].Unwear(pData);

                                player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slotTo, Game.Items.Item.ToClientJson(pData.Weapons[slotTo], Groups.Weapons));
                                player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(pData.Holster.Items[0], Groups.Holster));
                            });

                            pData.Holster.Update();

                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasCreated = false;
                            bool wasReplaced = false;

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
                                    pData.Items[slotTo] = await Game.Items.Items.CreateItem(fromItem.Data.AmmoID, 0, amount);

                                    wasCreated = true;

                                    fromItem.Update();
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

                                wasReplaced = true;
                            }
                            #endregion

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

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
                            });

                            if (wasReplaced)
                            {
                                pData.Holster.Update();

                                MySQL.UpdatePlayerInventory(pData, true, false, false, false, false, false, false);
                            }

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            bool wasCreated = false;
                            bool wasReplaced = false;

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
                                    pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(fromItem.Data.AmmoID, 0, amount);

                                    wasCreated = true;

                                    fromItem.Update();
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

                                wasReplaced = true;
                                wasCreated = true;
                            }
                            #endregion

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

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
                            });

                            if (wasReplaced || wasCreated)
                            {
                                pData.Holster.Update();
                                pData.Bag.Update();
                            }

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Armour,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                (pData.Items[slotTo] as Game.Items.Armour).Unwear(pData);
                                pData.Armour?.Wear(pData);

                                player.TriggerEvent("Inventory::Update", (int)Groups.Armour, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items));
                            });

                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                (pData.Bag.Items[slotTo] as Game.Items.Armour).Unwear(pData);
                                pData.Armour?.Wear(pData);

                                player.TriggerEvent("Inventory::Update", (int)Groups.Armour, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag));
                            });

                            pData.Bag.Update();
                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Clothes,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);

                                (pData.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                                pData.Clothes[slotFrom]?.Wear(pData);
                            });

                            MySQL.UpdatePlayerInventory(pData, true, true, false, false, false, false, false);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, upd2);

                                (pData.Bag.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                                pData.Clothes[slotFrom]?.Wear(pData);
                            });

                            pData.Bag.Update();
                            MySQL.UpdatePlayerInventory(pData, false, true, false, false, false, false, false);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.Accessories,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);

                                (pData.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                                (pData.Accessories[slotFrom])?.Wear(pData);
                            });

                            MySQL.UpdatePlayerInventory(pData, true, false, true, false, false, false, false);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slotFrom, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, upd2);

                                (pData.Bag.Items[slotTo] as Game.Items.Clothes).Unwear(pData);
                                pData.Accessories[slotFrom]?.Wear(pData);
                            });

                            pData.Bag.Update();
                            MySQL.UpdatePlayerInventory(pData, false, false, true, false, false, false, false);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.BagItem,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync (() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                player.TriggerEvent("Inventory::Update", (int)Groups.BagItem, upd1);
                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, upd2);

                                (pData.Items[slotTo] as Game.Items.Bag).Unwear(pData);
                                pData.Bag?.Wear(pData);
                            });

                            MySQL.UpdatePlayerInventory(pData, true, false, false, true, false, false, false);

                            return Results.Success;
                        }
                    },
                }
            },

            {
                Groups.HolsterItem,

                new Dictionary<Groups, Func<PlayerData, int, int, int, Task<Results>>>()
                {
                    {
                        Groups.Items,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                (pData.Items[slotTo] as Game.Items.Holster).Unwear(pData);
                                pData.Holster?.Wear(pData);

                                if (((pData.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                                {
                                    ((pData.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                    (pData.Holster?.Items[0] as Game.Items.Weapon)?.Equip(pData);
                                }

                                player.TriggerEvent("Inventory::Update", (int)Groups.Items, slotTo, Game.Items.Item.ToClientJson(pData.Items[slotTo], Groups.Items));
                                player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));
                            });

                            MySQL.UpdatePlayerInventory(pData, true, false, false, false, true, false, false);

                            return Results.Success;
                        }
                    },

                    {
                        Groups.Bag,

                        async (pData, slotTo, slotFrom, amount) =>
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

                            await NAPI.Task.RunAsync(() =>
                            {
                                if (player?.Exists != true)
                                    return;

                                (pData.Bag.Items[slotTo] as Game.Items.Holster).Unwear(pData);
                                pData.Holster?.Wear(pData);

                                if (((pData.Bag.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                                {
                                    ((pData.Bag.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(pData);
                                    (pData.Holster?.Items[0] as Game.Items.Weapon)?.Equip(pData);
                                }

                                player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slotTo, Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Groups.Bag));
                                player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(pData.Holster, Groups.HolsterItem));
                            });

                            pData.Bag.Update();
                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, true, false, false);

                            return Results.Success;
                        }
                    },
                }
            },
        };

        private static Dictionary<Groups, Func<PlayerData, int, int, Task<Game.Items.Item>>> DropActions = new Dictionary<Groups, Func<PlayerData, int, int, Task<Game.Items.Item>>>()
        {
            {
                Groups.Items,

                async (pData, slot, amount) =>
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
                            item = await Game.Items.Items.CreateItem(item.ID, 0, amount);
                        }
                        else
                            pData.Items[slot] = null;
                    }
                    else
                        pData.Items[slot] = null;

                    var upd = Game.Items.Item.ToClientJson(pData.Items[slot], Groups.Items);

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.Items, slot, upd);
                    });

                    MySQL.UpdatePlayerInventory(pData, true);

                    return item;
                }
            },

            {
                Groups.Bag,

                async (pData, slot, amount) =>
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
                            item = await Game.Items.Items.CreateItem(item.ID, 0, amount);
                        }
                        else
                            pData.Bag.Items[slot] = null;
                    }
                    else
                        pData.Bag.Items[slot] = null;

                    var upd = Game.Items.Item.ToClientJson(pData.Bag.Items[slot], Groups.Bag);

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.Bag, slot, upd);
                    });

                    pData.Bag.Update();

                    return item;
                }
            },

            {
                Groups.Weapons,

                async (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (slot >= pData.Weapons.Length)
                        return null;

                    var item = pData.Weapons[slot];

                    if (item == null)
                        return null;

                    if (item.Equiped)
                        Sync.WeaponSystem.UpdateAmmo(pData, item, false);

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.Weapons, slot, Game.Items.Item.ToClientJson(null, Groups.Weapons));

                        if (item.Equiped)
                            item.Unequip(pData, false, false);
                        else
                            item.Unwear(pData);
                    });

                    pData.Weapons[slot] = null;

                    MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);

                    return item;
                }
            },

            {
                Groups.Holster,

                async (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (pData.Holster == null)
                        return null;

                    var item = (Game.Items.Weapon)pData.Holster.Items[0];

                    if (item == null)
                        return null;

                    if (item.Equiped)
                        Sync.WeaponSystem.UpdateAmmo(pData, item, false);

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.Holster, 2, Game.Items.Item.ToClientJson(null, Groups.Holster));

                        if (item.Equiped)
                            item.Unequip(pData, false, false);
                        else
                            item.Unwear(pData);
                    });

                    pData.Holster.Items[0] = null;

                    pData.Holster.Update();

                    return item;
                }
            },

            {
                Groups.Clothes,

                async (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (slot >= pData.Clothes.Length)
                        return null;

                    var item = pData.Clothes[slot];

                    if (item == null)
                        return null;

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.Clothes, slot, Game.Items.Item.ToClientJson(null, Groups.Clothes));

                        item.Unwear(pData);
                    });

                    pData.Clothes[slot] = null;

                    MySQL.UpdatePlayerInventory(pData, false, true);

                    return item;
                }
            },

            {
                Groups.Accessories,

                async (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    if (slot >= pData.Accessories.Length)
                        return null;

                    var item = pData.Accessories[slot];

                    if (item == null)
                        return null;

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.Accessories, slot, Game.Items.Item.ToClientJson(null, Groups.Accessories));

                        item.Unwear(pData);
                    });

                    pData.Accessories[slot] = null;

                    MySQL.UpdatePlayerInventory(pData, false, false, true);

                    return item;
                }
            },

            {
                Groups.BagItem,

                async (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    var item = pData.Bag;

                    if (item == null)
                        return null;

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.BagItem, Game.Items.Item.ToClientJson(null, Groups.BagItem));

                        item.Unwear(pData);
                    });

                    pData.Bag = null;

                    MySQL.UpdatePlayerInventory(pData, false, false, false, true);

                    return item;
                }
            },

            {
                Groups.HolsterItem,

                async (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    var item = pData.Holster;

                    if (item == null)
                        return null;

                    if ((pData.Holster.Items[0] as Game.Items.Weapon)?.Equiped == true)
                        Sync.WeaponSystem.UpdateAmmo(pData, pData.Holster.Items[0] as Game.Items.Weapon, false);

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.HolsterItem, Game.Items.Item.ToClientJson(null, Groups.HolsterItem));

                        item.Unwear(pData);

                        (item.Items[0] as Game.Items.Weapon)?.Unequip(pData, false, false);
                    });

                    pData.Holster = null;

                    MySQL.UpdatePlayerInventory(pData, false, false, false, false, true);

                    return item;
                }
            },

            {
                Groups.Armour,

                async (pData, slot, amount) =>
                {
                    var player = pData.Player;

                    var item = pData.Armour;

                    if (item == null)
                        return null;

                    await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", (int)Groups.Armour, Game.Items.Item.ToClientJson(null, Groups.Armour));

                        item.Unwear(pData);
                    });

                    pData.Armour = null;

                    MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);

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
        public static async Task<Results> Replace(PlayerData pData, Groups to, int slotTo, Groups from, int slotFrom, int amount)
        {
            var player = pData.Player;

            var res = await Task.Run(async () =>
            {
                if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                    return Results.Error;

                var action = ReplaceActions.GetValueOrDefault(from)?.GetValueOrDefault(to);

                if (action != null)
                    return await action.Invoke(pData, slotTo, slotFrom, amount);

                return Results.Error;
            });

            var notification = ResultsNotifications.GetValueOrDefault(res);

            if (notification == null)
                return res;

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                player.Notify(notification);
            });

            return res;
        }

        [RemoteEvent("Inventory::Replace")]
        private static async Task ReplaceRemote(Player player, int to, int slotTo, int from, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (!Enum.IsDefined(typeof(Groups), to) || !Enum.IsDefined(typeof(Groups), from))
                    return;

                var offer = await pData.ActiveOffer;

                if (offer != null)
                {
                    if (offer.Type == Sync.Offers.Types.Exchange && offer.TradeData != null)
                        return;
                }

                await Replace(pData, (Groups)to, slotTo, (Groups)from, slotFrom, amount);
            });

            pData.Release();
        }
        #endregion

        #region Action
        /// <summary>Метод для выполнения действия предмета</summary>
        /// <param name="pData">PlayerData</param>
        /// <param name="group">Группа</param>
        /// <param name="slot">Слот</param>
        /// <param name="action">Действие (минимум - 5)</param>
        public static async Task<Results> Action(PlayerData pData, Groups group, int slot, int action = 5, params object[] args)
        {
            var player = pData.Player;

            return await Task.Run(async () =>
            {
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

                return await a2.Invoke(pData, item, group, slot, args);
            });
        }

        [RemoteEvent("Inventory::Action")]
        private static async Task ActionRemote(Player player, int group, int slot, int action)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (!Enum.IsDefined(typeof(Groups), group) || slot < 0 || action < 5)
                    return;

                if (pData.CurrentBusiness != null)
                    return;

                var offer = await pData.ActiveOffer;

                if (offer != null)
                {
                    if (offer.Type == Sync.Offers.Types.Exchange && offer.TradeData != null)
                        return;
                }

                await Action(pData, (Groups)group, slot, action);
            });

            pData.Release();
        }
        #endregion

        #region Drop
        /// <summary>Метод для выбрасывания предмета</summary>
        /// <param name="pData">PlayerData</param>
        /// <param name="group">Группа</param>
        /// <param name="slot">Слот</param>
        /// <param name="amount">Кол-во (минимум - 1, -1 - предмет целиком)</param>
        public static async Task Drop(PlayerData pData, Groups group, int slot, int amount)
        {
            if (pData == null)
                return;

            var player = pData.Player;

            await Task.Run(async () =>
            {
                if (amount < 1 || slot < 0)
                    return;

                var action = DropActions.GetValueOrDefault(group);

                if (action == null)
                    return;

                Game.Items.Item item = await action.Invoke(pData, slot, amount);

                if (item == null)
                    return;

                if (item.IsTemp)
                {
                    item = null;

                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.Notify("Inventory::TempItemDeleted");
                    });

                    return;
                }

                (Vector3 FrontOf, Vector3 Rotation, uint Dimension) dropData = await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return (null, null, 0);

                    if (!pData.CanPlayAnim())
                        pData.PlayAnim(Sync.Animations.FastTypes.Putdown);

                    return (player.GetFrontOf(0.6f), player.Rotation, player.Dimension);
                });

                if (dropData.FrontOf != null)
                    await Game.World.AddItemOnGround(player, item, dropData.FrontOf, dropData.Rotation, dropData.Dimension);
            });
        }

        [RemoteEvent("Inventory::Drop")]
        private static async Task DropRemote(Player player, int slotStr, int slot, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (!Enum.IsDefined(typeof(Groups), slotStr) || slot < 0)
                    return;

                var offer = await pData.ActiveOffer;

                if (offer != null)
                {
                    if (offer.Type == Sync.Offers.Types.Exchange && offer.TradeData != null)
                        return;
                }

                await Drop(pData, (Groups)slotStr, slot, amount);
            });

            pData.Release();
        }
        #endregion

        #region Take
        [RemoteEvent("Inventory::Take")]
        private static async Task TakeRemote(Player player, uint UID, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Game.World.IOGSemaphore.WaitAsync();

            await Task.Run(async () =>
            {
                var offer = await pData.ActiveOffer;

                if (offer != null)
                {
                    if (offer.Type == Sync.Offers.Types.Exchange && offer.TradeData != null)
                        return;
                }

                var item = Game.World.GetItemOnGround(UID);

                if (item == null)
                    return;

                var items = pData.Items;

                var curWeight = items.Sum(x => x?.Weight ?? 0f);

                var freeIdx = Array.FindIndex(items, x => x == null);

                var curAmount = (item.Item as Game.Items.IStackable)?.Amount ?? 1;

                if (amount > curAmount)
                    amount = curAmount;

                if (item.Item is Game.Items.IStackable)
                {
                    if (amount > curAmount)
                        amount = curAmount;

                    if (curWeight + amount * item.Item.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                    {
                        amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / item.Item.BaseWeight);
                    }

                    for (int i = 0; i < items.Length; i++)
                        if (items[i] != null && items[i].ID == item.Item.ID && (items[i] as Game.Items.IStackable).Amount + amount <= (items[i] as Game.Items.IStackable).MaxAmount)
                        {
                            freeIdx = i;

                            break;
                        }
                }
                else
                {
                    if (curWeight + item.Item.Weight > Settings.MAX_INVENTORY_WEIGHT)
                    {
                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.Notify("Inventory::NoSpace");
                        });

                        return;
                    }
                }

                if (freeIdx == -1 || amount == 0)
                {
                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.Notify("Inventory::NoSpace");
                    });

                    return;
                }

                var result = await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return false;

                    if (!player.AreEntitiesNearby(item.Object, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                        return false;

                    if (!pData.CanPlayAnim())
                        pData.PlayAnim(Sync.Animations.FastTypes.Pickup);

                    if (amount == curAmount)
                        item.Delete(items[freeIdx] != null);
                    else
                    {
                        (item.Item as Game.Items.IStackable).Amount -= amount;

                        item.UpdateAmount();
                    }

                    return true;
                });

                if (!result)
                    return;

                if (amount == curAmount)
                {
                    if (items[freeIdx] == null)
                        items[freeIdx] = item.Item;
                    else
                    {
                        (items[freeIdx] as Game.Items.IStackable).Amount += amount;

                        items[freeIdx].Update();
                    }
                }
                else
                {
                    if (items[freeIdx] == null)
                        items[freeIdx] = await Game.Items.Items.CreateItem(item.Item.ID, 0, amount);
                    else
                    {
                        (items[freeIdx] as Game.Items.IStackable).Amount += amount;

                        items[freeIdx].Update();
                    }
                }


                var upd = Game.Items.Item.ToClientJson(items[freeIdx], Groups.Items);

                NAPI.Task.Run(() =>
                {
                    player?.TriggerEvent("Inventory::Update", (int)Groups.Items, freeIdx, upd);
                });

                MySQL.UpdatePlayerInventory(pData, true);
            });

            Game.World.IOGSemaphore.Release();

            pData.Release();
        }
        #endregion
    }
}
