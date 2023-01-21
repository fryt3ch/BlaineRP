using Newtonsoft.Json;
using System.Collections.Generic;

namespace BCRPServer.Game.Items
{
    public class VehicleRepairKit : Item, IConsumable
    {
        new public class ItemData : Item.ItemData, Item.ItemData.IConsumable
        {
            public int MaxAmount { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {MaxAmount}";

            public ItemData(string Name, string Model, int MaxAmount, float Weight) : base(Name, Weight, Model)
            {
                this.MaxAmount = MaxAmount;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "vrk_0", new ItemData("Ремонтный набор S", "imp_prop_tool_box_01b", 3, 1f) },
            { "vrk_1", new ItemData("Ремонтный набор M", "gr_prop_gr_tool_box_01a", 7, 1.5f) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        public int Amount { get; set; }

        public void Apply(PlayerData pData, VehicleData vData)
        {
            vData.Vehicle.SetFixed();
        }

        public VehicleRepairKit(string ID) : base(ID, IDList[ID], typeof(VehicleRepairKit))
        {
            this.Amount = MaxAmount;
        }
    }
}
