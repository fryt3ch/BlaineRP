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

        private static void Remove(VehicleData vData)
        {
            if (vData == null)
                return;

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

            Radio = 255;

            DirtLevel = 0;

            AttachedObjects = new List<AttachSystem.AttachmentObjectNet>();
            AttachedEntities = new List<AttachSystem.AttachmentEntityNet>();

            Vehicle.SetData(Sync.AttachSystem.AttachedObjectsCancelsKey, new Dictionary<Sync.AttachSystem.Types, CancellationTokenSource>());

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
                if (VID > 0)
                {
                    if (IsDead)
                    {
                        Info.IsOnVehiclePound = true;
                    }
                    else
                    {
                        LastData.Position = Vehicle.Position;
                        LastData.Heading = Vehicle.Heading;
                        LastData.Dimension = Vehicle.Dimension;
                    }

                    if (Info != null)
                        MySQL.VehicleDeletionUpdate(Info);
                }
                else
                {
                    if (OwnerType == OwnerTypes.PlayerRent)
                    {
                        var oPData = PlayerData.All.Values.Where(x => x.CID == OwnerID).FirstOrDefault();

                        if (oPData != null)
                        {
                            oPData.RemoveRentedVehicle(this);
                        }
                    }
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

            Game.Items.Container cont = vType.TrunkData == null ? null : Game.Items.Container.Create($"vt_{vType.ID}", null);

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
                LastData = new LastVehicleData() { Position = position, Dimension = dimension, Heading = heading, Fuel = vType.Tank, Mileage = 0f, GarageSlot = -1 },
                RegistrationDate = Utils.GetCurrentTime(),
            };

            var vData = new VehicleData(vInfo.CreateVehicle(), vInfo);

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

        public static VehicleData NewJob(int jobId, Game.Data.Vehicles.Vehicle vType, Utils.Colour color1, Utils.Colour color2, Utils.Vector4 position, uint dimension)
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

            var veh = vData.Vehicle;

            if (job is Game.Jobs.IVehicles vehJob)
            {
                veh.NumberPlate = $"{vehJob.NumberplateText}{vehJob.Vehicles.Count + 1}";
            }

            NAPI.Task.Run(() =>
            {
                if (veh?.Exists != true)
                    return;

                veh.Dimension = dimension;
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
                if (Vehicle.GetData<PlayerData>(Game.Jobs.Job.RentedVehicleOwnerKey) == pData)
                    return true;
            }

            if (notify)
            {
                pData.Player.Notify("Vehicle::NotAllowed");
            }

            return false;
        }

        #endregion
    }
}