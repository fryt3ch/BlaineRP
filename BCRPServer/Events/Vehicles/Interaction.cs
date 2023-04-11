using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using static BCRPServer.Game.Items.Inventory;

namespace BCRPServer.Events.Vehicles
{
    internal class Interaction : Script
    {
        [RemoteEvent("Vehicles::TakePlate")]
        public static void TakePlate(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null)
                return;

            if (!vData.IsFullOwner(pData, true))
                return;

            if (vData.Numberplate == null)
                return;

            if (!pData.TryGiveExistingItem(vData.Numberplate, 1, true, true))
                return;

            vData.Numberplate.Take(vData);

            vData.Numberplate = null;

            MySQL.VehicleNumberplateUpdate(vData.Info);
        }

        [RemoteEvent("Vehicles::SetupPlate")]
        public static void SetupPlate(Player player, Vehicle veh, int npIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null)
                return;

            if (vData.Numberplate != null)
                return;

            if (!vData.IsFullOwner(pData, true))
                return;

            if (npIdx < 0 || npIdx >= pData.Items.Length)
                return;

            var np = pData.Items[npIdx] as Game.Items.Numberplate;

            if (np == null)
                return;

            pData.Items[npIdx] = null;

            player.InventoryUpdate(Groups.Items, npIdx, Game.Items.Item.ToClientJson(null, Game.Items.Inventory.Groups.Items));

            MySQL.CharacterItemsUpdate(pData.Info);

            np.Setup(vData);

            vData.Numberplate = np;

            MySQL.VehicleNumberplateUpdate(vData.Info);

