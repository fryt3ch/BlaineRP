using System.Collections.Generic;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;

namespace BlaineRP.Server.Game.Jobs
{
    public interface IVehicleRelated
    {
        public List<VehicleInfo> Vehicles { get; set; }

        public uint VehicleRentPrice { get; set; }

        public void OnVehicleRespawned(VehicleInfo vInfo, PlayerInfo pInfo);
    }
}