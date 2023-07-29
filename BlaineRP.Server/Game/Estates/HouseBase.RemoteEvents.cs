using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Estates
{
    public abstract partial class HouseBase
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("House::STG")]
            private static bool SellToGov(Player player)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                HouseBase house = pData.CurrentHouseBase;

                if (house == null)
                    return false;

                if (house.Owner != pData.Info)
                {
                    player.Notify("House::NotAllowed");

                    return false;
                }

                house.SellToGov(true, true);

                return true;
            }

            [RemoteProc("House::BuyGov")]
            private static bool BuyGov(Player player, int hTypeNum, uint id)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                if (!Enum.IsDefined(typeof(Types), hTypeNum))
                    return false;

                var hType = (Types)hTypeNum;

                HouseBase house = hType == Types.House ? (HouseBase)House.Get(id) : (HouseBase)Apartments.Get(id);

                if (house == null)
                    return false;

                if (!house.IsEntityNearEnter(player))
                    return false;

                if (house.Owner != null)
                {
                    player.Notify("House::AB");

                    return true;
                }

                bool res = house.BuyFromGov(pData);

                return res;
            }

            [RemoteEvent("House::Door")]
            public static void HouseDoor(Player player, int doorIdx, bool state)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                if (doorIdx < 0)
                    return;

                HouseBase houseBase = pData.CurrentHouseBase;

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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                HouseBase houseBase = pData.CurrentHouseBase;

                if (houseBase == null)
                    return;

                if (houseBase.Owner != pData.Info && pData.SettledHouseBase != houseBase)
                {
                    player.Notify("House::NotAllowed");

                    return;
                }

                player.TriggerEvent("HouseMenu::Show",
                    houseBase.Settlers.ToDictionary(x => $"{x.Key.Name}_{x.Key.Surname}_{x.Key.CID}", x => x.Value).SerializeToJson(),
                    houseBase.Balance,
                    houseBase.IsLocked,
                    houseBase.ContainersLocked
                );
            }

            [RemoteEvent("House::Menu::Light::State")]
            public static void HouseMenuLightState(Player player, int idx, bool state)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                if (idx < 0)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                HouseBase houseBase = pData.CurrentHouseBase;

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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                if (idx < 0)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                HouseBase houseBase = pData.CurrentHouseBase;

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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                if (idx < 0)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                HouseBase houseBase = pData.CurrentHouseBase;

                if (houseBase == null)
                    return;

                if (idx >= 5)
                    return;

                if (houseBase.Owner != pData.Info)
                {
                    player.Notify("House::NotAllowed");

                    return;
                }

                foreach (KeyValuePair<PlayerInfo, bool[]> x in houseBase.Settlers)
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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                HouseBase houseBase = pData.CurrentHouseBase;

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
                    if (houseBase.Furniture.Count + 1 >= Properties.Settings.Static.HOUSE_MAX_FURNITURE)
                    {
                        player.Notify("Inv::HMPF", Properties.Settings.Static.HOUSE_MAX_FURNITURE);

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

            [RemoteProc("House::Menu::Furn::End")]
            public static byte HouseMenuFurnitureEnd(Player player, uint fUid, float x, float y, float z, float rotZ)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return 0;

                HouseBase houseBase = pData.CurrentHouseBase;

                if (houseBase == null)
                    return 0;

                if (houseBase.Owner != pData.Info)
                {
                    player.Notify("House::NotAllowed");

                    return 0;
                }

                var furniture = Game.Estates.Furniture.Get(fUid);

                if (furniture == null)
                    return 0;

                if (rotZ > 180f)
                    rotZ = -(180f - rotZ % 180);
                else if (rotZ <= -180f)
                    rotZ = -(rotZ % 180f);

                var d = new Vector4(x, y, z, rotZ);

                if (!houseBase.StyleData.IsPositionInsideInterior(d.Position))
                {
                    player.Notify("House::FPE0");

                    return 255;
                }

                if (pData.Furniture.Contains(furniture))
                {
                    if (houseBase.Furniture.Count + 1 >= Properties.Settings.Static.HOUSE_MAX_FURNITURE)
                    {
                        player.Notify("Inv::HMPF", Properties.Settings.Static.HOUSE_MAX_FURNITURE);

                        return 255;
                    }

                    pData.Info.RemoveFurniture(furniture);

                    houseBase.Furniture.Add(furniture);

                    MySQL.HouseFurnitureUpdate(houseBase);
                }
                else
                {
                    if (!houseBase.Furniture.Contains(furniture))
                        return 0;
                }

                furniture.Data = d;

                furniture.Setup(houseBase);

                NAPI.ClientEvent.TriggerClientEventInDimension(houseBase.Dimension, "House::Furn", furniture.SerializeToJson());

                MySQL.FurnitureUpdate(furniture);

                return 1;
            }

            [RemoteEvent("House::Menu::Furn::Remove")]
            public static void HouseMenuFurnitureRemove(Player player, uint fUid)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                HouseBase houseBase = pData.CurrentHouseBase;

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

                pData.Info.AddFurniture(furniture);

                NAPI.ClientEvent.TriggerClientEventInDimension(houseBase.Dimension, "House::Furn", furniture.UID);

                MySQL.HouseFurnitureUpdate(houseBase);
            }

            [RemoteEvent("House::Menu::Expel")]
            public static void HouseMenuExpel(Player player, uint cid)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                HouseBase houseBase = pData.CurrentHouseBase;

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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                HouseBase houseBase = pData.CurrentHouseBase;

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

            [RemoteEvent("House::Enter")]
            public static void HouseEnter(Player player, int hTypeNum, uint id)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                if (!Enum.IsDefined(typeof(Types), hTypeNum))
                    return;

                var hType = (Types)hTypeNum;

                if (hType == Types.House)
                {
                    var house = House.Get(id);

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
                    var aps = Apartments.Get(id);

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
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return;

                HouseBase house = pData.CurrentHouseBase;

                if (house == null)
                    return;

                house.SetPlayersOutside(true, player);
            }
        }
    }
}