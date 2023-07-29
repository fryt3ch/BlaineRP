using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class VehicleDestruction
    {
        internal class RemoteEvents : Script
        {
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

                if (!veh.IsNearToEntity(player, 10f))
                    return null;

                var destr = Get(destrId);

                if (destr == null)
                    return null;

                if (player.Dimension != Properties.Settings.Static.MainDimension || destr.Position.DistanceTo(player.Position) > 10f)
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

                if (!veh.IsNearToEntity(player, 10f))
                    return false;

                var destr = Get(destrId);

                if (destr == null)
                    return false;

                if (player.Dimension != Properties.Settings.Static.MainDimension || destr.Position.DistanceTo(player.Position) > 10f)
                    return false;

                if (!vData.IsFullOwner(pData, true))
                    return false;

                ulong price = destr.GetPriceForVehicle(vData.Info);

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
}