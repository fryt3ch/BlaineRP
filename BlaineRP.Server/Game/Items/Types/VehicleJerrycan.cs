using Newtonsoft.Json;
using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;

namespace BlaineRP.Server.Game.Items
{
    public partial class VehicleJerrycan : Item, IConsumable
    {
        public new class ItemData : Item.ItemData, Item.ItemData.IConsumable
        {
            public int MaxAmount { get; set; }

            public bool IsPetrol { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {MaxAmount}, {IsPetrol.ToString().ToLower()}";

            public ItemData(string name, string model, int maxAmount, float weight, bool isPetrol) : base(name, weight, model)
            {
                MaxAmount = maxAmount;

                IsPetrol = isPetrol;
            }
        }

        [JsonIgnore]
        public new ItemData Data { get => (ItemData)base.Data; }

        [JsonIgnore]
        public int MaxAmount => Data.MaxAmount;

        public int Amount { get; set; }

        public void Apply(PlayerData pData, VehicleData vData, int amount)
        {
            if (amount <= 0)
                return;

            var newFuelLevel = vData.FuelLevel + amount;

            if (newFuelLevel > vData.Data.Tank)
                newFuelLevel = vData.Data.Tank;

            vData.FuelLevel = newFuelLevel;
        }

        public VehicleJerrycan(string id) : base(id, IdList[id], typeof(VehicleJerrycan))
        {
            Amount = MaxAmount;
        }
    }
}
