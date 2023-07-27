using GTANetworkAPI;
using System.Linq;
using BlaineRP.Server.EntitiesData.Vehicles;

namespace BlaineRP.Server.Events.Vehicles
{
    internal class Misc : Script
    {
        [RemoteEvent("Vehicles::LOWNV")]
        private static void LocateOwnedVehicle(Player player, uint vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vInfo = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

            if (vInfo == null)
                return;

            Sync.Vehicles.TryLocateOwnedVehicle(pData, vInfo);
        }

        [RemoteEvent("Vehicles::LRENV")]
        private static void LocateRentedVehicle(Player player, ushort rid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vData = VehicleData.All.Values.Where(x => x.Vehicle.Id == rid).FirstOrDefault();

            if (vData == null)
                return;

            if (vData.OwnerID != pData.CID)
                return;

            Sync.Vehicles.TryLocateRentedVehicle(pData, vData);
        }

        [RemoteEvent("Vehicles::EVAOWNV")]
        private static void EvacuateOwnedVehicle(Player player, uint vid, bool toHouse, uint pId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vInfo = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

            if (vInfo == null)
                return;

            if (vInfo.VehicleData != null && player.Vehicle == vInfo.VehicleData.Vehicle)
                return;

            if (vInfo.LastData.GarageSlot >= 0)
            {
                player.Notify("Vehicle::OIG");

                return;
            }

            if (!pData.HasBankAccount(true))
                return;

            ulong newBalance;

            if (!pData.BankAccount.TryRemoveMoneyDebit(Properties.Settings.Static.VEHICLE_EVACUATION_COST, out newBalance, true))
                return;

            if (toHouse)
            {
                var house = pData.OwnedHouses.Where(x => x.Id == pId).FirstOrDefault();

                if (house == null)
                    return;

                var garageData = house.GarageData;

                if (garageData == null)
                    return;

                var garageVehs = house.GetVehiclesInGarage();

                var freeSlots = Enumerable.Range(0, garageData.MaxVehicles).ToList();

                if (garageVehs.Count == freeSlots.Count)
                {
                    player.Notify("Garage::NVP");

                    return;
                }

                foreach (var x in garageVehs)
                {
                    freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
                }

                var garageSlot = freeSlots.First();

                if (vInfo.VehicleData != null)
                    VehicleData.Remove(vInfo.VehicleData);

                house.SetVehicleToGarageOnlyData(vInfo, garageSlot);
            }
            else
            {
                var garage = pData.OwnedGarages.Where(x => x.Id == pId).FirstOrDefault();

                if (garage == null)
                    return;

                var freeSlots = Enumerable.Range(0, garage.StyleData.MaxVehicles).ToList();

                var garageVehs = garage.GetVehiclesInGarage();

                if (garageVehs.Count == freeSlots.Count)
                {
                    player.Notify("Garage::NVP");

                    return;
                }

                foreach (var x in garageVehs)
                {
                    freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
                }

                var garageSlot = freeSlots.First();

                if (vInfo.VehicleData != null)
                    VehicleData.Remove(vInfo.VehicleData);

                garage.SetVehicleToGarageOnlyData(vInfo, garageSlot);
            }

            pData.BankAccount.SetDebitBalance(newBalance, null);

            vInfo.Spawn();
        }
    }
}
