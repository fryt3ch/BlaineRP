using GTANetworkAPI;
using System.Collections.Generic;

namespace BCRPServer.Sync
{
    public class Vehicles
    {
        public class VehicleSpawnData
        {
            public List<Utils.Vector4> SpawnPositions { get; set; }

            public int LastSpawnUsed { get; set; }

            public VehicleSpawnData(List<Utils.Vector4> spawnPositions)
            {
                SpawnPositions = spawnPositions;
            }

            public Utils.Vector4 GetNextVehicleSpawnPosition()
            {
                var idx = LastSpawnUsed + 1;

                if (idx >= SpawnPositions.Count)
                    idx = 0;

                LastSpawnUsed = idx;

                return SpawnPositions[idx];
            }
        }

        private static Dictionary<string, VehicleSpawnData> VehiclePoundsData = new Dictionary<string, VehicleSpawnData>()
        {
            {
                "vpound_w_0", new VehicleSpawnData(new List<Utils.Vector4>()
                {
                    new Utils.Vector4(496.2596f, -60.01876f, 77.08006f, 151.5f),
                    new Utils.Vector4(472.8677f, -61.93843f, 77.08006f, 151.5f),
                    new Utils.Vector4(476.3974f, -63.99604f, 77.08006f, 151.5f),

                    new Utils.Vector4(463.2267f, -70.33319f, 77.08006f, 337.8f),
                    new Utils.Vector4(466.5417f, -71.52922f, 77.08006f, 339.2f),
                    new Utils.Vector4(469.9377f, -73.03962f, 77.08006f, 339.9f),
                })
            },
        };

        private static Dictionary<string, VehicleSpawnData> VehicleRentSData = new Dictionary<string, VehicleSpawnData>()
        {
            {
                "vrent_s_0", new VehicleSpawnData(new List<Utils.Vector4>()
                {
                    new Utils.Vector4(-723.2322f, 5822.186f, 16.72265f, 178.3725f),
                })
            },
        };

        public static VehicleSpawnData GetVehiclePoundData(string npcId) => VehiclePoundsData.GetValueOrDefault(npcId);

        public static VehicleSpawnData GetVehicleRentSData(string npcId) => VehicleRentSData.GetValueOrDefault(npcId);

        public static void SetFixed(Vehicle veh)
        {
            if (veh.Controller is Player controller)
            {
                controller.TriggerEvent("Vehicles::Fix", veh);
            }
            else
            {
                veh.Repair();
            }
        }

        public static void SetVisualFixed(Vehicle veh)
        {
            if (veh.Controller is Player controller)
            {
                controller.TriggerEvent("Vehicles::FixV", veh);
            }
        }

        public static bool TryLocateOwnedVehicle(PlayerData pData, VehicleData.VehicleInfo vInfo)
        {
            if (vInfo.IsOnVehiclePound)
            {
                pData.Player.Notify("Vehicle::OVP");

                return false;
            }
            else if (vInfo.VehicleData?.Vehicle?.Exists != true)
            {
                return false;
            }
            else if (vInfo.VehicleData.Vehicle.Dimension != pData.Player.Dimension)
            {
                if (pData.Player.Dimension != Utils.Dimensions.Main)
                {
                    pData.Player.Notify("Vehicle::KENS");

                    return false;
                }
                else if (vInfo.LastData.GarageSlot >= 0)
                {
                    var hId = Utils.GetHouseIdByDimension(vInfo.LastData.Dimension);

                    var house = hId == 0 ? null : Game.Estates.House.Get(hId);

                    if (house == null)
                    {
                        hId = Utils.GetGarageIdByDimension(vInfo.LastData.Dimension);

                        var garage = hId == 0 ? null : Game.Estates.Garage.Get(hId);

                        if (garage == null)
                        {
                            return false;
                        }
                        else
                        {
                            pData.Player.CreateGPSBlip(garage.Root.EnterPosition.Position, pData.Player.Dimension, true);
                        }
                    }
                    else
                    {
                        pData.Player.CreateGPSBlip(house.PositionParams.Position, pData.Player.Dimension, true);
                    }
                }

            }
            else
            {
                pData.Player.CreateGPSBlip(vInfo.VehicleData.Vehicle.Position, pData.Player.Dimension, true);
            }

            return true;
        }

        public static bool TryLocateRentedVehicle(PlayerData pData, VehicleData vData)
        {
            if (vData.Vehicle.Dimension != pData.Player.Dimension)
            {
                if (pData.Player.Dimension != Utils.Dimensions.Main)
                {
                    pData.Player.Notify("Vehicle::KENS");

                    return false;
                }
            }
            else
            {
                pData.Player.CreateGPSBlip(vData.Vehicle.Position, pData.Player.Dimension, true);
            }

            return true;
        }

        public static void OnPlayerLeaveVehicle(PlayerData pData, VehicleData vData)
        {
            var curSeat = pData.VehicleSeat;

            if (curSeat < 0)
                return;

            if (vData.ForcedSpeed != 0f && curSeat == 0)
                vData.ForcedSpeed = 0f;

            pData.VehicleSeat = -1;

            if (vData.OwnerID == pData.CID && (vData.OwnerType == VehicleData.OwnerTypes.PlayerRent || vData.OwnerType == VehicleData.OwnerTypes.PlayerRentJob))
            {
                vData.StartDeletionTask(Settings.RENTED_VEHICLE_TIME_TO_AUTODELETE);
            }
        }
    }
}
