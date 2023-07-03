using BCRPServer.Sync;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer
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

                pData.Player.Teleport(pos, false, Settings.DEMORGAN_DIMENSION, null, false);
            }

            public static void SetFromDemorgan(PlayerData pData)
            {
                pData.Player.Teleport(Utils.DefaultSpawnPosition, false, Settings.MAIN_DIMENSION, Utils.DefaultSpawnHeading, false);
            }
        }

        public static uint GetPlayerIdByDimension(uint dim) => dim < Settings.PLAYER_PRIVATE_DIMENSION_BASE ? 0 : dim - Settings.PLAYER_PRIVATE_DIMENSION_BASE;

        public static uint GetHouseIdByDimension(uint dim) => dim < Settings.HOUSE_DIMENSION_BASE ? 0 : dim - Settings.HOUSE_DIMENSION_BASE;

        public static uint GetGarageIdByDimension(uint dim) => dim < Settings.GARAGE_DIMENSION_BASE ? 0 : dim - Settings.GARAGE_DIMENSION_BASE;

        public static uint GetApartmentsIdByDimension(uint dim) => dim < Settings.APARTMENTS_DIMENSION_BASE ? 0 : dim - Settings.APARTMENTS_DIMENSION_BASE;

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

        public static Game.Estates.Apartments.ApartmentsRoot GetApartmentsRootByDimension(uint dim)
        {
            var rootId = GetApartmentsRootIdByDimension(dim);

            if (rootId <= 0)
                return null;

            return Game.Estates.Apartments.ApartmentsRoot.Get(rootId);
        }

        public static Game.Estates.Garage GetGarageByDimension(uint dim)
        {
            var gId = GetGarageIdByDimension(dim);

            if (gId == 0)
                return null;

            return Game.Estates.Garage.Get(gId);
        }

        public static uint GetApartmentsRootIdByDimension(uint dim) => dim < Settings.APARTMENTS_ROOT_DIMENSION_BASE ? 0 : dim - Settings.APARTMENTS_ROOT_DIMENSION_BASE;

        /// <summary>Стандартная позиция спавна</summary>
        public static Vector3 DefaultSpawnPosition = new Vector3(-749.78f, 5818.21f, 17f);
        /// <summary>Стандартный поворот</summary>
        public static float DefaultSpawnHeading = 0f;

        public static uint GetPrivateDimension(Player player) => player.Id + Settings.PLAYER_PRIVATE_DIMENSION_BASE;

        /// <summary>Нулевой вектор (X=0, Y=0, Z=0)</summary>
        public static Vector3 ZeroVector => new Vector3(0, 0, 0);

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

        public class Colour
        {
            [JsonIgnore]
            public static Colour DefBlack => new Colour(0, 0, 0, 255);

            [JsonIgnore]
            public static Colour DefWhite => new Colour(255, 255, 255, 255);

            [JsonIgnore]
            public static Colour DefRed => new Colour(255, 0, 0, 255);

            [JsonIgnore]
            public static Colour DefGreen => new Colour(0, 255, 0, 255);

            [JsonIgnore]
            public static Colour DefBlue => new Colour(0, 0, 255, 255);

            [JsonIgnore]
            /// <summary>Красный</summary>
            public byte Red { get; set; }

            [JsonIgnore]
            /// <summary>Зеленый</summary>
            public byte Green { get; set; }

            [JsonIgnore]
            /// <summary>Синий</summary>
            public byte Blue { get; set; }

            [JsonIgnore]
            /// <summary>Непрозрачность</summary>
            public byte Alpha { get; set; }

            [JsonProperty(PropertyName = "H")]
            public string HEX => $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";

            public Colour(byte Red, byte Green, byte Blue, byte Alpha = 255)
            {
                this.Red = Red;
                this.Green = Green;
                this.Blue = Blue;

                this.Alpha = Alpha;
            }

            [JsonConstructor]
            public Colour(string HEX)
            {
                this.Red = byte.Parse(HEX.Substring(1, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                this.Green = byte.Parse(HEX.Substring(3, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                this.Blue = byte.Parse(HEX.Substring(5, 2), System.Globalization.NumberStyles.AllowHexSpecifier);

                if (HEX.Length == 6)
                    this.Alpha = byte.Parse(HEX.Substring(7, 2), System.Globalization.NumberStyles.AllowHexSpecifier);
                else
                    this.Alpha = 255;
            }

            public Color ToRageColour() => new Color(Red, Green, Blue, Alpha);

            public static Colour FromRageColour(Color colour) => new Colour((byte)colour.Red, (byte)colour.Green, (byte)colour.Blue, (byte)colour.Alpha);
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

        public class Vector4
        {
            [JsonProperty(PropertyName = "P")]
            public Vector3 Position { get; set; }

            [JsonProperty(PropertyName = "RZ")]
            public float RotationZ { get; set; }

            [JsonIgnore]
            public float X => Position.X;

            [JsonIgnore]
            public float Y => Position.Y;

            [JsonIgnore]
            public float Z => Position.Z;

            public Vector4(float X, float Y, float Z, float RotationZ = 0f)
            {
                this.Position = new Vector3(X, Y, Z);

                this.RotationZ = RotationZ;
            }

            public Vector4(Vector3 Position, float RotationZ = 0f) : this(Position.X, Position.Y, Position.Z, RotationZ) { }

            public Vector4(Vector4 Position) : this(Position.X, Position.Y, Position.Z, Position.RotationZ) { }

            public Vector4() { }
        }

        /// <summary>Получить текущее время (по МСК.)</summary>
        public static DateTime GetCurrentTime() => DateTime.UtcNow.AddHours(3);

        public static long GetUnixTimestamp(this DateTime dt) => (long)(new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, TimeSpan.Zero)).Subtract(DateTimeOffset.UnixEpoch).TotalSeconds;

        public static long GetUnixTimestampMil(this DateTime dt) => (long)(new DateTimeOffset(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, TimeSpan.Zero)).Subtract(DateTimeOffset.UnixEpoch).TotalMilliseconds;


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

        public static List<T> ToList<T>(this Newtonsoft.Json.Linq.JArray jArray) => jArray.ToObject<List<T>>();
        public static Dictionary<T1, T2> ToDictionary<T1, T2>(this Newtonsoft.Json.Linq.JArray jArray) => jArray.ToObject<Dictionary<T1, T2>>();

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
        public static VehicleData FindVehicleOnline(uint vid) => vid >= Settings.META_UID_FIRST_VID ? VehicleData.All.Values.Where(x => x.VID == vid).FirstOrDefault() : VehicleData.All.Values.Where(x => x.Vehicle.Id == vid).FirstOrDefault();

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

        public static void TriggerEventToStreamed(this Entity entity, string eventName, params object[] args) => TriggerEventInDistance(entity.Position, entity.Dimension, Settings.STREAM_DISTANCE, eventName, args);

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

        public static bool AreEntitiesNearby(this Entity entity, Entity target, float radius) => (entity.Dimension == target.Dimension && Vector3.Distance(entity.Position, target.Position) <= radius);

        public static bool AreEntitiesNearbyDiffDims(this Entity entity, Entity target, float radius) => (Vector3.Distance(entity.Position, target.Position) <= radius);

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

        public static void Teleport(this Vehicle vehicle, Vector3 position, uint? dimension = null, float? heading = null, bool fade = false, Additional.AntiCheat.VehicleTeleportTypes tpType = Additional.AntiCheat.VehicleTeleportTypes.Default, bool toGround = false) => Additional.AntiCheat.TeleportVehicle(vehicle, position, dimension, heading, fade, tpType, toGround);

        /// <inheritdoc cref="Additional.AntiCheat.SetPlayerPos(Player, Vector3, bool, uint?)"/>
        public static void Teleport(this Player player, Vector3 position, bool toGround, uint? dimension = null, float? heading = null, bool fade = false) => Additional.AntiCheat.TeleportPlayers(position, toGround, dimension, heading, fade, false, player.Dimension, player);

        public static void TeleportPlayers(Vector3 position, bool toGround, uint? dimension = null, float? heading = null, bool fade = false, params Player[] players) => Additional.AntiCheat.TeleportPlayers(position, toGround, dimension, heading, fade, false, null, players);

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
        public static PlayerData FindReadyPlayerOnline(uint pid) => pid >= Settings.META_UID_FIRST_CID ? PlayerData.All.Values.Where(x => x.CID == pid).FirstOrDefault() : PlayerData.All.Where(x => x.Key.Id == pid).Select(x => x.Value).FirstOrDefault();

        /// <summary>Метод для получения сущности игрока, который в сети</summary>
        /// <param name="pid">CID или RemoteID</param>
        /// <returns>Объект класса Player, если игрок найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static Player FindPlayerOnline(uint pid) => pid >= Settings.META_UID_FIRST_CID ? PlayerData.All.Values.Where(x => x.CID == pid).Select(x => x.Player).FirstOrDefault() : NAPI.Pools.GetAllPlayers().Where(x => x?.Id == pid).FirstOrDefault();

        /// <summary>Метод для получения пола игрока</summary>
        /// <param name="player">Сущность игрока</param>
        /// <returns>true - мужчина, false - женщина</returns>
        public static bool GetSex(this Player player) => player?.Model == (uint)PedHash.FreemodeMale01;

        /// <summary>Метод для получения прототипа сущности игрока, который, возможно, не в сети</summary>
        /// <param name="cid">CID</param>
        /// <returns>Объект класса Player, если игрок найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static PlayerData.PlayerInfo FindPlayerOffline(uint cid) => PlayerData.PlayerInfo.Get(cid);

        /// <inheritdoc cref="Additional.AntiSpam.CheckNormal(Player, int)"/>
        public static (bool IsSpammer, PlayerData Data) CheckSpamAttack(this Player player, int decreaseDelay = 250, bool checkPlayerReady = true) { Console.WriteLine((new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name); return Additional.AntiSpam.CheckNormal(player, decreaseDelay, checkPlayerReady); }
        /// <inheritdoc cref="Additional.AntiSpam.CheckTemp(Player, int)"/>
        public static (bool IsSpammer, TempData Data) CheckSpamAttackTemp(this Player player, int decreaseDelay = 250) => Additional.AntiSpam.CheckTemp(player, decreaseDelay);

        /// <inheritdoc cref="Game.Items.Inventory.Replace(PlayerData, Game.Items.Inventory.Groups, int, Game.Items.Inventory.Groups, int, int)"/>
        public static Game.Items.Inventory.Results InventoryReplace(this PlayerData pData, Game.Items.Inventory.Groups to, int slotTo, Game.Items.Inventory.Groups from, int slotFrom, int amount = -1) => Game.Items.Inventory.Replace(pData, to, slotTo, from, slotFrom, amount);

        /// <inheritdoc cref="Game.Items.Inventory.Action(PlayerData, Game.Items.Inventory.Groups, int, int, object[])"/>
        public static Game.Items.Inventory.Results InventoryAction(this PlayerData pData, Game.Items.Inventory.Groups slotStr, int slot, int action = 5, params string[] args) => Game.Items.Inventory.Action(pData, slotStr, slot, action, args);

        /// <inheritdoc cref="Game.Items.Inventory.Drop(PlayerData, Game.Items.Inventory.Groups, int, int)"/>
        public static void InventoryDrop(this PlayerData pData, Game.Items.Inventory.Groups slotStr, int slot, int amount) => Game.Items.Inventory.Drop(pData, slotStr, slot, amount);

        public static bool TryGiveExistingItem(this PlayerData pData, Game.Items.Item item, int amount, bool notifyOnFail = false, bool notifyOnSuccess = false) => Game.Items.Inventory.GiveExisting(pData, item, amount, notifyOnFail, notifyOnSuccess);

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

            var updList = new List<(Game.Items.Inventory.Groups Group, int Slot)>();

            for (int i = 0; i < pData.Weapons.Length; i++)
            {
                if (pData.Weapons[i] != null)
                {
                    pData.Weapons[i].Unwear(pData);

                    pData.Weapons[i].Delete();

                    pData.Weapons[i] = null;

                    updList.Add((Game.Items.Inventory.Groups.Weapons, i));
                }
            }

            MySQL.CharacterWeaponsUpdate(pData.Info);

            if (pData.Holster?.Items[0] is Game.Items.Weapon)
            {
                pData.Holster.Items[0].Delete();

                updList.Add((Game.Items.Inventory.Groups.Holster, 2));

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

                        updList.Add((Game.Items.Inventory.Groups.Items, i));
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

                        updList.Add((Game.Items.Inventory.Groups.Bag, i));
                    }
                }

                pData.Bag.Update();
            }

            foreach (var x in updList)
                pData.Player.InventoryUpdate(x.Group, x.Slot, Game.Items.Item.ToClientJson(null, x.Group));
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

        public static List<(Game.Items.Item Item, Game.Items.Inventory.Groups Group, int Slot)> TakeWeapons(this PlayerData pData)
        {
            pData.UnequipActiveWeapon();

            var tempItems = pData.TempItems;

            if (tempItems == null)
                tempItems = new List<(Game.Items.Item, Game.Items.Inventory.Groups, int)>();

            for (int i = 0; i < pData.Weapons.Length; i++)
            {
                if (pData.Weapons[i] is Game.Items.Weapon weapon)
                {
                    pData.Weapons[i] = null;

                    tempItems.Add((weapon, Game.Items.Inventory.Groups.Weapons, i));

                    pData.Player.InventoryUpdate(Game.Items.Inventory.Groups.Weapons, i, Game.Items.Item.ToClientJson(null, Game.Items.Inventory.Groups.Weapons));
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
                if (x.Group == Game.Items.Inventory.Groups.Weapons)
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

            pData.InventoryAction(Game.Items.Inventory.Groups.Weapons, 0, 5);

            return weapon;
        }

        /// <summary>Выслать уведомление игроку (кастомное)</summary>
        /// <param name="player">Сущность игрока</param>
        /// <param name="type">Тип уведомления</param>
        /// <param name="title">Заголовок</param>
        /// <param name="text">Текст</param>
        /// <param name="timeout">Время показа в мс.</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static void Notify(this Player player, NotificationTypes type, string title, string text, int timeout = 2500) => player.TriggerEvent("Notify::Custom", (int)type, title, text, timeout);

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
        //public static string SerializeToJson(this object value) => JsonConvert.SerializeObject(value, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
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
                Sync.Chat.SendServer(msg, player.Player);
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

        public static Regex NumberplatePattern { get; } = new Regex(@"^[A-Z0-9]{1,8}$", RegexOptions.Compiled);

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
        public static Vector3 GetFrontOf(this Player player, float coeffXY = 1.2f) => player.Position.GetFrontOf(player.Heading, coeffXY);

        public static Vector3 GetFrontOf(this Vector3 pos, float rotationZ = 0f, float coeffXY = 1.2f)
        {
            var radians = -rotationZ * Math.PI / 180;

            return new Vector3(pos.X + (coeffXY * Math.Sin(radians)), pos.Y + (coeffXY * Math.Cos(radians)), pos.Z);
        }

        public static float GetOppositeAngle(float angle) => (angle + 180) % 360;

        /// <summary>Получить сущность ближайшего игрока к сущности</summary>
        /// <param name="entity">Сущность</param>
        /// <returns>Объект класса Player, если игрок был найден, null - в противном случае</returns>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public static PlayerData GetClosestPlayer(this Entity entity)
        {
            if (entity?.Exists != true)
                return null;

            var minDist = Settings.STREAM_DISTANCE;
            PlayerData minPlayer = null;

            foreach (var x in PlayerData.All.Values.Where(x => x.Player.Dimension == entity.Dimension))
            {
                var dist = Vector3.Distance(x.Player.Position, entity.Position);

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
        public static bool AttachObject(this Entity entity, uint model, Sync.AttachSystem.Types type, int detachAfter, string syncData, params object[] args) => Sync.AttachSystem.AttachObject(entity, model, type, detachAfter, syncData, args);

        /// <inheritdoc cref="Sync.AttachSystem.DetachObject(Entity, string)"/>
        public static bool DetachObject(this Entity entity, Sync.AttachSystem.Types type, params object[] args) => Sync.AttachSystem.DetachObject(entity, type, args);

        /// <inheritdoc cref="Sync.AttachSystem.AttachEntity(Entity, int, Sync.AttachSystem.Types)"/>
        public static bool AttachEntity(this Entity entity, Entity target, Sync.AttachSystem.Types type) => Sync.AttachSystem.AttachEntity(entity, target, type);

        /// <inheritdoc cref="Sync.AttachSystem.DetachEntity(Entity, int)"/>
        public static bool DetachEntity(this Entity entity, Entity target) => Sync.AttachSystem.DetachEntity(entity, target);

        /// <inheritdoc cref="Sync.AttachSystem.DetachAllEntities(Entity)"/>
        public static bool DetachAllEntities(this Entity entity) => Sync.AttachSystem.DetachAllEntities(entity);

        /// <inheritdoc cref="Sync.AttachSystem.DetachAllObjects(Entity)"/>
        public static bool DetachAllObjects(this Entity entity) => Sync.AttachSystem.DetachAllObjects(entity);

        /// <inheritdoc cref="Sync.AttachSystem.DetachAllObjectsInHand(Entity)"/>
        public static bool DetachAllObjectsInHand(this Entity entity) => Sync.AttachSystem.DetachAllObjectsInHand(entity);

        /// <inheritdoc cref="Sync.AttachSystem.GetEntityAttachmentData(Entity, Entity)"/>
        public static Sync.AttachSystem.AttachmentEntityNet GetAttachmentData(this Entity entity, Entity target) => Sync.AttachSystem.GetEntityAttachmentData(entity, target);

        public static Entity GetEntityIsAttachedTo(this Entity entity) => Sync.AttachSystem.GetEntityIsAttachedToEntity(entity);

        /// <inheritdoc cref="Sync.Animations.Play(PlayerData, Sync.Animations.GeneralTypes)"/>
        public static void PlayAnim(this PlayerData pData, Sync.Animations.GeneralTypes type) => Sync.Animations.Play(pData, type);

        /// <inheritdoc cref="Sync.Animations.Play(Player, Sync.Animations.FastTypes)"/>
        public static void PlayAnim(this PlayerData pData, Sync.Animations.FastTypes type, int customTimeout = -1) => Sync.Animations.Play(pData, type, customTimeout);

        /// <inheritdoc cref="Sync.Animations.Play(Player, Sync.Animations.OtherTypes)"/>
        public static void PlayAnim(this PlayerData pData, Sync.Animations.OtherTypes type) => Sync.Animations.Play(pData, type);

        /// <inheritdoc cref="Sync.Animations.StopAll(pData)"/>
        public static void StopAllAnims(this PlayerData pData) => Sync.Animations.StopAll(pData);

        public static bool StopFastAnim(this PlayerData pData) => Sync.Animations.StopFastAnim(pData);

        public static bool StopGeneralAnim(this PlayerData pData) => Sync.Animations.StopGeneralAnim(pData);

        public static bool StopOtherAnim(this PlayerData pData) => Sync.Animations.StopOtherAnim(pData);

        /// <inheritdoc cref="Sync.Animations.Set(PlayerData, Sync.Animations.EmotionTypes, bool)"/>
        public static void SetEmotion(this PlayerData pData, Sync.Animations.EmotionTypes type) => Sync.Animations.Set(pData, type);

        /// <inheritdoc cref="Sync.Animations.Set(PlayerData, Sync.Animations.WalkstyleTypes, bool)"/>
        public static void SetWalkstyle(this PlayerData pData, Sync.Animations.WalkstyleTypes type) => Sync.Animations.Set(pData, type);

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

        /// <summary>Передать лог (авторизация) в очередь</summary>
        /// <param name="pData">Данные игрока</param>
        public static void LogAuth(PlayerData pData)
        {
            var cid = pData.CID;
            var rid = pData.Player.Id;
            var ip = pData.Player.Address;

            var date = pData.LastJoinDate;

            //todo
        }

        /// <summary>Передать лог (инвентарь) в очередь</summary>
        /// <param name="pData">Данные игрока</param>
        /// <param name="cont">Контейнер, если null - </param>
        /// <param name="item">Предмет</param>
        /// <param name="amount"></param>
        /// <param name="take">Получил игрок предмет или отдал?</param>
        public static void LogInventory(PlayerData pData, Game.Items.Container cont, Game.Items.Item item, int amount, bool take)
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
                    sw.WriteLine(x);
            }
        }

        public static string ToCSharpStr(this Vector3 v) => v == null ? "null" : $"new Vector3({v.X}f, {v.Y}f, {v.Z}f)";
        public static string ToCSharpStr(this Utils.Vector4 v) => v == null ? "null" : $"new Utils.Vector4({v.X}f, {v.Y}f, {v.Z}f, {v.RotationZ}f)";

        public static string GetBeautyString(this TimeSpan ts)
        {
            var days = ts.Days;

            if (days >= 1)
            {
                var hours = ts.Hours;

                if (hours >= 1)
                    return $"{days} дн. и {hours} ч.";

                return $"{days} дн.";
            }

            var hours1 = ts.Hours;

            if (hours1 >= 1)
            {
                var mins = ts.Minutes;

                if (mins >= 1)
                    return $"{hours1} ч. и {mins} мин.";

                return $"{hours1} ч.";
            }

            var mins1 = ts.Minutes;

            var secs = ts.Seconds;

            if (mins1 >= 1)
            {
                if (secs >= 1)
                    return $"{mins1} мин. и {secs} сек.";

                return $"{mins1} мин.";
            }

            return $"{secs} сек.";
        }

        public static void InventoryUpdate(this Player player, Game.Items.Inventory.Groups group, int slot, string updStr) => player.TriggerEvent("Inventory::Update", (int)group, slot, updStr);

        public static void InventoryUpdate(this Player player, Game.Items.Inventory.Groups group1, int slot1, string updStr1, Game.Items.Inventory.Groups group2, int slot2, string updStr2) => player.TriggerEvent("Inventory::Update", (int)group1, slot1, updStr1, (int)group2, slot2, updStr2);

        public static void InventoryUpdate(Game.Items.Inventory.Groups group1, int slot1, string updStr1, Game.Items.Inventory.Groups group2, int slot2, string updStr2, Player[] players) => NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Inventory::Update", (int)group1, slot1, updStr1, (int)group2, slot2, updStr2);

        public static void InventoryUpdate(Game.Items.Inventory.Groups group, int slot, string updStr, Player[] players) => NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Inventory::Update", (int)group, slot, updStr);

        public static void InventoryUpdate(this Player player, Game.Items.Inventory.Groups group, string updStr) => player.TriggerEvent("Inventory::Update", (int)group, 0, updStr);

        public static void InventoryUpdate(Game.Items.Inventory.Groups group, string updStr, Player[] players) => NAPI.ClientEvent.TriggerClientEventToPlayers(players, "Inventory::Update", (int)group, 0, updStr);

        public static void WarpToVehicleSeat(this Player player, Vehicle veh, int seatId, int timeout = 5000) => player.TriggerEvent("Vehicles::WTS", veh.Id, seatId, timeout);

        public static bool IsAnyAnimOn(this PlayerData pData) => pData.GeneralAnim != Animations.GeneralTypes.None || pData.FastAnim != Animations.FastTypes.None || pData.OtherAnim != Animations.OtherTypes.None;

        public static bool CanPlayAnimNow(this PlayerData pData) => !IsAnyAnimOn(pData) && !pData.CrawlOn;

        public static bool StopUseCurrentItem(this PlayerData pData)
        {
            var curItem = pData.CurrentItemInUse;

            if (curItem == null)
                return false;

            curItem.Value.Item.StopUse(pData, Game.Items.Inventory.Groups.Items, curItem.Value.Slot, true);

            return true;
        }

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

        public static bool TryAdd(this ulong source, ulong value, out ulong result)
        {
            unchecked
            {
                result = source + value;

                return result >= source;
            }
        }

        public static bool TrySubtract(this ulong source, ulong value, out ulong result)
        {
            unchecked
            {
                result = source - value;

                return result <= source;
            }
        }

        public static bool TryAdd(this uint source, uint value, out uint result)
        {
            unchecked
            {
                result = source + value;

                return result >= source;
            }
        }

        public static bool TrySubtract(this uint source, uint value, out uint result)
        {
            unchecked
            {
                result = source - value;

                return result <= source;
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
