using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
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
}
