using GTANetworkAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Game.Items
{
    [JsonConverter(typeof(ItemConverter))]
    public abstract class Item
    {
        public static UidHandlerUInt32 UidHandler { get; private set; } = new UidHandlerUInt32(1);

        public static void Add(Item item)
        {
            if (item == null)
                return;

            item.UID = UidHandler.MoveNextUid();

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
                    Remove(x);
                }
            }
            else
            {
                if (item is Game.Items.Numberplate np)
                {
                    np.RemoveTagFromUsed();
                }

                UidHandler.SetUidAsFree(item.UID);

                MySQL.ItemDelete(item);
            }
        }

        private static Dictionary<Game.Items.Inventory.GroupTypes, Func<Item, string>> ClientJsonFuncs = new Dictionary<Game.Items.Inventory.GroupTypes, Func<Item, string>>()
        {
            { Game.Items.Inventory.GroupTypes.Items, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}&{((item is IUsable itemU && itemU.InUse) ? 1 : 0)}" },

            { Game.Items.Inventory.GroupTypes.Bag, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}" },

            { Game.Items.Inventory.GroupTypes.Container, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}" },

            { Game.Items.Inventory.GroupTypes.CraftItems, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}" },

            { Game.Items.Inventory.GroupTypes.CraftTools, (item) => $"{item.ID}" },

            { Game.Items.Inventory.GroupTypes.CraftResult, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}" },

            {
                Game.Items.Inventory.GroupTypes.Weapons,

                (item) =>
                {
                    var weapon = (Weapon)item;

                    return $"{item.ID}&{weapon.Ammo}&{(weapon.Equiped ? 1 : 0)}&{weapon.Tag}&{weapon.GetWeaponComponentsString()}";
                }
            },

            {
                Game.Items.Inventory.GroupTypes.Holster,

                (item) =>
                {
                    var weapon = (Weapon)item;

                    return $"{item.ID}&{weapon.Ammo}&{(weapon.Equiped ? 1 : 0)}&{weapon.Tag}&{weapon.GetWeaponComponentsString()}";
                }
            },

            { Game.Items.Inventory.GroupTypes.Armour, (item) => $"{item.ID}&{((Armour)item).Strength}" },

            {
                Game.Items.Inventory.GroupTypes.BagItem,

                (item) =>
                {
                    var bag = (Bag)item;

                    return $"{item.ID}&{bag.Data.MaxWeight}|{string.Join('|', bag.Items.Select(x => ToClientJson(x, Game.Items.Inventory.GroupTypes.Bag)))}";
                }
            },

            { Game.Items.Inventory.GroupTypes.Clothes, (item) => $"{item.ID}" },

            { Game.Items.Inventory.GroupTypes.Accessories, (item) => $"{item.ID}" },

            {
                Game.Items.Inventory.GroupTypes.HolsterItem,

                (item) =>
                {
                    var holster = (Holster)item;

                    return $"{item.ID}|{ToClientJson(holster.Items[0], Game.Items.Inventory.GroupTypes.Holster)}";
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

            public interface ICraftIngredient
            {

            }

            /// <summary>Стандартная модель</summary>
            public static uint DefaultModel => NAPI.Util.GetHashKey("prop_drug_package_02");

            /// <summary>Название предмета</summary>
            public string Name { get; set; }

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
        public Sync.World.ItemOnGround OnGroundInstance => Sync.World.GetItemOnGround(UID);

        [JsonIgnore]
        public Type Type { get; set; }

        /// <summary>Данные предмета</summary>
        [JsonIgnore]
        public ItemData Data { get; set; }

        /// <summary>Стандартный вес предмета (1 единица)</summary>
        [JsonIgnore]
        public float BaseWeight => Data.Weight;

        /// <summary>Фактический вес предмета</summary>
        [JsonIgnore]
        public virtual float Weight => BaseWeight;

        /// <summary>ID модели предмета</summary>
        [JsonIgnore]
        public uint Model => Data.Model;

        /// <summary>Является ли предмет временным?</summary>
        [JsonIgnore]
        public bool IsTemp => UID == 0;

        /// <summary>Уникальный ID предмета</summary>
        /// <value>UID предмета, 0 - если предмет временный и не хранится в базе данных</value>
        [JsonIgnore]
        public uint UID { get; set; }

        [JsonProperty(PropertyName = "I", Order = int.MinValue)]
        /// <summary>ID предмета (см. Game.Items.Items.LoadAll)</summary>
        public string ID { get; set; }

        /// <summary>Метод для удаления предмета из базы данных</summary>
        public virtual void Delete()
        {
            if (IsTemp)
                return;

            if (OnGroundInstance is Sync.World.ItemOnGround iog)
            {
                iog.Delete(false);
            }

            Remove(this);
        }

        /// <summary>Метод для обновления предмета в базе данных</summary>
        public void Update()
        {
            if (IsTemp)
                return;

            MySQL.ItemUpdate(this);
        }

        public string ToClientJson(Game.Items.Inventory.GroupTypes group)
        {
            var func = ClientJsonFuncs.GetValueOrDefault(group);

            if (func == null)
                return "";

            return func.Invoke(this);
        }

        public static string ToClientJson(Item item, Game.Items.Inventory.GroupTypes group) => item == null ? "" : item.ToClientJson(group);

        public Item(string ID, ItemData Data, Type Type)
        {
            this.ID = ID;
            this.Data = Data;
            this.Type = Type;
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

            var type = Stuff.GetType(jo["I"].Value<string>());

            if (type == null)
                return null;

            return JsonConvert.DeserializeObject(jo.ToString(), type, SpecifiedSubclassConversion);
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
    }
    #endregion
}
