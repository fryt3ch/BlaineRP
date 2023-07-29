using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Jobs
{
    public partial class Cabbie
    {
        public class RemoteEvents : Script
        {
            [RemoteProc("Job::CAB::TO")]
            private static byte CabbieTakeOrder(Player player, uint orderId)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                PlayerData pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return 0;

                var job = pData.CurrentJob as Cabbie;

                if (job == null)
                    return 0;

                if (pData.VehicleSeat != 0)
                    return 0;

                VehicleData jobVehicle = player.Vehicle.GetMainData();

                if (jobVehicle == null || jobVehicle.OwnerID != pData.CID || job != jobVehicle.Job)
                    return 0;

                OrderInfo order = ActiveOrders.GetValueOrDefault(orderId);

                if (order == null)
                    return 1;

                if (ActiveOrders.Where(x => x.Value.CurrentWorker == pData.Info).Any())
                    return 1;

                if (order.CurrentWorker != null)
                    return 2;

                SetOrderAsTaken(orderId, order, pData);

                return byte.MaxValue;
            }

            [RemoteEvent("Job::CAB::OS")]
            private static void CabbieOrderSuccess(Player player, uint orderId)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                var job = pData.CurrentJob as Cabbie;

                if (job == null)
                    return;

                if (pData.VehicleSeat != 0)
                    return;

                VehicleData jobVehicle = player.Vehicle.GetMainData();

                if (jobVehicle == null || jobVehicle.OwnerID != pData.CID || job != jobVehicle.Job)
                    return;

                OrderInfo order = ActiveOrders.GetValueOrDefault(orderId);

                if (order == null || order.CurrentWorker != pData.Info)
                    return;

                if (player.Position.DistanceTo(order.Position) > 15f)
                    return;

                RemoveOrder(orderId, order, true);
            }

            [RemoteProc("Taxi::NO")]
            private static bool TaxiNewOrder(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return false;

                if (ActiveOrders.Where(x => x.Value.Entity == player).Any())
                    return false;

                if (pData.CurrentJob?.Type == JobType.Cabbie)
                    return false;

                AddPlayerOrder(pData);

                return true;
            }

            [RemoteEvent("Taxi::CO")]
            private static void TaxiCancelOrder(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                KeyValuePair<uint, OrderInfo> curOrderPair = ActiveOrders.Where(x => x.Value.Entity == player).FirstOrDefault();

                if (curOrderPair.Value == null)
                    return;

                RemoveOrder(curOrderPair.Key, curOrderPair.Value, false);
            }
        }
    }
}