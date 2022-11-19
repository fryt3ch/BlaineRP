using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Game.Items
{
    class Container : Script
    {
        /// <summary>Все загруженные контейнеры</summary>
        /// <value>Словарь, где ключ - UID контейнера, а значение - объект класса Container</value>
        public static Dictionary<uint, Container> All { get; private set; } = new Dictionary<uint, Container>();

        private static Queue<uint> FreeIDs { get; set; } = new Queue<uint>();

        private static uint LastAddedMaxId { get; set; }

        public static uint MoveNextId()
        {
            uint id;

            if (!FreeIDs.TryDequeue(out id))
            {
                id = ++LastAddedMaxId;
            }

            return id;
        }

        public static void AddFreeId(uint id) => FreeIDs.Enqueue(id);

        public static void AddOnLoad(Container cont)
        {
            if (cont == null)
                return;

            All.Add(cont.ID, cont);

            if (cont.ID > LastAddedMaxId)
                LastAddedMaxId = cont.ID;
        }

        public static void Add(Container cont)
        {
            if (cont == null)
                return;

            All.Add(cont.ID, cont);

            MySQL.ContainerAdd(cont);
        }

        public static void Remove(Container cont)
        {
            if (cont == null)
                return;

            var id = cont.ID;

            AddFreeId(id);

            All.Remove(id);

            MySQL.ContainerDelete(cont);
        }

        public enum AllowedItemTypes
        {
            All = 0, Wardrobe, Fridge
        }

        public enum ContainerTypes
        {
            None = -1,
            Trunk,
            Locker,
            Storage,
            Crate,
            Fridge,
            Wardrobe
        }

        public class Data
        {
            public int Slots { get; set; }

            public float MaxWeight { get; set; }

            public AllowedItemTypes AllowedItemsType { get; set; }

            public ContainerTypes ContainerType { get; set; }

            public Data(int Slots, float MaxWeight, AllowedItemTypes AllowedItemsType, ContainerTypes ContainerType)
            {
                this.Slots = Slots;
                this.MaxWeight = MaxWeight;
                this.AllowedItemsType = AllowedItemsType;
                this.ContainerType = ContainerType;
            }
        }

        /// <summary>Доступные типы предметов для хранения в контейнере</summary>
        /// <value>Словарь, где ключ - enum AllowedItemTypes, а значение - тип предмета</value>
        private static Dictionary<AllowedItemTypes, Type> AllowedItemsDict = new Dictionary<AllowedItemTypes, Type>()
        {
            { AllowedItemTypes.All, typeof(Game.Items.Item) },

            { AllowedItemTypes.Wardrobe, typeof(Game.Items.Clothes) },
        };

        /// <summary>Все доступные подтипы контейнеров</summary>
        /// <value>Словарь, где ключ - подID разновидности контейнера, а значение - кортеж её данных</value>
        public static Dictionary<string, Data> AllSIDs = new Dictionary<string, Data>();

        /// <summary>Предметы в контейнере</summary>
        /// <value>Массив объектов класса Game.Items.Item, в котором null - пустой слот</value>
        public Game.Items.Item[] Items { get; set; }

        /// <summary>Уникальный ID контейнера</summary>
        public uint ID { get; set; }

        /// <summary>ID разновидности контейнера (SubID)</summary>
        public string SID { get; set; }

        /// <summary>Игроки, в данный момент просматривающие контейнер</summary>
        /// <value>Список сущностей игроков</value>
        public List<PlayerData> PlayersObserving { get; set; }

        /// <summary>Максимальный вес контейнера</summary>
        public float MaxWeight { get => ContData.MaxWeight; }

        /// <summary>Тип предметов, доступных для хранения в контейнере</summary>
        private Type AllowedItemType { get => AllowedItemsDict[ContData.AllowedItemsType]; }

        /// <summary>Тип контейнера</summary>
        private ContainerTypes ContainerType { get => ContData.ContainerType; }

        /// <summary>Текущий общий вес контейнера</summary>
        public float Weight { get => Items.Sum(x => x?.Weight ?? 0f); }

        public Data ContData { get; set; }

        /// <summary>Сущность держателя контейнера</summary>
        public Entity Entity { get; set; }

        /// <summary>Метод для проверки, есть ли у игрока возможность просматривать контейнер</summary>
        public bool IsAccessableFor(PlayerData pData)
        {
            var player = pData.Player;

            if (Entity is Vehicle)
            {
                if (Entity?.Exists != true)
                    return false;

                var vData = (Entity as Vehicle).GetMainData();

                if (vData == null)
                    return false;

                if (vData.IsOwner(pData) != null)
                    return true;

                return !vData.TrunkLocked;
            }
            else if (Entity == null)
            {
                if (SID == "h_locker")
                {
                    if (pData.CurrentHouse == null)
                        return false;

                    var house = Game.Houses.House.Get((int)pData.CurrentHouse);

                    if (house == null || house.Locker != ID)
                        return false;

                    if (house.Owner == pData.CID)
                        return true;

                    return !house.ContainersLocked;
                }
                else if (SID == "h_wardrobe")
                {
                    if (pData.CurrentHouse == null)
                        return false;

                    var house = Game.Houses.House.Get((int)pData.CurrentHouse);

                    if (house == null || house.Wardrobe != ID)
                        return false;

                    if (house.Owner == pData.CID)
                        return true;

                    return !house.ContainersLocked;
                }
                else if (SID == "h_fridge")
                {
                    if (pData.CurrentHouse == null)
                        return false;

                    var house = Game.Houses.House.Get((int)pData.CurrentHouse);

                    if (house == null || house.Fridge != ID)
                        return false;

                    if (house.Owner == pData.CID)
                        return true;

                    return !house.ContainersLocked;
                }
                else
                    return false;
            }
            else
                return false;
        }

        /// <summary>Метод для проверки, рядом ли игрок (см. MaxDistance) </summary>
        public bool IsNear(PlayerData pData)
        {
            var player = pData.Player;

            if (Entity?.Exists == true)
                return player.AreEntitiesNearby(Entity, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            else
            {
                if (SID == "h_locker" || SID == "h_wardrobe" || SID == "h_fridge")
                    return pData.CurrentHouse != null;

                return false;
            }
        }

        public Container(string SID)
        {
            this.ID = MoveNextId();

            this.SID = SID;
            this.ContData = AllSIDs[SID];

            PlayersObserving = new List<PlayerData>();

            this.Items = new Item[ContData.Slots];
        }

        public Container(string SID, uint ID, Item[] Items)
        {
            this.ID = ID;

            this.SID = SID;
            this.ContData = AllSIDs[SID];

            PlayersObserving = new List<PlayerData>();

            this.Items = Items;
        }

        public Container()
        {

        }

        /// <summary>Метод для добавления игрока в качестве смотрящего контейнер</summary>
        /// <returns>true - если игрок был успешно добавлен, false - в противном случае</returns>
        public bool AddPlayerObserving(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.CurrentContainer != null || PlayersObserving.Contains(pData))
                return false;

            foreach (var x in PlayersObserving)
            {
                var target = x?.Player;

                if (target?.Exists != true || !target.AreEntitiesNearby(Entity, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                    RemovePlayerObserving(x);
            }

            if (PlayersObserving.Count >= Settings.CONTAINER_MAX_PLAYERS)
                return false;

            PlayersObserving.Add(pData);

            pData.CurrentContainer = this.ID;

            return true;
        }

        /// <summary>Метод для удаления игрока в качестве смотрящего контейнер</summary>
        public void RemovePlayerObserving(PlayerData pData)
        {
            var player = pData.Player;

            PlayersObserving.Remove(pData);

            player.TriggerEvent("Inventory::Close");

            pData.CurrentContainer = null;
        }

        /// <summary>Метод для очистки всех игроков, смотрящих контейнер</summary>
        public void ClearAllObservers()
        {
            foreach (var player in PlayersObserving)
                RemovePlayerObserving(player);
        }

        /// <summary>Метод для создания нового контейнера</summary>
        /// <remarks>Также, контейнер добавляется в базу данных</remarks>
        /// <returns>UID добавленного контейнера</returns>
        /// <param name="cont">Объект класса Container</param>
        /// <param name="owner">Сущность держателя контейнера</param>
        public static Container Create(string sid, Entity owner = null)
        {
            var cont = new Container(sid);

            cont.Entity = owner;

            Add(cont);

            return cont;
        }

        /// <summary>Метод для получения контейнера</summary>
        /// <remarks>Также, если контейнер не загружен, метод попытается загрузить его из базы данных</remarks>
        /// <returns>Объект класса Container, если таковой был найден, null - в противном случае</returns>
        /// <param name="id">UID контейнера</param>
        public static Container Get(uint id) => All.GetValueOrDefault(id);

        /// <summary>Метод для обновления предметов в контейнере в базе данных</summary>
        public void Update() => MySQL.ContainerUpdate(this);

        /// <summary>Метод для удаления контейнера</summary>
        public void Delete()
        {
            ClearAllObservers();

            Remove(this);
        }

        /// <summary>Метод для удаления ВСЕХ предметов из контейнера</summary>
        public void Clear()
        {
            ClearAllObservers();

            foreach (var item in Items)
                item.Delete();

            Items = new Item[Items.Length];

            Update();
        }

        /// <summary>Метод для проверки, допустим ли предмет для хранения в контейнере</summary>
        public bool IsItemAllowed(Game.Items.Item item)
        {
            if (item == null)
                return true;

            return this.AllowedItemType.IsAssignableFrom(item.Type);
        }

        /// <summary>Метод для обновления сущности держателя контейнера</summary>
        /// <param name="owner">Сущность держателя</param>
        public void UpdateOwner(Entity owner)
        {
            this.Entity = owner;
        }

        public string ToClientJson() => (new object[] { ContainerType, MaxWeight, Items.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Container)) }).SerializeToJson();

        private static Dictionary<CEF.Inventory.Groups, Dictionary<CEF.Inventory.Groups, Func<PlayerData, Container, int, int, int, CEF.Inventory.Results>>> ReplaceActions = new Dictionary<CEF.Inventory.Groups, Dictionary<CEF.Inventory.Groups, Func<PlayerData, Container, int, int, int, CEF.Inventory.Results>>>()
        {
            {
                CEF.Inventory.Groups.Items,

                new Dictionary<CEF.Inventory.Groups, Func<PlayerData, Container, int, int, int, CEF.Inventory.Results>>()
                {
                    {
                        CEF.Inventory.Groups.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= pData.Items.Length || slotTo >= cont.Items.Length)
                                return CEF.Inventory.Results.Error;

                            var fromItem = pData.Items[slotFrom];

                            if (fromItem == null)
                                return CEF.Inventory.Results.Error;

                            var toItem = cont.Items[slotTo];

                            if (fromItem.IsTemp)
                                return CEF.Inventory.Results.TempItem;

                            if (!cont.IsItemAllowed(fromItem))
                                return CEF.Inventory.Results.Error;

                            float curWeight = cont.Weight;
                            float maxWeight = cont.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return CEF.Inventory.Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return CEF.Inventory.Results.NoSpace;
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
                                        return CEF.Inventory.Results.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                cont.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                cont.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightBag - addWeightItems + curWeight > maxWeight) || (addWeightItems - addWeightBag + pData.Items.Sum(x => x?.Weight ?? 0f) > Settings.MAX_INVENTORY_WEIGHT))
                                    return CEF.Inventory.Results.NoSpace;

                                pData.Items[slotFrom] = toItem;
                                cont.Items[slotTo] = fromItem;

                                MySQL.CharacterItemsUpdate(pData.Info);
                                cont.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], CEF.Inventory.Groups.Items);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], CEF.Inventory.Groups.Container);

                            player.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Items, slotFrom, upd1);

                            foreach (var x in cont.PlayersObserving)
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotTo, upd2);
                            }

                            return CEF.Inventory.Results.Success;
                        }
                    },
                }
            },

            {
                CEF.Inventory.Groups.Bag,

                new Dictionary<CEF.Inventory.Groups, Func<PlayerData, Container, int, int, int, CEF.Inventory.Results>>()
                {
                    {
                        CEF.Inventory.Groups.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length || slotTo >= cont.Items.Length)
                                return CEF.Inventory.Results.Error;

                            var fromItem = pData.Bag.Items[slotFrom];

                            if (fromItem == null)
                                return CEF.Inventory.Results.Error;

                            var toItem = cont.Items[slotTo];

                            if (!cont.IsItemAllowed(fromItem))
                                return CEF.Inventory.Results.Error;

                            float curWeight = cont.Weight;
                            float maxWeight = cont.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return CEF.Inventory.Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return CEF.Inventory.Results.NoSpace;
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
                                        return CEF.Inventory.Results.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                cont.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                cont.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightBag - addWeightItems + curWeight > maxWeight) || (addWeightItems - addWeightBag + pData.Bag.Weight - pData.Bag.BaseWeight > pData.Bag.Data.MaxWeight))
                                    return CEF.Inventory.Results.NoSpace;

                                pData.Bag.Items[slotFrom] = toItem;
                                cont.Items[slotTo] = fromItem;

                                pData.Bag.Update();
                                cont.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], CEF.Inventory.Groups.Bag);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], CEF.Inventory.Groups.Container);

                            player.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Bag, slotFrom, upd1);

                            foreach (var x in cont.PlayersObserving.ToList())
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotTo, upd2);
                            }

                            return CEF.Inventory.Results.Success;
                        }
                    },
                }
            },

            {
                CEF.Inventory.Groups.Container,

                new Dictionary<CEF.Inventory.Groups, Func<PlayerData, Container, int, int, int, CEF.Inventory.Results>>()
                {
                    {
                        CEF.Inventory.Groups.Container,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return CEF.Inventory.Results.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return CEF.Inventory.Results.Error;

                            if (slotTo >= cont.Items.Length)
                                return CEF.Inventory.Results.Error;

                            var toItem = cont.Items[slotTo];

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return CEF.Inventory.Results.Error;

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

                                cont.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.ID, 0, amount);

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

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], CEF.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], CEF.Inventory.Groups.Container);

                            foreach (var x in cont.PlayersObserving.ToList())
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotFrom, upd1);
                                target.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotTo, upd2);
                            }

                            return CEF.Inventory.Results.Success;
                        }
                    },

                    {
                        CEF.Inventory.Groups.Items,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return CEF.Inventory.Results.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return CEF.Inventory.Results.Error;

                            if (slotTo >= pData.Items.Length)
                                return CEF.Inventory.Results.Error;

                            var toItem = pData.Items[slotTo];

                            if (!cont.IsItemAllowed(toItem))
                                return CEF.Inventory.Results.Error;

                            float curWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return CEF.Inventory.Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return CEF.Inventory.Results.NoSpace;
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
                                        return CEF.Inventory.Results.NoSpace;
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

                                if ((addWeightBag - addWeightItems + curWeight > Settings.MAX_INVENTORY_WEIGHT) || (addWeightItems - addWeightBag + cont.Weight > cont.MaxWeight))
                                    return CEF.Inventory.Results.NoSpace;

                                cont.Items[slotFrom] = toItem;
                                pData.Items[slotTo] = fromItem;

                                cont.Update();
                                MySQL.CharacterItemsUpdate(pData.Info);
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], CEF.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], CEF.Inventory.Groups.Items);

                            player.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Items, slotTo, upd2);

                            foreach (var x in cont.PlayersObserving)
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotFrom, upd1);
                            }

                            return CEF.Inventory.Results.Success;
                        }
                    },

                    {
                        CEF.Inventory.Groups.Bag,

                        (pData, cont, slotTo, slotFrom, amount) =>
                        {
                            var player = pData.Player;

                            if (slotFrom >= cont.Items.Length)
                                return CEF.Inventory.Results.Error;

                            var fromItem = cont.Items[slotFrom];

                            if (fromItem == null)
                                return CEF.Inventory.Results.Error;

                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return CEF.Inventory.Results.Error;

                            var toItem = pData.Bag.Items[slotTo];

                            if (!cont.IsItemAllowed(toItem))
                                return CEF.Inventory.Results.Error;

                            float curWeight = pData.Bag.Weight - pData.Bag.BaseWeight;
                            float maxWeight = pData.Bag.Data.MaxWeight;

                            #region Unite
                            if (toItem != null && toItem.ID == fromItem.ID && fromItem is Game.Items.IStackable fromStackable && toItem is Game.Items.IStackable toStackable)
                            {
                                int maxStack = toStackable.MaxAmount;

                                if (toStackable.Amount == maxStack)
                                    return CEF.Inventory.Results.Error;

                                if (amount == -1 || amount > fromStackable.Amount)
                                    amount = fromStackable.Amount;

                                if (curWeight + amount * fromItem.BaseWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / fromItem.BaseWeight);

                                    if (amount <= 0)
                                        return CEF.Inventory.Results.NoSpace;
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
                                        return CEF.Inventory.Results.NoSpace;
                                }

                                targetItem.Amount -= amount;
                                fromItem.Update();

                                pData.Bag.Items[slotTo] = Game.Items.Items.CreateItem(fromItem.ID, 0, amount); // but wait for that :)

                                pData.Bag.Update();
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = toItem?.Weight ?? 0f;
                                var addWeightBag = fromItem.Weight;

                                if ((addWeightBag - addWeightItems + curWeight > maxWeight) || (addWeightItems - addWeightBag + cont.Weight > cont.MaxWeight))
                                    return CEF.Inventory.Results.NoSpace;

                                cont.Items[slotFrom] = toItem;
                                pData.Bag.Items[slotTo] = fromItem;

                                cont.Update();
                                pData.Bag.Update();
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], CEF.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], CEF.Inventory.Groups.Bag);

                            player.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Bag, slotTo, upd2);

                            foreach (var x in cont.PlayersObserving.ToList())
                            {
                                var target = x?.Player;

                                if (target?.Exists != true)
                                    continue;

                                target.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotFrom, upd1);
                            }

                            return CEF.Inventory.Results.Success;
                        }
                    },
                }
            },
        };

        #region Show
        [RemoteEvent("Container::Show")]
        private static void Show(Player player, uint uid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentContainer != null)
                return;

            var cont = Get(uid);

            if (cont == null)
                return;

            if (!(cont.IsNear(pData) && cont.IsAccessableFor(pData)))
            {
                if (cont.Entity?.Type == EntityType.Vehicle)
                    player.Notify("Vehicle::NotAllowed");

                return;
            }

            if (!cont.AddPlayerObserving(pData))
            {
                player.Notify("Container::Wait");

                return;
            }

            string result = cont.ToClientJson();

            player.TriggerEvent("Inventory::Show", 1, result);
        }
        #endregion

        #region Replace
        [RemoteEvent("Container::Replace")]
        private static void Replace(Player player, int toStr, int slotTo, int fromStr, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentContainer == null)
                return;

            var offer = pData.ActiveOffer;

            if (offer != null)
            {
                if (offer.Type == Sync.Offers.Types.Exchange && offer.TradeData != null)
                    return;
            }

            if (!Enum.IsDefined(typeof(CEF.Inventory.Groups), toStr) || !Enum.IsDefined(typeof(CEF.Inventory.Groups), fromStr))
                return;

            if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                return;

            CEF.Inventory.Groups to = (CEF.Inventory.Groups)toStr;
            CEF.Inventory.Groups from = (CEF.Inventory.Groups)fromStr;

            var cont = Get((uint)pData.CurrentContainer);

            if (cont == null)
                return;

            if (!(cont.IsNear(pData) && cont.IsAccessableFor(pData)))
            {
                player.TriggerEvent("Inventory::Close");

                return;
            }

            var action = ReplaceActions.GetValueOrDefault(from)?.GetValueOrDefault(to);

            if (action != null)
            {
                var res = action.Invoke(pData, cont, slotTo, slotFrom, amount);

                var notification = CEF.Inventory.ResultsNotifications.GetValueOrDefault(res);

                if (notification == null)
                    return;

                player.Notify(notification);
            }
        }
        #endregion

        #region Drop
        [RemoteEvent("Container::Drop")]
        private static void Drop(Player player, int slot, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentContainer == null || amount < 1 || slot < 0)
                return;

            var cont = Get((uint)pData.CurrentContainer);

            if (cont == null)
                return;

            if (slot >= cont.Items.Length)
                return;

            var item = cont.Items[slot];

            if (item == null)
                return;

            if (!(cont.IsNear(pData) && cont.IsAccessableFor(pData)))
            {
                player.TriggerEvent("Inventory::Close");

                return;
            }

            if (item is Game.Items.IStackable itemStackable)
            {
                int curAmount = itemStackable.Amount;

                if (amount > curAmount)
                    amount = curAmount;

                curAmount -= amount;

                if (curAmount > 0)
                {
                    itemStackable.Amount = curAmount;
                    cont.Items[slot] = item;

                    item.Update();
                    item = Game.Items.Items.CreateItem(item.ID, 0, amount);
                }
                else
                    cont.Items[slot] = null;
            }
            else
                cont.Items[slot] = null;

            var upd = Game.Items.Item.ToClientJson(cont.Items[slot], CEF.Inventory.Groups.Container);

            foreach (var x in cont.PlayersObserving)
            {
                var target = x?.Player;

                if (target?.Exists != true)
                    return;

                target.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slot, upd);
            }

            cont.Update();

            if (!pData.CanPlayAnim())
                pData.PlayAnim(Sync.Animations.FastTypes.Putdown);

            Game.World.AddItemOnGround(player, item, player.GetFrontOf(0.6f), player.Rotation, player.Dimension);
        }
        #endregion

        #region Close
        [RemoteEvent("Container::Close")]
        private static void OnClose(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentContainer == null)
                return;

            var cont = Get((uint)pData.CurrentContainer);

            cont.RemovePlayerObserving(pData);
        }
        #endregion
    }
}
