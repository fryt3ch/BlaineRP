using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Game.Items
{
    public partial class Container
    {
        private static Dictionary<Game.Items.Inventory.Groups, Dictionary<Game.Items.Inventory.Groups, Func<PlayerData, Container, int, int, int, Game.Items.Inventory.Results>>> ReplaceActions = new Dictionary<Game.Items.Inventory.Groups, Dictionary<Game.Items.Inventory.Groups, Func<PlayerData, Container, int, int, int, Game.Items.Inventory.Results>>>()
        {
            {
                Game.Items.Inventory.Groups.Items,

                new Dictionary<Game.Items.Inventory.Groups, Func<PlayerData, Container, int, int, int, Game.Items.Inventory.Results>>()
                {
                    {
                        Game.Items.Inventory.Groups.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Items.Length || slotTo >= cont.Items.Length)
                                return Game.Items.Inventory.Results.Error;

                            var fromItem = pData.Items[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.Results.Error;

                            var toItem = cont.Items[slotTo];

                            if (fromItem.IsTemp)
                                return Game.Items.Inventory.Results.TempItem;

                            if (!cont.IsItemAllowed(fromItem))
                                return Game.Items.Inventory.Results.Error;

                            float curWeight = cont.Weight;
                            float maxWeight = cont.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.Results.NoSpace;
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
                                        return Game.Items.Inventory.Results.NoSpace;
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

                                if ((addWeightBag - addWeightItems + curWeight > maxWeight) || (addWeightItems - addWeightBag + pData.Items.Sum(x => x?.Weight ?? 0f) > Settings.MAX_INVENTORY_WEIGHT))
                                    return Game.Items.Inventory.Results.NoSpace;

                                pData.Items[slotFrom] = toItem;
                                cont.Items[slotTo] = fromItem;

                                if (fromItem is Game.Items.IUsable fromItemU && fromItemU.InUse)
                                    fromItemU.StopUse(pData, Game.Items.Inventory.Groups.Container, slotTo, false);

                                MySQL.CharacterItemsUpdate(pData.Info);
                                cont.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], Game.Items.Inventory.Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], Game.Items.Inventory.Groups.Container);

                            player.InventoryUpdate(Game.Items.Inventory.Groups.Items, slotFrom, upd1);

                            foreach (var x in cont.PlayersObserving)
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.InventoryUpdate(Game.Items.Inventory.Groups.Container, slotTo, upd2);
                            }

                            return Game.Items.Inventory.Results.Success;
                        }
                    },
                }
            },

            {
                Game.Items.Inventory.Groups.Bag,

                new Dictionary<Game.Items.Inventory.Groups, Func<PlayerData, Container, int, int, int, Game.Items.Inventory.Results>>()
                {
                    {
                        Game.Items.Inventory.Groups.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length || slotTo >= cont.Items.Length)
                                return Game.Items.Inventory.Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.Results.Error;

                            var toItem = cont.Items[slotTo];

                            if (!cont.IsItemAllowed(fromItem))
                                return Game.Items.Inventory.Results.Error;

                            float curWeight = cont.Weight;
                            float maxWeight = cont.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.Results.NoSpace;
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
                                        return Game.Items.Inventory.Results.NoSpace;
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
                                    return Game.Items.Inventory.Results.NoSpace;

                                pData.Bag.Items[slotFrom] = toItem;
                                cont.Items[slotTo] = fromItem;

                                pData.Bag.Update();
                                cont.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], Game.Items.Inventory.Groups.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], Game.Items.Inventory.Groups.Container);

                            player.InventoryUpdate(Game.Items.Inventory.Groups.Bag, slotFrom, upd1);

                            foreach (var x in cont.PlayersObserving.ToList())
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.InventoryUpdate(Game.Items.Inventory.Groups.Container, slotTo, upd2);
                            }

                            return Game.Items.Inventory.Results.Success;
                        }
                    },
                }
            },

            {
                Game.Items.Inventory.Groups.Container,

                new Dictionary<Game.Items.Inventory.Groups, Func<PlayerData, Container, int, int, int, Game.Items.Inventory.Results>>()
                {
                    {
                        Game.Items.Inventory.Groups.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return Game.Items.Inventory.Results.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.Results.Error;

                            if (slotTo >= cont.Items.Length)
                                return Game.Items.Inventory.Results.Error;

                            var toItem = cont.Items[slotTo];

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.Results.Error;

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

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], Game.Items.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], Game.Items.Inventory.Groups.Container);

                            foreach (var x in cont.PlayersObserving.ToList())
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.InventoryUpdate(Game.Items.Inventory.Groups.Container, slotFrom, upd1);
                                target.InventoryUpdate(Game.Items.Inventory.Groups.Container, slotTo, upd2);
                            }

                            return Game.Items.Inventory.Results.Success;
                        }
                    },

                    {
                        Game.Items.Inventory.Groups.Items,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return Game.Items.Inventory.Results.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return Game.Items.Inventory.Results.Error;

                            var toItem = pData.Items[slotTo];

                            if (!cont.IsItemAllowed(toItem))
                                return Game.Items.Inventory.Results.Error;

                            float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.Results.NoSpace;
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
                                if (fromItem.BaseWeight * amount + curWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.Results.NoSpace;
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

                                if ((addWeightBag - addWeightItems + curWeight > Settings.MAX_INVENTORY_WEIGHT) || (addWeightItems - addWeightBag + cont.Weight > cont.MaxWeight))
                                    return Game.Items.Inventory.Results.NoSpace;

                                cont.Items[slotFrom] = toItem;
                                pData.Items[slotTo] = fromItem;

                                cont.Update();
                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], Game.Items.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], Game.Items.Inventory.Groups.Items);

                            player.InventoryUpdate(Game.Items.Inventory.Groups.Items, slotTo, upd2);

                            foreach (var x in cont.PlayersObserving)
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.InventoryUpdate(Game.Items.Inventory.Groups.Container, slotFrom, upd1);
                            }

                            return Game.Items.Inventory.Results.Success;
                        }
                    },

                    {
                        Game.Items.Inventory.Groups.Bag,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return Game.Items.Inventory.Results.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return Game.Items.Inventory.Results.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return Game.Items.Inventory.Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (!cont.IsItemAllowed(toItem))
                                return Game.Items.Inventory.Results.Error;

                            float curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            float maxWeight = pData.Bag.Data.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return Game.Items.Inventory.Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return Game.Items.Inventory.Results.NoSpace;
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
                                        return Game.Items.Inventory.Results.NoSpace;
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
                                    return Game.Items.Inventory.Results.NoSpace;

                                cont.Items[slotFrom] = toItem;
                                pData.Bag.Items[slotTo] = fromItem;

                                cont.Update();
                                pData.Bag.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], Game.Items.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], Game.Items.Inventory.Groups.Bag);

                            player.InventoryUpdate(Game.Items.Inventory.Groups.Bag, slotTo, upd2);

                            foreach (var x in cont.PlayersObserving.ToList())
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.InventoryUpdate(Game.Items.Inventory.Groups.Container, slotFrom, upd1);
                            }

                            return Game.Items.Inventory.Results.Success;
                        }
                    },
                }
            },
        };

        public static Func<PlayerData, Container, int, int, int, Game.Items.Inventory.Results> GetReplaceAction(Game.Items.Inventory.Groups from, Game.Items.Inventory.Groups to) => ReplaceActions.GetValueOrDefault(from)?.GetValueOrDefault(to);
    }
}
