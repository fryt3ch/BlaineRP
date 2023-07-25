using System.Collections.Generic;

namespace BlaineRP.Client.Game.Data.Vehicles
{
    public partial class Vehicle
    {
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

        public string TypeName => Locale.Property.VehicleTypesNames.GetValueOrDefault<Types, string>(Type) ?? "null";

        public Vehicle(string id, uint model, string name, float tank, FuelTypes fuelType, Trunk trunkData = null, bool isModdable = true, bool hasCruiseControl = false, bool hasAutoPilot = false, Types type = Types.Car, int govPrice = 0, ClassTypes @class = ClassTypes.Classic)
        {
            ID = id;
            Name = name;
            Type = type;

            Model = model;

            Tank = tank;

            FuelType = fuelType;

            TrunkData = trunkData;

            IsModdable = isModdable;
            HasAutoPilot = hasAutoPilot;
            HasCruiseControl = hasCruiseControl;

            GovPrice = govPrice;
            Class = @class;

            Core.All.Add(id, this);
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
}