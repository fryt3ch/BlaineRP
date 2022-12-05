using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BCRPClient.Data
{
    class Items : Events.Script
    {
        public abstract class Item
        {
            public class ItemData
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

                public string Name { get; set; }

                public float Weight { get; set; }

                public ItemData(string Name, float Weight)
                {
                    this.Name = Name;
                    this.Weight = Weight;
                }
            }
        }

        public interface IWearable
        {

        }

        public interface IStackable
        {

        }

        public interface IContainer
        {

        }

        public interface ITagged
        {

        }

        public interface IConsumable
        {

        }

        public abstract class Clothes : Item, IWearable
        {
            public interface IToggleable
            {

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

                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight)
                {
                    this.Drawable = Drawable;
                    this.Textures = Textures;

                    this.Sex = Sex;

                    this.SexAlternativeID = SexAlternativeID;
                }
            }
        }

        public class Hat : Clothes, Clothes.IToggleable, Clothes.IProp
        {
            new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
            {
                public ExtraData ExtraData { get; set; }

                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID)
                {
                    this.ExtraData = ExtraData;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Top : Clothes, Clothes.IToggleable
        {
            new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
            {
                public int BestTorso { get; set; }

                public ExtraData ExtraData { get; set; }

                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID)
                {
                    this.BestTorso = BestTorso;
                    this.ExtraData = ExtraData;
                }

                public ItemData(bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : this(null, 0f, Sex, Drawable, Textures, BestTorso, ExtraData, SexAlternativeID) { }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Under : Clothes, Clothes.IToggleable
        {
            new public class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
            {
                public Top.ItemData BestTop { get; set; }

                public int BestTorso { get; set; }

                public ExtraData ExtraData { get; set; }

                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, Top.ItemData BestTop, int BestTorso, ExtraData ExtraData = null, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID)
                {
                    this.BestTop = BestTop;
                    this.ExtraData = ExtraData;

                    this.BestTorso = BestTorso;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Gloves : Clothes
        {
            new public class ItemData : Clothes.ItemData
            {
                public Dictionary<int, int> BestTorsos { get; set; }

                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, Dictionary<int, int> BestTorsos, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID)
                {
                    this.BestTorsos = BestTorsos;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Pants : Clothes
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID) { }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Shoes : Clothes
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID) { }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Accessory : Clothes
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID) { }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Glasses : Clothes, Clothes.IProp
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID) { }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Watches : Clothes, Clothes.IProp
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID) { }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Bracelet : Clothes, Clothes.IProp
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID) { }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Earrings : Clothes, Clothes.IProp
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID) { }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Weapon : Item, ITagged, IWearable
        {
            new public class ItemData : Item.ItemData
            {
                public int MaxAmmo { get; set; }

                public string AmmoID { get; set; }

                public uint Hash { get; set; }

                public ItemData(string Name, float Weight, string AmmoID, int MaxAmmo, uint Hash) : base(Name, Weight)
                {
                    this.MaxAmmo = MaxAmmo;

                    this.AmmoID = AmmoID;

                    this.Hash = Hash;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Ammo : Item, IStackable
        {
            new public class ItemData : Item.ItemData, Item.ItemData.IStackable
            {
                public int MaxAmount { get; set; }

                public ItemData(string Name, float Weight, int MaxAmount) : base(Name, Weight)
                {
                    this.MaxAmount = MaxAmount;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Armour : Clothes
        {
            new public class ItemData : Clothes.ItemData
            {
                public int MaxStrength { get; set; }

                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, int MaxStrength, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID)
                {
                    this.MaxStrength = MaxStrength;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Bag : Clothes, IContainer
        {
            new public class ItemData : Clothes.ItemData
            {
                /// <summary>Максимальное кол-во слотов</summary>
                public byte MaxSlots { get; set; }

                /// <summary>Максимальный вес содержимого</summary>
                public float MaxWeight { get; set; }

                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, byte MaxSlots, float MaxWeight, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID)
                {
                    this.MaxSlots = MaxSlots;

                    this.MaxWeight = MaxWeight;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Holster : Clothes, IContainer
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID)
                {

                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Numberplate : Item, ITagged
        {
            new public class ItemData : Item.ItemData
            {
                public int Number { get; set; }

                public ItemData(string Name, float Weight, int Number) : base(Name, Weight)
                {
                    this.Number = Number;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class VehicleKey : Item, ITagged
        {
            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public abstract class StatusChanger : Item
        {
            new public class ItemData : Item.ItemData
            {
                public int Satiety { get; set; }

                public int Mood { get; set; }

                public int Health { get; set; }

                public ItemData(string Name, float Weight, int Satiety = 0, int Mood = 0, int Health = 0) : base(Name, Weight)
                {
                    this.Satiety = Satiety;
                    this.Mood = Mood;
                    this.Health = Health;
                }
            }
        }

        public class Food : StatusChanger, IStackable
        {
            new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
            {
                public int MaxAmount { get; set; }

                public Sync.Animations.FastTypes Animation { get; set; }

                public Sync.AttachSystem.Types AttachType { get; set; }

                public ItemData(string Name, float Weight, int Satiety, int Mood, int Health, int MaxAmount) : base(Name, Weight, Satiety, Mood, Health)
                {
                    this.MaxAmount = MaxAmount;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class CigarettesPack : StatusChanger, IConsumable
        {
            new public class ItemData : StatusChanger.ItemData, Item.ItemData.IConsumable
            {
                public int MaxAmount { get; set; }

                public ItemData(string Name, float Weight, int Mood, int MaxAmount) : base(Name, Weight, 0, Mood, 0)
                {
                    this.MaxAmount = MaxAmount;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Cigarette : StatusChanger, IStackable
        {
            new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
            {
                public int MaxAmount { get; set; }

                public ItemData(string Name, float Weight, int Mood, int MaxAmount) : base(Name, Weight, 0, Mood, 0)
                {
                    this.MaxAmount = MaxAmount;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class Healing : StatusChanger, IStackable
        {
            new public class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
            {
                public int MaxAmount { get; set; }

                public ItemData(string Name, float Weight, int Health, int MaxAmount) : base(Name, Weight, 0, 0, Health)
                {
                    this.MaxAmount = MaxAmount;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        #region Enums
        public enum ActionTypes
        {
            Bind = 0,
            Split = 1,
            Throw = 2,
            Use = 3,
        }
        #endregion

        private static Dictionary<string, Type> AllTypes { get; set; } = new Dictionary<string, Type>();

        public static Dictionary<Type, Dictionary<string, Item.ItemData>> AllData { get; private set; } = new Dictionary<Type, Dictionary<string, Item.ItemData>>();

        private static Dictionary<Type, string[]> AbstractImageTypes = new Dictionary<Type, string[]>() // string[] - exceptions
        {
            { typeof(Clothes), new string[] { } },

            { typeof(Bag), new string[] { } },

            { typeof(Holster), new string[] { } },
        };

        public Items()
        {
            #region TO_REPLACE

            #endregion

            foreach (var x in typeof(Items).GetNestedTypes().Where(x => x.IsClass && !x.IsAbstract && typeof(Item).IsAssignableFrom(x)))
            {
                var idList = (Dictionary<string, Item.ItemData>)x.GetProperty("IDList")?.GetValue(null);

                if (idList == null)
                    continue;

                AllData.Add(x, idList);

                foreach (var t in idList)
                {
                    var id = t.Key.Split('_');

                    if (!AllTypes.ContainsKey(id[0]))
                        AllTypes.Add(id[0], x);
                }
            }
        }

        #region Stuff
        public static string GetImageId(string id, Type type = null)
        {
            if (type == null)
            {
                type = GetType(id, false);

                if (type == null)
                    return "null";
            }

            var aType = AbstractImageTypes.Where(x => (x.Key == type || x.Key.IsAssignableFrom(type)) && !x.Value.Contains(id)).Select(x => x.Key).FirstOrDefault();

            if (aType != null)
                return type.Name;

            return id;
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

        public static string GetName(string id) => GetData(id, null)?.Name ?? "null";

        public static object[][] GetActions(Type type, int amount, bool hasContainer = false, bool isContainer = false)
        {
            List<object[]> actions;

            if (!isContainer)
            {
                var action = Actions.Where(x => x.Key == type || x.Key.IsAssignableFrom(type)).Select(x => x.Value).FirstOrDefault();

                if (action != null)
                    actions = new List<object[]>() { action };
                else
                    actions = new List<object[]>();
            }
            else
                actions = new List<object[]>();

            if (hasContainer)
                actions.Add(new object[] { 4, Locale.General.Inventory.Actions.Shift });

            if (amount > 1)
                actions.Add(new object[] { 1, Locale.General.Inventory.Actions.Split });

            actions.Add(new object[] { 2, Locale.General.Inventory.Actions.Drop });

            return actions.ToArray();
        }
        #endregion

        #region All Custom Actions
        private static Dictionary<Type, object[][]> Actions = new Dictionary<Type, object[][]>()
        {
            { typeof(Weapon), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.Use } } },

            { typeof(StatusChanger), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.Use } } },

            { typeof(IWearable), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.TakeOn } } },

            { typeof(VehicleKey), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.FindVehicle } } },
        };
        #endregion
    }
}
