using BlaineRP.Server.Game.Inventory;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Items
{
    public partial class VehicleJerrycan
    {
        internal class RemoteEvents : Script
        {
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

                if (player.Vehicle != null || !player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return;

                var vDataData = vData.Data;

                if (vDataData.FuelType == EntitiesData.Vehicles.Static.Vehicle.FuelTypes.None || amount <= 0 || itemIdx < 0 || itemIdx >= pData.Items.Length)
                    return;

                var jerrycanItem = pData.Items[itemIdx] as VehicleJerrycan;

                if (jerrycanItem == null || vDataData.FuelType == EntitiesData.Vehicles.Static.Vehicle.FuelTypes.Petrol != jerrycanItem.Data.IsPetrol)
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

                player.InventoryUpdate(GroupTypes.Items, itemIdx, ToClientJson(pData.Items[itemIdx], GroupTypes.Items));

                player.Notify(vDataData.FuelType == EntitiesData.Vehicles.Static.Vehicle.FuelTypes.Petrol ? "Vehicle::JCUSP" : "Vehicle::JCUSE");
            }
        }
    }
}