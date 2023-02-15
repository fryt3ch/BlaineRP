using GTANetworkAPI;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players.Jobs
{
    public class Trucker : Script
    {
        [RemoteProc("Job::TR::TO")]
        private static bool TakeOrder(Player player, int orderId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var job = pData.CurrentJob as Game.Jobs.Trucker;

            if (job == null)
                return false;

            if (pData.VehicleSeat != 0)
                return false;

            var jobVehicle = player.Vehicle.GetMainData();

            if (jobVehicle == null || !job.Vehicles.Contains(jobVehicle))
                return false;

            if (job.ActiveOrders.Where(x => x.Value.CurrentVehicle == jobVehicle).Any())
                return false;

            var order = job.ActiveOrders.GetValueOrDefault(orderId);

            if (order == null)
                return false;

            if (order.CurrentVehicle != null)
            {
                // notify already taken

                return false;
            }

            job.SetOrderAsTaken(orderId, order, jobVehicle);

            return true;
        }

        [RemoteProc("Job::TR::NS")]
        private static bool NextStep(Player player, int orderId, byte nextStep)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (nextStep > 1)
                return false;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var job = pData.CurrentJob as Game.Jobs.Trucker;

            if (job == null)
                return false;

            var activeOrder = job.ActiveOrders.GetValueOrDefault(orderId);

            if (activeOrder == null)
                return false;

            if (pData.VehicleSeat != 0)
                return false;

            if (activeOrder.CurrentVehicle != player.Vehicle.GetMainData())
                return false;

            if (nextStep == 0)
            {
                if (activeOrder.GotMaterials)
                    return false;

                if (job.MaterialsPositions[activeOrder.MPIdx].DistanceTo(player.Position) > 10f)
                    return false;

                activeOrder.GotMaterials = true;
            }
            else
            {
                if (!activeOrder.GotMaterials)
                    return false;

                if (activeOrder.TargetBusiness.PositionInfo.DistanceTo(player.Position) > 10f)
                    return false;

                job.RemoveOrder(orderId);

                if (orderId >= 0)
                {
                    uint newMaterialsBalance;

                    if (activeOrder.TargetBusiness.Owner != null && activeOrder.TargetBusiness.TryAddMaterials(activeOrder.TargetBusiness.OrderedMaterials, out newMaterialsBalance, false))
                    {
                        activeOrder.TargetBusiness.SetMaterials(newMaterialsBalance);

                        MySQL.BusinessUpdateBalances(activeOrder.TargetBusiness);

                        // set ordered materials to 0
                    }
                }
                else
                {
                    job.TryAddRandomDefaultOrder();
                }
            }

            return true;
        }
    }
}
