using GTANetworkAPI;
using System;
using System.Linq;

namespace BCRPServer.Events.Players
{
    class Garages : Script
    {
        [RemoteProc("Garage::ToggleLock")]
        public static bool GarageToggleLock(Player player, uint id, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            var garage = Game.Estates.Garage.Get(id);

            if (garage == null)
                return false;

            if (garage.Owner != pData.Info)
                return false;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            if (!garage.Root.IsEntityNearEnter(player))
                return false;

            if (garage.IsLocked == state)
                return false;

            garage.IsLocked = state;

            return true;
        }

        [RemoteProc("Garage::GetIsLocked")]
        public static bool? GarageGetIsLocked(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            var garage = Game.Estates.Garage.Get(id);

            if (garage == null)
                return null;

            if (!garage.Root.IsEntityNearEnter(player))
                return null;

            return garage.IsLocked;
        }

        [RemoteProc("Garage::BuyGov")]
        public static bool GarageBuyGov(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var garage = Game.Estates.Garage.Get(id);

            if (garage == null)
                return false;

            if (garage.Owner != null)
            {
                player.Notify("House::AB");

                return false;
            }

            if (!garage.Root.IsEntityNearEnter(player))
                return false;

            return garage.BuyFromGov(pData);
        }

        [RemoteProc("Garage::STG")]
        public static bool GarageSellGov(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var garage = Game.Estates.Garage.Get(id);

            if (garage == null)
                return false;

            if (garage.Owner != pData.Info)
                return false;

            if (!garage.Root.IsEntityNearEnter(player))
                return false;

            garage.SellToGov(true, true);

            return true;
        }

        [RemoteEvent("Garage::Enter")]
        public static void GarageEnter(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            if (player.Vehicle != null)
                return;

            var garage = Game.Estates.Garage.Get(id);

            if (garage == null)
                return;

            if (!garage.Root.IsEntityNearEnter(player))
                return;

            player.Teleport(garage.StyleData.EnterPosition.Position, false, garage.Dimension, garage.StyleData.EnterPosition.RotationZ, true);

            player.TriggerEvent("Garage::Enter", id);
        }

        [RemoteEvent("Garage::Exit")]
        public static void GarageExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (player.Vehicle != null)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var garage = pData.CurrentGarage;

            if (garage == null)
                return;

            player.Teleport(garage.Root.EnterPosition.Position, false, Properties.Settings.Profile.Current.Game.MainDimension, garage.Root.EnterPosition.RotationZ, true);

            player.TriggerEvent("Garage::Exit");
        }

        [RemoteEvent("Garage::SlotsMenu")]
        private static void GarageSlotsMenu(Player player, Vehicle veh, uint gRootId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            var gRoot = Game.Estates.Garage.GarageRoot.Get(gRootId);

            if (gRoot == null)
                return;

            if (player.Dimension != Properties.Settings.Profile.Current.Game.MainDimension || !vData.IsFullOwner(pData))
                return;

            if (!player.AreEntitiesNearby(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var garage = pData.OwnedGarages.Where(x => x.Root == gRoot).FirstOrDefault();

            if (garage == null)
                return;

            var freeSlots = Enumerable.Range(0, garage.StyleData.MaxVehicles).ToList();

            var garageVehs = garage.GetVehiclesInGarage();

            if (freeSlots.Count == garageVehs.Count)
            {
                player.Notify("Garage::NVP");

                return;
            }

            foreach (var x in garageVehs)
            {
                freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
            }

            if (freeSlots.Count == 1)
            {
                garage.SetVehicleToGarage(vData, freeSlots[0]);
            }
            else
            {
                player.TriggerEvent("Vehicles::Garage::SlotsMenu", freeSlots);
            }
        }

        [RemoteEvent("Garage::Vehicle")]
        private static void GarageVehicle(Player player, int slot, Vehicle veh, uint gRootId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (!vData.IsFullOwner(pData))
                return;

            var gRoot = Game.Estates.Garage.GarageRoot.Get(gRootId);

            if (gRoot == null)
                return;

            if (slot >= 0)
            {
                if (player.Dimension != Properties.Settings.Profile.Current.Game.MainDimension)
                    return;

                var garage = pData.OwnedGarages.Where(x => x.Root == gRoot).FirstOrDefault();

                if (garage == null)
                    return;

                var freeSlots = Enumerable.Range(0, garage.StyleData.MaxVehicles).ToList();

                var garageVehs = garage.GetVehiclesInGarage();

                foreach (var x in garageVehs)
                {
                    freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
                }

                if (!freeSlots.Contains(slot))
                    return;

                garage.SetVehicleToGarage(vData, slot);
            }
            else
            {

            }
        }
    }
}
