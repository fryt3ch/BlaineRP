using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Inventory;

namespace BlaineRP.Server.Game.Containers
{
    public partial class Container
    {
        private static Dictionary<GroupTypes, Dictionary<GroupTypes, Func<PlayerData, Container, int, int, int, Inventory.Service.ResultTypes>>> ReplaceActions = new Dictionary<GroupTypes, Dictionary<GroupTypes, Func<PlayerData, Container, int, int, int, Inventory.Service.ResultTypes>>>()
        {
            {
                GroupTypes.Items,

                new Dictionary<GroupTypes, Func<PlayerData, Container, int, int, int, Inventory.Service.ResultTypes>>()
                {
                    {
                        GroupTypes.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Items.Length || slotTo >= cont.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = pData.Items[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = cont.Items[slotTo];

                            if (fromItem.IsTemp)
                                return Inventory.Service.ResultTypes.TempItem;

                            if (!cont.IsItemAllowed(fromItem))
                                return Inventory.Service.ResultTypes.Error;

                            float curWeight = cont.Weight;
                            float maxWeight = cont.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Inventory.Service.ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

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

                                    if (amount == 0)
                                        return Inventory.Service.ResultTypes.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                cont.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                cont.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightBag - addWeightItems + curWeight > maxWeight) || (addWeightItems - addWeightBag + pData.Items.Sum(x => x?.Weight ?? 0f) > Properties.Settings.Static.MAX_INVENTORY_WEIGHT))
                                    return Inventory.Service.ResultTypes.NoSpace;

                                pData.Items[slotFrom] = toItem;
                                cont.Items[slotTo] = fromItem;

                                if (fromItem is Game.Items.IUsable fromItemU && fromItemU.InUse)
                                    fromItemU.StopUse(pData, GroupTypes.Container, slotTo, false);

                                MySQL.CharacterItemsUpdate(pData.Info);
                                cont.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], GroupTypes.Items);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], GroupTypes.Container);

                            player.InventoryUpdate(GroupTypes.Items, slotFrom, upd1);

                            var players = cont.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.Container, slotTo, upd2, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.Bag,

                new Dictionary<GroupTypes, Func<PlayerData, Container, int, int, int, Inventory.Service.ResultTypes>>()
                {
                    {
                        GroupTypes.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length || slotTo >= cont.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = cont.Items[slotTo];

                            if (!cont.IsItemAllowed(fromItem))
                                return Inventory.Service.ResultTypes.Error;

                            float curWeight = cont.Weight;
                            float maxWeight = cont.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Inventory.Service.ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

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
                                if (fromItem.BaseWeight * amount + curWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Inventory.Service.ResultTypes.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                cont.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                cont.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightBag - addWeightItems + curWeight > maxWeight) || (addWeightItems - addWeightBag + pData.Bag.Weight - pData.Bag.BaseWeight > pData.Bag.Data.MaxWeight))
                                    return Inventory.Service.ResultTypes.NoSpace;

                                pData.Bag.Items[slotFrom] = toItem;
                                cont.Items[slotTo] = fromItem;

                                pData.Bag.Update();
                                cont.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], GroupTypes.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], GroupTypes.Container);

                            player.InventoryUpdate(GroupTypes.Bag, slotFrom, upd1);

                            var players = cont.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.Container, slotTo, upd2, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },
                }
            },

            {
                GroupTypes.Container,

                new Dictionary<GroupTypes, Func<PlayerData, Container, int, int, int, Inventory.Service.ResultTypes>>()
                {
                    {
                        GroupTypes.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            if (slotTo >= cont.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = cont.Items[slotTo];

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

                                        cont.Items[slotFrom] = null;

                                        cont.Update();
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

                                cont.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount);

                                cont.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                cont.Items[slotFrom] = toItem;
                                cont.Items[slotTo] = fromItem;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], GroupTypes.Container);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], GroupTypes.Container);

                            var players = cont.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.Container, slotFrom, upd1, GroupTypes.Container, slotTo, upd2, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Items,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            if (slotTo >= pData.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = pData.Items[slotTo];

                            if (!cont.IsItemAllowed(toItem))
                                return Inventory.Service.ResultTypes.Error;

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

                                        cont.Items[slotFrom] = null;

                                        cont.Update();
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

                                pData.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightBag - addWeightItems + curWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT) || (addWeightItems - addWeightBag + cont.Weight > cont.MaxWeight))
                                    return Inventory.Service.ResultTypes.NoSpace;

                                cont.Items[slotFrom] = toItem;
                                pData.Items[slotTo] = fromItem;

                                cont.Update();
                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], GroupTypes.Container);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], GroupTypes.Items);

                            player.InventoryUpdate(GroupTypes.Items, slotTo, upd2);

                            var players = cont.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.Container, slotFrom, upd1, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },

                    {
                        GroupTypes.Bag,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return Inventory.Service.ResultTypes.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return Inventory.Service.ResultTypes.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (!cont.IsItemAllowed(toItem))
                                return Inventory.Service.ResultTypes.Error;

                            float curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            float maxWeight = pData.Bag.Data.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Inventory.Service.ResultTypes.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

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

                                        cont.Items[slotFrom] = null;

                                        cont.Update();
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
                                        return Inventory.Service.ResultTypes.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Bag.Items[slotTo] = Game.Items.Stuff.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                pData.Bag.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightBag - addWeightItems + curWeight > maxWeight) || (addWeightItems - addWeightBag + cont.Weight > cont.MaxWeight))
                                    return Inventory.Service.ResultTypes.NoSpace;

                                cont.Items[slotFrom] = toItem;
                                pData.Bag.Items[slotTo] = fromItem;

                                cont.Update();
                                pData.Bag.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], GroupTypes.Container);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], GroupTypes.Bag);

                            player.InventoryUpdate(GroupTypes.Bag, slotTo, upd2);

                            var players = cont.GetPlayersObservingArray();

                            if (players.Length > 0)
                                Utils.InventoryUpdate(GroupTypes.Container, slotFrom, upd1, players);

                            return Inventory.Service.ResultTypes.Success;
                        }
                    },
                }
            },
        };

        public static Func<PlayerData, Container, int, int, int, Inventory.Service.ResultTypes> GetReplaceAction(GroupTypes from, GroupTypes to) => ReplaceActions.GetValueOrDefault(from)?.GetValueOrDefault(to);
    }
}
