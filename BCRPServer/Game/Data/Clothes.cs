using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Data
{
    public class Clothes
    {
        public static Dictionary<string, Data> AllClothes = new Dictionary<string, Data>()
        {

        };

        #region Classes
        public abstract class Data
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

            public Game.Items.Item.Types ItemType { get; set;}

            public bool Sex { get; set; }
            public int Drawable { get; set; }
            public int[] Textures { get; set; }
            public string SexAlternativeID { get; set; }

            public bool IsProp { get; set; }

            public Data(Items.Item.Types ItemType, bool Sex, int Drawable, int[] Textures, string SexAlternativeID = null)
            {
                this.Drawable = Drawable;
                this.Textures = Textures;

                this.ItemType = ItemType;

                this.Sex = Sex;

                this.SexAlternativeID = SexAlternativeID;

                this.IsProp = ItemType == Game.Items.Item.Types.Hat || ItemType == Game.Items.Item.Types.Glasses || ItemType == Game.Items.Item.Types.Ears || ItemType == Game.Items.Item.Types.Watches || ItemType == Game.Items.Item.Types.Bracelet;
            }
        }

        public class Top : Data
        {
            public int BestTorso { get; set; }
            public ExtraData ExtraData { get; set; }

            public Top(bool Sex, int Drawable, int[] Textures, int BestTorso, ExtraData ExtraData = null, string SexAlternative = null) : base(Items.Item.Types.Top, Sex, Drawable, Textures, SexAlternative)
            {
                this.BestTorso = BestTorso;
                this.ExtraData = ExtraData;
            }
        }

        public class Under : Data
        {
            public Top BestTop { get; set; }
            public int BestTorso { get; set; }
            public ExtraData ExtraData { get; set; }

            public Under(bool Sex, int Drawable, int[] Textures, Top BestTop, int BestTorso, ExtraData ExtraData = null, string SexAlternative = null) : base(Items.Item.Types.Under, Sex, Drawable, Textures, SexAlternative)
            {
                this.BestTop = BestTop;
                this.ExtraData = ExtraData;

                this.BestTorso = BestTorso;
            }
        }

        public class Gloves : Data
        {
            public Dictionary<int, int> BestTorsos { get; set; }

            public Gloves(bool Sex, int Drawable, int[] Textures, Dictionary<int, int> BestTorsos, string SexAlternative = null) : base(Items.Item.Types.Gloves, Sex, Drawable, Textures, SexAlternative)
            {
                this.BestTorsos = BestTorsos;
            }
        }

        public class Pants : Data
        {
            public Pants(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Pants, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Shoes : Data
        {
            public Shoes(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Shoes, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Hat : Data
        {
            public ExtraData ExtraData { get; set; }

            public Hat(bool Sex, int Drawable, int[] Textures, ExtraData ExtraData = null, string SexAlternative = null) : base(Items.Item.Types.Hat, Sex, Drawable, Textures, SexAlternative)
            {
                this.ExtraData = ExtraData;
            }
        }

        public class Accessory : Data
        {
            public Accessory(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Accessory, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Glasses : Data
        {
            public Glasses(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Glasses, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Watches : Data
        {
            public Watches(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Watches, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Bracelet : Data
        {
            public Bracelet(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Bracelet, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        public class Ears : Data
        {
            public Ears(bool Sex, int Drawable, int[] Textures, string SexAlternative = null) : base(Items.Item.Types.Ears, Sex, Drawable, Textures, SexAlternative)
            {

            }
        }

        #endregion

        private static Dictionary<Items.Item.Types, int> Slots = new Dictionary<Items.Item.Types, int>()
        {
            // SetClothes
            { Items.Item.Types.Top, 11 }, { Items.Item.Types.Under, 8 },  { Items.Item.Types.Pants, 4 },  { Items.Item.Types.Shoes, 6 },
            { Items.Item.Types.Gloves, 3 },  { Items.Item.Types.Mask, 1 },  { Items.Item.Types.Accessory, 7 },  { Items.Item.Types.Bag, 0 },
            // SetProp
            { Items.Item.Types.Hat, 0 }, { Items.Item.Types.Glasses, 1 }, { Items.Item.Types.Ears, 2 }, { Items.Item.Types.Watches, 6 }, { Items.Item.Types.Bracelet, 7 },
        };

        private static Dictionary<bool, Dictionary<Items.Item.Types, int>> NudeDefault = new Dictionary<bool, Dictionary<Items.Item.Types, int>>()
        {
            { true, new Dictionary<Items.Item.Types, int>() { { Items.Item.Types.Top, 15 }, { Items.Item.Types.Under, 15 }, { Items.Item.Types.Gloves, 15 }, { Items.Item.Types.Pants, 21 }, { Items.Item.Types.Shoes, 34 }, { Items.Item.Types.Accessory, 0 }, { Items.Item.Types.Mask, 0 }, { Items.Item.Types.Bag, 0 }  } },
            { false, new Dictionary<Items.Item.Types, int>() { { Items.Item.Types.Top, 15 }, { Items.Item.Types.Under, 15 }, { Items.Item.Types.Gloves, 15 }, { Items.Item.Types.Pants, 15 }, { Items.Item.Types.Shoes, 35 }, { Items.Item.Types.Accessory, 0 }, { Items.Item.Types.Mask, 0 }, { Items.Item.Types.Bag, 0 } } },
        };

        #region Stuff
        public static int GetSlot(Items.Item.Types type)
        {
            if (!Slots.ContainsKey(type))
                return -1;

            return Slots[type];
        }

        public static Data GetData(string id)
        {
            if (!AllClothes.ContainsKey(id))
                return null;

            return AllClothes[id];
        }

        public static Data GetSexAlternative(string id)
        {
            if (!AllClothes.ContainsKey(id))
                return null;

            var clothes = AllClothes[id];

            if (clothes.SexAlternativeID == null)
                return null;

            if (AllClothes.ContainsKey(clothes.SexAlternativeID))
                return AllClothes[clothes.SexAlternativeID];

            return null;
        }

        public static int GetNudeDrawable(Items.Item.Types type, bool sex)
        {
            if (!NudeDefault[sex].ContainsKey(type))
                return 0;

            return NudeDefault[sex][type];
        }
        #endregion
    }
}
