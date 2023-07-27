using GTANetworkAPI;
using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Vehicles;

namespace BlaineRP.Server.Game.Misc
{
    public partial class VehicleDestruction
    {
        public static List<VehicleDestruction> All { get; private set; }

        public int Id => All.IndexOf(this);

        public static VehicleDestruction Get(int id) => id < 0 || id >= All.Count ? null : All[id];

        public Vector3 Position { get; set; }

        public VehicleDestruction(Vector3 Position)
        {
            this.Position = Position;
        }

        public ulong GetPriceForVehicle(VehicleInfo vInfo)
        {
            return vInfo.Data.GovPrice / 2;
        }
    }
}