            player.Notify("NP::Set", np.Tag);
        }

        [RemoteEvent("Vehicles::ShowPass")]
        private static void ShowPassport(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (!player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (!vData.IsFullOwner(pData, true))
                return;

            vData.Info.ShowPassport(player);
        }

        [RemoteEvent("Vehicles::Fix")]
        private static void VehicleFix(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null || !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (vData.IsDead)
            {
                player.Notify("Vehicle::RKDE");

                return;
            }

            Game.Items.VehicleRepairKit rKit = null;

            var idx = -1;

            for (int i = 0; i < pData.Items.Length; i++)
            {
                if (pData.Items[i] is Game.Items.VehicleRepairKit vrk)
                {
                    rKit = vrk;

                    idx = i;

                    break;
                }
            }

            if (rKit == null)
            {
                player.Notify("Inventory::NoItem");

                return;
            }

            rKit.Apply(pData, vData);

            if (rKit.Amount == 1)
            {
                rKit.Delete();

                rKit = null;

                pData.Items[idx] = null;

                MySQL.CharacterItemsUpdate(pData.Info);
            }
            else
            {
                rKit.Amount -= 1;

                rKit.Update();
            }

            player.InventoryUpdate(Game.Items.Inventory.Groups.Items, idx, Game.Items.Item.ToClientJson(rKit, Game.Items.Inventory.Groups.Items));
        }

        [RemoteEvent("Vehicles::JerrycanUse")]
        private static void VehicleJerrycanUse(Player player, Vehicle veh, int itemIdx, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (player.Vehicle != null || !player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var vDataData = vData.Data;

            if (vDataData.FuelType == Game.Data.Vehicles.Vehicle.FuelTypes.None || amount <= 0 || itemIdx < 0 || itemIdx >= pData.Items.Length)
                return;

            var jerrycanItem = pData.Items[itemIdx] as Game.Items.VehicleJerrycan;

            if (jerrycanItem == null || ((vDataData.FuelType == Game.Data.Vehicles.Vehicle.FuelTypes.Petrol) != jerrycanItem.Data.IsPetrol))
                return;

            if (jerrycanItem.Amount < amount)
                amount = jerrycanItem.Amount;

            jerrycanItem.Apply(pData, vData, amount);

            jerrycanItem.Amount -= amount;

            if (jerrycanItem.Amount == 0)
            {
                jerrycanItem.Delete();

                pData.Items[itemIdx] = null;

                MySQL.CharacterItemsUpdate(pData.Info);
            }
            else
            {
                jerrycanItem.Update();
            }

            player.InventoryUpdate(Game.Items.Inventory.Groups.Items, itemIdx, Game.Items.Item.ToClientJson(pData.Items[itemIdx], Game.Items.Inventory.Groups.Items));

            player.Notify(vDataData.FuelType == Game.Data.Vehicles.Vehicle.FuelTypes.Petrol ? "Vehicle::JCUSP" : "Vehicle::JCUSE");
        }

        [RemoteEvent("Vehicles::ShuffleSeat")]
        public static void ShuffleSeat(Player player, int seatId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (seatId < 0)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (seatId >= veh.MaxOccupants)
                return;

            var currentSeat = pData.VehicleSeat;

            if (currentSeat == seatId || currentSeat <= 0 || veh.GetEntityInVehicleSeat(seatId) != null)
                return;

            if (seatId == 0)
            {
                player.SetIntoVehicle(veh, seatId);
            }
            else
                pData.VehicleSeat = seatId;
        }

        [RemoteEvent("Vehicles::KickPlayer")]
        public static void KickPlayer(Player player, Player target)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsFrozen || pData.IsCuffed)
                return;

            if (player.VehicleSeat != 0)
                return;

            var veh = player.Vehicle;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            var tData = target.GetMainData();

            if (tData == null)
                return;

            if (target.Vehicle != player.Vehicle)
                return;

            if (Sync.Chat.SendLocal(Sync.Chat.Types.TryPlayer, player, Locale.Chat.Vehicle.Kick, target))
            {
                target.WarpOutOfVehicle();
            }
        }

        [RemoteProc("Vehicles::HVIL")]
        private static bool HoodVehicleInfoLook(Player player, Vehicle vehicle)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var vData = vehicle.GetMainData();

            if (vData == null)
                return false;

            if (!player.AreEntitiesNearby(vehicle, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return false;

            if (vData.HoodLocked && !vData.CanManipulate(pData, false))
            {
                player.Notify("Vehicle::HISLE");

                return false;
            }

            return true;
        }

        [RemoteEvent("Vehicles::BTOW")]
        private static void VehicleBoatTrailerOnWater(Player player, Vehicle veh, float x, float y, float z)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (!veh.AreEntitiesNearby(player, 10f))
                return;

            if (!vData.CanManipulate(pData, true))
                return;

            if (x == float.MaxValue)
            {
                if (vData.DetachBoatFromTrailer())
                {

                }
            }
            else
            {
                var placePos = new Vector3(x, y, z);

                if (placePos.DistanceTo(veh.Position) > 15f)
                    return;

                if (vData.DetachBoatFromTrailer())
                {
                    vData.Vehicle.Teleport(placePos, null, null, false, Additional.AntiCheat.VehicleTeleportTypes.All);
                }
            }
        }

        [RemoteEvent("Vehicles::BTOT")]
        private static void VehicleBoatOnTrailer(Player player, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (!veh.AreEntitiesNearby(player, 10f))
                return;

            if (!vData.CanManipulate(pData, true))
                return;

            vData.AttachBoatToTrailer();
        }

        [RemoteProc("Vehicles::VDGP")]
        private static ulong? VehicleDestructionGetPrice(Player player, Vehicle veh, int destrId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return null;

            var vData = veh.GetMainData();

            if (vData == null)
                return null;

            if (!veh.AreEntitiesNearby(player, 10f))
                return null;

            var destr = Game.Misc.VehicleDestruction.Get(destrId);

            if (destr == null)
                return null;

            if (player.Dimension != Utils.Dimensions.Main || destr.Position.DistanceTo(player.Position) > 10f)
                return null;

            if (!vData.IsFullOwner(pData, true))
                return null;

            return destr.GetPriceForVehicle(vData.Info);
        }

        [RemoteProc("Vehicles::VDC")]
        private static bool VehicleDestructionConfirm(Player player, Vehicle veh, int destrId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var vData = veh.GetMainData();

            if (vData == null)
                return false;

            if (!veh.AreEntitiesNearby(player, 10f))
                return false;

            var destr = Game.Misc.VehicleDestruction.Get(destrId);

            if (destr == null)
                return false;

            if (player.Dimension != Utils.Dimensions.Main || destr.Position.DistanceTo(player.Position) > 10f)
                return false;

            if (!vData.IsFullOwner(pData, true))
                return false;

            var price = destr.GetPriceForVehicle(vData.Info);

            ulong newBalance;

            if (pData.TryAddCash(price, out newBalance, true))
            {
                vData.Delete(true);

                pData.SetCash(newBalance);
            }

            return true;
        }
    }
}
