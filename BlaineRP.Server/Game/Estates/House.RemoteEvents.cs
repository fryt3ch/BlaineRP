using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public partial class House
    {
        internal class RemoteEvents : Script
        {
            [RemoteEvent("House::Garage")]
            public static void HouseGarage(Player player, bool to)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                var house = pData.CurrentHouseBase as House;

                if (house == null || house.GarageData == null)
                    return;

                player.CloseAll(true);

                if (to)
                    player.Teleport(house.GarageData.EnterPosition.Position, false, null, house.GarageData.EnterPosition.RotationZ, true);
                else
                    player.Teleport(house.StyleData.Position, false, null, house.StyleData.Heading, true);
            }

            [RemoteEvent("House::Garage::Vehicle")]
            public static void HouseGarageVehicle(Player player, int slot, Vehicle veh, uint houseId)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                if (veh?.Exists != true)
                    return;

                VehicleData vData = veh.GetMainData();

                if (vData == null)
                    return;

                if (!vData.IsFullOwner(pData, true))
                    return;

                if (slot >= 0)
                {
                    if (player.Dimension != Properties.Settings.Static.MainDimension)
                        return;

                    var house = Get(houseId);

                    if (house == null || house.GarageData == null)
                        return;

                    if (house.Owner != pData.Info)
                        return;

                    if (!house.IsEntityNearVehicleEnter(player))
                        return;

                    var freeSlots = Enumerable.Range(0, house.GarageData.MaxVehicles).ToList();

                    List<VehicleInfo> garageVehs = house.GetVehiclesInGarage();

                    if (garageVehs == null)
                        return;

                    foreach (VehicleInfo x in garageVehs)
                    {
                        freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
                    }

                    if (!freeSlots.Contains(slot))
                        return;

                    house.SetVehicleToGarage(vData, slot);
                }
            }

            [RemoteEvent("House::Garage::SlotsMenu")]
            public static void HouseGarageSlotsMenu(Player player, Vehicle veh, uint houseId)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                if (veh?.Exists != true)
                    return;

                VehicleData vData = veh.GetMainData();

                if (vData == null)
                    return;

                if (!vData.IsFullOwner(pData))
                    return;

                if (!player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return;

                var house = Get(houseId);

                if (house == null || house.GarageData == null)
                    return;

                if (house.Owner != pData.Info)
                    return;

                if (!house.IsEntityNearVehicleEnter(player))
                    return;

                var freeSlots = Enumerable.Range(0, house.GarageData.MaxVehicles).ToList();

                List<VehicleInfo> garageVehs = house.GetVehiclesInGarage();

                foreach (VehicleInfo x in garageVehs)
                {
                    freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
                }

                if (freeSlots.Count == 0)
                {
                    player.Notify("Garage::NVP");

                    return;
                }
                else if (freeSlots.Count == 1)
                {
                    house.SetVehicleToGarage(vData, freeSlots[0]);
                }
                else
                {
                    player.TriggerEvent("Vehicles::Garage::SlotsMenu", freeSlots);
                }
            }
        }
    }
}