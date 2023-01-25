using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer
{
    public partial class VehicleData
    {
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

            public VehicleData VehicleData { get; set; }

            public Game.Data.Vehicles.Vehicle Data { get; set; }

            public PlayerData.PlayerInfo FullOwnerPlayer => OwnerType == OwnerTypes.Player ? PlayerData.PlayerInfo.Get(OwnerID) : null;

            public uint VID { get; set; }

            public string ID { get; set; }

            public OwnerTypes OwnerType { get; set; }

            public uint OwnerID { get; set; }

            public List<uint> AllKeys { get; set; }

            public DateTime RegistrationDate { get; set; }

            public uint OwnersCount { get; set; }

            public uint? TID { get; set; }

            public Game.Items.Numberplate Numberplate { get; set; }

            public Game.Data.Vehicles.Tuning Tuning { get; set; }

            public LastVehicleData LastData { get; set; }

            /// <summary>На штрафстоянке ли транспорт?</summary>
            public bool IsOnVehiclePound { get => LastData.GarageSlot == -2; set => LastData.GarageSlot = -2; }

            public VehicleInfo() { }

            public Vehicle CreateVehicle()
            {
                var data = Game.Data.Vehicles.All[ID];

                var veh = NAPI.Vehicle.CreateVehicle(data.Model, LastData.Position, LastData.Heading, 0, 0, "", 255, false, false, Utils.Dimensions.Stuff);

                return veh;
            }

            public VehicleData Spawn()
            {
                if (VehicleData == null)
                {
                    if (IsOnVehiclePound)
                        return null;

                    var owner = FullOwnerPlayer;

                    var freeGarageSlots = owner.TotalFreeGarageSlots;

                    if (LastData.Dimension != Utils.Dimensions.Main && LastData.GarageSlot >= 0 && freeGarageSlots > 0)
                    {
                        var hId = Utils.GetHouseIdByDimension(LastData.Dimension);

                        var house = hId == 0 ? null : Game.Estates.House.Get(hId);

                        if (house == null || house.Owner != owner)
                        {
                            var gId = Utils.GetGarageIdByDimension(LastData.Dimension);

                            var garage = hId == 0 ? null : Game.Estates.Garage.Get(hId);

                            if (garage == null || garage.Owner != owner)
                            {
                                IsOnVehiclePound = true;
                            }
                            else
                            {
                                VehicleData = new VehicleData(CreateVehicle(), this);

                                garage.SetVehicleToGarageOnSpawn(VehicleData);
                            }
                        }
                        else
                        {
                            VehicleData = new VehicleData(CreateVehicle(), this);

                            house.SetVehicleToGarageOnSpawn(VehicleData);
                        }
                    }
                    else
                    {
                        if (freeGarageSlots <= 0)
                        {
                            IsOnVehiclePound = true;
                        }
                        else
                        {
                            if (LastData.Dimension != Utils.Dimensions.Main)
                                LastData.Dimension = Utils.Dimensions.Main;

                            VehicleData = new VehicleData(CreateVehicle(), this);
                        }
                    }

                    if (VehicleData != null)
                    {
                        VehicleData.AttachBoatToTrailer();

                        NAPI.Task.Run(() =>
                        {
                            if (VehicleData?.Vehicle?.Exists != true)
                                return;

                            VehicleData.Vehicle.Dimension = VehicleData.LastData.Dimension;
                        }, 1500);
                    }

                    return VehicleData;
                }
                else
                {
                    VehicleData.CancelDeletionTask();

                    return VehicleData;
                }
            }

            public void ShowPassport(Player player)
            {
                var owner = PlayerData.PlayerInfo.Get(OwnerID);

                if (owner == null)
                    player.TriggerEvent("Documents::Show", 2, Data.Name, "null", "null", VID, OwnersCount, Numberplate?.Tag, RegistrationDate.SerializeToJson());
                else
                    player.TriggerEvent("Documents::Show", 2, Data.Name, owner.Name, owner.Surname, VID, OwnersCount, Numberplate?.Tag, RegistrationDate.SerializeToJson());
            }

            public static void UpdateLastDataOnWrongDimension(PlayerData.PlayerInfo pInfo)
            {
                foreach (var x in pInfo.OwnedHouses)
                {

                }
            }
        }
    }
}
