﻿using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync
{
    public class Vehicles
    {
        public class VehiclePoundData
        {
            public List<Utils.Vector4> SpawnPositions { get; set; }

            public int LastSpawnUsed { get; set; }

            public VehiclePoundData(List<Utils.Vector4> spawnPositions)
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

        private static Dictionary<string, VehiclePoundData> VehiclePoundsData = new Dictionary<string, VehiclePoundData>()
        {
            {
                "vpound_w_0", new VehiclePoundData(new List<Utils.Vector4>()
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

        public static VehiclePoundData GetVehiclePoundData(string npcId) => VehiclePoundsData.GetValueOrDefault(npcId);

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
    }
}
