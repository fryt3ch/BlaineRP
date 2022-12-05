using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using static BCRPServer.Game.Bank;

namespace BCRPServer.Events.Players
{
    class Business : Script
    {
        [RemoteEvent("Business::Info")]
        public static void BusinessInfo(Player player, int id)
        {

        }

        [RemoteEvent("TuningShop::Enter")]
        public static void TuningShopEnter(Player player, int id, Vehicle veh)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentBusiness != null || pData.CurrentTuningVehicle != null)
                return;

            var vData = veh.GetMainData();

            if (vData == null)
                return;

            var ts = Game.Businesses.Business.Get(id) as Game.Businesses.TuningShop;

            if (ts == null)
                return;

            pData.CurrentBusiness = ts;
            pData.CurrentTuningVehicle = vData;

            pData.UnequipActiveWeapon();

            Sync.Players.DisableMicrophone(pData);

            foreach (var x in player.Vehicle.Occupants)
            {
                if (x is Player pV)
                {
                    if (pV == player)
                        continue;

                    pV.WarpOutOfVehicle();
                }
            }

            var pDim = Utils.GetPrivateDimension(player);

            veh.Teleport(ts.EnterProperties.Position, pDim, ts.EnterProperties.RotationZ, true);

            vData.EngineOn = true;
            vData.LightsOn = true;

