﻿using BlaineRP.Server.Sync;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using BlaineRP.Server.Additional;
using BlaineRP.Server.Extensions.GTANetworkAPI;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.Containers;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Estates;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.AntiCheat;
using BlaineRP.Server.Game.Management.Chat;
using BlaineRP.Server.Game.Management.Misc;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server
{
    public static partial class Utils
    {
        public static class Demorgan
        {
            private static Vector3[] Positions = new Vector3[]
            {
                new Vector3(5345.206f, -5219.23f, 82.77666f),
                new Vector3(5345.876f, -5232.199f, 82.81819f),
                new Vector3(5354.328f, -5232.279f, 82.82253f),
                new Vector3(5357.439f, -5217.218f, 82.85942f),
            };

            private static int LastPosUsed { get; set; }

            public static Vector3 GetNextPos()
            {
                var pos = LastPosUsed >= Positions.Length ? Positions[LastPosUsed = 0] : Positions[LastPosUsed++];

                return new Vector3(pos.X, pos.Y, pos.Z);
            }

            public static void SetToDemorgan(PlayerData pData, bool justTeleport)
            {
                if (!justTeleport)
                {
                    pData.RemoveAllWeapons(true, true);
                }

                var pos = GetNextPos();

                pData.Player.Teleport(pos, false, Properties.Settings.Static.DemorganDimension, null, false);
            }

            public static void SetFromDemorgan(PlayerData pData)
            {
                pData.Player.Teleport(Utils.DefaultSpawnPosition, false, Properties.Settings.Static.MainDimension, Utils.DefaultSpawnHeading, false);
            }
        }

        public static uint GetPlayerIdByDimension(uint dim) => dim < Properties.Settings.Profile.Current.Game.PlayerPrivateDimensionBaseOffset ? 0 : dim - Properties.Settings.Profile.Current.Game.PlayerPrivateDimensionBaseOffset;

        public static uint GetHouseIdByDimension(uint dim) => dim < Properties.Settings.Profile.Current.Game.HouseDimensionBaseOffset ? 0 : dim - Properties.Settings.Profile.Current.Game.HouseDimensionBaseOffset;

        public static uint GetGarageIdByDimension(uint dim) => dim < Properties.Settings.Profile.Current.Game.GarageDimensionBaseOffset ? 0 : dim - Properties.Settings.Profile.Current.Game.GarageDimensionBaseOffset;

        public static uint GetApartmentsIdByDimension(uint dim) => dim < Properties.Settings.Profile.Current.Game.ApartmentsDimensionBaseOffset ? 0 : dim - Properties.Settings.Profile.Current.Game.ApartmentsDimensionBaseOffset;

        public static Game.Estates.HouseBase GetHouseBaseByDimension(uint dim)
        {
            var hid = GetApartmentsIdByDimension(dim);

            if (hid == 0)
            {
                hid = GetHouseIdByDimension(dim);

                if (hid == 0)
                    return null;

                return Game.Estates.House.Get(hid);
            }

            return Game.Estates.Apartments.Get(hid);
        }

        public static ApartmentsRoot GetApartmentsRootByDimension(uint dim)
        {
            var rootId = GetApartmentsRootIdByDimension(dim);

            if (rootId <= 0)
                return null;

            return ApartmentsRoot.Get(rootId);
        }

        public static Game.Estates.Garage GetGarageByDimension(uint dim)
        {
            var gId = GetGarageIdByDimension(dim);

            if (gId == 0)
                return null;

            return Game.Estates.Garage.Get(gId);
        }

        public static uint GetApartmentsRootIdByDimension(uint dim) => dim < Properties.Settings.Profile.Current.Game.ApartmentsRootDimensionBaseOffset ? 0 : dim - Properties.Settings.Profile.Current.Game.ApartmentsRootDimensionBaseOffset;

        /// <summary>Стандартная позиция спавна</summary>
        public static Vector3 DefaultSpawnPosition = new Vector3(-749.78f, 5818.21f, 17f);
        /// <summary>Стандартный поворот</summary>
        public static float DefaultSpawnHeading = 0f;

        public static uint GetPrivateDimension(Player player) => player.Id + Properties.Settings.Profile.Current.Game.PlayerPrivateDimensionBaseOffset;

        /// <summary>Нулевой вектор (X=0, Y=0, Z=0)</summary>
        public static Vector3 ZeroVector => new Vector3(0, 0, 0);

        public enum NotificationTypes
        {
            /// <summary>Информация (синий)</summary>
            Information = 0,
            /// <summary>Вопрос (жёлтый)</summary>
            Question,
            /// <summary>Успех (зелёный)</summary>
            Success,
            /// <summary>Ошибка (красный)</summary>
            Error,
            /// <summary>Наличные (зелёный)</summary>
            Cash,
            /// <summary>Банк (фиолетовый)</summary>
            Bank,
            /// <summary>Предложение (жёлтый)</summary>
            Offer,
            /// <summary>Подарок (розовый)</summary>
            Gift,
            /// <summary>Предмет (синий)</summary>
            Item,
            /// <summary>Достижение (золотой)</summary>
            Achievement,
            /// <summary>NPC (синий)</summary>
            NPC,

            /// <summary>Мут (красный)</summary>
            Mute,
            /// <summary>Тюрьма (красный)</summary>
            Jail1,
            /// <summary>Блокировка (красный)</summary>
            Ban,
            /// <summary>Предупреждение (оранжевый)</summary>
            Warn,

            /// <summary>Наручники (оранжевый)</summary>
            Cuffs,
            /// <summary>Тюрьма (оранжевый)</summary>
            Jail2,
        }

        /// <summary>Получить текущее время (по МСК.)</summary>
        public static DateTime GetCurrentTime() => DateTime.UtcNow.AddHours(3);

        /// <summary>Тихий кик игрока (со стороны сервера)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="reason">Причина кика</param>
        public static void Kick(Player player, string reason)
        {
            if (reason != null)
                player.Notify("Kick", reason);

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                player.KickSilent(reason);
            }, 1);
        }

        #region Vehicle Stuff

        /// <summary>Получить сущность транспорта по ID</summary>
        /// <param name="id">Remote ID</param>
        /// <returns>Объект класса Vehicle, если транспорт найден на сервере, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Vehicle GetVehicleByID(int id) => NAPI.Pools.GetAllVehicles().Where(x => x.Id == id).FirstOrDefault();

        /// <summary>Метод для получения сущности транспорта, который существует</summary>
        /// <param name="vid">VID или RemoteID</param>
        /// <returns>Объект класса VehicleData, если транспорт найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static VehicleData FindVehicleOnline(uint vid) => vid >= Properties.Settings.Profile.Current.Game.VIDBaseOffset ? VehicleData.All.Values.Where(x => x.VID == vid).FirstOrDefault() : VehicleData.All.Values.Where(x => x.Vehicle.Id == vid).FirstOrDefault();

        #endregion

        #region Main Thread Help

        public static void TriggerEventToStreamed(this Entity entity, string eventName, params object[] args) => TriggerEventInDistance(entity.Position, entity.Dimension, Properties.Settings.Profile.Current.Game.StreamDistance, eventName, args);

        public static void TriggerEventInDistance(this Entity entity, float distance, string eventName, params object[] args) => TriggerEventInDistance(entity.Position, entity.Dimension, distance, eventName, args);

        public static void TriggerEventInDistance2d(this Entity entity, float distance, string eventName, params object[] args) => TriggerEventInDistance2d(entity.Position, entity.Dimension, distance, eventName, args);

        public static void TriggerEventInDistance(this Vector3 pos, uint dimension, float distance, string eventName, params object[] args)
        {
            var pArr = PlayerData.All.Keys.Where(x => x.Dimension == dimension && x.Position.DistanceTo(pos) <= distance).ToArray();

            if (pArr.Length == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(pArr, eventName, args);
        }

        public static void TriggerEventInDistance2d(this Vector3 pos, uint dimension, float distance, string eventName, params object[] args)
        {
            var pArr = PlayerData.All.Keys.Where(x => x.Dimension == dimension && x.Position.DistanceIgnoreZ(pos) <= distance).ToArray();

            if (pArr.Length == 0)
                return;

            NAPI.ClientEvent.TriggerClientEventToPlayers(pArr, eventName, args);
        }

        public static bool IsNearToEntity(this Entity entity, Entity target, float radius) => (entity.Dimension == target.Dimension && Vector3.Distance(entity.Position, target.Position) <= radius);

        public static bool IsNearToEntityDifferentDimension(this Entity entity, Entity target, float radius) => (Vector3.Distance(entity.Position, target.Position) <= radius);

        public static Entity GetEntityById(EntityType eType, ushort id)
        {
            if (eType == EntityType.Player)
                return NAPI.Pools.GetAllPlayers().Where(x => x.Id == id).FirstOrDefault();

            if (eType == EntityType.Vehicle)
                return NAPI.Pools.GetAllVehicles().Where(x => x.Id == id).FirstOrDefault();

            if (eType == EntityType.Ped)
                return NAPI.Pools.GetAllPeds().Where(x => x.Id == id).FirstOrDefault();

            if (eType == EntityType.Object)
                return NAPI.Pools.GetAllObjects().Where(x => x.Id == id).FirstOrDefault();

            return null;
        }
        #endregion

        #region Client Utils

        public static void Teleport(this Vehicle vehicle, Vector3 position, uint? dimension = null, float? heading = null, bool fade = false, VehicleTeleportType tpType = VehicleTeleportType.Default, bool toGround = false) => Game.Management.AntiCheat.Service.TeleportVehicle(vehicle, position, dimension, heading, fade, tpType, toGround);

        /// <inheritdoc cref="Game.Management.AntiCheat.Service.SetPlayerPos(Player, Vector3, bool, uint?)"/>
        public static void Teleport(this Player player, Vector3 position, bool toGround, uint? dimension = null, float? heading = null, bool fade = false) => Game.Management.AntiCheat.Service.TeleportPlayers(position, toGround, dimension, heading, fade, false, player.Dimension, player);

        public static void TeleportPlayers(Vector3 position, bool toGround, uint? dimension = null, float? heading = null, bool fade = false, params Player[] players) => Game.Management.AntiCheat.Service.TeleportPlayers(position, toGround, dimension, heading, fade, false, null, players);

        /// <inheritdoc cref="Game.Management.AntiCheat.Service.SetPlayerInvincible(Player, bool)"/>
        public static void SetInvincible(this Player player, bool state) => Game.Management.AntiCheat.Service.SetPlayerInvincible(player, state);

        /// <inheritdoc cref="Game.Management.AntiCheat.Service.SetPlayerHealth(Player, int)"/>
        public static void SetHealth(this Player player, int value) => Game.Management.AntiCheat.Service.SetPlayerHealth(player, value);

        /// <inheritdoc cref="Game.Management.AntiCheat.Service.SetPlayerArmour(Player, int)"/>
        public static void SetArmour(this Player player, int value) => Game.Management.AntiCheat.Service.SetPlayerArmour(player, value);

        /// <inheritdoc cref="Game.Management.AntiCheat.Service.SetPlayerAlpha(Player, int)"/>
        public static void SetAlpha(this Player player, int value) => Game.Management.AntiCheat.Service.SetPlayerAlpha(player, value);

        /// <inheritdoc cref="Game.Management.AntiCheat.Service.SetPlayerWeapon(Player, WeaponHash, int)"/>
        public static void SetWeapon(this Player player, uint hash, int ammo = 0) => Game.Management.AntiCheat.Service.SetPlayerWeapon(player, hash, ammo);

        /// <inheritdoc cref="Game.Management.AntiCheat.Service.SetPlayerAmmo(Player, int)"/>
        public static void SetAmmo(this Player player, int ammo = 0) => Game.Management.AntiCheat.Service.SetPlayerAmmo(player, ammo);

        /// <summary>Метод для установки причёски игрока</summary>
        /// <remarks>Если ID причёски не существует, то будет установлена стандартная причёска!</remarks>
        /// <param name="player">Сущность игрока</param>
        /// <param name="id">ID причёски (см. Game.Data.Customization.AllHairs</param>
        public static void SetHair(this Player player, int id) => player?.SetClothes(2, Game.EntitiesData.Players.Customization.Service.GetHair(player.Model == (uint)PedHash.FreemodeMale01, id), 0);

        /// <summary>Получить сущность игрока по ID</summary>
        /// <param name="id">Remote ID</param>
        /// <returns>Объект класса Player, если игрок найден на сервере, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Player GetPlayerByID(ushort id) => NAPI.Pools.GetAllPlayers().Where(x => x.Id == id).FirstOrDefault();

        /// <summary>Получить сущность игрока по CID</summary>
        /// <param name="cid">Character ID</param>
        /// <returns>Объект класса Player, если игрок найден на сервере, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static PlayerData GetPlayerByCID(uint cid) => PlayerData.All.Values.Where(x => x.CID == cid).FirstOrDefault();

        //public static Player GetPlayerByName(string name) => NAPI.Player.GetPlayerFromName(name);

        /// <summary>Метод для получения сущности игрока, который в сети</summary>
        /// <param name="pid">CID или RemoteID</param>
        /// <returns>Объект класса PlayerData, если игрок найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static PlayerData FindReadyPlayerOnline(uint pid) => pid >= Properties.Settings.Profile.Current.Game.CIDBaseOffset ? PlayerData.All.Values.Where(x => x.CID == pid).FirstOrDefault() : PlayerData.All.Where(x => x.Key.Id == pid).Select(x => x.Value).FirstOrDefault();

        /// <summary>Метод для получения сущности игрока, который в сети</summary>
        /// <param name="pid">CID или RemoteID</param>
        /// <returns>Объект класса Player, если игрок найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Player FindPlayerOnline(uint pid) => pid >= Properties.Settings.Profile.Current.Game.CIDBaseOffset ? PlayerData.All.Values.Where(x => x.CID == pid).Select(x => x.Player).FirstOrDefault() : NAPI.Pools.GetAllPlayers().Where(x => x?.Id == pid).FirstOrDefault();

        /// <summary>Метод для получения пола игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <returns>true - мужчина, false - женщина</returns>
        public static bool GetSex(this Player player) => player?.Model == (uint)PedHash.FreemodeMale01;

        /// <summary>Метод для получения прототипа сущности игрока, который, возможно, не в сети</summary>
        /// <param name="cid">CID</param>
        /// <returns>Объект класса Player, если игрок найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static PlayerInfo FindPlayerOffline(uint cid) => PlayerInfo.Get(cid);

        /// <inheritdoc cref="AntiSpam.CheckNormal(Player, int)"/>
        public static (bool IsSpammer, PlayerData Data) CheckSpamAttack(this Player player, int decreaseDelay = 250, bool checkPlayerReady = true) { Console.WriteLine((new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name); return AntiSpam.CheckNormal(player, decreaseDelay, checkPlayerReady); }
        /// <inheritdoc cref="AntiSpam.CheckTemp(Player, int)"/>
        public static (bool IsSpammer, TempData Data) CheckSpamAttackTemp(this Player player, int decreaseDelay = 250) => AntiSpam.CheckTemp(player, decreaseDelay);

        /// <inheritdoc cref="Game.Inventory.Service.Replace(PInventorys.Inventory.GroupTInventorys.Inventory.GroupTypes, int, int)"/>
        public static Game.Inventory.Service.ResultTypes InventoryReplace(this PlayerData pData, GroupTypes to, int slotTo, GroupTypes from, int slotFrom, int amount = -1) => Game.Inventory.Service.Replace(pData, to, slotTo, from, slotFrom, amount);

        /// <inheritdoc cref="Game.Inventory.Service.Action(PInventorys.Inventory.GroupTypes, int, int, object[])"/>
        public static Game.Inventory.Service.ResultTypes InventoryAction(this PlayerData pData, GroupTypes slotStr, int slot, int action = 5, params string[] args) => Game.Inventory.Service.Action(pData, slotStr, slot, action, args);

        /// <inheritdoc cref="Game.Inventory.Service.Drop(PInventorys.Inventory.GroupTypes, int, int)"/>
        public static void InventoryDrop(this PlayerData pData, GroupTypes slotStr, int slot, int amount) => Game.Inventory.Service.Drop(pData, slotStr, slot, amount);

        public static bool TryGiveExistingItem(this PlayerData pData, Game.Items.Item item, int amount, bool notifyOnFail = false, bool notifyOnSuccess = false) => Game.Inventory.Service.GiveExisting(pData, item, amount, notifyOnFail, notifyOnSuccess);

        public static bool GiveItem(this PlayerData pData, out Game.Items.Item item, string id, int variation = 0, int amount = 1, bool notifyOnSuccess = true, bool notifyOnFault = true) => Game.Items.Stuff.GiveItem(pData, out item, id, variation, amount, notifyOnSuccess, notifyOnFault);

        public static bool GiveItemDropExcess(this PlayerData pData, out Game.Items.Item item, string id, int variation = 0, int amount = 1, bool notifyOnSuccess = true, bool notifyOnFault = true) => Game.Items.Stuff.GiveItemDropExcess(pData, out item, id, variation, amount, notifyOnSuccess, notifyOnFault);

        /// <summary>Метод для удаления всего оружия у игрока</summary>
        /// <param name="pData">PlayerData игрока</param>
        /// <param name="fromInventoryToo">Удалить ли всё оружие из инвентаря тоже?</param>
        /// <param name="fromBagToo">Удалить ли всё оружие из надетой сумки тоже?</param>
        /// <returns></returns>
        public static void RemoveAllWeapons(this PlayerData pData, bool fromInventoryToo = false, bool fromBagToo = false)
        {
            pData.UnequipActiveWeapon();

            var updList = new List<(GroupTypes Group, int Slot)>();

            for (int i = 0; i < pData.Weapons.Length; i++)
            {
                if (pData.Weapons[i] != null)
                {
                    pData.Weapons[i].Unwear(pData);

                    pData.Weapons[i].Delete();

                    pData.Weapons[i] = null;

                    updList.Add((GroupTypes.Weapons, i));
                }
            }

            MySQL.CharacterWeaponsUpdate(pData.Info);

            if (pData.Holster?.Items[0] is Game.Items.Weapon)
            {
                pData.Holster.Items[0].Delete();

                updList.Add((GroupTypes.Holster, 2));

                pData.Holster.Update();
            }

            if (fromInventoryToo)
            {
                for (int i = 0; i < pData.Items.Length; i++)
                {
                    if (pData.Items[i] is Game.Items.Weapon)
                    {
                        pData.Items[i].Delete();

                        pData.Items[i] = null;

                        updList.Add((GroupTypes.Items, i));
                    }
                }

                MySQL.CharacterItemsUpdate(pData.Info);
            }

            if (fromBagToo && pData.Bag != null)
            {
                for (int i = 0; i < pData.Bag.Items.Length; i++)
                {
                    if (pData.Bag.Items[i] is Game.Items.Weapon)
                    {
                        pData.Bag.Items[i].Delete();

                        pData.Bag.Items[i] = null;

                        updList.Add((GroupTypes.Bag, i));
                    }
                }

                pData.Bag.Update();
            }

            foreach (var x in updList)
                pData.Player.InventoryUpdate(x.Group, x.Slot, Game.Items.Item.ToClientJson(null, x.Group));
        }


        public static List<(Game.Items.Item Item, GroupTypes Group, int Slot)> TakeWeapons(this PlayerData pData)
        {
            pData.UnequipActiveWeapon();

            var tempItems = pData.TempItems;

            if (tempItems == null)
                tempItems = new List<(Game.Items.Item, GroupTypes, int)>();

            for (int i = 0; i < pData.Weapons.Length; i++)
            {
                if (pData.Weapons[i] is Game.Items.Weapon weapon)
                {
                    pData.Weapons[i] = null;

                    tempItems.Add((weapon, GroupTypes.Weapons, i));

                    pData.Player.InventoryUpdate(GroupTypes.Weapons, i, Game.Items.Item.ToClientJson(null, GroupTypes.Weapons));
                }
            }

            if (tempItems.Count == 0)
                return null;

            pData.TempItems = tempItems;

            return tempItems;
        }

        public static void GiveTakenItems(this PlayerData pData)
        {
            var takenItems = pData.TempItems;

            if (takenItems == null)
                return;

            foreach (var x in takenItems)
            {
                if (x.Group == GroupTypes.Weapons)
                {
                    pData.Weapons[x.Slot] = x.Item as Game.Items.Weapon;
                }

                pData.Player.InventoryUpdate(x.Group, x.Slot, x.Item.ToClientJson(x.Group));
            }

            pData.TempItems = null;
        }

        public static Game.Items.Weapon GiveTempWeapon(this PlayerData pData, string wId, int ammo = -1)
        {
            var weapon = Game.Items.Stuff.CreateItem(wId, 0, ammo, true) as Game.Items.Weapon;

            if (weapon == null)
                return null;

            pData.Weapons[0] = weapon;

            if (ammo < 0)
                weapon.Ammo = -1;

            pData.InventoryAction(GroupTypes.Weapons, 0, 5);

            return weapon;
        }

        /// <summary>Выслать уведомление игроку (кастомное)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="type">Тип уведомления</param>
        /// <param name="title">Заголовок</param>
        /// <param name="text">Текст</param>
        /// <param name="timeout">Время показа в мс.</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void Notify(this Player player, NotificationTypes type, string title, string text, int timeout = -1) => player.TriggerEvent("Notify::Custom", (int)type, title, text, timeout);
        public static void NotifyError(this Player player, string text, int timeout = -1) => player.TriggerEvent("Notify::CustomE", text, timeout);
        public static void NotifySuccess(this Player player, string text, int timeout = -1) => player.TriggerEvent("Notify::CustomS", text, timeout);

        /// <summary>Выслать уведомление игроку (заготовленное)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="key">Тип уведомления (см. на клиенте CEF.Notifications.Prepared</param>
        /// <param name="args">Дополнительные параметры</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void Notify(this Player player, string key, params object[] args) => player.TriggerEvent("Notify", key, args);

        public static void NotifyWithPlayer(this Player player, string key, Player player1, params object[] args) => player.TriggerEvent("Notify::P", key, player1.Id, args);
        #endregion

        #region Data Extansions
        public static PlayerData GetMainData(this Player player) => PlayerData.Get(player);

        public static void SetMainData(this Player player, PlayerData data) => PlayerData.Set(player, data);

        public static TempData GetTempData(this Player player) => TempData.Get(player);

        public static void SetTempData(this Player player, TempData data) => TempData.Set(player, data);

        public static VehicleData GetMainData(this Vehicle vehicle) => VehicleData.GetData(vehicle);

        public static void SetMainData(this Vehicle vehicle, VehicleData data) => VehicleData.SetData(vehicle, data);

        #endregion

        #region Other Stuff

        public static string SerializeToJson(this object value) => JsonConvert.SerializeObject(value);
        //public static string SerializeToJson(this object value) => JsonConvert.SerializeObject(value, new JsonSerializerProperties.Settings.Static.) { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        public static T DeserializeFromJson<T>(this string value) => JsonConvert.DeserializeObject<T>(value);

        public static async Task<string> SerializeToJsonAsync(this object value) => await Task.Run<string>(() => JsonConvert.SerializeObject(value));
        public static async Task<T> DeserializeFromJsonAsync<T>(this string value) => await Task.Run<T>(() => JsonConvert.DeserializeObject<T>(value));

        /// <summary>Вывести строку в консольное окно</summary>
        /// <remarks>Можно окрашивать текст в цвета: ~Red~SOME TEXT~/~</remarks>
        /// <param name="text">Строка</param>
        /// <param name="newLine">Перейти ли на новую строку перед выводом?</param>
        public static void ConsoleOutput(string text = "", bool newLine = true)
        {
            if (newLine)
                Console.WriteLine();

            Console.ResetColor();

            ConsoleColor color;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '~')
                {
                    int nextSpecIdx = -1;

                    StringBuilder specSb = new StringBuilder();

                    for (int j = i + 1; j < text.Length; j++)
                    {
                        if (text[j] == '~')
                        {
                            nextSpecIdx = j;

                            break;
                        }
                        else
                        {
                            specSb.Append(text[j]);
                        }
                    }

                    if (nextSpecIdx <= i)
                    {
                        specSb.Insert(0, text[i]);

                        Console.Write(specSb.ToString());

                        break;
                    }
                    else
                    {
                        var specStr = specSb.ToString();

                        i += specStr.Length + 1;

                        if (specStr == "/")
                        {
                            Console.ResetColor();
                        }
                        else
                        {
                            if (Enum.TryParse(specStr, out color))
                                Console.ForegroundColor = color;
                        }
                    }
                }
                else
                {
                    Console.Write(text[i]);
                }
            }

            Console.ResetColor();
        }

        /// <summary>Получить всех администраторов на сервере</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static IEnumerable<PlayerData> GetAdmins(int minLvl = 1) => PlayerData.All.Values.Where(x => x.AdminLevel >= minLvl);

        /// <summary>Отправить сообщение всем администраторам</summary>
        /// <param name="tag">Тэг</param>
        /// <param name="msg">Сообщение</param>
        /// <param name="minLvl">Минимальный уровень администратора</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void MsgToAdmins(string msg, int minLvl = 1)
        {
            foreach (var player in GetAdmins(minLvl))
                Game.Management.Chat.Service.SendServer(msg, player.Player);
        }

        #endregion

        public static Regex NumberplatePattern { get; } = new Regex(@"^[A-Z0-9]{1,8}$", RegexOptions.Compiled);

        /// <summary>Получить координату точки, которая находится напротив игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="coeffXY">Коэфициент отдаления от игрока</param>
        /// <returns>Координата точки, находящейся напротив игрока, null - если игрока не существует</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Vector3 GetFrontOf(this Player player, float coeffXY = 1.2f) => player.Position.GetFrontOf(player.Heading, coeffXY);

        public static Vector3 GetFrontOf(this Vector3 pos, float rotationZ = 0f, float coeffXY = 1.2f)
        {
            var radians = -rotationZ * Math.PI / 180;

            return new Vector3(pos.X + (coeffXY * Math.Sin(radians)), pos.Y + (coeffXY * Math.Cos(radians)), pos.Z);
        }

        public static float GetOppositeAngle(float angle) => (angle + 180) % 360;

        /// <summary>Метод для поворота одной точки относительно другой В указанный угол</summary>
        /// <param name="point">Точка (которую необходимо повернуть)</param>
        /// <param name="originPoint">Точка (относительно которой поворачиваем)</param>
        /// <param name="angle">Угол В который поворачиваем</param>
        /// <param name="radians">Радианы ли? Если нет, то метод сам переведет градусы в радианы</param>
        /// <returns></returns>
        public static Vector3 RotatePoint(Vector3 point, Vector3 originPoint, float angle, bool radians = false)
        {
            if (!radians)
                angle = DegreesToRadians(angle);

            float x = point.X, y = point.Y;
            float cos = (float)Math.Cos(angle), sin = (float)Math.Sin(angle);

            point.X = cos * (x - originPoint.X) - sin * (y - originPoint.Y) + originPoint.X;
            point.Y = sin * (x - originPoint.X) + cos * (y - originPoint.Y) + originPoint.Y;

            return point;
        }

        /// <summary>Метод для преобразования градусов в радианы</summary>
        /// <param name="degrees">Градусы</param>
        public static float DegreesToRadians(float degrees) => (float)(Math.PI / 180f) * degrees;

        /// <summary>Метод для преобразования радиан в градусы</summary>
        /// <param name="radians">Радианы</param>
        public static float RadiansToDegrees(float radians) => (float)(180f / Math.PI) * radians;

        public static int GetTotalYears(this DateTime dateTime) => (DateTime.MinValue + Utils.GetCurrentTime().Subtract(dateTime)).Year - 1;

        public static bool YearPassed(this DateTime dateTime)
        {
            var currentTime = Utils.GetCurrentTime();

            return currentTime.Month == dateTime.Month && currentTime.Day == dateTime.Day;
        }

        /// <inheritdoc cref="Game.Attachments.Service.AttachObject(Entity, string, AttachmentType, int)"/>
        public static bool AttachObject(this Entity entity, uint model, AttachmentType type, int detachAfter, string syncData, params object[] args) => Game.Attachments.Service.AttachObject(entity, model, type, detachAfter, syncData, args);

        /// <inheritdoc cref="Game.Attachments.Service.DetachObject(Entity, string)"/>
        public static bool DetachObject(this Entity entity, AttachmentType type, params object[] args) => Game.Attachments.Service.DetachObject(entity, type, args);

        /// <inheritdoc cref="Game.Attachments.Service.AttachEntity(Entity, int, AttachmentType)"/>
        public static bool AttachEntity(this Entity entity, Entity target, AttachmentType type, string syncData, params object[] args) => Game.Attachments.Service.AttachEntity(entity, target, type, syncData, args);

        /// <inheritdoc cref="Game.Attachments.Service.DetachEntity(Entity, int)"/>
        public static bool DetachEntity(this Entity entity, Entity target) => Game.Attachments.Service.DetachEntity(entity, target);

        /// <inheritdoc cref="Game.Attachments.Service.DetachAllEntities(Entity)"/>
        public static bool DetachAllEntities(this Entity entity) => Game.Attachments.Service.DetachAllEntities(entity);

        /// <inheritdoc cref="Game.Attachments.Service.DetachAllObjects(Entity)"/>
        public static bool DetachAllObjects(this Entity entity) => Game.Attachments.Service.DetachAllObjects(entity);

        /// <inheritdoc cref="Game.Attachments.Service.DetachAllObjectsInHand(Entity)"/>
        public static bool DetachAllObjectsInHand(this Entity entity) => Game.Attachments.Service.DetachAllObjectsInHand(entity);

        /// <inheritdoc cref="Game.Attachments.Service.GetEntityAttachmentData(Entity, Entity)"/>
        public static AttachmentEntityNet GetAttachmentData(this Entity entity, Entity target) => Game.Attachments.Service.GetEntityAttachmentData(entity, target);

        public static Entity GetEntityIsAttachedTo(this Entity entity) => Game.Attachments.Service.GetEntityIsAttachedToEntity(entity);

        /// <inheritdoc cref="Game.Animations.Service.Play(PlayerDAnimationstions.GeneralTypes)"/>
        public static void PlayAnim(this PlayerData pData, GeneralType type) => Game.Animations.Service.Play(pData, type);

        /// <inheritdoc cref="Game.Animations.Service.Play(PlaAnimationstions.FastTypes)"/>
        public static void PlayAnim(this PlayerData pData, FastType type, TimeSpan timeout) => Game.Animations.Service.Play(pData, type, timeout);

        /// <inheritdoc cref="Game.Animations.Service.Play(PlaAnimationstions.OtherTypes)"/>
        public static void PlayAnim(this PlayerData pData, OtherType type) => Game.Animations.Service.Play(pData, type);

        /// <inheritdoc cref="Game.Animations.Service.StopAll(pData)"/>
        public static void StopAllAnims(this PlayerData pData) => Game.Animations.Service.StopAll(pData);

        public static bool StopFastAnim(this PlayerData pData) => Game.Animations.Service.StopFastAnim(pData);

        public static bool StopGeneralAnim(this PlayerData pData) => Game.Animations.Service.StopGeneralAnim(pData);

        public static bool StopOtherAnim(this PlayerData pData) => Game.Animations.Service.StopOtherAnim(pData);

        /// <inheritdoc cref="Game.Animations.Service.Set(PlayerDAnimationstions.EmotionTypes, bool)"/>
        public static void SetEmotion(this PlayerData pData, EmotionType type) => Game.Animations.Service.Set(pData, type);

        /// <inheritdoc cref="Game.Animations.Service.Set(PlayerDAnimationstions.WalkstyleTypes, bool)"/>
        public static void SetWalkstyle(this PlayerData pData, WalkstyleType type) => Game.Animations.Service.Set(pData, type);

        /// <inheritdoc cref="SkyCamera.MoSkyCameral.SkyCamera.SwitchTypes, bool, string, object[])"></inheritdoc>
        public static void SkyCameraMove(this Player player, SkyCamera.SwitchType switchType, bool fade, string eventOnFinish = null, params object[] args) => SkyCamera.Move(player, switchType, fade, eventOnFinish, args);

        /// <summary>Метод, который закрывает все активные интерфейсы на стороне клиента</summary>
        /// <param name="player"></param>
        public static void CloseAll(this Player player, bool onlyInterfaces = false) => player.TriggerEvent("Player::CloseAll", onlyInterfaces);
        
        public static int CalculateDifference(int currentValue, int difference, int minValue, int maxValue)
        {
            var maxDifference = maxValue - currentValue;
            var minDifference = minValue - currentValue;

            var actualDifference = Math.Min(Math.Max(difference, minDifference), maxDifference);

            return actualDifference;
        }

        public static uint ToUInt32(this int value)
        {
            unchecked
            {
                return (uint)value;
            }
        }

        /// <summary>Передать лог (авторизация) в очередь</summary>
        /// <param name="pData">Данные игрока</param>
        public static void LogAuth(PlayerData pData)
        {
            var cid = pData.CID;
            var rid = pData.Player.Id;
            var ip = pData.Player.Address;

            var date = pData.Info.LastJoinDate;

            //todo
        }

        /// <summary>Передать лог (инвентарь) в очередь</summary>
        /// <param name="pData">Данные игрока</param>
        /// <param name="cont">Контейнер, если null - </param>
        /// <param name="item">Предмет</param>
        /// <param name="amount"></param>
        /// <param name="take">Получил игрок предмет или отдал?</param>
        public static void LogInventory(PlayerData pData, Container cont, Game.Items.Item item, int amount, bool take)
        {
            var cid = pData.CID;

            uint? contId = cont?.ID;

            var itemUid = item.UID;
            var itemId = item.ID;

            //todo
        }

        public static void LogTrade(PlayerData pData, PlayerData tData)
        {
            //todo
        }

        public static Entity GetEntityInVehicleSeat(this Vehicle veh, int seatId)
        {
            foreach (var x in veh.Occupants)
            {
                var seat = x.GetSharedData<int?>("VehicleSeat");

                if (seat == seatId)
                    return x;
            }

            return null;
        }

        public static bool TriggerEventOccupants(this Vehicle veh, string eventName, params object[] args)
        {
            var occupants = veh.Occupants.Select(x => x as Player).Where(x => x != null).ToArray();

            if (occupants.Length > 0)
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(occupants, eventName, args);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static void SetFixed(this Vehicle veh) => Sync.Vehicles.SetFixed(veh);
        public static void SetVisualFixed(this Vehicle veh) => Sync.Vehicles.SetVisualFixed(veh);

        public static void SetCleaned(this VehicleData vData) => vData.DirtLevel = 0;

        public static void SetHeading(this Vehicle veh, float value, bool resetXY = true)
        {
            var rot = veh.Rotation;

            if (resetXY)
            {
                rot.X = 0f; rot.Y = 0f;
            }

            rot.Z = value;

            veh.Rotation = rot;
        }

        public static void CreateGPSBlip(this Player player, Vector3 pos, uint dim, bool drawRoute = false) => player.TriggerEvent("Blip::CreateGPS", pos, dim, drawRoute);

        public static void FillFileToReplaceRegion(string fPath, string regionId, List<string> linesToInsert)
        {
            var lines = new List<string>();

            var insIdx = 0;

            regionId = $"#region {regionId}";

            using (var sr = new StreamReader(fPath))
            {
                bool ignore = false;

                string line;

                var i = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!ignore)
                    {
                        if (line.Contains(regionId))
                        {
                            ignore = true;

                            insIdx = i;
                        }

                        lines.Add(line);
                    }
                    else
                    {
                        if (line.Contains("#endregion"))
                        {
                            ignore = false;

                            lines.Add(line);
                        }
                    }

                    i++;
                }
            }

            foreach (var x in linesToInsert)
                lines.Insert(++insIdx, x);

            using (var sw = new StreamWriter(fPath))
            {
                foreach (var x in lines)
                    sw.WriteLine(x.Replace(System.Environment.NewLine, @"\r\n"));
            }
        }

        public static string ToCSharpStr(this Vector3 v) => v == null ? "null" : $"new RAGE.Vector3({v.X}f, {v.Y}f, {v.Z}f)";
        public static string ToCSharpStr(this Vector4 v) => v == null ? "null" : $"new {typeof(BlaineRP.Client.Utils.Vector4).FullName}({v.X}f, {v.Y}f, {v.Z}f, {v.RotationZ}f)";

        public static void InventoryUpdate(this Player player, GroupTypes group, int slot, string updStr) => player.TriggerEvent("Inventory::Update", (int)group, slot, updStr);

        public static void InventoryUpdate(this Player player, GroupTypes group1, int slot1, string updStr1, GroupTypes group2, int slot2, string updStr2) => player.TriggerEvent("Inventory::Update", (int)group1, slot1, updStr1, (int)group2, slot2, updStr2);

        public static void InventoryUpdate(GroupTypes group1, int slot1, string updStr1, GroupTypes group2, int slot2, string updStr2, Player[] players) => NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Inventory::Update", (int)group1, slot1, updStr1, (int)group2, slot2, updStr2);

        public static void InventoryUpdate(GroupTypes group, int slot, string updStr, Player[] players) => NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Inventory::Update", (int)group, slot, updStr);

        public static void InventoryUpdate(this Player player, GroupTypes group, string updStr) => player.TriggerEvent("Inventory::Update", (int)group, 0, updStr);

        public static void InventoryUpdate(GroupTypes group, string updStr, Player[] players) => NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Inventory::Update", (int)group, 0, updStr);

        public static void WarpToVehicleSeat(this Player player, Vehicle veh, int seatId, int timeout = 5000) => player.TriggerEvent("Vehicles::WTS", veh.Id, seatId, timeout);

        public static bool IsAnyAnimOn(this PlayerData pData) => pData.GeneralAnim != GeneralType.None || pData.FastAnim != FastType.None || pData.OtherAnim != OtherType.None;

        public static bool CanPlayAnimNow(this PlayerData pData) => !IsAnyAnimOn(pData) && !pData.CrawlOn;

        public static void CloneDirectory(DirectoryInfo source, DirectoryInfo dest)
        {
            foreach (var source_subdir in source.EnumerateDirectories())
            {
                var target_subdir = new DirectoryInfo(Path.Combine(dest.FullName, source_subdir.Name));
                target_subdir.Create();
                CloneDirectory(source_subdir, target_subdir);
            }

            foreach (var source_file in source.EnumerateFiles())
            {
                var target_file = new FileInfo(Path.Combine(dest.FullName, source_file.Name));
                source_file.CopyTo(target_file.FullName, true);
            }
        }

        public static IEnumerable<uint> GetUInt32Range(uint start, uint stop)
        {
            for (uint i = start; i < stop; i++)
                yield return i;
        }

        public static void SendGPSTracker(this Player player, int type, float x, float y, Entity entity) => player.TriggerEvent("Blip::Tracker", type, x, y, entity);

        public static Vector3 GetLockerPosition(this Game.Fractions.IUniformable unif, byte idx) => idx >= unif.LockerRoomPositions.Length ? null : unif.LockerRoomPositions[idx];
    }
}
