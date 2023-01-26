﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer.Game.Items
{
    public partial class Container
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

            { AllowedItemTypes.Fridge, typeof(Game.Items.Food) },
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
        public float MaxWeight => ContData.MaxWeight;

        /// <summary>Тип предметов, доступных для хранения в контейнере</summary>
        private Type AllowedItemType => AllowedItemsDict[ContData.AllowedItemsType];

        /// <summary>Тип контейнера</summary>
        private ContainerTypes ContainerType => ContData.ContainerType;

        /// <summary>Текущий общий вес контейнера</summary>
        public float Weight => Items.Sum(x => x?.Weight ?? 0f);

        public Data ContData { get; set; }

        /// <summary>Сущность держателя контейнера</summary>
        public Entity Entity { get; set; }

        /// <summary>Метод для проверки, есть ли у игрока возможность взаимодействовать с контейнером</summary>
        public bool IsAccessableFor(PlayerData pData)
        {
            var player = pData.Player;

            if (Entity?.Exists == true)
            {
                if (Entity is Vehicle veh)
                {
                    var vData = veh.GetMainData();

                    if (vData == null)
                        return false;

                    if (vData.IsOwner(pData) != null)
                        return true;

                    return !vData.TrunkLocked;
                }

                return false;
            }
            else
            {
                return PermissionCheckFuncs.GetValueOrDefault(SID)?.Invoke(this, pData) ?? false;
            }
        }

        /// <summary>Метод для проверки, рядом ли игрок (см. MaxDistance)</summary>
        public bool IsNear(PlayerData pData)
        {
            var player = pData.Player;

            if (Entity?.Exists == true)
            {
                return player.AreEntitiesNearby(Entity, Settings.ENTITY_INTERACTION_MAX_DISTANCE);
            }
            else
            {
                return NearnessCheckFuncs.GetValueOrDefault(SID)?.Invoke(this, pData) ?? false;
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

        /// <summary>Метод для добавления игрока в качестве смотрящего контейнер</summary>
        /// <returns>true - если игрок был успешно добавлен, false - в противном случае</returns>
        public bool AddPlayerObserving(PlayerData pData)
        {
            PlayersObserving.ForEach(x =>
            {
                var target = x?.Player;

                if (target?.Exists != true || !target.AreEntitiesNearby(Entity, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                    RemovePlayerObserving(x, true);
            });

            if (PlayersObserving.Count >= Settings.CONTAINER_MAX_PLAYERS)
                return false;

            PlayersObserving.Add(pData);

            pData.CurrentContainer = this;

            return true;
        }

        /// <summary>Метод для удаления игрока в качестве смотрящего контейнер</summary>
        public void RemovePlayerObserving(PlayerData pData, bool callRemoteClose)
        {
            var player = pData.Player;

            PlayersObserving.Remove(pData);

            pData.CurrentContainer = null;

            if (callRemoteClose)
                player.TriggerEvent("Inventory::Close");
        }

        /// <summary>Метод для очистки всех игроков, смотрящих контейнер</summary>
        public void ClearAllObservers()
        {
            var players = new List<Player>();

            PlayersObserving.ForEach(x =>
            {
                x.CurrentWorkbench = null;

                if (x.Player?.Exists == true)
                {
                    players.Add(x.Player);
                }
            });

            if (players.Count > 0)
                NAPI.ClientEvent.TriggerClientEventToPlayers(players.ToArray(), "Inventory::Close");
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

            for (int i = 0; i < Items.Length; i++)
            {
                Items[i].Delete();

                Items[i] = null;
            }

            Update();
        }

        /// <summary>Метод для проверки, допустим ли предмет для хранения в контейнере</summary>
        public bool IsItemAllowed(Game.Items.Item item)
        {
            if (item == null)
                return true;

            return AllowedItemType.IsAssignableFrom(item.Type);
        }

        /// <summary>Метод для обновления сущности держателя контейнера</summary>
        /// <param name="owner">Сущность держателя</param>
        public void UpdateOwner(Entity owner)
        {
            Entity = owner;
        }

        public Player[] GetPlayersObservingArray() => PlayersObserving.Where(x => x.Player?.Exists == true).Select(x => x.Player).ToArray();

        public string ToClientJson() => $"{(int)ContainerType}&{MaxWeight}|{string.Join('|', Items.Select(x => Game.Items.Item.ToClientJson(x, Game.Items.Inventory.Groups.Container)))}";
    }
}