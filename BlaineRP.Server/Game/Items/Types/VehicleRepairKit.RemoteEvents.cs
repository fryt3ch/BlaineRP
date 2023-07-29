using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class VehicleRepairKit
    {
        internal class RemoteEvents : Script
        {
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

                if (player.Vehicle != null || !player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return;

                if (vData.IsDead)
                {
                    player.Notify("Vehicle::RKDE");

                    return;
                }

                VehicleRepairKit rKit = null;

                int idx = -1;

                for (var i = 0; i < pData.Items.Length; i++)
                {
                    if (pData.Items[i] is VehicleRepairKit vrk)
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

                player.InventoryUpdate(GroupTypes.Items, idx, ToClientJson(rKit, GroupTypes.Items));
            }
        }
    }
}