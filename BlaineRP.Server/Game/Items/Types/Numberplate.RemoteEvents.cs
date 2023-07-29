using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class Numberplate
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("Vehicles::TakePlate")]
            public static void TakePlate(Player player, Vehicle veh)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                VehicleData vData = veh.GetMainData();

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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return;

                VehicleData vData = veh.GetMainData();

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

                var np = pData.Items[npIdx] as Numberplate;

                if (np == null)
                    return;

                pData.Items[npIdx] = null;

                player.InventoryUpdate(GroupTypes.Items, npIdx, ToClientJson(null, GroupTypes.Items));

                MySQL.CharacterItemsUpdate(pData.Info);

                np.Setup(vData);

                vData.Numberplate = np;

                MySQL.VehicleNumberplateUpdate(vData.Info);

                player.Notify("NP::Set", np.Tag);
            }
        }
    }
}