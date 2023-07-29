using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.EntitiesData.Vehicles.Static;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class GasStation
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("GasStation::Enter")]
            public static object GasStationEnter(Player player, GTANetworkAPI.Vehicle vehicle, int id)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                PlayerData pData = sRes.Data;

                if (pData.CurrentBusiness != null)
                    return null;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var gs = Get(id) as GasStation;

                if (gs == null)
                    return null;

                if (!gs.IsPlayerNearGasolinesPosition(pData))
                    return null;

                VehicleData vData = vehicle.GetMainData();

                if (vData == null)
                    return null;

                if (vData.FuelLevel == vData.Data.Tank)
                {
                    player.Notify(vData.Data.FuelType == EntitiesData.Vehicles.Static.Vehicle.FuelTypes.Petrol ? "Vehicle::FOFP" : "Vehicle::FOFE");

                    return null;
                }

                //player.CloseAll(true);

                pData.CurrentBusiness = gs;

                return (float)gs.Margin;
            }

            [RemoteEvent("GasStation::Exit")]
            public static void GasStationExit(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.CurrentBusiness == null)
                    return;

                pData.CurrentBusiness = null;
            }
        }
    }
}