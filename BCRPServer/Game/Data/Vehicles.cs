using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Bcpg.OpenPgp;

namespace BCRPServer.Game.Data
{
    public class Vehicles
    {
        public static Dictionary<string, Vehicle> All = new Dictionary<string, Vehicle>();

        public class Vehicle
        {
            /// <summary>Типы транспорта</summary>
            public enum Types
            {
                /// <summary>Автомобиль</summary>
                Car = 0,
                /// <summary>Мотоцикл</summary>
                Motorcycle,
                /// <summary>Лодка</summary>
                Boat,
                /// <summary>Самолет</summary>
                Plane,
                /// <summary>Вертолет</summary>
                Helicopter,
                /// <summary>Велосипед</summary>
                Cycle,
                /// <summary>Трейлер</summary>
                Trailer,
                /// <summary>Поезд</summary>
                Train,
                /// <summary>Джетпак</summary>
                Jetpack,
                /// <summary>Дерижабль</summary>
                Blimp,
            }

            /// <summary>Типы топлива</summary>
            public enum FuelTypes
            {
                /// <summary>Топливо не требуется</summary>
                None = -1,
                /// <summary>Бензин</summary>
                Petrol = 0,
                /// <summary>Электричество</summary>
                Electricity = 1,
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

            public string ID { get; set; }

            /// <summary>Модель транспорта</summary>
            public uint Model { get; set; }

            /// <summary>Название транспорта</summary>
            public string Name { get; set; }

            /// <summary>Объем бака</summary>
            public float Tank { get; set; }

            /// <summary>Тип топлива</summary>
            public FuelTypes FuelType { get; set; }

            /// <summary>Может ли тюнинговаться? (помимо цвета)</summary>
            public bool IsModdable { get; set; }

            /// <summary>Имеет ли круиз-контроль?</summary>
            public bool HasCruiseControl { get; set; }

            /// <summary>Имеет ли автопилот?</summary>
            public bool HasAutoPilot { get; set; }

            public Trunk TrunkData { get; set; }

            public Types Type { get; set; }

            public int GovPrice { get; set; }

            public ClassTypes Class => GetClass(this);
            
            /// <summary>Конструктор для создания нового транспорта (его прототипа)</summary>
            /// <param name="ID">Уникальный ID</param>
            /// <param name="Model">ID модели</param>
            /// <param name="Name">Название</param>
            /// <param name="Tank">Объем бака</param>
            /// <param name="FuelType">Тип топлива</param>
            /// <param name="TrunkData">Информация о багажнике (null - если багажника в этом транспорте не будет)</param>
            /// <param name="IsModdable">Можно ли устанавливать тюнинг на данный транспорт? (помимо цвета)</param>
            /// <param name="HasCruiseControl">Поддерживает ли круиз-контроль?</param>
            /// <param name="HasAutoPilot">Поддерживает ли автопилот?</param>
            public Vehicle(string ID, string Model, string Name, float Tank, FuelTypes FuelType, Trunk TrunkData = null, bool IsModdable = true, bool HasCruiseControl = false, bool HasAutoPilot = false, Types Type = Types.Car)
            {
                this.ID = ID;

                this.Model = NAPI.Util.GetHashKey(Model);

                this.Name = Name;
                this.Tank = Tank;
                this.FuelType = FuelType;

                this.Type = Type;

                this.TrunkData = TrunkData;

                this.IsModdable = IsModdable;

                this.HasCruiseControl = HasCruiseControl;
                this.HasAutoPilot = HasAutoPilot;

                if (TrunkData != null)
                    Game.Items.Container.AllSIDs.Add($"vt_{ID}", new Items.Container.Data(TrunkData.Slots, TrunkData.MaxWeight, Items.Container.AllowedItemTypes.All, Items.Container.ContainerTypes.Trunk));

                All.Add(ID, this);
            }

            /// <summary>Метод для создания транспорта на сервере</summary>
            /// <param name="ID">ID транспорта (см. Game.Data.Vehicles.All)</param>
            /// <param name="pos">Позиция</param>
            /// <param name="rot">Поворот</param>
            /// <param name="dimension">Измерение</param>
            /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
            /// <returns>Объект класса VehicleData, если транспорт был создан, null - в противном случае</returns>
            public static VehicleData Create(string ID, Vector3 pos, float heading, uint dimension)
            {
                var vehParams = All.GetValueOrDefault(ID);

                if (vehParams == null)
                    return null;

                var veh = NAPI.Vehicle.CreateVehicle(vehParams.Model, pos, heading, 0, 0, "", 255, true, false, Utils.Dimensions.Stuff);

                return new VehicleData(veh) { ID = ID };
            }
        }

        public class Tuning
        {
            private static Dictionary<byte, byte> DefaultMods => new Dictionary<byte, byte>()
            {
                { 0, 255 }, // Spoiler
                { 1, 255 }, // FrontBumper
                { 2, 255 }, // RearBumper
                { 3, 255 }, // SideSkirt
                { 4, 255 }, // Exhaust
                { 5, 255 }, // Frame
                { 6, 255 }, // Grille
                { 7, 255 }, // Hood
                { 8, 255 }, // Fender
                { 9, 255 }, // RightFender
                { 10, 255 }, // Roof
                { 11, 255 }, // Engine
                { 12, 255 }, // Brakes
                { 13, 255 }, // Transmission
                { 14, 255 }, // Horn
                { 15, 255 }, // Suspension
                { 23, 255 }, // FrontWheels
                { 24, 255 }, // BackWheels
                { 48, 255 }, // Livery
                { 32, 255 }, // Seats
                { 33, 255 }, // SteeringWheel
            };

            /// <summary>Основной цвет</summary>
            [JsonProperty(PropertyName = "C1")]
            public Utils.Colour Colour1 { get; set; }

            /// <summary>Второстепенный цвет</summary>
            [JsonProperty(PropertyName = "C2")]
            public Utils.Colour Colour2 { get; set; }

            [JsonProperty(PropertyName = "WC")]
            public byte WheelsColour { get; set; }

            [JsonProperty(PropertyName = "CT")]
            public byte ColourType { get; set; }

            [JsonProperty(PropertyName = "NC")]
            public Utils.Colour NeonColour { get; set; }

            [JsonProperty(PropertyName = "TSC")]
            public Utils.Colour TyresSmokeColour { get; set; }

            [JsonProperty(PropertyName = "PC")]
            public byte PearlescentColour { get; set; }

            [JsonProperty(PropertyName = "WT")]
            public byte WindowTint { get; set; }

            [JsonProperty(PropertyName = "X")]
            public sbyte Xenon { get; set; }

            [JsonProperty(PropertyName = "TT")]
            public bool Turbo { get; set; }

            [JsonProperty(PropertyName = "WHT")]
            public byte WheelsType { get; set; }

            /// <summary>Словарь модификаций, где ключ - индекс модификации, а значение - тип модификации</summary>
            public Dictionary<byte, byte> Mods { get; set; }

            public Tuning()
            {

            }

            public static Tuning CreateNew(Utils.Colour Colour1, Utils.Colour Colour2)
            {
                var res = new Tuning();

                res.Colour1 = Colour1;
                res.Colour2 = Colour2;

                res.NeonColour = null;

                res.TyresSmokeColour = null;

                res.Turbo = false;
                res.Xenon = -2;
                res.WindowTint = 0;
                res.PearlescentColour = 0;

                res.ColourType = 0;
                res.WheelsColour = 0;

                res.WheelsType = 0;

                res.Mods = DefaultMods;

                return res;
            }

            /// <summary>Метод для применения тюнинга к сущности транспорта</summary>
            /// <param name="vehicle">Сущность транспорта</param>
            /// <returns>Объект класса Tuning (себя же), если транспорт существует, null - в противном случае</returns>
            /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
            public void Apply(GTANetworkAPI.Vehicle vehicle)
            {
                UpdateColour(vehicle);

                if (NeonColour != null)
                {
                    vehicle.Neons = true;
                    vehicle.NeonColor = NeonColour.ToRageColour();

                    vehicle.SetSharedData("Mods::Neon", true);
                }
                else
                {
                    vehicle.Neons = false;

                    vehicle.ResetSharedData("Mods::Neon");
                }

                vehicle.WindowTint = WindowTint;

                vehicle.WheelType = WheelsType;

                if (TyresSmokeColour != null)
                {
                    vehicle.SetSharedData("Mods::TSColour", TyresSmokeColour);
                }
                else
                {
                    vehicle.ResetSharedData("Mods::TSColour");
                }

                if (Turbo)
                {
                    vehicle.SetSharedData("Mods::Turbo", true);
                }
                else
                {
                    vehicle.ResetSharedData("Mods::Turbo");
                }

                vehicle.SetSharedData("Mods::Xenon", Xenon);

                foreach (var x in Mods)
                {
                    vehicle.SetMod(x.Key, x.Value);
                }
            }

            public void UpdateColour(GTANetworkAPI.Vehicle vehicle)
            {
                vehicle.CustomPrimaryColor = Colour1.ToRageColour();
                vehicle.CustomSecondaryColor = Colour2.ToRageColour();

                vehicle.PearlescentColor = PearlescentColour;

                vehicle.WheelColor = WheelsColour;

                vehicle.SetSharedData("Mods::CT", ColourType);
            }

            public void UpdateWheels(GTANetworkAPI.Vehicle vehicle)
            {
                vehicle.WheelType = WheelsType;

                vehicle.SetMod(23, Mods[23]);
                vehicle.SetMod(24, Mods[24]);
            }
        }

        /// <summary>Метод для получения объекта класса Vehile</summary>
        /// <param name="id">ID транспорта (см. Game.Data.Vehicles.All)</param>
        /// <returns>Объект класса Vehicle, если ID найден, null - в противном случае</returns>
        public static Vehicle GetData(string id) => All.GetValueOrDefault(id);

