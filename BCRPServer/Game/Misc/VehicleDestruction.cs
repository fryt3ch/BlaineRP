using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game.Misc
{
    public class VehicleDestruction
    {
        public static List<VehicleDestruction> All { get; private set; }

        public int Id => All.IndexOf(this);

        public static VehicleDestruction Get(int id) => id < 0 || id >= All.Count ? null : All[id];

        public Vector3 Position { get; set; }

        public static void InitializeAll()
        {
            All = new List<VehicleDestruction>()
            {
                 new VehicleDestruction(new Vector3(2400.494f, 3108.046f, 47.17492f)),
                 new VehicleDestruction(new Vector3(-458.2042f, -1715.724f, 17.6409f)),
                 new VehicleDestruction(new Vector3(1284.828f, -2560.101f, 43.04144f)),
            };

            var lines = new List<string>();

            foreach (var x in All)
            {
                lines.Add($"new VehicleDestruction({x.Id}, {x.Position.ToCSharpStr()});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetLocationsLoaderPath, "VEHICLEDESTR_TO_REPLACE", lines);
        }

        public VehicleDestruction(Vector3 Position)
        {
            this.Position = Position; 
        }

        public ulong GetPriceForVehicle(VehicleData.VehicleInfo vInfo)
        {
            return vInfo.Data.GovPrice / 2;
        }
    }
}
