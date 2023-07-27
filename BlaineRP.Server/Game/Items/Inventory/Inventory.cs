using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Animations;

namespace BlaineRP.Server.Game.Items
{
    public partial class Inventory
    {
        #region Properties.Settings.Static.
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
            { typeof(Game.Items.Mask), 1 },
            { typeof(Game.Items.Earrings), 2 },
            { typeof(Game.Items.Accessory), 3 },
            { typeof(Game.Items.Watches), 4 },
            { typeof(Game.Items.Bracelet), 5 },
            { typeof(Game.Items.Ring), 6 },
            { typeof(Game.Items.Gloves), 7 },
        };

        /// <summary>Секции инвентаря</summary>
        public enum GroupTypes
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
            Container = 9,

            CraftItems = 20,
            CraftTools = 21,
            CraftResult = 22,
        }

        /// <summary>Типы результатов</summary>
        public enum ResultTypes
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
            NotEnoughGarageSlots,
            NotEnoughApartmentsSlots,
            NotEnoughBusinessSlots,

            SettledToHouse,
            SettledToApartments,

            NoBusinessLicense,
        }

        public static Dictionary<ResultTypes, string> ResultsNotifications = new Dictionary<ResultTypes, string>()
        {
            { ResultTypes.NoSpace, "Inventory::NoSpace" },
            { ResultTypes.PlaceRestricted, "Inventory::PlaceRestricted" },
            { ResultTypes.Wounded, "Inventory::Wounded" },
            { ResultTypes.TempItem, "Inventory::ItemIsTemp" },
        };
        #endregion

        private static Dictionary<GroupTypes, Func<PlayerData, int, Game.Items.Item>> PlayerDataGroups = new Dictionary<GroupTypes, Func<PlayerData, int, Game.Items.Item>>
        {
            { GroupTypes.Items, (pData, slot) => pData.Items.Length <= slot ? null : pData.Items[slot] },

            { GroupTypes.Bag, (pData, slot) => pData.Bag == null ? null : pData.Bag.Items.Length <= slot ? null : pData.Bag.Items[slot] },

            { GroupTypes.Weapons, (pData, slot) =>  pData.Weapons.Length <= slot ? null : pData.Weapons[slot] },

            { GroupTypes.Holster, (pData, slot) =>  pData.Holster == null ? null : pData.Holster.Items[0] },

            { GroupTypes.Armour, (pData, slot) =>  pData.Armour },

            { GroupTypes.BagItem, (pData, slot) =>  pData.Bag },

            { GroupTypes.HolsterItem, (pData, slot) =>  pData.Holster },

            { GroupTypes.Clothes, (pData, slot) =>  pData.Clothes.Length <= slot ? null : pData.Clothes[slot] },

            { GroupTypes.Accessories, (pData, slot) =>  pData.Accessories.Length <= slot ? null : pData.Accessories[slot] },
        };

        public static Game.Items.Item GetPlayerItem(PlayerData pData, GroupTypes group, int slot = 0)
        {
            var res = PlayerDataGroups.GetValueOrDefault(group);

            return res == null ? null : res.Invoke(pData, slot);
        }

        #region Replace
        /// <summary>Метод для перемещения/замены предметов в инвентаре игрока</summary>
        /// <param name="pData">PlayerData</param>
        /// <param name="to">Группа, куда переместить</param>
        /// <param name="slotTo">Слот, в который переместить</param>
        /// <param name="from">Группа, откуда переместить</param>
        /// <param name="slotFrom">Слот, откуда переместить</param>
        /// <param name="amount">Кол-во для перемещения (-1 - предмет целиком)</param>
        public static ResultTypes Replace(PlayerData pData, GroupTypes to, int slotTo, GroupTypes from, int slotFrom, int amount)
        {
            var player = pData.Player;

            if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                return ResultTypes.Error;

            var action = ReplaceActions.GetValueOrDefault(from)?.GetValueOrDefault(to);

            var res = ResultTypes.Error;

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
        public static ResultTypes Action(PlayerData pData, GroupTypes group, int slot, int action = 5, params string[] args)
        {
            var player = pData.Player;

            var item = GetPlayerItem(pData, group, slot);

            if (item == null)
                return ResultTypes.Error;

            var func = Actions.Where(x => x.Key == item.Type || x.Key.IsAssignableFrom(item.Type)).Select(x => x.Value).FirstOrDefault()?.GetValueOrDefault(action);

            if (func == null)
                return ResultTypes.Error;

            return func.Invoke(pData, item, group, slot, args);
        }
        #endregion

        #region Drop
        /// <summary>Метод для выбрасывания предмета</summary>
        /// <param name="pData">PlayerData</param>
        /// <param name="group">Группа</param>
        /// <param name="slot">Слот</param>
        /// <param name="amount">Кол-во (минимум - 1, -1 - предмет целиком)</param>
        public static void Drop(PlayerData pData, GroupTypes group, int slot, int amount)
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

            if (pData.CanPlayAnimNow())
                pData.PlayAnim(FastType.Putdown, Properties.Settings.Static.InventoryPutdownAnimationTime);

            World.Service.AddItemOnGround(pData, item, player.GetFrontOf(0.6f), player.Rotation, player.Dimension);
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

                if (curWeight + amount * item.BaseWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                {
                    amount = (int)Math.Floor((Properties.Settings.Static.MAX_INVENTORY_WEIGHT - curWeight) / item.BaseWeight);
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

                if (curWeight + item.Weight <= Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
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
                    pData.Items[freeIdx] = Game.Items.Stuff.CreateItem(item.ID, 0, amount, false);
                }
            }

            var upd = Game.Items.Item.ToClientJson(pData.Items[freeIdx], GroupTypes.Items);

            if (notifyOnSuccess)
                pData.Player.TriggerEvent("Item::Added", item.ID, amount);

            pData.Player.InventoryUpdate(GroupTypes.Items, freeIdx, upd);

            MySQL.CharacterItemsUpdate(pData.Info);

            return true;
        }

        public static void ClearSlot(PlayerData pData, GroupTypes group, int slot) => pData.Player.InventoryUpdate(group, slot, Game.Items.Item.ToClientJson(null, Game.Items.Inventory.GroupTypes.Items));
    }
}
