using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Attachments;
using BlaineRP.Server.Game.Misc;
using BlaineRP.Server.Game.Quests;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.EntitiesData.Vehicles
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

            vData.Vehicle.DetachAllEntities();

            vData.Vehicle.GetEntityIsAttachedTo()?.DetachEntity(vData.Vehicle);

            foreach (var x in vData.Vehicle.Occupants)
            {
                if (x is Player player)
                {
                    var pData = player.GetMainData();

                    if (pData != null)
                    {
                        player.WarpOutOfVehicle();

                        Sync.Vehicles.OnPlayerLeaveVehicle(pData, vData);
                    }
                }
                else
                {
                    x.ResetSharedData("VehicleSeat");
                }
            }

            if (vData.DeletionTimer is Timer deletionTimer)
                deletionTimer.Dispose();

            vData.Info.VehicleData = null;

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
            IndicatorsState = 0;

            TrunkLocked = true;
            HoodLocked = true;

            Radio = RadioStationTypes.Off;

            DirtLevel = 0;

            AttachedObjects = new List<AttachmentObjectNet>();
            AttachedEntities = new List<AttachmentEntityNet>();

            Vehicle.SetData(Service.AttachedObjectsTimersKey, new Dictionary<AttachmentType, Timer>());

            SetData(Vehicle, this);
        }

        public VehicleData(Vehicle Vehicle, VehicleInfo Info) : this(Vehicle)
        {
            this.Info = Info;

            TID = Info.TID;

            VID = Info.VID;

            Tuning.Apply(Vehicle);

            Numberplate?.Setup(this);

            var cont = Game.Items.Container.Get(TID);

            if (cont != null)
            {
                cont.UpdateOwner(Vehicle);
            }
        }

        public bool AttachBoatToTrailer()
        {
            if (Data.Type != Game.Data.Vehicles.Vehicle.Types.Boat)
                return false;

            return Vehicle.AttachObject(Game.Data.Vehicles.GetData("boattrailer").Model, AttachmentType.TrailerObjOnBoat, -1, null);
        }

        public bool DetachBoatFromTrailer()
        {
            if (Data.Type != Game.Data.Vehicles.Vehicle.Types.Boat)
                return false;

            return Vehicle.DetachObject(AttachmentType.TrailerObjOnBoat);
        }

        public bool IsBoatAttachedToTrailer()
        {
            if (Data.Type != Game.Data.Vehicles.Vehicle.Types.Boat)
                return false;

            return AttachedObjects.Where(x => x.Type == AttachmentType.TrailerObjOnBoat).Any();
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

                Game.Items.Container.Get(TID)?.Delete();

                if (OwnerType == OwnerTypes.Player)
                {
                    var pInfo = PlayerInfo.Get(OwnerID);

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
                    var owner = OwnerID > 0 ? PlayerInfo.Get(OwnerID) : null;

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

                    var owner = OwnerID > 0 ? PlayerInfo.Get(OwnerID) : null;

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

                    if (jobData is Game.Jobs.IVehicleRelated jobDataV)
                    {
                        jobDataV.OnVehicleRespawned(Info, owner);
                    }
                }
                else if (OwnerType == OwnerTypes.Fraction)
                {
                    Remove(this);
                }
                else if (OwnerType == OwnerTypes.PlayerDrivingSchool)
                {
                    var owner = OwnerID > 0 ? PlayerInfo.Get(OwnerID) : null;

                    OwnerID = 0;

                    if (owner != null)
                    {
                        if (owner.Quests.GetValueOrDefault(QuestType.DRSCHOOL0) is Quest quest)
                        {
                            quest.Cancel(owner, false);

                            if (owner.PlayerData != null)
                                owner.PlayerData.Player.Notify("DriveS::PEF0");
                        }
                    }

                    var numberplate = Vehicle.NumberPlate;
                    var numberplateS = Vehicle.NumberPlateStyle;

                    Remove(this);

                    Info.LastData.Fuel = Data.Tank;

                    var vData = Info.Spawn();

                    if (vData != null)
                    {
                        vData.Vehicle.NumberPlate = numberplate;
                        vData.Vehicle.NumberPlateStyle = numberplateS;
                    }
                }
            }
        }
        
        public static VehicleData New(PlayerData pData, Game.Data.Vehicles.Vehicle vType, Colour color1, Colour color2, Vector3 position, float heading, uint dimension, bool setInto = false)
        {
            var player = pData.Player;

            var vInfo = new VehicleInfo()
            {
                Data = vType,
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

            VehicleInfo.Add(vInfo);

            var vData = new VehicleData(vInfo.CreateVehicle(), vInfo);

            vInfo.VehicleData = vData;

            cont.UpdateOwner(vData.Vehicle);

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

        public static VehicleData NewTemp(PlayerData pData, Game.Data.Vehicles.Vehicle vType, Colour color1, Colour color2, Vector3 position, float heading, uint dimension)
        {
            var player = pData.Player;

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
                OwnerType = OwnerTypes.PlayerTemp,
                OwnerID = pData.CID,
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

                if (player?.Exists != true)
                    return;

                player.SetIntoVehicle(veh, 0);
            }, 1500);

            return vData;
        }

        public static VehicleData NewTemp(Game.Data.Vehicles.Vehicle vType, Colour color1, Colour color2, Vector3 position, float heading, uint dimension)
        {
            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
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

        public static VehicleData NewRent(PlayerData pData, Game.Data.Vehicles.Vehicle vType, Colour color1, Colour color2, Vector3 position, float heading, uint dimension)
        {
            var player = pData.Player;

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
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

        public static VehicleInfo NewJob(int jobId, string numberplateText, Game.Data.Vehicles.Vehicle vType, Colour color1, Colour color2, Vector4 position, uint dimension)
        {
            var job = Game.Jobs.Job.Get(jobId);

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
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

            if (job is Game.Jobs.IVehicleRelated vehJob)
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

        public static VehicleInfo NewAutoschool(int autoschoolId, Game.Data.Vehicles.Vehicle vType, Colour color1, Colour color2, Vector4 position, uint dimension)
        {
            var autoschool = DrivingSchool.Get(autoschoolId);

            var vInfo = new VehicleInfo()
            {
                VID = 0,

                Data = vType,
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
            else if (OwnerType == OwnerTypes.PlayerTemp || OwnerType == OwnerTypes.PlayerRent || OwnerType == OwnerTypes.PlayerRentJob || OwnerType == OwnerTypes.PlayerDrivingSchool)
            {
                if (OwnerID == pData.CID)
                    return true;
            }
            else if (OwnerType == OwnerTypes.Fraction)
            {
                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)OwnerID);

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
            if (!TrunkLocked && !Locked)
                return true;

            return CanManipulate(pData, notify);
        }
    }
}