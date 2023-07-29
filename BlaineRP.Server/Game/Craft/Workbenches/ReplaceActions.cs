using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Items;

namespace BlaineRP.Server.Game.Craft.Workbenches
{
    public abstract partial class Workbench
    {
        private static Dictionary<GroupTypes, Dictionary<GroupTypes, Func<PlayerData, Workbenches.Workbench, int, int, int, Inventory.Service.ResultTypes>>> ReplaceActions = new Dictionary<GroupTypes, Dictionary<GroupTypes, Func<PlayerData, Workbenches.Workbench, int, int, int, Inventory.Service.ResultTypes>>>()
        {
            {
                GroupTypes.Items,

                new Dictionary<GroupTypes, Func<PlayerData, Workbenches.Workbench, int, int, int, Inventory.Service.ResultTypes>>()
                {
                    {
                        GroupTypes.CraftItems,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Items.Length || slotTo >= wb.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = pData.Items[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = wb.Items[slotTo];

                            if (fromItem.IsTemp)
                                return Inventory.Service.ResultTypes.TempItem;

                            if (toItem is WorkbenchTool)
                                return Inventory.Service.ResultTypes.Error;

/*                            if (!wb.IsItemAllowed(fromItem))
                                return Game.Items.Inventory.Results.Error;*/

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Inventory.Service.ResultTypes.Error;

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
                            #region Split To New
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount)
                            {
                                targetItem.Amount -= amount;
                                fromItem.Update();

                                wb.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightItems - addWeightBag + pData.Items.Sum(x => x?.Weight ?? 0f) > Properties.Settings.Static.MAX_INVENTORY_WEIGHT))
                                    return Inventory.Service.ResultTypes.NoSpace;

                                pData.Items[slotFrom] = toItem;
                                wb.Items[slotTo] = fromItem;

                                if (fromItem is Game.Items.IUsable fromItemU && fromItemU.InUse)
                                    fromItemU.StopUse(pData, GroupTypes.CraftItems, slotTo, false);

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items);
                            var upd2 = Game.Items.Item.ToClientJson(wb.Items[slotTo], GroupTypes.CraftItems);

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, upd1);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.CraftItems, slotTo, upd2, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    }
                }
            },

            {
                GroupTypes.CraftItems,

                new Dictionary<GroupTypes, Func<PlayerData, Workbenches.Workbench, int, int, int, Inventory.Service.ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= wb.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = wb.Items[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (fromItem is WorkbenchTool)
                                return Inventory.Service.ResultTypes.Error;

/*                            if (!wb.IsItemAllowed(toItem))
                                return Game.Items.Inventory.Results.Error;*/

                            float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Inventory.Service.ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Properties.Settings.Static.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Inventory.Service.ResultTypes.NoSpace;
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

                                        wb.Items[slotFrom] = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount)
                            {
                                if (fromItem.BaseWeight * amount + curWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Properties.Settings.Static.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Inventory.Service.ResultTypes.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightBag - addWeightItems + curWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT))
                                    return Inventory.Service.ResultTypes.NoSpace;

                                wb.Items[slotFrom] = toItem;
                                pData.Items[slotTo] = fromItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(wb.Items[slotFrom], GroupTypes.CraftItems);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items);

                            player.InventoryUpdate(GroupTypes.Items, slotTo, upd2);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.CraftItems, slotFrom, upd1, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.CraftItems,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= wb.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = wb.Items[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            if (slotTo >= wb.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = wb.Items[slotTo];

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Inventory.Service.ResultTypes.Error;

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

                                        wb.Items[slotFrom] = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount)
                            {
                                targetItem.Amount -= amount;
                                fromItem.Update();

                                wb.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                wb.Items[slotFrom] = toItem;
                                wb.Items[slotTo] = fromItem;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(wb.Items[slotFrom], GroupTypes.CraftItems);
                            var upd2 = Game.Items.Item.ToClientJson(wb.Items[slotTo], GroupTypes.CraftItems);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.CraftItems, slotTo, upd2, GroupTypes.CraftItems, slotFrom, upd1, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.CraftTools,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= wb.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = wb.Items[slotFrom] as WorkbenchTool;

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            var wbData = wb.StaticData;

                            if (slotTo >= wbData.Tools.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = wbData.Tools[slotTo];

                            if (toItem == null || toItem.ID == fromItem.ID || wb.Items.Where(x => x?.ID == toItem.ID).Any())
                                wb.Items[slotFrom] = null;
                            else
                                wb.Items[slotFrom] = toItem;

                            var upd1 = Game.Items.Item.ToClientJson(wb.Items[slotFrom], GroupTypes.CraftItems);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.CraftItems, slotFrom, upd1, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    }
                }
            },

            {
                GroupTypes.CraftTools,

                new Dictionary<GroupTypes, Func<PlayerData, Workbenches.Workbench, int, int, int, Inventory.Service.ResultTypes>>()
                {
                    {
                        GroupTypes.CraftItems,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var wbData = wb.StaticData;

                            if (slotFrom >= wbData.Tools.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = wbData.Tools[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            if (slotTo >= wb.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = wb.Items[slotTo] as WorkbenchTool;

                            if (wb.Items[slotTo] != null && toItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            wb.Items[slotTo] = fromItem;

                            var upd1 = Game.Items.Item.ToClientJson(wb.Items[slotTo], GroupTypes.CraftItems);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.CraftItems, slotTo, upd1, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    }
                }
            },

            {
                GroupTypes.CraftResult,

                new Dictionary<GroupTypes, Func<PlayerData, Workbenches.Workbench, int, int, int, Inventory.Service.ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom > 0)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = wb.ResultItem;

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

/*                            if (!wb.IsItemAllowed(toItem))
                                return Game.Items.Inventory.Results.Error;*/

                            float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Inventory.Service.ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Properties.Settings.Static.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Inventory.Service.ResultTypes.NoSpace;
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

                                        wb.ResultItem = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount)
                            {
                                if (fromItem.BaseWeight * amount + curWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Properties.Settings.Static.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Inventory.Service.ResultTypes.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion
                            else
                            {
                                if (pData.Items[slotTo] != null)
                                    return Inventory.Service.ResultTypes.Error;

                                pData.Items[slotTo] = fromItem;
                                wb.ResultItem = null;

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }

                            var upd1 = Game.Items.Item.ToClientJson(wb.ResultItem, GroupTypes.CraftResult);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items);

                            player.InventoryUpdate(GroupTypes.Items, slotTo, upd2);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.CraftResult, upd1, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.CraftItems,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom > 0)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = wb.ResultItem;

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            if (slotTo >= wb.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = wb.Items[slotTo];

                            if (toItem is WorkbenchTool)
                                return Inventory.Service.ResultTypes.Error;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Inventory.Service.ResultTypes.Error;

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

                                        wb.ResultItem = null;
                                    }
                                }

                                toItem.Update();
                                fromItem?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (fromItem is Game.Items.IStackable targetItem && toItem == null && amount != -1 && amount < targetItem.Amount)
                            {
                                targetItem.Amount -= amount;
                                fromItem.Update();

                                wb.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);
                            }
                            #endregion
                            else
                            {
                                if (pData.Items[slotTo] != null)
                                    return Inventory.Service.ResultTypes.Error;

                                pData.Items[slotTo] = fromItem;
                                wb.ResultItem = null;

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }

                            var upd1 = Game.Items.Item.ToClientJson(wb.ResultItem, GroupTypes.CraftResult);
                            var upd2 = Game.Items.Item.ToClientJson(wb.Items[slotTo], GroupTypes.CraftItems);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.CraftResult, 0, upd1, GroupTypes.CraftItems, slotTo, upd2, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },
                }
            },
        };

        public static Func<PlayerData, Workbenches.Workbench, int, int, int, Inventory.Service.ResultTypes> GetReplaceAction(GroupTypes from, GroupTypes to) => ReplaceActions.GetValueOrDefault(from)?.GetValueOrDefault(to);
    }
}
