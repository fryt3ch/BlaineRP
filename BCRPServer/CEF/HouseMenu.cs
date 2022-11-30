using GTANetworkAPI;
using System;
using System.Collections.Generic;
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

            player.TriggerEvent("HouseMenu::Show", "[]", house.Balance, house.IsLocked, house.ContainersLocked);
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

            house.LightsStates[idx].Colour.Red = r;
            house.LightsStates[idx].Colour.Green = g;
            house.LightsStates[idx].Colour.Blue = b;

            NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "House::Light", idx, house.LightsStates[idx].Colour.SerializeToJson());
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
        public static void HouseMenuFurnitureEnd(Player player, uint fUid, string posStr, string rotStr)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (posStr == null || rotStr == null)
                return;

            var pos = posStr.DeserializeFromJson<Vector3>();
            var rot = posStr.DeserializeFromJson<Vector3>();

            var house = pData.CurrentHouse;

            if (house == null)
                return;

            var furniture = Game.Houses.Furniture.Get(fUid);

            if (furniture == null)
                return;

            if (pData.Furniture.Contains(furniture))
            {

            }
            else
            {
                if (!house.Furniture.Contains(furniture))
                    return;
            }

            furniture.Data.Position = pos;
            furniture.Data.Rotation = rot;

            NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "House::Furn", furniture.UID, posStr, rotStr);
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

            var furniture = Game.Houses.Furniture.Get(fUid);

            if (furniture == null)
                return;

            if (!house.Furniture.Contains(furniture))
                return;

            house.Furniture.Remove(furniture);

            pData.Furniture.Add(furniture);

            NAPI.ClientEvent.TriggerClientEventInDimension(house.Dimension, "House::Furn", furniture.UID);
        }
    }
}
