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
        [RemoteEvent("House::Enter")]
        public static void HouseEnter(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var house = pData.CurrentHouse;

            if (house != null)
                return;

            house = Game.Houses.House.All.GetValueOrDefault(id);

            if (house == null)
                return;

            if (player.Dimension != Utils.Dimensions.Main || Vector3.Distance(player.Position, house.PositionParams.Position) > Settings.ENTITY_INTERACTION_MAX_DISTANCE)
                return;

            if (house.IsLocked && house.Owner != pData.Info && !house.Settlers.ContainsKey(pData.Info))
            {
                player.Notify("House::HL");

                return;
            }

            var sData = house.StyleData;

            player.Teleport(sData.Position, false, house.Dimension, sData.Heading, true);

            player.TriggerEvent("House::Enter", house.ToClientJson());
        }

        [RemoteEvent("House::Exit")]
        public static void HouseExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (player.Dimension != house.Dimension)
                return;

            player.Teleport(house.PositionParams.Position, false, Utils.Dimensions.Main, house.PositionParams.RotationZ, true);

            player.TriggerEvent("House::Exit");
        }

        [RemoteEvent("House::Garage")]
        public static void HouseGarage(Player player, bool to)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var house = pData.CurrentHouse;

            if (house == null || house.GarageData == null)
                return;

            if (player.Dimension != house.Dimension)
                return;

            player.CloseAll(true);

            if (to)
            {
                player.Heading = house.GarageData.EnterHeading;
                player.Teleport(house.GarageData.EnterPosition, false);
            }
            else
            {
                player.Heading = house.StyleData.Heading;
                player.Teleport(house.StyleData.Position, false);
            }
        }

        [RemoteEvent("House::Garage::Vehicle")]
        public static void HouseGarageVehicle(Player player, bool to, Vehicle veh, uint houseId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (veh?.Exists != true)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            if (to)
            {
                if (player.Dimension != Utils.Dimensions.Main)
                    return;

                var house = Game.Houses.House.Get(houseId);

                if (house == null || house.GarageData == null)
                    return;

                veh.TriggerEventOccupants("House::Enter", house.ToClientJson());

                vData.EngineOn = false;

                veh.Teleport(house.GarageData.VehiclePositions[0].Position, house.Dimension, house.GarageData.VehiclePositions[0].Heading, true, false);

                veh.SetSharedData("InGarage", true);
            }
            else
            {

            }
        }

        [RemoteEvent("House::Door")]
        public static void HouseDoor(Player player, int doorIdx, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (doorIdx < 0)
                return;

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (doorIdx >= house.DoorsStates.Length)
                return;

            if (house.Owner != pData.Info && house.Settlers.GetValueOrDefault(pData.Info)?[1] != true)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            if (house.DoorsStates[doorIdx] == state)
                return;

            house.DoorsStates[doorIdx] = state;

            NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "House::Door", doorIdx, state);
        }

        [RemoteEvent("House::Menu::Show")]
        public static void HouseMenuShow(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (house.Owner != pData.Info && house.Settlers.ContainsKey(pData.Info))
            {
                player.Notify("House::NotAllowed");

                return;
            }

            player.TriggerEvent("HouseMenu::Show", house.Settlers.ToDictionary(x => $"{x.Key.Name}_{x.Key.Surname}_{x.Key.CID}", x => x.Value).SerializeToJson(), house.Balance, house.IsLocked, house.ContainersLocked);
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

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (idx >= house.LightsStates.Length)
                return;

            if (house.Owner != pData.Info && house.Settlers.GetValueOrDefault(pData.Info)?[0] != true)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            if (house.LightsStates[idx].State == state)
                return;

            house.LightsStates[idx].State = state;

            NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "House::Light", idx, state);
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

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (idx >= house.LightsStates.Length)
                return;

            if (house.Owner != pData.Info && house.Settlers.GetValueOrDefault(pData.Info)?[0] != true)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            house.LightsStates[idx].Colour.Red = r;
            house.LightsStates[idx].Colour.Green = g;
            house.LightsStates[idx].Colour.Blue = b;

            NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "House::Light", idx, house.LightsStates[idx].Colour.SerializeToJson());

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

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (idx >= 5)
                return;

            if (house.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            foreach (var x in house.Settlers)
            {
                if (x.Key.CID == cid)
                {
                    if (x.Value[idx] == state)
                        return;

                    x.Value[idx] = state;

                    house.TriggerEventForHouseOwners("HouseMenu::SettlerPerm", cid, idx, state);

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

            var house = pData.CurrentHouse;

            if (house == null)
                return false;

            if (house.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return false;
            }

            var furniture = Game.Houses.Furniture.Get(fUid);

            if (furniture == null)
                return false;

            if (pData.Furniture.Contains(furniture))
            {
                return true;
            }
            else
            {
                if (!house.Furniture.Contains(furniture))
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

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (house.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            var furniture = Game.Houses.Furniture.Get(fUid);

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
                pData.RemoveFurniture(furniture);

                house.Furniture.Add(furniture);

                MySQL.HouseFurnitureUpdate(house);
            }
            else
            {
                if (!house.Furniture.Contains(furniture))
                    return;
            }

            furniture.Data = new Utils.Vector4(x, y, z, rotZ);

            NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "House::Furn", furniture.SerializeToJson());

            MySQL.FurnitureUpdate(furniture);
        }

        [RemoteEvent("House::Menu::Furn::Remove")]
        public static void HouseMenuFurnitureRemove(Player player, uint fUid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (house.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            var furniture = Game.Houses.Furniture.Get(fUid);

            if (furniture == null)
                return;

            if (!house.Furniture.Contains(furniture))
                return;

            house.Furniture.Remove(furniture);

            pData.AddFurniture(furniture);

            NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "House::Furn", furniture.UID);

            MySQL.HouseFurnitureUpdate(house);
        }

        [RemoteEvent("House::Menu::Expel")]
        public static void HouseMenuExpel(Player player, uint cid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (house.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            var sInfo = house.Settlers.Where(x => x.Key.CID == cid).Select(x => x.Key).FirstOrDefault();

            if (sInfo == null)
                return;

            if (sInfo.PlayerData == null)
            {

            }
        }

        [RemoteEvent("House::Lock")]
        public static void HouseLock(Player player, bool doors, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            if (house.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return;
            }

            if (doors)
            {
                if (state == house.IsLocked)
                    return;

                house.IsLocked = state;
            }
            else
            {
                if (state == house.ContainersLocked)
                    return;

                house.ContainersLocked = state;
            }

            house.TriggerEventForHouseOwners("House::Lock", doors, state);
        }
    }
}
