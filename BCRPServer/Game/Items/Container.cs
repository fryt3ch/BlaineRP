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
        public static Dictionary<uint, Container> AllContainers { get; set; }

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
        public List<Player> PlayersObserving { get; set; }

        /// <summary>Максимальный вес контейнера</summary>
        public float MaxWeight { get => ContData.MaxWeight; }
        /// <summary>Тип предметов, доступных для хранения в контейнере</summary>
        private Type AllowedItemType { get => AllowedItemsDict[ContData.AllowedItemsType]; }
        /// <summary>Тип контейнера</summary>
        private ContainerTypes ContainerType { get => ContData.ContainerType; }
        /// <summary>Текущий общий вес контейнера</summary>
        public float Weight { get => Game.Items.Items.GetWeight(Items); }

        public Data ContData { get; set; }


        /// <summary>Сущность держателя контейнера</summary>
        public Entity Entity { get; set; }

        public SemaphoreSlim Semaphore { get; set; }

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
        public bool IsNear(Player player)
        {
            if (player == null)
                return false;

            var pData = player.GetMainData();

            if (pData == null)
                return false;

            if (Entity?.Exists == true)
                return player.AreEntitiesNearby(Entity, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            else
            {
                if (SID == "h_locker" || SID == "h_wardrobe" || SID == "h_fridge")
                    return pData.CurrentHouse != null;

                return false;
            }
        }

        public Container()
        {
            PlayersObserving = new List<Player>();

            Semaphore = new SemaphoreSlim(1, 1);
        }

        public Container(string SID) : this()
        {
            this.SID = SID;
            this.ContData = AllSIDs[SID];

            this.Items = new Item[ContData.Slots];
        }

        /// <summary>Метод для добавления игрока в качестве смотрящего контейнер</summary>
        /// <returns>true - если игрок был успешно добавлен, false - в противном случае</returns>
        public async Task<bool> AddPlayerObserving(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.CurrentContainer != null || PlayersObserving.Contains(player))
                return false;

            await NAPI.Task.RunAsync(() =>
            {
                foreach (var x in PlayersObserving.ToList())
                {
                    if (x?.Exists != true || !x.AreEntitiesNearby(Entity, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                        RemovePlayerObserving(x);
                }
            });

            if (PlayersObserving.Count >= Settings.CONTAINER_MAX_PLAYERS)
                return false;

            PlayersObserving.Add(player);

            pData.CurrentContainer = this.ID;

            return true;
        }

        /// <summary>Метод для удаления игрока в качестве смотрящего контейнер</summary>
        public void RemovePlayerObserving(Player player)
        {
            PlayersObserving.Remove(player);

            NAPI.Task.RunSafe(() =>
            {
                if (player?.Exists != true)
                    return;

                player?.TriggerEvent("Inventory::Close");

                var pData = player.GetMainData();

                pData.CurrentContainer = null;
            });
        }

        /// <summary>Метод для очистки всех игроков, смотрящих контейнер</summary>
        public void ClearAllObservers()
        {
            foreach (var player in PlayersObserving.ToList())
                RemovePlayerObserving(player);
        }

        /// <summary>Метод для создания нового контейнера</summary>
        /// <remarks>Также, контейнер добавляется в базу данных</remarks>
        /// <returns>UID добавленного контейнера</returns>
        /// <param name="cont">Объект класса Container</param>
        /// <param name="owner">Сущность держателя контейнера</param>
        public static async Task<Container> Create(Container cont, Entity owner = null)
        {
            cont = MySQL.AddContainer(cont);

            cont.Entity = owner;

            await NAPI.Task.RunAsync(() =>
            {
                if (AllContainers.ContainsKey(cont.ID))
                    AllContainers[cont.ID] = cont;
                else
                    AllContainers.Add(cont.ID, cont);
            });

            return cont;
        }

        /// <summary>Метод для получения контейнера</summary>
        /// <remarks>Также, если контейнер не загружен, метод попытается загрузить его из базы данных</remarks>
        /// <returns>Объект класса Container, если таковой был найден, null - в противном случае</returns>
        /// <param name="id">UID контейнера</param>
        public static Container Get(uint id)
        {
            if (AllContainers.ContainsKey(id))
                return AllContainers[id];
            else
                return null;
        }

        /// <summary>Метод для получения контейнера</summary>
        /// <remarks>Также, если контейнер не загружен, метод попытается загрузить его из базы данных</remarks>
        /// <returns>Объект класса Container, если таковой был найден, null - в противном случае</returns>
        /// <param name="id">UID контейнера</param>
        /// <param name="owner">Сущность держателя контейнера (для обновления)</param>
        public static Container Load(uint id, Entity owner = null)
        {
            var cont = MySQL.LoadContainerByID(id);

            if (cont == null)
                return null;

            cont.Entity = owner;

            NAPI.Task.RunAsync(() =>
            {
                if (AllContainers.ContainsKey(id))
                    AllContainers[id] = cont;
                else
                    AllContainers.Add(id, cont);
            }).GetAwaiter().GetResult();

            return cont;
        }

        /// <summary>Метод для обновления предметов в контейнере в базе данных</summary>
        public void Update() => MySQL.UpdateContainer(this);

        /// <summary>Метод для удаления контейнера</summary>
        /// <param name="completely">true - удалить из базы данных, false - выгрузить из сервера</param>
        public async Task Delete(bool completely = false)
        {
            await NAPI.Task.RunAsync(() =>
            {
                AllContainers.Remove(this.ID);
            });

            await this.WaitAsync();

            this.Semaphore?.Dispose();

            ClearAllObservers();

            if (completely)
                MySQL.DeleteContainer(this);
        }

        /// <summary>Метод для удаления ВСЕХ предметов из контейнера</summary>
        public async Task Clear()
        {
            await this.WaitAsync();

            ClearAllObservers();

            foreach (var item in Items)
                item.Delete();

            Items = new Item[Items.Length];

            Update();

            this.Release();
        }

        /// <summary>Метод для проверки, допустим ли предмет для хранения в контейнере</summary>
        public bool IsItemAllowed(Game.Items.Item item)
        {
            if (item == null)
                return true;

            if (this.AllowedItemType == typeof(Game.Items.Item))
                return true;

            return item.GetType() == this.AllowedItemType;
        }

        /// <summary>Метод для обновления сущности держателя контейнера</summary>
        /// <param name="owner">Сущность держателя</param>
        public void UpdateOwner(Entity owner)
        {
            this.Entity = owner;
        }

        public string ToClientJson() => (new object[] { ContainerType, MaxWeight, Items.Select(x => Game.Items.Item.ToClientJson(x, CEF.Inventory.Groups.Container)) }).SerializeToJson();

        #region Show
        [RemoteEvent("Container::Show")]
        private static async Task Show(Player player, uint uid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentContainer != null)
                    return;

                var cont = Get(uid);

                if (!await cont.WaitAsync())
                    return;

                await Task.Run(async () =>
                {
                    var res = await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return false;

                        var res = cont.IsNear(player) && cont.IsAccessableFor(pData);

                        if (!res)
                        {
                            if (cont.Entity?.Type == EntityType.Vehicle)
                                player.Notify("Vehicle::NotAllowed");
                        }

                        return res;
                    });

                    if (!res)
                        return;

                    if (!await cont.AddPlayerObserving(pData))
                    {
                        NAPI.Task.Run(() =>
                        {
                            if (player?.Exists != true)
                                return;

                            player.Notify("Container::Wait");
                        });

                        return;
                    }

                    string result = cont.ToClientJson();

                    await NAPI.Task.RunAsync(() =>
                    {
                        player?.TriggerEvent("Inventory::Show", 1, result);
                    });
                });

                cont.Release();
            });

            pData.Release();
        }
        #endregion

        #region Replace
        [RemoteEvent("Container::Replace")]
        private static async Task Replace(Player player, int toStr, int slotTo, int fromStr, int slotFrom, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentContainer == null)
                    return;

                var offer = await pData.ActiveOffer;

                if (offer != null)
                {
                    if (offer.Type == Sync.Offers.Types.Exchange && offer.TradeData != null)
                        return;
                }

                if (toStr > 9 || toStr < 0 || fromStr > 9 || fromStr < 0)
                    return;

                if (slotFrom < 0 || slotTo < 0 || amount < -1 || amount == 0)
                    return;

                CEF.Inventory.Groups to = (CEF.Inventory.Groups)toStr;
                CEF.Inventory.Groups from = (CEF.Inventory.Groups)fromStr;

                var cont = Get((uint)pData.CurrentContainer);

                if (!await cont.WaitAsync())
                    return;

                var res = await Task.Run<CEF.Inventory.Results>(async () =>
                {
                    if (cont == null)
                        return CEF.Inventory.Results.Error;

                    var res = await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return false;

                        var res = cont.IsNear(player) && cont.IsAccessableFor(pData);

                        if (!res)
                            player.TriggerEvent("Inventory::Close");

                        return res;
                    });

                    if (!res)
                        return CEF.Inventory.Results.Error;

                    #region From Pockets
                    if (from == CEF.Inventory.Groups.Items)
                    {
                        if (to != CEF.Inventory.Groups.Container)
                            return CEF.Inventory.Results.Error;

                        if (slotFrom >= pData.Items.Length || slotTo >= cont.Items.Length || pData.Items[slotFrom] == null)
                            return CEF.Inventory.Results.Error;

                        if (pData.Items[slotFrom].IsTemp)
                            return CEF.Inventory.Results.TempItem;

                        if (!cont.IsItemAllowed(pData.Items[slotFrom]))
                            return CEF.Inventory.Results.Error;

                        float curWeight = cont.Weight;
                        float maxWeight = cont.MaxWeight;

                        bool wasDeleted = false;
                        bool wasCreated = false;

                        #region Unite
                        if (cont.Items[slotTo] != null && cont.Items[slotTo].Type == pData.Items[slotFrom].Type && pData.Items[slotFrom] is Game.Items.IStackable)
                        {
                            int slotToAmount = (cont.Items[slotTo] as Game.Items.IStackable).Amount;
                            int slotFromAmount = (pData.Items[slotFrom] as Game.Items.IStackable).Amount;

                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            int maxStack = (pData.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                            if (slotToAmount == maxStack)
                                return CEF.Inventory.Results.Error;

                            // if amount*weight is too big -> reduce amount to fit the bag's maxWeight
                            if (curWeight + amount * pData.Items[slotFrom].Weight > maxWeight)
                            {
                                amount = (int)Math.Floor((maxWeight - curWeight) / pData.Items[slotFrom].Weight);

                                if (amount == 0)
                                    return CEF.Inventory.Results.NoSpace;
                            }

                            if (slotToAmount + amount > maxStack)
                            {
                                (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                (cont.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                            }
                            else
                            {
                                (cont.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                if ((pData.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                {
                                    var tItem = pData.Items[slotFrom];

                                    Task.Run(() => tItem.Delete());

                                    pData.Items[slotFrom] = null;

                                    wasDeleted = true;
                                }
                            }

                            cont.Items[slotTo].Update();
                            pData.Items[slotFrom]?.Update();
                        }
                        #endregion
                        #region Split To New
                        else if (pData.Items[slotFrom] is Game.Items.IStackable && cont.Items[slotTo] == null && amount != -1 && amount < (pData.Items[slotFrom] as Game.Items.IStackable).Amount)
                        {
                            if (pData.Items[slotFrom].Weight * amount + curWeight > maxWeight)
                            {
                                amount = (int)Math.Floor((maxWeight - curWeight) / pData.Items[slotFrom].Weight);

                                if (amount == 0)
                                    return CEF.Inventory.Results.NoSpace;
                            }

                            (pData.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                            pData.Items[slotFrom].Update();

                            cont.Items[slotTo] = await Game.Items.Items.CreateItem(pData.Items[slotFrom].ID, 0, amount); // but wait for that :)

                            wasCreated = true;
                        }
                        #endregion
                        #region Replace
                        else
                        {
                            var addWeightItems = cont.Items[slotTo] != null ? Game.Items.Items.GetItemWeight(cont.Items[slotTo], true) : 0;
                            var addWeightBag = Game.Items.Items.GetItemWeight(pData.Items[slotFrom], true);

                            if (addWeightBag - addWeightItems + curWeight > maxWeight || addWeightItems - addWeightBag + Game.Items.Items.GetWeight(pData.Items) > Settings.MAX_INVENTORY_WEIGHT)
                                return CEF.Inventory.Results.NoSpace;

                            var temp = pData.Items[slotFrom];
                            pData.Items[slotFrom] = cont.Items[slotTo];
                            cont.Items[slotTo] = temp;

                            wasDeleted = true; wasCreated = true;
                        }
                        #endregion

                        var upd1 = Game.Items.Item.ToClientJson(pData.Items[slotFrom], CEF.Inventory.Groups.Items);
                        var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], CEF.Inventory.Groups.Container);

                        NAPI.Task.Run(() =>
                        {
                            player?.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Items, slotFrom, upd1);

                            foreach (var x in cont.PlayersObserving.ToList())
                            {
                                if (x?.Exists != true)
                                    continue;

                                x.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotTo, upd2);
                            }
                        });

                        if (wasDeleted)
                            MySQL.UpdatePlayerInventory(pData, true);

                        if (wasCreated)
                            cont.Update();

                        return CEF.Inventory.Results.Success;
                    }
                    #endregion

                    #region From Bag
                    else if (from == CEF.Inventory.Groups.Bag)
                    {
                        if (to != CEF.Inventory.Groups.Container)
                            return CEF.Inventory.Results.Error;

                        if (pData.Bag == null || slotFrom >= pData.Bag.Items.Length || slotTo >= cont.Items.Length || pData.Bag.Items[slotFrom] == null)
                            return CEF.Inventory.Results.Error;

                        if (!cont.IsItemAllowed(pData.Bag.Items[slotFrom]))
                            return CEF.Inventory.Results.Error;

                        float curWeight = cont.Weight;
                        float maxWeight = cont.MaxWeight;

                        bool wasDeleted = false;
                        bool wasCreated = false;

                        #region Unite
                        if (cont.Items[slotTo] != null && cont.Items[slotTo].Type == pData.Bag.Items[slotFrom].Type && pData.Bag.Items[slotFrom] is Game.Items.IStackable)
                        {
                            int slotToAmount = (cont.Items[slotTo] as Game.Items.IStackable).Amount;
                            int slotFromAmount = (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount;

                            if (amount == -1 || amount > slotFromAmount)
                                amount = slotFromAmount;

                            int maxStack = (pData.Bag.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                            if (slotToAmount == maxStack)
                                return CEF.Inventory.Results.Error;

                            // if amount*weight is too big -> reduce amount to fit the bag's maxWeight
                            if (curWeight + amount * pData.Bag.Items[slotFrom].Weight > maxWeight)
                            {
                                amount = (int)Math.Floor((maxWeight - curWeight) / pData.Bag.Items[slotFrom].Weight);

                                if (amount == 0)
                                    return CEF.Inventory.Results.NoSpace;
                            }

                            if (slotToAmount + amount > maxStack)
                            {
                                (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                (cont.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                            }
                            else
                            {
                                (cont.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                if ((pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                {
                                    var tItem = pData.Bag.Items[slotFrom];

                                    Task.Run(() => tItem.Delete());

                                    pData.Bag.Items[slotFrom] = null;

                                    wasDeleted = true;
                                }
                            }

                            cont.Items[slotTo].Update();
                            pData.Bag.Items[slotFrom]?.Update();
                        }
                        #endregion
                        #region Split To New
                        else if (pData.Bag.Items[slotFrom] is Game.Items.IStackable && cont.Items[slotTo] == null && amount != -1 && amount < (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount)
                        {
                            if (pData.Bag.Items[slotFrom].Weight * amount + curWeight > maxWeight)
                            {
                                amount = (int)Math.Floor((maxWeight - curWeight) / pData.Bag.Items[slotFrom].Weight);

                                if (amount == 0)
                                    return CEF.Inventory.Results.NoSpace;
                            }

                            (pData.Bag.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                            pData.Bag.Items[slotFrom].Update();

                            cont.Items[slotTo] = await Game.Items.Items.CreateItem(pData.Bag.Items[slotFrom].ID, 0, amount); // but wait for that :)

                            wasCreated = true;
                        }
                        #endregion
                        #region Replace
                        else
                        {
                            var addWeightItems = cont.Items[slotTo] != null ? Game.Items.Items.GetItemWeight(cont.Items[slotTo], true) : 0;
                            var addWeightBag = Game.Items.Items.GetItemWeight(pData.Bag.Items[slotFrom], true);

                            if (addWeightBag - addWeightItems + curWeight > maxWeight || addWeightItems - addWeightBag + pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight > pData.Bag.Data.MaxWeight)
                                return CEF.Inventory.Results.NoSpace;

                            var temp = pData.Bag.Items[slotFrom];
                            pData.Bag.Items[slotFrom] = cont.Items[slotTo];
                            cont.Items[slotTo] = temp;

                            wasDeleted = true; wasCreated = true;
                        }
                        #endregion

                        var upd1 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotFrom], CEF.Inventory.Groups.Bag);
                        var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], CEF.Inventory.Groups.Container);

                        NAPI.Task.Run(() =>
                        {
                            player?.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Bag, slotFrom, upd1);

                            foreach (var x in cont.PlayersObserving.ToList())
                            {
                                if (x?.Exists != true)
                                    continue;

                                x.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotTo, upd2);
                            }
                        });

                        if (wasDeleted)
                            pData.Bag.Update();

                        if (wasCreated)
                            cont.Update();

                        return CEF.Inventory.Results.Success;
                    }
                    #endregion

                    #region From Container
                    else if (from == CEF.Inventory.Groups.Container)
                    {
                        if (slotFrom >= cont.Items.Length || cont.Items[slotFrom] == null)
                            return CEF.Inventory.Results.Error;

                        #region To Pockets
                        if (to == CEF.Inventory.Groups.Items)
                        {
                            if (slotTo >= pData.Items.Length)
                                return CEF.Inventory.Results.Error;

                            if (!cont.IsItemAllowed(pData.Items[slotTo]))
                                return CEF.Inventory.Results.Error;

                            float curWeight = Game.Items.Items.GetWeight(pData.Items);

                            bool wasDeleted = false, wasCreated = false;

                            #region Unite
                            if (pData.Items[slotTo] != null && pData.Items[slotTo].Type == cont.Items[slotFrom].Type && cont.Items[slotFrom] is Game.Items.IStackable)
                            {
                                int slotToAmount = (pData.Items[slotTo] as Game.Items.IStackable).Amount;
                                int slotFromAmount = (cont.Items[slotFrom] as Game.Items.IStackable).Amount;

                                if (amount == -1 || amount > slotFromAmount)
                                    amount = slotFromAmount;

                                int maxStack = (cont.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                                if (slotToAmount == maxStack)
                                    return CEF.Inventory.Results.Error;

                                // if amount*weight is too big -> reduce amount to fit the pData.Items's maxWeight
                                if (curWeight + amount * cont.Items[slotFrom].Weight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / cont.Items[slotFrom].Weight);

                                    if (amount == 0)
                                        return CEF.Inventory.Results.NoSpace;
                                }

                                if (slotToAmount + amount > maxStack)
                                {
                                    (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                    (pData.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                                }
                                else
                                {
                                    (pData.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                    (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                    if ((cont.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                    {
                                        var tItem = cont.Items[slotFrom];

                                        Task.Run(() => tItem.Delete());

                                        cont.Items[slotFrom] = null;

                                        wasDeleted = true;
                                    }
                                }

                                pData.Items[slotTo].Update();
                                cont.Items[slotFrom]?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (cont.Items[slotFrom] is Game.Items.IStackable && pData.Items[slotTo] == null && amount != -1 && amount < (cont.Items[slotFrom] as Game.Items.IStackable).Amount)
                            {
                                if (cont.Items[slotFrom].Weight * amount + curWeight > Settings.MAX_INVENTORY_WEIGHT)
                                {
                                    amount = (int)Math.Floor((Settings.MAX_INVENTORY_WEIGHT - curWeight) / cont.Items[slotFrom].Weight);

                                    if (amount == 0)
                                        return CEF.Inventory.Results.NoSpace;
                                }

                                (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                                cont.Items[slotFrom].Update();

                                pData.Items[slotTo] = await Game.Items.Items.CreateItem(cont.Items[slotFrom].ID, 0, amount); // but wait for that :)

                                wasCreated = true;
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = pData.Items[slotTo] != null ? Game.Items.Items.GetItemWeight(pData.Items[slotTo], true) : 0;
                                var addWeightBag = Game.Items.Items.GetItemWeight(cont.Items[slotFrom], true);

                                if (addWeightBag - addWeightItems + curWeight > Settings.MAX_INVENTORY_WEIGHT || addWeightItems - addWeightBag + cont.Weight > cont.MaxWeight)
                                    return CEF.Inventory.Results.NoSpace;

                                var temp = cont.Items[slotFrom];
                                cont.Items[slotFrom] = pData.Items[slotTo];
                                pData.Items[slotTo] = temp;

                                wasDeleted = true; wasCreated = true;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], CEF.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Items[slotTo], CEF.Inventory.Groups.Items);

                            NAPI.Task.Run(() =>
                            {
                                player?.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Items, slotTo, upd2);

                                foreach (var x in cont.PlayersObserving.ToList())
                                {
                                    if (x?.Exists != true)
                                        continue;

                                    x.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotFrom, upd1);
                                }
                            });

                            if (wasCreated)
                                MySQL.UpdatePlayerInventory(pData, true);

                            if (wasDeleted)
                                cont.Update();

                            return CEF.Inventory.Results.Success;
                        }
                        #endregion

                        #region To Bag
                        else if (to == CEF.Inventory.Groups.Bag)
                        {
                            if (pData.Bag == null || slotTo >= pData.Bag.Items.Length)
                                return CEF.Inventory.Results.Error;

                            if (!cont.IsItemAllowed(pData.Bag.Items[slotTo]))
                                return CEF.Inventory.Results.Error;

                            float curWeight = pData.Bag.Weight - (pData.Bag as Game.Items.Item).Weight;
                            float maxWeight = pData.Bag.Data.MaxWeight;

                            bool wasDeleted = false, wasCreated = false;

                            #region Unite
                            if (pData.Bag.Items[slotTo] != null && pData.Bag.Items[slotTo].Type == cont.Items[slotFrom].Type && cont.Items[slotFrom] is Game.Items.IStackable)
                            {
                                int slotToAmount = (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount;
                                int slotFromAmount = (cont.Items[slotFrom] as Game.Items.IStackable).Amount;

                                if (amount == -1 || amount > slotFromAmount)
                                    amount = slotFromAmount;

                                int maxStack = (cont.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                                if (slotToAmount == maxStack)
                                    return CEF.Inventory.Results.Error;

                                // if amount*weight is too big -> reduce amount to fit the pData.Bag.Items's maxWeight
                                if (curWeight + amount * cont.Items[slotFrom].Weight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / cont.Items[slotFrom].Weight);

                                    if (amount == 0)
                                        return CEF.Inventory.Results.NoSpace;
                                }

                                if (slotToAmount + amount > maxStack)
                                {
                                    (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                    (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                                }
                                else
                                {
                                    (pData.Bag.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                    (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                    if ((cont.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                    {
                                        var tItem = cont.Items[slotFrom];

                                        Task.Run(() => tItem.Delete());

                                        cont.Items[slotFrom] = null;

                                        wasDeleted = true;
                                    }
                                }

                                pData.Bag.Items[slotTo].Update();
                                cont.Items[slotFrom]?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (cont.Items[slotFrom] is Game.Items.IStackable && pData.Bag.Items[slotTo] == null && amount != -1 && amount < (cont.Items[slotFrom] as Game.Items.IStackable).Amount)
                            {
                                if (cont.Items[slotFrom].Weight * amount + curWeight > maxWeight)
                                {
                                    amount = (int)Math.Floor((maxWeight - curWeight) / cont.Items[slotFrom].Weight);

                                    if (amount == 0)
                                        return CEF.Inventory.Results.NoSpace;
                                }

                                (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                                cont.Items[slotFrom].Update();

                                pData.Bag.Items[slotTo] = await Game.Items.Items.CreateItem(cont.Items[slotFrom].ID, 0, amount); // but wait for that :)

                                wasCreated = true;
                            }
                            #endregion
                            #region Replace
                            else
                            {
                                var addWeightItems = pData.Bag.Items[slotTo] != null ? Game.Items.Items.GetItemWeight(pData.Bag.Items[slotTo], true) : 0;
                                var addWeightBag = Game.Items.Items.GetItemWeight(cont.Items[slotFrom], true);

                                if (addWeightBag - addWeightItems + curWeight > maxWeight || addWeightItems - addWeightBag + cont.Weight > cont.MaxWeight)
                                    return CEF.Inventory.Results.NoSpace;

                                var temp = cont.Items[slotFrom];
                                cont.Items[slotFrom] = pData.Bag.Items[slotTo];
                                pData.Bag.Items[slotTo] = temp;

                                wasDeleted = true; wasCreated = true;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], CEF.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(pData.Bag.Items[slotTo], CEF.Inventory.Groups.Bag);

                            NAPI.Task.Run(() =>
                            {
                                player?.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Bag, slotTo, upd2);

                                foreach (var x in cont.PlayersObserving.ToList())
                                {
                                    if (x?.Exists != true)
                                        continue;

                                    x.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotFrom, upd1);
                                }
                            });

                            if (wasCreated)
                                pData.Bag.Update();

                            if (wasDeleted)
                                cont.Update();

                            return CEF.Inventory.Results.Success;
                        }
                        #endregion

                        #region To Container
                        else if (to == CEF.Inventory.Groups.Container)
                        {
                            if (slotTo >= cont.Items.Length)
                                return CEF.Inventory.Results.Error;

                            bool wasCreated = false;
                            bool wasDeleted = false;

                            #region Unite
                            if (cont.Items[slotTo] != null && cont.Items[slotTo].Type == cont.Items[slotFrom].Type && cont.Items[slotFrom] is Game.Items.IStackable)
                            {
                                int slotToAmount = (cont.Items[slotTo] as Game.Items.IStackable).Amount;
                                int slotFromAmount = (cont.Items[slotFrom] as Game.Items.IStackable).Amount;

                                // if no amount requested -> suggest it's whole item
                                if (amount == -1 || amount > slotFromAmount)
                                    amount = slotFromAmount;

                                int maxStack = (cont.Items[slotFrom] as Game.Items.IStackable).MaxAmount;

                                if (slotToAmount == maxStack)
                                    return CEF.Inventory.Results.Error;

                                // if new amount > maxStack -> reduce new amount
                                if (slotToAmount + amount > maxStack)
                                {
                                    (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= maxStack - slotToAmount;
                                    (cont.Items[slotTo] as Game.Items.IStackable).Amount = maxStack;
                                }
                                else // if new amount <= maxStack
                                {
                                    (cont.Items[slotTo] as Game.Items.IStackable).Amount += amount;
                                    (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;

                                    // delete old item if amount is 0 now
                                    if ((cont.Items[slotFrom] as Game.Items.IStackable).Amount == 0)
                                    {
                                        wasDeleted = true;

                                        cont.Items[slotFrom].Delete(); // don't wait for it :)

                                        cont.Items[slotFrom] = null;
                                    }
                                }

                                cont.Items[slotTo].Update();
                                cont.Items[slotFrom]?.Update();
                            }
                            #endregion
                            #region Split To New
                            else if (cont.Items[slotFrom] is Game.Items.IStackable && cont.Items[slotTo] == null && amount != -1 && amount < (cont.Items[slotFrom] as Game.Items.IStackable).Amount) // split to new item
                            {
                                wasCreated = true;

                                (cont.Items[slotFrom] as Game.Items.IStackable).Amount -= amount;
                                cont.Items[slotFrom].Update();

                                cont.Items[slotTo] = await Game.Items.Items.CreateItem(cont.Items[slotFrom].ID, 0, amount);
                            }
                            #endregion
                            #region Replace
                            else // replace
                            {
                                var temp = cont.Items[slotFrom];
                                cont.Items[slotFrom] = cont.Items[slotTo];
                                cont.Items[slotTo] = temp;

                                wasCreated = true;
                            }
                            #endregion

                            var upd1 = Game.Items.Item.ToClientJson(cont.Items[slotFrom], CEF.Inventory.Groups.Container);
                            var upd2 = Game.Items.Item.ToClientJson(cont.Items[slotTo], CEF.Inventory.Groups.Container);

                            NAPI.Task.Run(() =>
                            {
                                foreach (var x in cont.PlayersObserving.ToList())
                                {
                                    if (x?.Exists != true)
                                        continue;

                                    x.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotFrom, upd1);
                                    x.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slotTo, upd2);
                                }
                            });

                            if (wasCreated || wasDeleted)
                                cont.Update();

                            return CEF.Inventory.Results.Success;
                        }
                        #endregion
                    }

                    return CEF.Inventory.Results.Error;
                    #endregion
                });

                if (res != CEF.Inventory.Results.Success && res != CEF.Inventory.Results.Error)
                {
                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        if (res == CEF.Inventory.Results.NoSpace)
                            player.Notify("Inventory::NoSpace");
                        else if (res == CEF.Inventory.Results.PlaceRestricted)
                            player.Notify("Inventory::PlaceRestricted");
                        else if (res == CEF.Inventory.Results.TempItem)
                            player.Notify("Inventory::ItemIsTemp");
                    });
                }

                cont.Release();
            });

            pData.Release();
        }
        #endregion

        #region Drop
        [RemoteEvent("Container::Drop")]
        private static async Task Drop(Player player, int slot, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentContainer == null || amount < 1 || slot < 0)
                    return;

                var cont = Get((uint)pData.CurrentContainer);

                if (!await cont.WaitAsync())
                    return;

                await Task.Run(async () =>
                {
                    var items = cont.Items;

                    if (slot >= items.Length || items[slot] == null)
                        return;

                    var res = await NAPI.Task.RunAsync(() =>
                    {
                        if (player?.Exists != true)
                            return false;

                        var res = cont.IsNear(player) && cont.IsAccessableFor(pData);

                        if (!res)
                            player.TriggerEvent("Inventory::Close");

                        return res;
                    });

                    var item = items[slot];

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

                    var upd = Game.Items.Item.ToClientJson(items[slot], CEF.Inventory.Groups.Container);

                    NAPI.Task.Run(() =>
                    {
                        foreach (var x in cont.PlayersObserving.ToList())
                        {
                            if (x?.Exists != true)
                                return;

                            x.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Container, slot, upd);
                        }
                    });

                    cont.Update();

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

                cont.Release();
            });

            pData.Release();
        }
        #endregion

        #region Close
        [RemoteEvent("Container::Close")]
        private static async Task OnClose(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!await pData.WaitAsync())
                return;

            await Task.Run(async () =>
            {
                if (pData.CurrentContainer == null)
                    return;

                var cont = Get((uint)pData.CurrentContainer);

                if (!await cont.WaitAsync())
                    return;

                cont.RemovePlayerObserving(player);

                cont.Release();
            });

            pData.Release();
        }
        #endregion
    }
}