            player.TriggerEvent("Shop::Show", (int)ts.Type, ts.Margin, ts.EnterProperties.RotationZ, veh);
        }

        [RemoteEvent("Business::Enter")]
        public static void BusinessEnter(Player player, int id)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.CurrentBusiness != null)
                return;

            var business = Game.Businesses.Business.Get(id);

            if (business == null)
                return;

            if (player.Dimension != Utils.Dimensions.Main || Vector3.Distance(player.Position, business.Position) > 20f)
                return;

            if (business is Game.Businesses.IEnterable enterable)
            {
                pData.CurrentBusiness = business;

                pData.UnequipActiveWeapon();

                Sync.Players.DisableMicrophone(pData);

                pData.IsInvincible = true;

                player.Teleport(enterable.EnterProperties.Position, false, Utils.GetPrivateDimension(player), enterable.EnterProperties.RotationZ, true);

                player.TriggerEvent("Shop::Show", (int)business.Type, business.Margin, enterable.EnterProperties.RotationZ);
            }
            else
            {
                pData.CurrentBusiness = business;

                player.CloseAll(true);

                player.TriggerEvent("Shop::Show", (int)business.Type, business.Margin);
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

            if (business is Game.Businesses.IEnterable enterable)
            {
                pData.IsInvincible = false;

                pData.CurrentBusiness = null;

                var t = Game.Businesses.Business.GetNextExitProperty(enterable);

                if (business is Game.Businesses.TuningShop ts)
                {
                    var veh = pData.CurrentTuningVehicle;

                    if (veh?.Vehicle?.Exists != true)
                    {
                        player.Teleport(t.Position, false, Utils.Dimensions.Main, t.RotationZ, true);
                    }
                    else
                    {
                        if (player.Vehicle != veh.Vehicle)
                        {
                            veh.Vehicle.Dimension = Utils.Dimensions.Main;
                            veh.Vehicle.Position = t.Position;

                            player.Teleport(t.Position, false, Utils.Dimensions.Main, t.RotationZ, true);

                            player.SetIntoVehicle(veh.Vehicle, 0);
                        }
                        else
                        {
                            veh.Vehicle.Teleport(t.Position, Utils.Dimensions.Main, t.RotationZ, true);
                        }

                        veh.Tuning.Apply(veh.Vehicle);
                    }

                    pData.CurrentTuningVehicle = null;

                    player.TriggerEvent("Shop::Close::Server");
                }
                else
                {
                    player.Teleport(t.Position, true, Utils.Dimensions.Main, t.RotationZ, true);

                    player.TriggerEvent("Shop::Close::Server");
                }
            }
            else
            {
                pData.CurrentBusiness = null;

                player.TriggerEvent("Shop::Close::Server");
            }
        }

        [RemoteProc("TuningShop::Buy")]
        private static bool TuningShopBuy(Player player, string item, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (item == null)
                return false;

            var ts = pData.CurrentBusiness as Game.Businesses.TuningShop;

            if (ts == null)
                return false;

            var vData = pData.CurrentTuningVehicle;

            if (vData == null)
                return false;

            var iData = item.Split('_');

            if (iData.Length <= 1)
                return false;

            var slot = ts.GetModSlot(iData[0]);

            if (slot is byte bSlot)
            {
                byte mNum;

                if (!byte.TryParse(iData[1], out mNum))
                    return false;

                if (iData[0] == "engine")
                {

                }
                else if (iData[0] == "brakes")
                {

                }
                else if (iData[0] == "trm")
                {

                }
                else if (iData[0] == "susp")
                {

                }
                else
                {
                    var price = ts.GetPrice(vData, iData[0], true);

                    if (price < 0)
                        return false;
                }

                if (mNum == 0)
                    mNum = 255;
                else
                    mNum--;

                vData.Tuning.Mods[bSlot] = mNum;
            }
            else
            {
                if (iData[0] == "wheel" || iData[0] == "rwheel")
                {
                    if (iData.Length != 3)
                        return false;

                    byte t, n;

                    if (!byte.TryParse(iData[1], out t) || !byte.TryParse(iData[2], out n))
                        return false;

                    if (vData.Data.Type == Game.Data.Vehicles.Vehicle.Types.Motorcycle)
                    {
                        vData.Tuning.WheelsType = 6;
                    }
                    else
                    {
                        if (t > 0)
                            t--;

                        vData.Tuning.WheelsType = t;
                    }

                    if (n == 0)
                        n = 255;
                    else
                        n--;

                    vData.Tuning.Mods[(byte)(iData[0] == "wheel" ? 23 : 24)] = n;
                }
                else if (iData[0] == "neon")
                {
                    if (iData.Length == 2)
                    {
                        vData.Tuning.NeonColour = null;
                    }
                    else if (iData.Length == 4)
                    {
                        byte r, g, b;

                        if (!byte.TryParse(iData[1], out r) || !byte.TryParse(iData[2], out g) || !byte.TryParse(iData[3], out b))
                            return false;

                        if (vData.Tuning.NeonColour == null)
                        {
                            vData.Tuning.NeonColour = new Utils.Colour(r, g, b, 255);
                        }
                        else
                        {
                            vData.Tuning.NeonColour.Red = r;
                            vData.Tuning.NeonColour.Green = g;
                            vData.Tuning.NeonColour.Blue = b;
                        }
                    }
                    else
                        return false;
                }
                else if (iData[0] == "tsmoke")
                {
                    if (iData.Length == 2)
                    {
                        vData.Tuning.TyresSmokeColour = null;
                    }
                    else if (iData.Length == 4)
                    {
                        byte r, g, b;

                        if (!byte.TryParse(iData[1], out r) || !byte.TryParse(iData[2], out g) || !byte.TryParse(iData[3], out b))
                            return false;

                        if (vData.Tuning.TyresSmokeColour == null)
                        {
                            vData.Tuning.TyresSmokeColour = new Utils.Colour(r, g, b, 255);
                        }
                        else
                        {
                            vData.Tuning.TyresSmokeColour.Red = r;
                            vData.Tuning.TyresSmokeColour.Green = g;
                            vData.Tuning.TyresSmokeColour.Blue = b;
                        }
                    }
                    else
                        return false;
                }
                else if (iData[0] == "colour")
                {
                    if (iData.Length != 7)
                        return false;

                    byte r1, g1, b1, r2, g2, b2;

                    if (!byte.TryParse(iData[1], out r1) || !byte.TryParse(iData[2], out g1) || !byte.TryParse(iData[3], out b1) || !byte.TryParse(iData[4], out r2) || !byte.TryParse(iData[5], out g2) || !byte.TryParse(iData[6], out b2))
                        return false;

                    vData.Tuning.Colour1.Red = r1;
                    vData.Tuning.Colour1.Green = g1;
                    vData.Tuning.Colour1.Blue = b1;

                    vData.Tuning.Colour2.Red = r2;
                    vData.Tuning.Colour2.Green = g2;
                    vData.Tuning.Colour2.Blue = b2;
                }
                else
                {
                    byte p;

                    if (!byte.TryParse(iData[1], out p))
                        return false;

                    if (iData[0] == "pearl")
                    {
                        if (p == 0)
                        {

                        }

                        vData.Tuning.PearlescentColour = p;
                    }
                    else if (iData[0] == "wcolour")
                    {
                        if (p == 0)
                        {

                        }

                        vData.Tuning.WheelsColour = p;
                    }
                    else if (iData[0] == "colourt")
                    {
                        vData.Tuning.ColourType = p;
                    }
                    else if (iData[0] == "tt")
                    {
                        vData.Tuning.Turbo = p == 1;
                    }
                    else if (iData[0] == "wtint")
                    {
                        vData.Tuning.WindowTint = p;
                    }
                    else if (iData[0] == "xenon")
                    {
                        vData.Tuning.Xenon = (sbyte)(p - 2);
                    }
                    else
                        return false;
                }
            }

            return true;
        }

        [RemoteEvent("Shop::Buy")]
        public static void ShopBuy(Player player, string id, int variation, int amount, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (amount <= 0 || variation < 0 || id == null)
                return;

            var shop = pData.CurrentBusiness as Game.Businesses.Shop;

            if (shop == null)
                return;

            int price = shop.GetPrice(id, true);

            if (price == -1)
                return;

            price *= amount;

            bool paid = ((Func<bool>)(() =>
            {
                if (shop.Owner != null)
                {
                    // operations with materials
                }

                if (useCash)
                    return pData.AddCash(-price, true);
                else
                    return false;

            })).Invoke();

            if (paid)
            {

            }
            else
                return;

            var item = Game.Items.Items.GiveItem(pData, id, variation, amount, false);

            if (item == null && shop is Game.Businesses.ClothesShop)
            {
                pData.Gifts.Add(Game.Items.Gift.Give(pData, Game.Items.Gift.Types.Item, id, variation, amount, Game.Items.Gift.SourceTypes.Shop, true, true));
            }
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

            if (player.Dimension != Utils.Dimensions.Main || Vector3.Distance(player.Position, gs.Position) > 50f)
                return;

            pData.CurrentBusiness = gs;

            player.CloseAll(true);

            player.TriggerEvent("GasStation::Show", gs.Margin);
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
        public static void GasStationBuy(Player player, Vehicle vehicle, int fNum, int amount, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (amount <= 0 || !Enum.IsDefined(typeof(Game.Data.Vehicles.Vehicle.FuelTypes), fNum))
                return;

            var gs = pData.CurrentBusiness as Game.Businesses.GasStation;

            if (gs == null)
                return;

            var fType = (Game.Data.Vehicles.Vehicle.FuelTypes)fNum;

            int price = gs.GetGasPrice(fType, true);

            if (price == -1)
                return;

            price *= amount;

            var vData = vehicle.GetMainData();

            if (vData == null)
                return;

            bool paid = ((Func<bool>)(() =>
            {
                if (gs.Owner != null)
                {
                    // operations with materials
                }

                if (useCash)
                    return pData.AddCash(-price, true);
                else
                    return false;

            })).Invoke();

            if (paid)
            {
                var newFuel = vData.FuelLevel + amount;

                if (newFuel > vData.Data.Tank)
                    newFuel = vData.Data.Tank;

                vData.FuelLevel = newFuel;

                player.CloseAll(true);

                pData.CurrentBusiness = null;
            }
            else
            {
                return;
            }
        }
    }
}
