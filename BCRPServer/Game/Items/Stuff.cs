using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Game.Items
{
    public class Stuff
    {
        private static Dictionary<string, Type> AllTypes = new Dictionary<string, Type>();

        private static Dictionary<Type, Dictionary<string, Item.ItemData>> AllData = new Dictionary<Type, Dictionary<string, Item.ItemData>>();

        #region Give
        /// <summary>Создать и выдать реальный предмет игроку</summary>
        /// <remarks>Данный метод пробует выдать предмет игроку в любой свободный слот (или любой подходящий, если предмет IStackable), лишнее - выкидывается на землю</remarks>
        public static bool GiveItemDropExcess(PlayerData pData, out Game.Items.Item item, string id, int variation = 0, int amount = 1, bool notifyOnSuccess = true, bool notifyOnFault = true)
        {
            item = null;

            var player = pData.Player;

            var type = GetType(id, false);

            if (type == null)
                return false;

            var data = GetData(id, type);

            if (data == null)
                return false;

            var totalWeight = 0f;
            var freeIdx = -1;

            var amountInFact = amount;

            if (data is Game.Items.Item.ItemData.IStackable dataStackable)
            {
                if (amount > dataStackable.MaxAmount)
                {
                    amount = dataStackable.MaxAmount;

                    amountInFact = amount;
                }

                var itemWeight = amount * data.Weight;

                totalWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                var amountExceed = amount;

                if (totalWeight + itemWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                    amountExceed = (int)Math.Floor((Properties.Settings.Static.MAX_INVENTORY_WEIGHT - totalWeight) / data.Weight);

                if (amountExceed > 0)
                {
                    amount -= amountExceed;

                    var freeIndexesSame = new List<(int Slot, int Amount)>();

                    for (int i = 0; i < pData.Items.Length; i++)
                    {
                        var curItem = pData.Items[i];

                        if (curItem != null)
                        {
                            if (amountExceed > 0)
                            {
                                if (curItem is Game.Items.IStackable curItemS && curItem.ID == id)
                                {
                                    var freeAmount = curItemS.MaxAmount - curItemS.Amount;

                                    if (freeAmount >= amountExceed)
                                    {
                                        freeIndexesSame.Add((i, amountExceed));

                                        amountExceed = 0;

                                        break;
                                    }
                                    else if (freeAmount > 0)
                                    {
                                        amountExceed -= freeAmount;

                                        freeIndexesSame.Add((i, freeAmount));
                                    }
                                }
                            }
                        }
                        else if (freeIdx < 0)
                        {
                            freeIdx = i;
                        }
                    }

                    freeIndexesSame.ForEach(x =>
                    {
                        var item = pData.Items[x.Slot] as IStackable;

                        item.Amount += x.Amount;

                        pData.Items[x.Slot].Update();

                        player.InventoryUpdate(Inventory.GroupTypes.Items, x.Slot, Item.ToClientJson(pData.Items[x.Slot], Inventory.GroupTypes.Items));
                    });

                    if (amountExceed > 0)
                    {
                        if (freeIdx >= 0)
                        {
                            item = CreateItem(id, type, data, variation, amountExceed, false);

                            pData.Items[freeIdx] = item;

                            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, freeIdx, Game.Items.Item.ToClientJson(item, Game.Items.Inventory.GroupTypes.Items));

                            MySQL.CharacterItemsUpdate(pData.Info);

                            amountExceed = 0;
                        }
                    }

                    if (freeIdx >= 0)
                        freeIdx = -1;

                    amount += amountExceed;
                }
            }
            else
            {
                if (amount != 1)
                    amount = 1;

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    var curItem = pData.Items[i];

                    if (curItem != null)
                    {
                        totalWeight += curItem.Weight;

                        if (totalWeight + data.Weight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                        {
                            if (freeIdx >= 0)
                                freeIdx = -1;

                            break;
                        }
                    }
                    else if (freeIdx < 0)
                    {
                        freeIdx = i;
                    }
                }
            }

            if (notifyOnSuccess)
                player.TriggerEvent("Item::Added", id, amountInFact);

            if (amount > 0)
            {
                item = CreateItem(id, type, data, variation, amount, false);

                if (freeIdx >= 0)
                {
                    pData.Items[freeIdx] = item;

                    player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, freeIdx, Game.Items.Item.ToClientJson(item, Game.Items.Inventory.GroupTypes.Items));

                    MySQL.CharacterItemsUpdate(pData.Info);
                }
                else
                {
                    Sync.World.AddItemOnGround(pData, item, pData.Player.Position, pData.Player.Rotation, pData.Player.Dimension);
                }
            }

            return true;
        }

        /// <summary>Создать и выдать реальный предмет игроку</summary>
        /// <remarks>Данный метод пробует выдать предмет игроку в любой свободный слот (или любой подходящий, если предмет IStackable)</remarks>
        public static bool GiveItem(PlayerData pData, out Game.Items.Item item, string id, int variation = 0, int amount = 1, bool notifyOnSuccess = true, bool notifyOnFault = true)
        {
            item = null;

            var player = pData.Player;

            var type = GetType(id, false);

            if (type == null)
                return false;

            var data = GetData(id, type);

            if (data == null)
                return false;

            var totalWeight = 0f;
            var freeIdx = -1;

            var amountInFact = amount;

            if (data is Game.Items.Item.ItemData.IStackable dataStackable)
            {
                if (amount > dataStackable.MaxAmount)
                    amount = dataStackable.MaxAmount;

                var itemWeight = amount * data.Weight;

                var freeIndexesSame = new List<(int Slot, int Amount)>();

                var amountExceed = amount;

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    var curItem = pData.Items[i];

                    if (curItem != null)
                    {
                        totalWeight += curItem.Weight;

                        if (totalWeight + itemWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                        {
                            if (freeIdx >= 0)
                                freeIdx = -1;

                            if (amountExceed <= 0)
                                amountExceed = 1;

                            break;
                        }

                        if (amountExceed > 0)
                        {
                            if (curItem is Game.Items.IStackable curItemS && curItem.ID == id)
                            {
                                var freeAmount = curItemS.MaxAmount - curItemS.Amount;

                                if (freeAmount >= amountExceed)
                                {
                                    freeIndexesSame.Add((i, amountExceed));

                                    amountExceed = 0;
                                }
                                else if (freeAmount > 0)
                                {
                                    amountExceed -= freeAmount;

                                    freeIndexesSame.Add((i, freeAmount));
                                }
                            }
                        }
                    }
                    else if (freeIdx < 0)
                    {
                        freeIdx = i;
                    }
                }

                if (amountExceed > 0 && freeIdx < 0)
                {
                    if (notifyOnFault)
                        player.Notify("Inventory::NoSpace");

                    return false;
                }

                freeIndexesSame.ForEach(x =>
                {
                    var item = pData.Items[x.Slot] as IStackable;

                    item.Amount += x.Amount;

                    pData.Items[x.Slot].Update();

                    player.InventoryUpdate(Inventory.GroupTypes.Items, x.Slot, Item.ToClientJson(pData.Items[x.Slot], Inventory.GroupTypes.Items));
                });

                if (amountExceed <= 0)
                {
                    if (notifyOnSuccess)
                        player.TriggerEvent("Item::Added", id, amount);

                    return true;
                }

                amount = amountExceed;
            }
            else
            {
                if (amount != 1)
                    amount = 1;

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    var curItem = pData.Items[i];

                    if (curItem != null)
                    {
                        totalWeight += curItem.Weight;

                        if (totalWeight + data.Weight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                        {
                            if (freeIdx >= 0)
                                freeIdx = -1;

                            break;
                        }
                    }
                    else if (freeIdx < 0)
                    {
                        freeIdx = i;
                    }
                }

                if (freeIdx < 0)
                {
                    if (notifyOnFault)
                        player.Notify("Inventory::NoSpace");

                    return false;
                }
            }

            item = CreateItem(id, type, data, variation, amount, false);

            if (item == null)
                return false;

            pData.Items[freeIdx] = item;

            if (notifyOnSuccess)
                player.TriggerEvent("Item::Added", item.ID, amountInFact);

            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, freeIdx, Game.Items.Item.ToClientJson(item, Game.Items.Inventory.GroupTypes.Items));

            MySQL.CharacterItemsUpdate(pData.Info);

            return true;
        }

        /// <summary>Создать и выдать временный предмет игроку</summary>
        /// <remarks>Данный метод пробует выдать предмет игроку в любой свободный слот (или любой подходящий, если предмет IStackable)</remarks>
        public static bool GiveTempItem(PlayerData pData, string id, int variation = 0, int amount = 1, bool notifyOnSuccess = true, bool notifyOnFault = true)
        {
            var player = pData.Player;

            var type = GetType(id, false);

            if (type == null)
                return false;

            var data = GetData(id, type);

            if (data == null)
                return false;

            var totalWeight = 0f;
            var freeIdx = -1;

            var amountInFact = amount;

            if (data is Game.Items.Item.ItemData.IStackable dataStackable)
            {
                if (amount > dataStackable.MaxAmount)
                    amount = dataStackable.MaxAmount;

                var itemWeight = amount * data.Weight;

                var freeIndexesSame = new List<(int Slot, int Amount)>();

                var amountExceed = amount;

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    var curItem = pData.Items[i];

                    if (curItem != null)
                    {
                        totalWeight += curItem.Weight;

                        if (totalWeight + itemWeight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                        {
                            if (freeIdx >= 0)
                                freeIdx = -1;

                            if (amountExceed <= 0)
                                amountExceed = 1;

                            break;
                        }

                        if (amountExceed > 0)
                        {
                            if (curItem is Game.Items.IStackable curItemS && curItem.ID == id)
                            {
                                var freeAmount = curItemS.MaxAmount - curItemS.Amount;

                                if (freeAmount >= amountExceed)
                                {
                                    freeIndexesSame.Add((i, amountExceed));

                                    amountExceed = 0;
                                }
                                else if (freeAmount > 0)
                                {
                                    amountExceed -= freeAmount;

                                    freeIndexesSame.Add((i, freeAmount));
                                }
                            }
                        }
                    }
                    else if (freeIdx < 0)
                    {
                        freeIdx = i;
                    }
                }

                if (amountExceed != 0 && freeIdx < 0)
                {
                    if (notifyOnFault)
                        player.Notify("Inventory::NoSpace");

                    return false;
                }

                freeIndexesSame.ForEach(x =>
                {
                    var item = pData.Items[x.Slot] as IStackable;

                    item.Amount += x.Amount;

                    pData.Items[x.Slot].Update();

                    player.InventoryUpdate(Inventory.GroupTypes.Items, x.Slot, Item.ToClientJson(pData.Items[x.Slot], Inventory.GroupTypes.Items));
                });

                if (amountExceed <= 0)
                {
                    if (notifyOnSuccess)
                        player.TriggerEvent("Item::Added", id, amount);

                    return true;
                }

                amountInFact = amount;

                amount = amountExceed;
            }
            else
            {
                if (amount != 1)
                    amount = 1;

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    var curItem = pData.Items[i];

                    if (curItem != null)
                    {
                        totalWeight += curItem.Weight;

                        if (totalWeight + data.Weight > Properties.Settings.Static.MAX_INVENTORY_WEIGHT)
                        {
                            if (freeIdx >= 0)
                                freeIdx = -1;

                            break;
                        }
                    }
                    else if (freeIdx < 0)
                    {
                        freeIdx = i;
                    }
                }

                if (freeIdx < 0)
                {
                    if (notifyOnFault)
                        player.Notify("Inventory::NoSpace");

                    return false;
                }

                amountInFact = amount;
            }

            var item = CreateItem(id, type, data, variation, amount, true);

            if (item == null)
                return false;

            pData.Items[freeIdx] = item;

            if (notifyOnSuccess)
                player.TriggerEvent("Item::Added", item.ID, amountInFact);

            player.InventoryUpdate(Game.Items.Inventory.GroupTypes.Items, freeIdx, Game.Items.Item.ToClientJson(item, Game.Items.Inventory.GroupTypes.Items));

            return true;
        }

        /// <summary>Выдать уже существующий предмет игроку</summary>
        /// <remarks>Данный метод пробует выдать предмет игроку в любой свободный слот (или любой подходящий, если предмет IStackable)</remarks>
        public static bool GiveExistingItem(PlayerData pData, Item item, int amount, bool notifyOnFail = true, bool notifyOnSuccess = true)
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
        #endregion

        #region Create
        /// <summary>Метод для создания нового предмета</summary>
        /// <param name="id">ID предмета (см. Game.Items.Item.LoadAll</param>
        /// <param name="variation">Вариация предмета (только для IWearable, в противном случае - игнорируется)</param>
        /// <param name="amount">Кол-во предмета (только для IStakable и Weapon, в противном случае - игнорируется)</param>
        /// <param name="isTemp">Временный ли предмет?<br/>Такой предмет не будет сохраняться в базу данных и его будет нельзя: <br/><br/>1) Разделять (если IStackable или Weapon)<br/>2) Перемещать в IContainer и Game.Items.Container<br/>3) Выбрасывать (предмет удалится, но не появится на земле)<br/>4) Передавать другим игрокам</param>
        /// <returns>Объект класса Item, если предмет был создан, null - в противном случае</returns>
        public static Item CreateItem(string id, int variation = 0, int amount = 1, bool isTemp = false)
        {
            var type = GetType(id, false);

            if (type == null)
                return null;

            var data = GetData(id, type);

            if (data == null)
                return null;

            return CreateItem(id, type, data, variation, amount, isTemp);
        }

        public static Item CreateItem(string id, Type type, Item.ItemData data, int variation, int amount, bool isTemp)
        {
            Item item = typeof(Clothes).IsAssignableFrom(type) ? (Clothes)Activator.CreateInstance(type, id, variation) : (Item)Activator.CreateInstance(type, id);

            if (item is IStackable stackable)
            {
                if (amount <= 0)
                    amount = 1;

                var maxAmount = stackable.MaxAmount;

                stackable.Amount = amount > maxAmount ? maxAmount : amount;
            }
            else if (item is Weapon weapon)
            {
                if (amount <= 1)
                    amount = 0;

                var maxAmount = weapon.Data.MaxAmmo;

                if (amount > 0)
                    weapon.Ammo = amount > maxAmount ? maxAmount : amount;
            }

            if (!isTemp)
            {
                Item.Add(item);

                return item;
            }
            else
            {
                item.UID = 0;

                return item;
            }
        }
        #endregion

        #region Stuff

        public static int GetItemAmount(Game.Items.Item item) => item is IStackable stackable ? stackable.Amount : 1;

        public static string GetItemTag(Game.Items.Item item)
        {
            if (item is Weapon weapon)
                return weapon.Ammo > 0 ? weapon.Ammo.ToString() : string.Empty;

            if (item is Armour armour)
                return armour.Strength.ToString();

            if (item is IConsumable consumable)
                return consumable.Amount.ToString();

            if (item is ITagged tagged)
                return tagged.Tag;

            return string.Empty;
        }

        public static string GetItemNameWithTag(Game.Items.Item item, string tag, out string baseName)
        {
            baseName = item.Data.Name;

            if (tag == null || tag.Length == 0)
                return baseName;

            if (typeof(ITaggedFull).IsAssignableFrom(item.Type))
            {
                return tag;
            }

            return $"{baseName} [{tag}]";
        }

        public static Type GetType(string id, bool checkFullId = true)
        {
            var data = id.Split('_');

            var type = AllTypes.GetValueOrDefault(data[0]);

            if (type == null || (checkFullId && !AllData[type].ContainsKey(id)))
                return null;

            return type;
        }

        public static Item.ItemData GetData(string id, Type type = null)
        {
            if (type == null)
            {
                type = GetType(id, false);

                if (type == null)
                    return null;
            }

            return AllData[type].GetValueOrDefault(id);
        }
        #endregion

        public static int LoadAll()
        {
            var ns = typeof(Item).Namespace;

            int counter = 0;

            var lines = new List<string>();

            foreach (var x in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == ns && t.IsClass && !t.IsAbstract && typeof(Item).IsAssignableFrom(t)))
            {
                //var idList= (IDictionary)x.GetField("IDList")?.GetValue(null).Cast<dynamic>().ToDictionary(a => (string)a.Key, a => (Item.ItemData)a.Value);

                var idList = (Dictionary<string, Item.ItemData>)x.GetField("IDList")?.GetValue(null);

                if (idList == null)
                    continue;

                AllData.Add(x, idList);

                counter += idList.Count;

                foreach (var t in idList)
                {
                    var id = t.Key.Split('_');

                    if (!AllTypes.ContainsKey(id[0]))
                        AllTypes.Add(id[0], x);

                    lines.Add($"{x.Name}.IDList.Add(\"{t.Key}\", (Item.ItemData)new {x.Name}.ItemData({t.Value.ClientData}));");
                }
            }

            foreach (var x in Craft.Craft.AllReceipts)
            {
                lines.Add($"Craft.AllReceipts.Add(new Craft.Receipt(new Craft.ResultData(\"{x.CraftResultData.ResultItem.Id}\", {x.CraftResultData.ResultItem.Amount}, {x.CraftResultData.CraftTime}), {string.Join(',', x.CraftNeededItems.Select(x => $"new Craft.ItemPrototype(\"{x.Id}\",{x.Amount})"))}));");
            }

            Utils.FillFileToReplaceRegion(Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Data\Items\Items.cs", "TO_REPLACE", lines);

            return counter;
        }
    }
}
