using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BCRPServer.Game.Items;
using BCRPServer.Sync;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace BCRPServer
{
    public class VehicleData
    {
        public static Dictionary<Vehicle, VehicleData> All { get; private set; } = new Dictionary<Vehicle, VehicleData>();

        /// <summary>Получить VehicleData транспорта</summary>
        /// <returns>Объект класса PlayerData если существует, иначе - null</returns>
        public static VehicleData GetData(Vehicle vehicle)
        {
            if (vehicle == null)
                return null;

            return All.GetValueOrDefault(vehicle);
        }

        /// <summary>Назначить объект класса VehicleData транспорту</summary>
        public static void SetData(Vehicle vehicle, VehicleData data)
        {
            if (vehicle == null)
                return;

            All.Add(vehicle, data);
        }

        private static void Remove(VehicleData vData)
        {
            if (vData == null)
                return;

            All.Remove(vData.Vehicle);

            vData.Vehicle.Delete();

            vData.Vehicle.ResetData();

            vData.Vehicle = null;

            if (vData.Info != null)
            {

            }
        }

        private static void Delete(VehicleData vData)
        {
            Remove(vData);

            VehicleInfo.Remove(vData.Info);
        }

        public enum OwningTypes
        {
            /// <summary>Основной владелец</summary>
            Owner = 0,
            /// <summary>Имеет действительный ключ</summary>
            HasKey,
        }

        public enum OwnerTypes
        {
            Player = 0,
            PlayerRent,
            PlayerTemp,
            Fraction,
        }

        #region Subclasses
        public class VehicleInfo
        {
            private static Queue<uint> FreeIDs { get; set; } = new Queue<uint>();

            public static Dictionary<uint, VehicleInfo> All { get; private set; } = new Dictionary<uint, VehicleInfo>();

            private static uint LastAddedMaxId { get; set; }

            public static uint MoveNextId()
            {
                uint id;

                if (!FreeIDs.TryDequeue(out id))
                {
                    id = ++LastAddedMaxId;
                }

                return id;
            }

            public static void AddFreeId(uint id) => FreeIDs.Enqueue(id);

            public static void AddOnLoad(VehicleInfo vInfo)
            {
                if (vInfo == null)
                    return;

                All.Add(vInfo.VID, vInfo);

                if (vInfo.VID > LastAddedMaxId)
                    LastAddedMaxId = vInfo.VID;
            }

            public static void Add(VehicleInfo vInfo)
            {
                if (vInfo == null)
                    return;

                All.Add(vInfo.VID, vInfo);

                MySQL.VehicleAdd(vInfo);
            }

            public static void Remove(VehicleInfo vInfo)
            {
                if (vInfo == null)
                    return;

                var vid = vInfo.VID;

                AddFreeId(vid);

                All.Remove(vid);

                if (vid > 0)
                    MySQL.VehicleDelete(vInfo);
            }

            public static VehicleInfo Get(uint id) => All.GetValueOrDefault(id);

            public static List<VehicleInfo> GetAllByCID(uint cid) => All.Values.Where(x => x != null && (x.OwnerType == OwnerTypes.Player && x.OwnerID == cid)).ToList();

            public Game.Data.Vehicles.Vehicle Data { get; set; }

            public uint VID { get; set; }

            public string ID { get; set; }

            public OwnerTypes OwnerType { get; set; }

            public uint OwnerID { get; set; }

            public List<uint> AllKeys { get; set; }

            public DateTime RegistrationDate { get; set; }

            public uint? TID { get; set; }

            public Game.Items.Numberplate Numberplate { get; set; }

            public Game.Data.Vehicles.Tuning Tuning { get; set; }

            public LastVehicleData LastData { get; set; }

            public VehicleInfo() { }

            public Vehicle CreateVehicle()
            {
                var data = Game.Data.Vehicles.All[ID];

                var veh = NAPI.Vehicle.CreateVehicle(data.Model, LastData.Pos, LastData.Heading, 0, 0, "", 255, false, false, Utils.Dimensions.Stuff);

                return veh;
            }

            public VehicleData Spawn()
            {
                var vData = VehicleData.All.Values.Where(x => x?.VID == VID).FirstOrDefault();

                if (vData == null)
                {
                    vData = new VehicleData(CreateVehicle(), this);

                    NAPI.Task.Run(() =>
                    {
                        if (vData.Vehicle?.Exists != true)
                            return;

                        vData.Vehicle.Dimension = vData.LastData.Dim;
                    }, 1500);

                    return vData;
                }
                else
                {
                    vData.CancelDeletionTask();

                    return vData;
                }
            }
        }

        public class LastVehicleData
        {
            public float Fuel { get; set; }

            public float Mileage { get; set; }

            public Vector3 Pos { get; set; }

            public float Heading { get; set; }

            public uint Dim { get; set; }

            public LastVehicleData() { }
        }
        #endregion

        /// <summary>Сущность транспорта</summary>
        /// <value>Объект класса Vehicle, если транспорт существует на сервере, null - в противном случае</value>
        public Vehicle Vehicle { get; set; }

        #region Local Data
        public OwnerTypes OwnerType { get => Info.OwnerType; set => Info.OwnerType = value; }

        /// <summary>Второстепенный ID транспорта</summary>
        /// <value>Не уникальный ID транспорта, а его идентификатор (см. Game.Data.Vehicles)</value>
        public string ID { get => Info.ID; set => Info.ID = value; }

        /// <summary>Второстепенные данные транспорта</summary>
        public Game.Data.Vehicles.Vehicle Data { get => Info.Data; set => Info.Data = value; }

        /// <summary>Дата создания транспорта</summary>
        public DateTime RegistrationDate { get => Info.RegistrationDate; set => Info.RegistrationDate = value; }

        /// <summary>Действительные ключи от транспорта</summary>
        /// <value>Список UID предметов Game.Items.VehicleKey</value>
        public List<uint> Keys { get => Info.AllKeys; set => Info.AllKeys = value; }

        /// <summary>Номерной знак транспорта</summary>
        /// <value>Объект класса Game.Items.Numberplate, null - если отсутствует</value>
        public Game.Items.Numberplate Numberplate { get => Info.Numberplate; set => Info.Numberplate = value; }

        /// <summary>Тюнинг транспорта</summary>
        public Game.Data.Vehicles.Tuning Tuning { get => Info.Tuning; set => Info.Tuning = value; }

        /// <summary>Токен отмены удаления транспорта с сервера</summary>
        /// <value>Объект класса CancellationTokenSource, null - если отсутствует</value>
        public CancellationTokenSource CTSDelete { get; set; }

        /// <summary>CID владельца транспорта</summary>
        public uint OwnerID { get => Info.OwnerID; set => Info.OwnerID = value; }

        public Player[] Passengers { get; set; }

        public VehicleInfo Info { get; set; }

        public LastVehicleData LastData { get => Info.LastData; set => Info.LastData = value; }
        #endregion

        #region Shared Data
        /// <summary>ID багажника</summary>
        /// <remarks>Фактически, это ID контейнера (см. Game.Items.Container)</remarks>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>ID багажника, если отсутвует - null</value>
        public uint? TID { get => Info.TID; set { Vehicle.SetSharedData("TID", value); Info.TID = value; } }

        /// <summary>Уровень топлива</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        public float FuelLevel { get => Info.LastData.Fuel; set { Vehicle.SetSharedData("Fuel::Level", value); Info.LastData.Fuel = value; } }

        /// <summary>Пробег</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Пробег в метрах</value>
        public float Mileage { get => Info.LastData.Mileage; set { Vehicle.SetSharedData("Mileage", value); Info.LastData.Mileage = value; } }

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

        public bool IsInvincible { get => Vehicle.GetSharedData<bool>("IsInvincible"); set { Vehicle.SetSharedData("IsInvincible", value); } }

        /// <summary>Уникальный ID транспорта</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>VID транспорта, null - если отсутствует</value>
        public uint VID { get => Info.VID; set { Vehicle.SetSharedData("VID", value); Info.VID = value; } }

        /// <summary>Прикрепленные объекты к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentObjectNet> AttachedObjects { get => Vehicle.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedObjectsKey).ToList<Sync.AttachSystem.AttachmentObjectNet>(); set { Vehicle.SetSharedData(Sync.AttachSystem.AttachedObjectsKey, value); } }
        
        /// <summary>Прикрепленные сущности к транспорту</summary>
        /// <exception cref="NonThreadSafeAPI">Только в основном потоке!</exception>
        /// <value>Список объектов класса Sync.AttachSystem.AttachmentNet</value>
        public List<Sync.AttachSystem.AttachmentEntityNet> AttachedEntities { get => Vehicle.GetSharedData<Newtonsoft.Json.Linq.JArray>(Sync.AttachSystem.AttachedEntitiesKey).ToList<Sync.AttachSystem.AttachmentEntityNet>(); set { Vehicle.SetSharedData(Sync.AttachSystem.AttachedEntitiesKey, value); } }
        #endregion

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

            ForcedSpeed = 0;

            Passengers = new Player[Vehicle.MaxOccupants];

            AttachedObjects = new List<AttachSystem.AttachmentObjectNet>();
            AttachedEntities = new List<AttachSystem.AttachmentEntityNet>();

            SetData(Vehicle, this);
        }

        public VehicleData(Vehicle Vehicle, VehicleInfo Info) : this(Vehicle)
        {
            this.Info = Info;

            TID = Info.TID;

            FuelLevel = Info.LastData.Fuel;
            Mileage = Info.LastData.Mileage;

            VID = Info.VID;

            Tuning.Apply(Vehicle);

            Numberplate?.Setup(this);

            if (TID != null)
            {
                var cont = Game.Items.Container.Get((uint)TID);

                if (cont != null)
                {
                    cont.UpdateOwner(Vehicle);
                }
            }
        }

        public void Delete(bool completely)
        {
            CancelDeletionTask();

            if (completely)
            {
                this.Numberplate?.Delete();

                if (TID != null)
                    Game.Items.Container.Get((uint)TID)?.Delete();

                if (OwnerType == OwnerTypes.Player)
                {
                    var pInfo = PlayerData.PlayerInfo.Get(OwnerID);

                    if (pInfo != null)
                    {
                        var pData = Utils.GetPlayerByCID(OwnerID);

                        if (pData == null)
                        {
                            pInfo.OwnedVehicles.Remove(Info);
                        }
                        else
                        {
                            pData.RemoveVehicleProperty(Info);
                        }
                    }

                    Delete(this);
                }
            }
            else
            {
                if (VID > 0)
                {
                    LastData.Pos = Vehicle.Position;
                    LastData.Heading = Vehicle.Heading;
                    LastData.Dim = Vehicle.Dimension;

                    MySQL.VehicleDeletionUpdate(this.Info);
                }

                Remove(this);
            }

            Console.WriteLine($"[VehDeletion] Deleted VID: {VID}");
        }

        public VehicleData Respawn()
        {
            /*            var veh = Vehicle;

                        var vid = NAPI.Task.RunAsync(() =>
                        {
                            if (veh?.Exists != true)
                                return -1;

                            return VID;
                        }).GetAwaiter().GetResult();

                        if (vid < 0)
                            return null;

                        Delete(false);

                        //return MySQL.GetVehicle(vid);

                        return null;*/

            return null;
        }

        #region Create New
        public static VehicleData New(PlayerData pData, Game.Data.Vehicles.Vehicle vType, Color color1, Color color2, Vector3 position, float heading, uint dimension, bool setInto = false)
        {
            var player = pData.Player;

            var vInfo = new VehicleInfo()
            {
                VID = VehicleInfo.MoveNextId(),

                AllKeys = new List<uint>(),
                OwnerType = OwnerTypes.Player,
                OwnerID = pData.CID,
                ID = vType.ID,
                Numberplate = null,
                Tuning = Game.Data.Vehicles.Tuning.CreateNew(color1, color2),
                LastData = new LastVehicleData() { Pos = position, Dim = dimension, Heading = heading, Fuel = vType.Tank, Mileage = 0f },
                RegistrationDate = Utils.GetCurrentTime(),
            };

            Game.Items.Container cont = vType.TrunkData == null ? null : Game.Items.Container.Create(vType.ID, null);

            if (cont != null)
            {
                vInfo.TID = cont.ID;
            }

            var vData = new VehicleData(vInfo.CreateVehicle(), vInfo);

            cont.UpdateOwner(vData.Vehicle);

            VehicleInfo.Add(vInfo);

            pData.AddVehicleProperty(vInfo);

            var veh = vData.Vehicle;

            NAPI.Task.Run(() =>
            {
                if (veh?.Exists != true)
                    return;

                veh.Dimension = dimension;

                if (player?.Exists != true || !setInto)
                    return;

                player.SetIntoVehicle(veh, 0);
            }, 1500);

            return vData;
        }

        public static VehicleData NewTemp(PlayerData pData, Game.Data.Vehicles.Vehicle vType, Color color1, Color color2, Vector3 position, float heading, uint dimension)
        {
            var player = pData.Player;

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                AllKeys = new List<uint>(),
                OwnerType = OwnerTypes.PlayerTemp,
                OwnerID = pData.CID,
                ID = vType.ID,
                Numberplate = null,
                Tuning = Game.Data.Vehicles.Tuning.CreateNew(color1, color2),
                LastData = new LastVehicleData() { Pos = position, Dim = dimension, Heading = heading, Fuel = vType.Tank, Mileage = 0f },
                RegistrationDate = Utils.GetCurrentTime(),
            };

            var vData = new VehicleData(vInfo.CreateVehicle(), vInfo);

            var veh = vData.Vehicle;

            NAPI.Task.Run(() =>
            {
                if (veh?.Exists != true)
                    return;

                veh.Dimension = dimension;

                if (player?.Exists != true)
                    return;

                player.SetIntoVehicle(veh, 0);
            }, 1500);

            return vData;
        }

        public void StartDeletionTask()
        {
            if (CTSDelete != null)
                return;

            CTSDelete = new CancellationTokenSource();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(Settings.OWNED_VEHICLE_TIME_TO_AUTODELETE, CTSDelete.Token);

                    if (CTSDelete != null)
                    {
                        CTSDelete.Cancel();

                        CTSDelete = null;
                    }

                    NAPI.Task.Run(() =>
                    {
                        Delete(false);
                    });
                }
                catch (Exception ex)
                {
                    if (CTSDelete != null)
                    {
                        CTSDelete.Cancel();

                        CTSDelete = null;
                    }
                }
            });

            Console.WriteLine("[VehDeletion] Started deletion");
        }

        public void CancelDeletionTask()
        {
            if (CTSDelete != null)
            {
                CTSDelete.Cancel();

                CTSDelete = null;

                Console.WriteLine("[VehDeletion] Cancelled deletion");
            }
        }

        public OwningTypes? IsOwner(PlayerData pData)
        {
            if (OwnerType == OwnerTypes.Player && OwnerID == pData.CID)
                return OwningTypes.Owner;

            foreach (var key in pData.Items.Where(x => x is Game.Items.VehicleKey key && key.VID == this.VID))
            {
                if (this.Keys.Contains(key.UID))
                {
                    return OwningTypes.HasKey;
                }
            }

            return null;
        }

        #endregion

        #region Passangers
        public void AddPassenger(int seatId, PlayerData pData)
        {
            var player = pData.Player;

            if (seatId == -1)
                return;

            if (seatId >= Passengers.Length)
                return;

            Passengers[seatId] = player;

            pData.VehicleSeat = seatId;

            //NAPI.Util.ConsoleOutput($"added {player.Id} to {seatId}");
        }

        public void RemovePassenger(PlayerData pData)
        {
            var seatId = pData.VehicleSeat;

            if (seatId == -1)
                return;

            if (seatId >= Passengers.Length)
                return;

            Passengers[seatId] = null;

            pData.VehicleSeat = -1;

            if (seatId == 0 && ForcedSpeed != 0f)
                ForcedSpeed = 0f;

            //NAPI.Util.ConsoleOutput($"removed {player.Id} from {seatId}");
        }
        #endregion
    }
}
