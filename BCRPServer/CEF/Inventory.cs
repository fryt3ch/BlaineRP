﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.CEF
{
    /*
        OVER 3.000 LINES! BE CAREFUL! IT SLOWS IDE
    */

    public class Inventory : Script
    {
        #region Settings
        /// <summary>Слоты одежды</summary>
        public static Dictionary<Game.Items.Item.Types, int> ClothesSlots = new Dictionary<Game.Items.Item.Types, int>()
        {
            { Game.Items.Item.Types.Hat, 0 },
            { Game.Items.Item.Types.Top, 1 },
            { Game.Items.Item.Types.Under, 2 },
            { Game.Items.Item.Types.Pants, 3 },
            { Game.Items.Item.Types.Shoes, 4 },
        };

        /// <summary>Слоты аксессуаров</summary>
        public static Dictionary<Game.Items.Item.Types, int> AccessoriesSlots = new Dictionary<Game.Items.Item.Types, int>()
        {
            { Game.Items.Item.Types.Glasses, 0 },
            { Game.Items.Item.Types.Mask, 1 },
            { Game.Items.Item.Types.Ears, 2 },
            { Game.Items.Item.Types.Accessory, 3 },
            { Game.Items.Item.Types.Watches, 4 },
            { Game.Items.Item.Types.Bracelet, 5 },
            { Game.Items.Item.Types.Ring, 6 },
            { Game.Items.Item.Types.Gloves, 7 },
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
        #endregion

        #region Show
        [RemoteEvent("Inventory::Show")]
        public static async Task Show(Player player, bool state, int ammo)
        {
            var sRes = player.CheckSpamAttack(1000);

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                var weapon = pData.ActiveWeapon;

                if (weapon != null)
                {
                    if (ammo > weapon.Value.WeaponItem.Ammo)
                    {
                        weapon.Value.WeaponItem.Ammo = 0;

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists == true)
                                weapon.Value.WeaponItem.UpdateAmmo(player);
                        });
                    }
                    else
                    {
                        if (ammo < 0)
                            ammo = 0;

                        weapon.Value.WeaponItem.Ammo = ammo;
                    }
                }
            });

            pData.Release();
        }
        #endregion

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
            if (pData == null)
                return Results.Error;

            var player = pData.Player;

            var res = await Task.Run<Results>(async () =>
            {
                if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                    return Results.Error;

                #region From Pockets (To - Pockets, Bag, Weapons, Holster, Clothes, Accessories, Holster Item, Bag Item, Armour)
                if (from == Groups.Items)
                {
                    if (slotFrom >= pData.Items.Length)
                        return Results.Error;

                    if (pData.Items[slotFrom] == null)
                        return Results.Error;

                    #region To Pockets
                    if (to == Groups.Items)
                    {
                        if (slotTo >= pData.Items.Length)
                            return Results.Error;

                        bool wasCreated = false;
                        bool wasDeleted = false;

                        #region Unite
                        if (pData.Items[slotTo] != null && pData.Items[slotTo].Type == pData.Items[slotFrom].Type && pData.Items[slotFrom] is Game.Items.IStackable)
                        {
                            if (pData.Items[slotFrom].IsTemp || pData.Items[slotTo]?.IsTemp == true)
                                return Results.TempItem;

                            int slotToAmount = (pData.Items[slotTo] as Game.Items.IStackable).Amount;
                            int slotFromAmount = (pData.Items[slotFrom] as Game.Items.IStackable).Amount;

                            // if no amount requested -> suggest it's whole item
                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            int maxStack = (pData.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                            if (slotToAmount == maxStack)
                                return Results.Error;

                            // if new amount > maxStack -> reduce new amount
                            if (slotToAmount + amount > maxStack)
                            {
                                (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                (pData.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                            }
                            else // if new amount <= maxStack
                            {
                                (pData.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                // delete old item if amount is 0 now
                                if ((pData.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                {
                                    wasDeleted = true;

                                    var tItem = pData.Items[slotFrom];

                                    Task.Run(() =>
                                    {
                                        tItem.Delete();
                                    });

                                    pData.Items[slotFrom] = null;
                                }
                            }

                            pData.Items[slotTo].Update();
                            pData.Items[slotFrom]?.Update();
                        }
                        #endregion
                        #region Split
                        else if (pData.Items[slotFrom] is Game.Items.IStackable && pData.Items[slotTo] == null && amount != -1 && amount < (pData.Items[slotFrom] as Game.Items.IStackable).Amount) // split to new item
                        {
                            if (pData.Items[slotFrom].IsTemp)
                                return Results.TempItem;

                            wasCreated = true;

                            (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                            pData.Items[slotFrom].Update();

                            pData.Items[slotTo] = await Game.Items.Items.CreateItem(pData.Items[slotFrom].ID, 0, amount);
                        }
                        #endregion
                        #region Replace
                        else
                        {
                            var temp = pData.Items[slotFrom];
                            pData.Items[slotFrom] = pData.Items[slotTo];
                            pData.Items[slotTo] = temp;

                            wasCreated = true;
                        }
                        #endregion

                        var upd1 = (pData.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Items[slotFrom].ID, Game.Items.Items.GetItemAmount(pData.Items[slotFrom]), Game.Items.Items.GetItemWeight(pData.Items[slotFrom]))).SerializeToJson();
                        var upd2 = (pData.Items[slotTo].ID, Game.Items.Items.GetItemAmount(pData.Items[slotTo]), Game.Items.Items.GetItemWeight(pData.Items[slotTo])).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 0, slotTo, upd2);
                        });

                        if (wasCreated || wasDeleted)
                            MySQL.UpdatePlayerInventory(pData, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Bag
                    else if (to == Groups.Bag)
                    {
                        if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                            return Results.Error;

                        if (pData.Items[slotFrom] is Game.Items.Bag)
                            return Results.PlaceRestricted;

                        if (pData.Items[slotFrom].IsTemp)
                            return Results.TempItem;

                        float curWeight = pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight;
                        float maxWeight = pData.Bag.Data.MaxWeight;

                        bool wasDeleted = false;
                        bool wasCreated = false;

                        #region Unite
                        if (pData.Bag.Items[slotTo] != null && pData.Bag.Items[slotTo].Type == pData.Items[slotFrom].Type && pData.Items[slotFrom] is Game.Items.IStackable)
                        {
                            int slotToAmount = (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount;
                            int slotFromAmount = (pData.Items[slotFrom] as Game.Items.IStackable).Amount;

                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            int maxStack = (pData.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                            if (slotToAmount == maxStack)
                                return Results.Error;

                            // if amount*weight is too big -> reduce amount to fit the pData.Bag.Items's maxWeight
                            if (curWeight + amount * pData.Items[slotFrom].Weight > maxWeight)
                            {
                                amount = (int)Math.Floor((maxWeight - curWeight) / pData.Items[slotFrom].Weight);

                                if (amount == 0)
                                    return Results.NoSpace;
                            }

                            if (slotToAmount + amount > maxStack)
                            {
                                (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                            }
                            else
                            {
                                (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                if ((pData.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                {
                                    var tItem = pData.Items[slotFrom];

                                    Task.Run(() =>
                                    {
                                        tItem.Delete();
                                    });

                                    pData.Items[slotFrom] = null;

                                    wasDeleted = true;
                                }
                            }

                            pData.Bag.Items[slotTo].Update();
                            pData.Items[slotFrom]?.Update();
                        }
                        #endregion
                        #region Split To New
                        else if (pData.Items[slotFrom] is Game.Items.IStackable && pData.Bag.Items[slotTo] == null && amount != -1 && amount < (pData.Items[slotFrom] as Game.Items.IStackable).Amount)
                        {
                            if (pData.Items[slotFrom].Weight * amount + curWeight > maxWeight)
                            {
                                amount = (int)Math.Floor((maxWeight - curWeight) / pData.Items[slotFrom].Weight);

                                if (amount == 0)
                                    return Results.NoSpace;
                            }

                            (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                            pData.Items[slotFrom].Update();

                            pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(pData.Items[slotFrom].ID, 0, amount); // but wait for that :)

                            wasCreated = true;
                        }
                        #endregion
                        #region Replace
                        else
                        {
                            var addWeightItems = pData.Bag.Items[slotTo] != null ? Game.Items.Items.GetItemWeight(pData.Bag.Items[slotTo], true) : 0;
                            var addWeightBag = Game.Items.Items.GetItemWeight(pData.Items[slotFrom], true);

                            if (addWeightBag - addWeightItems + curWeight > maxWeight || addWeightItems - addWeightBag + Game.Items.Items.GetWeight(pData.Items) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            var temp = pData.Items[slotFrom];
                            pData.Items[slotFrom] = pData.Bag.Items[slotTo];
                            pData.Bag.Items[slotTo] = temp;

                            wasDeleted = true; wasCreated = true;
                        }
                        #endregion

                        var upd1 = (pData.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Items[slotFrom].ID, Game.Items.Items.GetItemAmount(pData.Items[slotFrom]), Game.Items.Items.GetItemWeight(pData.Items[slotFrom]))).SerializeToJson();
                        var upd2 = (pData.Bag.Items[slotTo].ID, Game.Items.Items.GetItemAmount(pData.Bag.Items[slotTo]), Game.Items.Items.GetItemWeight(pData.Bag.Items[slotTo])).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 1, slotTo, upd2);
                        });

                        if (wasDeleted)
                            MySQL.UpdatePlayerInventory(pData, true);
                        
                        if (wasCreated)
                            pData.Bag.Update();

                        return Results.Success;
                    }
                    #endregion
                    #region To Weapons
                    else if (to == Groups.Weapons)
                    {
                        if (slotTo >= pData.Weapons.Length)
                            return Results.Error;

                        bool wasDeleted = false;
                        bool wasReplaced = false;

                        #region Replace
                        if (pData.Items[slotFrom] is Game.Items.Weapon)
                        {
                            if (pData.Weapons[slotTo] != null && Game.Items.Items.GetWeight(pData.Items) + pData.Weapons[slotTo].Weight - (pData.Items[slotFrom] as Game.Items.Weapon).Weight > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            var temp = pData.Weapons[slotTo];
                            pData.Weapons[slotTo] = pData.Items[slotFrom] as Game.Items.Weapon;
                            pData.Items[slotFrom] = temp;

                            wasReplaced = true;
                        }
                        #endregion
                        #region Load
                        else if (pData.Items[slotFrom] is Game.Items.Ammo && pData.Weapons[slotTo] != null && pData.Weapons[slotTo].Data.AmmoType != null && (pData.Items[slotFrom] as Game.Items.Ammo).Type == pData.Weapons[slotTo].Data.AmmoType)
                        {
                            var slotFromAmount = (pData.Items[slotFrom] as Game.Items.Ammo).Amount;
                            var slotToAmount = pData.Weapons[slotTo].Ammo;

                            var maxAmount = pData.Weapons[slotTo].Data.MaxAmmo;

                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            if (slotToAmount == maxAmount)
                                return Results.Error;

                            if (slotToAmount + amount > maxAmount)
                            {
                                (pData.Items[slotFrom] as Game.Items.Ammo).Amount -= maxAmount - slotToAmount;
                                pData.Weapons[slotTo].Ammo = maxAmount;
                            }
                            else
                            {
                                pData.Weapons[slotTo].Ammo += amount;
                                (pData.Items[slotFrom] as Game.Items.Ammo).Amount -= amount;

                                if ((pData.Items[slotFrom] as Game.Items.Ammo).Amount == 0)
                                {
                                    wasDeleted = true;

                                    var tItem = pData.Items[slotFrom];

                                    Task.Run(() =>
                                    {
                                        tItem.Delete();
                                    });

                                    pData.Items[slotFrom] = null;
                                }
                            }

                            pData.Weapons[slotTo].Update();
                            pData.Items[slotFrom]?.Update();
                        }
                        #endregion
                        else
                            return Results.Error;

                        var upd1 = pData.Items[slotFrom] == null ? "null" : ((pData.Items[slotFrom].ID, Game.Items.Items.GetItemAmount(pData.Items[slotFrom]), Game.Items.Items.GetItemWeight(pData.Items[slotFrom]))).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);

                            if (pData.Items[slotFrom] != null && pData.Items[slotFrom] is Game.Items.Weapon)
                            {
                                if ((pData.Items[slotFrom] as Game.Items.Weapon).Equiped)
                                {
                                    (pData.Items[slotFrom] as Game.Items.Weapon).Unequip(player);
                                    pData.Weapons[slotTo].Equip(player);
                                }
                                else
                                    (pData.Items[slotFrom] as Game.Items.Weapon).Unwear(player);
                            }
                            else
                            {
                                pData.Weapons[slotTo].UpdateAmmo(player);

                                pData.Weapons[slotTo].Wear(player);
                            }

                            player.TriggerEvent("Inventory::Update", 2, slotTo, (pData.Weapons[slotTo].ID, pData.Weapons[slotTo].Ammo, pData.Weapons[slotTo].Equiped).SerializeToJson());
                        });

                        if (wasDeleted)
                            MySQL.UpdatePlayerInventory(pData, true);
                        else if (wasReplaced)
                            MySQL.UpdatePlayerInventory(pData, true, false, false, false, false, true, false);

                        return Results.Success;
                    }
                    #endregion
                    #region To Clothes
                    else if (to == Groups.Clothes)
                    {
                        if (slotTo >= pData.Clothes.Length)
                            return Results.Error;

                        if (!(pData.Items[slotFrom] is Game.Items.Clothes))
                            return Results.Error;

                        if (!ClothesSlots.ContainsKey(pData.Items[slotFrom].Type) || ClothesSlots[pData.Items[slotFrom].Type] != slotTo)
                            return Results.Error;

                        if (pData.Clothes[slotTo] != null && pData.Clothes[slotTo].Weight + Game.Items.Items.GetWeight(pData.Items) - pData.Items[slotFrom].Weight > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Clothes[slotTo];
                        pData.Clothes[slotTo] = pData.Items[slotFrom] as Game.Items.Clothes;
                        pData.Items[slotFrom] = temp;

                        var upd1 = (pData.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Items[slotFrom].ID, 1, pData.Items[slotFrom].Weight)).SerializeToJson();
                        var upd2 = (pData.Clothes[slotTo].ID).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 4, slotTo, upd2);

                            temp?.Unwear(player);
                            pData.Clothes[slotTo].Wear(player);
                        });

                        MySQL.UpdatePlayerInventory(pData, true, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Accessories
                    else if (to == Groups.Accessories)
                    {
                        if (slotTo >= pData.Accessories.Length)
                            return Results.Error;

                        if (!(pData.Items[slotFrom] is Game.Items.Clothes))
                            return Results.Error;

                        if (!AccessoriesSlots.ContainsKey(pData.Items[slotFrom].Type) || AccessoriesSlots[pData.Items[slotFrom].Type] != slotTo)
                            return Results.Error;

                        if (pData.Accessories[slotTo] != null && pData.Accessories[slotTo].Weight + Game.Items.Items.GetWeight(pData.Items) - pData.Items[slotFrom].Weight > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Accessories[slotTo];
                        pData.Accessories[slotTo] = pData.Items[slotFrom] as Game.Items.Clothes;
                        pData.Items[slotFrom] = temp;

                        var upd1 = (pData.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Items[slotFrom].ID, 1, pData.Items[slotFrom].Weight)).SerializeToJson();
                        var upd2 = (pData.Accessories[slotTo].ID).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 5, slotTo, upd2);

                            temp?.Unwear(player);
                            pData.Accessories[slotTo].Wear(player);
                        });

                        MySQL.UpdatePlayerInventory(pData, true, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Bag Item
                    else if (to == Groups.BagItem)
                    {
                        if (!(pData.Items[slotFrom] is Game.Items.Bag))
                            return Results.Error;

                        if (pData.Items[slotFrom].IsTemp)
                            return Results.TempItem;

                        if (pData.Bag != null && pData.Bag.Weight + Game.Items.Items.GetWeight(pData.Items) - (pData.Items[slotFrom] as Game.Items.Bag).Weight > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Bag;
                        pData.Bag = pData.Items[slotFrom] as Game.Items.Bag;
                        pData.Items[slotFrom] = temp;

                        var upd1 = (pData.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Items[slotFrom].ID, 1, (pData.Items[slotFrom] as Game.Items.Bag).Weight)).SerializeToJson();
                        var upd2 = (pData.Bag == null ? null : ((string, float, IEnumerable<(string, int, float)?>)?)(pData.Bag.ID, pData.Bag.Data.MaxWeight, pData.Bag.Items.Select(x => x == null ? null : ((string, int, float)?)(x.ID, Game.Items.Items.GetItemAmount(x), Game.Items.Items.GetItemWeight(x))))).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 6, upd2);

                            temp?.Unwear(player);
                            pData.Bag.Wear(player);
                        });

                        MySQL.UpdatePlayerInventory(pData, true, false, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Holster Item
                    else if (to == Groups.HolsterItem)
                    {
                        if (!(pData.Items[slotFrom] is Game.Items.Holster))
                            return Results.Error;

                        if (pData.Items[slotFrom].IsTemp || pData.Holster?.Items[0]?.IsTemp == true)
                            return Results.TempItem;

                        if (pData.Holster != null && pData.Holster.Weight + Game.Items.Items.GetWeight(pData.Items) - (pData.Items[slotFrom] as Game.Items.Holster).Weight > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Holster;
                        pData.Holster = pData.Items[slotFrom] as Game.Items.Holster;
                        pData.Items[slotFrom] = temp;

                        var upd1 = pData.Items[slotFrom] == null ? "null" : ((pData.Items[slotFrom].ID, 1, (pData.Items[slotFrom] as Game.Items.Holster).Weight)).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);

                            (pData.Items[slotFrom] as Game.Items.Holster)?.Unwear(player);
                            pData.Holster.Wear(player);

                            if (pData.Items[slotFrom] != null && ((pData.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(player);
                                (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(player);
                            }

                            player.TriggerEvent("Inventory::Update", 7, (pData.Holster.ID, pData.Holster.Items[0] == null ? null : ((string, int, bool)?)(pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, (pData.Holster.Items[0] as Game.Items.Weapon).Equiped)).SerializeToJson());
                        });

                        MySQL.UpdatePlayerInventory(pData, true, false, false, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Armour
                    else if (to == Groups.Armour)
                    {
                        if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                            return Results.Wounded;

                        if (!(pData.Items[slotFrom] is Game.Items.BodyArmour))
                            return Results.Error;

                        if (pData.Armour != null && pData.Armour.Weight + Game.Items.Items.GetWeight(pData.Items) - pData.Items[slotFrom].Weight > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Armour;
                        pData.Armour = pData.Items[slotFrom] as Game.Items.BodyArmour;
                        pData.Items[slotFrom] = temp;

                        var upd1 = (pData.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Items[slotFrom].ID, 1, pData.Items[slotFrom].Weight)).SerializeToJson();
                        var upd2 = ((pData.Armour.ID, pData.Armour.Strength)).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 8, upd2);

                            temp?.Unwear(player);
                            pData.Armour.Wear(player);
                        });

                        MySQL.UpdatePlayerInventory(pData, true, false, false, false, false, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Holster
                    else if (to == Groups.Holster)
                    {
                        if (pData.Holster == null)
                            return Results.Error;

                        if (pData.Items[slotFrom].IsTemp)
                            return Results.TempItem;

                        bool wasDeleted = false;
                        bool wasReplaced = false;

                        #region Replace
                        if (pData.Items[slotFrom] is Game.Items.Weapon)
                        {
                            if ((pData.Items[slotFrom] as Game.Items.Weapon).Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                return Results.PlaceRestricted;

                            if (pData.Holster.Items[0] != null && Game.Items.Items.GetWeight(pData.Items) + (pData.Holster.Items[0] as Game.Items.Weapon).Weight - (pData.Items[slotFrom] as Game.Items.Weapon).Weight > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            var temp = pData.Holster.Items[0];
                            pData.Holster.Items[0] = pData.Items[slotFrom] as Game.Items.Weapon;
                            pData.Items[slotFrom] = temp;

                            pData.Holster.Update();

                            wasReplaced = true;
                        }
                        #endregion
                        #region Load
                        else if (pData.Items[slotFrom] is Game.Items.Ammo && pData.Holster.Items[0] != null && (pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType != null && (pData.Items[slotFrom] as Game.Items.Ammo).Type == (pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType)
                        {
                            var slotFromAmount = (pData.Items[slotFrom] as Game.Items.Ammo).Amount;
                            var slotToAmount = (pData.Holster.Items[0] as Game.Items.Weapon).Ammo;

                            var maxAmount = (pData.Holster.Items[0] as Game.Items.Weapon).Data.MaxAmmo;

                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            if (slotToAmount == maxAmount)
                                return Results.Error;

                            if (slotToAmount + amount > maxAmount)
                            {
                                (pData.Items[slotFrom] as Game.Items.Ammo).Amount -= maxAmount - slotToAmount;
                                (pData.Holster.Items[0] as Game.Items.Weapon).Ammo = maxAmount;
                            }
                            else
                            {
                                (pData.Holster.Items[0] as Game.Items.Weapon).Ammo += amount;
                                (pData.Items[slotFrom] as Game.Items.Ammo).Amount -= amount;

                                if ((pData.Items[slotFrom] as Game.Items.Ammo).Amount == 0)
                                {
                                    wasDeleted = true;

                                    var tItem = pData.Items[slotFrom];

                                    Task.Run(() =>
                                    {
                                        tItem.Delete();
                                    });

                                    pData.Items[slotFrom] = null;
                                }
                            }

                            pData.Holster.Items[0].Update();
                            pData.Items[slotFrom]?.Update();
                        }
                        #endregion
                        else
                            return Results.Error;

                        var upd1 = pData.Items[slotFrom] == null ? "null" : ((pData.Items[slotFrom].ID, Game.Items.Items.GetItemAmount(pData.Items[slotFrom]), Game.Items.Items.GetItemWeight(pData.Items[slotFrom]))).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotFrom, upd1);

                            if (pData.Items[slotFrom] != null && pData.Items[slotFrom] is Game.Items.Weapon)
                            {
                                if ((pData.Items[slotFrom] as Game.Items.Weapon).Equiped)
                                {
                                    (pData.Items[slotFrom] as Game.Items.Weapon).Unequip(player);
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Equip(player);
                                }
                                else
                                    (pData.Items[slotFrom] as Game.Items.Weapon).Unwear(player);
                            }
                            else
                            {
                                (pData.Holster.Items[0] as Game.Items.Weapon).UpdateAmmo(player);

                                (pData.Holster.Items[0] as Game.Items.Weapon).Wear(player);
                            }

                            player.TriggerEvent("Inventory::Update", 3, 2, (pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, (pData.Holster.Items[0] as Game.Items.Weapon).Equiped).SerializeToJson());
                        });

                        if (wasDeleted || wasReplaced)
                            MySQL.UpdatePlayerInventory(pData, true);

                        return Results.Success;
                    }
                    #endregion
                }
                #endregion
                #region From Bag (To - Pockets, Bag, Weapons, Holster, Clothes, Accessories, Holster Item, Armour)
                else if (from == Groups.Bag)
                {
                    if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length)
                        return Results.Error;

                    if (pData.Bag.Items[slotFrom] == null)
                        return Results.Error;

                    #region To Bag
                    if (to == Groups.Bag)
                    {
                        if (slotTo >= pData.Bag.Items.Length)
                            return Results.Error;

                        bool wasCreated = false;
                        bool wasDeleted = false;

                        #region Unite
                        if (pData.Bag.Items[slotTo] != null && pData.Bag.Items[slotTo].Type == pData.Bag.Items[slotFrom].Type && pData.Bag.Items[slotFrom] is Game.Items.IStackable)
                        {
                            int slotToAmount = (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount;
                            int slotFromAmount = (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount;

                            // if no amount requested -> suggest it's whole item
                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            int maxStack = (pData.Bag.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                            if (slotToAmount == maxStack)
                                return Results.Error;

                            // if new amount > maxStack -> reduce new amount
                            if (slotToAmount + amount > maxStack)
                            {
                                (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                            }
                            else // if new amount <= maxStack
                            {
                                (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                // delete old item if amount is 0 now
                                if ((pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                {
                                    wasDeleted = true;

                                    var tItem = pData.Bag.Items[slotFrom];

                                    Task.Run(() =>
                                    {
                                        tItem.Delete();
                                    });

                                    pData.Bag.Items[slotFrom] = null;
                                }
                            }

                            pData.Bag.Items[slotTo].Update();
                            pData.Bag.Items[slotFrom]?.Update();
                        }
                        #endregion
                        #region Split
                        else if (pData.Bag.Items[slotFrom] is Game.Items.IStackable && pData.Bag.Items[slotTo] == null && amount != -1 && amount < (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount) // split to new item
                        {
                            wasCreated = true;

                            (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                            pData.Bag.Items[slotFrom].Update();

                            pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(pData.Bag.Items[slotFrom].ID, 0, amount);
                        }
                        #endregion
                        #region Replace
                        else
                        {
                            var temp = pData.Bag.Items[slotFrom];
                            pData.Bag.Items[slotFrom] = pData.Bag.Items[slotTo];
                            pData.Bag.Items[slotTo] = temp;

                            wasCreated = true;
                        }
                        #endregion

                        var upd1 = (pData.Bag.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Bag.Items[slotFrom].ID, Game.Items.Items.GetItemAmount(pData.Bag.Items[slotFrom]), Game.Items.Items.GetItemWeight(pData.Bag.Items[slotFrom]))).SerializeToJson();
                        var upd2 = (pData.Bag.Items[slotTo].ID, Game.Items.Items.GetItemAmount(pData.Bag.Items[slotTo]), Game.Items.Items.GetItemWeight(pData.Bag.Items[slotTo])).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 1, slotTo, upd2);
                        });

                        if (wasCreated || wasDeleted)
                            pData.Bag.Update();

                        return Results.Success;
                    }
                    #endregion
                    #region To Pockets
                    else if (to == Groups.Items)
                    {
                        if (slotTo >= pData.Items.Length)
                            return Results.Error;

                        if (pData.Items[slotTo] is Game.Items.Bag)
                            return Results.Error;

                        float curWeight = Game.Items.Items.GetWeight(pData.Items);

                        bool wasDeleted = false, wasCreated = false;

                        #region Unite
                        if (pData.Items[slotTo] != null && pData.Items[slotTo].Type == pData.Bag.Items[slotFrom].Type && pData.Bag.Items[slotFrom] is Game.Items.IStackable)
                        {
                            int slotToAmount = (pData.Items[slotTo] as Game.Items.IStackable).Amount;
                            int slotFromAmount = (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount;

                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            int maxStack = (pData.Bag.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                            if (slotToAmount == maxStack)
                                return Results.Error;

                            // if amount*weight is too big -> reduce amount to fit the pData.Items's maxWeight
                            if (curWeight + amount * pData.Bag.Items[slotFrom].Weight > Settings.MAX_INVENTORY_WEIGHT)
                            {
                                amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / pData.Bag.Items[slotFrom].Weight);

                                if (amount == 0)
                                    return Results.NoSpace;
                            }

                            if (slotToAmount + amount > maxStack)
                            {
                                (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                (pData.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                            }
                            else
                            {
                                (pData.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                if ((pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                {
                                    var tItem = pData.Bag.Items[slotFrom];

                                    Task.Run(() =>
                                    {
                                        tItem.Delete();
                                    });

                                    pData.Bag.Items[slotFrom] = null;

                                    wasDeleted = true;
                                }
                            }

                            pData.Items[slotTo].Update();
                            pData.Bag.Items[slotFrom]?.Update();
                        }
                        #endregion
                        #region Split To New
                        else if (pData.Bag.Items[slotFrom] is Game.Items.IStackable && pData.Items[slotTo] == null && amount != -1 && amount < (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount)
                        {
                            if (pData.Bag.Items[slotFrom].Weight * amount + curWeight > Settings.MAX_INVENTORY_WEIGHT)
                            {
                                amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / pData.Bag.Items[slotFrom].Weight);

                                if (amount == 0)
                                    return Results.NoSpace;
                            }

                            (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                            pData.Bag.Items[slotFrom].Update();

                            pData.Items[slotTo] = await Game.Items.Items.CreateItem(pData.Bag.Items[slotFrom].ID, 0, amount); // but wait for that :)

                            wasCreated = true;
                        }
                        #endregion
                        #region Replace
                        else
                        {
                            var addWeightItems = pData.Items[slotTo] != null ? Game.Items.Items.GetItemWeight(pData.Items[slotTo], true) : 0;
                            var addWeightBag = Game.Items.Items.GetItemWeight(pData.Bag.Items[slotFrom], true);

                            if (addWeightBag - addWeightItems + curWeight > Settings.MAX_INVENTORY_WEIGHT || addWeightItems - addWeightBag + pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            var temp = pData.Bag.Items[slotFrom];
                            pData.Bag.Items[slotFrom] = pData.Items[slotTo];
                            pData.Items[slotTo] = temp;

                            wasDeleted = true; wasCreated = true;
                        }
                        #endregion

                        var upd1 = (pData.Bag.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Bag.Items[slotFrom].ID, Game.Items.Items.GetItemAmount(pData.Bag.Items[slotFrom]), Game.Items.Items.GetItemWeight(pData.Bag.Items[slotFrom]))).SerializeToJson();
                        var upd2 = (pData.Items[slotTo].ID, Game.Items.Items.GetItemAmount(pData.Items[slotTo]), Game.Items.Items.GetItemWeight(pData.Items[slotTo])).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 0, slotTo, upd2);
                        });

                        if (wasCreated)
                            MySQL.UpdatePlayerInventory(pData, true);

                        if (wasDeleted)
                            pData.Bag.Update();

                        return Results.Success;
                    }
                    #endregion
                    #region To Weapons
                    else if (to == Groups.Weapons)
                    {
                        if (slotTo >= pData.Weapons.Length)
                            return Results.Error;

                        bool wasDeleted = false;
                        bool wasReplaced = false;

                        #region Replace
                        if (pData.Bag.Items[slotFrom] is Game.Items.Weapon)
                        {
                            if (pData.Weapons[slotTo] != null && pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight + pData.Weapons[slotTo].Weight - (pData.Bag.Items[slotFrom] as Game.Items.Weapon).Weight > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            var temp = pData.Weapons[slotTo];
                            pData.Weapons[slotTo] = pData.Bag.Items[slotFrom] as Game.Items.Weapon;
                            pData.Bag.Items[slotFrom] = temp;

                            wasReplaced = true;
                        }
                        #endregion
                        #region Load
                        else if (pData.Bag.Items[slotFrom] is Game.Items.Ammo && pData.Weapons[slotTo] != null && pData.Weapons[slotTo].Data.AmmoType != null && (pData.Bag.Items[slotFrom] as Game.Items.Ammo).Type == pData.Weapons[slotTo].Data.AmmoType)
                        {
                            var slotFromAmount = (pData.Bag.Items[slotFrom] as Game.Items.Ammo).Amount;
                            var slotToAmount = pData.Weapons[slotTo].Ammo;

                            var maxAmount = pData.Weapons[slotTo].Data.MaxAmmo;

                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            if (slotToAmount == maxAmount)
                                return Results.Error;

                            if (slotToAmount + amount > maxAmount)
                            {
                                (pData.Bag.Items[slotFrom] as Game.Items.Ammo).Amount -= maxAmount - slotToAmount;
                                pData.Weapons[slotTo].Ammo = maxAmount;
                            }
                            else
                            {
                                pData.Weapons[slotTo].Ammo += amount;
                                (pData.Bag.Items[slotFrom] as Game.Items.Ammo).Amount -= amount;

                                if ((pData.Bag.Items[slotFrom] as Game.Items.Ammo).Amount == 0)
                                {
                                    wasDeleted = true;

                                    var tItem = pData.Bag.Items[slotFrom];

                                    Task.Run(() =>
                                    {
                                        tItem.Delete();
                                    });

                                    pData.Bag.Items[slotFrom] = null;
                                }
                            }

                            pData.Weapons[slotTo].Update();
                            pData.Bag.Items[slotFrom]?.Update();
                        }
                        #endregion
                        else
                            return Results.Error;

                        var upd1 = pData.Bag.Items[slotFrom] == null ? "null" : ((pData.Bag.Items[slotFrom].ID, Game.Items.Items.GetItemAmount(pData.Bag.Items[slotFrom]), Game.Items.Items.GetItemWeight(pData.Bag.Items[slotFrom]))).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotFrom, upd1);

                            if (pData.Bag.Items[slotFrom] != null && pData.Bag.Items[slotFrom] is Game.Items.Weapon)
                            {
                                if ((pData.Bag.Items[slotFrom] as Game.Items.Weapon).Equiped)
                                {
                                    (pData.Bag.Items[slotFrom] as Game.Items.Weapon).Unequip(player);
                                    pData.Weapons[slotTo].Equip(player);
                                }
                                else
                                    (pData.Bag.Items[slotFrom] as Game.Items.Weapon).Unwear(player);
                            }
                            else
                            {
                                pData.Weapons[slotTo].UpdateAmmo(player);

                                pData.Weapons[slotTo].Wear(player);
                            }

                            player.TriggerEvent("Inventory::Update", 2, slotTo, (pData.Weapons[slotTo].ID, pData.Weapons[slotTo].Ammo, pData.Weapons[slotTo].Equiped).SerializeToJson());
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
                    #endregion
                    #region To Clothes
                    else if (to == Groups.Clothes)
                    {
                        if (slotTo >= pData.Clothes.Length)
                            return Results.Error;

                        if (!(pData.Bag.Items[slotFrom] is Game.Items.Clothes))
                            return Results.Error;

                        if (!ClothesSlots.ContainsKey((pData.Bag.Items[slotFrom] as Game.Items.Clothes).Type) || ClothesSlots[(pData.Bag.Items[slotFrom] as Game.Items.Clothes).Type] != slotTo)
                            return Results.Error;

                        if (pData.Clothes[slotTo] != null && pData.Clothes[slotTo].Weight + pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight - pData.Bag.Items[slotFrom].Weight > pData.Bag.Data.MaxWeight)
                            return Results.NoSpace;

                        var temp = pData.Clothes[slotTo];
                        pData.Clothes[slotTo] = pData.Bag.Items[slotFrom] as Game.Items.Clothes;
                        pData.Bag.Items[slotFrom] = temp;

                        var upd1 = (pData.Bag.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Bag.Items[slotFrom].ID, 1, pData.Bag.Items[slotFrom].Weight)).SerializeToJson();
                        var upd2 = (pData.Clothes[slotTo].ID).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 4, slotTo, upd2);

                            temp?.Unwear(player);
                            pData.Clothes[slotTo].Wear(player);
                        });

                        pData.Bag.Update();
                        MySQL.UpdatePlayerInventory(pData, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Accessories
                    else if (to == Groups.Accessories)
                    {
                        if (slotTo >= pData.Accessories.Length)
                            return Results.Error;

                        if (!(pData.Bag.Items[slotFrom] is Game.Items.Clothes))
                            return Results.Error;

                        if (!AccessoriesSlots.ContainsKey((pData.Bag.Items[slotFrom] as Game.Items.Clothes).Type) || AccessoriesSlots[(pData.Bag.Items[slotFrom] as Game.Items.Clothes).Type] != slotTo)
                            return Results.Error;

                        if (pData.Accessories[slotTo] != null && pData.Accessories[slotTo].Weight + pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight - pData.Bag.Items[slotFrom].Weight > pData.Bag.Data.MaxWeight)
                            return Results.NoSpace;

                        var temp = pData.Accessories[slotTo];
                        pData.Accessories[slotTo] = pData.Bag.Items[slotFrom] as Game.Items.Clothes;
                        pData.Bag.Items[slotFrom] = temp;

                        var upd1 = (pData.Bag.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Bag.Items[slotFrom].ID, 1, pData.Bag.Items[slotFrom].Weight)).SerializeToJson();
                        var upd2 = (pData.Accessories[slotTo].ID).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 5, slotTo, upd2);

                            temp?.Unwear(player);
                            pData.Accessories[slotTo].Wear(player);
                        });

                        pData.Bag.Update();
                        MySQL.UpdatePlayerInventory(pData, false, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Holster Item
                    else if (to == Groups.HolsterItem)
                    {
                        if (!(pData.Bag.Items[slotFrom] is Game.Items.Holster))
                            return Results.Error;

                        if (pData.Holster != null && pData.Holster.Weight + pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight - (pData.Bag.Items[slotFrom] as Game.Items.Holster).Weight > pData.Bag.Data.MaxWeight)
                            return Results.NoSpace;

                        var temp = pData.Holster;
                        pData.Holster = pData.Bag.Items[slotFrom] as Game.Items.Holster;
                        pData.Bag.Items[slotFrom] = temp;

                        var upd1 = pData.Bag.Items[slotFrom] == null ? "null" : ((pData.Bag.Items[slotFrom].ID, 1, (pData.Bag.Items[slotFrom] as Game.Items.Holster).Weight)).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotFrom, upd1);

                            (pData.Bag.Items[slotFrom] as Game.Items.Holster)?.Unwear(player);
                            pData.Holster.Wear(player);

                            if (pData.Bag.Items[slotFrom] != null && ((pData.Bag.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Bag.Items[slotFrom] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(player);
                                (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(player);
                            }

                            player.TriggerEvent("Inventory::Update", 7, (pData.Holster.ID, pData.Holster.Items[0] == null ? null : ((string, int, bool)?)(pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, (pData.Holster.Items[0] as Game.Items.Weapon).Equiped)).SerializeToJson());
                        });

                        pData.Bag.Update();
                        MySQL.UpdatePlayerInventory(pData, false, false, false, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Armour
                    else if (to == Groups.Armour)
                    {
                        if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                            return Results.Wounded;

                        if (!(pData.Bag.Items[slotFrom] is Game.Items.BodyArmour))
                            return Results.Error;

                        if (pData.Armour != null && pData.Armour.Weight + pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight - pData.Bag.Items[slotFrom].Weight > pData.Bag.Data.MaxWeight)
                            return Results.NoSpace;

                        var temp = pData.Armour;
                        pData.Armour = pData.Bag.Items[slotFrom] as Game.Items.BodyArmour;
                        pData.Bag.Items[slotFrom] = temp;

                        var upd1 = (pData.Bag.Items[slotFrom] == null ? null : ((string, int, float)?)(pData.Bag.Items[slotFrom].ID, 1, pData.Bag.Items[slotFrom].Weight)).SerializeToJson();
                        var upd2 = ((pData.Armour.ID, pData.Armour.Strength)).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 8, upd2);

                            temp?.Unwear(player);
                            pData.Armour.Wear(player);
                        });

                        pData.Bag.Update();
                        MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Holster
                    else if (to == Groups.Holster)
                    {
                        if (pData.Holster == null)
                            return Results.Error;

                        bool wasDeleted = false;
                        bool wasReplaced = false;

                        #region Replace
                        if (pData.Bag.Items[slotFrom] is Game.Items.Weapon)
                        {
                            if ((pData.Bag.Items[slotFrom] as Game.Items.Weapon).Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                                return Results.PlaceRestricted;

                            if (pData.Holster.Items[0] != null && pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight + (pData.Holster.Items[0] as Game.Items.Weapon).Weight - (pData.Bag.Items[slotFrom] as Game.Items.Weapon).Weight > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            var temp = pData.Holster.Items[0];
                            pData.Holster.Items[0] = pData.Bag.Items[slotFrom] as Game.Items.Weapon;
                            pData.Bag.Items[slotFrom] = temp;

                            pData.Holster.Update();

                            wasReplaced = true;
                        }
                        #endregion
                        #region Load
                        else if (pData.Bag.Items[slotFrom] is Game.Items.Ammo && pData.Holster.Items[0] != null && (pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType != null && (pData.Bag.Items[slotFrom] as Game.Items.Ammo).Type == (pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType)
                        {
                            var slotFromAmount = (pData.Bag.Items[slotFrom] as Game.Items.Ammo).Amount;
                            var slotToAmount = (pData.Holster.Items[0] as Game.Items.Weapon).Ammo;

                            var maxAmount = (pData.Holster.Items[0] as Game.Items.Weapon).Data.MaxAmmo;

                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            if (slotToAmount == maxAmount)
                                return Results.Error;

                            if (slotToAmount + amount > maxAmount)
                            {
                                (pData.Bag.Items[slotFrom] as Game.Items.Ammo).Amount -= maxAmount - slotToAmount;
                                (pData.Holster.Items[0] as Game.Items.Weapon).Ammo = maxAmount;
                            }
                            else
                            {
                                (pData.Holster.Items[0] as Game.Items.Weapon).Ammo += amount;
                                (pData.Bag.Items[slotFrom] as Game.Items.Ammo).Amount -= amount;

                                if ((pData.Bag.Items[slotFrom] as Game.Items.Ammo).Amount == 0)
                                {
                                    wasDeleted = true;

                                    var tItem = pData.Bag.Items[slotFrom];

                                    Task.Run(() =>
                                    {
                                        tItem.Delete();
                                    });

                                    pData.Bag.Items[slotFrom] = null;
                                }
                            }

                            pData.Holster.Items[0].Update();
                            pData.Bag.Items[slotFrom]?.Update();
                        }
                        #endregion
                        else
                            return Results.Error;

                        var upd1 = pData.Bag.Items[slotFrom] == null ? "null" : ((pData.Bag.Items[slotFrom].ID, Game.Items.Items.GetItemAmount(pData.Bag.Items[slotFrom]), Game.Items.Items.GetItemWeight(pData.Bag.Items[slotFrom]))).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotFrom, upd1);

                            if (pData.Bag.Items[slotFrom] != null && pData.Bag.Items[slotFrom] is Game.Items.Weapon)
                            {
                                if ((pData.Bag.Items[slotFrom] as Game.Items.Weapon).Equiped)
                                {
                                    (pData.Bag.Items[slotFrom] as Game.Items.Weapon).Unequip(player);
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Equip(player);
                                }
                                else
                                    (pData.Bag.Items[slotFrom] as Game.Items.Weapon).Unwear(player);
                            }
                            else
                            {
                                (pData.Holster.Items[0] as Game.Items.Weapon).UpdateAmmo(player);

                                (pData.Holster.Items[0] as Game.Items.Weapon).Wear(player);
                            }

                            player.TriggerEvent("Inventory::Update", 3, 2, (pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, (pData.Holster.Items[0] as Game.Items.Weapon).Equiped).SerializeToJson());
                        });

                        if (wasDeleted || wasReplaced)
                            pData.Bag.Update();

                        return Results.Success;
                    }
                    #endregion
                }
                #endregion
                #region From Weapons (To - Pockets, Bag, Weapons, Holster, HolsterItem)
                else if (from == Groups.Weapons)
                {
                    if (slotFrom >= pData.Weapons.Length)
                        return Results.Error;

                    if (pData.Weapons[slotFrom] == null)
                        return Results.Error;

                    #region To Pockets
                    if (to == Groups.Items)
                    {
                        if (slotTo >= pData.Items.Length)
                            return Results.Error;

                        bool wasCreated = false;
                        bool wasReplaced = false;

                        bool extractToExisting = pData.Items[slotTo] != null && pData.Items[slotTo] is Game.Items.Ammo && pData.Items[slotTo].Type == pData.Weapons[slotFrom].Data.AmmoType;

                        #region Extract
                        if (amount != -1 || extractToExisting) // extract ammo from weapon
                        {
                            if (pData.Weapons[slotFrom].IsTemp)
                                return Results.TempItem;

                            if (pData.Weapons[slotFrom].Ammo == 0 || pData.Weapons[slotFrom].Data.AmmoType == null)
                                return Results.Error;

                            Sync.WeaponSystem.UpdateAmmo(pData, pData.Weapons[slotFrom], false);

                            if (amount == -1)
                                amount = pData.Weapons[slotFrom].Ammo;

                            var curWeight = Game.Items.Items.GetWeight(pData.Items);
                            var ammoWeight = Game.Items.Item.GetWeight((Game.Items.Item.Types)pData.Weapons[slotFrom].Data.AmmoType);

                            if (extractToExisting)
                            {
                                int slotToAmount = (pData.Items[slotTo] as Game.Items.Ammo).Amount;
                                int maxStack = (pData.Items[slotTo] as Game.Items.Ammo).MaxAmount;

                                // if amount*weight is too big -> reduce amount to fit the item's maxWeight
                                if (curWeight + amount * ammoWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / ammoWeight);

                                    if (amount == 0)
                                        return Results.NoSpace;
                                }

                                if (slotToAmount + amount > maxStack)
                                {
                                    pData.Weapons[slotFrom].Ammo -= maxStack - slotToAmount;
                                    (pData.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                                }
                                else
                                {
                                    (pData.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                    pData.Weapons[slotFrom].Ammo -= amount;
                                }

                                pData.Weapons[slotFrom].Update();
                                pData.Items[slotTo].Update();
                            }
                            else if (pData.Items[slotTo] == null)
                            {
                                // if amount*weight is too big -> reduce amount to fit the item's maxWeight
                                if (curWeight + amount * ammoWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / ammoWeight);

                                    if (amount == 0)
                                        return Results.NoSpace;
                                }

                                pData.Weapons[slotFrom].Ammo -= amount;
                                pData.Items[slotTo] = await Game.Items.Items.CreateItem(Game.Items.Ammo.IDList.First(x => x.Value == (Game.Items.Item.Types)pData.Weapons[slotFrom].Data.AmmoType).Key, 0, amount);

                                pData.Weapons[slotFrom].Update();

                                wasCreated = true;
                            }
                        }
                        #endregion
                        #region Replace
                        else if (pData.Items[slotTo] == null || pData.Items[slotTo] is Game.Items.Weapon)
                        {
                            if ((Game.Items.Items.GetWeight(pData.Items) + pData.Weapons[slotFrom].Weight - ((pData.Items[slotTo] as Game.Items.Weapon)?.Weight ?? 0)) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            var temp = pData.Items[slotTo];
                            pData.Items[slotTo] = pData.Weapons[slotFrom];
                            pData.Weapons[slotFrom] = temp as Game.Items.Weapon;

                            wasReplaced = true;
                        }
                        #endregion

                        var upd1 = (pData.Items[slotTo].ID, Game.Items.Items.GetItemAmount(pData.Items[slotTo]), Game.Items.Items.GetItemWeight(pData.Items[slotTo])).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotTo, upd1);

                            if (pData.Items[slotTo] is Game.Items.Weapon)
                            {
                                if ((pData.Items[slotTo] as Game.Items.Weapon).Equiped)
                                {
                                    (pData.Items[slotTo] as Game.Items.Weapon).Unequip(player);
                                    pData.Weapons[slotFrom]?.Equip(player);
                                }
                                else
                                    (pData.Items[slotTo] as Game.Items.Weapon).Unwear(player);
                            }
                            else
                            {
                                if (pData.Weapons[slotFrom] != null)
                                {
                                    pData.Weapons[slotFrom].UpdateAmmo(player);

                                    pData.Weapons[slotFrom].Wear(player);
                                }
                            }

                            player.TriggerEvent("Inventory::Update", 2, slotFrom, pData.Weapons[slotFrom] == null ? "null" : ((pData.Weapons[slotFrom].ID, pData.Weapons[slotFrom].Ammo, pData.Weapons[slotFrom].Equiped)).SerializeToJson());
                        });

                        pData.Weapons = pData.Weapons;
                        pData.Items = pData.Items;

                        if (wasCreated)
                            MySQL.UpdatePlayerInventory(pData, true);
                        else if (wasReplaced)
                            MySQL.UpdatePlayerInventory(pData, true, false, false, false, false, true, false);

                        return Results.Success;
                    }
                    #endregion
                    #region To Bag
                    else if (to == Groups.Bag)
                    {
                        if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                            return Results.Error;

                        if (pData.Weapons[slotFrom].IsTemp)
                            return Results.TempItem;

                        bool wasCreated = false;
                        bool wasReplaced = false;

                        bool extractToExisting = pData.Bag.Items[slotTo] != null && pData.Bag.Items[slotTo] is Game.Items.Ammo && pData.Bag.Items[slotTo].Type == pData.Weapons[slotFrom].Data.AmmoType;

                        #region Extract
                        if (amount != -1 || extractToExisting)
                        {
                            if (pData.Weapons[slotFrom].Ammo == 0 || pData.Weapons[slotFrom].Data.AmmoType == null)
                                return Results.Error;

                            Sync.WeaponSystem.UpdateAmmo(pData, pData.Weapons[slotFrom], false);

                            if (amount == -1)
                                amount = pData.Weapons[slotFrom].Ammo;

                            var curWeight = pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight;
                            var maxWeight = pData.Bag.Data.MaxWeight;
                            
                            var ammoWeight = Game.Items.Item.GetWeight((Game.Items.Item.Types)pData.Weapons[slotFrom].Data.AmmoType);

                            if (extractToExisting)
                            {
                                int slotToAmount = (pData.Bag.Items[slotTo] as Game.Items.Ammo).Amount;
                                int maxStack = (pData.Bag.Items[slotTo] as Game.Items.Ammo).MaxAmount;

                                // if amount*weight is too big -> reduce amount to fit the item's maxWeight
                                if (curWeight + amount * ammoWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / ammoWeight);

                                    if (amount == 0)
                                        return Results.NoSpace;
                                }

                                if (slotToAmount + amount > maxStack)
                                {
                                    pData.Weapons[slotFrom].Ammo -= maxStack - slotToAmount;
                                    (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                                }
                                else
                                {
                                    (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                    pData.Weapons[slotFrom].Ammo -= amount;
                                }

                                pData.Weapons[slotFrom].Update();
                                pData.Bag.Items[slotTo].Update();
                            }
                            else if (pData.Bag.Items[slotTo] == null)
                            {
                                // if amount*weight is too big -> reduce amount to fit the item's maxWeight
                                if (curWeight + amount * ammoWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / ammoWeight);

                                    if (amount == 0)
                                        return Results.NoSpace;
                                }

                                pData.Weapons[slotFrom].Ammo -= amount;
                                pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(Game.Items.Ammo.IDList.First(x => x.Value == (Game.Items.Item.Types)pData.Weapons[slotFrom].Data.AmmoType).Key, 0, amount);

                                pData.Weapons[slotFrom].Update();

                                wasCreated = true;
                            }
                        }
                        #endregion
                        #region Replace
                        else if (pData.Bag.Items[slotTo] == null || pData.Bag.Items[slotTo] is Game.Items.Weapon)
                        {
                            if ((pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight + pData.Weapons[slotFrom].Weight - ((pData.Bag.Items[slotTo] as Game.Items.Weapon)?.Weight ?? 0)) > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            var temp = pData.Bag.Items[slotTo];
                            pData.Bag.Items[slotTo] = pData.Weapons[slotFrom];
                            pData.Weapons[slotFrom] = temp as Game.Items.Weapon;

                            wasReplaced = true;
                            wasCreated = true;
                        }
                        #endregion

                        var upd1 = (pData.Bag.Items[slotTo].ID, Game.Items.Items.GetItemAmount(pData.Bag.Items[slotTo]), Game.Items.Items.GetItemWeight(pData.Bag.Items[slotTo])).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotTo, upd1);

                            if (pData.Bag.Items[slotTo] is Game.Items.Weapon)
                            {
                                if ((pData.Bag.Items[slotTo] as Game.Items.Weapon).Equiped)
                                {
                                    (pData.Bag.Items[slotTo] as Game.Items.Weapon).Unequip(player);
                                    pData.Weapons[slotFrom]?.Equip(player);
                                }
                            }
                            else
                            {
                                if (pData.Weapons[slotFrom] != null)
                                {
                                    pData.Weapons[slotFrom].UpdateAmmo(player);

                                    pData.Weapons[slotFrom].Wear(player);
                                }
                            }

                            player.TriggerEvent("Inventory::Update", 2, slotFrom, pData.Weapons[slotFrom] == null ? "null" : ((pData.Weapons[slotFrom].ID, pData.Weapons[slotFrom].Ammo, pData.Weapons[slotFrom].Equiped)).SerializeToJson());
                        });

                        if (wasCreated)
                            pData.Bag.Update();
                        
                        if (wasReplaced)
                            MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);
                    }
                    #endregion
                    #region To Weapons
                    if (to == Groups.Weapons)
                    {
                        var temp = pData.Weapons[slotTo];
                        pData.Weapons[slotTo] = pData.Weapons[slotFrom];
                        pData.Weapons[slotFrom] = temp;

                        var upd1 = pData.Weapons[slotFrom] == null ? "null" : ((pData.Weapons[slotFrom].ID, pData.Weapons[slotFrom].Ammo, false)).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 2, slotFrom, upd1);

                            if (pData.Weapons[slotFrom]?.Equiped == true)
                            {
                                pData.Weapons[slotFrom].Unequip(player);
                                pData.Weapons[slotTo].Equip(player);
                            }

                            player.TriggerEvent("Inventory::Update", 2, slotTo, ((pData.Weapons[slotTo].ID, pData.Weapons[slotTo].Ammo, pData.Weapons[slotTo].Equiped)).SerializeToJson());
                        });

                        MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);
                    }
                    #endregion
                    #region To Holster / Holster Item
                    if (to == Groups.Holster || to == Groups.HolsterItem)
                    {
                        if (pData.Holster == null)
                            return Results.Error;

                        if (pData.Weapons[slotFrom].Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun)
                            return Results.PlaceRestricted;

                        if (pData.Weapons[slotFrom].IsTemp)
                            return Results.TempItem;

                        var temp = pData.Holster.Items[0];
                        pData.Holster.Items[0] = pData.Weapons[slotFrom];
                        pData.Weapons[slotFrom] = temp as Game.Items.Weapon;

                        var upd1 = pData.Weapons[slotFrom] == null ? "null" : ((pData.Weapons[slotFrom].ID, pData.Weapons[slotFrom].Ammo, false)).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 2, slotFrom, upd1);

                            if (pData.Weapons[slotFrom]?.Equiped == true)
                            {
                                pData.Weapons[slotFrom].Unequip(player);
                                (pData.Holster.Items[0] as Game.Items.Weapon).Equip(player);
                            }

                            player.TriggerEvent("Inventory::Update", 3, 2, ((pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, (pData.Holster.Items[0] as Game.Items.Weapon).Equiped)).SerializeToJson());
                        });

                        pData.Holster.Update();

                        MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);

                        return Results.Success;
                    }
                    #endregion
                }
                #endregion
                #region From Holster (To - Pockets, Bag, Weapons)
                else if (from == Groups.Holster)
                {
                    if (pData.Holster == null || pData.Holster.Items[0] == null)
                        return Results.Error;

                    #region To Weapons
                    if (to == Groups.Weapons)
                    {
                        if (slotTo >= pData.Weapons.Length || (pData.Weapons[slotTo] != null && pData.Weapons[slotTo].Data.TopType != Game.Items.Weapon.ItemData.TopTypes.HandGun))
                            return Results.PlaceRestricted;

                        var temp = pData.Holster.Items[0];
                        pData.Holster.Items[0] = pData.Weapons[slotTo];
                        pData.Weapons[slotTo] = temp as Game.Items.Weapon;

                        var upd1 = (pData.Weapons[slotTo].ID, pData.Weapons[slotTo].Ammo, false).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 2, slotTo, upd1);

                            if (pData.Weapons[slotTo].Equiped)
                            {
                                pData.Weapons[slotTo].Unequip(player);
                                (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(player);
                            }

                            player.TriggerEvent("Inventory::Update", 3, 2, pData.Holster.Items[0] == null ? "null" : ((pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, false)).SerializeToJson());
                        });

                        pData.Holster.Update();

                        MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);

                        return Results.Success;
                    }
                    #endregion
                    #region To Pockets
                    else if (to == Groups.Items)
                    {
                        if (slotTo >= pData.Items.Length)
                            return Results.Error;

                        bool extractToExisting = pData.Items[slotTo] != null && pData.Items[slotTo] is Game.Items.Ammo && pData.Items[slotTo].Type == (pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType;

                        bool wasCreated = false;
                        bool wasReplaced = false;

                        #region Extract
                        if (amount != -1 || extractToExisting)
                        {
                            if ((pData.Holster.Items[0] as Game.Items.Weapon).Ammo == 0 || (pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType == null)
                                return Results.Error;

                            Sync.WeaponSystem.UpdateAmmo(pData, (pData.Holster.Items[0] as Game.Items.Weapon), false);

                            if (amount == -1)
                                amount = (pData.Holster.Items[0] as Game.Items.Weapon).Ammo;

                            var curWeight = Game.Items.Items.GetWeight(pData.Items);
                            var ammoWeight = Game.Items.Item.GetWeight((Game.Items.Item.Types)(pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType);

                            if (extractToExisting)
                            {
                                int slotToAmount = (pData.Items[slotTo] as Game.Items.Ammo).Amount;
                                int maxStack = (pData.Items[slotTo] as Game.Items.Ammo).MaxAmount;

                                // if amount*weight is too big -> reduce amount to fit the item's maxWeight
                                if (curWeight + amount * ammoWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / ammoWeight);

                                    if (amount == 0)
                                        return Results.NoSpace;
                                }

                                if (slotToAmount + amount > maxStack)
                                {
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Ammo -= maxStack - slotToAmount;
                                    (pData.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                                }
                                else
                                {
                                    (pData.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Ammo -= amount;
                                }

                                pData.Holster.Items[0].Update();
                                pData.Items[slotTo].Update();
                            }
                            else if (pData.Items[slotTo] == null)
                            {
                                // if amount*weight is too big -> reduce amount to fit the item's maxWeight
                                if (curWeight + amount * ammoWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / ammoWeight);

                                    if (amount == 0)
                                        return Results.NoSpace;
                                }

                                (pData.Holster.Items[0] as Game.Items.Weapon).Ammo -= amount;
                                pData.Items[slotTo] = await Game.Items.Items.CreateItem(Game.Items.Ammo.IDList.First(x => x.Value == (Game.Items.Item.Types)((pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType)).Key, 0, amount);

                                wasCreated = true;

                                pData.Holster.Items[0].Update();
                            }
                        }
                        #endregion
                        #region Replace
                        else if (pData.Items[slotTo] == null || pData.Items[slotTo] is Game.Items.Weapon)
                        {
                            if ((Game.Items.Items.GetWeight(pData.Items) + (pData.Holster.Items[0] as Game.Items.Weapon).Weight - ((pData.Items[slotTo] as Game.Items.Weapon)?.Weight ?? 0)) > Settings.MAX_INVENTORY_WEIGHT)
                                return Results.NoSpace;

                            var temp = pData.Items[slotTo];
                            pData.Items[slotTo] = pData.Holster.Items[0];
                            pData.Holster.Items[0] = temp as Game.Items.Weapon;

                            wasReplaced = true;
                        }
                        #endregion

                        var upd1 = (pData.Items[slotTo].ID, Game.Items.Items.GetItemAmount(pData.Items[slotTo]), Game.Items.Items.GetItemWeight(pData.Items[slotTo])).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotTo, upd1);

                            if (pData.Items[slotTo] is Game.Items.Weapon)
                            {
                                if ((pData.Items[slotTo] as Game.Items.Weapon).Equiped)
                                {
                                    (pData.Items[slotTo] as Game.Items.Weapon).Unequip(player);
                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(player);
                                }
                                else
                                    (pData.Items[slotTo] as Game.Items.Weapon).Unwear(player);
                            }
                            else
                            {
                                if ((pData.Holster.Items[0] as Game.Items.Weapon) != null)
                                {
                                    (pData.Holster.Items[0] as Game.Items.Weapon).UpdateAmmo(player);

                                    (pData.Holster.Items[0] as Game.Items.Weapon).Wear(player);
                                }
                            }

                            player.TriggerEvent("Inventory::Update", 3, 2, pData.Holster.Items[0] == null ? "null" : ((pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, false)).SerializeToJson());
                        });

                        if (wasReplaced)
                        {
                            pData.Holster.Update();

                            MySQL.UpdatePlayerInventory(pData, true, false, false, false, false, false, false);
                        }

                        return Results.Success;
                    }
                    #endregion
                    #region To Bag
                    else if (to == Groups.Bag)
                    {
                        if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                            return Results.Error;

                        bool extractToExisting = pData.Bag.Items[slotTo] != null && pData.Bag.Items[slotTo] is Game.Items.Ammo && pData.Bag.Items[slotTo].Type == (pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType;

                        bool wasCreated = false;
                        bool wasReplaced = false;

                        #region Extract
                        if (amount != -1 || extractToExisting)
                        {
                            if ((pData.Holster.Items[0] as Game.Items.Weapon).Ammo == 0 || (pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType == null)
                                return Results.Error;

                            Sync.WeaponSystem.UpdateAmmo(pData, (pData.Holster.Items[0] as Game.Items.Weapon), false);

                            if (amount == -1)
                                amount = (pData.Holster.Items[0] as Game.Items.Weapon).Ammo;

                            var curWeight = pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight;
                            var maxWeight = pData.Bag.Data.MaxWeight;

                            var ammoWeight = Game.Items.Item.GetWeight((Game.Items.Item.Types)(pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType);

                            if (extractToExisting)
                            {
                                int slotToAmount = (pData.Bag.Items[slotTo] as Game.Items.Ammo).Amount;
                                int maxStack = (pData.Bag.Items[slotTo] as Game.Items.Ammo).MaxAmount;

                                // if amount*weight is too big -> reduce amount to fit the item's maxWeight
                                if (curWeight + amount * ammoWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / ammoWeight);

                                    if (amount == 0)
                                        return Results.NoSpace;
                                }

                                if (slotToAmount + amount > maxStack)
                                {
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Ammo -= maxStack - slotToAmount;
                                    (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                                }
                                else
                                {
                                    (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                    (pData.Holster.Items[0] as Game.Items.Weapon).Ammo -= amount;
                                }

                                pData.Holster.Items[0].Update();
                                pData.Bag.Items[slotTo].Update();
                            }
                            else if (pData.Bag.Items[slotTo] == null)
                            {
                                // if amount*weight is too big -> reduce amount to fit the item's maxWeight
                                if (curWeight + amount * ammoWeight > pData.Bag.Data.MaxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / ammoWeight);

                                    if (amount == 0)
                                        return Results.NoSpace;
                                }

                                (pData.Holster.Items[0] as Game.Items.Weapon).Ammo -= amount;
                                pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(Game.Items.Ammo.IDList.First(x => x.Value == (Game.Items.Item.Types)((pData.Holster.Items[0] as Game.Items.Weapon).Data.AmmoType)).Key, 0, amount);

                                wasCreated = true;

                                pData.Holster.Items[0].Update();
                            }
                        }
                        #endregion
                        #region Replace
                        else if (pData.Bag.Items[slotTo] == null || pData.Bag.Items[slotTo] is Game.Items.Weapon)
                        {
                            if ((pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight + (pData.Holster.Items[0] as Game.Items.Weapon).Weight - ((pData.Bag.Items[slotTo] as Game.Items.Weapon)?.Weight ?? 0)) > pData.Bag.Data.MaxWeight)
                                return Results.NoSpace;

                            var temp = pData.Bag.Items[slotTo];
                            pData.Bag.Items[slotTo] = pData.Holster.Items[0];
                            pData.Holster.Items[0] = temp as Game.Items.Weapon;

                            wasReplaced = true;
                            wasCreated = true;
                        }
                        #endregion

                        var upd1 = (pData.Bag.Items[slotTo].ID, Game.Items.Items.GetItemAmount(pData.Bag.Items[slotTo]), Game.Items.Items.GetItemWeight(pData.Bag.Items[slotTo])).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotTo, upd1);

                            if (pData.Bag.Items[slotTo] is Game.Items.Weapon)
                            {
                                if ((pData.Bag.Items[slotTo] as Game.Items.Weapon).Equiped)
                                {
                                    (pData.Bag.Items[slotTo] as Game.Items.Weapon).Unequip(player);
                                    (pData.Holster.Items[0] as Game.Items.Weapon)?.Equip(player);
                                }
                                else
                                    (pData.Bag.Items[slotTo] as Game.Items.Weapon).Unwear(player);
                            }
                            else
                            {
                                if ((pData.Holster.Items[0] as Game.Items.Weapon) != null)
                                {
                                    (pData.Holster.Items[0] as Game.Items.Weapon).UpdateAmmo(player);

                                    (pData.Holster.Items[0] as Game.Items.Weapon).Wear(player);
                                }
                            }

                            player.TriggerEvent("Inventory::Update", 3, 2, pData.Holster.Items[0] == null ? "null" : ((pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, false)).SerializeToJson());
                        });

                        if (wasReplaced || wasCreated)
                        {
                            pData.Holster.Update();
                            pData.Bag.Update();
                        }

                        return Results.Success;
                    }
                    #endregion
                }
                #endregion
                #region From Armour (To - Pockets, Bag)
                else if (from == Groups.Armour)
                {
                    if (pData.Armour == null)
                        return Results.Error;

                    if (Utils.GetCurrentTime().Subtract(pData.LastDamageTime).TotalMilliseconds < Settings.WOUNDED_USE_TIMEOUT)
                        return Results.Wounded;

                    #region To Pockets
                    if (to == Groups.Items)
                    {
                        if (slotTo >= pData.Items.Length || (pData.Items[slotTo] != null && (!(pData.Items[slotTo] is Game.Items.BodyArmour))))
                            return Results.Error;

                        if (Game.Items.Items.GetWeight(pData.Items) + pData.Armour.Weight - (pData.Items[slotTo]?.Weight ?? 0) > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Armour;
                        pData.Armour = pData.Items[slotTo] as Game.Items.BodyArmour;
                        pData.Items[slotTo] = temp;

                        var upd1 = (pData.Armour == null ? null : ((string, int)?)(pData.Armour.ID, pData.Armour.Strength)).SerializeToJson();
                        var upd2 = (pData.Items[slotTo].ID, 1, pData.Items[slotTo].Weight).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 8, upd1);
                            player.TriggerEvent("Inventory::Update", 0, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.BodyArmour).Unwear(player);
                            pData.Armour?.Wear(player);
                        });

                        MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);

                        return Results.Success;
                    }
                    #endregion
                    #region To Bag
                    else if (to == Groups.Bag)
                    {
                        if (pData.Armour.IsTemp)
                            return Results.TempItem;

                        if (pData.Bag == null)
                            return Results.Error;

                        if (slotTo >= pData.Bag.Items.Length || (pData.Bag.Items[slotTo] != null && (!(pData.Bag.Items[slotTo] is Game.Items.BodyArmour))))
                            return Results.Error;

                        if (pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight + pData.Armour.Weight - (pData.Bag.Items[slotTo]?.Weight ?? 0) > pData.Bag.Data.MaxWeight)
                            return Results.NoSpace;

                        var temp = pData.Armour;
                        pData.Armour = pData.Bag.Items[slotTo] as Game.Items.BodyArmour;
                        pData.Bag.Items[slotTo] = temp;

                        var upd1 = (pData.Armour == null ? null : ((string, int)?)(pData.Armour.ID, pData.Armour.Strength)).SerializeToJson();
                        var upd2 = (pData.Bag.Items[slotTo].ID, 1, pData.Bag.Items[slotTo].Weight).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 8, upd1);
                            player.TriggerEvent("Inventory::Update", 1, slotTo, upd2);

                            (pData.Bag.Items[slotTo] as Game.Items.BodyArmour).Unwear(player);
                            pData.Armour?.Wear(player);
                        });

                        pData.Bag.Update();
                        MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);

                        return Results.Success;
                    }
                    #endregion
                }
                #endregion
                #region From Clothes (To - Pockets, Bag)
                else if (from == Groups.Clothes)
                {
                    if (to != Groups.Bag && to != Groups.Items)
                        return Results.Error;

                    if (slotFrom >= pData.Clothes.Length)
                        return Results.Error;

                    if (pData.Clothes[slotFrom] == null)
                        return Results.Error;

                    #region To Pockets
                    if (to == Groups.Items)
                    {
                        if (slotTo >= pData.Items.Length)
                            return Results.Error;

                        if (pData.Items[slotTo] != null && (!(pData.Items[slotTo] is Game.Items.Clothes) || pData.Items[slotTo].Type != pData.Clothes[slotFrom].Type))
                            return Results.Error;

                        if (Game.Items.Items.GetWeight(pData.Items) + pData.Clothes[slotFrom].Weight - (pData.Items[slotTo]?.Weight ?? 0) > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Items[slotTo];
                        pData.Items[slotTo] = pData.Clothes[slotFrom];
                        pData.Clothes[slotFrom] = temp as Game.Items.Clothes;

                        var upd1 = (pData.Clothes[slotFrom] == null ? null : pData.Clothes[slotFrom].ID).SerializeToJson();
                        var upd2 = (pData.Items[slotTo].ID, 1, pData.Items[slotTo].Weight).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 4, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 0, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.Clothes).Unwear(player);
                            (temp as Game.Items.Clothes)?.Wear(player);
                        });

                        MySQL.UpdatePlayerInventory(pData, true, true, false, false, false, false, false);

                        return Results.Success;
                    }
                    #endregion
                    #region To Bag
                    else
                    {
                        if (pData.Clothes[slotFrom].IsTemp)
                            return Results.TempItem;

                        if (pData.Bag == null)
                            return Results.Error;

                        var curWeight = pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight;
                        var maxWeight = pData.Bag.Data.MaxWeight;

                        if (slotTo >= pData.Bag.Items.Length)
                            return Results.Error;

                        if (pData.Bag.Items[slotTo] != null && (!(pData.Bag.Items[slotTo] is Game.Items.Clothes) || pData.Bag.Items[slotTo].Type != pData.Clothes[slotFrom].Type))
                            return Results.Error;

                        if (curWeight + pData.Clothes[slotFrom].Weight - (pData.Bag.Items[slotTo]?.Weight ?? 0) > maxWeight)
                            return Results.NoSpace;

                        var temp = pData.Bag.Items[slotTo];
                        pData.Bag.Items[slotTo] = pData.Clothes[slotFrom];
                        pData.Clothes[slotFrom] = temp as Game.Items.Clothes;

                        var upd1 = (pData.Clothes[slotFrom] == null ? null : pData.Clothes[slotFrom].ID).SerializeToJson();
                        var upd2 = (pData.Bag.Items[slotTo].ID, 1, pData.Bag.Items[slotTo].Weight).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 4, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 1, slotTo, upd2);

                            (pData.Bag.Items[slotTo] as Game.Items.Clothes).Unwear(player);
                            (temp as Game.Items.Clothes)?.Wear(player);
                        });

                        pData.Bag.Update();
                        MySQL.UpdatePlayerInventory(pData, false, true, false, false, false, false, false);

                        return Results.Success;
                    }
                    #endregion
                }
                #endregion
                #region From Accessories (To - Pockets, Bag)
                else if (from == Groups.Accessories)
                {
                    if (to != Groups.Bag && to != Groups.Items)
                        return Results.Error;

                    if (slotFrom >= pData.Accessories.Length)
                        return Results.Error;

                    if (pData.Accessories[slotFrom] == null)
                        return Results.Error;

                    #region To Pockets
                    if (to == Groups.Items)
                    {
                        if (slotTo >= pData.Items.Length)
                            return Results.Error;

                        if (pData.Items[slotTo] != null && (!(pData.Items[slotTo] is Game.Items.Clothes) || pData.Items[slotTo].Type != pData.Accessories[slotFrom].Type))
                            return Results.Error;

                        if (Game.Items.Items.GetWeight(pData.Items) + pData.Accessories[slotFrom].Weight - (pData.Items[slotTo]?.Weight ?? 0) > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Items[slotTo];
                        pData.Items[slotTo] = pData.Accessories[slotFrom];
                        pData.Accessories[slotFrom] = temp as Game.Items.Clothes;

                        var upd1 = (pData.Accessories[slotFrom] == null ? null : pData.Accessories[slotFrom].ID).SerializeToJson();
                        var upd2 = (pData.Items[slotTo].ID, 1, pData.Items[slotTo].Weight).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 5, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 0, slotTo, upd2);

                            (pData.Items[slotTo] as Game.Items.Clothes).Unwear(player);
                            (temp as Game.Items.Clothes)?.Wear(player);
                        });

                        MySQL.UpdatePlayerInventory(pData, true, false, true, false, false, false, false);

                        return Results.Success;
                    }
                    #endregion
                    #region To Bag
                    else
                    {
                        if (pData.Accessories[slotFrom].IsTemp)
                            return Results.TempItem;

                        if (pData.Bag == null)
                            return Results.Error;

                        var curWeight = pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight;
                        var maxWeight = pData.Bag.Data.MaxWeight;

                        if (slotTo >= pData.Bag.Items.Length)
                            return Results.Error;

                        if (pData.Bag.Items[slotTo] != null && (!(pData.Bag.Items[slotTo] is Game.Items.Clothes) || pData.Bag.Items[slotTo].Type != pData.Accessories[slotFrom].Type))
                            return Results.Error;

                        if (curWeight + pData.Accessories[slotFrom].Weight - (pData.Bag.Items[slotTo]?.Weight ?? 0) > maxWeight)
                            return Results.NoSpace;

                        var temp = pData.Bag.Items[slotTo];
                        pData.Bag.Items[slotTo] = pData.Accessories[slotFrom];
                        pData.Accessories[slotFrom] = temp as Game.Items.Clothes;

                        var upd1 = (pData.Accessories[slotFrom] == null ? null : pData.Accessories[slotFrom].ID).SerializeToJson();
                        var upd2 = (pData.Bag.Items[slotTo].ID, 1, pData.Bag.Items[slotTo].Weight).SerializeToJson();

                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 5, slotFrom, upd1);
                            player.TriggerEvent("Inventory::Update", 1, slotTo, upd2);

                            (pData.Bag.Items[slotTo] as Game.Items.Clothes).Unwear(player);
                            (temp as Game.Items.Clothes)?.Wear(player);
                        });

                        pData.Bag.Update();
                        MySQL.UpdatePlayerInventory(pData, false, false, true, false, false, false, false);

                        return Results.Success;
                    }
                    #endregion
                }
                #endregion
                #region From BagItem (To - Pockets)
                else if (from == Groups.BagItem)
                {
                    if (to != Groups.Items)
                    {
                        if (to == Groups.Bag)
                            return Results.PlaceRestricted;
                        else
                            return Results.Error;
                    }

                    if (pData.Bag == null)
                        return Results.Error;

                    if (slotTo >= pData.Items.Length)
                        return Results.Error;

                    if (!(pData.Items[slotTo] is Game.Items.Bag) && pData.Items[slotTo] != null)
                        return Results.Error;

                    if (pData.Bag.Weight + Game.Items.Items.GetWeight(pData.Items) - ((pData.Items[slotTo] as Game.Items.Bag)?.Weight ?? 0) > Settings.MAX_INVENTORY_WEIGHT)
                        return Results.NoSpace;

                    var temp = pData.Bag;
                    pData.Bag = pData.Items[slotTo] as Game.Items.Bag;
                    pData.Items[slotTo] = temp;

                    var upd1 = (pData.Bag == null ? null : ((string, float, IEnumerable<(string, int, float)?>)?)(pData.Bag.ID, pData.Bag.Data.MaxWeight, pData.Bag.Items.Select(x => x == null ? null : ((string, int, float)?)(x.ID, Game.Items.Items.GetItemAmount(x), Game.Items.Items.GetItemWeight(x))))).SerializeToJson();
                    var upd2 = (pData.Items[slotTo].ID, 1, (pData.Items[slotTo] as Game.Items.Bag).Weight).SerializeToJson();

                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.TriggerEvent("Inventory::Update", 6, upd1);
                        player.TriggerEvent("Inventory::Update", 0, slotTo, upd2);

                        temp.Unwear(player);
                        pData.Bag?.Wear(player);
                    });

                    MySQL.UpdatePlayerInventory(pData, true, false, false, true, false, false, false);

                    return Results.Success;
                }
                #endregion
                #region From Holster Item (To - Pockets, Bag)
                else if (from == Groups.HolsterItem)
                {
                    if (to != Groups.Bag && to != Groups.Items)
                        return Results.Error;

                    if (pData.Holster == null)
                        return Results.Error;

                    #region To Pockets
                    if (to == Groups.Items)
                    {
                        if (slotTo >= pData.Items.Length)
                            return Results.Error;

                        if (!(pData.Items[slotTo] is Game.Items.Holster) && pData.Items[slotTo] != null)
                            return Results.Error;

                        if (pData.Holster.Weight + Game.Items.Items.GetWeight(pData.Items) - ((pData.Items[slotTo] as Game.Items.Holster)?.Weight ?? 0) > Settings.MAX_INVENTORY_WEIGHT)
                            return Results.NoSpace;

                        var temp = pData.Holster;
                        pData.Holster = pData.Items[slotTo] as Game.Items.Holster;
                        pData.Items[slotTo] = temp;

                        var upd1 = (pData.Items[slotTo].ID, 1, (pData.Items[slotTo] as Game.Items.Holster).Weight).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 0, slotTo, upd1);

                            (pData.Items[slotTo] as Game.Items.Holster).Unwear(player);
                            pData.Holster?.Wear(player);

                            if (((pData.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(player);
                                (pData.Holster?.Items[0] as Game.Items.Weapon)?.Equip(player);
                            }

                            player.TriggerEvent("Inventory::Update", 7, (pData.Holster == null ? null : pData.Holster.Items[0] == null ? null : ((string, (string, int, bool))?)(pData.Holster.ID, (pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, false))).SerializeToJson());
                        });

                        MySQL.UpdatePlayerInventory(pData, true, false, false, false, true, false, false);

                        return Results.Success;
                    }
                    #endregion
                    #region To Bag
                    else
                    {
                        if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                            return Results.Error;

                        if (!(pData.Bag.Items[slotTo] is Game.Items.Holster) && pData.Bag.Items[slotTo] != null)
                            return Results.Error;

                        if (pData.Holster.Weight + pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight - ((pData.Bag.Items[slotTo] as Game.Items.Holster)?.Weight) > pData.Bag.Data.MaxWeight)
                            return Results.NoSpace;

                        var temp = pData.Holster;
                        pData.Holster = pData.Bag.Items[slotTo] as Game.Items.Holster;
                        pData.Bag.Items[slotTo] = temp;

                        var upd1 = (pData.Bag.Items[slotTo].ID, 1, (pData.Bag.Items[slotTo] as Game.Items.Holster).Weight).SerializeToJson();

                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.TriggerEvent("Inventory::Update", 1, slotTo, upd1);

                            (pData.Bag.Items[slotTo] as Game.Items.Holster).Unwear(player);
                            pData.Holster?.Wear(player);

                            if (((pData.Bag.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                ((pData.Bag.Items[slotTo] as Game.Items.Holster).Items[0] as Game.Items.Weapon).Unequip(player);
                                (pData.Holster?.Items[0] as Game.Items.Weapon)?.Equip(player);
                            }

                            player.TriggerEvent("Inventory::Update", 7, (pData.Holster == null ? null : pData.Holster.Items[0] == null ? null : ((string, (string, int, bool))?)(pData.Holster.ID, (pData.Holster.Items[0].ID, (pData.Holster.Items[0] as Game.Items.Weapon).Ammo, false))).SerializeToJson());
                        });

                        pData.Bag.Update();
                        MySQL.UpdatePlayerInventory(pData, false, false, false, false, true, false, false);

                        return Results.Success;
                    }
                    #endregion
                }
                #endregion

                // Nothing suitable
                return Results.Error;
            });

            if (res == Results.Success || res == Results.Error)
                return res;

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                if (res == Results.NoSpace)
                    player.Notify("Inventory::NoSpace");
                else if (res == Results.PlaceRestricted)
                    player.Notify("Inventory::PlaceRestricted");
                else if (res == Results.Wounded)
                    player.Notify("Inventory::Wounded");
                else if (res == Results.TempItem)
                    player.Notify("Inventory::ItemIsTemp");
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
                if (to < 0 || to > 8 || from < 0 || to > 8)
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
        /// <param name="slotStr">Группа</param>
        /// <param name="slot">Слот</param>
        /// <param name="action">Действие (минимум - 5)</param>
        public static async Task Action(PlayerData pData, Groups slotStr, int slot, int action)
        {
            if (pData == null)
                return;

            var player = pData.Player;

            await Task.Run(async () =>
            {
                if (action < 5)
                    return;

                #region Pockets & Bag
                if (slotStr == Groups.Items || slotStr == Groups.Bag)
                {
                    Game.Items.Item item = null;

                    if (slotStr == Groups.Items)
                    {
                        var items = pData.Items;

                        if (slot >= items.Length)
                            return;

                        item = items[slot];
                    }
                    else
                    {
                        var items = pData.Bag?.Items;

                        if (items == null || slot >= items.Length)
                            return;

                        item = items[slot];
                    }

                    if (item == null || !(item is Game.Items.IActionable))
                        return;

                    if (item is Game.Items.Clothes citem)
                    {
                        bool isAccs = false;

                        if (AccessoriesSlots.ContainsKey(citem.Type))
                            isAccs = true;

                        await Replace(pData, isAccs ? Groups.Accessories : Groups.Clothes, isAccs ? AccessoriesSlots[item.Type] : ClothesSlots[item.Type], slotStr, slot, -1);

                        return;
                    }
                    else if (item is Game.Items.Bag)
                    {
                        await Replace(pData, Groups.BagItem, 0, slotStr, slot, -1);

                        return;
                    }
                    else if (item is Game.Items.Holster)
                    {
                        await Replace(pData, Groups.HolsterItem, 0, slotStr, slot, -1);

                        return;
                    }
                    else if (item is Game.Items.Weapon)
                    {
                        var weapons = pData.Weapons;

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
                            return;

                        await Replace(pData, newSlot != 2 ? Groups.Weapons : Groups.Holster, newSlot, slotStr, slot, -1);

                        return;
                    }
                    else if (item is Game.Items.BodyArmour)
                    {
                        await Replace(pData, Groups.Armour, 0, slotStr, slot, -1);

                        return;
                    }
                }
                #endregion
                #region Weapons
                else if (slotStr == Groups.Weapons)
                {
                    var weapons = pData.Weapons;
                    var holster = pData.Holster?.Items;

                    if (slot >= weapons.Length)
                        return;

                    if (action != 5 && weapons[slot] == null)
                        return;

                    if (action == 5) // (Un)Equip
                    {
                        await NAPI.Task.RunAsync(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            if (weapons[slot]?.Equiped == true)
                            {
                                weapons[slot].Unequip(player);

                                player.TriggerEvent("Inventory::Update", 2, slot, (weapons[slot].ID, weapons[slot].Ammo, false).SerializeToJson());
                            }
                            else
                            {
                                if (player.Vehicle != null && weapons[slot]?.Data.CanUseInVehicle == false)
                                {
                                    player.Notify("Weapon::InVehicleRestricted");

                                    return;
                                }
                                
                                int idxToCheck = slot == 0 ? 1 : 0;

                                if (weapons[idxToCheck]?.Equiped == true)
                                {
                                    weapons[idxToCheck].Unequip(player);

                                    player.TriggerEvent("Inventory::Update", 2, idxToCheck, (weapons[idxToCheck].ID, weapons[idxToCheck].Ammo, false).SerializeToJson());
                                }
                                else if ((holster?[0] as Game.Items.Weapon)?.Equiped == true)
                                {
                                    (holster[0] as Game.Items.Weapon).Unequip(player);

                                    player.TriggerEvent("Inventory::Update", 3, 2, (holster[0].ID, (holster[0] as Game.Items.Weapon).Ammo, false).SerializeToJson());
                                }

                                if (weapons[slot] != null)
                                {
                                    weapons[slot].Equip(player);

                                    player.TriggerEvent("Inventory::Update", 2, slot, (weapons[slot].ID, weapons[slot].Ammo, true).SerializeToJson());
                                }
                            }
                        });
                    }
                    else if (action == 6) // load
                    {
                        var items = pData.Items;

                        int ammoToFill = weapons[slot].Data.MaxAmmo - weapons[slot].Ammo;

                        if (ammoToFill == 0)
                            return;

                        int ammoIdx = -1;
                        int maxAmmo = 0;

                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i] != null && items[i].Type == weapons[slot].Data.AmmoType && maxAmmo < (items[i] as Game.Items.Ammo).Amount)
                            {
                                ammoIdx = i;
                                maxAmmo = (items[i] as Game.Items.Ammo).Amount;
                            }
                        }

                        if (ammoIdx == -1)
                            return;

                        await Replace(pData, slotStr, slot, Groups.Items, ammoIdx, ammoToFill);
                    }
                }
                #endregion
                #region Holster
                else if (slotStr == Groups.Holster)
                {
                    var weapons = pData.Weapons;
                    var holster = pData.Holster?.Items;

                    if (action != 5 && (holster == null || holster[0] == null))
                        return;

                    if (action == 5) // (Un)Equip
                    {
                        await NAPI.Task.RunAsync(() =>
                        {
                            if ((holster?[0] as Game.Items.Weapon)?.Equiped == true)
                            {
                                (holster[0] as Game.Items.Weapon).Unequip(player);

                                player.TriggerEvent("Inventory::Update", 3, 2, (holster[0].ID, (holster[0] as Game.Items.Weapon).Ammo, false).SerializeToJson());
                            }
                            else
                            {
                                if (weapons[0]?.Equiped == true)
                                {
                                    weapons[0].Unequip(player);

                                    player.TriggerEvent("Inventory::Update", 2, 0, (weapons[0].ID, weapons[0].Ammo, false).SerializeToJson());
                                }
                                else if (weapons[1]?.Equiped == true)
                                {
                                    weapons[1].Unequip(player);

                                    player.TriggerEvent("Inventory::Update", 2, 1, (weapons[1].ID, weapons[1].Ammo, false).SerializeToJson());
                                }

                                if (holster?[0] != null)
                                {
                                    (holster[0] as Game.Items.Weapon).Equip(player);

                                    player.TriggerEvent("Inventory::Update", 3, 2, (holster[0].ID, (holster[0] as Game.Items.Weapon).Ammo, true).SerializeToJson());
                                }
                            }
                        });
                    }
                    else if (action == 6) // load
                    {
                        var items = pData.Items;

                        int ammoToFill = (holster[0] as Game.Items.Weapon).Data.MaxAmmo - (holster[0] as Game.Items.Weapon).Ammo;

                        if (ammoToFill == 0)
                            return;

                        int ammoIdx = -1;
                        int maxAmmo = 0;

                        for (int i = 0; i < items.Length; i++)
                        {
                            if (items[i] != null && items[i].Type == (holster[0] as Game.Items.Weapon).Data.AmmoType && maxAmmo < (items[i] as Game.Items.Ammo).Amount)
                            {
                                ammoIdx = i;
                                maxAmmo = (items[i] as Game.Items.Ammo).Amount;
                            }
                        }

                        if (ammoIdx == -1)
                            return;

                        await Replace(pData, slotStr, slot, Groups.Items, ammoIdx, ammoToFill);
                    }
                }
                #endregion
                #region Clothes & Accessories
                else if (slotStr == Groups.Clothes || slotStr == Groups.Accessories)
                {
                    Game.Items.Clothes item = null;

                    if (slotStr == Groups.Clothes)
                    {
                        if (slot >= pData.Clothes.Length)
                            return;

                        item = pData.Clothes[slot];
                    }
                    else
                    {
                        if (slot >= pData.Accessories.Length)
                            return;

                        item = pData.Accessories[slot];
                    }

                    if (item == null)
                        return;

                    NAPI.Task.Run(() =>
                    {
                        item.Action(player);
                    });
                }
                #endregion
            });
        }

        [RemoteEvent("Inventory::Action")]
        private static async Task ActionRemote(Player player, int slotStr, int slot, int action)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (slotStr < 0 || slotStr > 8)
                    return;

                if (pData.CurrentBusiness != null)
                    return;

                var offer = await pData.ActiveOffer;

                if (offer != null)
                {
                    if (offer.Type == Sync.Offers.Types.Exchange && offer.TradeData != null)
                        return;
                }

                await Action(pData, (Groups)slotStr, slot, action);
            });

            pData.Release();
        }
        #endregion

        #region Drop
        /// <summary>Метод для выбрасывания предмета</summary>
        /// <param name="pData">PlayerData</param>
        /// <param name="slotStr">Группа</param>
        /// <param name="slot">Слот</param>
        /// <param name="amount">Кол-во (минимум - 1, -1 - предмет целиком)</param>
        public static async Task Drop(PlayerData pData, Groups slotStr, int slot, int amount)
        {
            if (pData == null)
                return;

            var player = pData.Player;

            await Task.Run(async () =>
            {
                if (amount < 1 || slot < 0)
                    return;

                Game.Items.Item item = null;

                #region Pockets
                if (slotStr == Groups.Items)
                {
                    var items = pData.Items;

                    if (slot >= items.Length)
                        return;

                    item = items[slot];

                    if (item == null)
                        return;

                    if (item is Game.Items.IStackable)
                    {
                        int curAmount = (item as Game.Items.IStackable).Amount;

                        if (amount > curAmount)
                            amount = curAmount;

                        curAmount -= amount;

                        if (curAmount > 0)
                        {
                            (item as Game.Items.IStackable).Amount = curAmount;
                            items[slot] = item;

                            item.Update();
                            item = await Game.Items.Items.CreateItem(item.ID, 0, amount);
                        }
                        else
                            items[slot] = null;
                    }
                    else
                        items[slot] = null;

                    pData.Items = items;

                    var upd = (items[slot] == null ? "null" : (items[slot].ID, (items[slot] as Game.Items.IStackable).Amount, items[slot].Weight).SerializeToJson());

                    NAPI.Task.Run(() => player.TriggerEvent("Inventory::Update", 0, slot, upd));

                    MySQL.UpdatePlayerInventory(pData, true);
                }
                #endregion
                #region Bag
                else if (slotStr == Groups.Bag)
                {
                    var bag = pData.Bag?.Items;

                    if (slot >= bag.Length || bag == null)
                        return;

                    item = bag[slot];

                    if (item == null)
                        return;

                    if (item is Game.Items.IStackable)
                    {
                        int curAmount = (item as Game.Items.IStackable).Amount;

                        if (amount > curAmount)
                            amount = curAmount;

                        curAmount -= amount;

                        if (curAmount > 0)
                        {
                            (item as Game.Items.IStackable).Amount = curAmount;
                            bag[slot] = item;

                            item.Update();
                            item = await Game.Items.Items.CreateItem(item.ID, 0, amount);
                        }
                        else
                            bag[slot] = null;
                    }
                    else
                        bag[slot] = null;

                    var tempBag = pData.Bag;
                    tempBag.Items = bag;

                    pData.Bag = tempBag;

                    var upd = (bag[slot] == null ? "null" : (bag[slot].ID, (bag[slot] as Game.Items.IStackable).Amount, bag[slot].Weight).SerializeToJson());

                    NAPI.Task.Run(() => player.TriggerEvent("Inventory::Update", 1, slot, upd));

                    tempBag.Update();
                }
                #endregion
                #region Weapons
                else if (slotStr == Groups.Weapons)
                {
                    var weapons = pData.Weapons;

                    if (slot >= weapons.Length)
                        return;

                    item = weapons[slot];

                    if (item == null)
                        return;

                    if ((item as Game.Items.Weapon).Equiped)
                        Sync.WeaponSystem.UpdateAmmo(pData, item as Game.Items.Weapon, false);

                    await NAPI.Task.RunAsync(() =>
                    {
                        player.TriggerEvent("Inventory::Update", 2, slot, "null");

                        if (weapons[slot].Equiped)
                            weapons[slot].Unequip(player, false, false);
                        else
                            weapons[slot].Unwear(player);
                    });

                    weapons[slot] = null;

                    pData.Weapons = weapons;

                    MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, true, false);
                }
                #endregion
                #region Clothes
                else if (slotStr == Groups.Clothes)
                {
                    var clothes = pData.Clothes;

                    if (slot >= clothes.Length)
                        return;

                    item = clothes[slot];

                    if (item == null)
                        return;

                    clothes[slot] = null;

                    pData.Clothes = clothes;

                    MySQL.UpdatePlayerInventory(pData, false, true);

                    await NAPI.Task.RunAsync(() =>
                    {
                        player.TriggerEvent("Inventory::Update", 4, slot, "null");

                        (item as Game.Items.Clothes).Unwear(player);
                    });
                }
                #endregion
                #region Accessories
                else if (slotStr == Groups.Accessories)
                {
                    var accs = pData.Accessories;

                    if (slot >= accs.Length)
                        return;

                    item = accs[slot];

                    if (item == null)
                        return;

                    accs[slot] = null;

                    pData.Accessories = accs;

                    MySQL.UpdatePlayerInventory(pData, false, false, true);

                    await NAPI.Task.RunAsync(() =>
                    {
                        player.TriggerEvent("Inventory::Update", 5, slot, "null");

                        (item as Game.Items.Clothes).Unwear(player);
                    });
                }
                #endregion
                #region Bag Item
                else if (slotStr == Groups.BagItem)
                {
                    item = pData.Bag;

                    if (item == null)
                        return;

                    pData.Bag = null;

                    await NAPI.Task.RunAsync(() =>
                    {
                        player.TriggerEvent("Inventory::Update", 6, "null");

                        (item as Game.Items.Bag).Unwear(player);
                    });

                    MySQL.UpdatePlayerInventory(pData, false, false, false, true);
                }
                #endregion
                #region Armour
                else if (slotStr == Groups.Armour)
                {
                    item = pData.Armour;

                    if (item == null)
                        return;

                    pData.Armour = null;

                    await NAPI.Task.RunAsync(() =>
                    {
                        player.TriggerEvent("Inventory::Update", 8, "null");

                        (item as Game.Items.BodyArmour).Unwear(player);
                    });

                    MySQL.UpdatePlayerInventory(pData, false, false, false, false, false, false, true);
                }
                #endregion
                #region Holster Item
                else if (slotStr == Groups.HolsterItem)
                {
                    item = pData.Holster;

                    if (item == null)
                        return;

                    if ((pData.Holster.Items[0] as Game.Items.Weapon)?.Equiped == true)
                        Sync.WeaponSystem.UpdateAmmo(pData, pData.Holster.Items[0] as Game.Items.Weapon, false);

                    pData.Holster = null;

                    await NAPI.Task.RunAsync(() =>
                    {
                        player.TriggerEvent("Inventory::Update", 7, "null");

                        (item as Game.Items.Holster).Unwear(player);

                        ((item as Game.Items.Holster).Items[0] as Game.Items.Weapon)?.Unequip(player, false, false);
                    });

                    MySQL.UpdatePlayerInventory(pData, false, false, false, false, true);
                }
                #endregion
                #region Holster
                else if (slotStr == Groups.Holster)
                {
                    var holster = pData.Holster?.Items;

                    if (holster == null)
                        return;

                    item = holster[0];

                    if (item == null)
                        return;

                    if ((item as Game.Items.Weapon).Equiped)
                        Sync.WeaponSystem.UpdateAmmo(pData, item as Game.Items.Weapon, false);

                    await NAPI.Task.RunAsync(() =>
                    {
                        player.TriggerEvent("Inventory::Update", 3, 2, "null");

                        if ((holster[0] as Game.Items.Weapon).Equiped)
                            (holster[0] as Game.Items.Weapon).Unequip(player, false, false);
                        else
                            (holster[0] as Game.Items.Weapon).Unwear(player);
                    });

                    holster[0] = null;

                    var tempHolster = pData.Holster;
                    tempHolster.Items = holster;

                    pData.Holster = tempHolster;

                    tempHolster.Update();
                }
                #endregion

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

                    if (!pData.AnyAnimActive())
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
                if (slotStr < 0 || slotStr > 8 || slot < 0)
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

                var curWeight = Game.Items.Items.GetWeight(items);

                var freeIdx = Array.FindIndex(items, x => x == null);

                var curAmount = item.Item is Game.Items.IStackable ? (item.Item as Game.Items.IStackable).Amount : 1;

                if (amount > curAmount)
                    amount = curAmount;

                if (item.Item is Game.Items.IStackable)
                {
                    if (amount > curAmount)
                        amount = curAmount;

                    // if amount*weight is too big -> reduce amount to fit the bag's maxWeight
                    if (curWeight + amount * item.Item.Weight > Settings.MAX_INVENTORY_WEIGHT)
                    {
                        amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / item.Item.Weight);

                        if (amount == 0)
                            return;
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
                    if (curWeight + Game.Items.Items.GetItemWeight(item.Item, false) > Settings.MAX_INVENTORY_WEIGHT)
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

                if (freeIdx == -1)
                    return;

                var result = await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return false;

                    if (!player.AreEntitiesNearby(item.Object, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                        return false;

                    if (!pData.AnyAnimActive())
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


                var upd = (items[freeIdx] == null ? null : ((string, int, float)?)(items[freeIdx].ID, Game.Items.Items.GetItemAmount(items[freeIdx]), Game.Items.Items.GetItemWeight(items[freeIdx]))).SerializeToJson();

                NAPI.Task.Run(() =>
                {
                    player?.TriggerEvent("Inventory::Update", 0, freeIdx, upd);
                });

                MySQL.UpdatePlayerInventory(pData, true);
            });

            Game.World.IOGSemaphore.Release();

            pData.Release();
        }
        #endregion
    }
}
