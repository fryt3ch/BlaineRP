using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.EntitiesData.Vehicles
{
    public class VehicleInfo
    {
            public static Dictionary<uint, VehicleInfo> All { get; private set; } = new Dictionary<uint, VehicleInfo>();

            public static UtilsT.UidHandlers.UInt32 UidHandler { get; private set; } = new UtilsT.UidHandlers.UInt32(Properties.Settings.Profile.Current.Game.VIDBaseOffset);

            public static void AddOnLoad(VehicleInfo vInfo)
            {
                if (vInfo == null)
                    return;

                UidHandler.TryUpdateLastAddedMaxUid(vInfo.VID);

                All.Add(vInfo.VID, vInfo);
            }

            public static void Add(VehicleInfo vInfo)
            {
                if (vInfo == null)
                    return;

                vInfo.VID = UidHandler.MoveNextUid();

                All.Add(vInfo.VID, vInfo);

                MySQL.VehicleAdd(vInfo);
            }

            public static void Remove(VehicleInfo vInfo)
            {
                if (vInfo == null)
                    return;

                var vid = vInfo.VID;

                UidHandler.SetUidAsFree(vid);

                All.Remove(vid);

                if (vid > 0)
                    MySQL.VehicleDelete(vInfo);
            }

            public static VehicleInfo Get(uint id) => All.GetValueOrDefault(id);

            public static List<VehicleInfo> GetAllByCID(uint cid) => All.Values.Where(x => x != null && (x.OwnerType == OwnerTypes.Player && x.OwnerID == cid)).ToList();

            public VehicleData VehicleData { get; set; }

            public Game.Data.Vehicles.Vehicle Data { get; set; }

            public PlayerInfo FullOwnerPlayer => OwnerType == OwnerTypes.Player ? PlayerInfo.Get(OwnerID) : null;

            public uint VID { get; set; }

            public string ID { get; set; }

            public OwnerTypes OwnerType { get; set; }

            public uint OwnerID { get; set; }

            public Guid KeysUid { get; set; }

            public DateTime RegistrationDate { get; set; }

            public uint OwnersCount { get; set; }

            public uint TID { get; set; }

            public Game.Items.Numberplate Numberplate { get; set; }

            public Game.Data.Vehicles.Tuning Tuning { get; set; }

            public LastVehicleData LastData { get; set; }

            public string RegisteredNumberplate { get; set; }

            /// <summary>На штрафстоянке ли транспорт?</summary>
            public bool IsOnVehiclePound { get => LastData.GarageSlot == -2; set => LastData.GarageSlot = -2; }

            public VehicleInfo() { }

            public Vehicle CreateVehicle()
            {
                var data = Game.Data.Vehicles.All[ID];

                var veh = NAPI.Vehicle.CreateVehicle(data.Model, LastData.Position, LastData.Heading, 0, 0, "", 255, false, false, Properties.Settings.Static.StuffDimension);

                return veh;
            }

            public VehicleData Spawn()
            {
                if (VehicleData != null)
                {
                    VehicleData.CancelDeletionTask();

                    return VehicleData;
                }

                if (OwnerType == OwnerTypes.Player)
                {
                    if (IsOnVehiclePound)
                        return null;

                    var owner = FullOwnerPlayer;

                    var freeGarageSlots = owner.PlayerData.FreeVehicleSlots - owner.OwnedVehicles.Where(x => x.VehicleData != null).Count() + owner.OwnedVehicles.Count;

                    if (LastData.Dimension != Properties.Settings.Static.MainDimension && LastData.GarageSlot >= 0 && freeGarageSlots > 0)
                    {
                        var hId = Utils.GetHouseIdByDimension(LastData.Dimension);

                        var house = hId == 0 ? null : Game.Estates.House.Get(hId);

                        if (house == null)
                        {
                            var gId = Utils.GetGarageIdByDimension(LastData.Dimension);

                            var garage = gId == 0 ? null : Game.Estates.Garage.Get(gId);

                            if (garage == null || garage.Owner != owner || garage.GetVehiclesInGarage().Where(x => x.LastData.GarageSlot == LastData.GarageSlot && x.VID != VID).Any())
                            {
                                IsOnVehiclePound = true;
                            }
                            else
                            {
                                VehicleData = new VehicleData(CreateVehicle(), this);

                                garage.SetVehicleToGarageOnSpawn(VehicleData);
                            }
                        }
                        else if (house.Owner == FullOwnerPlayer && !house.GetVehiclesInGarage().Where(x => x.LastData.GarageSlot == LastData.GarageSlot && x.VID != VID).Any())
                        {
                            VehicleData = new VehicleData(CreateVehicle(), this);

                            house.SetVehicleToGarageOnSpawn(VehicleData);
                        }
                        else
                        {
                            IsOnVehiclePound = true;
                        }
                    }
                    else
                    {
                        if (freeGarageSlots <= 0 && LastData.GarageSlot != int.MinValue)
                        {
                            IsOnVehiclePound = true;
                        }
                        else
                        {
                            if (LastData.Dimension != Properties.Settings.Static.MainDimension)
                                LastData.Dimension = Properties.Settings.Static.MainDimension;

                            if (LastData.GarageSlot != -1)
                                LastData.GarageSlot = -1;

                            VehicleData = new VehicleData(CreateVehicle(), this);
                        }
                    }

                    if (VehicleData != null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            if (VehicleData?.Vehicle?.Exists != true)
                                return;

                            VehicleData.Vehicle.Dimension = VehicleData.LastData.Dimension;
                        }, 1500);
                    }
                }
                else if (OwnerType == OwnerTypes.Fraction || OwnerType == OwnerTypes.PlayerRentJob || OwnerType == OwnerTypes.PlayerRent || OwnerType == OwnerTypes.PlayerDrivingSchool)
                {
                    VehicleData = new VehicleData(CreateVehicle(), this);

                    NAPI.Task.Run(() =>
                    {
                        if (VehicleData?.Vehicle?.Exists != true)
                            return;

                        VehicleData.Vehicle.Dimension = VehicleData.LastData.Dimension;
                    }, 1500);
                }

                return VehicleData;
            }

            public void ShowPassport(Player player)
            {
                var owner = PlayerInfo.Get(OwnerID);

                if (owner == null)
                    player.TriggerEvent("Documents::Show", 2, Data.Name, "null", "null", VID, OwnersCount, RegisteredNumberplate, RegistrationDate.SerializeToJson());
                else
                    player.TriggerEvent("Documents::Show", 2, Data.Name, owner.Name, owner.Surname, VID, OwnersCount, RegisteredNumberplate, RegistrationDate.SerializeToJson());
            }

            public void SetToVehiclePound()
            {
                if (IsOnVehiclePound)
                    return;

                IsOnVehiclePound = true;

                if (VehicleData != null)
                {
                    VehicleData.Delete(false);
                }
                else
                {
                    MySQL.VehicleDeletionUpdate(this);
                }
            }

            public void ChangeOwner(PlayerInfo newOwner)
            {
                if (FullOwnerPlayer is PlayerInfo oInfo)
                {
                    OwnerID = newOwner.CID;

                    if (oInfo.PlayerData != null)
                    {
                        oInfo.PlayerData.RemoveVehicleProperty(this);
                    }

                    if (newOwner.PlayerData != null)
                    {
                        newOwner.PlayerData.AddVehicleProperty(this);
                    }

                    if (LastData.GarageSlot >= 0)
                    {
                        SetToVehiclePound();
                    }

                    MySQL.VehicleOwnerUpdate(this);
                }
            }
    }
}
