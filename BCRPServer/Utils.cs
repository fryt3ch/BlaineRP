﻿using GTANetworkAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Tls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace BCRPServer
{
    static class Utils
    {
        public static class Dimensions
        {
            public const uint Main = 0;

            public const uint Stuff = 1;

            public const uint Demorgan = 2;
        }

        public enum RespawnTypes
        {
            /// <summary>Смерть</summary>
            Death = 0,
            /// <summary>Телепортация</summary>
            Teleport,
        }

        /// <summary>Номер первого CID</summary>
        /// <remarks>Используется, чтобы отличать CID от Remote ID<br/>Пусть 3000 - макс. кол-во игроков на сервере, тогда 2999 - последний Remote ID</remarks>
        public static int FirstCID = 3000 * 1;

        /// <summary>Номер первого VID</summary>
        /// <remarks>Используется, чтобы отличать CID от Remote ID<br/>Пусть 3000 - макс. кол-во игроков на сервере, а машин у каждого - 100, тогда 299999 - посдений RemoteID</remarks>
        public static int FirstVID = 3000 * 100;

        public const int PlayerPrivateDimBase = 1000;
        public const int HouseDimBase = 10000;
        public const int ApartmentsDimBase = 20000;
        public const int ApartmentsRootDimBase = 30000;

        /// <summary>Стандартная позиция спавна</summary>
        public static Vector3 DefaultSpawnPosition = new Vector3(-749.78f, 5818.21f, 17f);
        /// <summary>Стандартный поворот</summary>
        public static float DefaultSpawnHeading = 0f;

        public static uint GetPrivateDimension(Player player) => (uint)(player.Id + PlayerPrivateDimBase);

        /// <summary>Нулевой вектор (X=0, Y=0, Z=0)</summary>
        public static Vector3 ZeroVector = new Vector3(0, 0, 0);

        public static Color WhiteColor = new Color(255, 255, 255);
        public static Color BlackColor = new Color(0, 0, 0);
        public static Color RedColor = new Color(255, 0, 0);
        public static Color BlueColor = new Color(0, 0, 255);
        public static Color GreenColor = new Color(0, 255, 0);
        public static Color YellowColor = new Color(255, 255, 0);

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

        public class Vector2
        {
            public float X { get; set; }
            public float Y { get; set; }

            public Vector2(float X = 0f, float Y = 0f)
            {
                this.X = X;
                this.Y = Y;
            }

            public float Distance(Vector2 pos1, Vector2 pos2) => (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));
        }

        /// <summary>Получить текущее время (по МСК.)</summary>
        public static DateTime GetCurrentTime() => DateTime.UtcNow.AddHours(3);


        /// <summary>Тихий кик игрока (со стороны сервера)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="reason">Причина кика</param>
        /// <param name="delay">Задержка перед киком</param>
        public static void KickSilent(Player player, string reason, int delay = 2000)
        {
            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                player.Notify("Kick", reason);

                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    player.KickSilent(reason);
                }, delay);
            });
        }

        /// <summary>Кик игрока со сторорны администратора</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="reason">Причина кика</param>
        /// <param name="delay">Задержка перед киком</param>
        public static void Kick(Player player, string adminStr, string reason, int delay = 2000)
        {
            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                player.Notify("KickBy", adminStr, reason);

                NAPI.Task.Run(() =>
                {
                    if (player?.Exists != true)
                        return;

                    player.KickSilent(reason);
                }, delay);
            });
        }

        public static List<T> ToList<T>(this Newtonsoft.Json.Linq.JArray jArray) => jArray.ToObject<List<T>>();
        public static Dictionary<T1, T2> ToDictionary<T1, T2>(this Newtonsoft.Json.Linq.JArray jArray) => jArray.ToObject<Dictionary<T1, T2>>();

        #region Vehicle Stuff

        /// <summary>Получить сущность транспорта по VID</summary>
        /// <param name="vid">Vehicle ID</param>
        /// <returns>Объект класса Vehicle, если транспорт найден на сервере, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Vehicle GetVehicleByVID(int vid) => NAPI.Pools.GetAllVehicles().Where(x => x.GetMainData()?.VID == vid).FirstOrDefault();
        /// <summary>Получить сущность транспорта по ID</summary>
        /// <param name="id">Remote ID</param>
        /// <returns>Объект класса Vehicle, если транспорт найден на сервере, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Vehicle GetVehicleByID(int id) => NAPI.Pools.GetAllVehicles().Where(x => x?.Id == id).FirstOrDefault();

        /// <summary>Метод для получения сущности транспорта, который существует</summary>
        /// <param name="vid">VID или RemoteID</param>
        /// <returns>Объект класса VehicleData, если транспорт найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static VehicleData FindVehicleOnline(int vid) => vid >= FirstVID ? VehicleData.All.Where(x => x.Value?.VID == vid).Select(x => x.Value).FirstOrDefault() : VehicleData.All.Where(x => x.Key?.Id == vid).Select(x => x.Value).FirstOrDefault();

        /// <summary>Является ли транспорт автомобилем?</summary>
        /// <param name="vehicle">Сущность транспорта</param>
        /// <returns>true - если транспорт является автомобилем, false - в противном случае</returns>
        public static bool IsCar(this Vehicle vehicle)
        {
            if (vehicle == null)
                return false;

            int type = vehicle.Class;

            if (type == 8 || type == 13 || type == 14 || type == 15 || type == 16)
                return false;

            return true;
        }

        /// <summary>Является ли транспорт мотоциклом?</summary>
        /// <param name="vehicle">Сущность транспорта</param>
        /// <returns>true - если транспорт является мотоциклом, false - в противном случае</returns>
        public static bool IsBike(this Vehicle vehicle) => vehicle?.Class == 8;

        /// <summary>Является ли транспорт лодкой?</summary>
        /// <param name="vehicle">Сущность транспорта</param>
        /// <returns>true - если транспорт является лодкой, false - в противном случае</returns>
        public static bool IsBoat(this Vehicle vehicle) => vehicle?.Class == 14;
        /// <summary>Является ли транспорт вертолетом?</summary>
        /// <param name="vehicle">Сущность транспорта</param>
        /// <returns>true - если транспорт является вертолетом, false - в противном случае</returns>
        public static bool IsHelicopter(this Vehicle vehicle) => vehicle?.Class == 15;
        /// <summary>Является ли транспорт самолетом?</summary>
        /// <param name="vehicle">Сущность транспорта</param>
        /// <returns>true - если транспорт является самолетом, false - в противном случае</returns>
        public static bool IsPlane(this Vehicle vehicle) => vehicle?.Class == 16;

        #endregion

        #region Main Thread Help
        public static async Task<T> RunAsync<T>(this GTANetworkMethods.Task task, Func<T> func, long delay = 0)
        {
            if (delay <= 0 && IsMainThread())
            {
                return func.Invoke();
            }

            var taskCompletionSource = new TaskCompletionSource<T>();

            task.Run(() => taskCompletionSource.SetResult(func.Invoke()), delay);

            return await taskCompletionSource.Task;
        }

        public static async Task RunAsync(this GTANetworkMethods.Task task, Action action, long delay = 0)
        {
            if (delay <= 0 && IsMainThread())
            {
                action.Invoke();

                return;
            }

            var taskCompletionSource = new TaskCompletionSource<object>();

            task.Run(() => { action.Invoke(); taskCompletionSource.SetResult(null); }, delay);

            await taskCompletionSource.Task;
        }

        public static void RunSafe(this GTANetworkMethods.Task task, Action action, long delay = 0)
        {
            if (delay <= 0 && IsMainThread())
            {
                action.Invoke();

                return;
            }

            task.Run(action, delay);
        }

        public static void TriggerEventToStreamed(this Entity entity, string eventName, params object[] args)
        {
            var players = PlayerData.All.Keys.Where(x => x != null && AreEntitiesNearby(x, entity, Settings.STREAM_DISTANCE));

            foreach (var player in players)
                player.TriggerEvent(eventName, args);
        }

        public static void TriggerEventInDistance(this Entity entity, float distance, string eventName, params object[] args)
        {
            var players = PlayerData.All.Keys.Where(x => x != null && AreEntitiesNearby(x, entity, distance));

            foreach (var player in players)
                player.TriggerEvent(eventName, args);
        }

        public static void TriggerEventToStreamed(this Vector3 pos, uint dimension, string eventName, params object[] args)
        {
            var players = PlayerData.All.Keys.Where(x => x != null && x.Dimension == dimension && Vector3.Distance(pos, x.Position) <= Settings.STREAM_DISTANCE);

            foreach (var player in players)
                player.TriggerEvent(eventName, args);
        }

        public static bool AreEntitiesNearby(this Entity entity, Entity target, float radius) => (entity.Dimension == target.Dimension && Vector3.Distance(entity.Position, target.Position) <= radius);
        public static bool AreEntitiesNearbyDiffDims(this Entity entity, Entity target, float radius) => (Vector3.Distance(entity.Position, target.Position) <= radius);

        public static Entity GetEntityById(EntityType eType, int id)
        {
            if (eType == EntityType.Player)
                return GetPlayerByID(id);

            if (eType == EntityType.Vehicle)
                return GetVehicleByID(id);

            return null;
        }
        #endregion

        #region Client Utils

        /// <inheritdoc cref="Additional.AntiCheat.SetPlayerPos(Player, Vector3, bool, uint?)"/>
        public static void Teleport(this Player player, Vector3 position, bool toGround, uint? dimension = null) => Additional.AntiCheat.SetPlayerPos(player, position, toGround, dimension);

        /// <inheritdoc cref="Additional.AntiCheat.SetPlayerInvincible(Player, bool)"/>
        public static void SetInvincible(this Player player, bool state) => Additional.AntiCheat.SetPlayerInvincible(player, state);

        /// <inheritdoc cref="Additional.AntiCheat.SetPlayerHealth(Player, int)"/>
        public static void SetHealth(this Player player, int value) => Additional.AntiCheat.SetPlayerHealth(player, value);

        /// <inheritdoc cref="Additional.AntiCheat.SetPlayerArmour(Player, int)"/>
        public static void SetArmour(this Player player, int value) => Additional.AntiCheat.SetPlayerArmour(player, value);

        /// <inheritdoc cref="Additional.AntiCheat.SetPlayerAlpha(Player, int)"/>
        public static void SetAlpha(this Player player, int value) => Additional.AntiCheat.SetPlayerAlpha(player, value);

        /// <inheritdoc cref="Additional.AntiCheat.SetPlayerWeapon(Player, WeaponHash, int)"/>
        public static void SetWeapon(this Player player, uint hash, int ammo = 0) => Additional.AntiCheat.SetPlayerWeapon(player, hash, ammo);

        /// <inheritdoc cref="Additional.AntiCheat.SetPlayerAmmo(Player, int)"/>
        public static void SetAmmo(this Player player, int ammo = 0) => Additional.AntiCheat.SetPlayerAmmo(player, ammo);

        /// <summary>Метод для установки причёски игрока</summary>
        /// <remarks>Если ID причёски не существует, то будет установлена стандартная причёска!</remarks>
        /// <param name="player">Сущность игрока</param>
        /// <param name="id">ID причёски (см. Game.Data.Customization.AllHairs</param>
        public static void SetHair(this Player player, int id) => player?.SetClothes(2, Game.Data.Customization.GetHair(player.Model == (uint)PedHash.FreemodeMale01, id), 0);

        /// <summary>Получить сущность игрока по ID</summary>
        /// <param name="id">Remote ID</param>
        /// <returns>Объект класса Player, если игрок найден на сервере, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Player GetPlayerByID(int id) => NAPI.Pools.GetAllPlayers().Where(x => x?.Id == id).FirstOrDefault();

        /// <summary>Получить сущность игрока по CID</summary>
        /// <param name="cid">Character ID</param>
        /// <returns>Объект класса Player, если игрок найден на сервере, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Player GetPlayerByCID(int cid) => NAPI.Pools.GetAllPlayers().Where(x => x.GetMainData()?.CID == cid).FirstOrDefault();

        //public static Player GetPlayerByName(string name) => NAPI.Player.GetPlayerFromName(name);

        /// <summary>Метод для получения сущности игрока, который в сети</summary>
        /// <param name="pid">CID или RemoteID</param>
        /// <returns>Объект класса PlayerData, если игрок найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static PlayerData FindReadyPlayerOnline(int pid) => pid >= FirstCID ? PlayerData.All.Where(x => x.Value?.CID == pid).Select(x => x.Value).FirstOrDefault() : PlayerData.All.Where(x => x.Key?.Id == pid).Select(x => x.Value).FirstOrDefault();

        /// <summary>Метод для получения сущности игрока, который в сети</summary>
        /// <param name="pid">CID или RemoteID</param>
        /// <returns>Объект класса Player, если игрок найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Player FindPlayerOnline(int pid) => pid >= FirstCID ? PlayerData.All.Where(x => x.Value?.CID == pid).Select(x => x.Key).FirstOrDefault() : NAPI.Pools.GetAllPlayers().Where(x => x?.Id == pid).FirstOrDefault();

        /// <summary>Метод для получения пола игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <returns>true - мужчина, false - женщина</returns>
        public static bool GetSex(this Player player) => player?.Model == (uint)PedHash.FreemodeMale01;

        /// <summary>Метод для получения прототипа сущности игрока, который, возможно, не в сети</summary>
        /// <param name="cid">CID</param>
        /// <returns>Объект класса Player, если игрок найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static PlayerData.PlayerInfo FindPlayerOffline(int cid) => PlayerData.PlayerInfo.Get(cid);

        /// <inheritdoc cref="Additional.AntiSpam.CheckNormal(Player, int)"/>
        public static (bool IsSpammer, PlayerData Data) CheckSpamAttack(this Player player, int decreaseDelay = 250) => Additional.AntiSpam.CheckNormal(player, decreaseDelay);
        /// <inheritdoc cref="Additional.AntiSpam.CheckTemp(Player, int)"/>
        public static (bool IsSpammer, TempData Data) CheckSpamAttackTemp(this Player player, int decreaseDelay = 250) => Additional.AntiSpam.CheckTemp(player, decreaseDelay);

        /// <inheritdoc cref="CEF.Inventory.Replace(PlayerData, CEF.Inventory.Groups, int, CEF.Inventory.Groups, int, int)"/>
        public static CEF.Inventory.Results InventoryReplace(this PlayerData pData, CEF.Inventory.Groups to, int slotTo, CEF.Inventory.Groups from, int slotFrom, int amount = -1) => CEF.Inventory.Replace(pData, to, slotTo, from, slotFrom, amount);

        /// <inheritdoc cref="CEF.Inventory.Action(PlayerData, CEF.Inventory.Groups, int, int, object[])"/>
        public static CEF.Inventory.Results InventoryAction(this PlayerData pData, CEF.Inventory.Groups slotStr, int slot, int action = 5, params object[] args) => CEF.Inventory.Action(pData, slotStr, slot, action, args);

        /// <inheritdoc cref="CEF.Inventory.Drop(PlayerData, CEF.Inventory.Groups, int, int)"/>
        public static void InventoryDrop(this PlayerData pData, CEF.Inventory.Groups slotStr, int slot, int amount) => CEF.Inventory.Drop(pData, slotStr, slot, amount);

        /// <summary>Метод для удаления всего оружия у игрока</summary>
        /// <param name="pData">PlayerData игрока</param>
        /// <param name="fromInventoryToo">Удалить ли всё оружие из инвентаря тоже?</param>
        /// <param name="fromBagToo">Удалить ли всё оружие из надетой сумки тоже?</param>
        /// <returns></returns>
        public static void RemoveAllWeapons(this PlayerData pData, bool fromInventoryToo = false, bool fromBagToo = false)
        {
            pData.UnequipActiveWeapon();

            List<(CEF.Inventory.Groups Group, int Slot)> updList = new List<(CEF.Inventory.Groups Group, int Slot)>();

            for (int i = 0; i < pData.Weapons.Length; i++)
            {
                if (pData.Weapons[i] != null)
                {
                    pData.Weapons[i].Delete();

                    updList.Add((CEF.Inventory.Groups.Weapons, i));
                }
            }

            if (pData.Holster?.Items[0] is Game.Items.Weapon)
            {
                pData.Holster.Items[0].Delete();

                updList.Add((CEF.Inventory.Groups.Holster, 2));
            }

            if (fromInventoryToo)
            {
                for (int i = 0; i < pData.Items.Length; i++)
                {
                    if (pData.Items[i] is Game.Items.Weapon)
                    {
                        pData.Items[i].Delete();

                        updList.Add((CEF.Inventory.Groups.Items, i));
                    }
                }
            }

            if (fromBagToo && pData.Bag != null)
            {
                for (int i = 0; i < pData.Bag.Items.Length; i++)
                {
                    if (pData.Bag.Items[i] is Game.Items.Weapon)
                    {
                        pData.Bag.Items[i].Delete();

                        updList.Add((CEF.Inventory.Groups.Bag, i));
                    }
                }
            }

            foreach (var x in updList)
                pData.Player.TriggerEvent("Inventory::Update", (int)x.Group, x.Slot, "null");
        }


        /// <summary>Метод для того, чтобы игрок перестал использовать текущее оружие</summary>
        /// <param name="pData">PlayerData игрока</param>
        /// <returns></returns>
        public static bool UnequipActiveWeapon(this PlayerData pData)
        {
            var weapon = pData.ActiveWeapon;

            if (weapon != null)
            {
                pData.InventoryAction(weapon.Value.Group, weapon.Value.Slot, 5);

                return true;
            }
            else
                return false;
        }

        /// <summary>Выслать уведомление игроку (кастомное)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="type">Тип уведомления</param>
        /// <param name="title">Заголовок</param>
        /// <param name="text">Текст</param>
        /// <param name="timeout">Время показа в мс.</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void Notify(this Player player, NotificationTypes type, string title, string text, int timeout = 2500)
        {
            if (player?.Exists != true)
                return;

            player.TriggerEvent("Notify::Custom", (int)type, title, text, timeout);
        }

        /// <summary>Выслать уведомление игроку (заготовленное)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="key">Тип уведомления (см. на клиенте CEF.Notifications.Prepared</param>
        /// <param name="args">Дополнительные параметры</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void Notify(this Player player, string key, params object[] args)
        {
            if (player?.Exists != true)
                return;

            player.TriggerEvent("Notify", key, args);
        }
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

            var splitted = text.Split('~');

            ConsoleColor color;

            foreach (var x in splitted)
            {
                if (x.StartsWith('/'))
                    Console.ResetColor();
                else if (Enum.TryParse(x.Substring(0), out color))
                    Console.ForegroundColor = color;
                else
                    Console.Write(x);
            }
        }

        /// <summary>Получить всех существующих игроков</summary>
        /// <remarks>Существующий игрок - такой, у которого уже есть PlayerData</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static IEnumerable<Player> GetExistingPlayers() => NAPI.Pools.GetAllPlayers().Where(x => x.GetMainData() != null);
        /// <summary>Получить всех администраторов на сервере</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static IEnumerable<Player> GetAdmins(int minLvl = 1) => NAPI.Pools.GetAllPlayers().Where(x => (x.GetMainData()?.AdminLevel ?? -1) >= minLvl);

        /// <summary>Отправить сообщение всем администраторам</summary>
        /// <param name="tag">Тэг</param>
        /// <param name="msg">Сообщение</param>
        /// <param name="minLvl">Минимальный уровень администратора</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void MsgToAdmins(string msg, int minLvl = 1)
        {
            foreach (var player in GetAdmins(minLvl))
                Sync.Chat.SendServer(msg, player);
        }

        #endregion

        #region Encryption Tools

        /// <summary>Метод для получения зашифрованной строки из исходной</summary>
        /// <param name="plainText">Строка, которую необходимо зашифровать</param>
        /// <param name="key">Ключ дешифрования</param>
        /// <returns>Зашифрованная строка в формате Base64</returns>
        public static string EncryptString(string plainText, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        /// <summary>Метод для получения исходной строки из зашифрованной</summary>
        /// <param name="cipherText">Строка, которую необходимо расшифровать</param>
        /// <param name="key">Ключ дешифрования</param>
        /// <returns>Расшифрованная строка</returns>
        public static string DecryptString(string cipherText, string key)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        /// <summary>Метод для получения строки, зашифрованной алгоритмом MD5</summary>
        /// <param name="input">Строка, которую необходимо зашифровать</param>
        /// <returns>Зашифрованная строка в нижнем регистре</returns>
        public static string ToMD5(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("X2"));

                return sb.ToString().ToLower();
            }
        }

        #endregion

        private static Regex MailPattern = new Regex(@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$", RegexOptions.Compiled);
        private static Regex LoginPattern = new Regex(@"^(?=.*[a-zA-Z0-9])[0-9a-zA-Z!@#$%^&*]{6,12}$", RegexOptions.Compiled);
        private static Regex NamePattern = new Regex(@"^[A-Z]{1}[a-zA-Z]{1,9}$", RegexOptions.Compiled);
        private static Regex PasswordPattern = new Regex(@"^(?=.*[a-zA-Z0-9])[0-9a-zA-Z!@#$%^&*]{6,64}$", RegexOptions.Compiled);

        /// <summary>Является ли имя верным (см. Utils.NamePattern)</summary>
        /// <param name="str">Имя</param>
        public static bool IsNameValid(string str) => NamePattern.IsMatch(str);
        /// <summary>Является ли почта верной (см. Utils.MailPattern)</summary>
        /// <param name="str">Почта</param>
        public static bool IsMailValid(string str) => MailPattern.IsMatch(str);
        /// <summary>Является ли логин верным (см. Utils.LoginPattern)</summary>
        /// <param name="str">Логин</param>
        public static bool IsLoginValid(string str) => LoginPattern.IsMatch(str);
        /// <summary>Является ли пароль верным (см. Utils.PasswordPattern)</summary>
        /// <param name="str">Пароль</param>
        public static bool IsPasswordValid(string str) => PasswordPattern.IsMatch(str);

        /// <summary>Получить координату точки, которая находится напротив игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="coeffXY">Коэфициент отдаления от игрока</param>
        /// <returns>Координата точки, находящейся напротив игрока, null - если игрока не существует</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Vector3 GetFrontOf(this Player player, float coeffXY = 1.2f)
        {
            if (player?.Exists != true)
                return null;

            var pos = player.Position;
            var radians = -player.Heading * Math.PI / 180;

            return new Vector3(pos.X + (coeffXY * Math.Sin(radians)), pos.Y + (coeffXY * Math.Cos(radians)), pos.Z);
        }

        public static float GetOppositeAngle(float angle) => (angle + 180) % 360;

        /// <summary>Получить сущность ближайшего игрока к сущности</summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Объект класса Player, если игрок был найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Player GetClosestPlayer(this Entity entity)
        {
            if (entity?.Exists != true)
                return null;

            var dim = entity.Dimension;

            var players = PlayerData.All.Keys.Where(x => x != null && x.Dimension == dim);
            var pos = entity.Position;

            var minDist = Settings.STREAM_DISTANCE;
            Player minPlayer = null;

            foreach (var x in players)
            {
                var dist = Vector3.Distance(pos, x.Position);

                if (dist < minDist)
                {
                    minDist = dist;
                    minPlayer = x;
                }
            }

            return minPlayer;

        }

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

        /// <summary>Найти расстояние между двумя точками в 3D пространстве</summary>
        /// <remarks>Игнорирует ось Z</remarks>
        /// <param name="pos1">Точка 1</param>
        /// <param name="pos2">Точка 2</param>
        public static float DistanceIgnoreZ(this Vector3 pos1, Vector3 pos2) => (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));

        public static int GetTotalYears(this DateTime dateTime) => (DateTime.MinValue + Utils.GetCurrentTime().Subtract(dateTime)).Year - 1;
        public static bool YearPassed(this DateTime dateTime)
        {
            var currentTime = Utils.GetCurrentTime();

            return currentTime.Month == dateTime.Month && currentTime.Day == dateTime.Day;
        }

        public static bool MonthPassed(this DateTime dateTime) => Utils.GetCurrentTime().Day == dateTime.Day;

        public static float DistanceXY(this Vector3 pos1, Vector3 pos2) => (float)Math.Sqrt((float)Math.Pow(pos1.X - pos2.X, 2) + (float)Math.Pow(pos1.Y - pos2.Y, 2));

        /// <inheritdoc cref="Sync.AttachSystem.AttachObject(Entity, string, Sync.AttachSystem.Types, int)"/>
        public static int AttachObject(this Entity entity, uint model, Sync.AttachSystem.Types type, int detachAfter = -1, params object[] args) => Sync.AttachSystem.AttachObject(entity, model, type, detachAfter, args);
        
        /// <inheritdoc cref="Sync.AttachSystem.DetachObject(Entity, string)"/>
        public static void DetachObject(this Entity entity, int id, params object[] args) => Sync.AttachSystem.DetachObject(entity, id, args);
        
        /// <inheritdoc cref="Sync.AttachSystem.AttachEntity(Entity, int, Sync.AttachSystem.Types)"/>
        public static void AttachEntity(this Entity entity, Entity target, Sync.AttachSystem.Types type) => Sync.AttachSystem.AttachEntity(entity, target, type);
        
        /// <inheritdoc cref="Sync.AttachSystem.DetachEntity(Entity, int)"/>
        public static void DetachEntity(this Entity entity, Entity target) => Sync.AttachSystem.DetachEntity(entity, target);
        
        /// <inheritdoc cref="Sync.AttachSystem.DetachAllEntities(Entity)"/>
        public static bool DetachAllEntities(this Entity entity) => Sync.AttachSystem.DetachAllEntities(entity);
        
        /// <inheritdoc cref="Sync.AttachSystem.DetachAllObjects(Entity)"/>
        public static bool DetachAllObjects(this Entity entity) => Sync.AttachSystem.DetachAllObjects(entity);
        
        /// <inheritdoc cref="Sync.AttachSystem.GetEntityAttachmentData(Entity, Entity)"/>
        public static Sync.AttachSystem.AttachmentEntityNet GetAttachmentData(this Entity entity, Entity target) => Sync.AttachSystem.GetEntityAttachmentData(entity, target);

        /// <inheritdoc cref="Sync.Animations.Play(PlayerData, Sync.Animations.GeneralTypes)"/>
        public static void PlayAnim(this PlayerData pData, Sync.Animations.GeneralTypes type) => Sync.Animations.Play(pData, type);

        /// <inheritdoc cref="Sync.Animations.Play(Player, Sync.Animations.FastTypes)"/>
        public static void PlayAnim(this PlayerData pData, Sync.Animations.FastTypes type) => Sync.Animations.Play(pData, type);

        /// <inheritdoc cref="Sync.Animations.Play(Player, Sync.Animations.OtherTypes)"/>
        public static void PlayAnim(this PlayerData pData, Sync.Animations.OtherTypes type) => Sync.Animations.Play(pData, type);

        /// <inheritdoc cref="Sync.Animations.Stop(Player)"/>
        public static void StopAnim(this PlayerData pData) => Sync.Animations.Stop(pData);

        /// <inheritdoc cref="Sync.Animations.Set(PlayerData, Sync.Animations.EmotionTypes, bool)"/>
        public static void SetCustomEmotion(this PlayerData pData, Sync.Animations.EmotionTypes type) => Sync.Animations.Set(pData, type, true);
        /// <inheritdoc cref="Sync.Animations.Set(PlayerData, Sync.Animations.EmotionTypes, bool)"/>
        public static void SetEmotion(this PlayerData pData, Sync.Animations.EmotionTypes type) => Sync.Animations.Set(pData, type, false);

        /// <inheritdoc cref="Sync.Animations.Set(PlayerData, Sync.Animations.WalkstyleTypes, bool)"/>
        public static void SetCustomWalkstyle(this PlayerData pData, Sync.Animations.WalkstyleTypes type) => Sync.Animations.Set(pData, type, true);
        /// <inheritdoc cref="Sync.Animations.Set(PlayerData, Sync.Animations.WalkstyleTypes, bool)"/>
        public static void SetWalkstyle(this PlayerData pData, Sync.Animations.WalkstyleTypes type) => Sync.Animations.Set(pData, type, false);

        public static bool CanPlayAnim(this PlayerData pData) => pData.CrawlOn || pData.PhoneOn || pData.IsAttachedTo != null || pData.FastAnim != Sync.Animations.FastTypes.None || pData.GeneralAnim != Sync.Animations.GeneralTypes.None || pData.OtherAnim != Sync.Animations.OtherTypes.None;

        public static void Respawn(this PlayerData pData, Vector3 position, float heading, RespawnTypes rType = RespawnTypes.Teleport)
        {
            if (pData != null)
            {
                var player = pData.Player;

                var offer = pData.ActiveOffer;

                if (offer != null)
                {
                    offer.Cancel(false, false, Sync.Offers.ReplyTypes.AutoCancel, false);
                }

                pData.IsAttachedTo?.Entity?.DetachEntity(player);

                player.DetachAllEntities();

                foreach (var x in pData.ObjectsInHand)
                    player.DetachObject(x.Id);

                pData.StopAnim();

                var arm = pData.Armour;

                if (arm != null)
                {
                    pData.Armour = null;

                    arm.Unwear(pData);

                    NAPI.Player.SpawnPlayer(player, position, heading);

                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        pData.Armour = arm;

                        arm.Wear(pData);
                    }, 500);
                }
                else
                    NAPI.Player.SpawnPlayer(player, position, heading);
            }
        }

        /// <inheritdoc cref="Additional.SkyCamera.Move(Player, Additional.SkyCamera.SwitchTypes, bool, string, object[])"></inheritdoc>
        public static void SkyCameraMove(this Player player, Additional.SkyCamera.SwitchTypes switchType, bool fade, string eventOnFinish = null, params object[] args) => Additional.SkyCamera.Move(player, switchType, fade, eventOnFinish, args);

        /// <summary>Метод, который закрывает все активные интерфейсы на стороне клиента</summary>
        /// <param name="player"></param>
        public static void CloseAll(this Player player, bool onlyInterfaces = false) => player.TriggerEvent("Player::CloseAll", onlyInterfaces);

        public static bool IsMainThread() => Thread.CurrentThread.ManagedThreadId == NAPI.MainThreadId;

        public static void UpdateOnEnter(this AccountData aData) => MySQL.AccountUpdateOnEnter(aData);

        public static T GetRandom<T>(this List<T> list) => list.Count == 0 ? default(T) : list[(new Random()).Next(0, list.Count - 1)];

        public static int GetCorrectDiff(int currentValue, int diff, int minValue = 0, int maxValue = 100)
        {
            if (currentValue + diff > maxValue)
                diff = maxValue - currentValue;
            else if (currentValue + diff < minValue)
                diff = -currentValue;

            return diff;
        }

        public static uint ToUInt32(this int value)
        {
            unchecked
            {
                return (uint)value;
            }
        }

        public static uint? ToUInt32(this int? value)
        {
            if (value == null)
                return null;

            unchecked
            {
                return (uint)value.Value;
            }
        }
    }
}
