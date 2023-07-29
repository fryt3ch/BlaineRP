using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;

namespace BlaineRP.Server.Game.Items
{
    public partial class VehicleRepairKit : Item, IConsumable
    {
        public new class ItemData : Item.ItemData, Item.ItemData.IConsumable
        {
            public int MaxAmount { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {MaxAmount}";

            public ItemData(string name, string model, int maxAmount, float weight) : base(name, weight, model)
            {
                MaxAmount = maxAmount;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        public int Amount { get; set; }

        public void Apply(PlayerData pData, VehicleData vData)
        {
            vData.Vehicle.SetFixed();
        }

        public VehicleRepairKit(string id) : base(id, IdList[id], typeof(VehicleRepairKit))
        {
            Amount = MaxAmount;
        }
    }
}
