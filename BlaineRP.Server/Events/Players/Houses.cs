using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlaineRP.Server.Events.Players
{
    class Houses : Script
    {
        [RemoteEvent("House::FSOV")]
        private static void StopStyleOverview(Player player, ushort currentStyleId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsCuffed || pData.IsFrozen)
                return;

            var house = pData.CurrentHouseBase;

            if (house == null)
                return;

            if (house.StyleData.IsPositionInsideInterior(player.Position))
                return;

            house.SetPlayersInside(true, player);
        }

        [RemoteProc("House::SSOV")]
        private static bool StartStyleOverview(Player player, ushort styleId, ushort currentStyleId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var house = pData.CurrentHouseBase;

            if (house == null)
                return false;

            if (house.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return false;
            }

            if (styleId == currentStyleId)
                return false;

            var style = Game.Estates.HouseBase.Style.Get(styleId);

            if (style == null)
                return false;

            var currentStyle = Game.Estates.HouseBase.Style.Get(currentStyleId);

            if (currentStyle == null)
                return false;

            if (!style.IsHouseTypeSupported(house.Type) || !style.IsRoomTypeSupported(house.RoomType))
                return false;

            if (style.IsTypeFamiliar(currentStyleId) && currentStyle.IsPositionInsideInterior(player.Position))
            {
                var offset = style.InteriorPosition.Position - currentStyle.InteriorPosition.Position;

                player.Teleport(player.Position + offset, false, null, null, false);
            }
            else
            {
                player.Teleport(style.Position, false, null, style.Heading, false);
            }

            return true;
        }

        [RemoteProc("House::BST")]
        private static bool BuyStyle(Player player, ushort styleId, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var house = pData.CurrentHouseBase;

            if (house == null)
                return false;

            if (house.Owner != pData.Info)
            {
                player.Notify("House::NotAllowed");

                return false;
            }

            var style = Game.Estates.HouseBase.Style.Get(styleId);

            if (style == null)
                return false;

            if (house.StyleType == styleId)
                return false;

            if (!style.IsHouseTypeSupported(house.Type) || !style.IsRoomTypeSupported(house.RoomType))
                return false;

            var price = style.Price;

            if (useCash)
            {
                ulong newBalance;

                if (!pData.TryRemoveCash(price, out newBalance, true, null))
                    return false;

                pData.SetCash(newBalance);
            }
            else
            {
                if (!pData.HasBankAccount(true))
                    return false;

                ulong newBalance;

                if (!pData.BankAccount.TryRemoveMoneyDebit(price, out newBalance, true, null))
                    return false;

                pData.BankAccount.SetDebitBalance(newBalance, null);
            }

            house.SetStyle(styleId, style, true);

            return true;
        }

        [RemoteProc("House::STG")]
        private static bool SellToGov(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var house = pData.CurrentHouseBase;

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
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            if (!Enum.IsDefined(typeof(Game.Estates.HouseBase.Types), hTypeNum))
                return false;

            var hType = (Game.Estates.HouseBase.Types)hTypeNum;

            var house = hType == Game.Estates.HouseBase.Types.House ? (Game.Estates.HouseBase)Game.Estates.House.Get(id) : (Game.Estates.HouseBase)Game.Estates.Apartments.Get(id);

            if (house == null)
                return false;

            if (!house.IsEntityNearEnter(player))
                return false;

            if (house.Owner != null)
            {
                player.Notify("House::AB");

                return true;
            }

            var res = house.BuyFromGov(pData);

            return res;
        }

        [RemoteEvent("ARoot::Elevator")]
        public static void ApartmentsRootElevator(Player player, ushort curFloor, ushort subIdx, ushort destFloor)
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

            var shell = aRoot.Shell;

            var curFloorPos = shell.GetFloorPosition(curFloor, subIdx);

            if (curFloorPos == null)
                return;

            if (Vector3.Distance(player.Position, curFloorPos.Position) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
                return;

            curFloorPos = shell.GetFloorPosition(destFloor, subIdx);

            if (curFloorPos == null)
                return;

            player.Teleport(curFloorPos.Position, false, aRoot.Dimension, curFloorPos.RotationZ, true);
        }

        [RemoteEvent("ARoot::Enter")]
        public static void ApartmentsRootEnter(Player player, uint id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var aRoot = Game.Estates.Apartments.ApartmentsRoot.Get(id);

            if (aRoot == null)
                return;

            if (player.Dimension != Properties.Settings.Static.MainDimension || Vector3.Distance(player.Position, aRoot.EnterParams.Position) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
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

            if (Vector3.Distance(player.Position, aRoot.Shell.EnterPosition.Position) > Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE)
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

            var house = pData.CurrentHouseBase as Game.Estates.House;

            if (house == null || house.GarageData == null)
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

            if (!vData.IsFullOwner(pData, true))
                return;

            if (slot >= 0)
            {
                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return;

                var house = Game.Estates.House.Get(houseId);

                if (house == null || house.GarageData == null)
                    return;

                if (house.Owner != pData.Info)
                    return;

                if (!house.IsEntityNearVehicleEnter(player))
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

            if (!vData.IsFullOwner(pData))
                return;

            if (!player.IsNearToEntity(veh, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var house = Game.Estates.House.Get(houseId);

            if (house == null || house.GarageData == null)
                return;

            if (house.Owner != pData.Info)
                return;

            if (!house.IsEntityNearVehicleEnter(player))
                return;

            var freeSlots = Enumerable.Range(0, house.GarageData.MaxVehicles).ToList();

            var garageVehs = house.GetVehiclesInGarage();

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
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return 0;

            var houseBase = pData.CurrentHouseBase;

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
            {
                rotZ = -(180f - rotZ % 180);
            }
            else if (rotZ <= -180f)
            {
                rotZ = -(rotZ % 180f);
            }

            var d = new Utils.Vector4(x, y, z, rotZ);

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

            pData.Info.AddFurniture(furniture);

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
