using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCRPClient.Data
{
    public class Items : Events.Script
    {
        public abstract class Item
        {
            public class ItemData
            {
                /// <summary>���� ��������� ��������� ������ ����� ���������, ������� ����� ������� � ���� ������ ��������</summary>
                public interface IContainer
                {
                    public float MaxWeight { get; }
                }

                /// <summary>���� ��������� ��������� ������ ����� ���������, ������� �������� ���������</summary>
                public interface IStackable
                {
                    /// <summary>������������ ���-�� ������ �������� � �����</summary>
                    public int MaxAmount { get; set; }
                }

                /// <summary>���� ��������� ��������� ������ ����� ���������, ������� �������� ���������</summary>
                /// <remarks>�� ������������ ������������ � IStackable!</remarks>
                public interface IConsumable
                {
                    public int MaxAmount { get; set; }
                }

                public interface ICraftIngredient
                {

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

        public interface IUsable
        {

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

        public interface ICraftIngredient
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

        public class Mask : Clothes, Clothes.IToggleable
        {
            new public class ItemData : Clothes.ItemData
            {
                public ItemData(string Name, float Weight, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null) : base(Name, Weight, Sex, Drawable, Textures, SexAlternativeID)
                {

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

        public class Ring : Clothes, Clothes.IToggleable, Clothes.IProp
        {
            new public class ItemData : Clothes.ItemData
            {
                public uint Model { get; private set; }

                public ItemData(string Name, float Weight, bool Sex, uint Model, string SexAlternativeID = null) : base(Name, Weight, Sex, 1, new int[] { 0 }, SexAlternativeID)
                {
                    this.Model = Model;
                }
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
                /// <summary>������������ ���-�� ������</summary>
                public byte MaxSlots { get; set; }

                /// <summary>������������ ��� �����������</summary>
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

        public class FoodIngredient : Food, ICraftIngredient
        {
            new public class ItemData : Food.ItemData, Item.ItemData.ICraftIngredient
            {
                public ItemData(string Name, float Weight, int Satiety, int Mood, int Health, int MaxAmount) : base(Name, Weight, Satiety, Mood, Health, MaxAmount)
                {

                }
            }

            new public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
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

        public class VehicleRepairKit : Item, IConsumable
        {
            new public class ItemData : Item.ItemData, Item.ItemData.IConsumable
            {
                public int MaxAmount { get; set; }

                public ItemData(string Name, float Weight, int MaxAmount) : base(Name, Weight)
                {
                    this.MaxAmount = MaxAmount;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class WeaponComponent : Item
        {
            new public class ItemData : Item.ItemData
            {
                public Sync.WeaponSystem.Weapon.ComponentTypes Type { get; set; }

                public ItemData(string Name, float Weight, Sync.WeaponSystem.Weapon.ComponentTypes Type) : base(Name, Weight)
                {
                    this.Type = Type;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class WeaponSkin : Item
        {
            new public class ItemData : Item.ItemData
            {
                public enum Types
                {
                    UniDef = 0,
                    UniMk2,
                }

                public Types Type { get; set; }

                public ItemData(string Name, float Weight, Types Type) : base(Name, Weight)
                {
                    this.Type = Type;
                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public class FishingRod : Item, IUsable
        {
            new public class ItemData : Item.ItemData
            {
                public ItemData(string Name, float Weight) : base(Name, Weight)
                {

                }
            }

            public static Dictionary<string, Item.ItemData> IDList { get; set; } = new Dictionary<string, Item.ItemData>();
        }

        public enum ActionTypes
        {
            Bind = 0,
            Split = 1,
            Throw = 2,
            Use = 3,
        }

        private static Dictionary<string, System.Type> AllTypes { get; set; } = new Dictionary<string, System.Type>();

        public static Dictionary<System.Type, Dictionary<string, Item.ItemData>> AllData { get; private set; } = new Dictionary<System.Type, Dictionary<string, Item.ItemData>>();

        private static Dictionary<System.Type, string[]> AbstractImageTypes = new Dictionary<System.Type, string[]>() // string[] - exceptions
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
        public static string GetImageId(string id, System.Type type = null)
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

        public static System.Type GetType(string id, bool checkFullId = true)
        {
            var data = id.Split('_');

            var type = AllTypes.GetValueOrDefault(data[0]);

            if (type == null || (checkFullId && !AllData[type].ContainsKey(id)))
                return null;

            return type;
        }

        public static Item.ItemData GetData(string id, System.Type type = null)
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

        public static object[][] GetActions(System.Type type, string id, int amount, bool isBag = false, bool inUse = false, bool hasContainer = false, bool isContainer = false)
        {
            List<object[]> actions = new List<object[]>();

            if (inUse)
            {
                actions.Add(new object[] { 5, Locale.General.Inventory.Actions.StopUse });
            }
            else
            {
                if (!isContainer)
                {
                    if (!isBag || !ItemsActionsNotBag.Where(x => x.Key.IsTypeOrAssignable(type) && !x.Value.Contains(id)).Any())
                    {
                        var action = Actions.Where(x => x.Key.IsTypeOrAssignable(type)).Select(x => x.Value).FirstOrDefault();

                        if (action != null)
                            actions.Add(action);
                    }
                }
            }

            if (hasContainer)
                actions.Add(new object[] { 4, Locale.General.Inventory.Actions.Shift });

            if (amount > 1)
                actions.Add(new object[] { 1, Locale.General.Inventory.Actions.Split });

            actions.Add(new object[] { 2, Locale.General.Inventory.Actions.Drop });

            return actions.ToArray();
        }
        #endregion

        #region All Custom Actions
        private static List<KeyValuePair<System.Type, object[][]>> Actions { get; set; } = new List<KeyValuePair<System.Type, object[][]>>()
        {
            new KeyValuePair<System.Type, object[][]>(typeof(FishingRod), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.FishingRodUseBait }, new object[] { 6, Locale.General.Inventory.Actions.FishingRodUseWorms } }),

            new KeyValuePair<System.Type, object[][]>(typeof(IUsable), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.Use } }),

            new KeyValuePair<System.Type, object[][]>(typeof(Weapon), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.Use } }),

            new KeyValuePair<System.Type, object[][]>(typeof(Food), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.Eat } }),

            new KeyValuePair<System.Type, object[][]>(typeof(StatusChanger), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.Use } }),

            new KeyValuePair<System.Type, object[][]>(typeof(IWearable), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.TakeOn } }),

            new KeyValuePair<System.Type, object[][]>(typeof(VehicleKey), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.FindVehicle } }),

            new KeyValuePair<System.Type, object[][]>(typeof(WeaponSkin), new object[][] { new object[] { 5, Locale.General.Inventory.Actions.Use } }),
        };

        private static List<KeyValuePair<System.Type, List<string>>> ItemsActionsNotBag { get; set; } = new List<KeyValuePair<System.Type, List<string>>>()
        {
            new KeyValuePair<System.Type, List<string>>(typeof(IUsable), new List<string> { } ),
        };

        private static List<KeyValuePair<System.Type, Func<List<string>>>> ItemsActionsValidators { get; set; } = new List<KeyValuePair<System.Type, Func<List<string>>>>()
        {
            new KeyValuePair<System.Type, Func<List<string>>>(typeof(FishingRod), () =>
            {
                var res = Utils.CanDoSomething(Utils.Actions.IsSwimming);

                if (!res)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Inventory.ActionRestricted);

                    return null;
                }

                var waterPos = Utils.FindEntityWaterIntersectionCoord(Player.LocalPlayer, new Vector3(0f, 0f, 1f), 7.5f, 7.5f, -3.5f, 360f, 0.5f, 31);

                if (waterPos == null)
                {
                    CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Inventory.FishingNotAllowedHere);

                    return null;
                }

                Player.LocalPlayer.SetData("MG::F::T::WZ", waterPos.Z);

                var eData = new List<string>() { };

                return eData;
            }),
        };

        public static Func<List<string>> GetActionToValidate(System.Type type) => ItemsActionsValidators.Where(x => x.Key.IsTypeOrAssignable(type)).Select(x => x.Value).FirstOrDefault();
        #endregion
    }
}
