using BlaineRP.Server.Additional;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Items;
using BlaineRP.Server.Game.Management.AntiCheat;
using BlaineRP.Server.Game.Management.Chat;
using GTANetworkAPI;

namespace BlaineRP.Server.Events.Vehicles
{
    internal class Interaction : Script
    {
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

            if (!player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (!vData.IsFullOwner(pData, true))
                return;

            vData.Info.ShowPassport(player);
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

            if (Game.Management.Chat.Service.SendLocal(MessageType.Try, player, Language.Strings.Get("CHAT_VEHICLE_PSGR_KICKED"), target, tData.IsKnocked || tData.IsCuffed ? true : (object)null))
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

            if (!player.IsNearToEntity(vehicle, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
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

            if (!veh.IsNearToEntity(player, 10f))
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
                    vData.Vehicle.Teleport(placePos, null, null, false, VehicleTeleportType.All);
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

            if (!veh.IsNearToEntity(player, 10f))
                return;

            if (!vData.CanManipulate(pData, true))
                return;

            vData.AttachBoatToTrailer();
        }
    }
}