        public static int LoadAll()
        {
            if (All.Count != 0)
                return All.Count;

            #region Cars
            new Vehicle("adder", "adder", "Truffade Adder", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3078201489
            new Vehicle("airbus", "airbus", "Автобус аэропорта", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1283517198
            new Vehicle("airtug", "airtug", "Airtug", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1560980623
            new Vehicle("alpha", "alpha", "Albany Alpha", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 767087018
            new Vehicle("ambulance", "ambulance", "Скорая помощь", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1171614426
            new Vehicle("apc", "apc", "HVY БТР", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 562680400
            new Vehicle("ardent", "ardent", "Ocelot Ardent", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 159274291
            new Vehicle("asbo", "asbo", "Maxwell Asbo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1118611807
            new Vehicle("asea", "asea", "Declasse Asea", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2485144969
            new Vehicle("asea2", "asea2", "Declasse Asea", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2487343317
            new Vehicle("asterope", "asterope", "Karin Asterope", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2391954683
            new Vehicle("autarch", "autarch", "Overflod Autarch", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3981782132
            new Vehicle("baller", "baller", "Gallivanter Baller", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3486135912
            new Vehicle("baller2", "baller2", "Gallivanter Baller", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 142944341
            new Vehicle("baller3", "baller3", "Gallivanter Baller LE", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1878062887
            new Vehicle("baller4", "baller4", "Gallivanter Baller LE LWB", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 634118882
            new Vehicle("baller5", "baller5", "Gallivanter Baller LE (броня)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 470404958
            new Vehicle("baller6", "baller6", "Gallivanter Baller LE LWB (броня)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 666166960
            new Vehicle("banshee", "banshee", "Bravado Banshee", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3253274834
            new Vehicle("banshee2", "banshee2", "Bravado Banshee 900R", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 633712403
            new Vehicle("barracks", "barracks", "Barracks", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3471458123
            new Vehicle("barracks2", "barracks2", "HVY Barracks Semi", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1074326203
            new Vehicle("barracks3", "barracks3", "Barracks", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 630371791
            new Vehicle("barrage", "barrage", "Barrage", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4081974053
            new Vehicle("benson", "benson", "Vapid Benson", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2053223216
            new Vehicle("bestiagts", "bestiagts", "Grotti Bestia GTS", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1274868363
            new Vehicle("bfinjection", "bfinjection", "BF Injection", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1126868326
            new Vehicle("biff", "biff", "HVY Biff", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 850991848
            new Vehicle("bifta", "bifta", "BF Bifta", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3945366167
            new Vehicle("bison", "bison", "Bravado Bison", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4278019151
            new Vehicle("bison2", "bison2", "Bravado Bison", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2072156101
            new Vehicle("bison3", "bison3", "Bravado Bison", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1739845664
            new Vehicle("bjxl", "bjxl", "Karin BeeJay XL", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 850565707
            new Vehicle("blade", "blade", "Vapid Blade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3089165662
            new Vehicle("blazer", "blazer", "Nagasaki Blazer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2166734073
            new Vehicle("blazer2", "blazer2", "Nagasaki Blazer Lifeguard", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4246935337
            new Vehicle("blazer3", "blazer3", "Nagasaki Hot Rod Blazer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3025077634
            new Vehicle("blazer4", "blazer4", "Nagasaki Street Blazer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3854198872
            new Vehicle("blazer5", "blazer5", "Nagasaki Blazer Aqua", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2704629607
            new Vehicle("blista", "blista", "Dinka Blista", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3950024287
            new Vehicle("blista2", "blista2", "Dinka Blista Compact", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1039032026
            new Vehicle("blista3", "blista3", "Dinka Go Go Monkey Blista", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3703315515
            new Vehicle("bobcatxl", "bobcatxl", "Vapid Bobcat XL", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1069929536
            new Vehicle("bodhi2", "bodhi2", "Canis Bodhi", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2859047862
            new Vehicle("boxville", "boxville", "Brute Boxville", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2307837162
            new Vehicle("boxville2", "boxville2", "Boxville", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4061868990
            new Vehicle("boxville3", "boxville3", "Brute Boxville", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 121658888
            new Vehicle("boxville4", "boxville4", "Brute Boxville", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 444171386
            new Vehicle("boxville5", "boxville5", "Boxville (броня)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 682434785
            new Vehicle("brawler", "brawler", "Coil Brawler", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2815302597
            new Vehicle("brickade", "brickade", "MTL Brickade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3989239879
            new Vehicle("brioso", "brioso", "Grotti BRIOSO R/A", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1549126457
            new Vehicle("brioso2", "brioso2", "Grotti Brioso 300", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1429622905
            new Vehicle("brioso3", "brioso3", "Grotti Brioso 300 широкий", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 15214558
            new Vehicle("bruiser", "bruiser", "Benefactor Bruiser (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 668439077
            new Vehicle("bruiser2", "bruiser2", "Benefactor Bruiser (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2600885406
            new Vehicle("bruiser3", "bruiser3", "Benefactor Bruiser (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2252616474
            new Vehicle("brutus", "brutus", "Declasse Brutus (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2139203625
            new Vehicle("brutus2", "brutus2", "Declasse Brutus (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2403970600
            new Vehicle("brutus3", "brutus3", "Declasse Brutus (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2038858402
            new Vehicle("btype", "btype", "Albany Roosevelt", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 117401876
            new Vehicle("btype2", "btype2", "Albany Fränken Stange", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3463132580
            new Vehicle("btype3", "btype3", "Albany Roosevelt Valor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3692679425
            new Vehicle("buccaneer", "buccaneer", "Albany Buccaneer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3612755468
            new Vehicle("buccaneer2", "buccaneer2", "Albany Buccaneer заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3281516360
            new Vehicle("buffalo", "buffalo", "Bravado Buffalo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3990165190
            new Vehicle("buffalo2", "buffalo2", "Bravado Buffalo S", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 736902334
            new Vehicle("buffalo3", "buffalo3", "Bravado Sprunk Buffalo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 237764926
            new Vehicle("bulldozer", "bulldozer", "HVY Dozer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1886712733
            new Vehicle("bullet", "bullet", "Vapid Bullet", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2598821281
            new Vehicle("burrito", "burrito", "Declasse Burrito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2948279460
            new Vehicle("burrito2", "burrito2", "Declasse Bugstars Burrito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3387490166
            new Vehicle("burrito3", "burrito3", "Declasse Burrito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2551651283
            new Vehicle("burrito4", "burrito4", "Declasse Burrito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 893081117
            new Vehicle("burrito5", "burrito5", "Declasse Burrito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1132262048
            new Vehicle("bus", "bus", "Автобус", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3581397346
            new Vehicle("caddy", "caddy", "Caddy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1147287684
            new Vehicle("caddy2", "caddy2", "Caddy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3757070668
            new Vehicle("caddy3", "caddy3", "Caddy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3525819835
            new Vehicle("calico", "calico", "Karin Calico GTF", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3101054893
            new Vehicle("camper", "camper", "Brute Camper", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1876516712
            new Vehicle("caracara", "caracara", "Vapid Caracara", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1254014755
            new Vehicle("caracara2", "caracara2", "Vapid Caracara 4x4", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2945871676
            new Vehicle("carbonizzare", "carbonizzare", "Grotti Carbonizzare", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2072687711
            new Vehicle("casco", "casco", "Lampadati Casco", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 941800958
            new Vehicle("cavalcade", "cavalcade", "Albany Cavalcade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2006918058
            new Vehicle("cavalcade2", "cavalcade2", "Albany Cavalcade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3505073125
            new Vehicle("cerberus", "cerberus", "MTL Cerberus (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3493417227
            new Vehicle("cerberus2", "cerberus2", "MTL Cerberus (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 679453769
            new Vehicle("cerberus3", "cerberus3", "MTL Cerberus (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1909700336
            new Vehicle("cheburek", "cheburek", "RUNE Cheburek", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3306466016
            new Vehicle("cheetah", "cheetah", "Grotti Cheetah", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2983812512
            new Vehicle("cheetah2", "cheetah2", "Grotti Cheetah Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 223240013
            new Vehicle("chernobog", "chernobog", "Chernobog", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3602674979
            new Vehicle("chino", "chino", "Vapid Chino", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 349605904
            new Vehicle("chino2", "chino2", "Vapid Chino заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2933279331
            new Vehicle("clique", "clique", "Vapid Clique", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2728360112
            new Vehicle("club", "club", "BF Club", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2196012677
            new Vehicle("coach", "coach", "Dashound", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2222034228
            new Vehicle("cog55", "cog55", "Enus Cognoscenti 55", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 906642318
            new Vehicle("cog552", "cog552", "Enus Cognoscenti 55 (броня)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 704435172
            new Vehicle("cogcabrio", "cogcabrio", "Enus Cognoscenti Cabrio", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 330661258
            new Vehicle("cognoscenti", "cognoscenti", "Enus Cognoscenti", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2264796000
            new Vehicle("cognoscenti2", "cognoscenti2", "Enus Cognoscenti (броня)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3690124666
            new Vehicle("comet2", "comet2", "Pfister Comet", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3249425686
            new Vehicle("comet3", "comet3", "Pfister Comet Retro заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2272483501
            new Vehicle("comet4", "comet4", "Pfister Comet Safari", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1561920505
            new Vehicle("comet5", "comet5", "Pfister Comet SR", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 661493923
            new Vehicle("comet6", "comet6", "Pfister Comet S2", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2568944644
            new Vehicle("contender", "contender", "Vapid Contender", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 683047626
            new Vehicle("coquette", "coquette", "Invetero Coquette", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 108773431
            new Vehicle("coquette2", "coquette2", "Invetero Coquette Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1011753235
            new Vehicle("coquette3", "coquette3", "Invetero Coquette BlackFin", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 784565758
            new Vehicle("coquette4", "coquette4", "Invetero Coquette D10", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2566281822
            new Vehicle("corsita", "corsita", "Lampadati Corsita", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3540279623
            new Vehicle("crusader", "crusader", "Canis Crusader", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 321739290
            new Vehicle("cutter", "cutter", "HVY Тоннелепроходчик", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3288047904
            new Vehicle("cyclone", "cyclone", "Coil Cyclone", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1392481335
            new Vehicle("cypher", "cypher", "Ubermacht Cypher", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1755697647
            new Vehicle("deluxo", "deluxo", "Imponte Deluxo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1483171323
            new Vehicle("deveste", "deveste", "Principe Deveste Eight", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1591739866
            new Vehicle("deviant", "deviant", "Schyster Deviant", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1279262537
            new Vehicle("dloader", "dloader", "Bravado Duneloader", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1770332643
            new Vehicle("docktug", "docktug", "Буксир", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3410276810
            new Vehicle("dominator", "dominator", "Vapid Dominator", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 80636076
            new Vehicle("dominator2", "dominator2", "Vapid Pisswasser Dominator", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3379262425
            new Vehicle("dominator3", "dominator3", "Vapid Dominator GTX", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3308022675
            new Vehicle("dominator4", "dominator4", "Vapid Dominator (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3606777648
            new Vehicle("dominator5", "dominator5", "Vapid Dominator (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2919906639
            new Vehicle("dominator6", "dominator6", "Vapid Dominator (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3001042683
            new Vehicle("dominator7", "dominator7", "Vapid Dominator ASP", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 426742808
            new Vehicle("dominator8", "dominator8", "Vapid Dominator GTT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 736672010
            new Vehicle("drafter", "drafter", "Obey 8F Drafter", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 686471183
            new Vehicle("draugur", "draugur", "Declasse Draugur", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3526730918
            new Vehicle("dubsta", "dubsta", "Benefactor Dubsta", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1177543287
            new Vehicle("dubsta2", "dubsta2", "Benefactor Dubsta", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3900892662
            new Vehicle("dubsta3", "dubsta3", "Benefactor Dubsta 6x6", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3057713523
            new Vehicle("dukes", "dukes", "Imponte Dukes", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 723973206
            new Vehicle("dukes2", "dukes2", "Imponte Duke O'Death", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3968823444
            new Vehicle("dukes3", "dukes3", "Imponte Beater Dukes", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2134119907
            new Vehicle("dump", "dump", "HVY Dump", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2164484578
            new Vehicle("dune", "dune", "BF Dune Buggy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2633113103
            new Vehicle("dune2", "dune2", "Space Docker", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 534258863
            new Vehicle("dune3", "dune3", "BF Dune FAV", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1897744184
            new Vehicle("dune4", "dune4", "Ramp Buggy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3467805257
            new Vehicle("dune5", "dune5", "Ramp Buggy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3982671785
            new Vehicle("dynasty", "dynasty", "Weeny Dynasty", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 310284501
            new Vehicle("elegy", "elegy", "Annis Elegy Retro заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 196747873
            new Vehicle("elegy2", "elegy2", "Annis Elegy RH8", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3728579874
            new Vehicle("ellie", "ellie", "Vapid Ellie", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3027423925
            new Vehicle("emerus", "emerus", "Progen Emerus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1323778901
            new Vehicle("emperor", "emperor", "Albany Emperor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3609690755
            new Vehicle("emperor2", "emperor2", "Albany Emperor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2411965148
            new Vehicle("emperor3", "emperor3", "Albany Emperor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3053254478
            new Vehicle("entity2", "entity2", "Overflod Entity XXR", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2174267100
            new Vehicle("entityxf", "entityxf", "Overflod Entity XF", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3003014393
            new Vehicle("euros", "euros", "Annis Euros", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2038480341
            new Vehicle("everon", "everon", "Karin Everon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2538945576
            new Vehicle("exemplar", "exemplar", "Dewbauchee Exemplar", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4289813342
            new Vehicle("f620", "f620", "Ocelot F620", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3703357000
            new Vehicle("faction", "faction", "Willard Faction", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2175389151
            new Vehicle("faction2", "faction2", "Willard Faction заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2504420315
            new Vehicle("faction3", "faction3", "Willard Faction Donk заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2255212070
            new Vehicle("fagaloa", "fagaloa", "Vulcar Fagaloa", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1617472902
            new Vehicle("fbi", "fbi", "ФРБ", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1127131465
            new Vehicle("fbi2", "fbi2", "ФРБ", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2647026068
            new Vehicle("felon", "felon", "Lampadati Felon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3903372712
            new Vehicle("felon2", "felon2", "Lampadati Felon GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4205676014
            new Vehicle("feltzer2", "feltzer2", "Benefactor Feltzer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2299640309
            new Vehicle("feltzer3", "feltzer3", "Benefactor Stirling GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2728226064
            new Vehicle("firetruk", "firetruk", "MTL Пожарная машина", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1938952078
            new Vehicle("flashgt", "flashgt", "Vapid Flash GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3035832600
            new Vehicle("flatbed", "flatbed", "MTL Flatbed", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1353720154
            new Vehicle("fmj", "fmj", "Vapid FMJ", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1426219628
            new Vehicle("forklift", "forklift", "HVY Forklift", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1491375716
            new Vehicle("formula", "formula", "Progen PR4", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 340154634
            new Vehicle("formula2", "formula2", "Ocelot R88", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2334210311
            new Vehicle("fq2", "fq2", "Fathom FQ 2", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3157435195
            new Vehicle("freecrawler", "freecrawler", "Canis Freecrawler", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4240635011
            new Vehicle("fugitive", "fugitive", "Cheval Fugitive", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1909141499
            new Vehicle("furia", "furia", "Grotti Furia", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 960812448
            new Vehicle("furoregt", "furoregt", "Lampadati Furore GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3205927392
            new Vehicle("fusilade", "fusilade", "Schyster Fusilade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 499169875
            new Vehicle("futo", "futo", "Karin Futo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2016857647
            new Vehicle("futo2", "futo2", "Karin Futo GTX", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2787736776
            new Vehicle("gauntlet", "gauntlet", "Bravado Gauntlet", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2494797253
            new Vehicle("gauntlet2", "gauntlet2", "Bravado Redwood Gauntlet", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 349315417
            new Vehicle("gauntlet3", "gauntlet3", "Bravado Gauntlet Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 722226637
            new Vehicle("gauntlet4", "gauntlet4", "Bravado Gauntlet Hellfire", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1934384720
            new Vehicle("gauntlet5", "gauntlet5", "Bravado Gauntlet Classic заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2172320429
            new Vehicle("gb200", "gb200", "Vapid GB200", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1909189272
            new Vehicle("gburrito", "gburrito", "Declasse Gang Burrito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2549763894
            new Vehicle("gburrito2", "gburrito2", "Declasse Gang Burrito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 296357396
            new Vehicle("glendale", "glendale", "Benefactor Glendale", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 75131841
            new Vehicle("glendale2", "glendale2", "Benefactor Glendale заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3381377750
            new Vehicle("gp1", "gp1", "Progen GP1", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1234311532
            new Vehicle("granger", "granger", "Declasse Granger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2519238556
            new Vehicle("greenwood", "greenwood", "Bravado Greenwood", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 40817712
            new Vehicle("gresley", "gresley", "Bravado Gresley", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2751205197
            new Vehicle("growler", "growler", "Pfister Growler", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1304459735
            new Vehicle("gt500", "gt500", "Grotti GT500", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2215179066
            new Vehicle("guardian", "guardian", "Vapid Guardian", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2186977100
            new Vehicle("habanero", "habanero", "Emperor Habanero", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 884422927
            new Vehicle("halftrack", "halftrack", "Bravado Half-track", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4262731174
            new Vehicle("handler", "handler", "Портовый погрузчик", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 444583674
            new Vehicle("hauler", "hauler", "JoBuilt Hauler", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1518533038
            new Vehicle("hauler2", "hauler2", "JoBuilt Hauler заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 387748548
            new Vehicle("hellion", "hellion", "Annis Hellion", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3932816511
            new Vehicle("hermes", "hermes", "Albany Hermes", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 15219735
            new Vehicle("hotknife", "hotknife", "Vapid Hotknife", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 37348240
            new Vehicle("hotring", "hotring", "Declasse Hotring Sabre", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1115909093
            new Vehicle("huntley", "huntley", "Enus Huntley S", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 486987393
            new Vehicle("hustler", "hustler", "Vapid Hustler", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 600450546
            new Vehicle("imorgon", "imorgon", "Overflod Imorgon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3162245632
            new Vehicle("impaler", "impaler", "Vapid Dominator (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3001042683
            new Vehicle("impaler2", "impaler2", "Declasse Impaler (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1009171724
            new Vehicle("impaler3", "impaler3", "Declasse Impaler (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2370166601
            new Vehicle("impaler4", "impaler4", "Declasse Impaler (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2550461639
            new Vehicle("imperator", "imperator", "Vapid Imperator (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 444994115
            new Vehicle("imperator2", "imperator2", "Vapid Imperator (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1637620610
            new Vehicle("imperator3", "imperator3", "Vapid Imperator (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3539435063
            new Vehicle("infernus", "infernus", "Pegassi Infernus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 418536135
            new Vehicle("infernus2", "infernus2", "Pegassi Infernus Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2889029532
            new Vehicle("ingot", "ingot", "Vulcar Ingot", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3005245074
            new Vehicle("insurgent", "insurgent", "HVY Insurgent (пикап)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2434067162
            new Vehicle("insurgent2", "insurgent2", "HVY Insurgent", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2071877360
            new Vehicle("insurgent3", "insurgent3", "HVY Insurgent пикап заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2370534026
            new Vehicle("intruder", "intruder", "Karin Intruder", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 886934177
            new Vehicle("issi2", "issi2", "Weeny Issi", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3117103977
            new Vehicle("issi3", "issi3", "Weeny Issi Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 931280609
            new Vehicle("issi4", "issi4", "Weeny Issi (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 628003514
            new Vehicle("issi5", "issi5", "Weeny Issi (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1537277726
            new Vehicle("issi6", "issi6", "Weeny Issi (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1239571361
            new Vehicle("issi7", "issi7", "Weeny Issi Sport", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1854776567
            new Vehicle("italigtb", "italigtb", "Progen Itali GTB", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2246633323
            new Vehicle("italigtb2", "italigtb2", "Progen Itali GTB заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3812247419
            new Vehicle("italigto", "italigto", "Grotti Itali GTO", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3963499524
            new Vehicle("italirsx", "italirsx", "Grotti Itali RSX", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3145241962
            new Vehicle("jackal", "jackal", "Ocelot Jackal", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3670438162
            new Vehicle("jb700", "jb700", "Dewbauchee JB 700", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1051415893
            new Vehicle("jb7002", "jb7002", "Dewbauchee JB 700W", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 394110044
            new Vehicle("jester", "jester", "Dinka Jester", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2997294755
            new Vehicle("jester2", "jester2", "Dinka Jester (гоночный)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3188613414
            new Vehicle("jester3", "jester3", "Dinka Jester Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4080061290
            new Vehicle("jester4", "jester4", "Dinka Jester RR", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2712905841
            new Vehicle("journey", "journey", "Zirconium Journey", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4174679674
            new Vehicle("jugular", "jugular", "Ocelot Jugular", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4086055493
            new Vehicle("kalahari", "kalahari", "Canis Kalahari", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 92612664
            new Vehicle("kamacho", "kamacho", "Canis Kamacho", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4173521127
            new Vehicle("kanjo", "kanjo", "Dinka Blista Kanjo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 409049982
            new Vehicle("kanjosj", "kanjosj", "Dinka Kanjo SJ", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4230891418
            new Vehicle("khamelion", "khamelion", "Hijak Khamelion", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 544021352
            new Vehicle("khanjali", "khanjali", "TM-02 Khanjali", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2859440138
            new Vehicle("komoda", "komoda", "Lampadati Komoda", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3460613305
            new Vehicle("krieger", "krieger", "Benefactor Krieger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3630826055
            new Vehicle("kuruma", "kuruma", "Karin Kuruma", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2922118804
            new Vehicle("kuruma2", "kuruma2", "Karin Kuruma (бронированный)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 410882957
            new Vehicle("landstalker", "landstalker", "Dundreary Landstalker", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1269098716
            new Vehicle("landstalker2", "landstalker2", "Dundreary Landstalker XL", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3456868130
            new Vehicle("le7b", "le7b", "Annis RE-7B", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3062131285
            new Vehicle("lguard", "lguard", "Declasse Спасатель", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 469291905
            new Vehicle("limo2", "limo2", "Benefactor Лимузин с турелью", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4180339789
            new Vehicle("lm87", "lm87", "Benefactor LM87", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4284049613
            new Vehicle("locust", "locust", "Ocelot Locust", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3353694737
            new Vehicle("lurcher", "lurcher", "Albany Lurcher", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2068293287
            new Vehicle("lynx", "lynx", "Ocelot Lynx", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 482197771
            new Vehicle("mamba", "mamba", "Declasse Mamba", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2634021974
            new Vehicle("manana", "manana", "Albany Manana", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2170765704
            new Vehicle("manana2", "manana2", "Albany Manana заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1717532765
            new Vehicle("marshall", "marshall", "Cheval Marshall", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1233534620
            new Vehicle("massacro", "massacro", "Dewbauchee Massacro", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4152024626
            new Vehicle("massacro2", "massacro2", "Dewbauchee Massacro (гоночный)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3663206819
            new Vehicle("menacer", "menacer", "HVY Menacer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2044532910
            new Vehicle("mesa", "mesa", "Canis Mesa", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 914654722
            new Vehicle("mesa2", "mesa2", "Canis Mesa", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3546958660
            new Vehicle("mesa3", "mesa3", "Canis Mesa", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2230595153
            new Vehicle("michelli", "michelli", "Lampadati Michelli GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1046206681
            new Vehicle("minitank", "minitank", "Танк Invade and Persuade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3040635986
            new Vehicle("minivan", "minivan", "Vapid Minivan", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3984502180
            new Vehicle("minivan2", "minivan2", "Vapid Minivan заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3168702960
            new Vehicle("mixer", "mixer", "HVY Бетономешалка", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3510150843
            new Vehicle("mixer2", "mixer2", "HVY Бетономешалка", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 475220373
            new Vehicle("monroe", "monroe", "Pegassi Monroe", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3861591579
            new Vehicle("monster", "monster", "Vapid The Liberator", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3449006043
            new Vehicle("monster3", "monster3", "Bravado Sasquatch (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1721676810
            new Vehicle("monster4", "monster4", "Bravado Sasquatch (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 840387324
            new Vehicle("monster5", "monster5", "Bravado Sasquatch (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3579220348
            new Vehicle("moonbeam", "moonbeam", "Declasse Moonbeam", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 525509695
            new Vehicle("moonbeam2", "moonbeam2", "Declasse Moonbeam заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1896491931
            new Vehicle("mower", "mower", "Газонокосилка", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1783355638
            new Vehicle("mule", "mule", "Maibatsu Mule", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 904750859
            new Vehicle("mule2", "mule2", "Maibatsu Mule", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3244501995
            new Vehicle("mule3", "mule3", "Maibatsu Mule", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2242229361
            new Vehicle("mule4", "mule4", "Maibatsu Mule заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1945374990
            new Vehicle("nebula", "nebula", "Vulcar Nebula Turbo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3412338231
            new Vehicle("neo", "neo", "Vysser Neo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2674840994
            new Vehicle("nero", "nero", "Truffade Nero", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1034187331
            new Vehicle("nero2", "nero2", "Truffade Nero заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1093792632
            new Vehicle("nightshade", "nightshade", "Imponte Nightshade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2351681756
            new Vehicle("nightshark", "nightshark", "HVY Nightshark", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 433954513
            new Vehicle("ninef", "ninef", "Obey 9F", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1032823388
            new Vehicle("ninef2", "ninef2", "Obey 9F Cabrio", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2833484545
            new Vehicle("novak", "novak", "Lampadati Novak", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2465530446
            new Vehicle("omnis", "omnis", "Obey Omnis", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3517794615
            new Vehicle("openwheel1", "openwheel1", "Benefactor BR8", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1492612435
            new Vehicle("openwheel2", "openwheel2", "Declasse DR1", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1181339704
            new Vehicle("oracle", "oracle", "Ubermacht Oracle XS", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1348744438
            new Vehicle("oracle2", "oracle2", "Ubermacht Oracle", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3783366066
            new Vehicle("osiris", "osiris", "Pegassi Osiris", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1987142870
            new Vehicle("outlaw", "outlaw", "Nagasaki Outlaw", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 408825843
            new Vehicle("packer", "packer", "MTL Packer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 569305213
            new Vehicle("panto", "panto", "Benefactor Panto", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3863274624
            new Vehicle("paradise", "paradise", "Bravado Paradise", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1488164764
            new Vehicle("paragon", "paragon", "Enus Paragon R", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3847255899
            new Vehicle("paragon2", "paragon2", "Enus Paragon R (броня)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1416466158
            new Vehicle("pariah", "pariah", "Ocelot Pariah", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 867799010
            new Vehicle("patriot", "patriot", "Mammoth Patriot", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3486509883
            new Vehicle("patriot2", "patriot2", "Mammoth Patriot Stretch", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3874056184
            new Vehicle("pbus", "pbus", "Автозак", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2287941233
            new Vehicle("pbus2", "pbus2", "Party Bus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 345756458
            new Vehicle("penetrator", "penetrator", "Ocelot Penetrator", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2536829930
            new Vehicle("penumbra", "penumbra", "Maibatsu Penumbra", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3917501776
            new Vehicle("penumbra2", "penumbra2", "Maibatsu Penumbra FF", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3663644634
            new Vehicle("peyote", "peyote", "Vapid Peyote", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1830407356
            new Vehicle("peyote2", "peyote2", "Vapid Peyote Gasser", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2490551588
            new Vehicle("peyote3", "peyote3", "Vapid Peyote заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1107404867
            new Vehicle("pfister811", "pfister811", "Pfister 811", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2465164804
            new Vehicle("phantom", "phantom", "JoBuilt Phantom", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2157618379
            new Vehicle("phantom2", "phantom2", "JoBuilt Phantom Wedge", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2645431192
            new Vehicle("phantom3", "phantom3", "JoBuilt Phantom заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 177270108
            new Vehicle("phoenix", "phoenix", "Imponte Phoenix", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2199527893
            new Vehicle("picador", "picador", "Cheval Picador", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1507916787
            new Vehicle("pigalle", "pigalle", "Lampadati Pigalle", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1078682497
            new Vehicle("police", "police", "Полицейский Cruiser", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2046537925
            new Vehicle("police2", "police2", "Полицейский Cruiser", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2667966721
            new Vehicle("police3", "police3", "Полицейский Cruiser", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1912215274
            new Vehicle("police4", "police4", "Cruiser без маркировки", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2321795001
            new Vehicle("policeold1", "policeold1", "Police Rancher", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2758042359
            new Vehicle("policeold2", "policeold2", "Полицейский Roadcruiser", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2515846680
            new Vehicle("policet", "policet", "Полицейский перевозчик", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 456714581
            new Vehicle("pony", "pony", "Brute Pony", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4175309224
            new Vehicle("pony2", "pony2", "Brute Pony", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 943752001
            new Vehicle("postlude", "postlude", "Dinka Postlude", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4000288633
            new Vehicle("pounder", "pounder", "MTL Pounder", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2112052861
            new Vehicle("pounder2", "pounder2", "MTL Pounder заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1653666139
            new Vehicle("prairie", "prairie", "Bollokan Prairie", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2844316578
            new Vehicle("pranger", "pranger", "Park Ranger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 741586030
            new Vehicle("premier", "premier", "Declasse Premier", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2411098011
            new Vehicle("previon", "previon", "Karin Previon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1416471345
            new Vehicle("primo", "primo", "Albany Primo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3144368207
            new Vehicle("primo2", "primo2", "Albany Primo заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2254540506
            new Vehicle("prototipo", "prototipo", "Grotti X80 Proto", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2123327359
            new Vehicle("radi", "radi", "Vapid Radius", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2643899483
            new Vehicle("rallytruck", "rallytruck", "MTL Dune", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2191146052
            new Vehicle("rancherxl", "rancherxl", "Declasse Rancher XL", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1645267888
            new Vehicle("rancherxl2", "rancherxl2", "Declasse Rancher XL", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1933662059
            new Vehicle("rapidgt", "rapidgt", "Dewbauchee Rapid GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2360515092
            new Vehicle("rapidgt2", "rapidgt2", "Dewbauchee Rapid GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1737773231
            new Vehicle("rapidgt3", "rapidgt3", "Dewbauchee Rapid GT Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2049897956
            new Vehicle("raptor", "raptor", "BF Raptor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3620039993
            new Vehicle("ratloader", "ratloader", "Rat-Loader", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3627815886
            new Vehicle("ratloader2", "ratloader2", "Bravado Rat-Truck", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3705788919
            new Vehicle("rcbandito", "rcbandito", "RC Bandito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4008920556
            new Vehicle("reaper", "reaper", "Pegassi Reaper", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 234062309
            new Vehicle("rebel", "rebel", "Karin Rusty Rebel", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3087195462
            new Vehicle("rebel2", "rebel2", "Karin Rebel", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2249373259
            new Vehicle("rebla", "rebla", "Ubermacht Rebla GTS", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 83136452
            new Vehicle("regina", "regina", "Dundreary Regina", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4280472072
            new Vehicle("remus", "remus", "Annis Remus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1377217886
            new Vehicle("rentalbus", "rentalbus", "Шаттл-автобус", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3196165219
            new Vehicle("retinue", "retinue", "Vapid Retinue", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1841130506
            new Vehicle("retinue2", "retinue2", "Vapid Retinue Mk II", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2031587082
            new Vehicle("revolter", "revolter", "Ubermacht Revolter", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3884762073
            new Vehicle("rhapsody", "rhapsody", "Declasse Rhapsody", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 841808271
            new Vehicle("rhinehart", "rhinehart", "Ubermacht Rhinehart", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2439462158
            new Vehicle("rhino", "rhino", "Танк Rhino", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 782665360
            new Vehicle("riata", "riata", "Vapid Riata", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2762269779
            new Vehicle("riot", "riot", "Полицейский Riot", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3089277354
            new Vehicle("riot2", "riot2", "RCV", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2601952180
            new Vehicle("ripley", "ripley", "Ripley", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3448987385
            new Vehicle("rocoto", "rocoto", "Obey Rocoto", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2136773105
            new Vehicle("romero", "romero", "Chariot Катафалк", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 627094268
            new Vehicle("rt3000", "rt3000", "Dinka RT3000", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3842363289
            new Vehicle("rubble", "rubble", "JoBuilt Rubble", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2589662668
            new Vehicle("ruiner", "ruiner", "Imponte Ruiner", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4067225593
            new Vehicle("ruiner2", "ruiner2", "Imponte Ruiner 2000", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 941494461
            new Vehicle("ruiner3", "ruiner3", "Imponte Ruiner", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 777714999
            new Vehicle("ruiner4", "ruiner4", "Imponte Ruiner ZZ-8", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1706945532
            new Vehicle("rumpo", "rumpo", "Bravado Rumpo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1162065741
            new Vehicle("rumpo2", "rumpo2", "Bravado Rumpo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2518351607
            new Vehicle("rumpo3", "rumpo3", "Bravado Rumpo заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1475773103
            new Vehicle("ruston", "ruston", "Hijak Ruston", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 719660200
            new Vehicle("s80", "s80", "Annis S80RR", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3970348707
            new Vehicle("sabregt", "sabregt", "Declasse Sabre Turbo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2609945748
            new Vehicle("sabregt2", "sabregt2", "Declasse Sabre Turbo заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 223258115
            new Vehicle("sadler", "sadler", "Vapid Sadler", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3695398481
            new Vehicle("sadler2", "sadler2", "Vapid Sadler", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 734217681
            new Vehicle("sandking", "sandking", "Vapid Sandking XL", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3105951696
            new Vehicle("sandking2", "sandking2", "Vapid Sandking SWB", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 989381445
            new Vehicle("savestra", "savestra", "Annis Savestra", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 903794909
            new Vehicle("sc1", "sc1", "Ubermacht SC1", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1352136073
            new Vehicle("scarab", "scarab", "HVY Scarab (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3147997943
            new Vehicle("scarab2", "scarab2", "HVY Scarab (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1542143200
            new Vehicle("scarab3", "scarab3", "HVY Scarab (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3715219435
            new Vehicle("schafter2", "schafter2", "Benefactor Schafter", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3039514899
            new Vehicle("schafter3", "schafter3", "Benefactor Schafter V12", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2809443750
            new Vehicle("schafter4", "schafter4", "Benefactor Schafter LWB", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1489967196
            new Vehicle("schafter5", "schafter5", "Benefactor Schafter V12 (броня)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3406724313
            new Vehicle("schafter6", "schafter6", "Benefactor Schafter LWB (броня)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1922255844
            new Vehicle("schlagen", "schlagen", "Benefactor Schlagen GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3787471536
            new Vehicle("schwarzer", "schwarzer", "Benefactor Schwartzer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3548084598
            new Vehicle("scramjet", "scramjet", "Declasse Scramjet", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3656405053
            new Vehicle("scrap", "scrap", "Старый грузовик", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2594165727
            new Vehicle("seminole", "seminole", "Canis Seminole", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1221512915
            new Vehicle("seminole2", "seminole2", "Canis Seminole Frontier", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2484160806
            new Vehicle("sentinel", "sentinel", "Ubermacht Sentinel XS", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1349725314
            new Vehicle("sentinel2", "sentinel2", "Ubermacht Sentinel", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 873639469
            new Vehicle("sentinel3", "sentinel3", "Ubermacht Sentinel Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1104234922
            new Vehicle("sentinel4", "sentinel4", "Ubermacht Sentinel Classic широкий", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2938086457
            new Vehicle("serrano", "serrano", "Benefactor Serrano", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1337041428
            new Vehicle("seven70", "seven70", "Dewbauchee Seven-70", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2537130571
            new Vehicle("sheava", "sheava", "Emperor ETR1", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 819197656
            new Vehicle("sheriff", "sheriff", "Sheriff Cruiser", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2611638396
            new Vehicle("sheriff2", "sheriff2", "Sheriff SUV", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1922257928
            new Vehicle("slamtruck", "slamtruck", "Vapid Slamtruck", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3249056020
            new Vehicle("slamvan", "slamvan", "Vapid Slamvan", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 729783779
            new Vehicle("slamvan2", "slamvan2", "Vapid Slamvan \'Пропащих\'", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 833469436
            new Vehicle("slamvan3", "slamvan3", "Vapid Slamvan заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1119641113
            new Vehicle("slamvan4", "slamvan4", "Vapid Slamvan (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2233918197
            new Vehicle("slamvan5", "slamvan5", "Vapid Slamvan (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 373261600
            new Vehicle("slamvan6", "slamvan6", "Vapid Slamvan (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1742022738
            new Vehicle("sm722", "sm722", "Benefactor SM722", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 775514032
            new Vehicle("specter", "specter", "Dewbauchee Specter", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1886268224
            new Vehicle("specter2", "specter2", "Dewbauchee Specter заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1074745671
            new Vehicle("speedo", "speedo", "Vapid Speedo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3484649228
            new Vehicle("speedo2", "speedo2", "Vapid Фургон клоуна", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 728614474
            new Vehicle("speedo4", "speedo4", "Vapid Speedo заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 219613597
            new Vehicle("squaddie", "squaddie", "Mammoth Squaddie", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4192631813
            new Vehicle("stafford", "stafford", "Enus Stafford", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 321186144
            new Vehicle("stalion", "stalion", "Declasse Stallion", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1923400478
            new Vehicle("stalion2", "stalion2", "Declasse Burger Shot Stallion", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3893323758
            new Vehicle("stanier", "stanier", "Vapid Stanier", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2817386317
            new Vehicle("stinger", "stinger", "Grotti Stinger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1545842587
            new Vehicle("stingergt", "stingergt", "Grotti Stinger GT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2196019706
            new Vehicle("stockade", "stockade", "Brute Stockade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1747439474
            new Vehicle("stockade3", "stockade3", "Brute Stockade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4080511798
            new Vehicle("stratum", "stratum", "Zirconium Stratum", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1723137093
            new Vehicle("streiter", "streiter", "Benefactor Streiter", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1741861769
            new Vehicle("stretch", "stretch", "Dundreary Stretch", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2333339779
            new Vehicle("stromberg", "stromberg", "Ocelot Stromberg", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 886810209
            new Vehicle("sugoi", "sugoi", "Dinka Sugoi", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 987469656
            new Vehicle("sultan", "sultan", "Karin Sultan", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 970598228
            new Vehicle("sultan2", "sultan2", "Karin Sultan Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 872704284
            new Vehicle("sultan3", "sultan3", "Karin Sultan RS Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4003946083
            new Vehicle("sultanrs", "sultanrs", "Karin Sultan RS", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3999278268
            new Vehicle("superd", "superd", "Enus Super Diamond", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1123216662
            new Vehicle("surano", "surano", "Benefactor Surano", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 384071873
            new Vehicle("surfer", "surfer", "BF Surfer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 699456151
            new Vehicle("surfer2", "surfer2", "BF Surfer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2983726598
            new Vehicle("swinger", "swinger", "Ocelot Swinger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 500482303
            new Vehicle("t20", "t20", "Progen T20", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1663218586
            new Vehicle("taco", "taco", "Фургон Тако", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1951180813
            new Vehicle("tailgater", "tailgater", "Obey Tailgater", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3286105550
            new Vehicle("tailgater2", "tailgater2", "Obey Tailgater S", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3050505892
            new Vehicle("taipan", "taipan", "Cheval Taipan", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3160260734
            new Vehicle("tampa", "tampa", "Declasse Tampa", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 972671128
            new Vehicle("tampa2", "tampa2", "Declasse Drift Tampa", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3223586949
            new Vehicle("tampa3", "tampa3", "Declasse Tampa с оружием", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3084515313
            new Vehicle("taxi", "taxi", "Такси", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3338918751
            new Vehicle("technical", "technical", "Karin Technical", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2198148358
            new Vehicle("technical2", "technical2", "Karin Technical Aqua", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1180875963
            new Vehicle("technical3", "technical3", "Karin Technical заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1356124575
            new Vehicle("tempesta", "tempesta", "Pegassi Tempesta", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 272929391
            new Vehicle("tenf", "tenf", "Obey 10F", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3400983137
            new Vehicle("tenf2", "tenf2", "Obey 10F широкий", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 274946574
            new Vehicle("terbyte", "terbyte", "Benefactor Terrorbyte", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2306538597
            new Vehicle("tezeract", "tezeract", "Pegassi Tezeract", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1031562256
            new Vehicle("thrax", "thrax", "Truffade Thrax", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1044193113
            new Vehicle("tigon", "tigon", "Lampadati Tigon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2936769864
            new Vehicle("tiptruck", "tiptruck", "Brute Tipper", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 48339065
            new Vehicle("tiptruck2", "tiptruck2", "Tipper", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3347205726
            new Vehicle("toreador", "toreador", "Pegassi Toreador", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1455990255
            new Vehicle("torero", "torero", "Pegassi Torero", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1504306544
            new Vehicle("torero2", "torero2", "Pegassi Torero XO", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4129572538
            new Vehicle("tornado", "tornado", "Declasse Tornado", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 464687292
            new Vehicle("tornado2", "tornado2", "Declasse Tornado", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1531094468
            new Vehicle("tornado3", "tornado3", "Declasse Tornado", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1762279763
            new Vehicle("tornado4", "tornado4", "Declasse Tornado", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2261744861
            new Vehicle("tornado5", "tornado5", "Declasse Tornado заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2497353967
            new Vehicle("tornado6", "tornado6", "Declasse Tornado Rat Rod", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2736567667
            new Vehicle("toros", "toros", "Pegassi Toros", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3126015148
            new Vehicle("tourbus", "tourbus", "Экскурсионный автобус", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1941029835
            new Vehicle("towtruck", "towtruck", "Эвакуатор", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2971866336
            new Vehicle("towtruck2", "towtruck2", "Эвакуатор", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3852654278
            new Vehicle("tractor", "tractor", "Трактор", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1641462412
            new Vehicle("tractor2", "tractor2", "Stanley Fieldmaster", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2218488798
            new Vehicle("tractor3", "tractor3", "Stanley Fieldmaster", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1445631933
            new Vehicle("trash", "trash", "Trashmaster", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1917016601
            new Vehicle("trash2", "trash2", "Trashmaster", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3039269212
            new Vehicle("trophytruck", "trophytruck", "Vapid Пикап трофи", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 101905590
            new Vehicle("trophytruck2", "trophytruck2", "Vapid Desert Raid", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3631668194
            new Vehicle("tropos", "tropos", "Lampadati Tropos Rallye", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1887331236
            new Vehicle("tulip", "tulip", "Declasse Tulip", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1456744817
            new Vehicle("turismo2", "turismo2", "Grotti Turismo Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3312836369
            new Vehicle("turismor", "turismor", "Grotti Turismo R", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 408192225
            new Vehicle("tyrant", "tyrant", "Overflod Tyrant", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3918533058
            new Vehicle("tyrus", "tyrus", "Progen Tyrus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2067820283
            new Vehicle("utillitruck", "utillitruck", "Машина техобслуживания", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 516990260
            new Vehicle("utillitruck2", "utillitruck2", "Машина техобслуживания", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 887537515
            new Vehicle("utillitruck3", "utillitruck3", "Машина техобслуживания", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2132890591
            new Vehicle("vacca", "vacca", "Pegassi Vacca", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 338562499
            new Vehicle("vagner", "vagner", "Dewbauchee Vagner", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1939284556
            new Vehicle("vagrant", "vagrant", "Maxwell Vagrant", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 740289177
            new Vehicle("vamos", "vamos", "Declasse Vamos", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4245851645
            new Vehicle("vectre", "vectre", "Emperor Vectre", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2754593701
            new Vehicle("verlierer2", "verlierer2", "Bravado Verlierer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1102544804
            new Vehicle("verus", "verus", "Dinka Verus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 298565713
            new Vehicle("vetir", "vetir", "Vetir", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2014313426
            new Vehicle("veto", "veto", "Dinka Veto Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3437611258
            new Vehicle("veto2", "veto2", "Dinka Veto Modern", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2802050217
            new Vehicle("vigero", "vigero", "Declasse Vigero", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3469130167
            new Vehicle("vigero2", "vigero2", "Declasse Vigero ZX", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2536587772
            new Vehicle("vigilante", "vigilante", "Vigilante", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3052358707
            new Vehicle("virgo", "virgo", "Albany Virgo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3796912450
            new Vehicle("virgo2", "virgo2", "Dundreary Virgo Classic заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3395457658
            new Vehicle("virgo3", "virgo3", "Dundreary Virgo Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 16646064
            new Vehicle("viseris", "viseris", "Lampadati Viseris", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3903371924
            new Vehicle("visione", "visione", "Grotti Visione", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3296789504
            new Vehicle("voodoo", "voodoo", "Declasse Voodoo заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2006667053
            new Vehicle("voodoo2", "voodoo2", "Declasse Voodoo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 523724515
            new Vehicle("vstr", "vstr", "Albany V-STR", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1456336509
            new Vehicle("warrener", "warrener", "Vulcar Warrener", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1373123368
            new Vehicle("washington", "washington", "Albany Washington", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1777363799
            new Vehicle("wastelander", "wastelander", "MTL Wastelander", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2382949506
            new Vehicle("weevil", "weevil", "BF Weevil", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1644055914
            new Vehicle("weevil2", "weevil2", "BF Weevil заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3300595976
            new Vehicle("windsor", "windsor", "Enus Windsor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1581459400
            new Vehicle("windsor2", "windsor2", "Enus Windsor кабриолет", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2364918497
            new Vehicle("winky", "winky", "Vapid Winky", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4084658662
            new Vehicle("xa21", "xa21", "Ocelot XA-21", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 917809321
            new Vehicle("xls", "xls", "Benefactor XLS", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1203490606
            new Vehicle("xls2", "xls2", "Benefactor XLS (бронированный)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3862958888
            new Vehicle("yosemite", "yosemite", "Declasse Yosemite", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1871995513
            new Vehicle("yosemite2", "yosemite2", "Declasse Drift Yosemite", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1693751655
            new Vehicle("yosemite3", "yosemite3", "Declasse Yosemite Rancher", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 67753863
            new Vehicle("youga", "youga", "Bravado Youga", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 65402552
            new Vehicle("youga2", "youga2", "Bravado Youga Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1026149675
            new Vehicle("youga3", "youga3", "Bravado Youga Classic 4x4", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1802742206
            new Vehicle("z190", "z190", "Karin 190z", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 838982985
            new Vehicle("zentorno", "zentorno", "Pegassi Zentorno", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2891838741
            new Vehicle("zhaba", "zhaba", "RUNE Zhaba", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1284356689
            new Vehicle("zion", "zion", "Ubermacht Zion", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3172678083
            new Vehicle("zion2", "zion2", "Ubermacht Zion Cabrio", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3101863448
            new Vehicle("zion3", "zion3", "Ubermacht Zion Classic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1862507111
            new Vehicle("zorrusso", "zorrusso", "Pegassi Zorrusso", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3612858749
            new Vehicle("zr350", "zr350", "Annis ZR350", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2436313176
            new Vehicle("zr380", "zr380", "Annis ZR380 (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 540101442
            new Vehicle("zr3802", "zr3802", "Annis ZR380 (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3188846534
            new Vehicle("zr3803", "zr3803", "Annis ZR380 (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2816263004
            new Vehicle("ztype", "ztype", "Truffade Z-Type", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 758895617

            new Vehicle("dilettante", "dilettante", "Karin Dilettante", 80f, Vehicle.FuelTypes.Electricity, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3164157193
            new Vehicle("dilettante2", "dilettante2", "Karin Dilettante", 80f, Vehicle.FuelTypes.Electricity, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1682114128
            new Vehicle("neon", "neon", "Pfister Neon", 80f, Vehicle.FuelTypes.Electricity, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2445973230
            new Vehicle("omnisegt", "omnisegt", "Obey Omnis e-GT", 80f, Vehicle.FuelTypes.Electricity, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3789743831
            new Vehicle("raiden", "raiden", "Coil Raiden", 80f, Vehicle.FuelTypes.Electricity, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2765724541
            new Vehicle("surge", "surge", "Cheval Surge", 80f, Vehicle.FuelTypes.Electricity, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2400073108
            new Vehicle("voltic", "voltic", "Coil Voltic", 80f, Vehicle.FuelTypes.Electricity, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2672523198
            new Vehicle("voltic2", "voltic2", "Coil Rocket Voltic", 80f, Vehicle.FuelTypes.Electricity, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 989294410

            new Vehicle("boor", "boor", "Karin Boor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 996383885
            new Vehicle("brickade2", "brickade2", "MTL Brickade 6x6", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2718380883
            new Vehicle("broadway", "broadway", "Classique Broadway", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2361724968
            new Vehicle("entity3", "entity3", "Overflod Entity MT", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1748565021
            new Vehicle("eudora", "eudora", "Willard Eudora", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3045179290
            new Vehicle("everon2", "everon2", "Karin Hotring Everon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 4163619118
            new Vehicle("issi8", "issi8", "Weeny Issi Rally", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1550581940
            new Vehicle("journey2", "journey2", "Zirconium Journey II", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2667889793
            new Vehicle("panthere", "panthere", "Toundra Panthere", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 2100457220
            new Vehicle("r300", "r300", "Annis 300R", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 1076201208
            new Vehicle("surfer3", "surfer3", "BF Surfer заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3259477733
            new Vehicle("tahoma", "tahoma", "Declasse Tahoma Coupe", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 3833117047
            new Vehicle("tulip2", "tulip2", "Declasse Tulip M-100", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 268758436
            new Vehicle("virtue", "virtue", "Ocelot Virtue", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Car); // 669204833
            #endregion

            #region Motorcycles
            new Vehicle("akuma", "akuma", "Dinka Akuma", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1672195559
            new Vehicle("avarus", "avarus", "LCC Avarus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2179174271
            new Vehicle("bagger", "bagger", "Western Bagger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2154536131
            new Vehicle("bati", "bati", "Pegassi Bati 801", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 4180675781
            new Vehicle("bati2", "bati2", "Pegassi Bati 801RR", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3403504941
            new Vehicle("bf400", "bf400", "Nagasaki BF400", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 86520421
            new Vehicle("carbonrs", "carbonrs", "Nagasaki Carbon RS", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 11251904
            new Vehicle("chimera", "chimera", "Nagasaki Chimera", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 6774487
            new Vehicle("cliffhanger", "cliffhanger", "Western Cliffhanger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 390201602
            new Vehicle("daemon", "daemon", "Western Daemon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2006142190
            new Vehicle("daemon2", "daemon2", "Western Daemon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2890830793
            new Vehicle("deathbike", "deathbike", "Western Deathbike (апокалипсис)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 4267640610
            new Vehicle("deathbike2", "deathbike2", "Western Deathbike (фантастика)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2482017624
            new Vehicle("deathbike3", "deathbike3", "Western Deathbike (кошмар)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2920466844
            new Vehicle("defiler", "defiler", "Shitzu Defiler", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 822018448
            new Vehicle("diablous", "diablous", "Principe Diabolus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 4055125828
            new Vehicle("diablous2", "diablous2", "Principe Diabolus заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1790834270
            new Vehicle("double", "double", "Dinka Double-T", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2623969160
            new Vehicle("enduro", "enduro", "Dinka Enduro", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1753414259
            new Vehicle("esskey", "esskey", "Pegassi Esskey", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2035069708
            new Vehicle("faggio", "faggio", "Pegassi Faggio гоночный", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2452219115
            new Vehicle("faggio2", "faggio2", "Pegassi Faggio", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 55628203
            new Vehicle("faggio3", "faggio3", "Pegassi Faggio заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3005788552
            new Vehicle("fcr", "fcr", "Pegassi FCR 1000", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 627535535
            new Vehicle("fcr2", "fcr2", "Pegassi FCR 1000 заказной", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3537231886
            new Vehicle("gargoyle", "gargoyle", "Western Gargoyle", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 741090084
            new Vehicle("hakuchou", "hakuchou", "Shitzu Hakuchou", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1265391242
            new Vehicle("hakuchou2", "hakuchou2", "Shitzu Hakuchou Drag", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 4039289119
            new Vehicle("hexer", "hexer", "LCC Hexer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 301427732
            new Vehicle("innovation", "innovation", "LCC Innovation", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 4135840458
            new Vehicle("lectro", "lectro", "Principe Lectro", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 640818791
            new Vehicle("manchez", "manchez", "Maibatsu Manchez", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2771538552
            new Vehicle("manchez2", "manchez2", "Maibatsu Manchez Scout", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1086534307
            new Vehicle("nemesis", "nemesis", "Principe Nemesis", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3660088182
            new Vehicle("nightblade", "nightblade", "Western Nightblade", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2688780135
            new Vehicle("oppressor", "oppressor", "Pegassi Oppressor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 884483972
            new Vehicle("oppressor2", "oppressor2", "Pegassi Oppressor Mk II", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2069146067
            new Vehicle("pcj", "pcj", "Shitzu PCJ 600", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3385765638
            new Vehicle("ratbike", "ratbike", "Western Rat Bike", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1873600305
            new Vehicle("ruffian", "ruffian", "Pegassi Ruffian", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3401388520
            new Vehicle("rrocket", "rrocket", "Western Rampant Rocket", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 916547552
            new Vehicle("sanchez", "sanchez", "Maibatsu Sanchez (раскраска)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 788045382
            new Vehicle("sanchez2", "sanchez2", "Maibatsu Sanchez", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2841686334
            new Vehicle("sanctus", "sanctus", "LCC Sanctus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1491277511
            new Vehicle("shotaro", "shotaro", "Nagasaki Shotaro", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3889340782
            new Vehicle("sovereign", "sovereign", "Western Sovereign", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 743478836
            new Vehicle("stryder", "stryder", "Nagasaki Stryder", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 301304410
            new Vehicle("thrust", "thrust", "Dinka Thrust", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1836027715
            new Vehicle("vader", "vader", "Shitzu Vader", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 4154065143
            new Vehicle("vindicator", "vindicator", "Dinka Vindicator", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2941886209
            new Vehicle("vortex", "vortex", "Pegassi Vortex", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3685342204
            new Vehicle("wolfsbane", "wolfsbane", "Western Wolfsbane", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3676349299
            new Vehicle("zombiea", "zombiea", "Western Zombie Bobber", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3285698347
            new Vehicle("zombieb", "zombieb", "Western Zombie Chopper", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 3724934023

            new Vehicle("policeb", "policeb", "Полицейский мотоцикл", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 4260343491

            new Vehicle("manchez3", "manchez3", "Maibatsu Manchez Scout C", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 1384502824
            new Vehicle("powersurge", "powersurge", "Western Powersurge", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Motorcycle); // 2908631255
            #endregion

            #region Helicopters
            new Vehicle("akula", "akula", "Akula", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1181327175
            new Vehicle("annihilator", "annihilator", "Annihilator", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 837858166
            new Vehicle("annihilator2", "annihilator2", "Annihilator (стелс)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 295054921
            new Vehicle("buzzard", "buzzard", "Штурмовой Buzzard", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 788747387
            new Vehicle("buzzard2", "buzzard2", "Buzzard", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 745926877
            new Vehicle("cargobob", "cargobob", "Cargobob", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 4244420235
            new Vehicle("cargobob2", "cargobob2", "Cargobob", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1621617168
            new Vehicle("cargobob3", "cargobob3", "Cargobob", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1394036463
            new Vehicle("cargobob4", "cargobob4", "Cargobob", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 2025593404
            new Vehicle("conada", "conada", "Buckingham Conada", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 3817135397
            new Vehicle("frogger", "frogger", "Frogger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 744705981
            new Vehicle("frogger2", "frogger2", "Frogger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1949211328
            new Vehicle("havok", "havok", "Nagasaki Havok", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 2310691317
            new Vehicle("hunter", "hunter", "FH-1 Hunter", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 4252008158
            new Vehicle("maverick", "maverick", "Maverick", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 2634305738
            new Vehicle("savage", "savage", "Savage", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 4212341271
            new Vehicle("seasparrow", "seasparrow", "Sea Sparrow", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 3568198617
            new Vehicle("seasparrow2", "seasparrow2", "Sparrow", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1229411063
            new Vehicle("seasparrow3", "seasparrow3", "Sparrow", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1593933419
            new Vehicle("skylift", "skylift", "Skylift", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1044954915
            new Vehicle("supervolito", "supervolito", "Buckingham SuperVolito", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 710198397
            new Vehicle("supervolito2", "supervolito2", "Buckingham SuperVolito Carbon", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 2623428164
            new Vehicle("swift", "swift", "Buckingham Swift", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 3955379698
            new Vehicle("swift2", "swift2", "Buckingham Swift Deluxe", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1075432268
            new Vehicle("valkyrie", "valkyrie", "Valkyrie", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 2694714877
            new Vehicle("valkyrie2", "valkyrie2", "Valkyrie MOD.0", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 1543134283
            new Vehicle("volatus", "volatus", "Buckingham Volatus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 2449479409

            new Vehicle("polmav", "polmav", "Полицейский Maverick", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Helicopter); // 353883353
            #endregion

            #region Planes
            new Vehicle("alphaz1", "alphaz1", "Buckingham Alpha-Z1", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2771347558
            new Vehicle("avenger", "avenger", "Mammoth Avenger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2176659152
            new Vehicle("avenger2", "avenger2", "Mammoth Avenger", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 408970549
            new Vehicle("besra", "besra", "Western Besra", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1824333165
            new Vehicle("bombushka", "bombushka", "RM-10 Bombushka", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 4262088844
            new Vehicle("cargoplane", "cargoplane", "Грузовой самолет", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 368211810
            new Vehicle("cuban800", "cuban800", "Cuban 800", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3650256867
            new Vehicle("dodo", "dodo", "Mammoth Dodo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3393804037
            new Vehicle("duster", "duster", "Duster", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 970356638
            new Vehicle("howard", "howard", "Buckingham Howard NX-25", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3287439187
            new Vehicle("hydra", "hydra", "Mammoth Hydra", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 970385471
            new Vehicle("jet", "jet", "Пассажирский самолет", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1058115860
            new Vehicle("lazer", "lazer", "P-996 LAZER", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3013282534
            new Vehicle("luxor", "luxor", "Buckingham Luxor", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 621481054
            new Vehicle("luxor2", "luxor2", "Buckingham Luxor Deluxe", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3080673438
            new Vehicle("mammatus", "mammatus", "Mammatus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2548391185
            new Vehicle("microlight", "microlight", "Nagasaki Ultralight", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2531412055
            new Vehicle("miljet", "miljet", "Buckingham Miljet", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 165154707
            new Vehicle("mogul", "mogul", "Mammoth Mogul", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3545667823
            new Vehicle("molotok", "molotok", "V-65 Molotok", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1565978651
            new Vehicle("nimbus", "nimbus", "Buckingham Nimbus", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2999939664
            new Vehicle("nokota", "nokota", "P-45 Nokota", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1036591958
            new Vehicle("pyro", "pyro", "Buckingham Pyro", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2908775872
            new Vehicle("rogue", "rogue", "Western Rogue", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3319621991
            new Vehicle("seabreeze", "seabreeze", "Western Seabreeze", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3902291871
            new Vehicle("shamal", "shamal", "Buckingham Shamal", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3080461301
            new Vehicle("starling", "starling", "LF-22 Starling", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2594093022
            new Vehicle("strikeforce", "strikeforce", "B-11 Strikeforce", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1692272545
            new Vehicle("stunt", "stunt", "Mallard", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2172210288
            new Vehicle("titan", "titan", "Titan", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1981688531
            new Vehicle("tula", "tula", "Mammoth Tula", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1043222410
            new Vehicle("velum", "velum", "Velum", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2621610858
            new Vehicle("velum2", "velum2", "Velum (пятиместный)", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1077420264
            new Vehicle("vestra", "vestra", "Buckingham Vestra", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 1341619767
            new Vehicle("volatol", "volatol", "Volatol", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 447548909
            new Vehicle("alkonost", "alkonost", "RO-86 Alkonost", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 3929093893

            new Vehicle("cargoplane2", "cargoplane2", "Грузовой самолет", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Plane); // 2336777441
            #endregion

            #region Boats
            new Vehicle("avisa", "avisa", "Kraken Avisa", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 2588363614
            new Vehicle("dinghy", "dinghy", "Nagasaki Dinghy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 1033245328
            new Vehicle("dinghy2", "dinghy2", "Nagasaki Dinghy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 276773164
            new Vehicle("dinghy3", "dinghy3", "Nagasaki Dinghy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 509498602
            new Vehicle("dinghy4", "dinghy4", "Nagasaki Dinghy", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 867467158
            new Vehicle("dinghy5", "dinghy5", "Nagasaki Лодка с вооружением", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 3314393930
            new Vehicle("jetmax", "jetmax", "Shitzu Jetmax", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 861409633
            new Vehicle("kosatka", "kosatka", "RUNE Kosatka", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 1336872304
            new Vehicle("longfin", "longfin", "Shitzu Longfin", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 1861786828
            new Vehicle("marquis", "marquis", "Dinka Marquis", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 3251507587
            new Vehicle("patrolboat", "patrolboat", "Патрульный катер Kurtz 31", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 4018222598
            new Vehicle("predator", "predator", "Полицейский Predator", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 3806844075
            new Vehicle("seashark", "seashark", "Speedophile Seashark", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 3264692260
            new Vehicle("seashark2", "seashark2", "Speedophile Seashark", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 3678636260
            new Vehicle("seashark3", "seashark3", "Speedophile Seashark", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 3983945033
            new Vehicle("speeder", "speeder", "Pegassi Speeder", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 231083307
            new Vehicle("speeder2", "speeder2", "Pegassi Speeder", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 437538602
            new Vehicle("squalo", "squalo", "Shitzu Squalo", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 400514754
            new Vehicle("submersible", "submersible", "Мини-подлодка", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 771711535
            new Vehicle("submersible2", "submersible2", "Kraken", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 3228633070
            new Vehicle("suntrap", "suntrap", "Shitzu Suntrap", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 4012021193
            new Vehicle("toro", "toro", "Lampadati Toro", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 1070967343
            new Vehicle("toro2", "toro2", "Lampadati Toro", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 908897389
            new Vehicle("tropic", "tropic", "Shitzu Tropic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 290013743
            new Vehicle("tropic2", "tropic2", "Shitzu Tropic", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 1448677353
            new Vehicle("tug", "tug", "Буксир", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Boat); // 2194326579
            #endregion

            #region Cycles
            new Vehicle("bmx", "bmx", "BMX", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Cycle); // 1131912276
            new Vehicle("cruiser", "cruiser", "Cruiser", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Cycle); // 448402357
            new Vehicle("fixter", "fixter", "Fixter", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Cycle); // 3458454463
            new Vehicle("scorcher", "scorcher", "Scorcher", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Cycle); // 4108429845
            new Vehicle("tribike", "tribike", "Whippet", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Cycle); // 1127861609
            new Vehicle("tribike2", "tribike2", "Endurex", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Cycle); // 3061159916
            new Vehicle("tribike3", "tribike3", "Tri-Cycles", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Cycle); // 3894672200
            #endregion

            #region Trains
            new Vehicle("freight", "freight", "Товарный поезд", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Train); // 1030400667
            new Vehicle("freightcar", "freightcar", "Товарный поезд", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Train); // 184361638
            new Vehicle("freightcar2", "freightcar2", "Товарный поезд", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Train); // 3186376089
            new Vehicle("freightcont1", "freightcont1", "Товарный поезд", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Train); // 920453016
            new Vehicle("freightcont2", "freightcont2", "Товарный поезд", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Train); // 240201337
            new Vehicle("freightgrain", "freightgrain", "Товарный поезд", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Train); // 642617954
            new Vehicle("metrotrain", "metrotrain", "Товарный поезд", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Train); // 868868440
            new Vehicle("tankercar", "tankercar", "Товарный поезд", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Train); // 586013744
            #endregion

            #region Trailers
            new Vehicle("armytanker", "armytanker", "Армейский фургон", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 3087536137
            new Vehicle("armytrailer", "armytrailer", "Армейский фургон", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2818520053
            new Vehicle("armytrailer2", "armytrailer2", "Армейский фургон", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2657817814
            new Vehicle("baletrailer", "baletrailer", "Грузовой трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 3895125590
            new Vehicle("boattrailer", "boattrailer", "Лодочный трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 524108981
            new Vehicle("cablecar", "cablecar", "Фуникулер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 3334677549
            new Vehicle("docktrailer", "docktrailer", "docktrailer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2154757102
            new Vehicle("freighttrailer", "freighttrailer", "freighttrailer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 3517691494
            new Vehicle("graintrailer", "graintrailer", "graintrailer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 1019737494
            new Vehicle("proptrailer", "proptrailer", "proptrailer", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 356391690
            new Vehicle("raketrailer", "raketrailer", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 390902130
            new Vehicle("tanker", "tanker", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 3564062519
            new Vehicle("tanker2", "tanker2", "tanker2", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 1956216962
            new Vehicle("tr2", "tr2", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2078290630
            new Vehicle("tr3", "tr3", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 1784254509
            new Vehicle("tr4", "tr4", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2091594960
            new Vehicle("trailerlarge", "trailerlarge", "Подвижный командный пункт", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 1502869817
            new Vehicle("trailerlogs", "trailerlogs", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2016027501
            new Vehicle("trailers", "trailers", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 3417488910
            new Vehicle("trailers2", "trailers2", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2715434129
            new Vehicle("trailers3", "trailers3", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2236089197
            new Vehicle("trailers4", "trailers4", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 3194418602
            new Vehicle("trailersmall", "trailersmall", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 712162987
            new Vehicle("trailersmall2", "trailersmall2", "Vom Feuer Трейлер ПВО", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2413121211
            new Vehicle("trflat", "trflat", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2942498482
            new Vehicle("tvtrailer", "tvtrailer", "Трейлер", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Trailer); // 2524324030
            #endregion

            #region Other
            new Vehicle("thruster", "thruster", "Mammoth Thruster", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Jetpack); // 1489874736

            new Vehicle("blimp", "blimp", "Atomic Blimp", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Blimp); // 4143991942
            new Vehicle("blimp2", "blimp2", "Xero Blimp", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Blimp); // 3681241380
            new Vehicle("blimp3", "blimp3", "Дирижабль", 80f, Vehicle.FuelTypes.Petrol, new Vehicle.Trunk(15, 25), true, false, false, Vehicle.Types.Blimp); // 3987008919
            #endregion

            var lines = new List<string>();

            foreach (var x in All)
            {
                x.Value.GovPrice = Game.Businesses.Shop.AllPrices.Where(y => y.Value.Prices.ContainsKey(x.Key)).Select(y => y.Value.Prices[x.Key] * y.Value.RealPrice).FirstOrDefault();

                lines.Add($"new Vehicle(\"{x.Key}\", {x.Value.Model}, \"{x.Value.Name}\", {x.Value.Tank}f, Vehicle.FuelTypes.{x.Value.FuelType}, {(x.Value.TrunkData == null ? "null" : $"new Vehicle.Trunk({x.Value.TrunkData.Slots}")}, {x.Value.TrunkData.MaxWeight}), {x.Value.IsModdable.ToString().ToLower()}, {x.Value.HasCruiseControl.ToString().ToLower()}, {x.Value.HasAutoPilot.ToString().ToLower()}, Vehicle.Types.{x.Value.Type}, {x.Value.GovPrice}, Vehicle.ClassTypes.{x.Value.Class});");
            }

            Utils.FillFileToReplaceRegion(Settings.DIR_CLIENT_VEHICLES_DATA_PATH, "TO_REPLACE", lines);

            return All.Count;
        }
    }
}
