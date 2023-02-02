using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players
{
    class Business : Script
    {
        [RemoteEvent("SRange::Exit::Shop")]
        private static void ShootingRangeExitShop(Player player, int score, int maxScore, float accuracy)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var ws = pData.CurrentBusiness as Game.Businesses.WeaponShop;

            if (ws == null)
                return;

            pData.CurrentBusiness = null;

            player.Teleport(ws.PositionShootingRangeEnter.Position, false, Utils.Dimensions.Main, ws.PositionShootingRangeEnter.RotationZ, true);

            pData.InventoryBlocked = false;

            if (pData.UnequipActiveWeapon())
            {
                if (pData.Weapons[0] is Game.Items.Weapon weapon)
                {
                    pData.Weapons[0] = null;

                    weapon.Delete();

                    Game.Items.Inventory.ClearSlot(pData, Game.Items.Inventory.Groups.Weapons, 0);
                }
            }

            pData.GiveTakenItems();

            var diff = score < maxScore ? -1 : 1;

            var currentSkill = pData.Skills[PlayerData.SkillTypes.Shooting];

            diff = Utils.GetCorrectDiff(currentSkill, diff, 0, PlayerData.MaxSkills[PlayerData.SkillTypes.Shooting]);

            if (diff != 0)
            {
                currentSkill += diff;

                pData.UpdateSkill(PlayerData.SkillTypes.Shooting, diff);

                pData.Info.Achievements[PlayerData.Achievement.Types.SR1].UpdateProgress(pData.Info, currentSkill);
            }

            if (currentSkill == 100)
                pData.Info.Achievements[PlayerData.Achievement.Types.SR2].UpdateProgress(pData.Info, (int)Math.Round(accuracy));

            pData.Info.SetCooldown(PlayerData.CooldownTypes.ShootingRange);
        }

        [RemoteEvent("SRange::Enter::Shop")]
        private static void ShootingRangeEnterShop(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main)
                return;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var ws = Game.Businesses.Business.Get(id) as Game.Businesses.WeaponShop;

            if (ws == null)
                return;

            if (!ws.IsPlayerNearShootingRangeEnterPosition(pData))
                return;

            if (pData.HasCooldown(PlayerData.CooldownTypes.ShootingRange, 2))
                return;

            if (!ws.TryBuyShootingRange(pData))
                return;

            var pDim = Utils.GetPrivateDimension(player);

            pData.StopUseCurrentItem();
            player.DetachAllObjectsInHand();
            pData.StopAllAnims();

            player.Teleport(Game.Businesses.WeaponShop.ShootingRangePosition.Position, false, pDim, Game.Businesses.WeaponShop.ShootingRangePosition.RotationZ, true);

            player.TriggerEvent("SRange::Start", 0);

            pData.InventoryBlocked = true;

            pData.TakeWeapons();

            pData.GiveTempWeapon("w_pistol", -1);

            pData.CurrentBusiness = ws;
        }

        [RemoteProc("Business::BuyGov")]
        private static bool BuyGov(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main)
                return false;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var business = Game.Businesses.Business.Get(id);

            if (business == null || !business.IsBuyable)
                return false;

            if (!business.IsPlayerNearInfoPosition(pData))
                return false;

            if (business.Owner != null)
            {
                player.Notify("Business::AB");

                return true;
            }

            var res = business.BuyFromGov(pData);

            return res;
        }

        [RemoteProc("Business::SellGov")]
        private static bool SellGov(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main)
                return false;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var business = Game.Businesses.Business.Get(id);

            if (business == null || !business.IsBuyable || business.Owner != pData.Info)
                return false;

            if (!business.IsPlayerNearInfoPosition(pData))
                return false;

            business.SellToGov(true);

            return true;
        }

        [RemoteProc("Business::TCash")]
        private static ulong? TakeCash(Player player, int id, int amountI)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (amountI <= 0)
                return null;

            if (player.Dimension != Utils.Dimensions.Main)
                return null;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var business = Game.Businesses.Business.Get(id);

            if (business == null || business.Owner != pData.Info)
                return null;

            if (!business.IsPlayerNearInfoPosition(pData))
                return null;

            var amount = (ulong)amountI;

            ulong newBalance;

            if (!business.TryRemoveMoneyCash(amount, out newBalance, true))
                return null;

            ulong newPlayerBalance;

            if (!pData.TryAddCash(amount, out newPlayerBalance, true))
                return null;

            business.SetCash(newBalance);

            pData.SetCash(newPlayerBalance);

            return newBalance;
        }

        [RemoteProc("Business::SSIS")]
        private static bool SetIncassationState(Player player, int id, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main)
                return false;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var business = Game.Businesses.Business.Get(id);

            if (business == null || business.Owner != pData.Info)
                return false;

            if (!business.IsPlayerNearInfoPosition(pData))
                return false;

            if (business.IncassationState == state)
                return false;

            business.IncassationState = state;

            return true;
        }

        [RemoteProc("Business::SSMA")]
        private static bool SetMargin(Player player, int id, ushort marginC)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main)
                return false;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var business = Game.Businesses.Business.Get(id);

            if (business == null || business.Owner != pData.Info)
                return false;

            if (!business.IsPlayerNearInfoPosition(pData))
                return false;

            if (marginC > Game.Businesses.Business.MAX_MARGIN_CLIENT)
                return false;

            var margin = marginC / 100m + 1;

            if (business.Margin == margin)
                return false;

            business.SetMargin(margin);

            return true;
        }

        [RemoteProc("Business::ShowMenu")]
        private static JObject ShowMenu(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main)
                return null;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            var business = Game.Businesses.Business.Get(id);

            if (business == null || business.Owner != pData.Info)
                return null;

            if (!business.IsPlayerNearInfoPosition(pData))
                return null;

            return business.ToClientMenuObject();
        }

        [RemoteEvent("TuningShop::Enter")]
        public static void TuningShopEnter(Player player, int id, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentBusiness != null || player.Dimension != Utils.Dimensions.Main)
                return;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            var ts = Game.Businesses.Business.Get(id) as Game.Businesses.TuningShop;

            if (ts == null)
                return;

            if (!ts.IsPlayerNearInteractPosition(pData))
                return;

            if (player.Vehicle != veh)
            {
                if (player.Vehicle?.Exists != true)
                    return;

                var plVehData = player.Vehicle.GetMainData();

                if (plVehData == null)
                    return;

                if (veh.GetAttachmentData(plVehData.Vehicle)?.Type != Sync.AttachSystem.Types.VehicleTrailerObjBoat)
                    return;

                var exitPos = Game.Businesses.Business.GetNextExitProperty(ts);

                plVehData.Vehicle.Teleport(exitPos.Position, null, exitPos.RotationZ, false, Additional.AntiCheat.VehicleTeleportTypes.Default);
            }

            vData.DetachBoatFromTrailer();

            pData.CurrentTuningVehicle = vData;

            pData.UnequipActiveWeapon();

            pData.StopUseCurrentItem();
            player.DetachAllObjectsInHand();
            pData.StopAllAnims();

            var pDim = Utils.GetPrivateDimension(player);

            if (player.Vehicle == veh)
            {
                if (vData.IsAttachedTo is Vehicle attachedVeh)
                {
                    var exitPos = Game.Businesses.Business.GetNextExitProperty(ts);

                    attachedVeh.Teleport(exitPos.Position, null, exitPos.RotationZ, false, Additional.AntiCheat.VehicleTeleportTypes.Default);
                }

                veh.Teleport(ts.EnterProperties.Position, pDim, ts.EnterProperties.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.OnlyDriver);
            }
            else
            {
                veh.Teleport(ts.EnterProperties.Position, pDim, ts.EnterProperties.RotationZ, true, Additional.AntiCheat.VehicleTeleportTypes.Default);

                player.Teleport(ts.EnterProperties.Position, false, pDim, ts.EnterProperties.RotationZ, true);
            }

            vData.SetFreezePosition(ts.EnterProperties.Position);

            if (!vData.EngineOn)
                vData.EngineOn = true;

            if (!vData.LightsOn)
                vData.LightsOn = true;

            player.TriggerEvent("Shop::Show", (int)ts.Type, (float)ts.Margin, ts.EnterProperties.RotationZ, ts.GetVehicleClassMargin(vData.Data.Class), veh);

            pData.CurrentBusiness = ts;
        }

        [RemoteEvent("Business::Furn::Enter")]
        public static void BusinessFurnitureEnter(Player player, int id, int subTypeNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentBusiness != null || player.Dimension != Utils.Dimensions.Main)
                return;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var business = Game.Businesses.Business.Get(id) as Game.Businesses.FurnitureShop;

            if (business == null)
                return;

            if (!business.IsPlayerNearInteractPosition(pData))
                return;

            player.CloseAll(true);

            player.TriggerEvent("Shop::Show", (int)business.Type, (float)business.Margin, null, subTypeNum);

            pData.CurrentBusiness = business;
        }

        [RemoteEvent("Business::Enter")]
        public static void BusinessEnter(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentBusiness != null || player.Dimension != Utils.Dimensions.Main)
                return;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var business = Game.Businesses.Business.Get(id);

            if (business == null)
                return;

            if (!business.IsPlayerNearInteractPosition(pData))
                return;

            if (business is Game.Businesses.IEnterable enterable)
            {
                pData.StopUseCurrentItem();
                player.DetachAllObjectsInHand();
                pData.StopAllAnims();

                pData.UnequipActiveWeapon();

                pData.IsInvincible = true;

                player.Teleport(enterable.EnterProperties.Position, false, Utils.GetPrivateDimension(player), enterable.EnterProperties.RotationZ, true);

                if (business.Type == Game.Businesses.Business.Types.BarberShop)
                {
                    player.TriggerEvent("Shop::Show", (int)business.Type, (float)business.Margin, enterable.EnterProperties.RotationZ, pData.HairStyle, pData.HeadOverlays[1], pData.HeadOverlays[10], pData.HeadOverlays[2], pData.HeadOverlays[8], pData.HeadOverlays[5], pData.HeadOverlays[4]);
                }
                else
                {
                    player.TriggerEvent("Shop::Show", (int)business.Type, (float)business.Margin, enterable.EnterProperties.RotationZ);
                }

                pData.CurrentBusiness = business;
            }
            else
            {
                player.CloseAll(true);

                player.TriggerEvent("Shop::Show", (int)business.Type, (float)business.Margin);

                pData.CurrentBusiness = business;
            }
        }

        [RemoteEvent("Business::Exit")]
        public static void BusinessExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            var business = pData.CurrentBusiness;

            if (business == null)
                return;

            Sync.Players.ExitFromBuiness(pData, true);
        }

        [RemoteProc("Shop::Buy")]
        public static bool ShopBuy(Player player, string id, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (id == null)
                return false;

            var shop = pData.CurrentBusiness as Game.Businesses.Shop;

            if (shop == null)
                return false;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            if (!(shop is Game.Businesses.IEnterable))
            {
                if (player.Dimension != Utils.Dimensions.Main || !shop.IsPlayerNearInteractPosition(pData))
                    return false;
            }

            var res = shop.TryBuyItem(pData, useCash, id);

            return res;
        }

        [RemoteEvent("GasStation::Enter")]
        public static void GasStationEnter(Player player, Vehicle vehicle, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentBusiness != null)
                return;

            var gs = Game.Businesses.Business.Get(id) as Game.Businesses.GasStation;

            if (gs == null)
                return;

            if (player.Dimension != Utils.Dimensions.Main || Vector3.Distance(player.Position, gs.PositionInfo) > 50f)
                return;

            var vData = vehicle.GetMainData();

            if (vData == null)
                return;

            if (vData.FuelLevel == vData.Data.Tank)
            {
                player.Notify(vData.Data.FuelType == Game.Data.Vehicles.Vehicle.FuelTypes.Petrol ? "Vehicle::FOFP" : "Vehicle::FOFE");

                return;
            }

            player.CloseAll(true);

            player.TriggerEvent("GasStation::Show", (float)gs.Margin);

            pData.CurrentBusiness = gs;
        }

        [RemoteEvent("GasStation::Exit")]
        public static void GasStationExit(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentBusiness == null)
                return;

            pData.CurrentBusiness = null;
        }

        [RemoteEvent("GasStation::Buy")]
        public static void GasStationBuy(Player player, Vehicle vehicle, int fNum, int amountI, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (amountI <= 0 || !Enum.IsDefined(typeof(Game.Data.Vehicles.Vehicle.FuelTypes), fNum))
                return;

            var amount = (uint)amountI;

            var gs = pData.CurrentBusiness as Game.Businesses.GasStation;

            if (gs == null)
                return;

            var fType = (Game.Data.Vehicles.Vehicle.FuelTypes)fNum;

            var vData = vehicle.GetMainData();

            if (vData == null)
                return;

            if (vData.Data.FuelType != fType)
                return;

            if (vData.FuelLevel == vData.Data.Tank)
            {
                player.Notify(fType == Game.Data.Vehicles.Vehicle.FuelTypes.Petrol ? "Vehicle::FOFP" : "Vehicle::FOFE");

                return;
            }

            var newFuelLevel = vData.FuelLevel + amount;

            if (newFuelLevel > vData.Data.Tank)
            {
                amount = (uint)Math.Ceiling(vData.Data.Tank - vData.FuelLevel);

                newFuelLevel = vData.Data.Tank;
            }

            uint newMats;
            ulong newBalance, newPlayerBalance;

            if (!gs.TryProceedPayment(pData, useCash, Game.Businesses.GasStation.GasIds[fType], amount, out newMats, out newBalance, out newPlayerBalance))
                return;

            gs.ProceedPayment(pData, useCash, newMats, newBalance, newPlayerBalance);

            vData.FuelLevel = newFuelLevel;

            pData.CurrentBusiness = null;

            player.CloseAll(true);
        }
    }
}
