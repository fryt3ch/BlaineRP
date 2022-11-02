using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BCRPServer.Sync;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace BCRPServer
{
    public class VehicleData
    {
        public static Dictionary<Vehicle, VehicleData> Vehicles { get; private set; } = new Dictionary<Vehicle, VehicleData>();

        /// <summary>Получить VehicleData транспорта</summary>
        /// <returns>Объект класса PlayerData если существует, иначе - null</returns>
        public static VehicleData GetData(Vehicle vehicle)
        {
            if (vehicle == null)
                return null;

            return Vehicles.GetValueOrDefault(vehicle);
        }

        /// <summary>Назначить объект класса VehicleData транспорту</summary>
        public static void SetData(Vehicle vehicle, VehicleData data)
        {
            if (vehicle == null)
                return;

            VehicleData existing;

            if (Vehicles.TryGetValue(vehicle, out existing))
                existing = data;
            else
                Vehicles.Add(vehicle, data);
        }

        public enum FuelTypes
        {
            /// <summary>Бензин</summary>
            Petrol = 0,
            /// <summary>Электричество</summary>
            Electricity,
        }

        public enum OwnerTypes
        {
            /// <summary>Основной владелец</summary>
            Owner = 0,
            /// <summary>Имеет действительный ключ</summary>
            HasKey,
        }

        public void RemoveData() => Vehicles.Remove(Vehicle);

        #region Subclasses
        public class ParkData
        {
            public Vector3 Position { get; set; }
            public Vector3 Rotation { get; set; }
            public uint Dimension { get; set; }

            public ParkData(Vector3 Position, Vector3 Rotation, uint Dimension)
            {
                this.Position = Position;
                this.Rotation = Rotation;

                this.Dimension = Dimension;
            }
        }

        public class LastVehicleData
        {
            public float FuelLevel { get; set; }
            public float Mileage { get; set; }
            public ParkData Park { get; set; }

            public LastVehicleData() { }

            public static LastVehicleData Get(VehicleData vData) => new LastVehicleData() { FuelLevel = vData.FuelLevel, Mileage = vData.Mileage, Park = vData.Park };
        }
        #endregion

        /// <summary>Сущность транспорта</summary>
        /// <value>Объект класса Vehicle, если транспорт существует на сервере, null - в противном случае</value>
        public Vehicle Vehicle { get; set; }

        #region Local Data
        /// <summary>Второстепенный ID транспорта</summary>
        /// <value>Не уникальный ID транспорта, а его идентификатор (см. Game.Data.Vehicles)</value>
        public string ID { get; set; }

        /// <summary>Второстепенные данные транспорта</summary>
        public Game.Data.Vehicles.Vehicle Data { get => Game.Data.Vehicles.All[ID]; }

        /// <summary>Дата создания транспорта</summary>
        public DateTime RegistrationDate { get; set; }

        /// <summary>Данные о парковке транспорта</summary>
        public ParkData Park { get; set; }

        /// <summary>Действительные ключи от транспорта</summary>
        /// <value>Список UID предметов Game.Items.VehicleKey</value>
        public List<uint> Keys { get; set; }

        /// <summary>Номерной знак транспорта</summary>
        /// <value>Объект класса Game.Items.Numberplate, null - если отсутствует</value>
        public Game.Items.Numberplate Numberplate { get; set; }

        /// <summary>Тюнинг транспорта</summary>
        public Game.Data.Vehicles.Tuning Tuning { get; set; }

        /// <summary>Токен отмены удаления транспорта с сервера</summary>
        /// <value>Объект класса CancellationTokenSource, null - если отсутствует</value>
        public CancellationTokenSource CTSDelete { get; set; }

        /// <summary>CID владельца транспорта</summary>
        /// <value>Если есть владелец - его CID (положительное значение), иначе - отрицательное значение</value>
        public int Owner { get; set; }

        /// <summary>Фракция транспорта</summary>
        /// <value>FractionType, если транспорт - фракционный, FractionType.None - в противном случае</value>
        public PlayerData.FractionTypes Fraction { get => Owner < 0 ? (PlayerData.FractionTypes)(-Owner) : PlayerData.FractionTypes.None; }

        public SemaphoreSlim Semaphore { get; set; }

        public Player[] Passengers { get; set; }
        #endregion

        #region Shared Data
        /// <summary>ID багажника</summary>
        /// <remarks>Фактически, это ID контейнера (см. Game.Items.Container)</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>ID багажника, если отсутвует - null</value>
        public uint? TID { get => Vehicle.GetSharedData<string>("TID").DeserializeFromJson<uint?>(); set => Vehicle.SetSharedData("TID", value.SerializeToJson()); }

        /// <summary>Уровень топлива</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public float FuelLevel { get => Vehicle.GetSharedData<float>("Fuel::Level"); set { Vehicle.SetSharedData("Fuel::Level", value); } }

        /// <summary>Пробег</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Пробег в метрах</value>
        public float Mileage { get => Vehicle.GetSharedData<float>("Mileage"); set { Vehicle.SetSharedData("Mileage", value); } }

        /// <summary>Включён ли двигатель?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool EngineOn { get => Vehicle.GetSharedData<bool>("Engine::On"); set { Vehicle.SetSharedData("Engine::On", value); Vehicle.EngineStatus = value; } }

        /// <summary>Заблокированы ли двери?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool Locked { get => Vehicle.GetSharedData<bool>("Doors::Locked"); set { Vehicle.SetSharedData("Doors::Locked", value); Vehicle.Locked = value; } }

        /// <summary>Включёны ли фары?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool LightsOn { get => Vehicle.GetSharedData<bool>("Lights::On"); set { Vehicle.SetSharedData("Lights::On", value); } }

        /// <summary>Включён ли левый поворотник?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool LeftIndicatorOn { get => Vehicle.GetSharedData<bool>("Indicators::LeftOn"); set { Vehicle.SetSharedData("Indicators::LeftOn", value); } }

        /// <summary>Включён ли правый поворотник?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool RightIndicatorOn { get => Vehicle.GetSharedData<bool>("Indicators::RightOn"); set { Vehicle.SetSharedData("Indicators::RightOn", value); } }

        /// <summary>Текущая радиостанция</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>255 - радио выключено</value>
        public int Radio { get => Vehicle.GetSharedData<int>("Radio"); set { Vehicle.SetSharedData("Radio", value); } }

        /// <summary>Текущая скорость толкания транспорта</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public float ForcedSpeed { get => Vehicle.GetSharedData<float>("ForcedSpeed"); set { Vehicle.SetSharedData("ForcedSpeed", value); } }

        /// <summary>Заблокирован ли багажник?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool TrunkLocked { get => Vehicle.GetSharedData<bool>("Trunk::Locked"); set { Vehicle.SetSharedData("Trunk::Locked", value); } }

        /// <summary>Заблокирован ли капот?</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public bool HoodLocked { get => Vehicle.GetSharedData<bool>("Hood::Locked"); set { Vehicle.SetSharedData("Hood::Locked", value); } }

        public float DirtLevel { get => Vehicle.GetSharedData<float>("Dirt::Level"); set { Vehicle.SetSharedData("Dirt::Level", value); } }

        public int[] DoorsStates { get => Vehicle.GetSharedData<Newtonsoft.Json.Linq.JArray>("Doors::States").ToObject<int[]>(); set => Vehicle.SetSharedData("Doors::States", value); }

        public bool IsInvincible { get => Vehicle.GetSharedData<bool>("IsInvincible"); set { Vehicle.SetSharedData("IsInvincible", value); } }

        /// <summary>Уникальный ID транспорта</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>VID транспорта, -1 - если отсутствует</value>
        public int VID { get => Vehicle.GetSharedData<int>("VID"); set => Vehicle.SetSharedData("VID", value); }

        /// <summary>Прикрепленные объекты к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentObjectNet> AttachedObjects { get => Vehicle.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedObjectsKey).ToList<Sync.AttachSystem.AttachmentObjectNet>(); set { Vehicle.SetSharedData(Sync.AttachSystem.AttachedObjectsKey, value); } }
        
        /// <summary>Прикрепленные сущности к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentEntityNet> AttachedEntities { get => Vehicle.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedEntitiesKey).ToList<Sync.AttachSystem.AttachmentEntityNet>(); set { Vehicle.SetSharedData(Sync.AttachSystem.AttachedEntitiesKey, value); } }
        #endregion

        public void Reset()
        {
            Vehicle.ResetData();
        }

        public VehicleData(Vehicle Vehicle)
        {
            this.Vehicle = Vehicle;

            IsInvincible = true;

            EngineOn = false;
            Locked = false;

            LightsOn = false;
            LeftIndicatorOn = false;
            RightIndicatorOn = false;

            TrunkLocked = true;
            HoodLocked = true;

            Radio = 255;

            DirtLevel = 0.001f;
            DoorsStates = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };

            ForcedSpeed = 0;

            Passengers = new Player[Vehicle.MaxOccupants];

            AttachedObjects = new List<AttachSystem.AttachmentObjectNet>();
            AttachedEntities = new List<AttachSystem.AttachmentEntityNet>();

            Semaphore = new SemaphoreSlim(1, 1);

            SetData(Vehicle, this);

/*            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);

                    NAPI.Task.Run(() =>
                    {
                        Console.WriteLine($"HP: {Vehicle.Health}, Controller: {Vehicle.Controller?.Id}, IsAlive: {Vehicle.IsInstanceAlive}");
                    });
                }
            });*/
        }

        public static async Task<VehicleData> Load(int vid)
        {
            var vData = await NAPI.Task.RunAsync(() => Vehicles.Where(x => x.Value?.VID == vid).Select(x => x.Value).FirstOrDefault());

            if (vData == null)
                return MySQL.GetVehicle(vid);
            else
            {
                vData.CancelDeletionTask();

                return vData;
            }
        }

        public void Delete(bool completely)
        {
            if (!this.WaitAsync().GetAwaiter().GetResult())
                return;

            CancelDeletionTask();

            (PlayerData pData, int VID, uint? TID)? safeData = NAPI.Task.RunAsync(() =>
            {
                RemoveData();

                if (Vehicle?.Exists != true)
                    return null;

                return ((PlayerData pData, int VID, uint? TID)?)(this.Owner > 0 ? Utils.GetPlayerByCID(this.Owner)?.GetMainData() : null, this.VID, this.TID);
            }).GetAwaiter().GetResult();

            if (safeData == null)
                return;

            if (completely)
            {
                if (safeData.Value.VID > 0)
                    MySQL.DeleteVehicle(safeData.Value.VID);

                this.Numberplate?.Delete();

                if (safeData.Value.TID != null)
                    (Game.Items.Container.Get((uint)safeData.Value.TID))?.Delete(true);

                if (safeData.Value.pData != null)
                {
                    if (safeData.Value.pData.WaitAsync().GetAwaiter().GetResult())
                    {
                        safeData.Value.pData.OwnedVehicles.Remove(safeData.Value.VID);

                        safeData.Value.pData.Release();
                    }
                }
            }
            else
            {
                if (safeData.Value.VID > 0)
                    MySQL.UpdateVehicle(this, false, false, false, true, false);
            }

            NAPI.Task.RunAsync(() =>
            {
                Reset();

                Vehicle?.Delete();
                Vehicle = null;
            }).GetAwaiter().GetResult();

            Console.WriteLine($"[VehDeletion] Deleted VID: {safeData.Value.VID}");
        }

        public VehicleData Respawn()
        {
            var veh = Vehicle;

            var vid = NAPI.Task.RunAsync(() =>
            {
                if (veh?.Exists != true)
                    return -1;

                return VID;
            }).GetAwaiter().GetResult();

            if (vid < 0)
                return null;

            Delete(false);

            return MySQL.GetVehicle(vid);
        }

        #region Create New
        public static async Task<VehicleData> New(PlayerData pData, string id, Color color, Vector3 position, Vector3 rotation, uint dimension, bool setInto = false)
        {
            if (!Game.Data.Vehicles.All.ContainsKey(id))
                return null;

            var player = pData.Player;

            var vehData = Game.Data.Vehicles.All[id];

            var cont = await Game.Items.Container.Create(new Game.Items.Container(id), null);

            var res = await NAPI.Task.RunAsync<VehicleData>(() =>
            {
                if (player?.Exists != true)
                    return null;

                var veh = NAPI.Vehicle.CreateVehicle(vehData.Model, position, 0f, 0, 0, "", 255, true, true, Utils.Dimensions.Stuff);

                veh.NumberPlateStyle = 0;
                veh.Rotation = rotation;
                veh.CustomPrimaryColor = color;
                veh.CustomSecondaryColor = color;

                var data = new VehicleData(veh)
                {
                    Owner = pData.CID,
                    ID = id,
                    RegistrationDate = Utils.GetCurrentTime(),
                    Keys = new List<uint>(),

                    FuelLevel = vehData.Tank,
                    Mileage = 0,
                    Park = null,

                    TID = cont.ID,
                    Numberplate = null,

                    Tuning = Game.Data.Vehicles.Tuning.GetTuning(veh, vehData.Moddable),
                };

                return data;
            });

            if (!await res.WaitAsync())
            {
                return null;
            }

            await Task.Run(async () =>
            {
                var veh = res.Vehicle;

                var vid = MySQL.AddVehicle(res);

                if (await pData.WaitAsync())
                {
                    pData.OwnedVehicles.Add(vid);
                }

                cont.UpdateOwner(veh);

                await NAPI.Task.RunAsync(() =>
                {
                    if (veh?.Exists != true)
                        return;

                    res.VID = vid;
                });

                NAPI.Task.Run(() =>
                {
                    if (veh?.Exists != true)
                        return;

                    veh.Dimension = dimension;

                    if (player?.Exists != true || !setInto)
                        return;

                    player.SetIntoVehicle(veh, 0);
                }, 1500);
            });

            res.Release();

            return res;
        }

        public static async Task<VehicleData> NewRent(PlayerData pData, string id, Color color, Vector3 position, Vector3 rotation, uint dimension, int deleteAfter = -1)
        {
            var player = pData.Player;

            if (!Game.Data.Vehicles.All.ContainsKey(id))
                return null;

            var vehData = Game.Data.Vehicles.All[id];

            var res = await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return null;

                var veh = NAPI.Vehicle.CreateVehicle(vehData.Model, position, 0f, 0, 0, "RENT", 255, true, true, Utils.Dimensions.Stuff);

                veh.NumberPlateStyle = 0;
                veh.Rotation = rotation;
                veh.CustomPrimaryColor = color;
                veh.CustomSecondaryColor = color;

                var data = new VehicleData(veh)
                {
                    FuelLevel = vehData.Tank,
                    Mileage = 0,
                    Park = null,

                    Owner = -1,
                    ID = id,
                    TID = null,
                    Numberplate = null,
                    VID = -1,

                    Tuning = null,
                };

                NAPI.Task.Run(() =>
                {
                    if (veh?.Exists != true)
                        return;

                    veh.Dimension = dimension;

                    player?.SetIntoVehicle(veh, 0);
                }, 1000);

                return data;
            });

            return res;
        }

        public static async Task<VehicleData> NewTemp(PlayerData pData, string id, Color color, Vector3 position, Vector3 rotation, uint dimension)
        {
            var player = pData.Player;
            
            if (!Game.Data.Vehicles.All.ContainsKey(id))
                return null;

            var vehData = Game.Data.Vehicles.All[id];

            var res = await NAPI.Task.RunAsync(() =>
            {
                if (player?.Exists != true)
                    return null;

                var veh = NAPI.Vehicle.CreateVehicle(vehData.Model, position, 0f, 0, 0, "BLAINERP", 255, true, true, Utils.Dimensions.Stuff);

                veh.NumberPlateStyle = 1;
                veh.Rotation = rotation;
                veh.CustomPrimaryColor = color;
                veh.CustomSecondaryColor = color;

                var data = new VehicleData(veh)
                {
                    FuelLevel = vehData.Tank,
                    Mileage = 0,
                    Park = null,

                    Owner = pData.CID,
                    ID = id,
                    TID = null,
                    Numberplate = null,
                    VID = -1,

                    Tuning = null,
                };

                NAPI.Task.Run(() =>
                {
                    veh.Dimension = dimension;

                    player?.SetIntoVehicle(veh, 0);
                }, 1000);

                return data;
            });

            return res;
        }

        public void StartDeletionTask()
        {
            if (CTSDelete != null)
                return;

            if (!this.WaitAsync().GetAwaiter().GetResult())
                return;

            CTSDelete = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Settings.OWNED_VEHICLE_TIME_TO_AUTODELETE, CTSDelete.Token);

                    if (CTSDelete?.Token.IsCancellationRequested == false)  
                        Delete(false);
                }
                catch (Exception ex)
                {
                    CTSDelete?.Dispose();

                    CTSDelete = null;
                }
            });

            this.Release();

            Console.WriteLine("[VehDeletion] Started deletion");
        }

        public void CancelDeletionTask()
        {
            CTSDelete?.Cancel();

            Console.WriteLine("[VehDeletion] Cancelled deletion");
        }

        public OwnerTypes? IsOwner(PlayerData pData)
        {
            if (this.Owner == pData.CID)
                return OwnerTypes.Owner;

            var keys = pData.Items.Where(x => x is Game.Items.VehicleKey && (x as Game.Items.VehicleKey).VID == this.VID);

            foreach (var key in keys)
                if (this.Keys.Contains(key.UID))
                    return OwnerTypes.HasKey;

            return null;
        }

        #endregion

        #region Passangers
        public void AddPassenger(int seatId, PlayerData pData)
        {
            var player = pData.Player;

            if (seatId == -1)
                return;

            var seats = Passengers;

            if (seatId >= seats.Length)
                return;

            seats[seatId] = player;

            pData.VehicleSeat = seatId;

            //NAPI.Util.ConsoleOutput($"added {player.Id} to {seatId}");
        }

        public void RemovePassenger(int seatId)
        {
            if (seatId == -1)
                return;

            var seats = Passengers;

            if (seatId >= seats.Length)
                return;

            if (seats[seatId] == null)
                return;

            var player = seats[seatId];

            seats[seatId] = null;

            Passengers = seats;

            if (player == null)
                return;

            var data = player.GetMainData();

            if (data == null)
                return;

            data.VehicleSeat = -1;

            if (seatId == 0 && ForcedSpeed != 0f)
                ForcedSpeed = 0f;

            //NAPI.Util.ConsoleOutput($"removed {player.Id} from {seatId}");
        }
        #endregion
    }
}
