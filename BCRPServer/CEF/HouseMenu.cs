using GTANetworkAPI;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.CEF
{
    public class HouseMenu : Script
    {
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

                    NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "HouseMenu::SettlerPerm", cid, idx, state);

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
    }
}
