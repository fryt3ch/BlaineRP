using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;

namespace BCRPServer.Game.Data
{
    public class Vehicles
    {
        public static Dictionary<string, Vehicle> All; // ID - Data

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

            /// <summary>Модель транспорта</summary>
            public uint Model { get; set; }
            /// <summary>Название транспорта</summary>
            public string Name { get; set; }
            /// <summary>Объем бака</summary>
            public float Tank { get; set; }
            /// <summary>Тип топлива</summary>
            public FuelTypes FuelType { get; set; }
            /// <summary>Может ли тюнинговаться? (помимо цвета)</summary>
            public bool Moddable { get; set; }
            /// <summary>Имеет ли круиз-контроль?</summary>
            public bool HasCruiseControl { get; set; }
            /// <summary>Имеет ли автопилот?</summary>
            public bool HasAutoPilot { get; set; }
            
            /// <summary>Конструктор для создания нового транспорта (его прототипа)</summary>
            /// <param name="ID">Уникальный ID</param>
            /// <param name="Model">ID модели</param>
            /// <param name="Name">Название</param>
            /// <param name="Tank">Объем бака</param>
            /// <param name="FuelType">Тип топлива</param>
            /// <param name="TrunkData">Информация о багажнике (null - если багажника в этом транспорте не будет)</param>
            /// <param name="Moddable">Можно ли устанавливать тюнинг на данный транспорт? (помимо цвета)</param>
            /// <param name="HasCruiseControl">Поддерживает ли круиз-контроль?</param>
            /// <param name="HasAutoPilot">Поддерживает ли автопилот?</param>
            public Vehicle(string ID, string Model, string Name, float Tank, FuelTypes FuelType, Trunk TrunkData = null, bool Moddable = true, bool HasCruiseControl = false, bool HasAutoPilot = false)
            {
                this.Model = NAPI.Util.GetHashKey(Model);
                this.Name = Name;
                this.Tank = Tank;
                this.FuelType = FuelType;

                this.Moddable = Moddable;

                this.HasCruiseControl = HasCruiseControl;
                this.HasAutoPilot = HasAutoPilot;

                if (TrunkData != null)
                    Game.Items.Container.AllSIDs.Add(ID, new Items.Container.Data(TrunkData.Slots, TrunkData.MaxWeight, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Trunk));
            }

            /// <summary>Метод для создания транспорта на сервере</summary>
            /// <param name="ID">ID транспорта (см. Game.Data.Vehicles.All)</param>
            /// <param name="pos">Позиция</param>
            /// <param name="rot">Поворот</param>
            /// <param name="dimension">Измерение</param>
            /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
            /// <returns>Объект класса VehicleData, если транспорт был создан, null - в противном случае</returns>
            public static VehicleData Create(string ID, Vector3 pos, Vector3 rot, uint dimension)
            {
                var vehParams = All.GetValueOrDefault(ID);

                if (vehParams == null)
                    return null;

                var veh = NAPI.Vehicle.CreateVehicle(vehParams.Model, pos, 0, 0, 0, "", 255, true, false, Utils.Dimensions.Stuff);

                return new VehicleData(veh) { ID = ID };
            }
        }

        public class Tuning
        {
            /// <summary>Основной цвет</summary>
            public Color Color1 { get; set; }
            /// <summary>Второстепенный цвет</summary>
            public Color Color2 { get; set; }

            /// <summary>Словарь модификаций, где ключ - индекс модификации, а значение - тип модификации</summary>
            public Dictionary<int, int> Mods;

            /// <summary>Индексы всех доступных модификаций</summary>
            public static List<int> AllMods = new List<int>
            {
                0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 22, 23, 24, 25, 27, 28, 30, 33, 34, 35, 38, 40, 55, 48
            };

            public Tuning()
            {
                this.Mods = new Dictionary<int, int>();
            }

            /// <summary>Метод для получения текущего тюнинга на транспорте</summary>
            /// <param name="vehicle">Сущность транспорта</param>
            /// <param name="moddable">Учитывать ли модификации?</param>
            /// <returns>Объект класса Tuning, если транспорт существует, null - в противном случае</returns>
            /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
            public static Tuning GetTuning(GTANetworkAPI.Vehicle vehicle, bool moddable = true)
            {
                Tuning temp = new Tuning();

                temp.Color1 = vehicle.CustomPrimaryColor;
                temp.Color1 = vehicle.CustomSecondaryColor;

                if (moddable)
                    foreach (var x in AllMods)
                        temp.Mods.Add(x, vehicle.GetMod(x));

                return temp;
            }

            /// <summary>Метод для применения тюнинга к сущности транспорта</summary>
            /// <param name="vehicle">Сущность транспорта</param>
            /// <returns>Объект класса Tuning (себя же), если транспорт существует, null - в противном случае</returns>
            /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
            public Tuning Apply(GTANetworkAPI.Vehicle vehicle)
            {
                vehicle.CustomPrimaryColor = Color1;
                vehicle.CustomSecondaryColor = Color2;

                foreach (var x in Mods)
                    vehicle.SetMod(x.Key, x.Value);

                return this;
            }
        }

        /// <summary>Метод для получения объекта класса Vehile</summary>
        /// <param name="id">ID транспорта (см. Game.Data.Vehicles.All)</param>
        /// <returns>Объект класса Vehicle, если ID найден, null - в противном случае</returns>
        public static Vehicle GetData(string id) => All.GetValueOrDefault(id);

        public static int LoadAll()
        {
            if (All != null)
                return All.Count;

            All = new Dictionary<string, Vehicle>()
            {
                { "buffalo", new Vehicle("buffalo", "buffalo", "Bravado Buffalo", 80, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25)) },
            };

            return All.Count;
        }
    }
}
