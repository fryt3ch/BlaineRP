using GTANetworkAPI;
using System.Linq;
using BlaineRP.Server.EntitiesData.Vehicles;

namespace BlaineRP.Server.Events.Vehicles
{
    internal class Rent : Script
    {
        [RemoteEvent("VRent::Cancel")]
        private static void VehicleRentCancel(Player player, ushort vId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var vData = VehicleData.All.Values.Where(x => x.Vehicle.Id == vId).FirstOrDefault();

            if (vData == null)
                return;

            if (player.Vehicle == vData.Vehicle)
                return;

            if (vData.OwnerID != pData.CID)
                return;

            if (vData.OwnerType != OwnerTypes.PlayerRent && vData.OwnerType != OwnerTypes.PlayerRentJob)
                return;

            vData.Delete(false);
        }

        [RemoteProc("Vehicles::JVRS")]
        private static byte JobVehicleRentStart(Player player, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            var vData = player.Vehicle?.GetMainData();

            if (vData == null || vData.OwnerType != OwnerTypes.PlayerRentJob)
                return 0;

            if (pData.VehicleSeat != 0)
                return 0;

            var jobData = vData.Job;
            var jobDataV = jobData as Game.Jobs.IVehicleRelated;

            if (jobData == null || jobDataV == null)
                return 0;

            if (pData.HasJob(false))
            {
                if (jobData.Type == Game.Jobs.JobType.Farmer)
                {
                    if (pData.CurrentJob != jobData)
                    {
                        player.Notify("Job::AHJ");

                        return 0;
                    }
                }
                else
                {
                    player.Notify("Job::AHJ");

                    return 0;
                }
            }

            if (!jobData.CanPlayerDoThisJob(pData))
                return 0;

            if (vData.OwnerID != 0)
                return 1;

            if (vData.OwnerID == pData.CID)
                return 0;

            if (pData.RentedJobVehicle != null)
                return 2;

            ulong newBalance;

            if (useCash)
            {
                if (!pData.TryRemoveCash(jobDataV.VehicleRentPrice, out newBalance, true))
                    return 3;

                pData.SetCash(newBalance);
            }
            else
            {
                if (!pData.HasBankAccount(true) || !pData.BankAccount.TryRemoveMoneyDebit(jobDataV.VehicleRentPrice, out newBalance, true))
                    return 3;

                pData.BankAccount.SetDebitBalance(newBalance, null);
            }

            vData.OwnerID = pData.CID;

            pData.AddRentedVehicle(vData, 600_000);

            if (pData.CurrentJob == null)
                jobData.SetPlayerJob(pData, vData);

            if (jobData is Game.Jobs.Farmer farmerJob)
            {
                if (vData.Data == Game.Jobs.Farmer.TractorVehicleData)
                    farmerJob.SetPlayerAsTractorTaker(pData, vData);
                else if (vData.Data == Game.Jobs.Farmer.PlaneVehicleData)
                    farmerJob.SetPlayerAsPlaneIrrigator(pData, vData);
            }

            return byte.MaxValue;
        }
    }
}
