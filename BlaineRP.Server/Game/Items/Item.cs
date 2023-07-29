using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.Inventory;

namespace BlaineRP.Server.Game.Items
{
    [JsonConverter(typeof(ItemConverter))]
    public abstract partial class Item
    {
        public static UtilsT.UidHandlers.UInt32 UidHandler { get; private set; } = new UtilsT.UidHandlers.UInt32(1);

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

        private static Dictionary<GroupTypes, Func<Item, string>> ClientJsonFuncs = new Dictionary<GroupTypes, Func<Item, string>>()
        {
            { GroupTypes.Items, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}&{((item is IUsable itemU && itemU.InUse) ? 1 : 0)}" },

            { GroupTypes.Bag, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}" },

            { GroupTypes.Container, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}" },

            { GroupTypes.CraftItems, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}" },

            { GroupTypes.CraftTools, (item) => $"{item.ID}" },

            { GroupTypes.CraftResult, (item) => $"{item.ID}&{Stuff.GetItemAmount(item)}&{(item is IStackable ? item.BaseWeight : item.Weight)}&{Stuff.GetItemTag(item)}" },

            {
                GroupTypes.Weapons,

                (item) =>
                {
                    var weapon = (Weapon)item;

                    return $"{item.ID}&{weapon.Ammo}&{(weapon.Equiped ? 1 : 0)}&{weapon.Tag}&{weapon.GetWeaponComponentsString()}";
                }
            },

            {
                GroupTypes.Holster,

                (item) =>
                {
                    var weapon = (Weapon)item;

                    return $"{item.ID}&{weapon.Ammo}&{(weapon.Equiped ? 1 : 0)}&{weapon.Tag}&{weapon.GetWeaponComponentsString()}";
                }
            },

            { GroupTypes.Armour, (item) => $"{item.ID}&{((Armour)item).Strength}" },

            {
                GroupTypes.BagItem,

                (item) =>
                {
                    var bag = (Bag)item;

                    return $"{item.ID}&{bag.Data.MaxWeight}|{string.Join('|', bag.Items.Select(x => ToClientJson(x, GroupTypes.Bag)))}";
                }
            },

            { GroupTypes.Clothes, (item) => $"{item.ID}" },

            { GroupTypes.Accessories, (item) => $"{item.ID}" },

            {
                GroupTypes.HolsterItem,

                (item) =>
                {
                    var holster = (Holster)item;

                    return $"{item.ID}|{ToClientJson(holster.Items[0], GroupTypes.Holster)}";
                }
            },
        };

        [JsonIgnore]
        public World.Service.ItemOnGround OnGroundInstance => World.Service.GetItemOnGround(UID);

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

            if (OnGroundInstance is World.Service.ItemOnGround iog)
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

        public string ToClientJson(GroupTypes group)
        {
            var func = ClientJsonFuncs.GetValueOrDefault(group);

            if (func == null)
                return "";

            return func.Invoke(this);
        }

        public static string ToClientJson(Item item, GroupTypes group) => item == null ? "" : item.ToClientJson(group);

        public Item(string ID, ItemData Data, Type Type)
        {
            this.ID = ID;
            this.Data = Data;
            this.Type = Type;
        }
    }
}
