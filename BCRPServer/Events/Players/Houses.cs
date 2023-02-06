using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BCRPServer.Events.Players
{
    class Houses : Script
    {
        [RemoteEvent("ARoot::Elevator")]
        public static void ApartmentsRootElevator(Player player, int curFloor, int destFloor)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var aRoot = pData.CurrentApartmentsRoot;

            if (aRoot == null)
                return;

            var curFloorPos = aRoot.GetFloorPosition(curFloor);

            if (curFloorPos == null)
                return;

            if (Vector3.Distance(player.Position, curFloorPos) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return;

            curFloorPos = aRoot.GetFloorPosition(destFloor);

            if (curFloorPos == null)
                return;

            player.Teleport(curFloorPos, false, aRoot.Dimension, null, true);
        }

        [RemoteEvent("ARoot::Enter")]
        public static void ApartmentsRootEnter(Player player, int numType)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            if (!Enum.IsDefined(typeof(Game.Estates.Apartments.ApartmentsRoot.Types), numType))
                return;

            var aRoot = Game.Estates.Apartments.ApartmentsRoot.Get((Game.Estates.Apartments.ApartmentsRoot.Types)numType);

            if (player.Dimension != Utils.Dimensions.Main || Vector3.Distance(player.Position, aRoot.EnterParams.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return;

            aRoot.SetPlayersInside(true, player);
        }

        [RemoteEvent("ARoot::Exit")]
        public static void ApartmentsRootExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var aRoot = pData.CurrentApartmentsRoot;

            if (aRoot == null)
                return;

            if (Vector3.Distance(player.Position, aRoot.ExitParams.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return;

            aRoot.SetPlayersOutside(true, player);
        }

        [RemoteEvent("House::Enter")]
        public static void HouseEnter(Player player, int hTypeNum, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            if (!Enum.IsDefined(typeof(Game.Estates.HouseBase.Types), hTypeNum))
                return;

            var hType = (Game.Estates.HouseBase.Types)hTypeNum;

            if (hType == Game.Estates.HouseBase.Types.House)
            {
                if (player.Dimension != Utils.Dimensions.Main)
                    return;

                var house = Game.Estates.House.Get(id);

                if (house == null)
                    return;

                if (!house.IsEntityNearEnter(player))
                    return;

                if (house.IsLocked && house.Owner != pData.Info && pData.SettledHouseBase != house)
                {
                    player.Notify("House::HL");

                    return;
                }

                house.SetPlayersInside(true, player);
            }
            else
            {
                var aps = Game.Estates.Apartments.Get(id);

                if (aps == null)
                    return;

                if (aps.Root != pData.CurrentApartmentsRoot)
                    return;

                if (!aps.IsEntityNearEnter(player))
                    return;

                if (aps.IsLocked && aps.Owner != pData.Info && pData.SettledHouseBase != aps)
                {
                    player.Notify("House::HL");

                    return;
                }

                aps.SetPlayersInside(true, player);
            }
        }

        [RemoteEvent("House::Exit")]
        public static void HouseExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var house = pData.CurrentHouseBase;

            if (house == null)
                return;

            house.SetPlayersOutside(true, player);
        }

        [RemoteEvent("House::Garage")]
        public static void HouseGarage(Player player, bool to)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var house = pData.CurrentHouse;

            if (house == null || house.GarageData == null)
                return;

            if (player.Dimension != house.Dimension)
                return;

            player.CloseAll(true);

            if (to)
            {
                player.Teleport(house.GarageData.EnterPosition.Position, false, null, house.GarageData.EnterPosition.RotationZ, true);
            }
            else
            {
                player.Teleport(house.StyleData.Position, false, null, house.StyleData.Heading, true);
            }
        }

        [RemoteEvent("House::Garage::Vehicle")]
        public static void HouseGarageVehicle(Player player, int slot, Vehicle veh, uint houseId)
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

            if (slot >= 0)
            {
                if (player.Dimension != Utils.Dimensions.Main)
                    return;

                var house = Game.Estates.House.Get(houseId);

                if (house == null || house.GarageData == null)
                    return;

                var freeSlots = Enumerable.Range(0, house.GarageData.MaxVehicles).ToList();

                var garageVehs = house.GetVehiclesInGarage();

                if (garageVehs == null)
                    return;

                foreach (var x in garageVehs)
                {
                    freeSlots.Remove(x.VehicleData.LastData.GarageSlot);
                }

                if (!freeSlots.Contains(slot))
                    return;

                house.SetVehicleToGarage(vData, slot);
            }
            else
            {

            }
        }

        [RemoteEvent("House::Garage::SlotsMenu")]
        public static void HouseGarageSlotsMenu(Player player, Vehicle veh, uint houseId)
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

            if (player.Dimension != Utils.Dimensions.Main || !vData.IsFullOwner(pData))
                return;

            if (!player.AreEntitiesNearby(veh, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var house = Game.Estates.House.Get(houseId);

            if (house == null || house.GarageData == null)
                return;

            if (house.Owner != pData.Info)
                return;

            var freeSlots = Enumerable.Range(0, house.GarageData.MaxVehicles).ToList();

            var garageVehs = house.GetVehiclesInGarage();

            if (garageVehs == null)
                return;

            foreach (var x in garageVehs)
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

        [RemoteEvent("House::Door")]
        public static void HouseDoor(Player player, int doorIdx, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            if (doorIdx < 0)
                return;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            if (doorIdx >= houseBase.DoorsStates.Length)
                return;

            if (houseBase.Owner != pData.Info && houseBase.Settlers.GetValueOrDefault(pData.Info)?[1] != true)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            if (houseBase.DoorsStates[doorIdx] == state)
                return;

            houseBase.DoorsStates[doorIdx] = state;

            NAPI.ClientEvent.TriggerClientEventInDimension(houseBase.Dimension, "House::Door", doorIdx, state);
        }

        [RemoteEvent("House::Menu::Show")]
        public static void HouseMenuShow(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            if (houseBase.Owner != pData.Info && pData.SettledHouseBase != houseBase)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            player.TriggerEvent("HouseMenu::Show", houseBase.Settlers.ToDictionary(x => $"{x.Key.Name}_{x.Key.Surname}_{x.Key.CID}", x => x.Value).SerializeToJson(), houseBase.Balance, houseBase.IsLocked, houseBase.ContainersLocked);
        }

        [RemoteEvent("House::Menu::Light::State")]
        public static void HouseMenuLightState(Player player, int idx, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (idx < 0)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            if (idx >= houseBase.LightsStates.Length)
                return;

            if (houseBase.Owner != pData.Info && houseBase.Settlers.GetValueOrDefault(pData.Info)?[0] != true)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            if (houseBase.LightsStates[idx].State == state)
                return;

            houseBase.LightsStates[idx].State = state;

            NAPI.ClientEvent.TriggerClientEventInDimension(houseBase.Dimension, "House::Light", idx, state);
        }

        [RemoteEvent("House::Menu::Light::RGB")]
        public static void HouseMenuLightState(Player player, int idx, byte r, byte g, byte b)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (idx < 0)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            if (idx >= houseBase.LightsStates.Length)
                return;

            if (houseBase.Owner != pData.Info && houseBase.Settlers.GetValueOrDefault(pData.Info)?[0] != true)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            houseBase.LightsStates[idx].Colour.Red = r;
            houseBase.LightsStates[idx].Colour.Green = g;
            houseBase.LightsStates[idx].Colour.Blue = b;

            NAPI.ClientEvent.TriggerClientEventInDimension(houseBase.Dimension, "House::Light", idx, houseBase.LightsStates[idx].Colour.SerializeToJson());

            player.TriggerEvent("House::LCC");
        }

        [RemoteEvent("House::Menu::Permission")]
        public static void HouseMenuPermission(Player player, int idx, uint cid, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            if (idx < 0)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            if (idx >= 5)
                return;

            if (houseBase.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            foreach (var x in houseBase.Settlers)
            {
                if (x.Key.CID == cid)
                {
                    if (x.Value[idx] == state)
                        return;

                    x.Value[idx] = state;

                    houseBase.TriggerEventForHouseOwners("HouseMenu::SettlerPerm", cid, idx, state);

                    break;
                }
            }
        }

        [RemoteProc("House::Menu::Furn::Start")]
        public static bool HouseMenuFurnitureStart(Player player, uint fUid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return false;

            if (houseBase.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return false;
            }

            var furniture = Game.Estates.Furniture.Get(fUid);

            if (furniture == null)
                return false;

            if (pData.Furniture.Contains(furniture))
            {
                if (houseBase.Furniture.Count + 1 >= Settings.HOUSE_MAX_FURNITURE)
                {
                    player.Notify("Inv::HMPF", Settings.HOUSE_MAX_FURNITURE);

                    return false;
                }

                return true;
            }
            else
            {
                if (!houseBase.Furniture.Contains(furniture))
                    return false;

                return true;
            }
        }

        [RemoteEvent("House::Menu::Furn::End")]
        public static void HouseMenuFurnitureEnd(Player player, uint fUid, float x, float y, float z, float rotZ)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            if (houseBase.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            var furniture = Game.Estates.Furniture.Get(fUid);

            if (furniture == null)
                return;

            if (rotZ > 180f)
            {
                rotZ = -(180f - rotZ % 180);
            }
            else if (rotZ <= -180f)
            {
                rotZ = -(rotZ % 180f);
            }

            if (pData.Furniture.Contains(furniture))
            {
                if (houseBase.Furniture.Count + 1 >= Settings.HOUSE_MAX_FURNITURE)
                {
                    player.Notify("Inv::HMPF", Settings.HOUSE_MAX_FURNITURE);

                    return;
                }

                pData.RemoveFurniture(furniture);

                houseBase.Furniture.Add(furniture);

                MySQL.HouseFurnitureUpdate(houseBase);
            }
            else
            {
                if (!houseBase.Furniture.Contains(furniture))
                    return;
            }

            furniture.Data = new Utils.Vector4(x, y, z, rotZ);

            furniture.Setup(houseBase);

            NAPI.ClientEvent.TriggerClientEventInDimension(houseBase.Dimension, "House::Furn", furniture.SerializeToJson());

            MySQL.FurnitureUpdate(furniture);
        }

        [RemoteEvent("House::Menu::Furn::Remove")]
        public static void HouseMenuFurnitureRemove(Player player, uint fUid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            if (houseBase.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            var furniture = Game.Estates.Furniture.Get(fUid);

            if (furniture == null)
                return;

            if (!houseBase.Furniture.Remove(furniture))
                return;

            furniture.Delete(houseBase);

            pData.AddFurniture(furniture);

            NAPI.ClientEvent.TriggerClientEventInDimension(houseBase.Dimension, "House::Furn", furniture.UID);

            MySQL.HouseFurnitureUpdate(houseBase);
        }

        [RemoteEvent("House::Menu::Expel")]
        public static void HouseMenuExpel(Player player, uint cid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            var sInfo = houseBase.Settlers.Where(x => x.Key.CID == cid).Select(x => x.Key).FirstOrDefault();

            if (sInfo == null)
                return;

            if (houseBase.Owner != pData.Info && sInfo != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            houseBase.SettlePlayer(sInfo, false, pData);
        }

        [RemoteEvent("House::Lock")]
        public static void HouseLock(Player player, bool doors, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null)
                return;

            if (houseBase.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            if (doors)
            {
                if (state == houseBase.IsLocked)
                    return;

                houseBase.IsLocked = state;
            }
            else
            {
                if (state == houseBase.ContainersLocked)
                    return;

                houseBase.ContainersLocked = state;
            }

            houseBase.TriggerEventForHouseOwners("House::Lock", doors, state);
        }
    }
}
