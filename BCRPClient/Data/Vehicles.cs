using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data
{
    public class Vehicles : Events.Script
    {
        public enum Types
        {
            Car = 0,
            Moto,
        }

        public class Vehicle
        {
            public enum FuelTypes
            {
                Petrol = 0,
                Electricity = 1
            }

            public class Trunk
            {
                /// <summary>Кол-во слотов в багажнике</summary>
                public int Slots { get; set; }

                /// <summary>Максимальный вес багажника</summary>
                public float MaxWeight { get; set; }

                public Trunk(int Slots, float MaxWeight)
                {
                    this.Slots = Slots;
                    this.MaxWeight = MaxWeight;
                }
            }

            public string Name { get; set; }

            public string ID { get; set; }

            public Types Type { get; set; }

            public string BrandName { get; set; }

            public string SubName { get; set; }

            public uint Model { get; set; }

            public float Tank { get; set; }

            public FuelTypes FuelType { get; set; }

            public Trunk TrunkData { get; set; }

            public bool Moddable { get; set; }

            public bool HasCruiseControl { get; set; }

            public bool HasAutoPilot { get; set; }

            public Vehicle(string ID, string Model, string Name, float Tank, FuelTypes FuelType, Trunk TrunkData = null, bool Moddable = true, bool HasCruiseControl = false, bool HasAutoPilot = false, Types Type = Types.Car)
            {
                this.ID = ID;
                this.Name = Name;
                this.Type = Type;

                var divider = Name.IndexOf(' ');

                this.BrandName = Name.Substring(0, divider == -1 ? Name.Length : divider + 1);
                this.SubName = divider == -1 ? "" : Name.Substring(divider + 1);

                this.Model = RAGE.Util.Joaat.Hash(Model);

                this.Tank = Tank;

                this.FuelType = FuelType;

                this.TrunkData = TrunkData;

                this.Moddable = Moddable;
                this.HasAutoPilot = HasAutoPilot;
                this.HasCruiseControl = HasCruiseControl;
            }
        }

        private static Dictionary<string, Vehicle> All;

        public Vehicles()
        {
            All = new Dictionary<string, Vehicle>()
            {
                { "buffalo", new Vehicle("buffalo", "buffalo", "Bravado Buffalo", 80, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25)) }
            };
        }

        public static Vehicle GetById(string id) => id == null ? null : All.GetValueOrDefault(id);

        public static Vehicle GetByModel(uint model) => All.Where(x => x.Value.Model == model).Select(x => x.Value).FirstOrDefault();
    }
}
