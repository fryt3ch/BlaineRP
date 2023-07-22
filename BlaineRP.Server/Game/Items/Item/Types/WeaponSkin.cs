using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public class WeaponSkin : Item
    {
        new public class ItemData : Item.ItemData
        {
            public override string ClientData => $"\"{Name}\", {Weight}f, WeaponSkin.ItemData.Types.{Type}";

            public enum Types
            {
                UniDef = 0,
                UniMk2,
            }

            public Types Type { get; set; }

            public int Variation { get; set; }

            public ItemData(string Name, float Weight, string Model, Types Type, int Variation) : base(Name, Weight, Model)
            {
                this.Type = Type;
                this.Variation = Variation;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "ws_0_0", new ItemData("Зеленая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 1 ) },
            { "ws_0_1", new ItemData("Золотая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 2 ) },
            { "ws_0_2", new ItemData("Розовая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 3 ) },
            { "ws_0_3", new ItemData("Армейская раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 4 ) },
            { "ws_0_4", new ItemData("Синяя раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 5 ) },
            { "ws_0_5", new ItemData("Оранжевая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 6 ) },
            { "ws_0_6", new ItemData("Платиновая раскраска (уни, обыч.)", 0f, "w_am_case", ItemData.Types.UniDef, 7 ) },

            { "ws_1_1", new ItemData("Черно-белая раскраска (уни, Mk2)", 0f, "w_am_case", ItemData.Types.UniMk2, 2 ) },
        };

        public static ItemData GetData(string id) => (ItemData)IDList[id];

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public override float Weight { get => Amount * BaseWeight; }

        public int Amount { get; set; }

        public WeaponSkin(string ID) : base(ID, IDList[ID], typeof(WeaponSkin))
        {

        }
    }
}
