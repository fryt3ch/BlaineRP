using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.Containers;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.EntitiesData.Vehicles.Static
{
    public partial class Vehicle
    {
        public static ClassTypes GetClass(Vehicle veh)
        {
            if (veh.GovPrice <= 100_000)
                return ClassTypes.Classic;

            if (veh.GovPrice <= 500_000)
                return ClassTypes.Premium;

            if (veh.GovPrice <= 1_000_000)
                return ClassTypes.Luxe;

            return ClassTypes.Elite;
        }

        private static readonly HashSet<uint> CanTrailVehiclesModels = new HashSet<string>()
        {
            "bison", "bison2",
        }.Select(x => NAPI.Util.GetHashKey(x)).ToHashSet();

        private static readonly HashSet<uint> CanBeTrailedVehiclesModels = new HashSet<string>()
        {

        }.Select(x => NAPI.Util.GetHashKey(x)).ToHashSet();

        public string Id { get; }

        /// <summary>Модель транспорта</summary>
        public uint Model { get; }

        /// <summary>Название транспорта</summary>
        public string Name { get; }

        /// <summary>Объем бака</summary>
        public float Tank { get; }

        /// <summary>Тип топлива</summary>
        public FuelTypes FuelType { get; }

        /// <summary>Может ли тюнинговаться? (помимо цвета)</summary>
        public bool IsModdable { get; }

        /// <summary>Имеет ли круиз-контроль?</summary>
        public bool HasCruiseControl { get; }

        /// <summary>Имеет ли автопилот?</summary>
        public bool HasAutoPilot { get; }

        public Trunk TrunkData { get; }

        public VehicleTypes Type { get; }

        public uint GovPrice { get; set; }

        public bool CanTrail => CanTrailVehiclesModels.Contains(Model);
        
        public bool CanBeTrailed => CanBeTrailedVehiclesModels.Contains(Model);

        public ClassTypes Class => GetClass(this);

        /// <summary>Конструктор для создания нового транспорта (его прототипа)</summary>
        /// <param name="id">Уникальный ID</param>
        /// <param name="model">ID модели</param>
        /// <param name="name">Название</param>
        /// <param name="tank">Объем бака</param>
        /// <param name="fuelType">Тип топлива</param>
        /// <param name="trunkData">Информация о багажнике (null - если багажника в этом транспорте не будет)</param>
        /// <param name="isModdable">Можно ли устанавливать тюнинг на данный транспорт? (помимо цвета)</param>
        /// <param name="hasCruiseControl">Поддерживает ли круиз-контроль?</param>
        /// <param name="hasAutoPilot">Поддерживает ли автопилот?</param>
        public Vehicle(string id, string model, string name, float tank, FuelTypes fuelType, Trunk trunkData = null, bool isModdable = true, bool hasCruiseControl = false, bool hasAutoPilot = false, VehicleTypes type = VehicleTypes.Car)
        {
            Id = id;

            Model = NAPI.Util.GetHashKey(model);

            Name = name;
            Tank = tank;
            FuelType = fuelType;

            Type = type;

            TrunkData = trunkData;

            IsModdable = isModdable;

            HasCruiseControl = hasCruiseControl;
            HasAutoPilot = hasAutoPilot;

            if (trunkData != null)
                Container.AllSIDs.Add($"vt_{id}", new Container.Data(trunkData.Slots, trunkData.MaxWeight, Container.AllowedItemTypes.All, ContainerTypes.Trunk));

            Service.All.Add(id, this);
        }
    }
}