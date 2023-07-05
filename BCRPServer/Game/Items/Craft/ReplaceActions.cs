using System;
using System.Collections.Generic;
using System.Linq;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Game.Items.Craft
{
    public abstract partial class Workbench
    {
        private static Dictionary<GroupTypes, Dictionary<GroupTypes, Func<PlayerData, Workbench, int, int, int, ResultTypes>>> ReplaceActions = new Dictionary<GroupTypes, Dictionary<GroupTypes, Func<PlayerData, Workbench, int, int, int, ResultTypes>>>()
        {
            {
                GroupTypes.Items,

                new Dictionary<GroupTypes, Func<PlayerData, Workbench, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.CraftItems,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Items.Length || slotTo >= wb.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var fromItem = pData.Items[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var toItem = wb.Items[slotTo];

                            if (fromItem.IsTemp)
                                return Game.Items.Inventory.ResultTypes.TempItem;

                            if (toItem is WorkbenchTool)
                                return ResultTypes.Error;

/*                            if (!wb.IsItemAllowed(fromItem))
                                return Game.Items.Inventory.Results.Error;*/

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.ResultTypes.Error;

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

                                if ((addWeightItems - addWeightBag + pData.Items.Sum(x => x?.Weight ?? 0f) > Settings.MAX_INVENTORY_WEIGHT))
                                    return Game.Items.Inventory.ResultTypes.NoSpace;

                                pData.Items[slotFrom] = toItem;
                                wb.Items[slotTo] = fromItem;

                                if (fromItem is Game.Items.IUsable fromItemU && fromItemU.InUse)
                                    fromItemU.StopUse(pData, Game.Items.Inventory.GroupTypes.CraftItems, slotTo, false);

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Game.Items.Inventory.GroupTypes.Items);
                            var upd2 = Game.Items.Item.ToClientJson(wb.Items[slotTo], Game.Items.Inventory.GroupTypes.CraftItems);

                            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, slotFrom, upd1);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(Game.Items.Inventory.GroupTypes.CraftItems, slotTo, upd2, players);

                            return Game.Items.Inventory.ResultTypes.Success;
                        }
                    }
                }
            },

            {
                GroupTypes.CraftItems,

                new Dictionary<GroupTypes, Func<PlayerData, Workbench, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= wb.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var fromItem = wb.Items[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (fromItem is WorkbenchTool)
                                return ResultTypes.Error;

/*                            if (!wb.IsItemAllowed(toItem))
                                return Game.Items.Inventory.Results.Error;*/

                            float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.ResultTypes.NoSpace;
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
                                if (fromItem.BaseWeight * amount + curWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.ResultTypes.NoSpace;
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

                                if ((addWeightBag - addWeightItems + curWeight > Settings.MAX_INVENTORY_WEIGHT))
                                    return Game.Items.Inventory.ResultTypes.NoSpace;

                                wb.Items[slotFrom] = toItem;
                                pData.Items[slotTo] = fromItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(wb.Items[slotFrom], Game.Items.Inventory.GroupTypes.CraftItems);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Game.Items.Inventory.GroupTypes.Items);

                            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, slotTo, upd2);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(Game.Items.Inventory.GroupTypes.CraftItems, slotFrom, upd1, players);

                            return Game.Items.Inventory.ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.CraftItems,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= wb.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var fromItem = wb.Items[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.ResultTypes.Error;

                            if (slotTo >= wb.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var toItem = wb.Items[slotTo];

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.ResultTypes.Error;

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

                            var upd1 = Game.Items.Item.ToClientJson(wb.Items[slotFrom], Game.Items.Inventory.GroupTypes.CraftItems);
                            var upd2 = Game.Items.Item.ToClientJson(wb.Items[slotTo], Game.Items.Inventory.GroupTypes.CraftItems);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(Game.Items.Inventory.GroupTypes.CraftItems, slotTo, upd2, Game.Items.Inventory.GroupTypes.CraftItems, slotFrom, upd1, players);

                            return Game.Items.Inventory.ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.CraftTools,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= wb.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var fromItem = wb.Items[slotFrom] as WorkbenchTool;

                            if (fromItem == null)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var wbData = wb.StaticData;

                            if (slotTo >= wbData.Tools.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var toItem = wbData.Tools[slotTo];

                            if (toItem == null || toItem.ID == fromItem.ID || wb.Items.Where(x => x?.ID == toItem.ID).Any())
                                wb.Items[slotFrom] = null;
                            else
                                wb.Items[slotFrom] = toItem;

                            var upd1 = Game.Items.Item.ToClientJson(wb.Items[slotFrom], Game.Items.Inventory.GroupTypes.CraftItems);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(Game.Items.Inventory.GroupTypes.CraftItems, slotFrom, upd1, players);

                            return Game.Items.Inventory.ResultTypes.Success;
                        }
                    }
                }
            },

            {
                GroupTypes.CraftTools,

                new Dictionary<GroupTypes, Func<PlayerData, Workbench, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.CraftItems,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            var wbData = wb.StaticData;

                            if (slotFrom >= wbData.Tools.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var fromItem = wbData.Tools[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.ResultTypes.Error;

                            if (slotTo >= wb.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var toItem = wb.Items[slotTo] as WorkbenchTool;

                            if (wb.Items[slotTo] != null && toItem == null)
                                return Game.Items.Inventory.ResultTypes.Error;

                            wb.Items[slotTo] = fromItem;

                            var upd1 = Game.Items.Item.ToClientJson(wb.Items[slotTo], Game.Items.Inventory.GroupTypes.CraftItems);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(Game.Items.Inventory.GroupTypes.CraftItems, slotTo, upd1, players);

                            return Game.Items.Inventory.ResultTypes.Success;
                        }
                    }
                }
            },

            {
                GroupTypes.CraftResult,

                new Dictionary<GroupTypes, Func<PlayerData, Workbench, int, int, int, ResultTypes>>()
                {
                    {
                        GroupTypes.Items,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom > 0)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var fromItem = wb.ResultItem;

                            if (fromItem == null)
                                return Game.Items.Inventory.ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

/*                            if (!wb.IsItemAllowed(toItem))
                                return Game.Items.Inventory.Results.Error;*/

                            float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.ResultTypes.NoSpace;
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
                                if (fromItem.BaseWeight * amount + curWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.ResultTypes.NoSpace;
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
                                    return ResultTypes.Error;

                                pData.Items[slotTo] = fromItem;
                                wb.ResultItem = null;

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }

                            var upd1 = Game.Items.Item.ToClientJson(wb.ResultItem, Game.Items.Inventory.GroupTypes.CraftResult);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Game.Items.Inventory.GroupTypes.Items);

                            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, slotTo, upd2);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(Game.Items.Inventory.GroupTypes.CraftResult, upd1, players);

                            return Game.Items.Inventory.ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.CraftItems,

                        (pData, wb, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom > 0)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var fromItem = wb.ResultItem;

                            if (fromItem == null)
                                return Game.Items.Inventory.ResultTypes.Error;

                            if (slotTo >= wb.Items.Length)
                                return Game.Items.Inventory.ResultTypes.Error;

                            var toItem = wb.Items[slotTo];

                            if (toItem is WorkbenchTool)
                                return ResultTypes.Error;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.ResultTypes.Error;

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
                                    return ResultTypes.Error;

                                pData.Items[slotTo] = fromItem;
                                wb.ResultItem = null;

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }

                            var upd1 = Game.Items.Item.ToClientJson(wb.ResultItem, Game.Items.Inventory.GroupTypes.CraftResult);
                            var upd2 = Game.Items.Item.ToClientJson(wb.Items[slotTo], Game.Items.Inventory.GroupTypes.CraftItems);

                            var players = wb.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(Game.Items.Inventory.GroupTypes.CraftResult, 0, upd1, Game.Items.Inventory.GroupTypes.CraftItems, slotTo, upd2, players);

                            return Game.Items.Inventory.ResultTypes.Success;
                        }
                    },
                }
            },
        };

        public static Func<PlayerData, Workbench, int, int, int, Game.Items.Inventory.ResultTypes> GetReplaceAction(Game.Items.Inventory.GroupTypes from, Game.Items.Inventory.GroupTypes to) => ReplaceActions.GetValueOrDefault(from)?.GetValueOrDefault(to);
    }
}
