using BCRPServer.Game.Data;
using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BCRPServer.Game.Items
{

    #region Item Class

    [JsonConverter(typeof(ItemConverter))]
    public abstract class Item
    {
        private static Dictionary<CEF.Inventory.Groups, Func<Item, string>> ClientJsonFuncs = new Dictionary<CEF.Inventory.Groups, Func<Item, string>>()
        {
            { CEF.Inventory.Groups.Items, (item) => (new object[] { item.ID, Items.GetItemAmount(item), Items.GetItemWeight(item, false), Items.GetItemTag(item) }).SerializeToJson() },

            { CEF.Inventory.Groups.Bag, (item) => (new object[] { item.ID, Items.GetItemAmount(item), Items.GetItemWeight(item, false), Items.GetItemTag(item) }).SerializeToJson() },

            { CEF.Inventory.Groups.Container, (item) => (new object[] { item.ID, Items.GetItemAmount(item), Items.GetItemWeight(item, false), Items.GetItemTag(item) }).SerializeToJson() },

            {
                CEF.Inventory.Groups.Weapons,

                (item) =>
                {
                    var weapon = (Weapon)item;

                    return (new object[] { item.ID, weapon.Ammo, weapon.Equiped, weapon.Tag }).SerializeToJson();
                }
            },

            {
                CEF.Inventory.Groups.Holster,

                (item) =>
                {
                    var weapon = (Weapon)item;

                    return (new object[] { item.ID, weapon.Ammo, weapon.Equiped, weapon.Tag }).SerializeToJson();
                }
            },

            { CEF.Inventory.Groups.Armour, (item) => (new object[] { (item.ID, ((Armour)item).Strength) }).SerializeToJson() },

            {
                CEF.Inventory.Groups.BagItem,

                (item) =>
                {
                    var bag = (Bag)item;

                    return (new object[] { item.ID, bag.Data.MaxWeight, bag.Items.Select(x => ToClientJson(x, CEF.Inventory.Groups.Bag)) }).SerializeToJson();
                }
            },

            { CEF.Inventory.Groups.Clothes, (item) => (new object[] { item.ID }).SerializeToJson() },

            { CEF.Inventory.Groups.Accessories, (item) => (new object[] { item.ID }).SerializeToJson() },

            {
                CEF.Inventory.Groups.HolsterItem,
                
                (item) =>
                {
                    var holster = (Holster)item;

                    return (new object[] { item.ID, ToClientJson(holster.Items[0], CEF.Inventory.Groups.Holster) }).SerializeToJson();
                }
            },
        };

        public abstract class ItemData
        {
            /// <summary>Стандартная модель</summary>
            public static uint DefaultModel = NAPI.Util.GetHashKey("prop_drug_package_02");

            /// <summary>Вес единицы предмета</summary>
            public float Weight { get; set; }

            /// <summary>Основная модель</summary>
            public uint Model { get => Models[0]; }

            /// <summary>Все модели</summary>
            private uint[] Models { get; set; }

            public ItemData(float Weight, params uint[] Models)
            {
                this.Weight = Weight;

                this.Models = Models.Length > 0 ? Models : new uint[] { DefaultModel };
            }

            public ItemData(float Weight, params string[] Models) : this(Weight, Models.Select(x => NAPI.Util.GetHashKey(x)).ToArray()) { }

            public uint GetModelAt(int idx) => idx < 0 || idx >= Models.Length ? Model : Models[idx];
        }

        [JsonIgnore]
        public Type Type { get; set; }

        /// <summary>Данные предмета</summary>
        [JsonIgnore]
        public ItemData Data { get; set; }

        /// <summary>Стандартный вес предмета (1 единица)</summary>
        [JsonIgnore]
        public float Weight { get => Data.Weight; }

        /// <summary>ID модели предмета</summary>
        [JsonIgnore]
        public uint Model { get => Data.Model; }

        /// <summary>Является ли предмет временным?</summary>
        [JsonIgnore]
        public bool IsTemp { get => UID == 0; }

        /// <summary>Уникальный ID предмета</summary>
        /// <value>UID предмета, 0 - если предмет временный и не хранится в базе данных</value>
        [JsonIgnore]
        public uint UID { get; set; }

        /// <summary>ID предмета (см. Game.Items.Items.LoadAll)</summary>
        public string ID { get; set; }

        /// <summary>Метод для удаления предмета из базы данных</summary>
        public void Delete()
        {
            if (IsTemp)
                return;

            MySQL.DeleteItem(this);
        }

        /// <summary>Метод для обновления предмета в базе данных</summary>
        public void Update()
        {
            if (IsTemp)
                return;

            MySQL.UpdateItem(this);
        }

        public string ToClientJson(CEF.Inventory.Groups group)
        {
            var func = ClientJsonFuncs.GetValueOrDefault(group);

            if (func == null)
                return "null";

            return func.Invoke(this);
        }

        public static string ToClientJson(Item item, CEF.Inventory.Groups group) => item == null ? "null" : item.ToClientJson(group);

        public Item(string ID, ItemData Data, Type Type)
        {
            this.ID = ID;
            this.Data = Data;
            this.Type = Type;
        }
    }
    #endregion

    #region Interfaces
    /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут хранить в себе другие предметы</summary>
    public interface IContainer
    {
        /// <summary>Предметы в контейнере</summary>
        /// <value>Массив объектов класса Game.Items.Item, в котором null - пустой слот</value>
        public Item[] Items { get; set; }

        /// <summary>Вес контейнера</summary>
        [JsonIgnore]
        public float Weight { get; }
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут надеваться на игрока</summary>
    public interface IWearable
    {
        /// <summary>Метод для того, чтобы надеть предмет на игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        void Wear(PlayerData pData);

        /// <summary>Метод для того, чтобы снять предмет с игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        void Unwear(PlayerData pData);
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны стакаться</summary>
    public interface IStackable
    {
        /// <summary>Максимальное кол-во единиц предмета в стаке</summary>
        [JsonIgnore]
        public int MaxAmount { get; }

        /// <summary>Общий вес стака</summary>
        [JsonIgnore]
        public float Weight { get; }

        /// <summary>Кол-во единиц предмета в стаке</summary>
        public int Amount { get; set; }
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые, помимо названия, имеют уникальный тэг</summary>
    public interface ITagged
    {
        /// <summary>Тэг</summary>
        public string Tag { get; set; }

        /// <summary>Метод для генерации тэга</summary>
        /// <returns>Тэг</returns>
        public string GenerateTag();
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны тратиться</summary>
    /// <remarks>Не использовать одновременно с IStackable!</remarks>
    public interface IConsumable
    {
        /// <summary>Кол-во оставшихся единиц предмета</summary>
        public int Amount { get; set; }
    }
    #endregion

    #region Weapon
    public class Weapon : Item, ITagged, IWearable
    {
        new public class ItemData : Item.ItemData
        {
            public enum TopTypes
            {
                /// <summary>Рукопашное</summary>
                Melee = 0,
                /// <summary>Одноручное</summary>
                HandGun,
                /// <summary>Полуавтоматическое</summary>
                SubMachine,
                /// <summary>Пулемёты</summary>
                LightMachine,
                /// <summary>Дробовик</summary>
                Shotgun,
                /// <summary>Штурмовая винтовка</summary>
                AssaultRifle,
                /// <summary>Снайперская винтовка</summary>
                SniperRifle,
                /// <summary>Тяжелое оружие</summary>
                HeavyWeapon,
                /// <summary>Метательное</summary>
                Throwable,
                /// <summary>Прочее</summary>
                Misc
            }

            /// <summary>ID подходящих патронов</summary>
            /// <value>Тип патронов, если оружие способно стрелять и заряжаться, null - в противном случае</value>
            public string AmmoID { get; set; }

            /// <summary>Высший тип оружия</summary>
            public TopTypes TopType { get; set; }

            /// <summary>Максимальное кол-во патронов в обойме</summary>
            public int MaxAmmo { get; set; }

            /// <summary>Хэш оружия</summary>
            public uint Hash { get; set; }

            /// <summary>Может ли использоваться в транспорте?</summary>
            public bool CanUseInVehicle { get; set; }

            /// <summary>Подходящие типы для крепления оружия на игрока</summary>
            [JsonIgnore]
            public Sync.AttachSystem.Types[] AttachTypes { get; set; }

            /// <summary>Создать новое оружие</summary>
            /// <param name="ItemType">Глобальный тип предмета</param>
            /// <param name="TopType">Тип оружия</param>
            /// <param name="AmmoType">Тип патронов (если нет - null)</param>
            /// <param name="Hash">Хэш оружия</param>
            /// <param name="MaxAmmo">Максимальное кол-во патронов</param>
            /// <param name="CanUseInVehicle">Может ли использоваться в транспорте?</param>
            public ItemData(float Weight, string Model, TopTypes TopType, string AmmoType, uint Hash, int MaxAmmo, bool CanUseInVehicle = false) : base(Weight, Model)
            {
                this.TopType = TopType;
                this.AmmoID = AmmoType;

                this.CanUseInVehicle = CanUseInVehicle;

                this.Hash = Hash;

                this.MaxAmmo = MaxAmmo;

                if (TopType == TopTypes.Shotgun || TopType == TopTypes.AssaultRifle || TopType == TopTypes.SniperRifle || TopType == TopTypes.HeavyWeapon)
                    this.AttachTypes = new Sync.AttachSystem.Types[] { Sync.AttachSystem.Types.WeaponLeftBack, Sync.AttachSystem.Types.WeaponRightBack };
                else if (TopType == TopTypes.SubMachine)
                    this.AttachTypes = new Sync.AttachSystem.Types[] { Sync.AttachSystem.Types.WeaponLeftTight, Sync.AttachSystem.Types.WeaponRightTight };
            }

            /// <inheritdoc cref="ItemData.ItemData(Types, TopTypes, Types?, uint, int, bool)"/>
            public ItemData(float Weight, string Model, TopTypes TopType, string AmmoType, WeaponHash Hash, int MaxAmmo, bool CanUseInVehicle = false) : this(Weight, Model, TopType, AmmoType, (uint)Hash, MaxAmmo, CanUseInVehicle) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "w_asrifle", new ItemData(1.5f, "w_ar_assaultrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Assaultrifle, 30, false) },
            { "w_asrifle_mk2", new ItemData(1.5f, "w_ar_assaultriflemk2", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Assaultrifle_mk2, 35, false) },
            { "w_advrifle", new ItemData(1f, "w_ar_advancedrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Advancedrifle, 30, false) },
            { "w_carbrifle", new ItemData(1f, "w_ar_carbinerifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Carbinerifle, 30, false) },
            { "w_comprifle", new ItemData(1f, "w_ar_assaultrifle_smg", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Compactrifle, 30, false) },
            { "w_heavyrifle", new ItemData(1f, "w_ar_heavyrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", 0xC78D71B4, 30, false) },

            { "w_microsmg", new ItemData(1f, "w_sb_microsmg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Microsmg, 15, true) },
            { "w_minismg", new ItemData(1f, "w_sb_minismg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Minismg, 15, true) },
            { "w_smg", new ItemData(1f, "w_sb_smg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Smg, 15, true) },
            { "w_smg_mk2", new ItemData(1f, "w_sb_smgmk2", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Smg_mk2, 15, true) },
            { "w_asmsg", new ItemData(1f, "w_sb_assaultsmg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Assaultsmg, 15, false) },
            { "w_combpdw", new ItemData(1f, "w_sb_pdw", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Combatpdw, 15, false) },

            { "w_combmg", new ItemData(1f, "w_mg_combatmg", ItemData.TopTypes.LightMachine, "am_9", WeaponHash.Combatmg, 15, false) },
            { "w_gusenberg", new ItemData(1f, "w_sb_gusenberg", ItemData.TopTypes.LightMachine, "am_9", WeaponHash.Gusenberg, 15, false) },

            { "w_heavysnp", new ItemData(1f, "w_sr_heavysniper", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Heavysniper, 15, false) },
            { "w_markrifle", new ItemData(1f, "w_sr_marksmanrifle", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Marksmanrifle, 15, false) },
            { "w_musket", new ItemData(1f, "w_ar_musket", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Musket, 15, false) },

            { "w_assgun", new ItemData(1f, "w_sg_assaultshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Assaultshotgun, 15, false) },
            { "w_heavysgun", new ItemData(1f, "w_sg_heavyshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Heavyshotgun, 15, false) },
            { "w_pumpsgun", new ItemData(1f, "w_sg_pumpshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Pumpshotgun, 15, false) },
            { "w_pumpsgun_mk2", new ItemData(1f, "w_sg_pumpshotgunmk2", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Pumpshotgun_mk2, 15, false) },
            { "w_sawnsgun", new ItemData(1f, "w_sg_sawnoff", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Sawnoffshotgun, 15, false) },

            { "w_pistol", new ItemData(0.5f, "w_pi_pistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Pistol, 15, true) },
            { "w_pistol_mk2", new ItemData(1f, "w_pi_pistolmk2", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Pistol_mk2, 20, true) },
            { "w_appistol", new ItemData(1f, "w_pi_appistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Appistol, 20, true) },
            { "w_combpistol", new ItemData(1f, "w_pi_combatpistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Combatpistol, 20, true) },
            { "w_heavypistol", new ItemData(1f, "w_pi_heavypistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Heavypistol, 20, true) },
            { "w_machpistol", new ItemData(1f, "w_sb_smgmk2", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Machinepistol, 20, true) },
            { "w_markpistol", new ItemData(1f, "w_pi_singleshot", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Marksmanpistol, 20, true) },
            { "w_vintpistol", new ItemData(1f, "w_pi_vintage_pistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Vintagepistol, 20, true) },

            { "w_revolver", new ItemData(1f, "w_pi_revolver", ItemData.TopTypes.HandGun, "am_11.43", WeaponHash.Revolver, 20, true) },
            { "w_revolver_mk2", new ItemData(1f, "w_pi_revolvermk2", ItemData.TopTypes.HandGun, "am_11.43", WeaponHash.Revolver_mk2, 20, true) },

            { "w_bat", new ItemData(1f, "w_me_bat", ItemData.TopTypes.Melee, null, WeaponHash.Bat, 0, false) },
            { "w_bottle", new ItemData(1f, "w_me_bottle", ItemData.TopTypes.Melee, null, WeaponHash.Bottle, 0, false) },
            { "w_crowbar", new ItemData(1f, "w_me_crowbar", ItemData.TopTypes.Melee, null, WeaponHash.Crowbar, 0, false) },
            { "w_dagger", new ItemData(1f, "w_me_dagger", ItemData.TopTypes.Melee, null, WeaponHash.Dagger, 0, false) },
            { "w_flashlight", new ItemData(1f, "w_me_flashlight", ItemData.TopTypes.Melee, null, WeaponHash.Flashlight, 0, false) },
            { "w_golfclub", new ItemData(1f, "w_me_gclub", ItemData.TopTypes.Melee, null, WeaponHash.Golfclub, 0, false) },
            { "w_hammer", new ItemData(1f, "w_me_hammer", ItemData.TopTypes.Melee, null, WeaponHash.Hammer, 0, false) },
            { "w_hatchet", new ItemData(1f, "w_me_hatchet", ItemData.TopTypes.Melee, null, WeaponHash.Hatchet, 0, false) },
            { "w_knuckles", new ItemData(1f, "w_me_knuckle", ItemData.TopTypes.Melee, null, WeaponHash.Knuckle, 0, false) },
            { "w_machete", new ItemData(1f, "prop_ld_w_me_machette", ItemData.TopTypes.Melee, null, WeaponHash.Machete, 0, false) },
            { "w_nightstick", new ItemData(1f, "w_me_nightstick", ItemData.TopTypes.Melee, null, WeaponHash.Nightstick, 0, false) },
            { "w_poolcue", new ItemData(1f, "prop_pool_cue", ItemData.TopTypes.Melee, null, WeaponHash.Poolcue, 0, false) },
            { "w_switchblade", new ItemData(1f, "w_me_switchblade", ItemData.TopTypes.Melee, null, WeaponHash.Switchblade, 0, false) },
            { "w_wrench", new ItemData(0.75f, "prop_cs_wrench", ItemData.TopTypes.Melee, null, WeaponHash.Wrench, 0, false) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        /// <summary>Обший вес оружия (вместе с патронами в обойме)</summary>
        [JsonIgnore]
        new public float Weight { get => Data.AmmoID == null ? base.Weight : base.Weight + Ammo * (Game.Items.Ammo.IDList[Data.AmmoID].Weight); }

        public string Tag { get; set; }

        /// <summary>Используется ли оружие?</summary>
        [JsonIgnore]
        public bool Equiped { get; set; }

        /// <summary>ID привязанного к игроку объекта оружия</summary>
        [JsonIgnore]
        public int AttachID { get; set; }

        /// <summary>Кол-во патронов в обойме</summary>
        public int Ammo { get; set; }

        /// <summary>Метод для выдачи оружия игроку</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void Equip(PlayerData pData)
        {
            if (Equiped)
                return;

            var player = pData.Player;

            if (pData.PhoneOn)
                Sync.Players.StopUsePhone(player);

            Unwear(pData);

            Equiped = true;

            player.SetWeapon(Data.Hash, Ammo);

            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                var weap = pData.ActiveWeapon;

                if (weap != null && weap.Value.WeaponItem == this)
                    player.SetWeapon(Data.Hash, Ammo);
            }, 250);
        }

        /// <summary>Метод, чтобы забрать оружие у игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <param name="updateLastAmmoFirst">Обновить ли кол-во патронов в обойме?</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void Unequip(PlayerData pData, bool updateLastAmmoFirst = true, bool wearAfter = true)
        {
            var player = pData.Player;

            if (!Equiped)
                return;

            if (updateLastAmmoFirst)
            {
                var amount = NAPI.Player.GetPlayerWeaponAmmo(player, this.Data.Hash);

                if (amount < 0)
                    amount = 0;

                if (amount <= this.Ammo)
                {
                    this.Ammo = amount;

                    this.Update();
                }
            }

            player.SetWeapon((uint)WeaponHash.Unarmed);

            Equiped = false;

            if (wearAfter)
                Wear(pData);
        }

        /// <summary>Метод, чтобы выдать патроны игроку</summary>
        /// <param name="pData">Сущность игрока</param>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public void UpdateAmmo(PlayerData pData)
        {
            if (!Equiped)
                return;

            var player = pData.Player;

            player.SetAmmo(Ammo);
            player.TriggerEvent("Weapon::TaskReload");
        }

        public string GenerateTag()
        {
            return "";
        }

        public void Wear(PlayerData pData)
        {
            if (AttachID != -1 || Equiped)
                return;

            var player = pData.Player;

            if (pData.Holster?.Items[0] == this)
            {
                pData.Holster.WearWeapon(pData);

                return;
            }

            var attachTypes = this.Data.AttachTypes;

            if (attachTypes == null)
                return;

            if (pData.AttachedObjects.Where(x => x.Type == attachTypes[0]).Any())
                AttachID = player.AttachObject(Model, attachTypes[1], -1);
            else
                AttachID = player.AttachObject(Model, attachTypes[0], -1);
        }

        public void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.Holster != null && (pData.Holster.Items[0] == null || pData.Holster.Items[0] == this))
                pData.Holster.UnwearWeapon(pData);

            if (AttachID == -1)
                return;

            player.DetachObject(AttachID);

            AttachID = -1;
        }

        public Weapon(string ID) : base(ID, IDList[ID], typeof(Weapon))
        {
            this.Tag = "";

            this.AttachID = -1;
        }
    }
    #endregion

    #region Ammo
    public class Ammo : Item, IStackable
    {
        new public class ItemData : Item.ItemData
        {
            public ItemData(float Weight, string Model) : base(Weight, Model)
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "am_5.56", new ItemData(0.01f, "w_am_case") },
            { "am_7.62", new ItemData(0.01f, "w_am_case") },
            { "am_9", new ItemData(0.01f, "w_am_case") },
            { "am_11.43", new ItemData(0.015f, "w_am_case") },
            { "am_12", new ItemData(0.015f, "w_am_case") },
            { "am_12.7", new ItemData(0.015f, "w_am_case") },
        };

        public static ItemData GetData(string id) => (ItemData)IDList[id];

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public float Weight { get => Amount * base.Weight; }

        [JsonIgnore]
        public int MaxAmount => 999;

        public int Amount { get; set; }

        public Ammo(string ID) : base(ID, IDList[ID], typeof(Ammo))
        {

        }
    }
    #endregion

    #region Clothes
    public abstract class Clothes : Item, IWearable
    {
        public interface IToggleable
        {
            [JsonIgnore]
            public bool Toggled { get; set; }

            public void Action(PlayerData pData);
        }

        new public abstract class ItemData : Item.ItemData
        {
            public class ExtraData
            {
                public int Drawable { get; set; }

                public int BestTorso { get; set; }

                public ExtraData(int Drawable, int BestTorso)
                {
                    this.Drawable = Drawable;
                    this.BestTorso = BestTorso;
                }
            }

            public bool Sex { get; set; }

            public int Drawable { get; set; }

            public int[] Textures { get; set; }

            public string SexAlternativeID { get; set; }

            public ItemData(float Weight, string Model, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Weight, Model)
            {
                this.Drawable = Drawable;
                this.Textures = Textures;

                this.Sex = Sex;

                this.SexAlternativeID = SexAlternativeID;
            }
        }

        public abstract void Wear(PlayerData pData);

        public abstract void Unwear(PlayerData pData);

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public ItemData SexAlternativeData { get; set; }

        /// <summary>Вариация одежды</summary>
        public int Var { get; set; }

        public Clothes(string ID, Item.ItemData Data, Type Type, int Var) : base(ID, Data, Type)
        {
            this.Var = Var;

            var sexAltId = this.Data.SexAlternativeID;

            if (sexAltId != null)
                SexAlternativeData = (ItemData)Items.GetData(sexAltId, Type);
        }
    }

    public class Hat : Clothes, Clothes.IToggleable
    {
        new public class ItemData : Clothes.ItemData
        {
            public ExtraData ExtraData { get; set; }

            public ItemData(bool Sex, int Drawable, int[] Textures, ExtraData ExtraData = null, string SexAlternativeID = null) : base(0.1f, "prop_proxy_hat_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.ExtraData = ExtraData;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region Hats Male
            { "hat_m_0", new ItemData(true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_1", new ItemData(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_2", new ItemData(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_3", new ItemData(true, 28, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "hat_m_4", new ItemData(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_5", new ItemData(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_6", new ItemData(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_7", new ItemData(true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_8", new ItemData(true, 20, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "hat_m_9", new ItemData(true, 31, new int[] { 0 }) },
            { "hat_m_10", new ItemData(true, 32, new int[] { 0 }) },
            { "hat_m_11", new ItemData(true, 34, new int[] { 0 }) },
            { "hat_m_12", new ItemData(true, 37, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "hat_m_13", new ItemData(true, 54, new int[] { 0, 1 }) },
            { "hat_m_14", new ItemData(true, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_15", new ItemData(true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_16", new ItemData(true, 65, new int[] { 0 }) },
            { "hat_m_17", new ItemData(true, 83, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_m_18", new ItemData(true, 94, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_19", new ItemData(true, 120, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_m_20", new ItemData(true, 135, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            { "hat_m_21", new ItemData(true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_22", new ItemData(true, 50, new int[] { 0 }) },
            { "hat_m_23", new ItemData(true, 51, new int[] { 0 }) },
            { "hat_m_24", new ItemData(true, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "hat_m_25", new ItemData(true, 78, new int[] { 0, 1, 2, 3, 4 }) },
            { "hat_m_26", new ItemData(true, 80, new int[] { 0, 1, 2, 3 }) },
            { "hat_m_27", new ItemData(true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            { "hat_m_28", new ItemData(true, 85, new int[] { 0 }) },
            { "hat_m_29", new ItemData(true, 86, new int[] { 0 }) },
            { "hat_m_30", new ItemData(true, 87, new int[] { 0 }) },
            { "hat_m_31", new ItemData(true, 88, new int[] { 0 }) },
            { "hat_m_32", new ItemData(true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }) },
            { "hat_m_33", new ItemData(true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "hat_m_34", new ItemData(true, 130, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }) },
            { "hat_m_35", new ItemData(true, 139, new int[] { 0, 1, 2 }) },
            { "hat_m_36", new ItemData(true, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_m_37", new ItemData(true, 151, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_38", new ItemData(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_39", new ItemData(true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_40", new ItemData(true, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_41", new ItemData(true, 25, new int[] { 0, 1, 2 }) },
            { "hat_m_42", new ItemData(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "hat_m_43", new ItemData(true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "hat_m_44", new ItemData(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_m_45", new ItemData(true, 30, new int[] { 0, 1 }) },
            { "hat_m_46", new ItemData(true, 33, new int[] { 0, 1 }) },
            { "hat_m_47", new ItemData(true, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_m_48", new ItemData(true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_49", new ItemData(true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "hat_m_50", new ItemData(true, 84, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_51", new ItemData(true, 89, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_m_52", new ItemData(true, 90, new int[] { 0 }) },
            { "hat_m_53", new ItemData(true, 96, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "hat_m_54", new ItemData(true, 154, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            #endregion

            #region Hats Female
            { "hat_f_0", new ItemData(false, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_1", new ItemData(false, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_2", new ItemData(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_3", new ItemData(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_4", new ItemData(false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_f_5", new ItemData(false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_f_6", new ItemData(false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_f_7", new ItemData(false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_8", new ItemData(false, 30, new int[] { 0 }) },
            { "hat_f_9", new ItemData(false, 31, new int[] { 0 }) },
            { "hat_f_10", new ItemData(false, 33, new int[] { 0 }) },
            { "hat_f_11", new ItemData(false, 36, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "hat_f_12", new ItemData(false, 53, new int[] { 0, 1 }) },
            { "hat_f_13", new ItemData(false, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_14", new ItemData(false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_15", new ItemData(false, 64, new int[] { 0 }) },
            { "hat_f_16", new ItemData(false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "hat_f_17", new ItemData(false, 131, new int[] { 0, 1, 2, 3 }) },
            { "hat_f_18", new ItemData(false, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_f_19", new ItemData(false, 134, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            { "hat_f_20", new ItemData(false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_21", new ItemData(false, 50, new int[] { 0 }) },
            { "hat_f_22", new ItemData(false, 49, new int[] { 0 }) },
            { "hat_f_23", new ItemData(false, 72, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "hat_f_24", new ItemData(false, 77, new int[] { 0, 1, 2, 3, 4 }) },
            { "hat_f_25", new ItemData(false, 79, new int[] { 0, 1, 2, 3 }) },
            { "hat_f_26", new ItemData(false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            { "hat_f_27", new ItemData(false, 84, new int[] { 0 }) },
            { "hat_f_28", new ItemData(false, 85, new int[] { 0 }) },
            { "hat_f_29", new ItemData(false, 86, new int[] { 0 }) },
            { "hat_f_30", new ItemData(false, 87, new int[] { 0 }) },
            { "hat_f_31", new ItemData(false, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }) },
            { "hat_f_32", new ItemData(false, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "hat_f_33", new ItemData(false, 129, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }) },
            { "hat_f_34", new ItemData(false, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "hat_f_35", new ItemData(false, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_36", new ItemData(false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_37", new ItemData(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_38", new ItemData(false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_39", new ItemData(false, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_40", new ItemData(false, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "hat_f_41", new ItemData(false, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "hat_f_42", new ItemData(false, 54, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "hat_f_43", new ItemData(false, 32, new int[] { 0, 1 }) },
            { "hat_f_44", new ItemData(false, 95, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "hat_f_45", new ItemData(false, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_46", new ItemData(false, 83, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_47", new ItemData(false, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "hat_f_48", new ItemData(false, 89, new int[] { 0 }) },
            { "hat_f_49", new ItemData(false, 153, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            #endregion
        };

        [JsonIgnore]
        /// <summary>Переключено ли состояние</summary>
        public bool Toggled { get; set; }

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        [JsonIgnore]
        public const int Slot = (int)Customization.AccessoryTypes.Hat;

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != player.GetSex())
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            if (data.ExtraData != null)
                player.SetAccessories(Slot, Toggled ? data.ExtraData.Drawable : data.Drawable, variation);
            else
                player.SetAccessories(Slot, data.Drawable, variation);

            pData.Hat = $"{this.ID}|{variation}|{(Toggled ? 1 : 0)}";
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(0);

            pData.Hat = null;
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Hat(string ID, int Variation) : base(ID, IDList[ID], typeof(Hat), Variation)
        {

        }
    }

    public class Top : Clothes, Clothes.IToggleable
    {
        new public class ItemData : Clothes.ItemData
        {
            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public ItemData(bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : base(0.3f, "prop_ld_shirt_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTorso = BestTorso;
                this.ExtraData = ExtraData;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region Tops Male
            { "top_m_0", new ItemData(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_1", new ItemData(true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_m_3", new ItemData(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_4", new ItemData(true, 37, new int[] { 0, 1, 2 }, 14) },
            { "top_m_5", new ItemData(true, 57, new int[] { 0 }, 4) },
            { "top_m_6", new ItemData(true, 105, new int[] { 0 }, 0) },
            { "top_m_7", new ItemData(true, 81, new int[] { 0, 1, 2 }, 0) },
            { "top_m_8", new ItemData(true, 123, new int[] { 0, 1, 2 }, 0) },
            { "top_m_9", new ItemData(true, 128, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0) },
            { "top_m_10", new ItemData(true, 83, new int[] { 0, 1, 2, 3, 4 }, 0) },
            { "top_m_11", new ItemData(true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0) },
            { "top_m_12", new ItemData(true, 84, new int[] { 0, 1, 2, 3, 4, 5 }, 4) },
            { "top_m_13", new ItemData(true, 61, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_14", new ItemData(true, 62, new int[] { 0 }, 14) },
            { "top_m_15", new ItemData(true, 64, new int[] { 0 }, 14) },
            { "top_m_16", new ItemData(true, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6) },
            { "top_m_17", new ItemData(true, 69, new int[] { 0, 1, 2, 3, 4, 5 }, 14, new ItemData.ExtraData(68, 14)) },
            { "top_m_18", new ItemData(true, 79, new int[] { 0 }, 6) },
            { "top_m_19", new ItemData(true, 235, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0, new ItemData.ExtraData(236, 0)) },
            { "top_m_20", new ItemData(true, 86, new int[] { 0, 1, 2, 3, 4 }, 4) },
            { "top_m_22", new ItemData(true, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(87, 14)) },
            { "top_m_23", new ItemData(true, 106, new int[] { 0 }, 14) },
            { "top_m_24", new ItemData(true, 234, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11) },
            { "top_m_25", new ItemData(true, 107, new int[] { 0, 1, 2, 3, 4 }, 4) },
            { "top_m_26", new ItemData(true, 110, new int[] { 0 }, 4) },
            { "top_m_27", new ItemData(true, 113, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_m_28", new ItemData(true, 114, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14) },
            { "top_m_29", new ItemData(true, 118, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_m_30", new ItemData(true, 121, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4) },
            { "top_m_31", new ItemData(true, 122, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14) },
            { "top_m_32", new ItemData(true, 126, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 4, new ItemData.ExtraData(127, 14)) },
            { "top_m_33", new ItemData(true, 130, new int[] { 0 }, 14) },
            { "top_m_34", new ItemData(true, 164, new int[] { 0, 1, 2 }, 0) },
            { "top_m_35", new ItemData(true, 136, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14) },
            { "top_m_36", new ItemData(true, 137, new int[] { 0, 1, 2 }, 15) },
            { "top_m_37", new ItemData(true, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4) },
            { "top_m_38", new ItemData(true, 147, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4) },
            { "top_m_39", new ItemData(true, 148, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4) },
            { "top_m_40", new ItemData(true, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_m_41", new ItemData(true, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4) },
            { "top_m_42", new ItemData(true, 151, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_43", new ItemData(true, 153, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4) },
            { "top_m_44", new ItemData(true, 154, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 12) },
            { "top_m_45", new ItemData(true, 157, new int[] { 0, 1, 2, 3 }, 112) },
            { "top_m_46", new ItemData(true, 160, new int[] { 0, 1 }, 112) },
            { "top_m_47", new ItemData(true, 162, new int[] { 0, 1, 2, 3 }, 114) },
            { "top_m_48", new ItemData(true, 163, new int[] { 0 }, 14) },
            { "top_m_49", new ItemData(true, 166, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_50", new ItemData(true, 167, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_51", new ItemData(true, 169, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_52", new ItemData(true, 170, new int[] { 0, 1, 2, 3 }, 112) },
            { "top_m_53", new ItemData(true, 172, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_54", new ItemData(true, 173, new int[] { 0, 1, 2, 3 }, 112) },
            { "top_m_55", new ItemData(true, 174, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_m_56", new ItemData(true, 175, new int[] { 0, 1, 2, 3 }, 114) },
            { "top_m_57", new ItemData(true, 184, new int[] { 0, 1, 2, 3 }, 6, new ItemData.ExtraData(185, 14)) },
            { "top_m_58", new ItemData(true, 187, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 6, new ItemData.ExtraData(204, 6)) },
            { "top_m_59", new ItemData(true, 200, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(203, 4)) },
            { "top_m_60", new ItemData(true, 205, new int[] { 0, 1, 2, 3, 4 }, 114, new ItemData.ExtraData(202, 114)) },
            { "top_m_61", new ItemData(true, 230, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(229, 14)) },
            { "top_m_62", new ItemData(true, 251, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(253, 4)) },
            { "top_m_63", new ItemData(true, 193, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0) },
            { "top_m_64", new ItemData(true, 258, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 6) },
            { "top_m_65", new ItemData(true, 261, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_66", new ItemData(true, 255, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4) },
            { "top_m_67", new ItemData(true, 257, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6) },
            { "top_m_68", new ItemData(true, 259, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 6) },
            { "top_m_69", new ItemData(true, 262, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4, new ItemData.ExtraData(263, 4)) },
            { "top_m_70", new ItemData(true, 264, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 6) },
            { "top_m_71", new ItemData(true, 271, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0) },
            { "top_m_72", new ItemData(true, 273, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, 0) },
            { "top_m_73", new ItemData(true, 279, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 4, new ItemData.ExtraData(280, 4)) },
            { "top_m_74", new ItemData(true, 281, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6) },
            { "top_m_75", new ItemData(true, 282, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0) },
            { "top_m_76", new ItemData(true, 296, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(297, 4)) },
            { "top_m_77", new ItemData(true, 308, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4) },
            { "top_m_78", new ItemData(true, 313, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0) },
            { "top_m_79", new ItemData(true, 325, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 0) },
            { "top_m_80", new ItemData(true, 323, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0) },
            { "top_m_81", new ItemData(true, 50, new int[] { 0, 1, 2, 3, 4 }, 4) },
            { "top_m_82", new ItemData(true, 59, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_83", new ItemData(true, 67, new int[] { 0, 1, 2, 3 }, 4) },
            { "top_m_84", new ItemData(true, 85, new int[] { 0 }, 1) },
            { "top_m_85", new ItemData(true, 89, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_m_86", new ItemData(true, 109, new int[] { 0 }, 15) },
            { "top_m_87", new ItemData(true, 111, new int[] { 0, 1, 2, 3, 4, 5 }, 4) },
            { "top_m_88", new ItemData(true, 124, new int[] { 0 }, 14) },
            { "top_m_89", new ItemData(true, 125, new int[] { 0 }, 4) },
            { "top_m_90", new ItemData(true, 131, new int[] { 0 }, 0, new ItemData.ExtraData(132, 0)) },
            { "top_m_92", new ItemData(true, 134, new int[] { 0, 1, 2 }, 4) },
            { "top_m_93", new ItemData(true, 135, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 0) },
            { "top_m_94", new ItemData(true, 138, new int[] { 0, 1, 2 }, 4) },
            { "top_m_97", new ItemData(true, 155, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_98", new ItemData(true, 158, new int[] { 0, 1, 2 }, 113) },
            { "top_m_99", new ItemData(true, 159, new int[] { 0, 1 }, 114) },
            { "top_m_100", new ItemData(true, 165, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 6) },
            { "top_m_101", new ItemData(true, 168, new int[] { 0, 1, 2 }, 12) },
            { "top_m_102", new ItemData(true, 171, new int[] { 0, 1 }, 4) },
            { "top_m_103", new ItemData(true, 176, new int[] { 0 }, 114) },
            { "top_m_104", new ItemData(true, 177, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 2) },
            { "top_m_105", new ItemData(true, 181, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_106", new ItemData(true, 189, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, new ItemData.ExtraData(188, 14)) },
            { "top_m_107", new ItemData(true, 190, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 6) },
            { "top_m_108", new ItemData(true, 223, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 2) },
            { "top_m_109", new ItemData(true, 224, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 12) },
            { "top_m_110", new ItemData(true, 225, new int[] { 0, 1 }, 8) },
            { "top_m_112", new ItemData(true, 237, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5) },
            { "top_m_113", new ItemData(true, 238, new int[] { 0, 1, 2, 3, 4, 5 }, 2) },
            { "top_m_114", new ItemData(true, 241, new int[] { 0, 1, 2, 3, 4, 5 }, 0, new ItemData.ExtraData(242, 0)) },
            { "top_m_116", new ItemData(true, 329, new int[] { 0 }, 4) },
            { "top_m_117", new ItemData(true, 330, new int[] { 0 }, 4, new ItemData.ExtraData(331, 4)) },
            { "top_m_118", new ItemData(true, 332, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4) },
            { "top_m_119", new ItemData(true, 334, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0) },
            { "top_m_120", new ItemData(true, 335, new int[] { 0, 1, 2, 3, 4, 5 }, 8) },
            { "top_m_121", new ItemData(true, 340, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 14, new ItemData.ExtraData(341, 1)) },
            { "top_m_122", new ItemData(true, 342, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 4) },
            { "top_m_123", new ItemData(true, 344, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14, new ItemData.ExtraData(343, 14)) },
            { "top_m_124", new ItemData(true, 350, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0) },
            { "top_m_126", new ItemData(true, 217, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 6) },
            { "top_m_127", new ItemData(true, 233, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, new ItemData.ExtraData(232, 14)) },
            { "top_m_128", new ItemData(true, 351, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0) },
            { "top_m_129", new ItemData(true, 352, new int[] { 0, 1, 2 }, 4, new ItemData.ExtraData(353, 4)) },
            { "top_m_130", new ItemData(true, 357, new int[] { 0, 1 }, 2) },
            { "top_m_132", new ItemData(true, 382, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, new ItemData.ExtraData(383, 0)) },
            { "top_m_133", new ItemData(true, 384, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(385, 4)) },
            { "top_m_134", new ItemData(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, new ItemData.ExtraData(10, 14)) },
            { "top_m_135", new ItemData(true, 119, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_m_136", new ItemData(true, 21, new int[] { 0, 1, 2, 3 }, 15) },
            { "top_m_137", new ItemData(true, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 15) },
            { "top_m_138", new ItemData(true, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 14) },
            { "top_m_139", new ItemData(true, 27, new int[] { 0, 1, 2 }, 14) },
            { "top_m_140", new ItemData(true, 28, new int[] { 0, 1, 2 }, 14) },
            { "top_m_141", new ItemData(true, 35, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14) },
            { "top_m_142", new ItemData(true, 45, new int[] { 0, 1, 2 }, 15) },
            { "top_m_143", new ItemData(true, 46, new int[] { 0, 1, 2 }, 14) },
            { "top_m_144", new ItemData(true, 58, new int[] { 0 }, 14) },
            { "top_m_145", new ItemData(true, 70, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_m_146", new ItemData(true, 72, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_147", new ItemData(true, 74, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, new ItemData.ExtraData(75, 14)) },
            { "top_m_148", new ItemData(true, 76, new int[] { 0, 1, 2, 3, 4 }, 14) },
            { "top_m_149", new ItemData(true, 77, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_150", new ItemData(true, 99, new int[] { 0, 1, 2, 3, 4 }, 14, new ItemData.ExtraData(100, 14)) },
            { "top_m_151", new ItemData(true, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14) },
            { "top_m_152", new ItemData(true, 142, new int[] { 0, 1, 2 }, 14) },
            { "top_m_153", new ItemData(true, 140, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14) },
            { "top_m_154", new ItemData(true, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14) },
            { "top_m_155", new ItemData(true, 183, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_156", new ItemData(true, 191, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14) },
            { "top_m_157", new ItemData(true, 192, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_m_158", new ItemData(true, 269, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_m_159", new ItemData(true, 266, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 14, new ItemData.ExtraData(265, 14)) },
            { "top_m_160", new ItemData(true, 260, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11) },
            { "top_m_161", new ItemData(true, 298, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 4) },
            { "top_m_162", new ItemData(true, 299, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0) },
            { "top_m_163", new ItemData(true, 303, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(300, 6)) },
            { "top_m_164", new ItemData(true, 301, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(302, 14)) },
            { "top_m_165", new ItemData(true, 304, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_m_166", new ItemData(true, 305, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(306, 4)) },
            { "top_m_167", new ItemData(true, 307, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6) },
            { "top_m_168", new ItemData(true, 309, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14) },
            { "top_m_170", new ItemData(true, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 15) },
            { "top_m_171", new ItemData(true, 20, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_172", new ItemData(true, 23, new int[] { 0, 1, 2, 3 }, 14) },
            { "top_m_173", new ItemData(true, 40, new int[] { 0, 1 }, 15) },
            { "top_m_174", new ItemData(true, 103, new int[] { 0 }, 14) },
            { "top_m_175", new ItemData(true, 112, new int[] { 0 }, 14) },
            { "top_m_176", new ItemData(true, 115, new int[] { 0 }, 14) },
            { "top_m_177", new ItemData(true, 120, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15) },
            { "top_m_178", new ItemData(true, 161, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_m_179", new ItemData(true, 240, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_180", new ItemData(true, 310, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14) },
            { "top_m_181", new ItemData(true, 338, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_m_182", new ItemData(true, 348, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 1, new ItemData.ExtraData(349, 1)) },
            { "top_m_183", new ItemData(true, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6) },
            { "top_m_184", new ItemData(true, 355, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 184, new ItemData.ExtraData(354, 184)) },
            { "top_m_185", new ItemData(true, 358, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 6) },
            { "top_m_186", new ItemData(true, 360, new int[] { 0 }, 14, new ItemData.ExtraData(359, 14)) },
            { "top_m_187", new ItemData(true, 361, new int[] { 0 }, 4) },
            { "top_m_188", new ItemData(true, 369, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 2) },
            { "top_m_189", new ItemData(true, 370, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 12) },
            { "top_m_190", new ItemData(true, 371, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4) },
            { "top_m_191", new ItemData(true, 374, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 4, new ItemData.ExtraData(373, 4)) },
            { "top_m_192", new ItemData(true, 376, new int[] { 0, 1, 2 }, 14, new ItemData.ExtraData(375, 14)) },
            { "top_m_193", new ItemData(true, 377, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0) },
            { "top_m_194", new ItemData(true, 378, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 12) },
            { "top_m_195", new ItemData(true, 381, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(379, 14)) },
            { "top_m_196", new ItemData(true, 387, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, new ItemData.ExtraData(386, 14)) },
            { "top_m_197", new ItemData(true, 390, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(388, 14)) },
            { "top_m_198", new ItemData(true, 391, new int[] { 0, 1, 2 }, 14, new ItemData.ExtraData(389, 14)) },

            { "top_m_199", new ItemData(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, new ItemData.ExtraData(30, 14)) },
            { "top_m_200", new ItemData(true, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, new ItemData.ExtraData(32, 14)) },
            #endregion

            #region Tops Female
            { "top_f_1", new ItemData(false, 161, new int[] { 0, 1, 2 }, 9) },
            { "top_f_2", new ItemData(false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 4) },
            { "top_f_3", new ItemData(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_4", new ItemData(false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_5", new ItemData(false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 7) },
            { "top_f_6", new ItemData(false, 17, new int[] { 0 }, 9) },
            { "top_f_8", new ItemData(false, 31, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 5) },
            { "top_f_10", new ItemData(false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 4) },
            { "top_f_11", new ItemData(false, 37, new int[] { 0, 1, 2, 3, 4, 5 }, 4) },
            { "top_f_12", new ItemData(false, 43, new int[] { 0, 1, 2, 3, 4 }, 3) },
            { "top_f_13", new ItemData(false, 50, new int[] { 0 }, 3) },
            { "top_f_14", new ItemData(false, 54, new int[] { 0, 1, 2, 3 }, 3) },
            { "top_f_15", new ItemData(false, 55, new int[] { 0 }, 3) },
            { "top_f_16", new ItemData(false, 63, new int[] { 0, 1, 2, 3, 4, 5 }, 3) },
            { "top_f_17", new ItemData(false, 69, new int[] { 0 }, 5) },
            { "top_f_18", new ItemData(false, 71, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1) },
            { "top_f_19", new ItemData(false, 72, new int[] { 0 }, 1) },
            { "top_f_20", new ItemData(false, 76, new int[] { 0, 1, 2, 3, 4 }, 9) },
            { "top_f_21", new ItemData(false, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3) },
            { "top_f_22", new ItemData(false, 79, new int[] { 0, 1, 2, 3 }, 1) },
            { "top_f_23", new ItemData(false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_24", new ItemData(false, 86, new int[] { 0, 1, 2 }, 9) },
            { "top_f_25", new ItemData(false, 96, new int[] { 0 }, 9) },
            { "top_f_26", new ItemData(false, 97, new int[] { 0 }, 6) },
            { "top_f_27", new ItemData(false, 98, new int[] { 0, 1, 2, 3, 4 }, 3) },
            { "top_f_28", new ItemData(false, 121, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 3) },
            { "top_f_29", new ItemData(false, 100, new int[] { 0 }, 6) },
            { "top_f_30", new ItemData(false, 105, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0) },
            { "top_f_31", new ItemData(false, 106, new int[] { 0, 1, 2, 3 }, 6) },
            { "top_f_32", new ItemData(false, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0) },
            { "top_f_33", new ItemData(false, 110, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3) },
            { "top_f_34", new ItemData(false, 116, new int[] { 0, 1, 2 }, 11) },
            { "top_f_35", new ItemData(false, 119, new int[] { 0, 1, 2 }, 14) },
            { "top_f_36", new ItemData(false, 125, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_f_37", new ItemData(false, 127, new int[] { 0 }, 3) },
            { "top_f_38", new ItemData(false, 132, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 9) },
            { "top_f_39", new ItemData(false, 133, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 6) },
            { "top_f_40", new ItemData(false, 138, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6) },
            { "top_f_41", new ItemData(false, 140, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3) },
            { "top_f_42", new ItemData(false, 144, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3) },
            { "top_f_43", new ItemData(false, 123, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_44", new ItemData(false, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_45", new ItemData(false, 146, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 7) },
            { "top_f_46", new ItemData(false, 147, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_47", new ItemData(false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_48", new ItemData(false, 151, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 1) },
            { "top_f_49", new ItemData(false, 154, new int[] { 0, 1, 2, 3 }, 129) },
            { "top_f_50", new ItemData(false, 157, new int[] { 0, 1 }, 132) },
            { "top_f_51", new ItemData(false, 159, new int[] { 0, 1, 2, 3 }, 131) },
            { "top_f_52", new ItemData(false, 160, new int[] { 0 }, 5) },
            { "top_f_53", new ItemData(false, 163, new int[] { 0, 1, 2, 3, 4, 5 }, 5) },
            { "top_f_54", new ItemData(false, 164, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_55", new ItemData(false, 166, new int[] { 0, 1, 2, 3 }, 5) },
            { "top_f_56", new ItemData(false, 167, new int[] { 0, 1, 2, 3 }, 129) },
            { "top_f_57", new ItemData(false, 171, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 153) },
            { "top_f_58", new ItemData(false, 174, new int[] { 0, 1, 2, 3 }, 5) },
            { "top_f_59", new ItemData(false, 175, new int[] { 0, 1, 2, 3 }, 129) },
            { "top_f_60", new ItemData(false, 158, new int[] { 0, 1, 2, 3 }, 7) },
            { "top_f_61", new ItemData(false, 176, new int[] { 0, 1, 2, 3 }, 7) },
            { "top_f_62", new ItemData(false, 177, new int[] { 0, 1, 2, 3 }, 131) },
            { "top_f_63", new ItemData(false, 186, new int[] { 0, 1, 2, 3 }, 3) },
            { "top_f_64", new ItemData(false, 192, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 1) },
            { "top_f_65", new ItemData(false, 195, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 153) },
            { "top_f_66", new ItemData(false, 202, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_68", new ItemData(false, 233, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 11) },
            { "top_f_69", new ItemData(false, 234, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6) },
            { "top_f_70", new ItemData(false, 251, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_71", new ItemData(false, 239, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_72", new ItemData(false, 259, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_73", new ItemData(false, 262, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 7) },
            { "top_f_74", new ItemData(false, 270, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_75", new ItemData(false, 267, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 1) },
            { "top_f_76", new ItemData(false, 266, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6) },
            { "top_f_77", new ItemData(false, 268, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 1) },
            { "top_f_78", new ItemData(false, 271, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 3) },
            { "top_f_79", new ItemData(false, 280, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14) },
            { "top_f_80", new ItemData(false, 286, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, 14) },
            { "top_f_81", new ItemData(false, 292, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 3) },
            { "top_f_82", new ItemData(false, 294, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 1) },
            { "top_f_83", new ItemData(false, 295, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_f_84", new ItemData(false, 319, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3) },
            { "top_f_85", new ItemData(false, 321, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0) },
            { "top_f_86", new ItemData(false, 324, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14) },
            { "top_f_87", new ItemData(false, 338, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14) },
            { "top_f_88", new ItemData(false, 337, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 14) },
            { "top_f_89", new ItemData(false, 335, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14) },
            { "top_f_90", new ItemData(false, 273, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_91", new ItemData(false, 284, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15) },
            { "top_f_92", new ItemData(false, 264, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_93", new ItemData(false, 21, new int[] { 0, 1, 2, 3, 4, 5 }, 16) },
            { "top_f_94", new ItemData(false, 74, new int[] { 0, 1, 2 }, 15) },
            { "top_f_95", new ItemData(false, 77, new int[] { 0 }, 6) },
            { "top_f_97", new ItemData(false, 112, new int[] { 0, 1, 2 }, 11) },
            { "top_f_98", new ItemData(false, 113, new int[] { 0, 1, 2 }, 11) },
            { "top_f_99", new ItemData(false, 114, new int[] { 0, 1, 2 }, 11) },
            { "top_f_100", new ItemData(false, 126, new int[] { 0, 1, 2 }, 14) },
            { "top_f_101", new ItemData(false, 131, new int[] { 0, 1, 2 }, 3) },
            { "top_f_102", new ItemData(false, 128, new int[] { 0 }, 14) },
            { "top_f_103", new ItemData(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_104", new ItemData(false, 148, new int[] { 0, 1, 2, 3, 4, 5 }, 3) },
            { "top_f_105", new ItemData(false, 155, new int[] { 0, 1, 2 }, 130) },
            { "top_f_106", new ItemData(false, 156, new int[] { 0, 1 }, 131) },
            { "top_f_107", new ItemData(false, 165, new int[] { 0, 1, 2 }, 0) },
            { "top_f_108", new ItemData(false, 168, new int[] { 0, 1, 2, 3, 4, 5 }, 161) },
            { "top_f_109", new ItemData(false, 169, new int[] { 0, 1, 2, 3, 4, 5 }, 153) },
            { "top_f_110", new ItemData(false, 170, new int[] { 0, 1, 2, 3, 4, 5 }, 15) },
            { "top_f_111", new ItemData(false, 172, new int[] { 0, 1 }, 3) },
            { "top_f_112", new ItemData(false, 178, new int[] { 0 }, 131) },
            { "top_f_113", new ItemData(false, 190, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 3) },
            { "top_f_114", new ItemData(false, 189, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 7) },
            { "top_f_115", new ItemData(false, 207, new int[] { 0, 1, 2, 3, 4 }, 131) },
            { "top_f_117", new ItemData(false, 227, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 3) },
            { "top_f_118", new ItemData(false, 235, new int[] { 0, 1 }, 9) },
            { "top_f_119", new ItemData(false, 244, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9) },
            { "top_f_120", new ItemData(false, 246, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14) },
            { "top_f_121", new ItemData(false, 247, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4) },
            { "top_f_122", new ItemData(false, 249, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_f_123", new ItemData(false, 347, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_124", new ItemData(false, 349, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14) },
            { "top_f_125", new ItemData(false, 350, new int[] { 0, 1, 2, 3, 4, 5 }, 9) },
            { "top_f_126", new ItemData(false, 354, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 5) },
            { "top_f_127", new ItemData(false, 356, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3) },
            { "top_f_128", new ItemData(false, 361, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 3) },
            { "top_f_129", new ItemData(false, 363, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 5) },
            { "top_f_130", new ItemData(false, 344, new int[] { 0 }, 3) },
            { "top_f_131", new ItemData(false, 345, new int[] { 0 }, 3) },
            { "top_f_132", new ItemData(false, 368, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14) },
            { "top_f_133", new ItemData(false, 369, new int[] { 0, 1, 2, 3, 4 }, 14) },
            { "top_f_134", new ItemData(false, 370, new int[] { 0, 1, 2 }, 3) },
            { "top_f_135", new ItemData(false, 377, new int[] { 0, 1, 2, 3, 4, 5 }, 14) },
            { "top_f_137", new ItemData(false, 400, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14) },
            { "top_f_138", new ItemData(false, 407, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_139", new ItemData(false, 406, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 7) },
            { "top_f_140", new ItemData(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_141", new ItemData(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6) },
            { "top_f_142", new ItemData(false, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6) },
            { "top_f_143", new ItemData(false, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15) },
            { "top_f_144", new ItemData(false, 279, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 15) },
            { "top_f_145", new ItemData(false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_146", new ItemData(false, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0) },
            { "top_f_147", new ItemData(false, 35, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_148", new ItemData(false, 39, new int[] { 0 }, 5) },
            { "top_f_149", new ItemData(false, 51, new int[] { 0 }, 3) },
            { "top_f_150", new ItemData(false, 52, new int[] { 0, 1, 2, 3 }, 3) },
            { "top_f_151", new ItemData(false, 64, new int[] { 0, 1, 2, 3, 4 }, 5) },
            { "top_f_152", new ItemData(false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_153", new ItemData(false, 66, new int[] { 0, 1, 2, 3 }, 5) },
            { "top_f_154", new ItemData(false, 70, new int[] { 0, 1, 2, 3, 4 }, 5) },
            { "top_f_155", new ItemData(false, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 3) },
            { "top_f_156", new ItemData(false, 83, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 0) },
            { "top_f_157", new ItemData(false, 90, new int[] { 0, 1, 2, 3, 4 }, 3) },
            { "top_f_158", new ItemData(false, 99, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 7) },
            { "top_f_159", new ItemData(false, 173, new int[] { 0 }, 4) },
            { "top_f_160", new ItemData(false, 101, new int[] { 0, 1, 2, 3, 4, 5 }, 15) },
            { "top_f_161", new ItemData(false, 185, new int[] { 0, 1, 2, 3, 4, 5 }, 3) },
            { "top_f_162", new ItemData(false, 193, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5) },
            { "top_f_163", new ItemData(false, 194, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 6) },
            { "top_f_164", new ItemData(false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 9) },
            { "top_f_165", new ItemData(false, 34, new int[] { 0 }, 6) },
            { "top_f_166", new ItemData(false, 52, new int[] { 0, 1, 2, 3 }, 3) },
            { "top_f_167", new ItemData(false, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 3) },
            { "top_f_168", new ItemData(false, 102, new int[] { 0 }, 3) },
            { "top_f_169", new ItemData(false, 137, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 7) },
            { "top_f_170", new ItemData(false, 139, new int[] { 0, 1, 2 }, 6) },
            { "top_f_171", new ItemData(false, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 0) },
            { "top_f_172", new ItemData(false, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 7) },
            { "top_f_173", new ItemData(false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_174", new ItemData(false, 305, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3) },
            { "top_f_175", new ItemData(false, 307, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_176", new ItemData(false, 318, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 1) },
            { "top_f_177", new ItemData(false, 353, new int[] { 0, 1, 2, 3, 4, 5 }, 3) },
            { "top_f_178", new ItemData(false, 366, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 3) },
            { "top_f_179", new ItemData(false, 278, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5) },
            { "top_f_180", new ItemData(false, 274, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 3) },
            { "top_f_181", new ItemData(false, 269, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9) },
            { "top_f_182", new ItemData(false, 309, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 3) },
            { "top_f_183", new ItemData(false, 310, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9) },
            { "top_f_184", new ItemData(false, 311, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_185", new ItemData(false, 314, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5) },
            { "top_f_186", new ItemData(false, 315, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 5) },
            { "top_f_187", new ItemData(false, 316, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3) },
            { "top_f_188", new ItemData(false, 320, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 5) },
            { "top_f_189", new ItemData(false, 332, new int[] { 0 }, 3) },
            { "top_f_191", new ItemData(false, 322, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11) },
            { "top_f_192", new ItemData(false, 323, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11) },
            { "top_f_193", new ItemData(false, 339, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3) },
            { "top_f_194", new ItemData(false, 373, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 229) },
            { "top_f_195", new ItemData(false, 376, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1) },
            { "top_f_196", new ItemData(false, 379, new int[] { 0 }, 5) },
            { "top_f_197", new ItemData(false, 380, new int[] { 0 }, 3) },
            { "top_f_198", new ItemData(false, 388, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 11) },
            { "top_f_199", new ItemData(false, 389, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6) },
            { "top_f_200", new ItemData(false, 390, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 3) },
            { "top_f_201", new ItemData(false, 392, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 3) },
            { "top_f_202", new ItemData(false, 394, new int[] { 0, 1, 2 }, 3) },
            { "top_f_203", new ItemData(false, 395, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14) },
            { "top_f_204", new ItemData(false, 396, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1) },
            { "top_f_205", new ItemData(false, 399, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5) },
            { "top_f_206", new ItemData(false, 403, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 5) },
            { "top_f_207", new ItemData(false, 411, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5) },
            { "top_f_208", new ItemData(false, 412, new int[] { 0, 1, 2 }, 5) },
            { "top_f_209", new ItemData(false, 404, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 12) },
            { "top_f_210", new ItemData(false, 405, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 12) },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.ClothesTypes.Top;

        [JsonIgnore]
        /// <summary>Переключено ли состояние</summary>
        public bool Toggled { get; set; }

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            if (Toggled)
            {
                player.SetClothes(Slot, data.ExtraData.Drawable, variation);
                player.SetClothes(Gloves.Slot, data.ExtraData.BestTorso, 0);
            }
            else
            {
                player.SetClothes(Slot, data.Drawable, variation);
                player.SetClothes(Gloves.Slot, data.BestTorso, 0);
            }

            if (pData.Armour != null)
            {
                var aData = pData.Armour.Data;

                player.SetClothes(9, aData.Drawables[pData.Sex].DrawableTop, aData.Texture);
            }

            if (pData.Clothes[2] != null)
                pData.Clothes[2].Wear(pData);
            else
                pData.Accessories[7]?.Wear(pData);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Top, pData.Sex), 0);
            player.SetClothes(Gloves.Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Gloves, pData.Sex), 0);

            if (pData.Armour != null)
            {
                var aData = pData.Armour.Data;

                player.SetClothes(9, aData.Drawables[pData.Sex].Drawable, aData.Texture);
            }

            if (pData.Clothes[2] != null)
                pData.Clothes[2].Wear(pData);
            else
                pData.Accessories[7]?.Wear(pData);
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Top(string ID, int Variation) : base(ID, IDList[ID], typeof(Top), Variation)
        {

        }
    }

    public class Under : Clothes, Clothes.IToggleable
    {
        new public class ItemData : Clothes.ItemData
        {
            public Top.ItemData BestTop { get; set; }

            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public ItemData(bool Sex, int Drawable, int[] Textures, Top.ItemData BestTop, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : base(0.2f, "prop_ld_tshirt_02", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTop = BestTop;
                this.ExtraData = ExtraData;

                this.BestTorso = BestTorso;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region Unders Male
            { "under_m_0", new ItemData(true, 0, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 }, new Top.ItemData(true, 0, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 }, 0), 4, new ItemData.ExtraData(2, 4)) },
            { "under_m_1", new ItemData(true, 5, new int[] { 0, 1, 2, 7 }, new Top.ItemData(true, 5, new int[] { 0, 1, 2, 7 }, 5), 6) },
            { "under_m_2", new ItemData(true, 1, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 11, 12, 14 }, new Top.ItemData(true, 1, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 11, 12, 14 }, 0), 1, new ItemData.ExtraData(14, 1)) },
            { "under_m_3", new ItemData(true, 8, new int[] { 0, 10, 13, 14 }, new Top.ItemData(true, 8, new int[] { 0, 10, 13, 14 }, 11), 4) },
            { "under_m_4", new ItemData(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0), 1) },
            { "under_m_5", new ItemData(true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 1), 1, new ItemData.ExtraData(64, 1)) },
            { "under_m_6", new ItemData(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 4, new ItemData.ExtraData(30, 4)) },
            { "under_m_7", new ItemData(true, 17, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(true, 17, new int[] { 0, 1, 2, 3, 4, 5 }, 5), 6) },
            { "under_m_8", new ItemData(true, 41, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(true, 38, new int[] { 0, 1, 2, 3, 4 }, 8), 4) },
            { "under_m_9", new ItemData(true, 42, new int[] { 0, 1 }, new Top.ItemData(true, 39, new int[] { 0, 1 }, 0), 1) },
            { "under_m_10", new ItemData(true, 43, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 41, new int[] { 0, 1, 2, 3 }, 12), 1) },
            { "under_m_11", new ItemData(true, 45, new int[] { 0 }, new Top.ItemData(true, 42, new int[] { 0 }, 11, new ItemData.ExtraData(43, 11)), 4, new ItemData.ExtraData(46, 4)) },
            { "under_m_12", new ItemData(true, 53, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 47, new int[] { 0, 1, 2, 3 }, 0), 4, new ItemData.ExtraData(54, 4)) },
            { "under_m_13", new ItemData(true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 11, new ItemData.ExtraData(7, 11)) },
            { "under_m_14", new ItemData(true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Top.ItemData(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 11), 4) },
            { "under_m_15", new ItemData(true, 67, new int[] { 0 }, new Top.ItemData(true, 71, new int[] { 0 }, 0), 4) },
            { "under_m_16", new ItemData(true, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, new Top.ItemData(true, 73, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, 0), 4) },
            { "under_m_17", new ItemData(true, 4, new int[] { 0, 1, 2 }, null, 4, new ItemData.ExtraData(3, 4)) }, // -
            { "under_m_18", new ItemData(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 4, new ItemData.ExtraData(11, 4)) }, // - 
            { "under_m_19", new ItemData(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null, 4, new ItemData.ExtraData(25, 4)) }, // -
            { "under_m_20", new ItemData(true, 33, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, 4, new ItemData.ExtraData(34, 4)) }, // -
            { "under_m_21", new ItemData(true, 52, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, null, 4, new ItemData.ExtraData(51, 4)) },
            { "under_m_22", new ItemData(true, 69, new int[] { 0, 1, 2, 3, 4 }, null, 14) }, // -
            { "under_m_24", new ItemData(true, 22, new int[] { 0, 1, 2, 3, 4 }, null, 4) }, // -
            { "under_m_25", new ItemData(true, 93, new int[] { 0, 1 }, null, 11) }, // -
            { "under_m_26", new ItemData(true, 158, new int[] { 0 }, new Top.ItemData(true, 322, new int[] { 0 }, 1, new ItemData.ExtraData(321, 1)), 4, new ItemData.ExtraData(157, 4)) },

            { "under_m_27", new ItemData(true, 79, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 152, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 4, new ItemData.ExtraData(80, 4)) },
            { "under_m_28", new ItemData(true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(true, 235, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0), 1, new ItemData.ExtraData(110, 1)) },
            { "under_m_29", new ItemData(true, 187, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(true, 392, new int[] { 0, 1, 2, 3, 4 }, 0), 4, new ItemData.ExtraData(188, 4)) },
            { "under_m_30", new ItemData(true, 168, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new Top.ItemData(true, 345, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0), 4, new ItemData.ExtraData(169, 4)) },
            { "under_m_31", new ItemData(true, 47, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 44, new int[] { 0, 1, 2, 3 }, 0), 4, new ItemData.ExtraData(48, 4)) },
            { "under_m_32", new ItemData(true, 16, new int[] { 0, 1, 2 }, new Top.ItemData(true, 16, new int[] { 0, 1, 2 }, 0), 1, new ItemData.ExtraData(18, 1)) },
            { "under_m_33", new ItemData(true, 72, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(true, 139, new int[] { 0, 1, 2, 3, 4, 5 }, 4), 4, new ItemData.ExtraData(71, 4)) },
            { "under_m_34", new ItemData(true, 76, new int[] { 0, 1, 2, 3, 4, 5, 7, 8 }, new Top.ItemData(true, 146, new int[] { 0, 1, 2, 3, 4, 5, 7, 8 }, 0), 4, new ItemData.ExtraData(77, 4)) },
            { "under_m_35", new ItemData(true, 178, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, 4, new ItemData.ExtraData(179, 4)) },
            #endregion

            #region Unders Female
            { "under_f_0", new ItemData(false, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, new Top.ItemData(false, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 12), 0) }, // 26
            { "under_f_1", new ItemData(false, 26, new int[] { 0, 1, 2 }, new Top.ItemData(false, 30, new int[] { 0, 1, 2 }, 2), 0) }, // 30
            { "under_f_2", new ItemData(false, 27, new int[] { 0, 1, 2 }, new Top.ItemData(false, 32, new int[] { 0, 1, 2 }, 4), 0) }, // 32
            { "under_f_3", new ItemData(false, 71, new int[] { 0, 1, 2 }, new Top.ItemData(false, 73, new int[] { 0, 1, 2 }, 14), 0) }, // 73
            { "under_f_4", new ItemData(false, 31, new int[] { 0, 1 }, new Top.ItemData(false, 40, new int[] { 0, 1 }, 2), 0) }, // 40
            { "under_f_5", new ItemData(false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(false, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 0) }, // 149
            { "under_f_6", new ItemData(false, 61, new int[] { 0, 1, 2, 3 }, new Top.ItemData(false, 75, new int[] { 0, 1, 2, 3 }, 9), 0) }, // 75
            { "under_f_7", new ItemData(false, 67, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 103, new int[] { 0, 1, 2, 3, 4, 5 }, 3), 0) }, // 103
            { "under_f_8", new ItemData(false, 78, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 141, new int[] { 0, 1, 2, 3, 4, 5 }, 14), 0) }, // 141
            { "under_f_9", new ItemData(false, 147, new int[] { 0 }, new Top.ItemData(false, 236, new int[] { 0 }, 14), 0) }, // 236
            { "under_f_10", new ItemData(false, 22, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 22, new int[] { 0, 1, 2, 3, 4 }, 4), 0) }, // 22
            { "under_f_11", new ItemData(false, 29, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 36, new int[] { 0, 1, 2, 3, 4 }, 11), 0) }, // 36
            { "under_f_12", new ItemData(false, 45, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, new Top.ItemData(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 14), 0) }, // 68
            { "under_f_13", new ItemData(false, 49, new int[] { 0 }, new Top.ItemData(false, 67, new int[] { 0 }, 2), 0) }, // 67
            { "under_f_14", new ItemData(false, 117, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, new Top.ItemData(false, 209, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 12), 0) }, // 209
            { "under_f_15", new ItemData(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4), 0) }, // 13
            { "under_f_16", new ItemData(false, 38, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 0) }, // -
            { "under_f_17", new ItemData(false, 40, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, 0) },
            { "under_f_18", new ItemData(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(false, 111, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4), 0) }, // 111
            { "under_f_19", new ItemData(false, 176, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(false, 283, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 12), 0) }, // 283

            { "under_f_20", new ItemData(false, 11, new int[] { 0, 1, 2, 10, 11, 15 }, new Top.ItemData(false, 11, new int[] { 0, 1, 2, 10, 11, 15 }, 11), 0) }, // 11
            { "under_f_21", new ItemData(false, 24, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 27, new int[] { 0, 1, 2, 3, 4, 5 }, 0), 0) }, // 27
            { "under_f_22", new ItemData(false, 30, new int[] { 0, 1, 2, 3 }, new Top.ItemData(false, 38, new int[] { 0, 1, 2, 3 }, 2), 0) }, // 38
            { "under_f_23", new ItemData(false, 227, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 413, new int[] { 0, 1, 2, 3, 4  }, 14), 0) }, // 413
            { "under_f_24", new ItemData(false, 217, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, 0) }, // -
            #endregion
        };

        [JsonIgnore]
        public const int Slot =  (int)Customization.ClothesTypes.Under;

        [JsonIgnore]
        /// <summary>Переключено ли состояние</summary>
        public bool Toggled { get; set; }

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            if (pData.Clothes[1] == null && data.BestTop != null)
            {
                if (Toggled && data.BestTop.ExtraData != null)
                {
                    player.SetClothes(Top.Slot, data.BestTop.ExtraData.Drawable, variation);
                    player.SetClothes(Gloves.Slot, data.BestTop.ExtraData.BestTorso, 0);
                }
                else
                {
                    player.SetClothes(Top.Slot, data.BestTop.Drawable, variation);
                    player.SetClothes(Gloves.Slot, data.BestTop.BestTorso, 0);
                }
            }
            else
            {
                if (Toggled && data.ExtraData != null)
                {
                    player.SetClothes(Slot, data.ExtraData.Drawable, variation);
                    player.SetClothes(Gloves.Slot, data.ExtraData.BestTorso, 0);
                }
                else
                {
                    player.SetClothes(Slot, data.Drawable, variation);
                    player.SetClothes(Gloves.Slot, data.BestTorso, 0);
                }
            }

            pData.Accessories[7]?.Wear(pData);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.Clothes[1] == null)
            {
                player.SetClothes(Top.Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Top, pData.Sex), 0);
                player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Top, pData.Sex), 0);
                player.SetClothes(Gloves.Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Gloves, pData.Sex), 0);

                pData.Accessories[7]?.Wear(pData);
            }
            else
            {
                player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Under, pData.Sex), 0);

                pData.Clothes[1].Wear(pData);
            }
        }

        public void Action(PlayerData pData)
        {
            if (Data.ExtraData == null)
                return;

            Toggled = !Toggled;

            Wear(pData);
        }

        public Under(string ID, int Variation) : base(ID, IDList[ID], typeof(Under), Variation)
        {

        }
    }

    public class Gloves : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public Dictionary<int, int> BestTorsos { get; set; }

            public ItemData(bool Sex, int Drawable, int[] Textures, Dictionary<int, int> BestTorsos, string SexAlternativeID = null) : base(0.1f, "prop_ld_tshirt_02", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTorsos = BestTorsos;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "gloves_m_0", new ItemData(true, 51, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 51 }, { 14, 50 }, { 12, 49 }, { 11, 48 }, { 8, 47 }, { 6, 46 }, { 5, 45 }, { 4, 44 }, { 2, 43 }, { 1, 42 }, { 0, 41 }, { 184, 187 }, { 112, 117 }, { 113, 124 }, { 114, 131 }
                }, "gloves_f_0")
            },
            { "gloves_m_1", new ItemData(true, 62, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 62 }, { 14, 61 }, { 12, 60 }, { 11, 59 }, { 8, 58 }, { 6, 57 }, { 5, 56 }, { 4, 55 }, { 2, 54 }, { 1, 53 }, { 0, 52 }, { 184, 188 }, { 112, 118 }, { 113, 125 }, { 114, 132 }
                }, "gloves_f_1")
            },
            { "gloves_m_2", new ItemData(true, 73, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 73 }, { 14, 72 }, { 12, 71 }, { 11, 70 }, { 8, 69 }, { 6, 68 }, { 5, 67 }, { 4, 66 }, { 2, 65 }, { 1, 64 }, { 0, 63 }, { 184, 189 }, { 112, 119 }, { 113, 126 }, { 114, 133 }
                }, "gloves_f_2")
            },
            { "gloves_m_3", new ItemData(true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Dictionary<int, int>()
                {
                    { 15, 109 }, { 14, 108 }, { 12, 107 }, { 11, 106 }, { 8, 105 }, { 6, 104 }, { 5, 103 }, { 4, 102 }, { 2, 101 }, { 1, 100 }, { 0, 99 }
                }, "gloves_f_3")
            },
            { "gloves_m_4", new ItemData(true, 95, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 95 }, { 14, 94 }, { 12, 93 }, { 11, 92 }, { 8, 91 }, { 6, 90 }, { 5, 89 }, { 4, 88 }, { 2, 87 }, { 1, 86 }, { 0, 85 }, { 184, 191 }, { 112, 121 }, { 113, 128 }, { 114, 135 }
                }, "gloves_f_4")
            },
            { "gloves_m_5", new ItemData(true, 29, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 29 }, { 14, 28 }, { 12, 27 }, { 11, 26 }, { 8, 25 }, { 6, 24 }, { 5, 23 }, { 4, 22 }, { 2, 21 }, { 1, 20 }, { 0, 19 }, { 184, 185 }, { 112, 115 }, { 113, 122 }, { 114, 129 }
                }, "gloves_f_5")
            },
            { "gloves_m_6", new ItemData(true, 40, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 40 }, { 14, 39 }, { 12, 38 }, { 11, 37 }, { 8, 36 }, { 6, 35 }, { 5, 34 }, { 4, 33 }, { 2, 32 }, { 1, 31 }, { 0, 30 }, { 184, 186 }, { 112, 116 }, { 113, 123 }, { 114, 130 }
                }, "gloves_f_6")
            },
            { "gloves_m_7", new ItemData(true, 84, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 84 }, { 14, 83 }, { 12, 82 }, { 11, 81 }, { 8, 80 }, { 6, 79 }, { 5, 78 }, { 4, 77 }, { 2, 76 }, { 1, 75 }, { 0, 74 }, { 184, 190 }, { 112, 120 }, { 113, 127 }, { 114, 134 }
                }, "gloves_f_7")
            },
            { "gloves_m_8", new ItemData(true, 170, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new Dictionary<int, int>()
                {
                    { 15, 170 }, { 14, 180 }, { 12, 179 }, { 11, 178 }, { 8, 177 }, { 6, 176 }, { 5, 175 }, { 4, 174 }, { 2, 173 }, { 1, 172 }, { 0, 171 }, { 184, 194 }, { 112, 181 }, { 113, 182 }, { 114, 183 }
                }, "gloves_f_8")
            },
            #endregion

            #region ItemData Female
            { "gloves_f_0", new ItemData(false, 58, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 58 }, { 14, 57 }, { 12, 56 }, { 11, 55 }, { 9, 54 }, { 7, 53 }, { 6, 52 }, { 5, 51 }, { 4, 50 }, { 3, 49 }, { 2, 48 }, { 1, 47 }, { 0, 46 }, { 129, 134 }, { 130, 141 }, { 131, 148 }, { 153, 156 }, { 161, 164 }, { 229, 232 }
                }, "gloves_m_0")
            },
            { "gloves_f_1", new ItemData(false, 71, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 71 }, { 14, 70 }, { 12, 69 }, { 11, 68 }, { 9, 67 }, { 7, 66 }, { 6, 65 }, { 5, 64 }, { 4, 63 }, { 3, 62 }, { 2, 61 }, { 1, 60 }, { 0, 59 }, { 129, 135 }, { 130, 142 }, { 131, 149 }, { 153, 157 }, { 161, 165 }, { 229, 233 }
                }, "gloves_m_1")
            },
            { "gloves_f_2", new ItemData(false, 84, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 84 }, { 14, 83 }, { 12, 82 }, { 11, 81 }, { 9, 80 }, { 7, 79 }, { 6, 78 }, { 5, 77 }, { 4, 76 }, { 3, 75 }, { 2, 74 }, { 1, 73 }, { 0, 72 }, { 129, 136 }, { 130, 143 }, { 131, 150 }, { 153, 158 }, { 161, 166 }, { 229, 234 }
                }, "gloves_m_2")
            },
            { "gloves_f_3", new ItemData(false, 126, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Dictionary<int, int>()
                {
                    { 15, 126 }, { 14, 125 }, { 12, 124 }, { 11, 123 }, { 9, 122 }, { 7, 121 }, { 6, 120 }, { 5, 119 }, { 4, 118 }, { 3, 117 }, { 2, 116 }, { 1, 115 }, { 0, 114 }
                }, "gloves_m_3")
            },
            { "gloves_f_4", new ItemData(false, 110, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 110 }, { 14, 109 }, { 12, 108 }, { 11, 107 }, { 9, 106 }, { 7, 105 }, { 6, 104 }, { 5, 103 }, { 4, 102 }, { 3, 101 }, { 2, 100 }, { 1, 99 }, { 0, 98 }, { 129, 138 }, { 130, 145 }, { 131, 152 }, { 153, 160 }, { 161, 168 }, { 229, 236 }
                }, "gloves_m_4")
            },
            { "gloves_f_5", new ItemData(false, 32, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 32 }, { 14, 31 }, { 12, 30 }, { 11, 29 }, { 9, 28 }, { 7, 27 }, { 6, 26 }, { 5, 25 }, { 4, 24 }, { 3, 23 }, { 2, 22 }, { 1, 21 }, { 0, 20 }, { 129, 132 }, { 130, 139 }, { 131, 146 }, { 153, 154 }, { 161, 162 }, { 229, 230 }
                }, "gloves_m_5")
            },
            { "gloves_f_6", new ItemData(false, 45, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 45 }, { 14, 44 }, { 12, 43 }, { 11, 42 }, { 9, 41 }, { 7, 40 }, { 6, 39 }, { 5, 38 }, { 4, 37 }, { 3, 36 }, { 2, 35 }, { 1, 34 }, { 0, 33 }, { 129, 133 }, { 130, 140 }, { 131, 147 }, { 153, 155 }, { 161, 163 }, { 229, 231 }
                }, "gloves_m_6")
            },
            { "gloves_f_7", new ItemData(false, 97, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 97 }, { 14, 96 }, { 12, 95 }, { 11, 94 }, { 9, 93 }, { 7, 92 }, { 6, 91 }, { 5, 90 }, { 4, 89 }, { 3, 88 }, { 2, 87 }, { 1, 86 }, { 0, 85 }, { 129, 137 }, { 130, 144 }, { 131, 151 }, { 153, 159 }, { 161, 167 }, { 229, 235 }
                }, "gloves_m_7")
            },
            { "gloves_f_8", new ItemData(false, 211, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new Dictionary<int, int>()
                {
                    { 15, 211 }, { 14, 223 }, { 12, 222 }, { 11, 221 }, { 9, 220 }, { 7, 219 }, { 6, 218 }, { 5, 217 }, { 4, 216 }, { 3, 215 }, { 2, 214 }, { 1, 213 }, { 0, 212 }, { 129, 224 }, { 130, 225 }, { 131, 226 }, { 153, 227 }, { 161, 228 }, { 229, 239 }
                }, "gloves_m_8")
            },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.ClothesTypes.Gloves;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            int curTorso = player.GetClothesDrawable(3);

            int bestTorso;

            if (data.BestTorsos.TryGetValue(curTorso, out bestTorso))
                player.SetClothes(3, bestTorso, variation);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (player.GetClothesDrawable(Top.Slot) == Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Top, pData.Sex))
                player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Gloves, pData.Sex), 0);

            if (pData.Clothes[1] != null)
                pData.Clothes[1].Wear(pData);
            else
                pData.Clothes[2]?.Wear(pData);
        }

        public Gloves(string ID, int Variation) : base(ID, IDList[ID], typeof(Gloves), Variation)
        {

        }
    }

    public class Pants : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public ItemData(bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(0.4f, "p_laz_j02_s", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "pants_m_0", new ItemData(true, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_1", new ItemData(true, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_2", new ItemData(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_3", new ItemData(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_4", new ItemData(true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_5", new ItemData(true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_6", new ItemData(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_7", new ItemData(true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_8", new ItemData(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_9", new ItemData(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_10", new ItemData(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_11", new ItemData(true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_12", new ItemData(true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_13", new ItemData(true, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_m_14", new ItemData(true, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_15", new ItemData(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_16", new ItemData(true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_17", new ItemData(true, 29, new int[] { 0, 1, 2 }) },
            { "pants_m_18", new ItemData(true, 32, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_19", new ItemData(true, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_20", new ItemData(true, 43, new int[] { 0, 1 }) },
            { "pants_m_21", new ItemData(true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_m_22", new ItemData(true, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_23", new ItemData(true, 62, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_24", new ItemData(true, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_25", new ItemData(true, 69, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "pants_m_26", new ItemData(true, 71, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_m_27", new ItemData(true, 73, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_m_28", new ItemData(true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_29", new ItemData(true, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_30", new ItemData(true, 83, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_31", new ItemData(true, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_32", new ItemData(true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_33", new ItemData(true, 90, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_34", new ItemData(true, 98, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_35", new ItemData(true, 100, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "pants_m_36", new ItemData(true, 117, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_m_37", new ItemData(true, 55, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_38", new ItemData(true, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_39", new ItemData(true, 59, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_40", new ItemData(true, 63, new int[] { 0 }) },
            { "pants_m_41", new ItemData(true, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_m_42", new ItemData(true, 81, new int[] { 0, 1, 2 }) },
            { "pants_m_43", new ItemData(true, 83, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_44", new ItemData(true, 132, new int[] { 0, 1, 2 }) },
            { "pants_m_45", new ItemData(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_46", new ItemData(true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_47", new ItemData(true, 20, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_48", new ItemData(true, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_m_49", new ItemData(true, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_m_50", new ItemData(true, 24, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "pants_m_51", new ItemData(true, 25, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "pants_m_52", new ItemData(true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_m_53", new ItemData(true, 35, new int[] { 0 }) },
            { "pants_m_54", new ItemData(true, 37, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_55", new ItemData(true, 44, new int[] { 0 }) },
            { "pants_m_56", new ItemData(true, 49, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_m_57", new ItemData(true, 54, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "pants_m_58", new ItemData(true, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_m_59", new ItemData(true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "pants_m_60", new ItemData(true, 103, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "pants_m_61", new ItemData(true, 116, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_m_62", new ItemData(true, 19, new int[] { 0, 1 }) },
            { "pants_m_63", new ItemData(true, 48, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_m_64", new ItemData(true, 51, new int[] { 0 }) },
            { "pants_m_65", new ItemData(true, 52, new int[] { 0, 1, 2, 3 }) },
            { "pants_m_66", new ItemData(true, 53, new int[] { 0 }) },
            { "pants_m_67", new ItemData(true, 118, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_68", new ItemData(true, 119, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_m_69", new ItemData(true, 131, new int[] { 0 }) },
            { "pants_m_70", new ItemData(true, 138, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_71", new ItemData(true, 139, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_72", new ItemData(true, 140, new int[] { 0, 1, 2 }) },
            { "pants_m_73", new ItemData(true, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_m_74", new ItemData(true, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }) },
            { "pants_m_75", new ItemData(true, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            #endregion

            #region ItemData Female
            { "pants_f_0", new ItemData(false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_1", new ItemData(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_2", new ItemData(false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_3", new ItemData(false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_4", new ItemData(false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_5", new ItemData(false, 18, new int[] { 0, 1 }) },
            { "pants_f_6", new ItemData(false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_7", new ItemData(false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_8", new ItemData(false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_9", new ItemData(false, 22, new int[] { 0, 1, 2 }) },
            { "pants_f_10", new ItemData(false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_11", new ItemData(false, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_12", new ItemData(false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_13", new ItemData(false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_14", new ItemData(false, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_15", new ItemData(false, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_f_16", new ItemData(false, 66, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_f_17", new ItemData(false, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_18", new ItemData(false, 67, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "pants_f_19", new ItemData(false, 43, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_20", new ItemData(false, 71, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "pants_f_21", new ItemData(false, 73, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_f_22", new ItemData(false, 74, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_f_23", new ItemData(false, 84, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_f_24", new ItemData(false, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_25", new ItemData(false, 102, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }) },
            { "pants_f_26", new ItemData(false, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "pants_f_27", new ItemData(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_f_28", new ItemData(false, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_f_29", new ItemData(false, 81, new int[] { 0, 1, 2 }) },
            { "pants_f_30", new ItemData(false, 92, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_31", new ItemData(false, 20, new int[] { 0, 1, 2 }) },
            { "pants_f_32", new ItemData(false, 26, new int[] { 0 }) },
            { "pants_f_33", new ItemData(false, 28, new int[] { 0 }) },
            { "pants_f_34", new ItemData(false, 30, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_35", new ItemData(false, 36, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_36", new ItemData(false, 44, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_37", new ItemData(false, 45, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_38", new ItemData(false, 52, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_39", new ItemData(false, 54, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_40", new ItemData(false, 58, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_41", new ItemData(false, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_f_42", new ItemData(false, 64, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_43", new ItemData(false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_f_44", new ItemData(false, 83, new int[] { 0, 1, 2 }) },
            { "pants_f_45", new ItemData(false, 85, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_46", new ItemData(false, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "pants_f_47", new ItemData(false, 106, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "pants_f_48", new ItemData(false, 139, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_49", new ItemData(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_50", new ItemData(false, 19, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_51", new ItemData(false, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_f_52", new ItemData(false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_f_53", new ItemData(false, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }) },
            { "pants_f_54", new ItemData(false, 51, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_55", new ItemData(false, 50, new int[] { 0, 1, 2, 3, 4 }) },
            { "pants_f_56", new ItemData(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_57", new ItemData(false, 56, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "pants_f_58", new ItemData(false, 62, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_59", new ItemData(false, 37, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "pants_f_60", new ItemData(false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_61", new ItemData(false, 75, new int[] { 0, 1, 2 }) },
            { "pants_f_62", new ItemData(false, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_63", new ItemData(false, 78, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_64", new ItemData(false, 104, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "pants_f_65", new ItemData(false, 112, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_66", new ItemData(false, 124, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_67", new ItemData(false, 125, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_f_68", new ItemData(false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "pants_f_69", new ItemData(false, 34, new int[] { 0 }) },
            { "pants_f_70", new ItemData(false, 41, new int[] { 0, 1, 2, 3 }) },
            { "pants_f_71", new ItemData(false, 49, new int[] { 0, 1 }) },
            { "pants_f_72", new ItemData(false, 76, new int[] { 0, 1, 2 }) },
            { "pants_f_73", new ItemData(false, 77, new int[] { 0, 1, 2 }) },
            { "pants_f_74", new ItemData(false, 107, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "pants_f_75", new ItemData(false, 123, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "pants_f_76", new ItemData(false, 53, new int[] { 0 }) },
            { "pants_f_77", new ItemData(false, 55, new int[] { 0 }) },
            { "pants_f_78", new ItemData(false, 138, new int[] { 0 }) },
            { "pants_f_79", new ItemData(false, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_80", new ItemData(false, 146, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_81", new ItemData(false, 147, new int[] { 0, 1, 2 }) },
            { "pants_f_82", new ItemData(false, 148, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "pants_f_83", new ItemData(false, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }) },
            { "pants_f_84", new ItemData(false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.ClothesTypes.Pants;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            player.SetClothes(4, data.Drawable, variation);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Pants, pData.Sex), 0);
        }

        public Pants(string ID, int Variation) : base(ID, IDList[ID], typeof(Pants), Variation)
        {

        }
    }

    public class Shoes : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public ItemData(bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(0.3f, "prop_ld_shoe_01", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "shoes_m_0", new ItemData(true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_1", new ItemData(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_2", new ItemData(true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_3", new ItemData(true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_4", new ItemData(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_5", new ItemData(true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_6", new ItemData(true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_7", new ItemData(true, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_8", new ItemData(true, 24, new int[] { 0 }) },
            { "shoes_m_9", new ItemData(true, 25, new int[] { 0 }) },
            { "shoes_m_10", new ItemData(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_11", new ItemData(true, 28, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_m_12", new ItemData(true, 31, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_m_13", new ItemData(true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_14", new ItemData(true, 44, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_m_15", new ItemData(true, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "shoes_m_16", new ItemData(true, 46, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "shoes_m_17", new ItemData(true, 50, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_m_18", new ItemData(true, 53, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_m_19", new ItemData(true, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_20", new ItemData(true, 58, new int[] { 0, 1, 2 }) },
            { "shoes_m_21", new ItemData(true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "shoes_m_22", new ItemData(true, 70, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_23", new ItemData(true, 79, new int[] { 0, 1 }) },
            { "shoes_m_24", new ItemData(true, 80, new int[] { 0, 1 }) },
            { "shoes_m_25", new ItemData(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_26", new ItemData(true, 27, new int[] { 0 }) },
            { "shoes_m_27", new ItemData(true, 85, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_28", new ItemData(true, 86, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_29", new ItemData(true, 83, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }) },
            { "shoes_m_30", new ItemData(true, 37, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_m_31", new ItemData(true, 38, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_m_32", new ItemData(true, 43, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_33", new ItemData(true, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_34", new ItemData(true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_35", new ItemData(true, 62, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_36", new ItemData(true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_m_37", new ItemData(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_38", new ItemData(true, 35, new int[] { 0, 1 }) },
            { "shoes_m_39", new ItemData(true, 52, new int[] { 0, 1 }) },
            { "shoes_m_40", new ItemData(true, 65, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "shoes_m_41", new ItemData(true, 66, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "shoes_m_42", new ItemData(true, 72, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_43", new ItemData(true, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_44", new ItemData(true, 74, new int[] { 0, 1 }) },
            { "shoes_m_45", new ItemData(true, 81, new int[] { 0, 1, 2 }) },
            { "shoes_m_46", new ItemData(true, 82, new int[] { 0, 1, 2 }) },
            { "shoes_m_47", new ItemData(true, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_48", new ItemData(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_49", new ItemData(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_50", new ItemData(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_51", new ItemData(true, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_52", new ItemData(true, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_53", new ItemData(true, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_m_54", new ItemData(true, 36, new int[] { 0, 1, 2, 3 }) },
            { "shoes_m_55", new ItemData(true, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_m_56", new ItemData(true, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_57", new ItemData(true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_58", new ItemData(true, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }) },
            { "shoes_m_59", new ItemData(true, 77, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_60", new ItemData(true, 94, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }) },
            { "shoes_m_61", new ItemData(true, 30, new int[] { 0, 1 }) },
            { "shoes_m_62", new ItemData(true, 18, new int[] { 0, 1 }) },
            { "shoes_m_63", new ItemData(true, 40, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_m_64", new ItemData(true, 19, new int[] { 0 }) },
            { "shoes_m_65", new ItemData(true, 29, new int[] { 0 }) },
            { "shoes_m_66", new ItemData(true, 41, new int[] { 0 }) },
            { "shoes_m_67", new ItemData(true, 69, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_m_68", new ItemData(true, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "shoes_m_69", new ItemData(true, 95, new int[] { 0 }) },
            { "shoes_m_70", new ItemData(true, 99, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            #endregion

            #region ItemData Female
            { "shoes_f_0", new ItemData(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_1", new ItemData(false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_2", new ItemData(false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_3", new ItemData(false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_4", new ItemData(false, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_5", new ItemData(false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_6", new ItemData(false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_7", new ItemData(false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_8", new ItemData(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_9", new ItemData(false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_10", new ItemData(false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "shoes_f_11", new ItemData(false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_12", new ItemData(false, 30, new int[] { 0 }) },
            { "shoes_f_13", new ItemData(false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_14", new ItemData(false, 29, new int[] { 0, 1, 2 }) },
            { "shoes_f_15", new ItemData(false, 32, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_f_16", new ItemData(false, 44, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_17", new ItemData(false, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_18", new ItemData(false, 61, new int[] { 0, 1, 2 }) },
            { "shoes_f_19", new ItemData(false, 67, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }) },
            { "shoes_f_20", new ItemData(false, 77, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }) },
            { "shoes_f_21", new ItemData(false, 85, new int[] { 0, 1, 2 }) },
            { "shoes_f_22", new ItemData(false, 26, new int[] { 0 }) },
            { "shoes_f_23", new ItemData(false, 38, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_f_24", new ItemData(false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_25", new ItemData(false, 45, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_f_26", new ItemData(false, 39, new int[] { 0, 1, 2, 3, 4 }) },
            { "shoes_f_27", new ItemData(false, 46, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_f_28", new ItemData(false, 51, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_f_29", new ItemData(false, 52, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_f_30", new ItemData(false, 54, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_f_31", new ItemData(false, 55, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "shoes_f_32", new ItemData(false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_33", new ItemData(false, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_34", new ItemData(false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_35", new ItemData(false, 66, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_36", new ItemData(false, 83, new int[] { 0, 1 }) },
            { "shoes_f_37", new ItemData(false, 84, new int[] { 0, 1 }) },
            { "shoes_f_38", new ItemData(false, 27, new int[] { 0 }) },
            { "shoes_f_39", new ItemData(false, 28, new int[] { 0 }) },
            { "shoes_f_40", new ItemData(false, 53, new int[] { 0, 1 }) },
            { "shoes_f_41", new ItemData(false, 59, new int[] { 0, 1 }) },
            { "shoes_f_42", new ItemData(false, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }) },
            { "shoes_f_43", new ItemData(false, 89, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_44", new ItemData(false, 90, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_45", new ItemData(false, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_46", new ItemData(false, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_47", new ItemData(false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_48", new ItemData(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_49", new ItemData(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_50", new ItemData(false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_51", new ItemData(false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "shoes_f_52", new ItemData(false, 19, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_53", new ItemData(false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_54", new ItemData(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6 }) },
            { "shoes_f_55", new ItemData(false, 37, new int[] { 0, 1, 2, 3 }) },
            { "shoes_f_56", new ItemData(false, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_57", new ItemData(false, 31, new int[] { 0 }) },
            { "shoes_f_58", new ItemData(false, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "shoes_f_59", new ItemData(false, 79, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_60", new ItemData(false, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_61", new ItemData(false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_62", new ItemData(false, 96, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }) },
            { "shoes_f_63", new ItemData(false, 97, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }) },
            { "shoes_f_64", new ItemData(false, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_65", new ItemData(false, 74, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }) },
            { "shoes_f_66", new ItemData(false, 18, new int[] { 0, 1, 2 }) },
            { "shoes_f_67", new ItemData(false, 56, new int[] { 0, 1, 2 }) },
            { "shoes_f_68", new ItemData(false, 57, new int[] { 0, 1, 2 }) },
            { "shoes_f_69", new ItemData(false, 24, new int[] { 0 }) },
            { "shoes_f_70", new ItemData(false, 25, new int[] { 0 }) },
            { "shoes_f_71", new ItemData(false, 36, new int[] { 0, 1 }) },
            { "shoes_f_72", new ItemData(false, 78, new int[] { 0, 1 }) },
            { "shoes_f_73", new ItemData(false, 47, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "shoes_f_74", new ItemData(false, 91, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }) },
            { "shoes_f_75", new ItemData(false, 92, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "shoes_f_76", new ItemData(false, 98, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "shoes_f_77", new ItemData(false, 99, new int[] { 0 }) },
            { "shoes_f_78", new ItemData(false, 23, new int[] { 0, 1, 2 }) },
            { "shoes_f_79", new ItemData(false, 103, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }) },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.ClothesTypes.Shoes;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            player.SetClothes(Slot, data.Drawable, variation);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Shoes, pData.Sex), 0);
        }

        public Shoes(string ID, int Variation) : base(ID, IDList[ID], typeof(Shoes), Variation)
        {

        }
    }

    public class Accessory : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public ItemData(bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(0.2f, "p_jewel_necklace_02", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region Accessories Male
            { "accs_m_0", new ItemData(true, 16, new int[] { 0, 1, 2 }) },
            { "accs_m_1", new ItemData(true, 17, new int[] { 0, 1, 2 }) },
            { "accs_m_2", new ItemData(true, 30, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "accs_m_3", new ItemData(true, 44, new int[] { 0 }) },
            { "accs_m_4", new ItemData(true, 74, new int[] { 0, 1 }) },
            { "accs_m_5", new ItemData(true, 85, new int[] { 0, 1 }) },
            { "accs_m_6", new ItemData(true, 87, new int[] { 0, 1 }) },
            { "accs_m_7", new ItemData(true, 110, new int[] { 0, 1 }) },
            { "accs_m_8", new ItemData(true, 112, new int[] { 0, 1, 2 }) },
            { "accs_m_9", new ItemData(true, 114, new int[] { 0 }) },
            { "accs_m_10", new ItemData(true, 119, new int[] { 0, 1 }) },
            { "accs_m_11", new ItemData(true, 124, new int[] { 0, 1 }) },
            { "accs_m_12", new ItemData(true, 151, new int[] { 0 }) },
            { "accs_m_13", new ItemData(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_14", new ItemData(true, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_15", new ItemData(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_16", new ItemData(true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_17", new ItemData(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_18", new ItemData(true, 32, new int[] { 0, 1, 2 }) },
            { "accs_m_19", new ItemData(true, 37, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_20", new ItemData(true, 38, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_21", new ItemData(true, 39, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_m_22", new ItemData(true, 118, new int[] { 0 }) },
            #endregion

            #region Accessories Female
            { "accs_f_0", new ItemData(false, 83, new int[] { 0, 1, 2 }) },
            { "accs_f_1", new ItemData(false, 9, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "accs_f_2", new ItemData(false, 15, new int[] { 0, 1, 2, 3, 4 }) },
            { "accs_f_3", new ItemData(false, 85, new int[] { 0 }) },
            { "accs_f_4", new ItemData(false, 94, new int[] { 0, 1 }) },
            { "accs_f_5", new ItemData(false, 120, new int[] { 0 }) },
            { "accs_f_6", new ItemData(false, 12, new int[] { 0, 1, 2 }) },
            { "accs_f_7", new ItemData(false, 13, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "accs_f_8", new ItemData(false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_f_9", new ItemData(false, 21, new int[] { 0, 1, 2 }) },
            { "accs_f_10", new ItemData(false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }) },
            { "accs_f_11", new ItemData(false, 23, new int[] { 0, 1, 2 }) },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.ClothesTypes.Accessory;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            player.SetClothes(Slot, data.Drawable, variation);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, Game.Data.Customization.GetNudeDrawable(Game.Data.Customization.ClothesTypes.Accessory, pData.Sex), 0);
        }

        public Accessory(string ID, int Variation) : base(ID, IDList[ID], typeof(Accessory), Variation)
        {

        }
    }

    public class Glasses : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public ItemData(bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(0.2f, "prop_cs_sol_glasses", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "glasses_m_0", new ItemData(true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_1", new ItemData(true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_2", new ItemData(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_3", new ItemData(true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_4", new ItemData(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_5", new ItemData(true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_6", new ItemData(true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "glasses_m_7", new ItemData(true, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_8", new ItemData(true, 21, new int[] { 0 }) },
            { "glasses_m_9", new ItemData(true, 22, new int[] { 0 }) },
            { "glasses_m_10", new ItemData(true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_11", new ItemData(true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_12", new ItemData(true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_13", new ItemData(true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_14", new ItemData(true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_15", new ItemData(true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_16", new ItemData(true, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_17", new ItemData(true, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_m_18", new ItemData(true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_m_19", new ItemData(true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }) },
            { "glasses_m_20", new ItemData(true, 30, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_m_21", new ItemData(true, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_m_22", new ItemData(true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_m_23", new ItemData(true, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            #endregion

            #region ItemData Female
            { "glasses_f_0", new ItemData(false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_1", new ItemData(false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_2", new ItemData(false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_3", new ItemData(false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_4", new ItemData(false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_5", new ItemData(false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_6", new ItemData(false, 22, new int[] { 0 }) },
            { "glasses_f_7", new ItemData(false, 23, new int[] { 0 }) },
            { "glasses_f_8", new ItemData(false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_9", new ItemData(false, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_10", new ItemData(false, 19, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_11", new ItemData(false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_12", new ItemData(false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "glasses_f_13", new ItemData(false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "glasses_f_14", new ItemData(false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_15", new ItemData(false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "glasses_f_16", new ItemData(false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }) },
            { "glasses_f_17", new ItemData(false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_18", new ItemData(false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }) },
            { "glasses_f_19", new ItemData(false, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "glasses_f_20", new ItemData(false, 30, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_f_21", new ItemData(false, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }) },
            { "glasses_f_22", new ItemData(false, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_f_23", new ItemData(false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_f_24", new ItemData(false, 34, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            { "glasses_f_25", new ItemData(false, 35, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }) },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.AccessoryTypes.Glasses;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            player.SetAccessories(Slot, data.Drawable, variation);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(Slot);
        }

        public Glasses(string ID, int Variation) : base(ID, IDList[ID], typeof(Glasses), Variation)
        {

        }
    }

    public class Watches : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public ItemData(bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(0.1f, "prop_jewel_02b", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "watches_m_0", new ItemData(true, 1, new int[] { 0, 1, 2, 3, 4 }) },
            { "watches_m_1", new ItemData(true, 3, new int[] { 0, 1, 2, 3, 4 }) },
            { "watches_m_2", new ItemData(true, 5, new int[] { 0, 1, 2, 3 }) },
            { "watches_m_3", new ItemData(true, 7, new int[] { 0, 1, 2 }) },
            { "watches_m_4", new ItemData(true, 10, new int[] { 0, 1, 2 }) },
            { "watches_m_5", new ItemData(true, 12, new int[] { 0, 1, 2 }) },
            { "watches_m_6", new ItemData(true, 13, new int[] { 0, 1, 2 }) },
            { "watches_m_7", new ItemData(true, 14, new int[] { 0, 1, 2 }) },
            { "watches_m_8", new ItemData(true, 15, new int[] { 0, 1, 2 }) },
            { "watches_m_9", new ItemData(true, 20, new int[] { 0, 1, 2 }) },
            { "watches_m_10", new ItemData(true, 21, new int[] { 0, 1, 2 }) },
            { "watches_m_11", new ItemData(true, 36, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_m_12", new ItemData(true, 0, new int[] { 0, 1, 2, 3, 4 }) },
            { "watches_m_13", new ItemData(true, 6, new int[] { 0, 1, 2 }) },
            { "watches_m_14", new ItemData(true, 8, new int[] { 0, 1, 2 }) },
            { "watches_m_15", new ItemData(true, 9, new int[] { 0, 1, 2 }) },
            { "watches_m_16", new ItemData(true, 11, new int[] { 0, 1, 2 }) },
            { "watches_m_17", new ItemData(true, 16, new int[] { 0, 1, 2 }) },
            { "watches_m_18", new ItemData(true, 17, new int[] { 0, 1, 2 }) },
            { "watches_m_19", new ItemData(true, 18, new int[] { 0, 1, 2 }) },
            { "watches_m_20", new ItemData(true, 19, new int[] { 0, 1, 2 }) },
            { "watches_m_21", new ItemData(true, 30, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_m_22", new ItemData(true, 31, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_m_23", new ItemData(true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "watches_m_24", new ItemData(true, 34, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_m_25", new ItemData(true, 35, new int[] { 0, 1, 2, 3, 4, 5 }) },
            #endregion

            #region ItemData Female
            { "watches_f_0", new ItemData(false, 3, new int[] { 0, 1, 2 }) },
            { "watches_f_1", new ItemData(false, 4, new int[] { 0, 1, 2 }) },
            { "watches_f_2", new ItemData(false, 5, new int[] { 0, 1, 2 }) },
            { "watches_f_3", new ItemData(false, 6, new int[] { 0, 1, 2 }) },
            { "watches_f_4", new ItemData(false, 8, new int[] { 0, 1, 2 }) },
            { "watches_f_5", new ItemData(false, 24, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_6", new ItemData(false, 25, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_7", new ItemData(false, 26, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_8", new ItemData(false, 2, new int[] { 0, 1, 2, 3 }) },
            { "watches_f_9", new ItemData(false, 7, new int[] { 0, 1, 2 }) },
            { "watches_f_10", new ItemData(false, 9, new int[] { 0, 1, 2 }) },
            { "watches_f_11", new ItemData(false, 19, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_12", new ItemData(false, 20, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_13", new ItemData(false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }) },
            { "watches_f_14", new ItemData(false, 23, new int[] { 0, 1, 2, 3, 4, 5 }) },
            { "watches_f_15", new ItemData(false, 24, new int[] { 0, 1, 2, 3, 4, 5 }) },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.AccessoryTypes.Watches;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            player.SetAccessories(Slot, data.Drawable, variation);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(Slot);
        }

        public Watches(string ID, int Variation) : base(ID, IDList[ID], typeof(Watches), Variation)
        {

        }
    }

    public class Bracelet : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public ItemData(bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(0.1f, "prop_jewel_02b", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region Bracelets Male
            { "bracelet_m_0", new ItemData(true, 0, new int[] { 0 }, "bracelet_f_7") },
            { "bracelet_m_1", new ItemData(true, 1, new int[] { 0 }, "bracelet_f_8") },
            { "bracelet_m_2", new ItemData(true, 2, new int[] { 0 }, "bracelet_f_9") },
            { "bracelet_m_3", new ItemData(true, 3, new int[] { 0 }, "bracelet_f_10") },
            { "bracelet_m_4", new ItemData(true, 4, new int[] { 0 }, "bracelet_f_11") },
            { "bracelet_m_5", new ItemData(true, 5, new int[] { 0 }, "bracelet_f_12") },
            { "bracelet_m_6", new ItemData(true, 6, new int[] { 0 }, "bracelet_f_13") },
            { "bracelet_m_7", new ItemData(true, 7, new int[] { 0, 1, 2, 3 }, "bracelet_f_14") },
            { "bracelet_m_8", new ItemData(true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, "bracelet_f_15") },
            #endregion

            #region Bracelets Female
            { "bracelet_f_0", new ItemData(false, 0, new int[] { 0 }) },
            { "bracelet_f_1", new ItemData(false, 1, new int[] { 0 }) },
            { "bracelet_f_2", new ItemData(false, 2, new int[] { 0 }) },
            { "bracelet_f_3", new ItemData(false, 3, new int[] { 0 }) },
            { "bracelet_f_4", new ItemData(false, 4, new int[] { 0 }) },
            { "bracelet_f_5", new ItemData(false, 5, new int[] { 0 }) },
            { "bracelet_f_6", new ItemData(false, 6, new int[] { 0 }) },
            { "bracelet_f_7", new ItemData(false, 7, new int[] { 0 }, "bracelet_m_0") },
            { "bracelet_f_8", new ItemData(false, 8, new int[] { 0 }, "bracelet_m_1") },
            { "bracelet_f_9", new ItemData(false, 9, new int[] { 0 }, "bracelet_m_2") },
            { "bracelet_f_10", new ItemData(false, 10, new int[] { 0 }, "bracelet_m_3") },
            { "bracelet_f_11", new ItemData(false, 11, new int[] { 0 }, "bracelet_m_4") },
            { "bracelet_f_12", new ItemData(false, 12, new int[] { 0 }, "bracelet_m_5") },
            { "bracelet_f_13", new ItemData(false, 13, new int[] { 0 }, "bracelet_m_6") },
            { "bracelet_f_14", new ItemData(false, 14, new int[] { 0, 1, 2, 3 }, "bracelet_m_7") },
            { "bracelet_f_15", new ItemData(false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, "bracelet_m_8") },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.AccessoryTypes.Bracelet;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            player.SetAccessories(Slot, data.Drawable, variation);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(Slot);
        }

        public Bracelet(string ID, int Variation) : base(ID, IDList[ID], typeof(Bracelet), Variation)
        {

        }
    }

    public class Earrings : Clothes
    {
        new public class ItemData : Clothes.ItemData
        {
            public ItemData(bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(0.1f, "p_tmom_earrings_s", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "ears_m_0", new ItemData(true, 3, new int[] { 0, 1, 2 }) },
            { "ears_m_1", new ItemData(true, 4, new int[] { 0, 1, 2 }) },
            { "ears_m_2", new ItemData(true, 5, new int[] { 0, 1, 2 }) },
            { "ears_m_3", new ItemData(true, 6, new int[] { 0, 1 }) },
            { "ears_m_4", new ItemData(true, 7, new int[] { 0, 1 }) },
            { "ears_m_5", new ItemData(true, 8, new int[] { 0, 1 }) },
            { "ears_m_6", new ItemData(true, 9, new int[] { 0, 1, 2 }) },
            { "ears_m_7", new ItemData(true, 10, new int[] { 0, 1, 2 }) },
            { "ears_m_8", new ItemData(true, 11, new int[] { 0, 1, 2 }) },
            { "ears_m_9", new ItemData(true, 12, new int[] { 0, 1, 2 }) },
            { "ears_m_10", new ItemData(true, 13, new int[] { 0, 1, 2 }) },
            { "ears_m_11", new ItemData(true, 14, new int[] { 0, 1, 2 }) },
            { "ears_m_12", new ItemData(true, 15, new int[] { 0, 1, 2 }) },
            { "ears_m_13", new ItemData(true, 16, new int[] { 0, 1, 2 }) },
            { "ears_m_14", new ItemData(true, 17, new int[] { 0, 1, 2 }) },
            { "ears_m_15", new ItemData(true, 18, new int[] { 0, 1, 2, 3, 4 }) },
            { "ears_m_16", new ItemData(true, 19, new int[] { 0, 1, 2, 3, 4 }) },
            { "ears_m_17", new ItemData(true, 20, new int[] { 0, 1, 2, 3, 4 }) },
            { "ears_m_18", new ItemData(true, 21, new int[] { 0, 1 }) },
            { "ears_m_19", new ItemData(true, 22, new int[] { 0, 1 }) },
            { "ears_m_20", new ItemData(true, 23, new int[] { 0, 1 }) },
            { "ears_m_21", new ItemData(true, 24, new int[] { 0, 1, 2, 3 }) },
            { "ears_m_22", new ItemData(true, 25, new int[] { 0, 1, 2, 3 }) },
            { "ears_m_23", new ItemData(true, 26, new int[] { 0, 1, 2, 3 }) },
            { "ears_m_24", new ItemData(true, 27, new int[] { 0, 1 }) },
            { "ears_m_25", new ItemData(true, 28, new int[] { 0, 1 }) },
            { "ears_m_26", new ItemData(true, 29, new int[] { 0, 1 }) },
            { "ears_m_27", new ItemData(true, 30, new int[] { 0, 1, 2 }) },
            { "ears_m_28", new ItemData(true, 21, new int[] { 0, 1 }) },
            { "ears_m_29", new ItemData(true, 32, new int[] { 0, 1, 2 }) },
            { "ears_m_30", new ItemData(true, 33, new int[] { 0 }) },
            { "ears_m_31", new ItemData(true, 34, new int[] { 0, 1 }) },
            { "ears_m_32", new ItemData(true, 35, new int[] { 0, 1 }) },
            { "ears_m_33", new ItemData(true, 37, new int[] { 0, 1 }, "ears_f_14") },
            { "ears_m_34", new ItemData(true, 38, new int[] { 0, 1, 2, 3 }, "ears_f_15") },
            { "ears_m_35", new ItemData(true, 39, new int[] { 0, 1, 2, 3 }, "ears_f_16") },
            { "ears_m_36", new ItemData(true, 40, new int[] { 0, 1, 2, 3 }, "ears_f_17") },
            #endregion

            #region ItemData Female
            { "ears_f_0", new ItemData(false, 3, new int[] { 0 }) },
            { "ears_f_1", new ItemData(false, 4, new int[] { 0 }) },
            { "ears_f_2", new ItemData(false, 5, new int[] { 0 }) },
            { "ears_f_3", new ItemData(false, 6, new int[] { 0, 1, 2 }) },
            { "ears_f_4", new ItemData(false, 7, new int[] { 0, 1, 2 }) },
            { "ears_f_5", new ItemData(false, 8, new int[] { 0, 1, 2 }) },
            { "ears_f_6", new ItemData(false, 9, new int[] { 0, 1, 2 }) },
            { "ears_f_7", new ItemData(false, 10, new int[] { 0, 1, 2 }) },
            { "ears_f_8", new ItemData(false, 11, new int[] { 0, 1, 2 }) },
            { "ears_f_9", new ItemData(false, 12, new int[] { 0, 1, 2 }) },
            { "ears_f_10", new ItemData(false, 13, new int[] { 0 }) },
            { "ears_f_11", new ItemData(false, 14, new int[] { 0 }) },
            { "ears_f_12", new ItemData(false, 15, new int[] { 0 }) },
            { "ears_f_13", new ItemData(false, 16, new int[] { 0 }) },
            { "ears_f_14", new ItemData(false, 18, new int[] { 0, 1 }, "ears_m_33") },
            { "ears_f_15", new ItemData(false, 19, new int[] { 0, 1, 2, 3 }, "ears_m_34") },
            { "ears_f_16", new ItemData(false, 20, new int[] { 0, 1, 2, 3 }, "ears_m_35") },
            { "ears_f_17", new ItemData(false, 21, new int[] { 0, 1, 2, 3 }, "ears_m_36") },
            #endregion
        };

        [JsonIgnore]
        public const int Slot = (int)Customization.AccessoryTypes.Earrings;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            if (Data.Sex != pData.Sex)
                data = SexAlternativeData;

            if (data == null)
                return;

            var variation = Var < data.Textures.Length && Var >= 0 ? data.Textures[Var] : 0;

            player.SetAccessories(Slot, data.Drawable, variation);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.ClearAccessory(Slot);
        }

        public Earrings(string ID, int Variation) : base(ID, IDList[ID], typeof(Earrings), Variation)
        {

        }
    }
    #endregion

    #region Armour
    public class Armour : Item, IWearable
    {
        new public class ItemData : Item.ItemData
        {
            /// <summary>ID текстуры разных цветов</summary>
            /// <remarks>Работает только для Drawable 28!</remarks>
            public enum Colours
            {
                Green = 0,
                Orange = 1,
                Purple = 2,
                Pink = 3,
                Red = 4,
                Blue = 5,
                Grey = 6,
                LightGrey = 7,
                White = 8,
                Black = 9
            }

            public int MaxStrength { get; set; }

            public Dictionary<bool, (int Drawable, int DrawableTop)> Drawables { get; set; }

            public int Texture { get; set; }

            public ItemData(float Weight, string Model, int MaxStrength, int DrawableMale, int DrawableMaleTop, int DrawableFemale, int DrawableFemaleTop, int Texture) : base(Weight, Model)
            {
                this.MaxStrength = MaxStrength;

                this.Drawables = new Dictionary<bool, (int Drawable, int DrawableTop)>()
                {
                    { true, (DrawableMale, DrawableMaleTop) },
                    { false, (DrawableFemale, DrawableFemaleTop) },
                };

                this.Texture = Texture;
            }

            public ItemData(float Weight, string Model, int MaxStrength, int DrawableMale, int DrawableMaleTop, int DrawableFemale, int DrawableFemaleTop, Colours Colour) : this(Weight, Model, MaxStrength, DrawableMale, DrawableMaleTop, DrawableFemale, DrawableFemaleTop, (int)Colour) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "arm_shop", new ItemData(0.5f, "prop_armour_pickup", 100, 28, 19, 0, 0, ItemData.Colours.Grey) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public int Strength { get; set; }

        /// <summary>Метод для надевания брони на игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        public void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            player.SetClothes(9, pData.Clothes[1] == null ? data.Drawables[player.GetSex()].Drawable : data.Drawables[player.GetSex()].DrawableTop, data.Texture);
            player.SetArmour(Strength);
        }

        /// <summary>Метод для снятия брони с игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        public void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            var value = player.Armor;

            if (value < Strength)
            {
                Strength = value;

                this.Update();
            }

            player.SetClothes(9, 0, 0);
            player.SetArmour(0);
        }

        /// <summary>Метод для обновления прочности брони</summary>
        /// <param name="player">Сущность игрока</param>
        public void UpdateStrength(PlayerData pData)
        {
            var player = pData.Player;

            var value = player.Armor;

            if (value < Strength)
            {
                Strength = value;

                this.Update();
            }
        }
        public Armour(string ID) : base(ID, IDList[ID], typeof(Armour))
        {
            this.Strength = Data.MaxStrength;
        }
    }
    #endregion

    #region Bag
    public class Bag : Item, IWearable, IContainer
    {
        new public class ItemData : Item.ItemData
        {
            public Dictionary<bool, int> Drawables;

            /// <summary>Текстуры</summary>
            public int[] Textures { get; set; }

            /// <summary>Максимальное кол-во слотов</summary>
            public byte MaxSlots { get; set; }

            /// <summary>Максимальный вес содержимого</summary>
            public float MaxWeight { get; set; }

            public ItemData(int DrawableMale, int DrawableFemale, int[] Textures, byte MaxSlots, float MaxWeight) : base(0.25f, "prop_cs_heist_bag_01")
            {
                this.Drawables = new Dictionary<bool, int>()
                {
                    { true, DrawableMale },
                    { false, DrawableFemale },
                };

                this.Textures = Textures;

                this.MaxSlots = MaxSlots;
                this.MaxWeight = MaxWeight;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "bag_0", new ItemData(81, 81, new int[] { 0 }, 10, 5f) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        /// <summary>Предметы внутри</summary>
        [JsonIgnore]
        public Item[] Items { get; set; }

        public void Wear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.BeltOn)
                return;

            var iData = Data;

            player.SetClothes(5, iData.Drawables[player.GetSex()], iData.Textures[Var]);
        }

        public void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.BeltOn)
                return;

            player.SetClothes(5, 0, 0);
        }

        /// <summary>Итоговый вес</summary>
        /// <remarks>Включает в себя вес самой сумки!</remarks>
        [JsonIgnore]
        new public float Weight { get => base.Weight + Items.Sum(x => (x == null ? 0 : (x is Weapon ? (x as Weapon).Weight : (x is IContainer ? (x as IContainer).Weight : (x is IStackable ? (x as IStackable).Weight : x.Weight))))); }

        public int Var { get; set; }

        public Bag(string ID, int Variation = 0) : base(ID, IDList[ID], typeof(Bag))
        {
            this.Var = Variation;

            this.Items = new Item[Data.MaxSlots];
        }
    }
    #endregion

    #region Holster
    public class Holster : Item, IWearable, IContainer
    {
        new public class ItemData : Item.ItemData
        {
            public Dictionary<bool, (int Drawable, int DrawableWeapon)> Drawables;

            public int[] Textures { get; set; }

            public ItemData(int DrawableMale, int DrawableWeaponMale, int DrawableFemale, int DrawableWeaponFemale, int[] Textures) : base(0.1f, "prop_holster_01")
            {
                this.Drawables = new Dictionary<bool, (int Drawable, int DrawableWeapon)>()
                {
                    { true, (DrawableMale, DrawableWeaponMale) },
                    { false, (DrawableFemale, DrawableWeaponFemale) },
                };

                this.Textures = Textures;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "holster_0", new ItemData(136, 134, 0, 0, new int[] { 0, 1 }) },
            { "holster_1", new ItemData(135, 137, 0, 0, new int[] { 0, 1 }) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public Item[] Items { get; set; }

        public void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var iData = Data;
            var drawables = iData.Drawables[pData.Sex];

            player.SetClothes(10, Items[0] == null ? drawables.Drawable : drawables.DrawableWeapon, iData.Textures[Var]);
        }

        public void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(10, 0, 0);
        }

        public void WearWeapon(PlayerData pData)
        {
            var player = pData.Player;

            var iData = Data;
            var drawables = iData.Drawables[pData.Sex];

            player.SetClothes(10, drawables.DrawableWeapon, iData.Textures[Var]);
        }

        public void UnwearWeapon(PlayerData pData)
        {
            var player = pData.Player;

            var iData = Data;
            var drawables = iData.Drawables[pData.Sex];

            player.SetClothes(10, drawables.Drawable, iData.Textures[Var]);
        }

        [JsonIgnore]
        new public float Weight { get => (this as Item).Weight + ((Items[0] as Weapon)?.Weight ?? 0); }

        public int Var { get; set; }

        public Holster(string ID, int Variation = 0) : base(ID, IDList[ID], typeof(Holster))
        {
            this.Var = Variation;

            this.Items = new Item[1];
        }
    }
    #endregion

    #region Numberplate
    public class Numberplate : Item, ITagged
    {
        new public class ItemData : Item.ItemData
        {
            public int Number { get; set; }

            public ItemData(string Model, int Number) : base(0.15f, Model)
            {
                this.Number = Number;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>()
        {
            { "np_0", new ItemData("p_num_plate_01", 0) },
            { "np_1", new ItemData("p_num_plate_04", 1) },
            { "np_2", new ItemData("p_num_plate_02", 2) },
            { "np_3", new ItemData("p_num_plate_02", 3) },
            { "np_4", new ItemData("p_num_plate_01", 4) },
            { "np_5", new ItemData("p_num_plate_01", 5) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public string Tag { get; set; }

        public void Setup(Vehicle veh)
        {
            veh.NumberPlateStyle = Data.Number;
            veh.NumberPlate = Tag;
        }

        public void Take(Vehicle veh)
        {
            veh.NumberPlateStyle = 0;
            veh.NumberPlate = "";
        }

        private static char[] Chars = new char[]
        {
            'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public string GenerateTag()
        {
            Random rand = new Random();
            StringBuilder str = new StringBuilder();

            do
            {
                str.Clear();

                for (int i = 0; i < 9; i++)
                    str.Append(Chars[rand.Next(0, Chars.Length - 1)]);
            }
            while (ServerEvents.UsedNumberplates.Contains(str.ToString()));

            var retStr = str.ToString();

            ServerEvents.UsedNumberplates.Add(retStr);

            return retStr;
        }

        public Numberplate(string ID) : base(ID, IDList[ID], typeof(Numberplate))
        {
            this.Tag = "";
        }
    }
    #endregion

    #region Vehicle Key
    public class VehicleKey : Item, ITagged
    {
        new public class ItemData : Item.ItemData
        {
            public ItemData(float Weight, string Model) : base(Weight, Model) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {

        };

        public void Setup(VehicleData vData)
        {

        }

        public void Take(VehicleData vData)
        {

        }

        public string GenerateTag()
        {
            return null;
        }

        public bool IsKeyValid(Vehicle veh)
        {
            var vData = veh.GetMainData();

            if (vData == null)
                return false;

            if (vData.Keys.Contains(UID))
                return true;

            return false;
        }

        public string Tag { get; set; }

        public int VID { get; set; }

        public VehicleKey(string ID) : base(ID, IDList[ID], typeof(VehicleKey))
        {
            this.Tag = "";

            this.VID = -1;
        }
    }
    #endregion

    public class Cigarettes : Item, IConsumable
    {
        public int Amount { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Cigarettes(string ID, ItemData Data, Type Type) : base(ID, Data, Type)
        {

        }
    }

    public class StatusChanger : Item, IStackable
    {
        new public class ItemData : Item.ItemData
        {
            public int Satiety { get; set; }

            public int Mood { get; set; }

            public int EffectTime { get; set; }

            public Sync.Animations.FastTypes Animation { get; set; }

            public Sync.AttachSystem.Types? AttachType { get; set; }

            public int AttachTime { get; set; }

            public int AttachModelIdx { get; set; }

            public ItemData(float Weight, string[] Models, int Satiety = 0, int Mood = 0, int EffectTime = 0, Sync.Animations.FastTypes Animation = Sync.Animations.FastTypes.None, Sync.AttachSystem.Types? AttachType = null, int AttachTime = -1, int AttachModelIdx = 0) : base(Weight, Models)
            {
                this.Satiety = Satiety;
                this.Mood = Mood;

                this.EffectTime = EffectTime;

                this.Animation = Animation;

                this.AttachType = AttachType;
                this.AttachTime = AttachTime;

                this.AttachModelIdx = AttachModelIdx;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "sc_burger", new ItemData(0.15f, new string[] { "prop_cs_burger_01" }, 25, 0, 0, Sync.Animations.FastTypes.ItemBurger, Sync.AttachSystem.Types.ItemBurger, 6000) },
            { "sc_chips", new ItemData(0.15f, new string[] { "prop_food_bs_chips" }, 15, 0, 0, Sync.Animations.FastTypes.ItemChips, Sync.AttachSystem.Types.ItemChips, 6000) },
            { "sc_pizza", new ItemData(0.15f, new string[] { "v_res_tt_pizzaplate" }, 50, 15, 0, Sync.Animations.FastTypes.ItemPizza, Sync.AttachSystem.Types.ItemPizza, 6000) },
            { "sc_chocolate", new ItemData(0.15f, new string[] { "prop_candy_pqs" }, 10, 20, 0, Sync.Animations.FastTypes.ItemChocolate, Sync.AttachSystem.Types.ItemChocolate, 6000) },
            { "sc_hotdog", new ItemData(0.15f, new string[] { "prop_cs_hotdog_01" }, 10, 20, 0, Sync.Animations.FastTypes.ItemChocolate, Sync.AttachSystem.Types.ItemChocolate, 6000) },

            { "sc_cola", new ItemData(0.15f, new string[] { "prop_food_juice01" }, 5, 20, 0, Sync.Animations.FastTypes.ItemCola, Sync.AttachSystem.Types.ItemCola, 6000) },

            { "sc_cigs", new ItemData(0.15f, new string[] { "prop_cigar_pack_01", "prop_amb_ciggy_01", "ng_proc_cigarette01a" }, 0, 25, 0, Sync.Animations.FastTypes.None, Sync.AttachSystem.Types.ItemCigMouth, -1, 2) },
            //{ "sc_joint", new StatusChanger.ItemData(Item.Types.Cigarettes, 0, 50, 0, Sync.Animations.FastTypes.None, Sync.AttachSystem.Types.ItemJoint, 6000) },

            { "sc_beer", new ItemData(0.15f, new string[] { "prop_sh_beer_pissh_01" }, 5, 50, 0, Sync.Animations.FastTypes.ItemBeer, Sync.AttachSystem.Types.ItemBeer, 6000) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public float Weight { get => Amount * base.Weight; }

        [JsonIgnore]
        public int MaxAmount => 999;

        public int Amount { get; set; }

        public void Apply(PlayerData pData)
        {
            var player = pData.Player;

            if (player?.Exists != true)
                return;

            if (Data.Satiety > 0)
            {
                var satietyDiff = Utils.GetCorrectDiff(pData.Satiety, Data.Satiety, 0, 100);

                if (satietyDiff != 0)
                {
                    pData.Satiety += satietyDiff;
                }
            }

            if (Data.Mood > 0)
            {
                var moodDiff = Utils.GetCorrectDiff(pData.Mood, Data.Mood, 0, 100);

                if (moodDiff != 0)
                {
                    pData.Mood += moodDiff;
                }
            }

            if (Data.AttachType != null)
            {
                player.AttachObject(base.Data.GetModelAt(Data.AttachModelIdx), (Sync.AttachSystem.Types)Data.AttachType, Data.AttachTime);
            }

            if (Data.Animation != Sync.Animations.FastTypes.None)
            {
                pData.PlayAnim(Data.Animation);
            }
        }

        public StatusChanger(string ID) : base(ID, IDList[ID], typeof(StatusChanger))
        {

        }
    }

    public class Items
    {
        private static Dictionary<string, Type> AllTypes = new Dictionary<string, Type>();

        private static Dictionary<Type, Dictionary<string, Item.ItemData>> AllData = new Dictionary<Type, Dictionary<string, Item.ItemData>>();

        #region Give
        /// <summary>Создать и выдать предмет игроку</summary>
        /// <param name="player">Сущность игрока, которому необходимо выдать предмет</param>
        /// <inheritdoc cref="CreateItem(string, int, int, bool)"/>
        public static async Task<Item> GiveItem(PlayerData pData, string id, int variation = 0, int amount = 1, bool isTemp = false)
        {
            if (pData == null)
                return null;

            var player = pData.Player;

            var item = await Task.Run(async () =>
            {
                var type = GetType(id, false);

                if (type == null)
                    return null;

                var data = GetData(id, type);

                if (data == null)
                    return null;

                var interfaces = type.GetInterfaces();

                var inv = pData.Items;

                var totalWeight = 0f;
                var totalFreeSlots = 0;

                bool stackable = interfaces.Contains(typeof(IStackable));
                bool weapon = type == typeof(Weapon);

                if (!weapon && amount == 0)
                    return null;

                foreach (var x in inv)
                    if (x != null)
                        totalWeight += GetItemWeight(x, true);
                    else
                        totalFreeSlots += 1;

                bool weightOk = false;

                if (weapon)
                {
                    var ammoType = ((Weapon.ItemData)Weapon.IDList[id]).AmmoID;

                    weightOk = totalWeight + data.Weight + (ammoType == null ? 0 : (Ammo.IDList[ammoType].Weight * amount)) < Settings.MAX_INVENTORY_WEIGHT;
                }
                else
                    weightOk = totalWeight + data.Weight * amount < Settings.MAX_INVENTORY_WEIGHT;

                if (totalFreeSlots <= 0 || !weightOk)
                {
                    NAPI.Task.Run(() =>
                    {
                        if (player?.Exists != true)
                            return;

                        player.Notify("Inventory::NoSpace");
                    });

                    return null;

                }

                var item = await CreateItem(id, variation, amount, isTemp);

                if (item == null)
                    return null;

                var freeIdx = -1;

                for (int i = 0; i < pData.Items.Length; i++)
                    if (pData.Items[i] == null)
                    {
                        freeIdx = i;

                        pData.Items[i] = item;

                        break;
                    }

                amount = (item as Game.Items.IStackable)?.Amount ?? 1;

                var upd = Game.Items.Item.ToClientJson(item, CEF.Inventory.Groups.Items);

                await NAPI.Task.RunAsync(() =>
                {
                    if (player?.Exists != true)
                        return;

                    player.TriggerEvent("Item::Added", item.ID, amount);
                    player.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Items, freeIdx, upd);
                });

                MySQL.UpdatePlayerInventory(pData, true);

                return item;
            });

            return item;
        }
        #endregion

        #region Create
        /// <summary>Метод для создания нового предмета</summary>
        /// <param name="id">ID предмета (см. Game.Items.Item.LoadAll</param>
        /// <param name="variation">Вариация предмета (только для IWearable, в противном случае - игнорируется)</param>
        /// <param name="amount">Кол-во предмета (только для IStakable и Weapon, в противном случае - игнорируется)</param>
        /// <param name="isTemp">Временный ли предмет?<br/>Такой предмет не будет сохраняться в базу данных и его будет нельзя: <br/><br/>1) Разделять (если IStackable или Weapon)<br/>2) Перемещать в IContainer и Game.Items.Container<br/>3) Выбрасывать (предмет удалится, но не появится на земле)<br/>4) Передавать другим игрокам</param>
        /// <returns>Объект класса Item, если предмет был создан, null - в противном случае</returns>
        public static async Task<Item> CreateItem(string id, int variation = 0, int amount = 1, bool isTemp = false)
        {
            return await Task.Run<Item>(async () =>
            {
                var type = GetType(id, false);

                if (type == null)
                    return null;

                var data = GetData(id, type);

                if (data == null)
                    return null;

                var interfaces = type.GetInterfaces();

                bool stackable = interfaces.Contains(typeof(IStackable));
                bool weapon = type == typeof(Weapon);

                if (!weapon && amount == 0)
                    return null;

                if (!stackable && !weapon)
                    amount = 1;

                Item item = null;

                if (type == typeof(Clothes))
                {
                    var textures = ((Clothes.ItemData)data).Textures.Length - 1;

                    if (textures < variation || variation < 0)
                        variation = 0;

                    item = (Clothes)Activator.CreateInstance(type, id, variation);
                }
                else if (type == typeof(Holster))
                {
                    var textures = ((Holster.ItemData)data).Textures.Length - 1;

                    if (textures < variation || variation < 0)
                        variation = 0;

                    item = (Holster)Activator.CreateInstance(type, id, variation);
                }
                else if (type == typeof(Bag))
                {
                    var textures = ((Bag.ItemData)data).Textures.Length - 1;

                    if (textures < variation || variation < 0)
                        variation = 0;

                    item = (Bag)Activator.CreateInstance(type, id, variation);
                }
                else
                    item = (Item)Activator.CreateInstance(type, id);

                if (stackable)
                {
                    var maxAmount = (item as IStackable).MaxAmount;

                    (item as IStackable).Amount = amount > maxAmount ? maxAmount : amount;
                }            
                else if (weapon)
                {
                    var maxAmount = (item as Weapon).Data.MaxAmmo;

                    (item as Weapon).Ammo = amount > maxAmount ? maxAmount : amount;
                }

                /*            if (!temp && interfaces.Contains(typeof(ITagged)))
                                (item as ITagged).Tag = (item as ITagged).GenerateTag();*/

                if (!isTemp)
                    return MySQL.AddNewItem(item);
                else
                {
                    item.UID = 0;
                    return item;
                }
            });
        }
        #endregion

        #region Stuff
        public static float GetItemWeight(Item item, bool calculateStackable = false)
        {
            if (item is Game.Items.IContainer)
                return (item as Game.Items.IContainer).Weight;

            if (calculateStackable && item is Game.Items.IStackable)
                return (item as Game.Items.IStackable).Weight;

            if (item is Game.Items.Weapon)
                return (item as Game.Items.Weapon).Weight;

            return item.Weight;
        }

        public static int GetItemAmount(Game.Items.Item item) => (item as Game.Items.IStackable)?.Amount ?? 1;

        public static float GetWeight(Game.Items.Item[] items, bool calculateStackable = true) => items.Sum(x => x == null ? 0 : GetItemWeight(x, calculateStackable));

        public static string GetItemTag(Game.Items.Item item)
        {
            if (item is Weapon weapon)
                return weapon.Ammo > 0 ? weapon.Ammo.ToString() : null;

            if (item is Armour armour)
                return armour.Strength.ToString();

            if (item is IConsumable consumable)
                return consumable.Amount.ToString();

            if (item is ITagged tagged)
                return tagged.Tag;

            return null;
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

            var insIdx = 0;

            using (var sr = new StreamReader(Settings.DIR_CLIENT_ITEMS_DATA_PATH))
            {
                bool ignore = false;

                string line;

                var i = 0;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!ignore)
                    {
                        if (line.Contains("#region TO_REPLACE"))
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

            foreach (var x in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == ns && t.IsClass && !t.IsAbstract && typeof(Item).IsAssignableFrom(t)))
            {
                //var idList= (IDictionary)x.GetField("IDList")?.GetValue(null).Cast<dynamic>().ToDictionary(a => (string)a.Key, a => (Item.ItemData)a.Value);

                var idList = (Dictionary<string, Item.ItemData>)x.GetField("IDList")?.GetValue(null);

                if (idList == null)
                    continue;

                string str = $"public class {x.Name} : {x.BaseType.Name}";

                var interfaces = x.GetInterfaces();

                if (interfaces.Length > 0)
                    str += $", {string.Join(", ", interfaces.Select(x => x.FullName.Replace(ns + ".", "").Replace('+', '.')))}";

                str += "\n{\n\tpublic static Dictionary<string, Data> IDList { get; set; } = new Dictionary<string, Data>()\n\t{\n\t\t" + string.Join(",\n\t\t", idList.Select(x => "{ " + $"\"{x.Key}\", new Data()" + " }")) + "\n\t};\n}\n";

                lines.Insert(++insIdx, str);

                AllData.Add(x, idList);

                counter += idList.Count;

                foreach (var t in idList)
                {
                    var id = t.Key.Split('_');

                    if (!AllTypes.ContainsKey(id[0]))
                        AllTypes.Add(id[0], x);
                }
            }

            File.WriteAllLines(Settings.DIR_CLIENT_ITEMS_DATA_PATH, lines);

            return counter;
        }
    }

    #region Item JSON Converter

    public class BaseSpecifiedConcreteClassConverter : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(Item).IsAssignableFrom(objectType) && !objectType.IsAbstract)
                return null; // pretend TableSortRuleConvert is not specified (thus avoiding a stack overflow)
            return base.ResolveContractConverter(objectType);
        }
    }

    public class ItemConverter : JsonConverter
    {
        static JsonSerializerSettings SpecifiedSubclassConversion = new JsonSerializerSettings() { ContractResolver = new BaseSpecifiedConcreteClassConverter() };

        public override bool CanConvert(Type objectType) => objectType == typeof(Item);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jo = JObject.Load(reader);

            var type = Items.GetType(jo["ID"].Value<string>());

            return JsonConvert.DeserializeObject(jo.ToString(), type, SpecifiedSubclassConversion);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {}
    }
    #endregion
}
