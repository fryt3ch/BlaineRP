using BCRPServer.Sync;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BCRPServer
{
    public partial class VehicleData
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

        public static void Remove(VehicleData vData)
        {
            if (vData == null)
                return;

            if (vData.DeletionTimer is Timer deletionTimer)
                deletionTimer.Dispose();

            vData.Info.VehicleData = null;

            All.Remove(vData.Vehicle);

            vData.Vehicle.DetachAllEntities();

            vData.Vehicle.GetEntityIsAttachedTo()?.DetachEntity(vData.Vehicle);

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

        public enum OwnerTypes
        {
            /// <summary>Доступна всем</summary>
            AlwaysFree = -1,
            /// <summary>Принадлежит игроку</summary>
            Player,
            /// <summary>Временная, назначена игроку</summary>
            PlayerTemp,
            /// <summary>Арендуется игроком</summary>
            PlayerRent,
            /// <summary>Арендуется игроком, при этом - принадлежит работе</summary>
            PlayerRentJob,
            /// <summary>Используется игроком для прохождения практического экзамена в автошколе</summary>
            PlayerDrivingSchool,
            /// <summary>Принадлежит фракции</summary>
            Fraction,
        }

        public enum StationTypes : byte
        {
            /// <summary>Радио выкл.</summary>
            Off = 0,

            /// <summary>Los Santos Rock Radio</summary>
            LSRR,
            /// <summary>Non-Stop-Pop FM</summary>
            NSPFM,
            /// <summary>Radio Los Santos</summary>
            RLS,
            /// <summary>Channel X</summary>
            CHX,
            /// <summary>West Coast Talk Radio</summary>
            WCTR,
            /// <summary>Rebel Radio</summary>
            RR,
            /// <summary>Soulwax FM</summary>
            SWFM,
            /// <summary>East Los FM</summary>
            ELFM,
            /// <summary>West Coast Classics</summary>
            WCC,
            /// <summary>Blaine County Talk Radio</summary>
            BCTR,
            /// <summary>Blue Ark</summary>
            BA,
            /// <summary>Worldwide FM</summary>
            WWFM,
            /// <summary>FlyLo FM</summary>
            FLFM,
            /// <summary>The Lowdown 91.1</summary>
            TLD,
            /// <summary>Radio Mirror Park</summary>
            RMP,
            /// <summary>Space 103.2</summary>
            SPA,
            /// <summary>Vinewood Boulevard Radio</summary>
            VWBR,
            /// <summary>Self Radio</summary>
            SR,
            /// <summary>The Lab</summary>
            TL,
            /// <summary>Blonded Los Santos 97.8 FM</summary>
            BLS,
            /// <summary>Los Santos Underground Radiok</summary>
            LSUR,
            /// <summary>iFruit Radio</summary>
            IFR,
            /// <summary>Still Slipping Los Santos</summary>
            SSLS,
            /// <summary>Kult FM</summary>
            KFM,
            /// <summary>The Music Locker</summary>
            TML,
            /// <summary>MOTOMAMI Los Santos</summary>
            MMLS,

            /// <summary>Media Player</summary>
            /// <remarks>Фактически, радио Blaine RP</remarks>
            MP_BRP,

            BRP_0,
            BRP_1,
            BRP_2,
            BRP_3,
            BRP_4,
        }

        /// <summary>Сущность транспорта</summary>
        /// <value>Объект класса Vehicle, если транспорт существует на сервере, null - в противном случае</value>
        public Vehicle Vehicle { get; set; }

        public VehicleData(Vehicle Vehicle)
        {
            this.Vehicle = Vehicle;

            //IsInvincible = true;

            EngineOn = false;
            Locked = false;

            LightsOn = false;
            LeftIndicatorOn = false;
            RightIndicatorOn = false;

            TrunkLocked = true;
            HoodLocked = true;

            Radio = StationTypes.Off;

            DirtLevel = 0;

            AttachedObjects = new List<AttachSystem.AttachmentObjectNet>();
            AttachedEntities = new List<AttachSystem.AttachmentEntityNet>();

            Vehicle.SetData(Sync.AttachSystem.AttachedObjectsTimersKey, new Dictionary<Sync.AttachSystem.Types, Timer>());

            SetData(Vehicle, this);
        }

        public VehicleData(Vehicle Vehicle, VehicleInfo Info) : this(Vehicle)
        {
            this.Info = Info;

            TID = Info.TID;

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

        public bool AttachBoatToTrailer()
        {
            if (Data.Type != Game.Data.Vehicles.Vehicle.Types.Boat)
                return false;

            return Vehicle.AttachObject(Game.Data.Vehicles.GetData("boattrailer").Model, AttachSystem.Types.TrailerObjOnBoat, -1, null);
        }

        public bool DetachBoatFromTrailer()
        {
            if (Data.Type != Game.Data.Vehicles.Vehicle.Types.Boat)
                return false;

            return Vehicle.DetachObject(AttachSystem.Types.TrailerObjOnBoat);
        }

        public bool IsBoatAttachedToTrailer()
        {
            if (Data.Type != Game.Data.Vehicles.Vehicle.Types.Boat)
                return false;

            return AttachedObjects.Where(x => x.Type == AttachSystem.Types.TrailerObjOnBoat).Any();
        }

        public void SetFreezePosition(Vector3 pos, float? heading = null)
        {
            if (heading is float fHeading)
            {
                Vehicle.SetSharedData("IsFrozen", $"{pos.X}_{pos.Y}_{pos.Z}_{fHeading}");
            }
            else
            {
                Vehicle.SetSharedData("IsFrozen", $"{pos.X}_{pos.Y}_{pos.Z}");
            }
        }

        public void Delete(bool completely)
        {
            if (completely)
            {
                this.Numberplate?.Delete();

                if (TID is uint tid)
                    Game.Items.Container.Get(tid)?.Delete();

                if (OwnerType == OwnerTypes.Player)
                {
                    var pInfo = PlayerData.PlayerInfo.Get(OwnerID);

                    if (pInfo != null)
                    {
                        var pData = pInfo.PlayerData;

                        if (pData != null)
                        {
                            pData.RemoveVehicleProperty(Info);
                        }
                    }

                    Delete(this);
                }
            }
            else
            {
                if (OwnerType == OwnerTypes.Player)
                {
                    if (IsDead)
                    {
                        Info.IsOnVehiclePound = true;
                    }
                    else
                    {
                        if (LastData.GarageSlot < 0)
                        {
                            LastData.Position = Vehicle.Position;
                            LastData.Heading = Vehicle.Heading;
                            LastData.Dimension = Vehicle.Dimension;
                        }
                    }

                    Remove(this);

                    MySQL.VehicleDeletionUpdate(Info);

                    Console.WriteLine($"[VehDeletion] Deleted VID: {VID}");
                }
                else if (OwnerType == OwnerTypes.PlayerRent)
                {
                    var owner = OwnerID > 0 ? PlayerData.PlayerInfo.Get(OwnerID) : null;

                    OwnerID = 0;

                    if (owner != null)
                    {
                        if (owner.PlayerData != null)
                        {
                            owner.PlayerData.RemoveRentedVehicle(this);
                        }
                    }

                    if (LastData.GarageSlot == int.MinValue)
                    {
                        Remove(this);
                    }
                    else
                    {
                        var numberplate = Vehicle.NumberPlate;
                        var numberplateS = Vehicle.NumberPlateStyle;

                        Remove(this);

                        var data = Data;

                        Info.LastData.Fuel = data.Tank;

                        var vData = Info.Spawn();

                        if (vData != null)
                        {
                            vData.Vehicle.NumberPlate = numberplate;
                            vData.Vehicle.NumberPlateStyle = numberplateS;
                        }
                    }
                }
                else if (OwnerType == OwnerTypes.PlayerRentJob)
                {
                    var jobData = Job;

                    var owner = OwnerID > 0 ? PlayerData.PlayerInfo.Get(OwnerID) : null;

                    OwnerID = 0;

                    if (owner != null)
                    {
                        if (owner.PlayerData != null)
                        {
                            owner.PlayerData.RemoveRentedVehicle(this);
                        }
                    }

                    var numberplate = Vehicle.NumberPlate;
                    var numberplateS = Vehicle.NumberPlateStyle;

                    Remove(this);

                    Info.LastData.Fuel = Data.Tank;

                    var vData = Info.Spawn();

                    if (vData != null)
                    {
                        vData.Job = jobData;

                        vData.Vehicle.NumberPlate = numberplate;
                        vData.Vehicle.NumberPlateStyle = numberplateS;
                    }

                    if (jobData is Game.Jobs.IVehicles jobDataV)
                    {
                        jobDataV.OnVehicleRespawned(Info, owner);
                    }
                }
                else if (OwnerType == OwnerTypes.Fraction)
                {
                    Remove(this);
                }
            }
        }

        #region Create New
        public static VehicleData New(PlayerData pData, Game.Data.Vehicles.Vehicle vType, Utils.Colour color1, Utils.Colour color2, Vector3 position, float heading, uint dimension, bool setInto = false)
        {
            var player = pData.Player;

            var vInfo = new VehicleInfo()
            {
                VID = VehicleInfo.MoveNextId(),

                Data = vType,
                AllKeys = new List<uint>(),
                OwnerType = OwnerTypes.Player,
                OwnerID = pData.CID,
                ID = vType.ID,
                Numberplate = null,
                Tuning = Game.Data.Vehicles.Tuning.CreateNew(color1, color2),
                LastData = new LastVehicleData() { Position = position, Dimension = dimension, Heading = heading, Fuel = vType.Tank, Mileage = 0f, GarageSlot = -1 },
                RegistrationDate = Utils.GetCurrentTime(),
            };

            var cont = vType.TrunkData == null ? null : Game.Items.Container.Create($"vt_{vType.ID}", null);

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

                if (setInto && player?.Exists == true)
                {
                    player.SetIntoVehicle(veh, 0);
                }
            }, 1500);

            return vData;
        }

        public static VehicleData NewTemp(PlayerData pData, Game.Data.Vehicles.Vehicle vType, Utils.Colour color1, Utils.Colour color2, Vector3 position, float heading, uint dimension)
        {
            var player = pData.Player;

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
                AllKeys = new List<uint>(),
                OwnerType = OwnerTypes.PlayerTemp,
                OwnerID = pData.CID,
                ID = vType.ID,
                Numberplate = null,
                Tuning = Game.Data.Vehicles.Tuning.CreateNew(color1, color2),
                LastData = new LastVehicleData() { Position = position, Dimension = dimension, Heading = heading, Fuel = vType.Tank, Mileage = 0f, GarageSlot = -1 },
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

        public static VehicleData NewTemp(Game.Data.Vehicles.Vehicle vType, Utils.Colour color1, Utils.Colour color2, Vector3 position, float heading, uint dimension)
        {
            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
                AllKeys = new List<uint>(),
                OwnerType = OwnerTypes.AlwaysFree,
                OwnerID = 0,
                ID = vType.ID,
                Numberplate = null,
                Tuning = Game.Data.Vehicles.Tuning.CreateNew(color1, color2),
                LastData = new LastVehicleData() { Position = position, Dimension = dimension, Heading = heading, Fuel = vType.Tank, Mileage = 0f, GarageSlot = -1 },
                RegistrationDate = Utils.GetCurrentTime(),
            };

            var vData = new VehicleData(vInfo.CreateVehicle(), vInfo);

            vInfo.VehicleData = vData;

            var veh = vData.Vehicle;

            NAPI.Task.Run(() =>
            {
                if (veh?.Exists != true)
                    return;

                veh.Dimension = dimension;
            }, 1500);

            return vData;
        }

        public static VehicleData NewRent(PlayerData pData, Game.Data.Vehicles.Vehicle vType, Utils.Colour color1, Utils.Colour color2, Vector3 position, float heading, uint dimension)
        {
            var player = pData.Player;

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
                AllKeys = new List<uint>(),
                OwnerType = OwnerTypes.PlayerRent,
                OwnerID = pData.CID,
                ID = vType.ID,
                Numberplate = null,
                Tuning = Game.Data.Vehicles.Tuning.CreateNew(color1, color2),
                LastData = new LastVehicleData() { Position = position, Dimension = dimension, Heading = heading, Fuel = vType.Tank, Mileage = 0f, GarageSlot = int.MinValue },
                RegistrationDate = Utils.GetCurrentTime(),
            };

            var vData = new VehicleData(vInfo.CreateVehicle(), vInfo);

            vInfo.VehicleData = vData;

            pData.AddRentedVehicle(vData, 300_000);

            var veh = vData.Vehicle;

            veh.NumberPlate = "RENT";

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

        public static VehicleData.VehicleInfo NewJob(int jobId, string numberplateText, Game.Data.Vehicles.Vehicle vType, Utils.Colour color1, Utils.Colour color2, Utils.Vector4 position, uint dimension)
        {
            var job = Game.Jobs.Job.Get(jobId);

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
                AllKeys = new List<uint>(),
                OwnerType = OwnerTypes.PlayerRentJob,
                OwnerID = 0,
                ID = vType.ID,
                Numberplate = null,
                Tuning = Game.Data.Vehicles.Tuning.CreateNew(color1, color2),
                LastData = new LastVehicleData() { Position = position.Position, Dimension = dimension, Heading = position.RotationZ, Fuel = vType.Tank, Mileage = 0f, GarageSlot = -1 },
                RegistrationDate = Utils.GetCurrentTime(),
            };

            var vData = new VehicleData(vInfo.CreateVehicle(), vInfo);

            vInfo.VehicleData = vData;

            vData.Job = job;

            var veh = vData.Vehicle;

            if (job is Game.Jobs.IVehicles vehJob)
            {
                veh.NumberPlate = $"{numberplateText}{vehJob.Vehicles.Count + 1}";
            }

            NAPI.Task.Run(() =>
            {
                if (veh?.Exists != true)
                    return;

                veh.Dimension = dimension;
            }, 1500);

            return vInfo;
        }

        public static VehicleData.VehicleInfo NewAutoschool(int autoschoolId, Game.Data.Vehicles.Vehicle vType, Utils.Colour color1, Utils.Colour color2, Utils.Vector4 position, uint dimension)
        {
            var autoschool = Game.Autoschool.Get(autoschoolId);

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
                AllKeys = new List<uint>(),
                OwnerType = OwnerTypes.PlayerDrivingSchool,
                OwnerID = 0,
                ID = vType.ID,
                Numberplate = null,
                Tuning = Game.Data.Vehicles.Tuning.CreateNew(color1, color2),
                LastData = new LastVehicleData() { Position = position.Position, Dimension = dimension, Heading = position.RotationZ, Fuel = vType.Tank, Mileage = 0f, GarageSlot = -1 },
                RegistrationDate = Utils.GetCurrentTime(),
            };

            var vData = new VehicleData(vInfo.CreateVehicle(), vInfo);

            vInfo.VehicleData = vData;

            var veh = vData.Vehicle;

            veh.NumberPlate = "DRSCHOOL";

            NAPI.Task.Run(() =>
            {
                if (veh?.Exists != true)
                    return;

                veh.Dimension = dimension;
            }, 1500);

            return vInfo;
        }

        public bool StartDeletionTask(int time)
        {
            if (DeletionTimer is Timer deletionTimer)
                deletionTimer.Dispose();

            DeletionTimer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (Vehicle?.Exists != true)
                        return;

                    Delete(false);
                });
            }, null, time, Timeout.Infinite);

            Console.WriteLine("[VehDeletion] Started deletion");

            return true;
        }

        public bool CancelDeletionTask()
        {
            if (DeletionTimer is Timer deletionTimer)
            {
                deletionTimer.Dispose();

                DeletionTimer = null;

                Console.WriteLine("[VehDeletion] Cancelled deletion");

                return true;
            }

            return false;
        }

        public bool IsFullOwner(PlayerData pData, bool notify = true)
        {
            if (OwnerType == OwnerTypes.Player && OwnerID == pData.CID)
            {
                return true;
            }

            if (notify)
            {
                pData.Player.Notify("Vehicle::NFO");
            }

            return false;
        }

        public bool CanManipulate(PlayerData pData, bool notify = true)
        {
            if (OwnerType == OwnerTypes.AlwaysFree)
                return true;

            if (OwnerType == OwnerTypes.Player)
            {
                if (OwnerID == pData.CID)
                    return true;

                for (int i = 0; i < pData.Items.Length; i++)
                {
                    if (pData.Items[i] is Game.Items.VehicleKey vKey)
                    {
                        if (vKey.IsKeyValid(Info))
                            return true;
                    }
                }
            }
            else if (OwnerType == OwnerTypes.PlayerTemp || OwnerType == OwnerTypes.PlayerRent)
            {
                if (OwnerID == pData.CID)
                    return true;
            }
            else if (OwnerType == OwnerTypes.PlayerRentJob)
            {
                if (OwnerID == pData.CID)
                    return true;
            }
            else if (OwnerType == OwnerTypes.Fraction)
            {
                var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)OwnerID);

                if (pData.Fraction == fData.Type && pData.Info.FractionRank >= (fData.AllVehicles.GetValueOrDefault(Info)?.MinimalRank ?? byte.MaxValue))
                    return true;
            }

            if (notify)
            {
                pData.Player.Notify("Vehicle::NotAllowed");
            }

            return false;
        }

        public bool CanUseTrunk(PlayerData pData, bool notify = true)
        {
            if (!TrunkLocked)
                return true;

            return CanManipulate(pData, notify);
        }

        #endregion
    }
}