using GTANetworkAPI;
using Mysqlx.Crud;
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
        private static byte TakeOrder(Player player, uint orderId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            var job = pData.CurrentJob as Game.Jobs.Trucker;

            if (job == null)
                return 0;

            if (pData.VehicleSeat != 0)
                return 0;

            var jobVehicle = player.Vehicle.GetMainData();

            if (jobVehicle == null || !job.Vehicles.Contains(jobVehicle))
                return 0;

            var order = job.ActiveOrders.GetValueOrDefault(orderId);

            if (order == null)
                return 1;

            if (job.ActiveOrders.Where(x => x.Value.CurrentVehicle == jobVehicle).Any())
                return 1;

            if (order.CurrentVehicle != null)
            {
                // notify already taken

                return 2;
            }

            job.SetOrderAsTaken(orderId, order, jobVehicle);

            pData.Info.Quests.GetValueOrDefault(Sync.Quest.QuestData.Types.JTR1)?.UpdateStep(pData.Info, 1, $"{orderId}&{order.MPIdx}&{order.TargetBusiness.ID}&{order.Reward}");

            return byte.MaxValue;
        }
    }
}
