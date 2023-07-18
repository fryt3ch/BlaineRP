using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class MiscStackable : Item, IStackable
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
            { "mis_0", new ItemData("Приманка для рыбы", 0.02f, "prop_paints_can04", 1024) },
            { "mis_1", new ItemData("Червяк", 0.01f, "prop_paints_can04", 1024) },

            { "mis_gpstr", new ItemData("GPS-трекер", 0.05f, "lr_prop_carkey_fob", 5) },
            { "mis_lockpick", new ItemData("Отмычка", 0.01f, "prop_cuff_keys_01", 32) },
        };

        public static ItemData GetData(string id) => (ItemData)IDList[id];

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; set => base.Data = value; }

        [JsonIgnore]
        public override float Weight { get => Amount * BaseWeight; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        public int Amount { get; set; }

        public MiscStackable(string ID) : base(ID, IDList[ID], typeof(MiscStackable))
        {

        }
    }
}
