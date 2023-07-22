using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Events.Players.Jobs
{
    public class Cabbie : Script
    {
        [RemoteProc("Job::CAB::TO")]
        private static byte CabbieTakeOrder(Player player, uint orderId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            var job = pData.CurrentJob as Game.Jobs.Cabbie;

            if (job == null)
                return 0;

            if (pData.VehicleSeat != 0)
                return 0;

            var jobVehicle = player.Vehicle.GetMainData();

            if (jobVehicle == null || jobVehicle.OwnerID != pData.CID || job != jobVehicle.Job)
                return 0;

            var order = Game.Jobs.Cabbie.ActiveOrders.GetValueOrDefault(orderId);

            if (order == null)
                return 1;

            if (Game.Jobs.Cabbie.ActiveOrders.Where(x => x.Value.CurrentWorker == pData.Info).Any())
                return 1;

            if (order.CurrentWorker != null)
                return 2;

            Game.Jobs.Cabbie.SetOrderAsTaken(orderId, order, pData);

            return byte.MaxValue;
        }

        [RemoteEvent("Job::CAB::OS")]
        private static void CabbieOrderSuccess(Player player, uint orderId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var job = pData.CurrentJob as Game.Jobs.Cabbie;

            if (job == null)
                return;

            if (pData.VehicleSeat != 0)
                return;

            var jobVehicle = player.Vehicle.GetMainData();

            if (jobVehicle == null || jobVehicle.OwnerID != pData.CID || job != jobVehicle.Job)
                return;

            var order = Game.Jobs.Cabbie.ActiveOrders.GetValueOrDefault(orderId);

            if (order == null || order.CurrentWorker != pData.Info)
                return;

            if (player.Position.DistanceTo(order.Position) > 15f)
                return;

            Game.Jobs.Cabbie.RemoveOrder(orderId, order, true);
        }
    }
}
