using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Items
{
    public class VehicleJerrycan : Item, IConsumable
    {
        new public class ItemData : Item.ItemData, Item.ItemData.IConsumable
        {
            public int MaxAmount { get; set; }

            public bool IsPetrol { get; set; }

            public override string ClientData => $"\"{Name}\", {Weight}f, {MaxAmount}, {IsPetrol.ToString().ToLower()}";

            public ItemData(string Name, string Model, int MaxAmount, float Weight, bool IsPetrol) : base(Name, Weight, Model)
            {
                this.MaxAmount = MaxAmount;

                this.IsPetrol = IsPetrol;
            }
        }

        public static Dictionary<string, Item.ItemData> IDList = new Dictionary<string, Item.ItemData>()
        {
            { "vjc_0", new ItemData("Канистра с топливом", "w_am_jerrycan", 30, 2.5f, true) },
            { "vjc_1", new ItemData("Аккумулятор", "prop_car_battery_01", 30, 2.5f, false) },
        };

        [JsonIgnore]
        new public ItemData Data { get => (ItemData)base.Data; }

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

        public VehicleJerrycan(string ID) : base(ID, IDList[ID], typeof(VehicleJerrycan))
        {
            this.Amount = MaxAmount;
        }
    }
}
