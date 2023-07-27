using System.Collections.Generic;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;

namespace BlaineRP.Server.Game.Jobs
{
    public interface IVehicleRelated
    {
        public List<VehicleInfo> Vehicles { get; set; }

        public uint VehicleRentPrice { get; set; }

        public void OnVehicleRespawned(VehicleInfo vInfo, PlayerInfo pInfo);
    }
}