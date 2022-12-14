using BCRPServer.Game.Data;
using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1;
using System;
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
        public static Queue<uint> FreeIDs { get; private set; } = new Queue<uint>();

        public static Dictionary<uint, Item> All { get; private set; } = new Dictionary<uint, Item>();

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

        public static void AddOnLoad(Item item)
        {
            if (item == null)
                return;

            All.Add(item.UID, item);

            if (item.UID > LastAddedMaxId)
                LastAddedMaxId = item.UID;
        }

        public static void Add(Item item)
        {
            if (item == null)
                return;

            All.Add(item.UID, item);

            MySQL.ItemAdd(item);
        }

        public static void Remove(Item item)
        {
            if (item == null)
                return;

            if (item is Game.Items.IContainer cont)
            {
                foreach (var x in cont.Items)
                {
                    if (x != null)
                    {
                        if (x is Game.Items.Numberplate np)
                        {
                            Numberplate.UsedTags.Remove(np.Tag);
                        }
                        else if (x is Game.Items.Weapon weapon)
                        {
                            Weapon.UsedTags.Remove(weapon.Tag);
                        }

                        AddFreeId(x.UID);

                        All.Remove(x.UID);

                        MySQL.ItemDelete(x);
                    }
                }
            }
            else
            {
                if (item is Game.Items.Numberplate np)
                {
                    Numberplate.UsedTags.Remove(np.Tag);
                }
                else if (item is Game.Items.Weapon weapon)
                {
                    Weapon.UsedTags.Remove(weapon.Tag);
                }

                AddFreeId(item.UID);

                All.Remove(item.UID);

                MySQL.ItemDelete(item);
            }
        }

        public static void RemoveOnLoad(Item item)
        {
            if (item == null)
                return;

            if (item is Game.Items.Numberplate np)
            {
                Numberplate.UsedTags.Remove(np.Tag);
            }
            else if (item is Game.Items.Weapon weapon)
            {
                Weapon.UsedTags.Remove(weapon.Tag);
            }

            AddFreeId(item.UID);

            All.Remove(item.UID);
        }

        public static Item Get(uint id) => All.GetValueOrDefault(id);

        private static Dictionary<Game.Items.Inventory.Groups, Func<Item, string>> ClientJsonFuncs = new Dictionary<Game.Items.Inventory.Groups, Func<Item, string>>()
        {
            { Game.Items.Inventory.Groups.Items, (item) => (new object[] { item.ID, Items.GetItemAmount(item), item is IStackable ? item.BaseWeight : item.Weight, Items.GetItemTag(item) }).SerializeToJson() },

            { Game.Items.Inventory.Groups.Bag, (item) => (new object[] { item.ID, Items.GetItemAmount(item), item is IStackable ? item.BaseWeight : item.Weight, Items.GetItemTag(item) }).SerializeToJson() },

            { Game.Items.Inventory.Groups.Container, (item) => (new object[] { item.ID, Items.GetItemAmount(item), item is IStackable ? item.BaseWeight : item.Weight, Items.GetItemTag(item) }).SerializeToJson() },

            {
                Game.Items.Inventory.Groups.Weapons,

                (item) =>
                {
                    var weapon = (Weapon)item;

                    return (new object[] { item.ID, weapon.Ammo, weapon.Equiped, weapon.Tag }).SerializeToJson();
                }
            },

            {
                Game.Items.Inventory.Groups.Holster,

                (item) =>
                {
                    var weapon = (Weapon)item;

                    return (new object[] { item.ID, weapon.Ammo, weapon.Equiped, weapon.Tag }).SerializeToJson();
                }
            },

            { Game.Items.Inventory.Groups.Armour, (item) => (new object[] { item.ID, ((Armour)item).Strength }).SerializeToJson() },

            {
                Game.Items.Inventory.Groups.BagItem,

                (item) =>
                {
                    var bag = (Bag)item;

                    return (new object[] { item.ID, bag.Data.MaxWeight, bag.Items.Select(x => ToClientJson(x, Game.Items.Inventory.Groups.Bag)) }).SerializeToJson();
                }
            },

            { Game.Items.Inventory.Groups.Clothes, (item) => (new object[] { item.ID }).SerializeToJson() },

            { Game.Items.Inventory.Groups.Accessories, (item) => (new object[] { item.ID }).SerializeToJson() },

            {
                Game.Items.Inventory.Groups.HolsterItem,
                
                (item) =>
                {
                    var holster = (Holster)item;

                    return (new object[] { item.ID, ToClientJson(holster.Items[0], Game.Items.Inventory.Groups.Holster) }).SerializeToJson();
                }
            },
        };

        public abstract class ItemData
        {
            /// <summary>Этот интерфейс реализуют классы таких предметов, которые могут хранить в себе другие предметы</summary>
            public interface IContainer
            {
                public float MaxWeight { get; }
            }

            /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны стакаться</summary>
            public interface IStackable
            {
                /// <summary>Максимальное кол-во единиц предмета в стаке</summary>
                public int MaxAmount { get; set; }
            }

            /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны тратиться</summary>
            /// <remarks>Не использовать одновременно с IStackable!</remarks>
            public interface IConsumable
            {
                public int MaxAmount { get; set; }
            }

            public interface IDependent
            {
                /// <summary>ID предмета, от которого зависит данный предмет</summary>
                public string DependentByID { get; set;  }

                /// <summary>Необходимое кол-во зависимого предмета для траты</summary>
                public int DependentByAmount { get; set; }
            }

            /// <summary>Стандартная модель</summary>
            public static uint DefaultModel = NAPI.Util.GetHashKey("prop_drug_package_02");

            /// <summary>Название предмета</summary>
            public string Name{ get; set; }

            /// <summary>Вес единицы предмета</summary>
            public float Weight { get; set; }

            /// <summary>Основная модель</summary>
            public uint Model { get => Models[0]; }

            /// <summary>Все модели</summary>
            private uint[] Models { get; set; }

            public abstract string ClientData { get; }

            public ItemData(string Name, float Weight, params uint[] Models)
            {
                this.Name = Name;

                this.Weight = Weight;

                this.Models = Models.Length > 0 ? Models : new uint[] { DefaultModel };
            }

            public ItemData(string Name, float Weight, params string[] Models) : this(Name, Weight, Models.Select(x => NAPI.Util.GetHashKey(x)).ToArray()) { }

            public uint GetModelAt(int idx) => idx < 0 || idx >= Models.Length ? Model : Models[idx];
        }

        [JsonIgnore]
        public Type Type { get; set; }

        /// <summary>Данные предмета</summary>
        [JsonIgnore]
        public ItemData Data { get; set; }

        /// <summary>Стандартный вес предмета (1 единица)</summary>
        [JsonIgnore]
        public float BaseWeight { get => Data.Weight; }

        /// <summary>Фактический вес предмета</summary>
        [JsonIgnore]
        public virtual float Weight { get => BaseWeight; }

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

            Remove(this);
        }

        /// <summary>Метод для обновления предмета в базе данных</summary>
        public void Update()
        {
            if (IsTemp)
                return;

            MySQL.ItemUpdate(this);
        }

        public string ToClientJson(Game.Items.Inventory.Groups group)
        {
            var func = ClientJsonFuncs.GetValueOrDefault(group);

            if (func == null)
                return "null";

            return func.Invoke(this);
        }

        public static string ToClientJson(Item item, Game.Items.Inventory.Groups group) => item == null ? "null" : item.ToClientJson(group);

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
    }

    /// <summary>Этот интерфейс реализуют классы таких предметов, которые способны тратиться</summary>
    /// <remarks>Не использовать одновременно с IStackable!</remarks>
    public interface IConsumable
    {
        /// <summary>Максимальное кол-во единиц предмета</summary>
        [JsonIgnore]
        public int MaxAmount { get; }

        /// <summary>Кол-во оставшихся единиц предмета</summary>
        public int Amount { get; set; }
    }

    public interface IDependent
    {
        /// <summary>ID предмета, от которого зависит данный предмет</summary>
        [JsonIgnore]
        public string DependentByID { get; }

        /// <summary>Необходимое кол-во зависимого предмета для траты</summary>
        [JsonIgnore]
        public int DependentByAmount { get; }
    }
    #endregion

    #region Weapon
    public class Weapon : Item, ITagged, IWearable
    {
        public static List<string> UsedTags { get; private set; } = new List<string>();

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
            public Sync.AttachSystem.Types[] AttachTypes { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {(AmmoID == null ? "null" : $"\"{AmmoID}\"")}, {MaxAmmo}, {Hash}";

            /// <summary>Создать новое оружие</summary>
            /// <param name="ItemType">Глобальный тип предмета</param>
            /// <param name="TopType">Тип оружия</param>
            /// <param name="AmmoID">Тип патронов (если нет - null)</param>
            /// <param name="Hash">Хэш оружия</param>
            /// <param name="MaxAmmo">Максимальное кол-во патронов</param>
            /// <param name="CanUseInVehicle">Может ли использоваться в транспорте?</param>
            public ItemData(string Name, float Weight, string Model, TopTypes TopType, string AmmoID, uint Hash, int MaxAmmo, bool CanUseInVehicle = false) : base(Name, Weight, Model)
            {
                this.TopType = TopType;
                this.AmmoID = AmmoID;

                this.CanUseInVehicle = CanUseInVehicle;

                this.Hash = Hash;

                this.MaxAmmo = MaxAmmo;

                if (TopType == TopTypes.Shotgun || TopType == TopTypes.AssaultRifle || TopType == TopTypes.SniperRifle || TopType == TopTypes.HeavyWeapon)
                    this.AttachTypes = new Sync.AttachSystem.Types[] { Sync.AttachSystem.Types.WeaponLeftBack, Sync.AttachSystem.Types.WeaponRightBack };
                else if (TopType == TopTypes.SubMachine)
                    this.AttachTypes = new Sync.AttachSystem.Types[] { Sync.AttachSystem.Types.WeaponLeftTight, Sync.AttachSystem.Types.WeaponRightTight };
            }

            /// <inheritdoc cref="ItemData.ItemData(Types, TopTypes, Types?, uint, int, bool)"/>
            public ItemData(string Name, float Weight, string Model, TopTypes TopType, string AmmoType, WeaponHash Hash, int MaxAmmo, bool CanUseInVehicle = false) : this(Name, Weight, Model, TopType, AmmoType, (uint)Hash, MaxAmmo, CanUseInVehicle) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "w_asrifle", new ItemData("AK-47", 1.5f, "w_ar_assaultrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Assaultrifle, 30, false) },
            { "w_asrifle_mk2", new ItemData("AK-47 (улучш.)", 1.5f, "w_ar_assaultriflemk2", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Assaultrifle_mk2, 35, false) },
            { "w_advrifle", new ItemData("TAR-21", 1f, "w_ar_advancedrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Advancedrifle, 30, false) },
            { "w_carbrifle", new ItemData("AR-15", 1f, "w_ar_carbinerifle", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Carbinerifle, 30, false) },
            { "w_comprifle", new ItemData("AK-47 (укороч.)", 1f, "w_ar_assaultrifle_smg", ItemData.TopTypes.AssaultRifle, "am_7.62", WeaponHash.Compactrifle, 30, false) },
            { "w_heavyrifle", new ItemData("SCAR", 1f, "w_ar_heavyrifle", ItemData.TopTypes.AssaultRifle, "am_7.62", 0xC78D71B4, 30, false) },

            { "w_microsmg", new ItemData("UZI", 1f, "w_sb_microsmg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Microsmg, 15, true) },
            { "w_minismg", new ItemData("Мини SMG", 1f, "w_sb_minismg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Minismg, 15, true) },
            { "w_smg", new ItemData("MP5", 1f, "w_sb_smg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Smg, 15, true) },
            { "w_smg_mk2", new ItemData("MP5 (улучш.)", 1f, "w_sb_smgmk2", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Smg_mk2, 15, true) },
            { "w_asmsg", new ItemData("null", 1f, "w_sb_assaultsmg", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Assaultsmg, 15, false) },
            { "w_combpdw", new ItemData("Боевой PDW", 1f, "w_sb_pdw", ItemData.TopTypes.SubMachine, "am_5.56", WeaponHash.Combatpdw, 15, false) },

            { "w_combmg", new ItemData("M249", 1f, "w_mg_combatmg", ItemData.TopTypes.LightMachine, "am_9", WeaponHash.Combatmg, 15, false) },
            { "w_gusenberg", new ItemData("ПП Томпсона", 1f, "w_sb_gusenberg", ItemData.TopTypes.LightMachine, "am_9", WeaponHash.Gusenberg, 15, false) },

            { "w_heavysnp", new ItemData("Barrett M82", 1f, "w_sr_heavysniper", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Heavysniper, 15, false) },
            { "w_markrifle", new ItemData("Винтовка Марксмана", 1f, "w_sr_marksmanrifle", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Marksmanrifle, 15, false) },
            { "w_musket", new ItemData("Мушкет", 1f, "w_ar_musket", ItemData.TopTypes.SniperRifle, "am_12.7", WeaponHash.Musket, 15, false) },

            { "w_assgun", new ItemData("null", 1f, "w_sg_assaultshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Assaultshotgun, 15, false) },
            { "w_heavysgun", new ItemData("null", 1f, "w_sg_heavyshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Heavyshotgun, 15, false) },
            { "w_pumpsgun", new ItemData("null", 1f, "w_sg_pumpshotgun", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Pumpshotgun, 15, false) },
            { "w_pumpsgun_mk2", new ItemData("null", 1f, "w_sg_pumpshotgunmk2", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Pumpshotgun_mk2, 15, false) },
            { "w_sawnsgun", new ItemData("null", 1f, "w_sg_sawnoff", ItemData.TopTypes.Shotgun, "am_12", WeaponHash.Sawnoffshotgun, 15, false) },

            { "w_pistol", new ItemData("Пистолет", 0.5f, "w_pi_pistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Pistol, 15, true) },
            { "w_pistol_mk2", new ItemData("Пистолет (улучш.)", 1f, "w_pi_pistolmk2", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Pistol_mk2, 20, true) },
            { "w_appistol", new ItemData("Автоматический пистолет", 1f, "w_pi_appistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Appistol, 20, true) },
            { "w_combpistol", new ItemData("P2000", 1f, "w_pi_combatpistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Combatpistol, 20, true) },
            { "w_heavypistol", new ItemData("Remington 1911", 1f, "w_pi_heavypistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Heavypistol, 20, true) },
            { "w_machpistol", new ItemData("TEC-9", 1f, "w_sb_smgmk2", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Machinepistol, 20, true) },
            { "w_markpistol", new ItemData("Пистолет Марксмана", 1f, "w_pi_singleshot", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Marksmanpistol, 20, true) },
            { "w_vintpistol", new ItemData("Винтажный пистолет", 1f, "w_pi_vintage_pistol", ItemData.TopTypes.HandGun, "am_5.56", WeaponHash.Vintagepistol, 20, true) },

            { "w_revolver", new ItemData("Револьвер", 1f, "w_pi_revolver", ItemData.TopTypes.HandGun, "am_11.43", WeaponHash.Revolver, 20, true) },
            { "w_revolver_mk2", new ItemData("Револьвер (улучш.)", 1f, "w_pi_revolvermk2", ItemData.TopTypes.HandGun, "am_11.43", WeaponHash.Revolver_mk2, 20, true) },

            { "w_bat", new ItemData("Бита", 1f, "w_me_bat", ItemData.TopTypes.Melee, null, WeaponHash.Bat, 0, false) },
            { "w_bottle", new ItemData("'Розочка'", 1f, "w_me_bottle", ItemData.TopTypes.Melee, null, WeaponHash.Bottle, 0, false) },
            { "w_crowbar", new ItemData("Гвоздодёр", 1f, "w_me_crowbar", ItemData.TopTypes.Melee, null, WeaponHash.Crowbar, 0, false) },
            { "w_dagger", new ItemData("Клинок", 1f, "w_me_dagger", ItemData.TopTypes.Melee, null, WeaponHash.Dagger, 0, false) },
            { "w_flashlight", new ItemData("Фонарик", 1f, "w_me_flashlight", ItemData.TopTypes.Melee, null, WeaponHash.Flashlight, 0, false) },
            { "w_golfclub", new ItemData("Клюшка", 1f, "w_me_gclub", ItemData.TopTypes.Melee, null, WeaponHash.Golfclub, 0, false) },
            { "w_hammer", new ItemData("Молоток", 1f, "w_me_hammer", ItemData.TopTypes.Melee, null, WeaponHash.Hammer, 0, false) },
            { "w_hatchet", new ItemData("Топор", 1f, "w_me_hatchet", ItemData.TopTypes.Melee, null, WeaponHash.Hatchet, 0, false) },
            { "w_knuckles", new ItemData("Кастет", 1f, "w_me_knuckle", ItemData.TopTypes.Melee, null, WeaponHash.Knuckle, 0, false) },
            { "w_machete", new ItemData("Мачете", 1f, "prop_ld_w_me_machette", ItemData.TopTypes.Melee, null, WeaponHash.Machete, 0, false) },
            { "w_nightstick", new ItemData("Резиновая дубинка", 1f, "w_me_nightstick", ItemData.TopTypes.Melee, null, WeaponHash.Nightstick, 0, false) },
            { "w_poolcue", new ItemData("Кий", 1f, "prop_pool_cue", ItemData.TopTypes.Melee, null, WeaponHash.Poolcue, 0, false) },
            { "w_switchblade", new ItemData("Складной нож", 1f, "w_me_switchblade", ItemData.TopTypes.Melee, null, WeaponHash.Switchblade, 0, false) },
            { "w_wrench", new ItemData("Гаечный ключ", 0.75f, "prop_cs_wrench", ItemData.TopTypes.Melee, null, WeaponHash.Wrench, 0, false) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        /// <summary>Обший вес оружия (вместе с патронами в обойме)</summary>
        [JsonIgnore]
        public override float Weight { get => Data.AmmoID == null ? BaseWeight : BaseWeight + Ammo * (Game.Items.Ammo.IDList[Data.AmmoID].Weight); }

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

            Sync.Players.StopUsePhone(pData);

            Unwear(pData);

            Equiped = true;

            player.SetWeapon(Data.Hash, Ammo);

/*            NAPI.Task.Run(() =>
            {
                if (player?.Exists != true)
                    return;

                var weap = pData.ActiveWeapon;

                if (weap != null && weap.Value.WeaponItem == this)
                    player.SetWeapon(Data.Hash, Ammo);
            }, 250);*/
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

                if (amount < this.Ammo)
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
            return null;
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
            this.AttachID = -1;
        }
    }
    #endregion

    #region Ammo
    public class Ammo : Item, IStackable
    {
        new public class ItemData : Item.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {MaxAmount}";

            public ItemData(string Name, float Weight, string Model, int MaxAmount = 1024) : base(Name, Weight, Model)
            {
                this.MaxAmount = MaxAmount;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "am_5.56", new ItemData("Патроны 5.56мм", 0.01f, "w_am_case", 1024) },
            { "am_7.62", new ItemData("Патроны 7.62мм", 0.01f, "w_am_case", 1024) },
            { "am_9", new ItemData("Патроны 9мм", 0.01f, "w_am_case", 1024) },
            { "am_11.43", new ItemData("Патроны 11.43мм", 0.015f, "w_am_case", 512) },
            { "am_12", new ItemData("Патроны 12мм", 0.015f, "w_am_case", 512) },
            { "am_12.7", new ItemData("Патроны 12.7мм", 0.015f, "w_am_case", 256) },
        };

        public static ItemData GetData(string id) => (ItemData)IDList[id];

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public override float Weight { get => Amount * BaseWeight; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

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

        public interface IProp
        {

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

            public interface IToggleable
            {
                public ExtraData ExtraData { get; set; }
            }

            public bool Sex { get; set; }

            public int Drawable { get; set; }

            public int[] Textures { get; set; }

            public string SexAlternativeID { get; set; }

            public ItemData(string Name, float Weight, string Model, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Model)
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
            this.Var = Var >= this.Data.Textures.Length ? this.Data.Textures.Length - 1 : (Var < 0 ? 0 : Var);

            var sexAltId = this.Data.SexAlternativeID;

            if (sexAltId != null)
                SexAlternativeData = (ItemData)Items.GetData(sexAltId, Type);
        }
    }

    public class Hat : Clothes, Clothes.IToggleable, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(ExtraData == null ? "null" : $"new Hat.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, 0.1f, "prop_proxy_hat_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.ExtraData = ExtraData;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "hat_m_0", new ItemData("Шапка обычная", true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_1", new ItemData("Панама", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_2", new ItemData("Кепка Snapback", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_3", new ItemData("Шапка трикотажная", true, 28, new int[] { 0, 1, 2, 3, 4, 5 }, null, null) },
            { "hat_m_4", new ItemData("Шапка восьмиклинка", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_5", new ItemData("Кепка козырьком назад", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_6", new ItemData("Бандана", true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_7", new ItemData("Наушники", true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_8", new ItemData("Панама с принтами", true, 20, new int[] { 0, 1, 2, 3, 4, 5 }, null, null) },
            { "hat_m_9", new ItemData("Шляпа USA", true, 31, new int[] { 0 }, null, null) },
            { "hat_m_10", new ItemData("Цилиндр USA", true, 32, new int[] { 0 }, null, null) },
            { "hat_m_11", new ItemData("Шапка USA", true, 34, new int[] { 0 }, null, null) },
            { "hat_m_12", new ItemData("Каска болельщика", true, 37, new int[] { 0, 1, 2, 3, 4, 5 }, null, null) },
            { "hat_m_13", new ItemData("Кепка Snapback #2", true, 54, new int[] { 0, 1 }, null, null) },
            { "hat_m_14", new ItemData("Кепка с принтами", true, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_15", new ItemData("Кепка дизайнерская", true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_16", new ItemData("Кепка SecuroServ", true, 65, new int[] { 0 }, null, null) },
            { "hat_m_17", new ItemData("Бандана байкерская", true, 83, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_m_18", new ItemData("Панама разноцветная", true, 94, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_19", new ItemData("Шапка трикотажная #2", true, 120, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_m_20", new ItemData("Кепка Diamond", true, 135, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null, null) },
            { "hat_m_21", new ItemData("Шлем с принтами", true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_22", new ItemData("Шлем обычный", true, 50, new int[] { 0 }, null, null) },
            { "hat_m_23", new ItemData("Шлем зеркальный", true, 51, new int[] { 0 }, null, null) },
            { "hat_m_24", new ItemData("Шлем разноцветный", true, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null, null) },
            { "hat_m_25", new ItemData("Шлем It's Go Time", true, 78, new int[] { 0, 1, 2, 3, 4 }, null, null) },
            { "hat_m_26", new ItemData("Шлем It's Go Time #2", true, 80, new int[] { 0, 1, 2, 3 }, null, null) },
            { "hat_m_27", new ItemData("Шлем модника", true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null, null) },
            { "hat_m_28", new ItemData("Каска байкерская #2", true, 85, new int[] { 0 }, null, null) },
            { "hat_m_29", new ItemData("Каска байкерская #2 (козырёк)", true, 86, new int[] { 0 }, null, null) },
            { "hat_m_30", new ItemData("Каска байкерская #2 (ирокез)", true, 87, new int[] { 0 }, null, null) },
            { "hat_m_31", new ItemData("Каска байкерская #2 (шипы)", true, 88, new int[] { 0 }, null, null) },
            { "hat_m_32", new ItemData("Кепка брендовая #1", true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, null) },
            { "hat_m_33", new ItemData("Кепка брендовая #2", true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null, null) },
            { "hat_m_34", new ItemData("Кепка брендовая #3", true, 130, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, null, null) },
            { "hat_m_35", new ItemData("Кепка с принтами #2", true, 139, new int[] { 0, 1, 2 }, null, null) },
            { "hat_m_36", new ItemData("Кепка цветная", true, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_m_37", new ItemData("Кепка современная", true, 151, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_38", new ItemData("Элегантная панама", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_39", new ItemData("Ковбойская шляпа", true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_40", new ItemData("Порк-пай", true, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_41", new ItemData("Шляпа с бабочкой", true, 25, new int[] { 0, 1, 2 }, null, null) },
            { "hat_m_42", new ItemData("Котелок", true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
            { "hat_m_43", new ItemData("Цилиндр", true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
            { "hat_m_44", new ItemData("Трилби", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_m_45", new ItemData("Борсалино", true, 30, new int[] { 0, 1 }, null, null) },
            { "hat_m_46", new ItemData("Цилиндр USA #2", true, 33, new int[] { 0, 1 }, null, null) },
            { "hat_m_47", new ItemData("Кепка Snapback модника", true, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_m_48", new ItemData("Хомбург", true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_49", new ItemData("Хомбург с принтами", true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null, null) },
            { "hat_m_50", new ItemData("Каска байкерская", true, 84, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_51", new ItemData("Каска байкерская #3", true, 89, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_m_52", new ItemData("Каска байкерская (металлик)", true, 90, new int[] { 0 }, null, null) },
            { "hat_m_53", new ItemData("Кепка модника", true, 96, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, null) },
            { "hat_m_54", new ItemData("Кепка автолюбителя", true, 154, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },

            { "hat_f_0", new ItemData("Шапка обычная", false, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_1", new ItemData("Шапка с принтами", false, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_2", new ItemData("Фуражка", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_3", new ItemData("Восьмиклинка", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_4", new ItemData("Шляпа фермерская", false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_f_5", new ItemData("Панама", false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_f_6", new ItemData("Шляпа пляжная", false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_f_7", new ItemData("Наушники", false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_8", new ItemData("Шляпа USA", false, 30, new int[] { 0 }, null, null) },
            { "hat_f_9", new ItemData("Цилиндр USA", false, 31, new int[] { 0 }, null, null) },
            { "hat_f_10", new ItemData("Шапка USA", false, 33, new int[] { 0 }, null, null) },
            { "hat_f_11", new ItemData("Каска болельщицы", false, 36, new int[] { 0, 1, 2, 3, 4, 5 }, null, null) },
            { "hat_f_12", new ItemData("Кепка Snapback", false, 53, new int[] { 0, 1 }, null, null) },
            { "hat_f_13", new ItemData("Кепка Snapback #2", false, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_14", new ItemData("Кепка дизайнерская", false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_15", new ItemData("Кепка SecuroServ", false, 64, new int[] { 0 }, null, null) },
            { "hat_f_16", new ItemData("Бандана байкерская", false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, null) },
            { "hat_f_17", new ItemData("Панама с принтами", false, 131, new int[] { 0, 1, 2, 3 }, null, null) },
            { "hat_f_18", new ItemData("Кепка модницы", false, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_f_19", new ItemData("Кепка Diamond", false, 134, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null, null) },
            { "hat_f_20", new ItemData("Шлем с принтами", false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_21", new ItemData("Шлем зеркальный", false, 50, new int[] { 0 }, null, null) },
            { "hat_f_22", new ItemData("Шлем обычный", false, 49, new int[] { 0 }, null, null) },
            { "hat_f_23", new ItemData("Шлем разноцветный", false, 72, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null, null) },
            { "hat_f_24", new ItemData("Шлем It's Go Time", false, 77, new int[] { 0, 1, 2, 3, 4 }, null, null) },
            { "hat_f_25", new ItemData("Шлем It's Go Time #2", false, 79, new int[] { 0, 1, 2, 3 }, null, null) },
            { "hat_f_26", new ItemData("Шлем модницы", false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null, null) },
            { "hat_f_27", new ItemData("Каска байкерская #2", false, 84, new int[] { 0 }, null, null) },
            { "hat_f_28", new ItemData("Каска байкерская #2 (козырек)", false, 85, new int[] { 0 }, null, null) },
            { "hat_f_29", new ItemData("Каска байкерская #2 (ирокез)", false, 86, new int[] { 0 }, null, null) },
            { "hat_f_30", new ItemData("Каска байкерская #2 (шипы)", false, 87, new int[] { 0 }, null, null) },
            { "hat_f_31", new ItemData("Кепка брендовая", false, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, null) },
            { "hat_f_32", new ItemData("Кепка брендовая #2", false, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null, null) },
            { "hat_f_33", new ItemData("Кепка брендовая #3", false, 129, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, null, null) },
            { "hat_f_34", new ItemData("Кепка цветная", false, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null, null) },
            { "hat_f_35", new ItemData("Панама разноцветная", false, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_36", new ItemData("Кепка современная", false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_37", new ItemData("Элегантная шляпа", false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_38", new ItemData("Элегантная панама", false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_39", new ItemData("Порк-пай", false, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_40", new ItemData("Котелок", false, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
            { "hat_f_41", new ItemData("Цилиндр", false, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
            { "hat_f_42", new ItemData("Трилби", false, 54, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null, null) },
            { "hat_f_43", new ItemData("Цилиндр USA #2", false, 32, new int[] { 0, 1 }, null, null) },
            { "hat_f_44", new ItemData("Кепка модницы #2", false, 95, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, null) },
            { "hat_f_45", new ItemData("Хомбург", false, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_46", new ItemData("Каска байкерская", false, 83, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_47", new ItemData("Каска байкерская #3", false, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, null) },
            { "hat_f_48", new ItemData("Каска байкерская (металлик)", false, 89, new int[] { 0 }, null, null) },
            { "hat_f_49", new ItemData("Кепка автолюбителя", false, 153, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null, null) },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            if (data.ExtraData != null)
                player.SetAccessories(Slot, Toggled ? data.ExtraData.Drawable : data.Drawable, data.Textures[variation]);
            else
                player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);

            pData.Hat = $"{this.ID}|{Var}|{(Toggled ? 1 : 0)}";
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
        new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {BestTorso}, {(ExtraData == null ? "null" : $"new Top.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, 0.3f, "prop_ld_shirt_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTorso = BestTorso;
                this.ExtraData = ExtraData;
            }

            public ItemData(bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : this(null, Sex, Drawable, Textures, BestTorso, ExtraData, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "top_m_0", new ItemData("Олимпийка", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_1", new ItemData("Куртка рейсера", true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_m_3", new ItemData("Худи открытое", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_4", new ItemData("Куртка рейсера #2", true, 37, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_5", new ItemData("Худи закрытое", true, 57, new int[] { 0 }, 4, null, null) },
            { "top_m_6", new ItemData("Рубашка с узорами", true, 105, new int[] { 0 }, 0, null, null) },
            { "top_m_7", new ItemData("Футболка регби", true, 81, new int[] { 0, 1, 2 }, 0, null, null) },
            { "top_m_8", new ItemData("Поло Н", true, 123, new int[] { 0, 1, 2 }, 0, null, null) },
            { "top_m_9", new ItemData("Футболка хоккейная", true, 128, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0, null, null) },
            { "top_m_10", new ItemData("Футболка на выпуск", true, 83, new int[] { 0, 1, 2, 3, 4 }, 0, null, null) },
            { "top_m_11", new ItemData("Поло длинное разноцветное", true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0, null, null) },
            { "top_m_12", new ItemData("Худи спортивное", true, 84, new int[] { 0, 1, 2, 3, 4, 5 }, 4, null, null) },
            { "top_m_13", new ItemData("Куртка брутальная", true, 61, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_14", new ItemData("Куртка кожаная открытая", true, 62, new int[] { 0 }, 14, null, null) },
            { "top_m_15", new ItemData("Куртка кожаная закрытая", true, 64, new int[] { 0 }, 14, null, null) },
            { "top_m_16", new ItemData("Олимпийка спортивная", true, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6, null, null) },
            { "top_m_17", new ItemData("Куртка с капюшоном", true, 69, new int[] { 0, 1, 2, 3, 4, 5 }, 14, new ItemData.ExtraData(68, 14), null) },
            { "top_m_18", new ItemData("Бомбер", true, 79, new int[] { 0 }, 6, null, null) },
            { "top_m_19", new ItemData("Поло рабочее", true, 235, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0, new ItemData.ExtraData(236, 0), null) },
            { "top_m_20", new ItemData("Худи обычное", true, 86, new int[] { 0, 1, 2, 3, 4 }, 4, null, null) },
            { "top_m_22", new ItemData("Бомбер с принтами", true, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(87, 14), null) },
            { "top_m_23", new ItemData("Куртка стёганая", true, 106, new int[] { 0 }, 14, null, null) },
            { "top_m_24", new ItemData("Рубашка с принтами", true, 234, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11, null, null) },
            { "top_m_25", new ItemData("Азиатский стиль", true, 107, new int[] { 0, 1, 2, 3, 4 }, 4, null, null) },
            { "top_m_26", new ItemData("Кожаная куртка преследователя", true, 110, new int[] { 0 }, 4, null, null) },
            { "top_m_27", new ItemData("Куртка спортивная", true, 113, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_m_28", new ItemData("Кимоно", true, 114, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, null, null) },
            { "top_m_29", new ItemData("Куртка с узорами", true, 118, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_m_30", new ItemData("Худи необычное", true, 121, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4, null, null) },
            { "top_m_31", new ItemData("Куртка обычная", true, 122, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, null, null) },
            { "top_m_32", new ItemData("Рубашка гангстера", true, 126, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 4, new ItemData.ExtraData(127, 14), null) },
            { "top_m_33", new ItemData("Куртка SecuroServ", true, 130, new int[] { 0 }, 14, null, null) },
            { "top_m_34", new ItemData("Тишка", true, 164, new int[] { 0, 1, 2 }, 0, null, null) },
            { "top_m_35", new ItemData("Куртка стёганая разноцветная", true, 136, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14, null, null) },
            { "top_m_36", new ItemData("Жилет вязаный", true, 137, new int[] { 0, 1, 2 }, 15, null, null) },
            { "top_m_37", new ItemData("Бомбер разноцветный", true, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4, null, null) },
            { "top_m_38", new ItemData("Куртка стритрейсера", true, 147, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 4, null, null) },
            { "top_m_39", new ItemData("Куртка гонщика", true, 148, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4, null, null) },
            { "top_m_40", new ItemData("Куртка с вырезом", true, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_m_41", new ItemData("Куртка с принтами", true, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4, null, null) },
            { "top_m_42", new ItemData("Куртка из старой кожи", true, 151, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_43", new ItemData("Куртка с принтами #2", true, 153, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, null, null) },
            { "top_m_44", new ItemData("Куртка стритрейсера #2", true, 154, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 12, null, null) },
            { "top_m_45", new ItemData("Байкерская жилетка", true, 157, new int[] { 0, 1, 2, 3 }, 112, null, null) },
            { "top_m_46", new ItemData("Жилетка расстёгнутая", true, 160, new int[] { 0, 1 }, 112, null, null) },
            { "top_m_47", new ItemData("Жилетка кожаная", true, 162, new int[] { 0, 1, 2, 3 }, 114, null, null) },
            { "top_m_48", new ItemData("Куртка чёрная", true, 163, new int[] { 0 }, 14, null, null) },
            { "top_m_49", new ItemData("Куртка кожаная расстёгнутая", true, 166, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_50", new ItemData("Пуховик", true, 167, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_51", new ItemData("Куртка джинсовая", true, 169, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_52", new ItemData("Жилет джинсовый", true, 170, new int[] { 0, 1, 2, 3 }, 112, null, null) },
            { "top_m_53", new ItemData("Джинсовка байкерская", true, 172, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_54", new ItemData("Жилетка джинсовая байкерская", true, 173, new int[] { 0, 1, 2, 3 }, 112, null, null) },
            { "top_m_55", new ItemData("Куртка кожаная байкерская", true, 174, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_m_56", new ItemData("Жилет кожаный байкерский", true, 175, new int[] { 0, 1, 2, 3 }, 114, null, null) },
            { "top_m_57", new ItemData("Ветровка с капюшоном", true, 184, new int[] { 0, 1, 2, 3 }, 6, new ItemData.ExtraData(185, 14), null) },
            { "top_m_58", new ItemData("Ветровка удлиненная", true, 187, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 6, new ItemData.ExtraData(204, 6), null) },
            { "top_m_59", new ItemData("Худи модника", true, 200, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(203, 4), null) },
            { "top_m_60", new ItemData("Жилет с капюшоном", true, 205, new int[] { 0, 1, 2, 3, 4 }, 114, new ItemData.ExtraData(202, 114), null) },
            { "top_m_61", new ItemData("Бомбер открытый", true, 230, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(229, 14), null) },
            { "top_m_62", new ItemData("Куртка с принтами #3", true, 251, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(253, 4), null) },
            { "top_m_63", new ItemData("Футболка модника", true, 193, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0, null, null) },
            { "top_m_64", new ItemData("Свитер вязаный", true, 258, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 6, null, null) },
            { "top_m_65", new ItemData("Бомбер модника открытый", true, 261, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_66", new ItemData("Спортивная водолазка", true, 255, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, null, null) },
            { "top_m_67", new ItemData("Куртка модника", true, 257, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6, null, null) },
            { "top_m_68", new ItemData("Толстовка модника", true, 259, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 6, null, null) },
            { "top_m_69", new ItemData("Худи модника #2", true, 262, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4, new ItemData.ExtraData(263, 4), null) },
            { "top_m_70", new ItemData("Куртка с ремнями", true, 264, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 6, null, null) },
            { "top_m_71", new ItemData("Футболка с логотипами", true, 271, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0, null, null) },
            { "top_m_72", new ItemData("Футболка модника #2", true, 273, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, 0, null, null) },
            { "top_m_73", new ItemData("Худи с принтами", true, 279, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 4, new ItemData.ExtraData(280, 4), null) },
            { "top_m_74", new ItemData("Толстовка с логотипами", true, 281, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6, null, null) },
            { "top_m_75", new ItemData("Футболка удлиненная", true, 282, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0, null, null) },
            { "top_m_76", new ItemData("Ветровка с принтами #2", true, 296, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(297, 4), null) },
            { "top_m_77", new ItemData("Толстовка с принтами", true, 308, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4, null, null) },
            { "top_m_78", new ItemData("Футболка с картинками", true, 313, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0, null, null) },
            { "top_m_79", new ItemData("Футболка с рисунками", true, 325, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 0, null, null) },
            { "top_m_80", new ItemData("Футболка с рисунками #2", true, 323, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0, null, null) },
            { "top_m_81", new ItemData("Свитер боевой", true, 50, new int[] { 0, 1, 2, 3, 4 }, 4, null, null) },
            { "top_m_82", new ItemData("Пиджак мятый", true, 59, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_83", new ItemData("Водолазка мятая", true, 67, new int[] { 0, 1, 2, 3 }, 4, null, null) },
            { "top_m_84", new ItemData("Ветровка осенняя", true, 85, new int[] { 0 }, 1, null, null) },
            { "top_m_85", new ItemData("Свитшот разноцветный", true, 89, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_m_86", new ItemData("Жилетка вязаная", true, 109, new int[] { 0 }, 15, null, null) },
            { "top_m_87", new ItemData("Водолазка разноцветная", true, 111, new int[] { 0, 1, 2, 3, 4, 5 }, 4, null, null) },
            { "top_m_88", new ItemData("Куртка с капюшоном", true, 124, new int[] { 0 }, 14, null, null) },
            { "top_m_89", new ItemData("Куртка закрытая с воротником", true, 125, new int[] { 0 }, 4, null, null) },
            { "top_m_90", new ItemData("Поло Liberty", true, 131, new int[] { 0 }, 0, new ItemData.ExtraData(132, 0), null) },
            { "top_m_92", new ItemData("Худи Liberty", true, 134, new int[] { 0, 1, 2 }, 4, null, null) },
            { "top_m_93", new ItemData("Рубашка с принтами", true, 135, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 0, null, null) },
            { "top_m_94", new ItemData("Пальто кожаное на ремнях", true, 138, new int[] { 0, 1, 2 }, 4, null, null) },
            { "top_m_97", new ItemData("Куртка танцора", true, 155, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_98", new ItemData("Жилет с ремнями", true, 158, new int[] { 0, 1, 2 }, 113, null, null) },
            { "top_m_99", new ItemData("Жилет кожаный", true, 159, new int[] { 0, 1 }, 114, null, null) },
            { "top_m_100", new ItemData("Толстовка гонщика", true, 165, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 6, null, null) },
            { "top_m_101", new ItemData("Толстовка уличная", true, 168, new int[] { 0, 1, 2 }, 12, null, null) },
            { "top_m_102", new ItemData("Худи уличное", true, 171, new int[] { 0, 1 }, 4, null, null) },
            { "top_m_103", new ItemData("Жилет STFU", true, 176, new int[] { 0 }, 114, null, null) },
            { "top_m_104", new ItemData("Жилет гонщика", true, 177, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 2, null, null) },
            { "top_m_105", new ItemData("Куртка кожаная с застёжками #2", true, 181, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_106", new ItemData("Парка разноцветная открытая", true, 189, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, new ItemData.ExtraData(188, 14), null) },
            { "top_m_107", new ItemData("Толстовка модника #3", true, 190, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 6, null, null) },
            { "top_m_108", new ItemData("Жилет стёганый", true, 223, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 2, null, null) },
            { "top_m_109", new ItemData("Куртка стёганая #2", true, 224, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 12, null, null) },
            { "top_m_110", new ItemData("Толстовка Class Of", true, 225, new int[] { 0, 1 }, 8, null, null) },
            { "top_m_112", new ItemData("Майка разноцветная #2", true, 237, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, null, null) },
            { "top_m_113", new ItemData("Футболка без рукавов", true, 238, new int[] { 0, 1, 2, 3, 4, 5 }, 2, null, null) },
            { "top_m_114", new ItemData("Поло разноцветное", true, 241, new int[] { 0, 1, 2, 3, 4, 5 }, 0, new ItemData.ExtraData(242, 0), null) },
            { "top_m_116", new ItemData("Куртка с фиолетовыми чертами", true, 329, new int[] { 0 }, 4, null, null) },
            { "top_m_117", new ItemData("Толстовка с фиолетовыми чертами", true, 330, new int[] { 0 }, 4, new ItemData.ExtraData(331, 4), null) },
            { "top_m_118", new ItemData("Спортивная толстовка", true, 332, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, null, null) },
            { "top_m_119", new ItemData("Фиолетовая удлиненная футболка", true, 334, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0, null, null) },
            { "top_m_120", new ItemData("Толстовка Baseball", true, 335, new int[] { 0, 1, 2, 3, 4, 5 }, 8, null, null) },
            { "top_m_121", new ItemData("Рубашка гангстера расстегнутая", true, 340, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 14, new ItemData.ExtraData(341, 1), null) },
            { "top_m_122", new ItemData("Толстовка с желтыми принтами", true, 342, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 4, null, null) },
            { "top_m_123", new ItemData("Бомбер с желтыми принтами", true, 344, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14, new ItemData.ExtraData(343, 14), null) },
            { "top_m_124", new ItemData("Футболка свободная Bigness", true, 350, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0, null, null) },
            { "top_m_126", new ItemData("Ветровка с капюшоном цветная", true, 217, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 6, null, null) },
            { "top_m_127", new ItemData("Джинсовая ветровка", true, 233, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, new ItemData.ExtraData(232, 14), null) },
            { "top_m_128", new ItemData("Футболка лёгкая", true, 351, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 0, null, null) },
            { "top_m_129", new ItemData("Худи лёгкое", true, 352, new int[] { 0, 1, 2 }, 4, new ItemData.ExtraData(353, 4), null) },
            { "top_m_130", new ItemData("Майка баскетбольная", true, 357, new int[] { 0, 1 }, 2, null, null) },
            { "top_m_132", new ItemData("Поло гольфиста", true, 382, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, new ItemData.ExtraData(383, 0), null) },
            { "top_m_133", new ItemData("Худи свободное", true, 384, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(385, 4), null) },
            { "top_m_134", new ItemData("Пиджак Блэйзер", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, new ItemData.ExtraData(10, 14), null) },
            { "top_m_135", new ItemData("Пиджак двубортный праздничный", true, 119, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_m_136", new ItemData("Жилет с цепочкой", true, 21, new int[] { 0, 1, 2, 3 }, 15, null, null) },
            { "top_m_137", new ItemData("Жилет обычный", true, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 15, null, null) },
            { "top_m_138", new ItemData("Пиджак Блэйзер с принтами", true, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 14, null, null) },
            { "top_m_139", new ItemData("Пиджак двубортный", true, 27, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_140", new ItemData("Пиджак однобортный", true, 28, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_141", new ItemData("Пиджак модника", true, 35, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 14, null, null) },
            { "top_m_142", new ItemData("Жилет USA", true, 45, new int[] { 0, 1, 2 }, 15, null, null) },
            { "top_m_143", new ItemData("Фрак USA", true, 46, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_144", new ItemData("Фрак", true, 58, new int[] { 0 }, 14, null, null) },
            { "top_m_145", new ItemData("Дубленка с мехом", true, 70, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_m_146", new ItemData("Пальто Редингот", true, 72, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_147", new ItemData("Куртка модника открытая", true, 74, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, new ItemData.ExtraData(75, 14), null) },
            { "top_m_148", new ItemData("Пальто Бушлат", true, 76, new int[] { 0, 1, 2, 3, 4 }, 14, null, null) },
            { "top_m_149", new ItemData("Пальто Кромби удлиненное", true, 77, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_150", new ItemData("Пиджак для встреч открытый", true, 99, new int[] { 0, 1, 2, 3, 4 }, 14, new ItemData.ExtraData(100, 14), null) },
            { "top_m_151", new ItemData("Пиджак хозяина", true, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, null, null) },
            { "top_m_152", new ItemData("Пальто строгое", true, 142, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_m_153", new ItemData("Пиджак строгий", true, 140, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14, null, null) },
            { "top_m_154", new ItemData("Пиджак хозяина #2", true, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, null, null) },
            { "top_m_155", new ItemData("Пиджак праздничный", true, 183, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_156", new ItemData("Пуховик модника", true, 191, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, null, null) },
            { "top_m_157", new ItemData("Пальто строгое разноцветное", true, 192, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_m_158", new ItemData("Пуховик модника #2", true, 269, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_m_159", new ItemData("Бомбер модника #2", true, 266, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 14, new ItemData.ExtraData(265, 14), null) },
            { "top_m_160", new ItemData("Рубашка модника", true, 260, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11, null, null) },
            { "top_m_161", new ItemData("Куртка спортивная с принтами", true, 298, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 4, null, null) },
            { "top_m_162", new ItemData("Рубашка на выпуск с принтами", true, 299, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 0, null, null) },
            { "top_m_163", new ItemData("Парка модника", true, 303, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(300, 6), null) },
            { "top_m_164", new ItemData("Парка модника с капюшоном", true, 301, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(302, 14), null) },
            { "top_m_165", new ItemData("Куртка на меху", true, 304, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_m_166", new ItemData("Худи Diamond", true, 305, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, new ItemData.ExtraData(306, 4), null) },
            { "top_m_167", new ItemData("Толстовка модника #2", true, 307, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6, null, null) },
            { "top_m_168", new ItemData("Пуховик модника #3", true, 309, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, null, null) },
            { "top_m_170", new ItemData("Жилет обычный #2", true, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 15, null, null) },
            { "top_m_171", new ItemData("Пиджак жениха", true, 20, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_172", new ItemData("Пиджак Блейзер #2 открытый", true, 23, new int[] { 0, 1, 2, 3 }, 14, null, null) },
            { "top_m_173", new ItemData("Жилет праздничный", true, 40, new int[] { 0, 1 }, 15, null, null) },
            { "top_m_174", new ItemData("Пиджак с узорами", true, 103, new int[] { 0 }, 14, null, null) },
            { "top_m_175", new ItemData("Пальто закрытое", true, 112, new int[] { 0 }, 14, null, null) },
            { "top_m_176", new ItemData("Пальто серое", true, 115, new int[] { 0 }, 14, null, null) },
            { "top_m_177", new ItemData("Жилет с принтами", true, 120, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15, null, null) },
            { "top_m_178", new ItemData("Куртка с застежками", true, 161, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_m_179", new ItemData("Куртка с мехом", true, 240, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_180", new ItemData("Кимоно с принтами", true, 310, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 14, null, null) },
            { "top_m_181", new ItemData("Кожаная разноцветная куртка", true, 338, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_m_182", new ItemData("Рубашка солидная с принтами", true, 348, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 1, new ItemData.ExtraData(349, 1), null) },
            { "top_m_183", new ItemData("Свитшот модника", true, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6, null, null) },
            { "top_m_184", new ItemData("Рубашка гавайская", true, 355, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 184, new ItemData.ExtraData(354, 184), null) },
            { "top_m_185", new ItemData("Свитшот тусовщика", true, 358, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 6, null, null) },
            { "top_m_186", new ItemData("Бомбер с тигром", true, 360, new int[] { 0 }, 14, new ItemData.ExtraData(359, 14), null) },
            { "top_m_187", new ItemData("Бомбер Cayo Perico", true, 361, new int[] { 0 }, 4, null, null) },
            { "top_m_188", new ItemData("Жилет на молнии брендированный", true, 369, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 2, null, null) },
            { "top_m_189", new ItemData("Куртка дутая брендированная", true, 370, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 12, null, null) },
            { "top_m_190", new ItemData("Куртка автолюбителя", true, 371, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4, null, null) },
            { "top_m_191", new ItemData("Худи автолюбителя", true, 374, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 4, new ItemData.ExtraData(373, 4), null) },
            { "top_m_192", new ItemData("Регби брендированная", true, 376, new int[] { 0, 1, 2 }, 14, new ItemData.ExtraData(375, 14), null) },
            { "top_m_193", new ItemData("Футболка автолюбителя", true, 377, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 0, null, null) },
            { "top_m_194", new ItemData("Ветровка автолюбителя", true, 378, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 12, null, null) },
            { "top_m_195", new ItemData("Кожанка глянцевая", true, 381, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, new ItemData.ExtraData(379, 14), null) },
            { "top_m_196", new ItemData("Кожанка на молнии", true, 387, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 14, new ItemData.ExtraData(386, 14), null) },
            { "top_m_197", new ItemData("Куртка Broker", true, 390, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, new ItemData.ExtraData(388, 14), null) },
            { "top_m_198", new ItemData("Куртка Sweatbox", true, 391, new int[] { 0, 1, 2 }, 14, new ItemData.ExtraData(389, 14), null) },
            { "top_m_199", new ItemData("null", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, new ItemData.ExtraData(30, 14), null) },
            { "top_m_200", new ItemData("null", true, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, new ItemData.ExtraData(32, 14), null) },
            { "top_f_1", new ItemData("Бейсбольная рубашка", false, 161, new int[] { 0, 1, 2 }, 9, null, null) },
            { "top_f_2", new ItemData("Майка обычная", false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 4, null, null) },
            { "top_f_3", new ItemData("Джинсовка", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_4", new ItemData("Косуха", false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_5", new ItemData("Куртка спортивная", false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 7, null, null) },
            { "top_f_6", new ItemData("Поло экзотическое", false, 17, new int[] { 0 }, 9, null, null) },
            { "top_f_8", new ItemData("Джинсовка с рукавами", false, 31, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 5, null, null) },
            { "top_f_10", new ItemData("Топик с принтами", false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 4, null, null) },
            { "top_f_11", new ItemData("Летний сарафан", false, 37, new int[] { 0, 1, 2, 3, 4, 5 }, 4, null, null) },
            { "top_f_12", new ItemData("Свитер боевой", false, 43, new int[] { 0, 1, 2, 3, 4 }, 3, null, null) },
            { "top_f_13", new ItemData("Кофта с капюшоном", false, 50, new int[] { 0 }, 3, null, null) },
            { "top_f_14", new ItemData("Куртка брутальная", false, 54, new int[] { 0, 1, 2, 3 }, 3, null, null) },
            { "top_f_15", new ItemData("Куртка кожаная", false, 55, new int[] { 0 }, 3, null, null) },
            { "top_f_16", new ItemData("Куртка с капюшоном", false, 63, new int[] { 0, 1, 2, 3, 4, 5 }, 3, null, null) },
            { "top_f_17", new ItemData("Куртка кожаная с ремнем", false, 69, new int[] { 0 }, 5, null, null) },
            { "top_f_18", new ItemData("Кофта модницы", false, 71, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1, null, null) },
            { "top_f_19", new ItemData("Бомбер с волком", false, 72, new int[] { 0 }, 1, null, null) },
            { "top_f_20", new ItemData("Рубашка на выпуск", false, 76, new int[] { 0, 1, 2, 3, 4 }, 9, null, null) },
            { "top_f_21", new ItemData("Худи обычное", false, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3, null, null) },
            { "top_f_22", new ItemData("Кофта обычная", false, 79, new int[] { 0, 1, 2, 3 }, 1, null, null) },
            { "top_f_23", new ItemData("Бомбер с принтами", false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_24", new ItemData("Поло обычное #2", false, 86, new int[] { 0, 1, 2 }, 9, null, null) },
            { "top_f_25", new ItemData("Поло с узорами", false, 96, new int[] { 0 }, 9, null, null) },
            { "top_f_26", new ItemData("Куртка стеганая", false, 97, new int[] { 0 }, 6, null, null) },
            { "top_f_27", new ItemData("Азиатский стиль", false, 98, new int[] { 0, 1, 2, 3, 4 }, 3, null, null) },
            { "top_f_28", new ItemData("Рубашка гангстера", false, 121, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 3, null, null) },
            { "top_f_29", new ItemData("Жилет вязаный", false, 100, new int[] { 0 }, 6, null, null) },
            { "top_f_30", new ItemData("Кимоно", false, 105, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 0, null, null) },
            { "top_f_31", new ItemData("Куртка спортивная", false, 106, new int[] { 0, 1, 2, 3 }, 6, null, null) },
            { "top_f_32", new ItemData("Рубашка с принтами", false, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0, null, null) },
            { "top_f_33", new ItemData("Куртка рейсера", false, 110, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, null, null) },
            { "top_f_34", new ItemData("Платье с узорами", false, 116, new int[] { 0, 1, 2 }, 11, null, null) },
            { "top_f_35", new ItemData("Поло H", false, 119, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_f_36", new ItemData("Футболка H", false, 125, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_f_37", new ItemData("Куртка SecuroServ закрытая", false, 127, new int[] { 0 }, 3, null, null) },
            { "top_f_38", new ItemData("Рубашка с принтами #2", false, 132, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 9, null, null) },
            { "top_f_39", new ItemData("Куртка стеганая разноцветная", false, 133, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 6, null, null) },
            { "top_f_40", new ItemData("Олимпийка", false, 138, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6, null, null) },
            { "top_f_41", new ItemData("Бомбер разноцветный", false, 140, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, null, null) },
            { "top_f_42", new ItemData("Куртка стритрейсера", false, 144, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, null, null) },
            { "top_f_43", new ItemData("Худи необычное", false, 123, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_44", new ItemData("Куртка гонщика", false, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_45", new ItemData("Куртка с вырезом", false, 146, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 7, null, null) },
            { "top_f_46", new ItemData("Куртка с принтами", false, 147, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_47", new ItemData("Куртка с принтами #2", false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_48", new ItemData("Куртка стритрейсера #2", false, 151, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 1, null, null) },
            { "top_f_49", new ItemData("Байкерская жилетка", false, 154, new int[] { 0, 1, 2, 3 }, 129, null, null) },
            { "top_f_50", new ItemData("Жилетка расстегнутая", false, 157, new int[] { 0, 1 }, 132, null, null) },
            { "top_f_51", new ItemData("Жилетка кожаная", false, 159, new int[] { 0, 1, 2, 3 }, 131, null, null) },
            { "top_f_52", new ItemData("Куртка черная", false, 160, new int[] { 0 }, 5, null, null) },
            { "top_f_53", new ItemData("Куртка кожаная расстегнутая", false, 163, new int[] { 0, 1, 2, 3, 4, 5 }, 5, null, null) },
            { "top_f_54", new ItemData("Пуховик", false, 164, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_55", new ItemData("Куртка джинсовая", false, 166, new int[] { 0, 1, 2, 3 }, 5, null, null) },
            { "top_f_56", new ItemData("Жилет джинсовый", false, 167, new int[] { 0, 1, 2, 3 }, 129, null, null) },
            { "top_f_57", new ItemData("Джинсовый топ", false, 171, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 153, null, null) },
            { "top_f_58", new ItemData("Джинсовка байкерская", false, 174, new int[] { 0, 1, 2, 3 }, 5, null, null) },
            { "top_f_59", new ItemData("Жилетка джинсовая байкерская", false, 175, new int[] { 0, 1, 2, 3 }, 129, null, null) },
            { "top_f_60", new ItemData("Куртка с застежками", false, 158, new int[] { 0, 1, 2, 3 }, 7, null, null) },
            { "top_f_61", new ItemData("Куртка кожаная байкерская", false, 176, new int[] { 0, 1, 2, 3 }, 7, null, null) },
            { "top_f_62", new ItemData("Жилет кожаный байкерский", false, 177, new int[] { 0, 1, 2, 3 }, 131, null, null) },
            { "top_f_63", new ItemData("Ветровка с капюшоном", false, 186, new int[] { 0, 1, 2, 3 }, 3, null, null) },
            { "top_f_64", new ItemData("Кофта с принтами", false, 192, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 1, null, null) },
            { "top_f_65", new ItemData("Топик модницы", false, 195, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 153, null, null) },
            { "top_f_66", new ItemData("Худи модницы", false, 202, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_68", new ItemData("Жилет стеганый с принтами", false, 233, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 11, null, null) },
            { "top_f_69", new ItemData("Куртка стегеная с принтами", false, 234, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6, null, null) },
            { "top_f_70", new ItemData("Куртка с принтами #3", false, 251, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_71", new ItemData("Бомбер открытый", false, 239, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_72", new ItemData("Спортивная ветровка", false, 259, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_73", new ItemData("Рейсерский стиль", false, 262, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 7, null, null) },
            { "top_f_74", new ItemData("Бомбер модницы", false, 270, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_75", new ItemData("Свитер вязаный", false, 267, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 1, null, null) },
            { "top_f_76", new ItemData("Куртка модницы", false, 266, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 6, null, null) },
            { "top_f_77", new ItemData("Толстовка модницы", false, 268, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 1, null, null) },
            { "top_f_78", new ItemData("Худи модницы #2", false, 271, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 3, null, null) },
            { "top_f_79", new ItemData("Футболка с логотипами", false, 280, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14, null, null) },
            { "top_f_80", new ItemData("Футболка модницы #2", false, 286, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, 14, null, null) },
            { "top_f_81", new ItemData("Худи с принтами", false, 292, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 3, null, null) },
            { "top_f_82", new ItemData("Толстовка с логотипами", false, 294, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 1, null, null) },
            { "top_f_83", new ItemData("Футболка удлиненная", false, 295, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_f_84", new ItemData("Толстовка с принтами", false, 319, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 3, null, null) },
            { "top_f_85", new ItemData("Кимоно с принтами", false, 321, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0, null, null) },
            { "top_f_86", new ItemData("Футболка с картинками", false, 324, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14, null, null) },
            { "top_f_87", new ItemData("Футболка с картинками #2", false, 338, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 14, null, null) },
            { "top_f_88", new ItemData("Футболка с рисунками", false, 337, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 14, null, null) },
            { "top_f_89", new ItemData("Футболка с рисунками #2", false, 335, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 14, null, null) },
            { "top_f_90", new ItemData("Куртка на ремнях", false, 273, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_91", new ItemData("Топик с принтами", false, 284, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15, null, null) },
            { "top_f_92", new ItemData("Спортивная водолазка", false, 264, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_93", new ItemData("Платье с бахромой", false, 21, new int[] { 0, 1, 2, 3, 4, 5 }, 16, null, null) },
            { "top_f_94", new ItemData("Топик обычный", false, 74, new int[] { 0, 1, 2 }, 15, null, null) },
            { "top_f_95", new ItemData("Куртка весенняя", false, 77, new int[] { 0 }, 6, null, null) },
            { "top_f_97", new ItemData("Платье с узорами #2", false, 112, new int[] { 0, 1, 2 }, 11, null, null) },
            { "top_f_98", new ItemData("Платье с узорами #3", false, 113, new int[] { 0, 1, 2 }, 11, null, null) },
            { "top_f_99", new ItemData("Платье с узорами #4", false, 114, new int[] { 0, 1, 2 }, 11, null, null) },
            { "top_f_100", new ItemData("Футболка хоккейная", false, 126, new int[] { 0, 1, 2 }, 14, null, null) },
            { "top_f_101", new ItemData("Худи Liberty", false, 131, new int[] { 0, 1, 2 }, 3, null, null) },
            { "top_f_102", new ItemData("Футболка Libery на выпуск", false, 128, new int[] { 0 }, 14, null, null) },
            { "top_f_103", new ItemData("Джинсовка", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_104", new ItemData("Куртка JackCandy", false, 148, new int[] { 0, 1, 2, 3, 4, 5 }, 3, null, null) },
            { "top_f_105", new ItemData("Жилет гонщицы", false, 155, new int[] { 0, 1, 2 }, 130, null, null) },
            { "top_f_106", new ItemData("Жилет кожаный", false, 156, new int[] { 0, 1 }, 131, null, null) },
            { "top_f_107", new ItemData("Куртка кожаная с капюшоном", false, 165, new int[] { 0, 1, 2 }, 0, null, null) },
            { "top_f_108", new ItemData("Майка порезанная", false, 168, new int[] { 0, 1, 2, 3, 4, 5 }, 161, null, null) },
            { "top_f_109", new ItemData("Топик порезанный", false, 169, new int[] { 0, 1, 2, 3, 4, 5 }, 153, null, null) },
            { "top_f_110", new ItemData("Майка порезанная #2", false, 170, new int[] { 0, 1, 2, 3, 4, 5 }, 15, null, null) },
            { "top_f_111", new ItemData("Худи обычное", false, 172, new int[] { 0, 1 }, 3, null, null) },
            { "top_f_112", new ItemData("Жилет STFU", false, 178, new int[] { 0 }, 131, null, null) },
            { "top_f_113", new ItemData("Ветровка разноцветная", false, 190, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 3, null, null) },
            { "top_f_114", new ItemData("Ветровка удлиненная", false, 189, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 7, null, null) },
            { "top_f_115", new ItemData("Худи без рукавов", false, 207, new int[] { 0, 1, 2, 3, 4 }, 131, null, null) },
            { "top_f_117", new ItemData("Ветровка разноцветная с капюшоном", false, 227, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 3, null, null) },
            { "top_f_118", new ItemData("Блузка Class Of", false, 235, new int[] { 0, 1 }, 9, null, null) },
            { "top_f_119", new ItemData("Поло с принтами", false, 244, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9, null, null) },
            { "top_f_120", new ItemData("Поло с брендами", false, 246, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 14, null, null) },
            { "top_f_121", new ItemData("Майка разноцветная", false, 247, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 4, null, null) },
            { "top_f_122", new ItemData("Поло обычное", false, 249, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_f_123", new ItemData("Спортивная толстовка", false, 347, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_124", new ItemData("Фиолетовая удлиненная футболка", false, 349, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 14, null, null) },
            { "top_f_125", new ItemData("Толстовка Base Ball", false, 350, new int[] { 0, 1, 2, 3, 4, 5 }, 9, null, null) },
            { "top_f_126", new ItemData("Рубашка гангстера #2", false, 354, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 5, null, null) },
            { "top_f_127", new ItemData("Рубашка гангстера #3", false, 356, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3, null, null) },
            { "top_f_128", new ItemData("Толстовка с желтыми принтами", false, 361, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 3, null, null) },
            { "top_f_129", new ItemData("Бомбер с желтыми принтами", false, 363, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 5, null, null) },
            { "top_f_130", new ItemData("Бомбер с фиолетовыми чертами", false, 344, new int[] { 0 }, 3, null, null) },
            { "top_f_131", new ItemData("Толстовка с фиолетовыми чертами", false, 345, new int[] { 0 }, 3, null, null) },
            { "top_f_132", new ItemData("Поло фиолетовое Bigness", false, 368, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 14, null, null) },
            { "top_f_133", new ItemData("Футболка модницы #3", false, 369, new int[] { 0, 1, 2, 3, 4 }, 14, null, null) },
            { "top_f_134", new ItemData("Худи легкое", false, 370, new int[] { 0, 1, 2 }, 3, null, null) },
            { "top_f_135", new ItemData("Футболка легкая", false, 377, new int[] { 0, 1, 2, 3, 4, 5 }, 14, null, null) },
            { "top_f_137", new ItemData("Поло гольфиста", false, 400, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 14, null, null) },
            { "top_f_138", new ItemData("Худи свободное", false, 407, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_139", new ItemData("Джемпер с V вырезом", false, 406, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 7, null, null) },
            { "top_f_140", new ItemData("Жакет закрытый", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_141", new ItemData("Пиджак строгий", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6, null, null) },
            { "top_f_142", new ItemData("Пиджак с принтами", false, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 6, null, null) },
            { "top_f_143", new ItemData("Лифчик с принтами", false, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 15, null, null) },
            { "top_f_144", new ItemData("Лифчик модницы", false, 279, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 15, null, null) },
            { "top_f_145", new ItemData("Жакет модницы", false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_146", new ItemData("Жилет обычный", false, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0, null, null) },
            { "top_f_147", new ItemData("Жакет из кожи", false, 35, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_148", new ItemData("Фрак USA", false, 39, new int[] { 0 }, 5, null, null) },
            { "top_f_149", new ItemData("Фрак", false, 51, new int[] { 0 }, 3, null, null) },
            { "top_f_150", new ItemData("Пиджак Блэйзер", false, 52, new int[] { 0, 1, 2, 3 }, 3, null, null) },
            { "top_f_151", new ItemData("Пальто шинель", false, 64, new int[] { 0, 1, 2, 3, 4 }, 5, null, null) },
            { "top_f_152", new ItemData("Куртка с мехом", false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_153", new ItemData("Пиджак на поясе", false, 66, new int[] { 0, 1, 2, 3 }, 5, null, null) },
            { "top_f_154", new ItemData("Пальто тренчкот", false, 70, new int[] { 0, 1, 2, 3, 4 }, 5, null, null) },
            { "top_f_155", new ItemData("Пиджак приталенный закрытый", false, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 3, null, null) },
            { "top_f_156", new ItemData("Рубашка модницы", false, 83, new int[] { 0, 1, 2, 3, 4, 5, 6 }, 0, null, null) },
            { "top_f_157", new ItemData("Пиджак строгий #2", false, 90, new int[] { 0, 1, 2, 3, 4 }, 3, null, null) },
            { "top_f_158", new ItemData("Пиджак хозяйки", false, 99, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 7, null, null) },
            { "top_f_159", new ItemData("Кожаный топ", false, 173, new int[] { 0 }, 4, null, null) },
            { "top_f_160", new ItemData("Лифчик модницы #2", false, 101, new int[] { 0, 1, 2, 3, 4, 5 }, 15, null, null) },
            { "top_f_161", new ItemData("Пиджак праздничный", false, 185, new int[] { 0, 1, 2, 3, 4, 5 }, 3, null, null) },
            { "top_f_162", new ItemData("Пуховик модницы", false, 193, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, null, null) },
            { "top_f_163", new ItemData("Пальто строгое расстегнутое", false, 194, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 6, null, null) },
            { "top_f_164", new ItemData("Поло расстегнутое", false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 9, null, null) },
            { "top_f_165", new ItemData("Пиджак в камуфляжной расцветке", false, 34, new int[] { 0 }, 6, null, null) },
            { "top_f_166", new ItemData("Пиджак Блэйзер", false, 52, new int[] { 0, 1, 2, 3 }, 3, null, null) },
            { "top_f_167", new ItemData("Пиджак строгий #2", false, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 3, null, null) },
            { "top_f_168", new ItemData("Куртка кожаная коричневая", false, 102, new int[] { 0 }, 3, null, null) },
            { "top_f_169", new ItemData("Пиджак на заклепках", false, 137, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, 7, null, null) },
            { "top_f_170", new ItemData("Пальто удлиненное", false, 139, new int[] { 0, 1, 2 }, 6, null, null) },
            { "top_f_171", new ItemData("Пижама с принтами", false, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 0, null, null) },
            { "top_f_172", new ItemData("Пижама хозяйки", false, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 7, null, null) },
            { "top_f_173", new ItemData("Куртка с мехом", false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_174", new ItemData("Пиджак солидный (с платком)", false, 305, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 3, null, null) },
            { "top_f_175", new ItemData("Ветровка Bigness", false, 307, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_176", new ItemData("Толстовка с принтами #2", false, 318, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, 1, null, null) },
            { "top_f_177", new ItemData("Кожаная разноцветная куртка", false, 353, new int[] { 0, 1, 2, 3, 4, 5 }, 3, null, null) },
            { "top_f_178", new ItemData("Рубашка солидная с принтами", false, 366, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 3, null, null) },
            { "top_f_179", new ItemData("Пуховик модницы #2", false, 278, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 5, null, null) },
            { "top_f_180", new ItemData("Бомбер модницы #2", false, 274, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, 3, null, null) },
            { "top_f_181", new ItemData("Рубашка модницы", false, 269, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9, null, null) },
            { "top_f_182", new ItemData("Куртка спортивная с принтами", false, 309, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 }, 3, null, null) },
            { "top_f_183", new ItemData("Рубашка на выпуск с принтами", false, 310, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 9, null, null) },
            { "top_f_184", new ItemData("Парка модницы", false, 311, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_185", new ItemData("Парка модницы #2", false, 314, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, null, null) },
            { "top_f_186", new ItemData("Куртка на меху", false, 315, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 5, null, null) },
            { "top_f_187", new ItemData("Худи Diamond", false, 316, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 3, null, null) },
            { "top_f_188", new ItemData("Пуховик модницы #3", false, 320, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 5, null, null) },
            { "top_f_189", new ItemData("Рубашка с рукавами", false, 332, new int[] { 0 }, 3, null, null) },
            { "top_f_191", new ItemData("Платье модницы", false, 322, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11, null, null) },
            { "top_f_192", new ItemData("Платье модницы #2", false, 323, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 11, null, null) },
            { "top_f_193", new ItemData("Пиджак разноцветный", false, 339, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, 3, null, null) },
            { "top_f_194", new ItemData("Рубашка гавайская", false, 373, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 229, null, null) },
            { "top_f_195", new ItemData("Худи тусовщицы", false, 376, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1, null, null) },
            { "top_f_196", new ItemData("Бомбер с тигром", false, 379, new int[] { 0 }, 5, null, null) },
            { "top_f_197", new ItemData("Бомбер Cayo Perico", false, 380, new int[] { 0 }, 3, null, null) },
            { "top_f_198", new ItemData("Жилет на молнии брендированный", false, 388, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 11, null, null) },
            { "top_f_199", new ItemData("Куртка дутая брендированная", false, 389, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 6, null, null) },
            { "top_f_200", new ItemData("Куртка автолюбительницы", false, 390, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 3, null, null) },
            { "top_f_201", new ItemData("Худи автолюбительнцы", false, 392, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 3, null, null) },
            { "top_f_202", new ItemData("Регби брендированная", false, 394, new int[] { 0, 1, 2 }, 3, null, null) },
            { "top_f_203", new ItemData("Футболка автолюбительнцы", false, 395, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, 14, null, null) },
            { "top_f_204", new ItemData("Ветровка автолюбительницы", false, 396, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 1, null, null) },
            { "top_f_205", new ItemData("Кожанка глянцевая", false, 399, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 5, null, null) },
            { "top_f_206", new ItemData("Кожанка на молнии", false, 403, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, 5, null, null) },
            { "top_f_207", new ItemData("Куртка Broker", false, 411, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 5, null, null) },
            { "top_f_208", new ItemData("Куртка Sweatbox", false, 412, new int[] { 0, 1, 2 }, 5, null, null) },
            { "top_f_209", new ItemData("Блуза без рукавов", false, 404, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, 12, null, null) },
            { "top_f_210", new ItemData("Блуза без рукавов #2", false, 405, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, 12, null, null) },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            if (Toggled)
            {
                player.SetClothes(Slot, data.ExtraData.Drawable, data.Textures[variation]);
                player.SetClothes(Gloves.Slot, data.ExtraData.BestTorso, 0);
            }
            else
            {
                player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
                player.SetClothes(Gloves.Slot, data.BestTorso, 0);
            }

            if (pData.Armour != null)
            {
                var aData = pData.Armour.Data;

                var aVar = pData.Armour.Var;

                if (Data.Sex != pData.Sex)
                {
                    aData = pData.Armour.SexAlternativeData;

                    if (aData == null)
                        return;

                    if (aVar >= aData.Textures.Length)
                        aVar = aData.Textures.Length;
                }

                player.SetClothes(9, aData.DrawableTop, aData.Textures[aVar]);
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

                var aVar = pData.Armour.Var;

                if (Data.Sex != pData.Sex)
                {
                    aData = pData.Armour.SexAlternativeData;

                    if (aData == null)
                        return;

                    if (aVar >= aData.Textures.Length)
                        aVar = aData.Textures.Length;
                }

                player.SetClothes(9, aData.Drawable, aData.Textures[aVar]);
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
        new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public Top.ItemData BestTop { get; set; }

            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(BestTop == null ? "null" : $"new Top.ItemData({BestTop.Sex.ToString().ToLower()}, {BestTop.Drawable}, new int[] {{ {string.Join(", ", BestTop.Textures)} }}, {BestTorso}, {(BestTop.ExtraData == null ? "null" : $"new Under.ItemData.ExtraData({BestTop.ExtraData.Drawable}, {BestTop.ExtraData.BestTorso})")}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")})")} , {BestTorso}, {(ExtraData == null ? "null" : $"new Under.ItemData.ExtraData({ExtraData.Drawable}, {ExtraData.BestTorso})")}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, Top.ItemData BestTop, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, 0.2f, "prop_ld_tshirt_02", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTop = BestTop;
                this.ExtraData = ExtraData;

                this.BestTorso = BestTorso;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "under_m_0", new ItemData("Футболка стандартная", true, 0, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 }, new Top.ItemData(true, 0, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 }, 0), 4, new ItemData.ExtraData(2, 4), null) },
            { "under_m_1", new ItemData("Майка стандартная", true, 5, new int[] { 0, 1, 2, 7 }, new Top.ItemData(true, 5, new int[] { 0, 1, 2, 7 }, 5), 6, null, null) },
            { "under_m_2", new ItemData("Футболка обычная", true, 1, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 11, 12, 14 }, new Top.ItemData(true, 1, new int[] { 0, 1, 3, 4, 5, 6, 7, 8, 11, 12, 14 }, 0), 1, new ItemData.ExtraData(14, 1), null) },
            { "under_m_3", new ItemData("Кофта обычная", true, 8, new int[] { 0, 10, 13, 14 }, new Top.ItemData(true, 8, new int[] { 0, 10, 13, 14 }, 11), 4, null, null) },
            { "under_m_4", new ItemData("Поло обычное", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 0), 1, null, null) },
            { "under_m_5", new ItemData("Рубашка свободная", true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 1), 1, new ItemData.ExtraData(64, 1), null) },
            { "under_m_6", new ItemData("Рубашка на выпуск", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 4, new ItemData.ExtraData(30, 4), null) },
            { "under_m_7", new ItemData("Майка обычная", true, 17, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(true, 17, new int[] { 0, 1, 2, 3, 4, 5 }, 5), 6, null, null) },
            { "under_m_8", new ItemData("Свитшот", true, 41, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(true, 38, new int[] { 0, 1, 2, 3, 4 }, 8), 4, null, null) },
            { "under_m_9", new ItemData("Поло с рисунками", true, 42, new int[] { 0, 1 }, new Top.ItemData(true, 39, new int[] { 0, 1 }, 0), 1, null, null) },
            { "under_m_10", new ItemData("Рубашка на выпуск", true, 43, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 41, new int[] { 0, 1, 2, 3 }, 12), 1, null, null) },
            { "under_m_11", new ItemData("Рубашка с подтяжками", true, 45, new int[] { 0 }, new Top.ItemData(true, 42, new int[] { 0 }, 11, new ItemData.ExtraData(43, 11)), 4, new ItemData.ExtraData(46, 4), null) },
            { "under_m_12", new ItemData("Футболка обычная #2", true, 53, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 47, new int[] { 0, 1, 2, 3 }, 0), 4, new ItemData.ExtraData(54, 4), null) },
            { "under_m_13", new ItemData("Рубашка приталенная", true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 11, new ItemData.ExtraData(7, 11), null) },
            { "under_m_14", new ItemData("Рубашка приталенная #2", true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Top.ItemData(true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 11), 4, null, null) },
            { "under_m_15", new ItemData("Футболка золотая", true, 67, new int[] { 0 }, new Top.ItemData(true, 71, new int[] { 0 }, 0), 4, null, null) },
            { "under_m_16", new ItemData("Футболка модника", true, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, new Top.ItemData(true, 73, new int[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 }, 0), 4, null, null) },
            { "under_m_17", new ItemData("Рубашка с жилетом", true, 4, new int[] { 0, 1, 2 }, null, 4, new ItemData.ExtraData(3, 4), null) },
            { "under_m_18", new ItemData("Рубашка обычная", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 4, new ItemData.ExtraData(11, 4), null) },
            { "under_m_19", new ItemData("Рубашка с разноцветным жилетом", true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null, 4, new ItemData.ExtraData(25, 4), null) },
            { "under_m_20", new ItemData("Рубашка под смокинг", true, 33, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null, 4, new ItemData.ExtraData(34, 4), null) },
            { "under_m_21", new ItemData("Рубашка с жилетом USA", true, 52, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, null, 4, new ItemData.ExtraData(51, 4), null) },
            { "under_m_22", new ItemData("Рубашка нараспашку", true, 69, new int[] { 0, 1, 2, 3, 4 }, null, 14, null, null) },
            { "under_m_24", new ItemData("Рубашка под жилетку #2", true, 22, new int[] { 0, 1, 2, 3, 4 }, null, 4, null, null) },
            { "under_m_25", new ItemData("Рубашка под жилетку #3", true, 93, new int[] { 0, 1 }, null, 11, null, null) },
            { "under_m_26", new ItemData("Рубашка под жилетку #4", true, 158, new int[] { 0 }, new Top.ItemData(true, 322, new int[] { 0 }, 1, new ItemData.ExtraData(321, 1)), 4, new ItemData.ExtraData(157, 4), null) },
            { "under_m_27", new ItemData("null", true, 79, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(true, 152, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 4, new ItemData.ExtraData(80, 4), null) },
            { "under_m_28", new ItemData("null", true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(true, 235, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 0), 1, new ItemData.ExtraData(110, 1), null) },
            { "under_m_29", new ItemData("null", true, 187, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(true, 392, new int[] { 0, 1, 2, 3, 4 }, 0), 4, new ItemData.ExtraData(188, 4), null) },
            { "under_m_30", new ItemData("null", true, 168, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, new Top.ItemData(true, 345, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 0), 4, new ItemData.ExtraData(169, 4), null) },
            { "under_m_31", new ItemData("null", true, 47, new int[] { 0, 1, 2, 3 }, new Top.ItemData(true, 44, new int[] { 0, 1, 2, 3 }, 0), 4, new ItemData.ExtraData(48, 4), null) },
            { "under_m_32", new ItemData("null", true, 16, new int[] { 0, 1, 2 }, new Top.ItemData(true, 16, new int[] { 0, 1, 2 }, 0), 1, new ItemData.ExtraData(18, 1), null) },
            { "under_m_33", new ItemData("null", true, 72, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(true, 139, new int[] { 0, 1, 2, 3, 4, 5 }, 4), 4, new ItemData.ExtraData(71, 4), null) },
            { "under_m_34", new ItemData("null", true, 76, new int[] { 0, 1, 2, 3, 4, 5, 7, 8 }, new Top.ItemData(true, 146, new int[] { 0, 1, 2, 3, 4, 5, 7, 8 }, 0), 4, new ItemData.ExtraData(77, 4), null) },
            { "under_m_35", new ItemData("null", true, 178, new int[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, 4, new ItemData.ExtraData(179, 4), null) },
            { "under_f_0", new ItemData("Майка дизайнерская", false, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, new Top.ItemData(false, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, 12), 0, null, null) },
            { "under_f_1", new ItemData("Футболка на выпуск", false, 26, new int[] { 0, 1, 2 }, new Top.ItemData(false, 30, new int[] { 0, 1, 2 }, 2), 0, null, null) },
            { "under_f_2", new ItemData("Майка с принтами", false, 27, new int[] { 0, 1, 2 }, new Top.ItemData(false, 32, new int[] { 0, 1, 2 }, 4), 0, null, null) },
            { "under_f_3", new ItemData("Футболка облегающая", false, 71, new int[] { 0, 1, 2 }, new Top.ItemData(false, 73, new int[] { 0, 1, 2 }, 14), 0, null, null) },
            { "under_f_4", new ItemData("Футболка USA", false, 31, new int[] { 0, 1 }, new Top.ItemData(false, 40, new int[] { 0, 1 }, 2), 0, null, null) },
            { "under_f_5", new ItemData("Свитшот", false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(false, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 1), 0, null, null) },
            { "under_f_6", new ItemData("Кофта обычная", false, 61, new int[] { 0, 1, 2, 3 }, new Top.ItemData(false, 75, new int[] { 0, 1, 2, 3 }, 9), 0, null, null) },
            { "under_f_7", new ItemData("Водолазка", false, 67, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 103, new int[] { 0, 1, 2, 3, 4, 5 }, 3), 0, null, null) },
            { "under_f_8", new ItemData("Футболка обычная", false, 78, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 141, new int[] { 0, 1, 2, 3, 4, 5 }, 14), 0, null, null) },
            { "under_f_9", new ItemData("Футболка черная", false, 147, new int[] { 0 }, new Top.ItemData(false, 236, new int[] { 0 }, 14), 0, null, null) },
            { "under_f_10", new ItemData("Корсет с кружевами", false, 22, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 22, new int[] { 0, 1, 2, 3, 4 }, 4), 0, null, null) },
            { "under_f_11", new ItemData("Майка с принтами #2", false, 29, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 36, new int[] { 0, 1, 2, 3, 4 }, 11), 0, null, null) },
            { "under_f_12", new ItemData("Футболка модницы", false, 45, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, new Top.ItemData(false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, 14), 0, null, null) },
            { "under_f_13", new ItemData("Футболка золотая", false, 49, new int[] { 0 }, new Top.ItemData(false, 67, new int[] { 0 }, 2), 0, null, null) },
            { "under_f_14", new ItemData("Майка модницы", false, 117, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, new Top.ItemData(false, 209, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }, 12), 0, null, null) },
            { "under_f_15", new ItemData("Корсет с принтами", false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, new Top.ItemData(false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, 4), 0, null, null) },
            { "under_f_16", new ItemData("Рубашка обычная", false, 38, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null, 0, null, null) },
            { "under_f_17", new ItemData("Рубашка в разноцветном жилете", false, 40, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null, 0, null, null) },
            { "under_f_18", new ItemData("Корсет с принтами #2", false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(false, 111, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 4), 0, null, null) },
            { "under_f_19", new ItemData("Блузка с принтами", false, 176, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, new Top.ItemData(false, 283, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, 12), 0, null, null) },
            { "under_f_20", new ItemData("Майка спортивная", false, 11, new int[] { 0, 1, 2, 10, 11, 15 }, new Top.ItemData(false, 11, new int[] { 0, 1, 2, 10, 11, 15 }, 11), 0, null, null) },
            { "under_f_21", new ItemData("Рубашка обычная #2", false, 24, new int[] { 0, 1, 2, 3, 4, 5 }, new Top.ItemData(false, 27, new int[] { 0, 1, 2, 3, 4, 5 }, 0), 0, null, null) },
            { "under_f_22", new ItemData("Футболка цветная", false, 30, new int[] { 0, 1, 2, 3 }, new Top.ItemData(false, 38, new int[] { 0, 1, 2, 3 }, 2), 0, null, null) },
            { "under_f_23", new ItemData("Футболка Xmas Criminal", false, 227, new int[] { 0, 1, 2, 3, 4 }, new Top.ItemData(false, 413, new int[] { 0, 1, 2, 3, 4  }, 14), 0, null, null) },
            { "under_f_24", new ItemData("Рубашка обычная #3", false, 217, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null, 0, null, null) },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            if (pData.Clothes[1] == null && data.BestTop != null)
            {
                if (Toggled && data.BestTop.ExtraData != null)
                {
                    player.SetClothes(Top.Slot, data.BestTop.ExtraData.Drawable, data.Textures[variation]);
                    player.SetClothes(Gloves.Slot, data.BestTop.ExtraData.BestTorso, 0);
                }
                else
                {
                    player.SetClothes(Top.Slot, data.BestTop.Drawable, data.Textures[variation]);
                    player.SetClothes(Gloves.Slot, data.BestTop.BestTorso, 0);
                }
            }
            else
            {
                if (Toggled && data.ExtraData != null)
                {
                    player.SetClothes(Slot, data.ExtraData.Drawable, data.Textures[variation]);
                    player.SetClothes(Gloves.Slot, data.ExtraData.BestTorso, 0);
                }
                else
                {
                    player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
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

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, new Dictionary<int, int>() {{ {string.Join(", ", BestTorsos.Select(x => $"{{ {x.Key}, {x.Value} }}"))} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, Dictionary<int, int> BestTorsos, string SexAlternativeID = null) : base(Name, 0.1f, "prop_ld_tshirt_02", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.BestTorsos = BestTorsos;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "gloves_m_0", new ItemData("Перчатки вязаные", true, 51, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 51 }, { 14, 50 }, { 12, 49 }, { 11, 48 }, { 8, 47 }, { 6, 46 }, { 5, 45 }, { 4, 44 }, { 2, 43 }, { 1, 42 }, { 0, 41 }, { 184, 187 }, { 112, 117 }, { 113, 124 }, { 114, 131 }
                }, "gloves_f_0")
            },
            { "gloves_m_1", new ItemData("Перчатки без пальцев", true, 62, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 62 }, { 14, 61 }, { 12, 60 }, { 11, 59 }, { 8, 58 }, { 6, 57 }, { 5, 56 }, { 4, 55 }, { 2, 54 }, { 1, 53 }, { 0, 52 }, { 184, 188 }, { 112, 118 }, { 113, 125 }, { 114, 132 }
                }, "gloves_f_1")
            },
            { "gloves_m_2", new ItemData("Перчатки рабочего", true, 73, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 73 }, { 14, 72 }, { 12, 71 }, { 11, 70 }, { 8, 69 }, { 6, 68 }, { 5, 67 }, { 4, 66 }, { 2, 65 }, { 1, 64 }, { 0, 63 }, { 184, 189 }, { 112, 119 }, { 113, 126 }, { 114, 133 }
                }, "gloves_f_2")
            },
            { "gloves_m_3", new ItemData("Перчатки вязаные разноцветные", true, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Dictionary<int, int>()
                {
                    { 15, 109 }, { 14, 108 }, { 12, 107 }, { 11, 106 }, { 8, 105 }, { 6, 104 }, { 5, 103 }, { 4, 102 }, { 2, 101 }, { 1, 100 }, { 0, 99 }
                }, "gloves_f_3")
            },
            { "gloves_m_4", new ItemData("Перчатки резиновые", true, 95, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 95 }, { 14, 94 }, { 12, 93 }, { 11, 92 }, { 8, 91 }, { 6, 90 }, { 5, 89 }, { 4, 88 }, { 2, 87 }, { 1, 86 }, { 0, 85 }, { 184, 191 }, { 112, 121 }, { 113, 128 }, { 114, 135 }
                }, "gloves_f_4")
            },
            { "gloves_m_5", new ItemData("Перчатки с вырезом", true, 29, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 29 }, { 14, 28 }, { 12, 27 }, { 11, 26 }, { 8, 25 }, { 6, 24 }, { 5, 23 }, { 4, 22 }, { 2, 21 }, { 1, 20 }, { 0, 19 }, { 184, 185 }, { 112, 115 }, { 113, 122 }, { 114, 129 }
                }, "gloves_f_5")
            },
            { "gloves_m_6", new ItemData("Перчатки из кожи", true, 40, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 40 }, { 14, 39 }, { 12, 38 }, { 11, 37 }, { 8, 36 }, { 6, 35 }, { 5, 34 }, { 4, 33 }, { 2, 32 }, { 1, 31 }, { 0, 30 }, { 184, 186 }, { 112, 116 }, { 113, 123 }, { 114, 130 }
                }, "gloves_f_6")
            },
            { "gloves_m_7", new ItemData("Перчатки по крою", true, 84, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 84 }, { 14, 83 }, { 12, 82 }, { 11, 81 }, { 8, 80 }, { 6, 79 }, { 5, 78 }, { 4, 77 }, { 2, 76 }, { 1, 75 }, { 0, 74 }, { 184, 190 }, { 112, 120 }, { 113, 127 }, { 114, 134 }
                }, "gloves_f_7")
            },
            { "gloves_m_8", new ItemData("Перчатки с протектором", true, 170, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new Dictionary<int, int>()
                {
                    { 15, 170 }, { 14, 180 }, { 12, 179 }, { 11, 178 }, { 8, 177 }, { 6, 176 }, { 5, 175 }, { 4, 174 }, { 2, 173 }, { 1, 172 }, { 0, 171 }, { 184, 194 }, { 112, 181 }, { 113, 182 }, { 114, 183 }
                }, "gloves_f_8")
            },
            #endregion

            #region ItemData Female
            { "gloves_f_0", new ItemData("Перчатки вязаные", false, 58, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 58 }, { 14, 57 }, { 12, 56 }, { 11, 55 }, { 9, 54 }, { 7, 53 }, { 6, 52 }, { 5, 51 }, { 4, 50 }, { 3, 49 }, { 2, 48 }, { 1, 47 }, { 0, 46 }, { 129, 134 }, { 130, 141 }, { 131, 148 }, { 153, 156 }, { 161, 164 }, { 229, 232 }
                }, "gloves_m_0")
            },
            { "gloves_f_1", new ItemData("Перчатки без пальцев", false, 71, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 71 }, { 14, 70 }, { 12, 69 }, { 11, 68 }, { 9, 67 }, { 7, 66 }, { 6, 65 }, { 5, 64 }, { 4, 63 }, { 3, 62 }, { 2, 61 }, { 1, 60 }, { 0, 59 }, { 129, 135 }, { 130, 142 }, { 131, 149 }, { 153, 157 }, { 161, 165 }, { 229, 233 }
                }, "gloves_m_1")
            },
            { "gloves_f_2", new ItemData("Перчатки рабочего", false, 84, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 84 }, { 14, 83 }, { 12, 82 }, { 11, 81 }, { 9, 80 }, { 7, 79 }, { 6, 78 }, { 5, 77 }, { 4, 76 }, { 3, 75 }, { 2, 74 }, { 1, 73 }, { 0, 72 }, { 129, 136 }, { 130, 143 }, { 131, 150 }, { 153, 158 }, { 161, 166 }, { 229, 234 }
                }, "gloves_m_2")
            },
            { "gloves_f_3", new ItemData("Перчатки вязаные разноцветные", false, 126, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, new Dictionary<int, int>()
                {
                    { 15, 126 }, { 14, 125 }, { 12, 124 }, { 11, 123 }, { 9, 122 }, { 7, 121 }, { 6, 120 }, { 5, 119 }, { 4, 118 }, { 3, 117 }, { 2, 116 }, { 1, 115 }, { 0, 114 }
                }, "gloves_m_3")
            },
            { "gloves_f_4", new ItemData("Перчатки резиновые", false, 110, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 110 }, { 14, 109 }, { 12, 108 }, { 11, 107 }, { 9, 106 }, { 7, 105 }, { 6, 104 }, { 5, 103 }, { 4, 102 }, { 3, 101 }, { 2, 100 }, { 1, 99 }, { 0, 98 }, { 129, 138 }, { 130, 145 }, { 131, 152 }, { 153, 160 }, { 161, 168 }, { 229, 236 }
                }, "gloves_m_4")
            },
            { "gloves_f_5", new ItemData("Перчатки с вырезом", false, 32, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 32 }, { 14, 31 }, { 12, 30 }, { 11, 29 }, { 9, 28 }, { 7, 27 }, { 6, 26 }, { 5, 25 }, { 4, 24 }, { 3, 23 }, { 2, 22 }, { 1, 21 }, { 0, 20 }, { 129, 132 }, { 130, 139 }, { 131, 146 }, { 153, 154 }, { 161, 162 }, { 229, 230 }
                }, "gloves_m_5")
            },
            { "gloves_f_6", new ItemData("Перчатки из кожи", false, 45, new int[] { 0, 1 }, new Dictionary<int, int>()
                {
                    { 15, 45 }, { 14, 44 }, { 12, 43 }, { 11, 42 }, { 9, 41 }, { 7, 40 }, { 6, 39 }, { 5, 38 }, { 4, 37 }, { 3, 36 }, { 2, 35 }, { 1, 34 }, { 0, 33 }, { 129, 133 }, { 130, 140 }, { 131, 147 }, { 153, 155 }, { 161, 163 }, { 229, 231 }
                }, "gloves_m_6")
            },
            { "gloves_f_7", new ItemData("Перчатки по крою", false, 97, new int[] { 0 }, new Dictionary<int, int>()
                {
                    { 15, 97 }, { 14, 96 }, { 12, 95 }, { 11, 94 }, { 9, 93 }, { 7, 92 }, { 6, 91 }, { 5, 90 }, { 4, 89 }, { 3, 88 }, { 2, 87 }, { 1, 86 }, { 0, 85 }, { 129, 137 }, { 130, 144 }, { 131, 151 }, { 153, 159 }, { 161, 167 }, { 229, 235 }
                }, "gloves_m_7")
            },
            { "gloves_f_8", new ItemData("Перчатки с протектором", false, 211, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, new Dictionary<int, int>()
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            int curTorso = player.GetClothesDrawable(3);

            int bestTorso;

            if (data.BestTorsos.TryGetValue(curTorso, out bestTorso))
                player.SetClothes(3, bestTorso, data.Textures[variation]);
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
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.4f, "p_laz_j02_s", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "pants_m_0", new ItemData("Джинсы обычные", true, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_1", new ItemData("Джинсы свободные", true, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_2", new ItemData("Спортивные штаны", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_3", new ItemData("Джинсы джогеры", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_4", new ItemData("Свободные спортивные штаны", true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_5", new ItemData("Шорты на веревках", true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_6", new ItemData("Джинсы очень свободные", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_7", new ItemData("Мятые брюки", true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_8", new ItemData("Брюки с карманами", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_9", new ItemData("Шорты обычные", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_10", new ItemData("Шорты беговые", true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_11", new ItemData("Бриджи с карманами", true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_12", new ItemData("Бриджи с принтами", true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_13", new ItemData("Бриджи обычные", true, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_m_14", new ItemData("Шорты беговые #2", true, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_15", new ItemData("Брюки хулигана", true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_16", new ItemData("Брюки карго", true, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_17", new ItemData("Брюки хаки", true, 29, new int[] { 0, 1, 2 }, null) },
            { "pants_m_18", new ItemData("Трико", true, 32, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_19", new ItemData("Шорты длинные", true, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_20", new ItemData("Джинсы бандитские", true, 43, new int[] { 0, 1 }, null) },
            { "pants_m_21", new ItemData("Штаны спортивки", true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_m_22", new ItemData("Штаны с принтами", true, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_23", new ItemData("Шорты хулигана", true, 62, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_24", new ItemData("Штаны гонщика", true, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_25", new ItemData("Штаны с принтами #2", true, 69, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "pants_m_26", new ItemData("Штаны байкерские", true, 71, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_m_27", new ItemData("Штаны байкерские #2", true, 73, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_m_28", new ItemData("Джинсы рваные", true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_29", new ItemData("Штаны хулигана на веревках", true, 78, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_30", new ItemData("Штаны хулигана на веревках #2", true, 83, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_31", new ItemData("Бриджи зауженные", true, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_32", new ItemData("Джинсы хулигана", true, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_33", new ItemData("Комбинезон", true, 90, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_34", new ItemData("Штаны разноцветные", true, 98, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_35", new ItemData("Штаны пижамные модника", true, 100, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "pants_m_36", new ItemData("Шорты модника", true, 117, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_m_37", new ItemData("Штаны спортивные", true, 55, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_38", new ItemData("Кимоно (М)", true, 56, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_39", new ItemData("Штаны хаки разноцветные", true, 59, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_40", new ItemData("Джинсы ретро", true, 63, new int[] { 0 }, null) },
            { "pants_m_41", new ItemData("Джинсы старого стиля", true, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_m_42", new ItemData("Штаны с веревками", true, 81, new int[] { 0, 1, 2 }, null) },
            { "pants_m_43", new ItemData("Штаны хулигана на веревках #2", true, 83, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_44", new ItemData("Шорты баскетбольные", true, 132, new int[] { 0, 1, 2 }, null) },
            { "pants_m_45", new ItemData("Брюки обычные", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_46", new ItemData("Брюки свободные", true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_47", new ItemData("Брюки классические", true, 20, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_48", new ItemData("Брюки классические #2", true, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_m_49", new ItemData("Брюки классические свободные", true, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_m_50", new ItemData("Брюки зауженные", true, 24, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "pants_m_51", new ItemData("Брюки вельветовые", true, 25, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "pants_m_52", new ItemData("Брюки слаксы", true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_m_53", new ItemData("Брюки вельветовые #2", true, 35, new int[] { 0 }, null) },
            { "pants_m_54", new ItemData("Брюки хаки #2", true, 37, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_55", new ItemData("Штаны модника", true, 44, new int[] { 0 }, null) },
            { "pants_m_56", new ItemData("Брюки слаксы #2", true, 49, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_m_57", new ItemData("Бриджи модника", true, 54, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "pants_m_58", new ItemData("Брюки с узорами", true, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_m_59", new ItemData("Шорты для бега с принтами", true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "pants_m_60", new ItemData("Бриджи с цепочкой", true, 103, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "pants_m_61", new ItemData("Брюки модника", true, 116, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_m_62", new ItemData("Брюки вельветовые разноцветные", true, 19, new int[] { 0, 1 }, null) },
            { "pants_m_63", new ItemData("Брюки классические разноцветные", true, 48, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_m_64", new ItemData("Брюки с узорами", true, 51, new int[] { 0 }, null) },
            { "pants_m_65", new ItemData("Брюки слаксы разноцветные", true, 52, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_m_66", new ItemData("Брюки слаксы с узорами", true, 53, new int[] { 0 }, null) },
            { "pants_m_67", new ItemData("Штаны модника c принтами", true, 118, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_68", new ItemData("Кимоно модника", true, 119, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_m_69", new ItemData("Брюки дизайнерские", true, 131, new int[] { 0 }, null) },
            { "pants_m_70", new ItemData("Джоггеры модника", true, 138, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_71", new ItemData("Джоггеры модника #2", true, 139, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_72", new ItemData("Джоггеры модника #3", true, 140, new int[] { 0, 1, 2 }, null) },
            { "pants_m_73", new ItemData("Слаксы современные", true, 141, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_m_74", new ItemData("Слаксы современные #2", true, 142, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, null) },
            { "pants_m_75", new ItemData("Слаксы кислотные", true, 143, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_0", new ItemData("Джинсы обычные", false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_1", new ItemData("Джинсы свободные", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_2", new ItemData("Спортивные штаны", false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_3", new ItemData("Джоггеры", false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_4", new ItemData("Юбка обычная", false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_5", new ItemData("Юбка строгая", false, 18, new int[] { 0, 1 }, null) },
            { "pants_f_6", new ItemData("Юбка с принтами", false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_7", new ItemData("Шорты обычные", false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_8", new ItemData("Трусы с принтами", false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_9", new ItemData("Чулки", false, 22, new int[] { 0, 1, 2 }, null) },
            { "pants_f_10", new ItemData("Брюки вельветовые", false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_11", new ItemData("Юбка укороченная", false, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_12", new ItemData("Шорты в обтяжку", false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_13", new ItemData("Шорты в обтяжку #2", false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_14", new ItemData("Штаны зауженные", false, 27, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_15", new ItemData("Юбка кимоно", false, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_f_16", new ItemData("Штаны спортивки", false, 66, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_f_17", new ItemData("Штаны свободные с принтами", false, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_18", new ItemData("Штаны обычные с принтами", false, 67, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "pants_f_19", new ItemData("Штаны с низкой посадкой", false, 43, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_20", new ItemData("Штаны карго с принтами", false, 71, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "pants_f_21", new ItemData("Джинсы Slim", false, 73, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_f_22", new ItemData("Джинсы Skinny", false, 74, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_f_23", new ItemData("Джинсы хулиганки", false, 84, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_f_24", new ItemData("Леггинсы с принтами", false, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_25", new ItemData("Штаны кокетки", false, 102, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 }, null) },
            { "pants_f_26", new ItemData("Брюки хаки", false, 109, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "pants_f_27", new ItemData("Штаны гонщицы", false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_f_28", new ItemData("Штаны хулиганки на веревках", false, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_f_29", new ItemData("Штаны хулиганки на веревках #2", false, 81, new int[] { 0, 1, 2 }, null) },
            { "pants_f_30", new ItemData("Комбинезон", false, 92, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_31", new ItemData("Чулки кружевные", false, 20, new int[] { 0, 1, 2 }, null) },
            { "pants_f_32", new ItemData("Леопардовое мини", false, 26, new int[] { 0 }, null) },
            { "pants_f_33", new ItemData("Мини в полоску", false, 28, new int[] { 0 }, null) },
            { "pants_f_34", new ItemData("Бриджи Military", false, 30, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_35", new ItemData("Юбка карандаш легкая", false, 36, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_36", new ItemData("Брюки с боковыми разрезами", false, 44, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_37", new ItemData("Брюки льняные", false, 45, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_38", new ItemData("Брюки облегающие прямые", false, 52, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_39", new ItemData("Леггинсы матовые", false, 54, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_40", new ItemData("Широкие спортивные штаны", false, 58, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_41", new ItemData("Штаны хаки разноцветные", false, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_f_42", new ItemData("Штаны мятые прямые", false, 64, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_43", new ItemData("Бриджи хулиганки на веревках", false, 82, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_f_44", new ItemData("Бриджи на веревках", false, 83, new int[] { 0, 1, 2 }, null) },
            { "pants_f_45", new ItemData("Штаны хулиганки кожаные", false, 85, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_46", new ItemData("Комбинезон джинсовый", false, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "pants_f_47", new ItemData("Брюки цветные с порезами", false, 106, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "pants_f_48", new ItemData("Шорты баскетбольные", false, 139, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_49", new ItemData("Брюки обычные", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_50", new ItemData("Трусы с кружевами", false, 19, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_51", new ItemData("Шорты джинсовые с принтами", false, 25, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_f_52", new ItemData("Юбка строгая с принтами", false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_f_53", new ItemData("Брюки свободные", false, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, null) },
            { "pants_f_54", new ItemData("Брюки узкие", false, 51, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_55", new ItemData("Брюки классически #2", false, 50, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "pants_f_56", new ItemData("Юбка классическая", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_57", new ItemData("Трусы модницы", false, 56, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "pants_f_58", new ItemData("Трусы Кюлот", false, 62, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_59", new ItemData("Брюки слаксы", false, 37, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "pants_f_60", new ItemData("Чулки со швом", false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_61", new ItemData("Штаны Skinny", false, 75, new int[] { 0, 1, 2 }, null) },
            { "pants_f_62", new ItemData("Юбка модницы", false, 108, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_63", new ItemData("Шорты с чулками", false, 78, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_64", new ItemData("Штаны с принтами", false, 104, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "pants_f_65", new ItemData("Штаны Skinny #2", false, 112, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_66", new ItemData("Штаны с рисунками", false, 124, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_67", new ItemData("Кимоно с рисунками", false, 125, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_f_68", new ItemData("Чиносы современные", false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "pants_f_69", new ItemData("Брюки строгие", false, 34, new int[] { 0 }, null) },
            { "pants_f_70", new ItemData("Чиносы легкие", false, 41, new int[] { 0, 1, 2, 3 }, null) },
            { "pants_f_71", new ItemData("Чиносы льняные", false, 49, new int[] { 0, 1 }, null) },
            { "pants_f_72", new ItemData("Брюки Skinny глянцевые", false, 76, new int[] { 0, 1, 2 }, null) },
            { "pants_f_73", new ItemData("Брюки Skinny кожаные", false, 77, new int[] { 0, 1, 2 }, null) },
            { "pants_f_74", new ItemData("Шорты модницы с принтами", false, 107, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "pants_f_75", new ItemData("Шорты длинные с принтами", false, 123, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "pants_f_76", new ItemData("Брюки облегающие с узором", false, 53, new int[] { 0 }, null) },
            { "pants_f_77", new ItemData("Леггинсы с узором", false, 55, new int[] { 0 }, null) },
            { "pants_f_78", new ItemData("Брюки Метро", false, 138, new int[] { 0 }, null) },
            { "pants_f_79", new ItemData("Джоггеры модницы", false, 145, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_80", new ItemData("Джоггеры модницы #2", false, 146, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_81", new ItemData("Джоггеры модницы #3", false, 147, new int[] { 0, 1, 2 }, null) },
            { "pants_f_82", new ItemData("Слаксы современные", false, 148, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "pants_f_83", new ItemData("Слаксы современные #2", false, 149, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21 }, null) },
            { "pants_f_84", new ItemData("Слаксы кислотные", false, 150, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(4, data.Drawable, data.Textures[variation]);
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
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.3f, "prop_ld_shoe_01", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "shoes_m_0", new ItemData("Кроссовки обычные", true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_1", new ItemData("Кеды", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_2", new ItemData("Сланцы", true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_3", new ItemData("Шлёпки с носками", true, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_4", new ItemData("Кроссовки скейтерские", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_5", new ItemData("Кроссовки спортивные", true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_6", new ItemData("Сапоги Челси", true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_7", new ItemData("Кеды большие", true, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_8", new ItemData("Берцы", true, 24, new int[] { 0 }, null) },
            { "shoes_m_9", new ItemData("Берцы #2", true, 25, new int[] { 0 }, null) },
            { "shoes_m_10", new ItemData("Кеды обычные", true, 26, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_11", new ItemData("Кеды с шипами", true, 28, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_m_12", new ItemData("Кроссовки для бега", true, 31, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_m_13", new ItemData("Кроссовки высокие", true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_14", new ItemData("Сапоги", true, 44, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_m_15", new ItemData("Кроссовки скользящие", true, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "shoes_m_16", new ItemData("Кроссовки с ремешком", true, 46, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "shoes_m_17", new ItemData("Сапоги байкерские", true, 50, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_m_18", new ItemData("Сапоги низкие", true, 53, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_m_19", new ItemData("Кроссовки с язычком", true, 57, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_20", new ItemData("Тапочки с подсветкой", true, 58, new int[] { 0, 1, 2 }, null) },
            { "shoes_m_21", new ItemData("Кроссовки боксерки", true, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "shoes_m_22", new ItemData("Сапоги разноцветные", true, 70, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_23", new ItemData("Сапоги с принтами", true, 79, new int[] { 0, 1 }, null) },
            { "shoes_m_24", new ItemData("Сапоги с принтами укороченные", true, 80, new int[] { 0, 1 }, null) },
            { "shoes_m_25", new ItemData("Кеды с носками", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_26", new ItemData("Потрёпанные берцы", true, 27, new int[] { 0 }, null) },
            { "shoes_m_27", new ItemData("Ботинки брутальные", true, 85, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_28", new ItemData("Ботинки брутальные укороченные", true, 86, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_29", new ItemData("Тапочки с подсветкой #2", true, 83, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, null) },
            { "shoes_m_30", new ItemData("Казаки кожаные", true, 37, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_m_31", new ItemData("Казаки кожаные укороченные", true, 38, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_m_32", new ItemData("Хайкеры низкие с двойной шнуровкой", true, 43, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_33", new ItemData("Хайкеры высокие на шнуровке", true, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_34", new ItemData("Хайкеры на шнуровке укороченные", true, 61, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_35", new ItemData("Хайкеры крепкие на шнуровке", true, 62, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_36", new ItemData("Хайкеры крепкие на шнуровке укороченные", true, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_m_37", new ItemData("Хайкеры разноцветные", true, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_38", new ItemData("Ботинки высокие на шнуровке", true, 35, new int[] { 0, 1 }, null) },
            { "shoes_m_39", new ItemData("Ботинки без шнуровки", true, 52, new int[] { 0, 1 }, null) },
            { "shoes_m_40", new ItemData("Походные ботинки со шнуровкой", true, 65, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "shoes_m_41", new ItemData("Походные ботинки со шнуровкой укороченные", true, 66, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "shoes_m_42", new ItemData("Ботинки прорезиненные", true, 72, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_43", new ItemData("Ботинки прорезиненные укороченные", true, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_44", new ItemData("Кеды с белыми вставками", true, 74, new int[] { 0, 1 }, null) },
            { "shoes_m_45", new ItemData("Ботинки зимние высокие", true, 81, new int[] { 0, 1, 2 }, null) },
            { "shoes_m_46", new ItemData("Ботинки зимние укороченные", true, 82, new int[] { 0, 1, 2 }, null) },
            { "shoes_m_47", new ItemData("Высокие сапоги без шнуровки", true, 88, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_48", new ItemData("Мокасины", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_49", new ItemData("Оксфорды черные", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_50", new ItemData("Ботинки из замши", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_51", new ItemData("Оксфорды разноцветные", true, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_52", new ItemData("Лоферы", true, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_53", new ItemData("Топсайдеры", true, 23, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_m_54", new ItemData("Лоферы с пряжками", true, 36, new int[] { 0, 1, 2, 3 }, null) },
            { "shoes_m_55", new ItemData("Кроссовки с подсветкой", true, 55, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_m_56", new ItemData("Кроссовки модника", true, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_57", new ItemData("Кроссовки модника укороченные", true, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_58", new ItemData("Кроссовки модника #2", true, 93, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, null) },
            { "shoes_m_59", new ItemData("Кроссовки с подсветкой #2", true, 77, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_60", new ItemData("Кроссовки модника #2 укороченные", true, 94, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, null) },
            { "shoes_m_61", new ItemData("Двухцветные лоферы", true, 30, new int[] { 0, 1 }, null) },
            { "shoes_m_62", new ItemData("Черно-белые оксфорды", true, 18, new int[] { 0, 1 }, null) },
            { "shoes_m_63", new ItemData("Оксфорды с носками цветные", true, 40, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_m_64", new ItemData("Джодпуры черно-белые", true, 19, new int[] { 0 }, null) },
            { "shoes_m_65", new ItemData("Кроссовки дутые глянцевые", true, 29, new int[] { 0 }, null) },
            { "shoes_m_66", new ItemData("Слипперы красные", true, 41, new int[] { 0 }, null) },
            { "shoes_m_67", new ItemData("Ботинки модника с вставками", true, 69, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_m_68", new ItemData("Кроссовки-носки модника", true, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "shoes_m_69", new ItemData("Слипоны с бананами", true, 95, new int[] { 0 }, null) },
            { "shoes_m_70", new ItemData("Кроссовки автолюбителя", true, 99, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null) },
            { "shoes_f_0", new ItemData("Кроссовки обычные", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_1", new ItemData("Валенки", false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_2", new ItemData("Кеды обычные", false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_3", new ItemData("Кроссовки спортивные", false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_4", new ItemData("Сланцы", false, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_5", new ItemData("Туфли закрытые", false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_6", new ItemData("Сапоги высокие", false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_7", new ItemData("Кроссовки баскетбольные", false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_8", new ItemData("Балетки", false, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_9", new ItemData("Сандалии", false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_10", new ItemData("Сапоги разноцветные", false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "shoes_f_11", new ItemData("Туфли с 2 язычком", false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_12", new ItemData("Ботильоны", false, 30, new int[] { 0 }, null) },
            { "shoes_f_13", new ItemData("Кеды закрытые", false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_14", new ItemData("Ботинки дезерты", false, 29, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_15", new ItemData("Кроссовки на высокой подошве", false, 32, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_f_16", new ItemData("Ботильоны #2", false, 44, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_17", new ItemData("Кроссовки скейтерские", false, 60, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_18", new ItemData("Тапочки с подсветкой", false, 61, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_19", new ItemData("Кроссовки боксерки", false, 67, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 }, null) },
            { "shoes_f_20", new ItemData("Сапоги сникерсы", false, 77, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 }, null) },
            { "shoes_f_21", new ItemData("Сапоги байкерские", false, 85, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_22", new ItemData("Ботинки высокие", false, 26, new int[] { 0 }, null) },
            { "shoes_f_23", new ItemData("Казаки", false, 38, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_f_24", new ItemData("Валенки", false, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_25", new ItemData("Казаки #2", false, 45, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_f_26", new ItemData("Казаки #1 укороченные", false, 39, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "shoes_f_27", new ItemData("Казаки #2 укороченные", false, 46, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_f_28", new ItemData("Берцы", false, 51, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_f_29", new ItemData("Берцы укороченные", false, 52, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_f_30", new ItemData("Берцы #2", false, 54, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_f_31", new ItemData("Берцы #2 укороченные", false, 55, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "shoes_f_32", new ItemData("Хайкеры крепкие на шнуровке", false, 63, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_33", new ItemData("Хайкеры крепкие на шнуровке укороченные", false, 64, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_34", new ItemData("Походные ботинки со шнуровкой", false, 65, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_35", new ItemData("Походные ботинки со шнуровкой укороченные", false, 66, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_36", new ItemData("Сапоги с принтами", false, 83, new int[] { 0, 1 }, null) },
            { "shoes_f_37", new ItemData("Сапоги с принтами укороченные", false, 84, new int[] { 0, 1 }, null) },
            { "shoes_f_38", new ItemData("Кеды простые", false, 27, new int[] { 0 }, null) },
            { "shoes_f_39", new ItemData("Кеды беговые", false, 28, new int[] { 0 }, null) },
            { "shoes_f_40", new ItemData("Полусапожки", false, 53, new int[] { 0, 1 }, null) },
            { "shoes_f_41", new ItemData("Полусапожки укороченные", false, 59, new int[] { 0, 1 }, null) },
            { "shoes_f_42", new ItemData("Тапочки с подсветкой #2", false, 87, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, null) },
            { "shoes_f_43", new ItemData("Ботинки брутальные", false, 89, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_44", new ItemData("Ботинки брутальные укороченные", false, 90, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_45", new ItemData("Ботинки прорезиненные", false, 75, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_46", new ItemData("Ботинки прорезиненные укороченные", false, 76, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_47", new ItemData("Туфли обычные", false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_48", new ItemData("Туфли Стилеты", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_49", new ItemData("Туфли Lita", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_50", new ItemData("Кроссовки спортивные #2", false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_51", new ItemData("Туфли открытые", false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "shoes_f_52", new ItemData("Туфли на шпильках", false, 19, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_53", new ItemData("Лодочки", false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_54", new ItemData("Сапоги Хайкеры", false, 68, new int[] { 0, 1, 2, 3, 4, 5, 6 }, null) },
            { "shoes_f_55", new ItemData("Лоферы", false, 37, new int[] { 0, 1, 2, 3 }, null) },
            { "shoes_f_56", new ItemData("Туфли на платформе", false, 42, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_57", new ItemData("Кроссовки позолоченные", false, 31, new int[] { 0 }, null) },
            { "shoes_f_58", new ItemData("Кроссовки с подсветкой", false, 58, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "shoes_f_59", new ItemData("Кроссовки модницы", false, 79, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_60", new ItemData("Кроссовки модницы укороченные", false, 80, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_61", new ItemData("Кроссовки с подсветкой #2", false, 81, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_62", new ItemData("Кроссовки модницы #2", false, 96, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, null) },
            { "shoes_f_63", new ItemData("Кроссовки модницы #2 укороченные", false, 97, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 }, null) },
            { "shoes_f_64", new ItemData("Сапоги с узорами", false, 73, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_65", new ItemData("Сапоги с узорами укороченные", false, 74, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 }, null) },
            { "shoes_f_66", new ItemData("Туфли с носками", false, 18, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_67", new ItemData("Сапоги высокие с застежками", false, 56, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_68", new ItemData("Сапоги укороченные с застежками", false, 57, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_69", new ItemData("Сапоги высокие на шнуровке", false, 24, new int[] { 0 }, null) },
            { "shoes_f_70", new ItemData("Ботинки высокие кожаные", false, 25, new int[] { 0 }, null) },
            { "shoes_f_71", new ItemData("Походные ботинки", false, 36, new int[] { 0, 1 }, null) },
            { "shoes_f_72", new ItemData("Кроссовки с белыми вставками", false, 78, new int[] { 0, 1 }, null) },
            { "shoes_f_73", new ItemData("Кроссовки с застежкой", false, 47, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "shoes_f_74", new ItemData("Кроссовки-носки модницы", false, 91, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17 }, null) },
            { "shoes_f_75", new ItemData("Высокие сапоги без шнуровки", false, 92, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "shoes_f_76", new ItemData("Слипоны модницы", false, 98, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "shoes_f_77", new ItemData("Слипоны с бананами", false, 99, new int[] { 0 }, null) },
            { "shoes_f_78", new ItemData("Туфли на шпильке с ремешком", false, 23, new int[] { 0, 1, 2 }, null) },
            { "shoes_f_79", new ItemData("Кроссовки автолюбителя", false, 103, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23 }, null) },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
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
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.2f, "p_jewel_necklace_02", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region Accessories Male
            { "accs_m_0", new ItemData("Цепочка Бисмарк", true, 16, new int[] { 0, 1, 2 }, null) },
            { "accs_m_1", new ItemData("Цепочка Бисмарк #2", true, 17, new int[] { 0, 1, 2 }, null) },
            { "accs_m_2", new ItemData("Шарф", true, 30, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "accs_m_3", new ItemData("Цепочка с балаклавой", true, 44, new int[] { 0 }, null) },
            { "accs_m_4", new ItemData("Цепочка с якорным плетением", true, 74, new int[] { 0, 1 }, null) },
            { "accs_m_5", new ItemData("Цепь плетение Фигаро", true, 85, new int[] { 0, 1 }, null) },
            { "accs_m_6", new ItemData("Цепь плетение Ролло", true, 87, new int[] { 0, 1 }, null) },
            { "accs_m_7", new ItemData("Цепь M", true, 110, new int[] { 0, 1 }, null) },
            { "accs_m_8", new ItemData("Шарф Арафатка", true, 112, new int[] { 0, 1, 2 }, null) },
            { "accs_m_9", new ItemData("Наушники", true, 114, new int[] { 0 }, null) },
            { "accs_m_10", new ItemData("Цепь с колесом", true, 119, new int[] { 0, 1 }, null) },
            { "accs_m_11", new ItemData("Наушники #2", true, 124, new int[] { 0, 1 }, null) },
            { "accs_m_12", new ItemData("Галстук пионера", true, 151, new int[] { 0 }, null) },
            { "accs_m_13", new ItemData("Галстук Виндзор", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_14", new ItemData("Бабочка", true, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_15", new ItemData("Узкий галстук", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_16", new ItemData("Галстук Виндзор длинный", true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_17", new ItemData("Галстук Виндзор узкий", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_18", new ItemData("Бабочка цветная", true, 32, new int[] { 0, 1, 2 }, null) },
            { "accs_m_19", new ItemData("Галстук Регат", true, 37, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_20", new ItemData("Галстук обычный", true, 38, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_21", new ItemData("Галстук Регат удлиненный", true, 39, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_m_22", new ItemData("Аккуратная бабочка", true, 118, new int[] { 0 }, null) },
            #endregion

            #region Accessories Female
            { "accs_f_0", new ItemData("Шарф Арафатка", false, 83, new int[] { 0, 1, 2 }, null) },
            { "accs_f_1", new ItemData("Шарф Арафатка #2", false, 9, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "accs_f_2", new ItemData("Шарф Арафатка #3", false, 15, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "accs_f_3", new ItemData("Наушники", false, 85, new int[] { 0 }, null) },
            { "accs_f_4", new ItemData("Наушники #2", false, 94, new int[] { 0, 1 }, null) },
            { "accs_f_5", new ItemData("Галстук пионера", false, 120, new int[] { 0 }, null) },
            { "accs_f_6", new ItemData("Бусы длинные", false, 12, new int[] { 0, 1, 2 }, null) },
            { "accs_f_7", new ItemData("Галстук Французский", false, 13, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "accs_f_8", new ItemData("Галстук Stillini", false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_f_9", new ItemData("Строгий галстук", false, 21, new int[] { 0, 1, 2 }, null) },
            { "accs_f_10", new ItemData("Строгий дизайнерский галстук", false, 22, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, null) },
            { "accs_f_11", new ItemData("Бабочка", false, 23, new int[] { 0, 1, 2 }, null) },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
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

    public class Glasses : Clothes, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.2f, "prop_cs_sol_glasses", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "glasses_m_0", new ItemData("Спортивные очки", true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_1", new ItemData("Очки Панто", true, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_2", new ItemData("Спортивные очки #2", true, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_3", new ItemData("Прямоугольные очки", true, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_4", new ItemData("Очки Авиаторы обычные", true, 12, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_5", new ItemData("Спортивные очки #3", true, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_6", new ItemData("Очки обычные", true, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "glasses_m_7", new ItemData("Очки Авиаторы обычные #2", true, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_8", new ItemData("Очки USA", true, 21, new int[] { 0 }, null) },
            { "glasses_m_9", new ItemData("Очки USA #2", true, 22, new int[] { 0 }, null) },
            { "glasses_m_10", new ItemData("Спортивные очки", true, 2, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_11", new ItemData("Browline", true, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_12", new ItemData("Авиаторы", true, 5, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_13", new ItemData("Wayfarer", true, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_14", new ItemData("Авиаторы #2", true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_15", new ItemData("Прямоугольные очки #2", true, 13, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_16", new ItemData("Прямоугольные очки #3", true, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_17", new ItemData("Wayfarer #2", true, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_m_18", new ItemData("Авиаторы защищенные", true, 28, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_m_19", new ItemData("Очки модника", true, 29, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, null) },
            { "glasses_m_20", new ItemData("Очки неоновые", true, 30, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_m_21", new ItemData("Очки клубные", true, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_m_22", new ItemData("Очки современные", true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_m_23", new ItemData("Очки модника #2", true, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            #endregion

            #region ItemData Female
            { "glasses_f_0", new ItemData("Прямоугольные очки", false, 0, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_1", new ItemData("Спортивные очки", false, 9, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_2", new ItemData("Круглые очки", false, 1, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_3", new ItemData("Очки-кошки", false, 4, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_4", new ItemData("Овальные очки", false, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_5", new ItemData("Спортивные очки #2", false, 3, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_6", new ItemData("Очки USA", false, 22, new int[] { 0 }, null) },
            { "glasses_f_7", new ItemData("Очки USA #2", false, 23, new int[] { 0 }, null) },
            { "glasses_f_8", new ItemData("Очки переплетающиеся", false, 7, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_9", new ItemData("Очки строгие", false, 18, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_10", new ItemData("Очки строгие #2", false, 19, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_11", new ItemData("Очки DS", false, 6, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_12", new ItemData("Очки DS #2", false, 16, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "glasses_f_13", new ItemData("Авиаторы", false, 11, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "glasses_f_14", new ItemData("Очки-кошки #2", false, 14, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_15", new ItemData("Wayfarer", false, 20, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "glasses_f_16", new ItemData("Wayfarer #2", false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7 }, null) },
            { "glasses_f_17", new ItemData("Wayfarer #3", false, 24, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_18", new ItemData("Прямоугольные очки #2", false, 10, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, null) },
            { "glasses_f_19", new ItemData("Овальные очки #2", false, 17, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "glasses_f_20", new ItemData("Авиаторы защищенные", false, 30, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_f_21", new ItemData("Очки модницы", false, 31, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 }, null) },
            { "glasses_f_22", new ItemData("Очки неоновые", false, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_f_23", new ItemData("Очки клубные", false, 33, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_f_24", new ItemData("Очки современные", false, 34, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
            { "glasses_f_25", new ItemData("Очки модницы #2", false, 35, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, null) },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);
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

    public class Watches : Clothes, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.1f, "prop_jewel_02b", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "watches_m_0", new ItemData("Спортивные часы", true, 1, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "watches_m_1", new ItemData("Классические часы", true, 3, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "watches_m_2", new ItemData("Спортивные часы #2", true, 5, new int[] { 0, 1, 2, 3 }, null) },
            { "watches_m_3", new ItemData("Часы угловатые", true, 7, new int[] { 0, 1, 2 }, null) },
            { "watches_m_4", new ItemData("Часы с механизмом кинетик", true, 10, new int[] { 0, 1, 2 }, null) },
            { "watches_m_5", new ItemData("Часы обычные", true, 12, new int[] { 0, 1, 2 }, null) },
            { "watches_m_6", new ItemData("Часы с картинками", true, 13, new int[] { 0, 1, 2 }, null) },
            { "watches_m_7", new ItemData("Часы аккуратные", true, 14, new int[] { 0, 1, 2 }, null) },
            { "watches_m_8", new ItemData("Часы с большим циферблатом", true, 15, new int[] { 0, 1, 2 }, null) },
            { "watches_m_9", new ItemData("Часы с механизмом кинетик #2", true, 20, new int[] { 0, 1, 2 }, null) },
            { "watches_m_10", new ItemData("Часы смарт", true, 21, new int[] { 0, 1, 2 }, null) },
            { "watches_m_11", new ItemData("Часы квадратные", true, 36, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_m_12", new ItemData("Механические часы", true, 0, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "watches_m_13", new ItemData("Хронометр", true, 6, new int[] { 0, 1, 2 }, null) },
            { "watches_m_14", new ItemData("Хронограф", true, 8, new int[] { 0, 1, 2 }, null) },
            { "watches_m_15", new ItemData("Часы двойные", true, 9, new int[] { 0, 1, 2 }, null) },
            { "watches_m_16", new ItemData("Хронограф #2", true, 11, new int[] { 0, 1, 2 }, null) },
            { "watches_m_17", new ItemData("Часы с большим циферблатом #2", true, 16, new int[] { 0, 1, 2 }, null) },
            { "watches_m_18", new ItemData("Часы необычные #2", true, 17, new int[] { 0, 1, 2 }, null) },
            { "watches_m_19", new ItemData("Механические часы #2", true, 18, new int[] { 0, 1, 2 }, null) },
            { "watches_m_20", new ItemData("Хронограф #3", true, 19, new int[] { 0, 1, 2 }, null) },
            { "watches_m_21", new ItemData("Часы солидные", true, 30, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_m_22", new ItemData("MIDO", true, 31, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_m_23", new ItemData("Tudor", true, 32, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "watches_m_24", new ItemData("Longines", true, 34, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_m_25", new ItemData("TAG", true, 35, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            #endregion

            #region ItemData Female
            { "watches_f_0", new ItemData("Diesel", false, 3, new int[] { 0, 1, 2 }, null) },
            { "watches_f_1", new ItemData("Skagen", false, 4, new int[] { 0, 1, 2 }, null) },
            { "watches_f_2", new ItemData("Часы шестиугольные", false, 5, new int[] { 0, 1, 2 }, null) },
            { "watches_f_3", new ItemData("Часы с механизмом кинетик", false, 6, new int[] { 0, 1, 2 }, null) },
            { "watches_f_4", new ItemData("Часы спортивные", false, 8, new int[] { 0, 1, 2 }, null) },
            { "watches_f_5", new ItemData("Часы круглые", false, 24, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_6", new ItemData("Часы аккуратные", false, 25, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_7", new ItemData("Часы с большим циферблатом", false, 26, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_8", new ItemData("Часы позолоченные", false, 2, new int[] { 0, 1, 2, 3 }, null) },
            { "watches_f_9", new ItemData("Seiko", false, 7, new int[] { 0, 1, 2 }, null) },
            { "watches_f_10", new ItemData("Часы позолоченные #2", false, 9, new int[] { 0, 1, 2 }, null) },
            { "watches_f_11", new ItemData("Часы солидные", false, 19, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_12", new ItemData("MIDO", false, 20, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_13", new ItemData("Tudor", false, 21, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }, null) },
            { "watches_f_14", new ItemData("Longines", false, 23, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
            { "watches_f_15", new ItemData("Часы круглые", false, 24, new int[] { 0, 1, 2, 3, 4, 5 }, null) },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);
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

    public class Bracelet : Clothes, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.1f, "prop_jewel_02b", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region Bracelets Male
            { "bracelet_m_0", new ItemData("Плетеный браслет", true, 0, new int[] { 0 }, "bracelet_f_7") },
            { "bracelet_m_1", new ItemData("Плетеный браслет #2", true, 1, new int[] { 0 }, "bracelet_f_8") },
            { "bracelet_m_2", new ItemData("Жесткое плетение", true, 2, new int[] { 0 }, "bracelet_f_9") },
            { "bracelet_m_3", new ItemData("Плетение с черепами", true, 3, new int[] { 0 }, "bracelet_f_10") },
            { "bracelet_m_4", new ItemData("Плетение Z", true, 4, new int[] { 0 }, "bracelet_f_11") },
            { "bracelet_m_5", new ItemData("Плетение с браслетом", true, 5, new int[] { 0 }, "bracelet_f_12") },
            { "bracelet_m_6", new ItemData("Браслет с шипами", true, 6, new int[] { 0 }, "bracelet_f_13") },
            { "bracelet_m_7", new ItemData("Кожаный напульсник", true, 7, new int[] { 0, 1, 2, 3 }, "bracelet_f_14") },
            { "bracelet_m_8", new ItemData("Светящиеся браслеты", true, 8, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, "bracelet_f_15") },
            #endregion

            #region Bracelets Female
            { "bracelet_f_0", new ItemData("Золотой браслет", false, 0, new int[] { 0 }, null) },
            { "bracelet_f_1", new ItemData("Золотой браслет #2", false, 1, new int[] { 0 }, null) },
            { "bracelet_f_2", new ItemData("Золотой браслет #3", false, 2, new int[] { 0 }, null) },
            { "bracelet_f_3", new ItemData("Золотой браслет #4", false, 3, new int[] { 0 }, null) },
            { "bracelet_f_4", new ItemData("Золотой браслет #5", false, 4, new int[] { 0 }, null) },
            { "bracelet_f_5", new ItemData("Золотой браслет #6", false, 5, new int[] { 0 }, null) },
            { "bracelet_f_6", new ItemData("Золотой браслет #7", false, 6, new int[] { 0 }, null) },
            { "bracelet_f_7", new ItemData("Плетеный браслет", false, 7, new int[] { 0 }, "bracelet_m_0") },
            { "bracelet_f_8", new ItemData("Плетеный браслет #2", false, 8, new int[] { 0 }, "bracelet_m_1") },
            { "bracelet_f_9", new ItemData("Жесткое плетение", false, 9, new int[] { 0 }, "bracelet_m_2") },
            { "bracelet_f_10", new ItemData("Плетение с черепами", false, 10, new int[] { 0 }, "bracelet_m_3") },
            { "bracelet_f_11", new ItemData("Плетение Z", false, 11, new int[] { 0 }, "bracelet_m_4") },
            { "bracelet_f_12", new ItemData("Плетение с браслетом", false, 12, new int[] { 0 }, "bracelet_m_5") },
            { "bracelet_f_13", new ItemData("Браслет с шипами", false, 13, new int[] { 0 }, "bracelet_m_6") },
            { "bracelet_f_14", new ItemData("Кожаный напульсник", false, 14, new int[] { 0, 1, 2, 3 }, "bracelet_m_7") },
            { "bracelet_f_15", new ItemData("Светящиеся браслеты", false, 15, new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }, "bracelet_m_8") },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);
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

    public class Earrings : Clothes, Clothes.IProp
    {
        new public class ItemData : Clothes.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, 0.1f, "p_tmom_earrings_s", Sex, Drawable, Textures, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            #region ItemData Male
            { "ears_m_0", new ItemData("Солитер (левый)", true, 3, new int[] { 0, 1, 2 }, null) },
            { "ears_m_1", new ItemData("Солитер (правый)", true, 4, new int[] { 0, 1, 2 }, null) },
            { "ears_m_2", new ItemData("Солитер (оба)", true, 5, new int[] { 0, 1, 2 }, null) },
            { "ears_m_3", new ItemData("Гвоздники (левый)", true, 6, new int[] { 0, 1 }, null) },
            { "ears_m_4", new ItemData("Гвоздники (правый)", true, 7, new int[] { 0, 1 }, null) },
            { "ears_m_5", new ItemData("Гвоздники (оба)", true, 8, new int[] { 0, 1 }, null) },
            { "ears_m_6", new ItemData("Diamond (левый)", true, 9, new int[] { 0, 1, 2 }, null) },
            { "ears_m_7", new ItemData("Diamond (правый)", true, 10, new int[] { 0, 1, 2 }, null) },
            { "ears_m_8", new ItemData("Diamond (оба)", true, 11, new int[] { 0, 1, 2 }, null) },
            { "ears_m_9", new ItemData("Ромб (левый)", true, 12, new int[] { 0, 1, 2 }, null) },
            { "ears_m_10", new ItemData("Ромб (правый)", true, 13, new int[] { 0, 1, 2 }, null) },
            { "ears_m_11", new ItemData("Ромб (оба)", true, 14, new int[] { 0, 1, 2 }, null) },
            { "ears_m_12", new ItemData("Кнопка (левый)", true, 15, new int[] { 0, 1, 2 }, null) },
            { "ears_m_13", new ItemData("Кнопка (правый)", true, 16, new int[] { 0, 1, 2 }, null) },
            { "ears_m_14", new ItemData("Кнопка (оба)", true, 17, new int[] { 0, 1, 2 }, null) },
            { "ears_m_15", new ItemData("Квадрат платиновый (левый)", true, 18, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "ears_m_16", new ItemData("Квадрат платиновый (правый)", true, 19, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "ears_m_17", new ItemData("Квадрат платиновый (оба)", true, 20, new int[] { 0, 1, 2, 3, 4 }, null) },
            { "ears_m_18", new ItemData("Серьги NS (левый)", true, 21, new int[] { 0, 1 }, null) },
            { "ears_m_19", new ItemData("Серьги NS (правый)", true, 22, new int[] { 0, 1 }, null) },
            { "ears_m_20", new ItemData("Серьги NS (оба)", true, 23, new int[] { 0, 1 }, null) },
            { "ears_m_21", new ItemData("Череп (левый)", true, 24, new int[] { 0, 1, 2, 3 }, null) },
            { "ears_m_22", new ItemData("Череп (правый)", true, 25, new int[] { 0, 1, 2, 3 }, null) },
            { "ears_m_23", new ItemData("Череп (оба)", true, 26, new int[] { 0, 1, 2, 3 }, null) },
            { "ears_m_24", new ItemData("Острый цилиндр (левый)", true, 27, new int[] { 0, 1 }, null) },
            { "ears_m_25", new ItemData("Острый цилиндр (правый)", true, 28, new int[] { 0, 1 }, null) },
            { "ears_m_26", new ItemData("Острый цилиндр (оба)", true, 29, new int[] { 0, 1 }, null) },
            { "ears_m_27", new ItemData("Черный сапфир (левый)", true, 30, new int[] { 0, 1, 2 }, null) },
            { "ears_m_28", new ItemData("Серьги NS (левый)", true, 21, new int[] { 0, 1 }, null) },
            { "ears_m_29", new ItemData("Черный сапфир (оба)", true, 32, new int[] { 0, 1, 2 }, null) },
            { "ears_m_30", new ItemData("Позолоченный NS (левый)", true, 33, new int[] { 0 }, null) },
            { "ears_m_31", new ItemData("Позолоченный NS (правый)", true, 34, new int[] { 0, 1 }, null) },
            { "ears_m_32", new ItemData("Позолоченный NS (оба)", true, 35, new int[] { 0, 1 }, null) },
            { "ears_m_33", new ItemData("Микрофоны (оба)", true, 37, new int[] { 0, 1 }, "ears_f_14") },
            { "ears_m_34", new ItemData("Карты (оба)", true, 38, new int[] { 0, 1, 2, 3 }, "ears_f_15") },
            { "ears_m_35", new ItemData("Игральные кости (оба)", true, 39, new int[] { 0, 1, 2, 3 }, "ears_f_16") },
            { "ears_m_36", new ItemData("Игральные фишки (оба)", true, 40, new int[] { 0, 1, 2, 3 }, "ears_f_17") },
            #endregion

            #region ItemData Female
            { "ears_f_0", new ItemData("Шандельеры", false, 3, new int[] { 0 }, null) },
            { "ears_f_1", new ItemData("Конго", false, 4, new int[] { 0 }, null) },
            { "ears_f_2", new ItemData("Серьги-жирандоль", false, 5, new int[] { 0 }, null) },
            { "ears_f_3", new ItemData("Серьги-протяжки", false, 6, new int[] { 0, 1, 2 }, null) },
            { "ears_f_4", new ItemData("Кластеры", false, 7, new int[] { 0, 1, 2 }, null) },
            { "ears_f_5", new ItemData("Петли", false, 8, new int[] { 0, 1, 2 }, null) },
            { "ears_f_6", new ItemData("Цепочки", false, 9, new int[] { 0, 1, 2 }, null) },
            { "ears_f_7", new ItemData("Петли с камнем", false, 10, new int[] { 0, 1, 2 }, null) },
            { "ears_f_8", new ItemData("Петли снежинки", false, 11, new int[] { 0, 1, 2 }, null) },
            { "ears_f_9", new ItemData("Кнопки", false, 12, new int[] { 0, 1, 2 }, null) },
            { "ears_f_10", new ItemData("Калаши", false, 13, new int[] { 0 }, null) },
            { "ears_f_11", new ItemData("Кольца переплет", false, 14, new int[] { 0 }, null) },
            { "ears_f_12", new ItemData("Кольца", false, 15, new int[] { 0 }, null) },
            { "ears_f_13", new ItemData("Кольца FY", false, 16, new int[] { 0 }, null) },
            { "ears_f_14", new ItemData("Микрофоны", false, 18, new int[] { 0, 1 }, "ears_m_33") },
            { "ears_f_15", new ItemData("Карты", false, 19, new int[] { 0, 1, 2, 3 }, "ears_m_34") },
            { "ears_f_16", new ItemData("Игральные кости", false, 20, new int[] { 0, 1, 2, 3 }, "ears_m_35") },
            { "ears_f_17", new ItemData("Игральные фишки", false, 21, new int[] { 0, 1, 2, 3 }, "ears_m_36") },
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

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetAccessories(Slot, data.Drawable, data.Textures[variation]);
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
    public class Armour : Clothes
    {
        new public class ItemData : Clothes.ItemData
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

            public int DrawableTop { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {MaxStrength}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, int DrawableTop, int MaxStrength, string SexAlternativeID = null) : base(Name, Weight, "prop_armour_pickup", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.DrawableTop = DrawableTop;

                this.MaxStrength = MaxStrength;
            }

            public ItemData(string Name, float Weight, bool Sex, int Drawable, Colours[] Colours, int DrawableTop, int MaxStrength, string SexAlternativeID = null) : this(Name, Weight, Sex, Drawable, Colours.Select(x => (int)x).ToArray(), DrawableTop, MaxStrength, SexAlternativeID) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "arm_m_s", new ItemData("Обычный бронежилет", 0.5f, true, 28, new ItemData.Colours[] { ItemData.Colours.White }, 19, 100, null) },
        };

        [JsonIgnore]
        public const int Slot = (int)Game.Data.Customization.ClothesTypes.Armour;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        public int Strength { get; set; }

        /// <summary>Метод для надевания брони на игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, pData.Clothes[1] == null ? data.Drawable : data.DrawableTop, data.Textures[variation]);

            player.SetArmour(Strength);
        }

        /// <summary>Метод для снятия брони с игрока</summary>
        /// <param name="pData">Сущность игрока</param>
        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            var value = player.Armor;

            if (value < 0)
                value = 0;

            if (value < Strength)
            {
                Strength = value;

                this.Update();
            }

            player.SetClothes(Slot, 0, 0);

            player.SetArmour(0);
        }

        public Armour(string ID, int Var = 0) : base(ID, IDList[ID], typeof(Armour), Var)
        {
            this.Strength = Data.MaxStrength;
        }
    }
    #endregion

    #region Bag
    public class Bag : Clothes, IContainer
    {
        new public class ItemData : Clothes.ItemData
        {
            /// <summary>Максимальное кол-во слотов</summary>
            public byte MaxSlots { get; set; }

            /// <summary>Максимальный вес содержимого</summary>
            public float MaxWeight { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {MaxSlots}, {MaxWeight}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, byte MaxSlots, float MaxWeight, string SexAlternativeID = null) : base(Name, 0.25f, "prop_cs_heist_bag_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.MaxSlots = MaxSlots;

                this.MaxWeight = MaxWeight;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "bag_m_0", new ItemData("Обычная сумка", true, 81, new int[] { 0 }, 10, 5f, "bag_f_0") },

            { "bag_f_0", new ItemData("Обычная сумка", false, 81, new int[] { 0 }, 10, 5f, "bag_m_0") },
        };

        public const int Slot = 5;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        /// <summary>Предметы внутри</summary>
        [JsonIgnore]
        public Item[] Items { get; set; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.BeltOn)
                return;

            var data = Data;

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            if (pData.BeltOn)
                return;

            player.SetClothes(Slot, 0, 0);
        }

        /// <summary>Итоговый вес</summary>
        /// <remarks>Включает в себя вес самой сумки!</remarks>
        [JsonIgnore]
        public override float Weight { get => BaseWeight + Items.Sum(x => x?.Weight ?? 0f); }

        public Bag(string ID, int Variation = 0) : base(ID, IDList[ID], typeof(Bag), Variation)
        {
            this.Items = new Item[Data.MaxSlots];
        }
    }
    #endregion

    #region Holster
    public class Holster : Clothes, IContainer
    {
        new public class ItemData : Clothes.ItemData
        {
            public int DrawableWeapon { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Sex.ToString().ToLower()}, {Drawable}, new int[] {{ {string.Join(", ", Textures)} }}, {(SexAlternativeID == null ? "null" : $"\"{SexAlternativeID}\"")}";

            public ItemData(string Name, bool Sex, int Drawable, int[] Textures, int DrawableWeapon, string SexAlternativeID = null) : base(Name, 0.15f, "prop_holster_01", Sex, Drawable, Textures, SexAlternativeID)
            {
                this.DrawableWeapon = DrawableWeapon;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "holster_m_0", new ItemData("Кобура на ногу", true, 136, new int[] { 0, 1 }, 134, null) },
            { "holster_m_1", new ItemData("Кобура простая", true, 135, new int[] { 0, 1 }, 137, null) },
        };

        public const int Slot = 10;

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        new public ItemData SexAlternativeData { get => (ItemData)base.SexAlternativeData; set => base.SexAlternativeData = value; }

        [JsonIgnore]
        public Item[] Items { get; set; }

        [JsonIgnore]
        public Weapon Weapon { get => (Weapon)Items[0]; }

        public override void Wear(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, Items[0] == null ? data.Drawable : data.DrawableWeapon, data.Textures[variation]);
        }

        public override void Unwear(PlayerData pData)
        {
            var player = pData.Player;

            player.SetClothes(Slot, 0, 0);
        }

        public void WearWeapon(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, data.DrawableWeapon, data.Textures[variation]);
        }

        public void UnwearWeapon(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            var variation = Var;

            if (Data.Sex != pData.Sex)
            {
                data = SexAlternativeData;

                if (data == null)
                    return;

                if (variation >= data.Textures.Length)
                    variation = data.Textures.Length;
            }

            player.SetClothes(Slot, data.Drawable, data.Textures[variation]);
        }

        [JsonIgnore]
        public override float Weight { get => BaseWeight + Items.Sum(x => x?.Weight ?? 0f); }

        public Holster(string ID, int Variation = 0) : base(ID, IDList[ID], typeof(Holster), Variation)
        {
            this.Items = new Item[1];
        }
    }
    #endregion

    #region Numberplate
    public class Numberplate : Item, ITagged
    {
        public static List<string> UsedTags { get; private set; } = new List<string>();

        new public class ItemData : Item.ItemData
        {
            public int Variation { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Variation}";

            public ItemData(string Name, string Model, int Number) : base(Name, 0.15f, Model)
            {
                this.Variation = Number;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "np_0", new ItemData("Номерной знак", "p_num_plate_01", 0) },
            { "np_1", new ItemData("Номерной знак", "p_num_plate_04", 1) },
            { "np_2", new ItemData("Номерной знак", "p_num_plate_02", 2) },
            { "np_3", new ItemData("Номерной знак", "p_num_plate_02", 3) },
            { "np_4", new ItemData("Номерной знак", "p_num_plate_01", 4) },
            { "np_5", new ItemData("Номерной знак", "p_num_plate_01", 5) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public string Tag { get; set; }

        public void Setup(VehicleData vData)
        {
            var veh = vData.Vehicle;

            veh.NumberPlateStyle = Data.Variation;
            veh.NumberPlate = Tag;
        }

        public void Take(VehicleData vData)
        {
            var veh = vData.Vehicle;

            veh.NumberPlateStyle = 0;
            veh.NumberPlate = "";
        }

        private static char[] Chars = new char[]
        {
            'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        /// <summary>Метод для генерации случайного номера</summary>
        /// <param name="length">Длина номера (от 1 до 8)</param>
        public string GenerateTag(int length)
        {
            if (UsedTags.Count == int.MaxValue)
                return null;

            if (length < 1 || length > 8)
                length = 8;

            Random rand = new Random();
            StringBuilder str = new StringBuilder();

            do
            {
                str.Clear();

                for (int i = 0; i < length + 1; i++)
                    str.Append(Chars[rand.Next(0, Chars.Length - 1)]);
            }
            while (UsedTags.Contains(str.ToString()));

            var retStr = str.ToString();

            UsedTags.Add(retStr);

            return retStr;
        }

        public Numberplate(string ID) : base(ID, IDList[ID], typeof(Numberplate))
        {

        }
    }
    #endregion

    #region Vehicle Key
    public class VehicleKey : Item, ITagged
    {
        new public class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f";

            public ItemData(string Name, float Weight, string Model) : base(Name, Weight, Model) { }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "vk_0", new ItemData("Ключ", 0.01f, "p_car_keys_01") },
        };

        public Vector3 GetVehiclePos() => VehicleInfo?.VehicleData?.Vehicle?.Position;

        [JsonIgnore]
        public VehicleData.VehicleInfo VehicleInfo => VehicleData.VehicleInfo.Get(VID);

        public bool IsKeyValid(VehicleData.VehicleInfo vInfo) => vInfo.AllKeys.Contains(UID);

        public string Tag { get; set; }

        public uint VID { get; set; }

        public VehicleKey(string ID) : base(ID, IDList[ID], typeof(VehicleKey))
        {

        }
    }
    #endregion

    public abstract class StatusChanger : Item
    {
        new public abstract class ItemData : Item.ItemData
        {
            public int Satiety { get; set; }

            public int Mood { get; set; }

            public int Health { get; set; }

            public ItemData(string Name, float Weight, string[] Models, int Satiety = 0, int Mood = 0, int Health = 0) : base(Name, Weight, Models)
            {
                this.Satiety = Satiety;
                this.Mood = Mood;
                this.Health = Health;
            }
        }

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        public abstract void Apply(PlayerData pData);

        public StatusChanger(string ID, Item.ItemData Data, Type Type) : base(ID, Data, Type)
        {

        }
    }

    public class Food : StatusChanger, IStackable
    {
        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public Sync.Animations.FastTypes Animation { get; set; }

            public Sync.AttachSystem.Types AttachType { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Satiety}, {Mood}, {Health}, {MaxAmount}";

            public ItemData(string Name, float Weight, string Model, int Satiety, int Mood, int Health, int MaxAmount, Sync.Animations.FastTypes Animation, Sync.AttachSystem.Types AttachType) : base(Name, Weight, new string[] { Model }, Satiety, Mood, Health)
            {
                this.MaxAmount = MaxAmount;

                this.Animation = Animation;

                this.AttachType = AttachType;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "f_burger", new ItemData("Бургер", 0.15f, "prop_cs_burger_01", 25, 0, 0, 64, Sync.Animations.FastTypes.ItemBurger, Sync.AttachSystem.Types.ItemBurger) },
            { "f_chips", new ItemData("Чипсы",0.15f, "prop_food_bs_chips", 15, 0, 0, 64, Sync.Animations.FastTypes.ItemChips, Sync.AttachSystem.Types.ItemChips) },
            { "f_pizza", new ItemData("Пицца", 0.15f, "v_res_tt_pizzaplate", 50, 15, 0, 64, Sync.Animations.FastTypes.ItemPizza, Sync.AttachSystem.Types.ItemPizza) },
            { "f_chocolate", new ItemData("Шоколадка", 0.15f,  "prop_candy_pqs", 10, 20, 0, 64, Sync.Animations.FastTypes.ItemChocolate, Sync.AttachSystem.Types.ItemChocolate) },
            { "f_hotdog", new ItemData("Хот-дог", 0.15f, "prop_cs_hotdog_01", 10, 20, 0, 64, Sync.Animations.FastTypes.ItemChocolate, Sync.AttachSystem.Types.ItemChocolate) },

            { "f_cola", new ItemData("Кола", 0.15f, "prop_food_juice01", 5, 20, 0, 64, Sync.Animations.FastTypes.ItemCola, Sync.AttachSystem.Types.ItemCola) },

            { "f_beer", new ItemData("Пиво", 0.15f, "prop_sh_beer_pissh_01", 5, 50, 0, 64, Sync.Animations.FastTypes.ItemBeer, Sync.AttachSystem.Types.ItemBeer) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        [JsonIgnore]
        public override float Weight { get => BaseWeight * Amount; }

        public int Amount { get; set; }

        public override void Apply(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            player.AttachObject(data.Model, data.AttachType, Sync.Animations.FastTimeouts[data.Animation]);

            pData.PlayAnim(data.Animation);

            if (Data.Satiety > 0)
            {
                var satietyDiff = Utils.GetCorrectDiff(pData.Satiety, data.Satiety, 0, 100);

                if (satietyDiff != 0)
                {
                    pData.Satiety += satietyDiff;
                }
            }

            if (Data.Mood > 0)
            {
                var moodDiff = Utils.GetCorrectDiff(pData.Mood, data.Mood, 0, 100);

                if (moodDiff != 0)
                {
                    pData.Mood += moodDiff;
                }
            }
        }

        public Food(string ID) : base(ID, IDList[ID], typeof(Food))
        {
            this.Amount = MaxAmount;
        }
    }

    public class CigarettesPack : StatusChanger, IConsumable
    {
        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IConsumable
        {
            public const int UseCigModelIdx = 1;

            public int MaxAmount { get; set; }

            public int MaxPuffs { get; set; }

            public int MaxTime { get; set; }

            public Sync.AttachSystem.Types AttachType { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Mood}, {MaxAmount}";

            public ItemData(string Name, string[] Models, int Mood, int MaxPuffs, int MaxTime, Sync.AttachSystem.Types AttachType, int MaxAmount) : base(Name, 0.1f, Models, 0, Mood, 0)
            {
                this.MaxAmount = MaxAmount;

                this.MaxPuffs = MaxPuffs;
                this.MaxTime = MaxTime;

                this.AttachType = AttachType;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "cigs_0", new ItemData("Сигареты Redwood", new string[] { "v_ret_ml_cigs", "ng_proc_cigarette01a" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCigMouth, 20) },

            { "cigs_1", new ItemData("Сигареты Chartman", new string[] { "prop_cigar_pack_01", "prop_sh_cigar_01" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCig1Mouth, 20) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        public int Amount { get; set; }

        public override void Apply(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            player.AttachObject(data.GetModelAt(ItemData.UseCigModelIdx), data.AttachType, -1, data.MaxTime, data.MaxPuffs);

            var moodDiff = Utils.GetCorrectDiff(pData.Mood, data.Mood, 0, 100);

            if (moodDiff != 0)
            {
                pData.Mood += moodDiff;
            }
        }

        public CigarettesPack(string ID) : base(ID, IDList[ID], typeof(CigarettesPack))
        {
            this.Amount = MaxAmount;
        }
    }

    public class Cigarette : StatusChanger, IStackable
    {
        public static Dictionary<Sync.AttachSystem.Types, Sync.AttachSystem.Types> DependentTypes = new Dictionary<Sync.AttachSystem.Types, Sync.AttachSystem.Types>()
        {
            { Sync.AttachSystem.Types.ItemCigMouth, Sync.AttachSystem.Types.ItemCigHand },
            { Sync.AttachSystem.Types.ItemCig1Mouth, Sync.AttachSystem.Types.ItemCig1Hand },
            { Sync.AttachSystem.Types.ItemCig2Mouth, Sync.AttachSystem.Types.ItemCig2Hand },
            { Sync.AttachSystem.Types.ItemCig3Mouth, Sync.AttachSystem.Types.ItemCig3Hand },

            { Sync.AttachSystem.Types.ItemCigHand, Sync.AttachSystem.Types.ItemCigMouth },
            { Sync.AttachSystem.Types.ItemCig1Hand, Sync.AttachSystem.Types.ItemCig1Mouth },
            { Sync.AttachSystem.Types.ItemCig2Hand, Sync.AttachSystem.Types.ItemCig2Mouth },
            { Sync.AttachSystem.Types.ItemCig3Hand, Sync.AttachSystem.Types.ItemCig3Mouth },
        };

        public static List<Sync.AttachSystem.Types> AttachTypes { get; set; } = new List<Sync.AttachSystem.Types>(Cigarette.DependentTypes.Keys);

        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public const int UseCigModelIdx = 1;

            public int MaxAmount { get; set; }

            public int MaxPuffs { get; set; }

            public int MaxTime { get; set; }

            public Sync.AttachSystem.Types AttachType { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Mood}, {MaxAmount}";

            public ItemData(string Name, string[] Models, int Mood, int MaxPuffs, int MaxTime, Sync.AttachSystem.Types AttachType, int MaxAmount) : base(Name, 0.01f, Models, 0, Mood, 0)
            {
                this.MaxAmount = MaxAmount;

                this.MaxPuffs = MaxPuffs;
                this.MaxTime = MaxTime;

                this.AttachType = AttachType;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "cig_0", new ItemData("Сигарета Redwood", new string[] { "prop_cs_ciggy_01", "ng_proc_cigarette01a" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCigMouth, 20) },

            { "cig_1", new ItemData("Сигарета Chartman", new string[] { "prop_sh_cigar_01", "prop_sh_cigar_01" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCig1Mouth, 20) },

            { "cig_c_0", new ItemData("Сигара", new string[] { "prop_cigar_02", "prop_cigar_01" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCig2Mouth, 20) },

            { "cig_j_0", new ItemData("Косяк", new string[] { "p_cs_joint_02", "prop_sh_joint_01" }, 25, 15, 300000, Sync.AttachSystem.Types.ItemCig3Mouth, 20) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        [JsonIgnore]
        public override float Weight { get => BaseWeight * Amount; }

        public int Amount { get; set; }

        public override void Apply(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            player.AttachObject(data.GetModelAt(ItemData.UseCigModelIdx), data.AttachType, -1, data.MaxTime, data.MaxPuffs);

            var moodDiff = Utils.GetCorrectDiff(pData.Mood, data.Mood, 0, 100);

            if (moodDiff != 0)
            {
                pData.Mood += moodDiff;
            }
        }

        public Cigarette(string ID) : base(ID, IDList[ID], typeof(Cigarette))
        {

        }
    }

    public class Healing : StatusChanger, IStackable
    {
        new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public Sync.Animations.FastTypes Animation { get; set; }

            public Sync.AttachSystem.Types AttachType { get; set; }

            public bool RemovesWounded { get; set; }

            public bool RemovesKnocked { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {Health}, {MaxAmount}";

            public ItemData(string Name, float Weight, string Model, int Health, bool RemovesWounded, bool RemovesKnocked, int MaxAmount, Sync.Animations.FastTypes Animation, Sync.AttachSystem.Types AttachType) : base(Name, Weight, new string[] { Model }, 0, 0, Health)
            {
                this.MaxAmount = MaxAmount;

                this.Animation = Animation;

                this.AttachType = AttachType;

                this.RemovesWounded = RemovesWounded;
                this.RemovesKnocked = RemovesKnocked;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "med_b_0", new ItemData("Бинт", 0.1f, "prop_gaffer_arm_bind", 10, true, false, 256, Sync.Animations.FastTypes.ItemBandage, Sync.AttachSystem.Types.ItemBandage) },

            { "med_kit_0", new ItemData("Аптечка", 0.25f, "prop_ld_health_pack", 50, false, false, 128, Sync.Animations.FastTypes.ItemMedKit, Sync.AttachSystem.Types.ItemMedKit) },
            { "med_kit_1", new ItemData("Аптечка ПП", 0.25f, "prop_ld_health_pack", 50, true, true, 128, Sync.Animations.FastTypes.ItemMedKit, Sync.AttachSystem.Types.ItemMedKit) },
            { "med_kit_2", new ItemData("Аптечка EMS", 0.25f, "prop_ld_health_pack", 85, true, true, 128, Sync.Animations.FastTypes.ItemMedKit, Sync.AttachSystem.Types.ItemMedKit) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        [JsonIgnore]
        public override float Weight { get => BaseWeight * Amount; }

        public int Amount { get; set; }

        public override void Apply(PlayerData pData)
        {
            var player = pData.Player;

            var data = Data;

            player.AttachObject(data.Model, data.AttachType, Sync.Animations.FastTimeouts[data.Animation]);

            pData.PlayAnim(data.Animation);

            var hp = player.Health;

            var healthDiff = Utils.GetCorrectDiff(hp, data.Health, 0, 100);

            if (healthDiff != 0)
            {
                player.SetHealth(hp + healthDiff);
            }
        }

        public void ApplyToOther(PlayerData pData, PlayerData tData)
        {
            var player = pData.Player;
            var target = tData.Player;

            var data = Data;

            player.AttachObject(data.Model, data.AttachType, Sync.Animations.FastTimeouts[data.Animation]);

            pData.PlayAnim(data.Animation);

            var hp = target.Health;

            var healthDiff = Utils.GetCorrectDiff(hp, data.Health, 0, 100);

            if (healthDiff != 0)
            {
                target.SetHealth(hp + healthDiff);
            }
        }

        public Healing(string ID) : base(ID, IDList[ID], typeof(Healing))
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
        public static Item GiveItem(PlayerData pData, string id, int variation = 0, int amount = 1, bool isTemp = false, bool notifyOnSuccess = true, bool notifyOnFault = true)
        {
            var player = pData.Player;

            var type = GetType(id, false);

            if (type == null)
                return null;

            var data = GetData(id, type);

            if (data == null)
                return null;

            var totalWeight = 0f;
            var freeIdx = -1;

            for (int i = 0; i < pData.Items.Length; i++)
            {
                var curItem = pData.Items[i];

                if (curItem != null)
                {
                    totalWeight += curItem.Weight;
                }
                else if (freeIdx < 0)
                {
                    freeIdx = i;
                }
            }

            if (freeIdx < 0 || totalWeight + data.Weight * amount >= Settings.MAX_INVENTORY_WEIGHT)
            {
                if (notifyOnFault)
                    player.Notify("Inventory::NoSpace");

                return null;
            }

            var item = CreateItem(id, type, data, variation, amount, isTemp);

            if (item == null)
                return null;

            pData.Items[freeIdx] = item;

            var upd = Game.Items.Item.ToClientJson(item, Game.Items.Inventory.Groups.Items);

            if (notifyOnSuccess)
                player.TriggerEvent("Item::Added", item.ID, GetItemAmount(item));

            player.TriggerEvent("Inventory::Update", (int)Game.Items.Inventory.Groups.Items, freeIdx, upd);

            MySQL.CharacterItemsUpdate(pData.Info);

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
                if (amount < 0)
                    amount = 0;

                var maxAmount = weapon.Data.MaxAmmo;

                weapon.Ammo = amount > maxAmount ? maxAmount : amount;
            }

            if (!isTemp)
            {
                item.UID = Item.MoveNextId();

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

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_ITEMS_DATA_PATH, "TO_REPLACE", lines);

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

            if (type == null)
                return null;

            return JsonConvert.DeserializeObject(jo.ToString(), type, SpecifiedSubclassConversion);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {}
    }
    #endregion
}
