using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data
{
    public class Vehicles : Events.Script
    {
        public class Vehicle
        {
            public enum Types
            {
                Car = 0,
                Motorcycle,
                Boat,
                Plane,
                Helicopter,
                Cycle,
                Trailer,
                Train,
                Jetpack,
                Blimp,
            }

            public enum FuelTypes
            {
                None = -1,
                Petrol = 0,
                Electricity = 1
            }

            /// <summary>Классы транспорта</summary>
            /// <remarks>Зависит от цены</remarks>
            public enum ClassTypes
            {
                /// <summary>Обычный</summary>
                Classic = 0,
                /// <summary>Премиум</summary>
                Premium,
                /// <summary>Люкс</summary>
                Luxe,
                /// <summary>Элитный</summary>
                Elite,
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

            public string BrandName
            {
                get
                {
                    var divIdx = Name.IndexOf(' ');

                    if (divIdx < 0)
                        return Name;

                    return Name.Substring(0, divIdx + 1);
                }
            }

            public string SubName
            {
                get
                {
                    var divIdx = Name.IndexOf(' ');

                    if (divIdx < 0)
                        return "";

                    return Name.Substring(divIdx + 1);
                }
            }

            public uint Model { get; set; }

            public float Tank { get; set; }

            public FuelTypes FuelType { get; set; }

            public Trunk TrunkData { get; set; }

            public bool IsModdable { get; set; }

            public bool HasCruiseControl { get; set; }

            public bool HasAutoPilot { get; set; }

            public int GovPrice { get; private set; }

            public ClassTypes Class { get; private set; }

            public string TypeName => Locale.Property.VehicleTypesNames.GetValueOrDefault(Type) ?? "null";

            public Vehicle(string ID, uint Model, string Name, float Tank, FuelTypes FuelType, Trunk TrunkData = null, bool IsModdable = true, bool HasCruiseControl = false, bool HasAutoPilot = false, Types Type = Types.Car, int GovPrice = 0, ClassTypes Class = ClassTypes.Classic)
            {
                this.ID = ID;
                this.Name = Name;
                this.Type = Type;

                this.Model = Model;

                this.Tank = Tank;

                this.FuelType = FuelType;

                this.TrunkData = TrunkData;

                this.IsModdable = IsModdable;
                this.HasAutoPilot = HasAutoPilot;
                this.HasCruiseControl = HasCruiseControl;

                this.GovPrice = GovPrice;
                this.Class = Class;

                All.Add(ID, this);
            }

            public string GetEstateSvgName()
            {
                if (Type == Types.Car)
                    return "Veh";

                if (Type == Types.Motorcycle)
                    return "Moto";

                if (Type == Types.Helicopter)
                    return "Heli";

                return Type.ToString();
        }
        }

        public static Dictionary<string, Vehicle> All = new Dictionary<string, Vehicle>();

        public static Vehicle GetById(string id) => id == null ? null : All.GetValueOrDefault(id);

        public static Vehicle GetByModel(uint model) => All.Where(x => x.Value.Model == model).Select(x => x.Value).FirstOrDefault();

        public Vehicles()
        {
            #region TO_REPLACE

            #endregion

/*            var newModels = new List<string>
            {
                "boor", "brickade2", "broadway", "cargoplane2", "entity3", "eudora", "everon2", "issi8", "journey2", "manchez3", "panthere", "powersurge", "r300", "surfer3", "tahoma", "tulip2", "virtue",
            };

            foreach (var x in newModels)
            {
                var model = RAGE.Util.Joaat.Hash(x);

                JObject data = new JObject();

                var name = RAGE.Game.Ui.GetLabelText(RAGE.Game.Vehicle.GetDisplayNameFromVehicleModel(model));
                var brand = RAGE.Game.Ui.GetLabelText(RAGE.Game.Invoker.Invoke<string>(0xF7AF4F159FF99F97, (int)model));

                if (name == "NULL")
                    name = x;

                if (brand != "NULL")
                    name = $"{brand} {name}";

                data.Add("DisplayName", name);

                data.Add("MaxSpeed", RAGE.Game.Vehicle.GetVehicleModelMaxSpeed(model));
                data.Add("MaxBraking", RAGE.Game.Vehicle.GetVehicleModelMaxBraking(model));
                data.Add("MaxTraction", RAGE.Game.Vehicle.GetVehicleModelMaxTraction(model));
                data.Add("MaxAcceleration", RAGE.Game.Vehicle.GetVehicleModelAcceleration(model));

                data.Add("_0xBFBA3BA79CFF7EBF", RAGE.Game.Invoker.Invoke<float>(RAGE.Game.Natives._0xBFBA3BA79CFF7EBF, (int)model));
                data.Add("_0x53409B5163D5B846", RAGE.Game.Invoker.Invoke<float>(RAGE.Game.Natives._0x53409B5163D5B846, (int)model));
                data.Add("_0xC6AD107DDC9054CC", RAGE.Game.Invoker.Invoke<float>(RAGE.Game.Natives._0xC6AD107DDC9054CC, (int)model));
                data.Add("_0x5AA3F878A178C4FC", RAGE.Game.Invoker.Invoke<float>(RAGE.Game.Natives._0x5AA3F878A178C4FC, (int)model));

                var seats = RAGE.Game.Vehicle.GetVehicleModelNumberOfSeats(model);

                if (seats < 0)
                    seats = 0;

                data.Add("MaxNumberOfPassengers", seats == 0 ? 0 : seats - 1);
                data.Add("MaxOccupants", seats);
                data.Add("VehicleClass", RAGE.Game.Vehicle.GetVehicleClassFromName(model));

                Events.CallRemote("vehicle_data_p", model.ToString(), JsonConvert.SerializeObject(data));
            }

            Events.CallRemote("vehicle_data_f", newModels.Count);*/
        }
    }
}
