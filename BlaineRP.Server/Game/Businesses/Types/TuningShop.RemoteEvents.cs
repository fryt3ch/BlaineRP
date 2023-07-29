using BlaineRP.Server.Additional;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Management.AntiCheat;
using BlaineRP.Server.Sync;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Businesses
{
    public partial class TuningShop
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("TuningShop::Enter")]
            public static void TuningShopEnter(Player player, int id, Vehicle veh)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.CurrentBusiness != null)
                    return;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                VehicleData vData = veh.GetMainData();

                if (vData == null)
                    return;

                var ts = Get(id) as TuningShop;

                if (ts == null)
                    return;

                if (!ts.IsPlayerNearInteractPosition(pData))
                    return;

                if (player.Vehicle != veh)
                {
                    if (player.Vehicle?.Exists != true)
                        return;

                    VehicleData plVehData = player.Vehicle.GetMainData();

                    if (plVehData == null)
                        return;

                    if (veh.GetAttachmentData(plVehData.Vehicle)?.Type != AttachmentType.VehicleTrailerObjBoat)
                        return;

                    Vector4 exitPos = GetNextExitProperty(ts);

                    plVehData.Vehicle.Teleport(exitPos.Position, null, exitPos.RotationZ, false, VehicleTeleportType.Default);
                }

                vData.DetachBoatFromTrailer();

                pData.CurrentTuningVehicle = vData;

                pData.UnequipActiveWeapon();

                pData.StopUseCurrentItem();
                player.DetachAllObjectsInHand();
                pData.StopAllAnims();

                uint pDim = Utils.GetPrivateDimension(player);

                if (player.Vehicle == veh)
                {
                    if (vData.IsAttachedTo is Vehicle attachedVeh)
                    {
                        Vector4 exitPos = GetNextExitProperty(ts);

                        attachedVeh.Teleport(exitPos.Position, null, exitPos.RotationZ, false, VehicleTeleportType.Default);
                    }

                    veh.Teleport(ts.EnterProperties.Position, pDim, ts.EnterProperties.RotationZ, true, VehicleTeleportType.OnlyDriver);
                }
                else
                {
                    veh.Teleport(ts.EnterProperties.Position, pDim, ts.EnterProperties.RotationZ, true, VehicleTeleportType.Default);

                    player.Teleport(ts.EnterProperties.Position, false, pDim, ts.EnterProperties.RotationZ, true);
                }

                vData.SetFreezePosition(ts.EnterProperties.Position);

                if (!vData.EngineOn)
                    vData.EngineOn = true;

                if (!vData.LightsOn)
                    vData.LightsOn = true;

                player.TriggerEvent("Shop::Show", (int)ts.Type, (float)ts.Margin, ts.EnterProperties.RotationZ, ts.GetVehicleClassMargin(vData.Data.Class), veh);

                pData.CurrentBusiness = ts;
            }
        }
    }
}