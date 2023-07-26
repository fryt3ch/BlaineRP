using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Estates.Garages;
using BlaineRP.Client.Game.Estates.Houses;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Input.Core;

namespace BlaineRP.Client.UI.CEF
{
    [Script(int.MaxValue)]
    public class Estate
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.Estate);

        public static DateTime LastSent;

        public static Types? CurrentType { get; set; }

        public static PropertyTypes? CurrentPropertyType { get; set; }

        private static int EscBindIdx { get; set; } = -1;

        private static ExtraColshape CloseColshape { get; set; }

        private static Player TargetPlayer { get; set; }

        public enum Types
        {
            Info = 0,
            Offer = 1,
            SellEstate = 2,
            SellVehicle = 3,
            SellBusiness = 4,
        }

        public Estate()
        {
            Events.Add("Estate::Action", async (object[] args) =>
            {
                if (!IsActive)
                    return;

                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var id = (string)args[0]; // enter/mail/buy/cancel/accept/decline

                if (!Player.LocalPlayer.HasData("Estate::CurrentData"))
                    return;

                if (LastSent.IsSpam(250, false, true))
                    return;

                LastSent = Game.World.Core.ServerTime;

                if (CurrentType == Types.SellVehicle || CurrentType == Types.SellBusiness || CurrentType == Types.SellEstate)
                {
                    if (id == "cancel")
                    {
                        Close(true);

                        return;
                    }
                    else if (id == "accept")
                    {
                        var price = Utils.Convert.ToDecimal(args[1]);

                        if (!price.IsNumberValid<decimal>(1, int.MaxValue, out _, true))
                            return;

                        var num = (int)args[2];

                        if (TargetPlayer is Player player)
                        {
                            if (price <= 0)
                            {
                                return;
                            }

                            if (CurrentType == Types.SellVehicle)
                            {
                                var vehs = Player.LocalPlayer.GetData<List<(uint VID, Game.Data.Vehicles.Vehicle Data)>>("Estate::CurrentData");

                                if (vehs == null)
                                    return;

                                if (num >= vehs.Count)
                                    return;

                                var vid = vehs[num].VID;

                                Close(true);

                                Offers.Request(player, OfferTypes.SellVehicle, new { VID = vid, Price = price, });
                            }
                            else if (CurrentType == Types.SellBusiness)
                            {
                                var businesses = Player.LocalPlayer.GetData<List<Business>>("Estate::CurrentData");

                                if (businesses == null)
                                    return;

                                if (num >= businesses.Count)
                                    return;

                                var businessId = businesses[num].Id;
                                Close(true);

                                Offers.Request(player, OfferTypes.SellBusiness, new { BID = businessId, Price = price, });
                            }
                            else if (CurrentType == Types.SellEstate)
                            {
                                var ids = Player.LocalPlayer.GetData<List<(PropertyTypes, uint)>>("Estate::CurrentData");

                                if (ids == null)
                                    return;

                                if (num >= ids.Count)
                                    return;

                                var estate = ids[num];

                                Close(true);

                                Offers.Request(player, OfferTypes.SellEstate, new { EST = (int)estate.Item1, UID = estate.Item2, Price = price, });
                            }
                        }
                        else
                        {
                            Close(true);

                            return;
                        }
                    }
                }
                else if (CurrentType == Types.Offer)
                {
                    if (id == "accept")
                    {
                        Events.CallRemote("Trade::Accept");
                    }
                    else
                    {
                        Close(true);
                    }
                }
                else if (CurrentType == Types.Info)
                {
                    if (CurrentPropertyType == PropertyTypes.House || CurrentPropertyType == PropertyTypes.Apartments)
                    {
                        var houseBase = Player.LocalPlayer.GetData<HouseBase>("Estate::CurrentData");

                        if (houseBase == null)
                            return;

                        if (id == "enter")
                        {
                            Events.CallRemote("House::Enter", (int)houseBase.Type, houseBase.Id);
                        }
                        else if (id == "mail")
                        {
                            CEF.Notification.Show(CEF.Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Пока нет писем!");

                            // todo
                        }
                        else if (id == "buy")
                        {
                            if ((bool)await Events.CallRemoteProc("House::BuyGov", (int)houseBase.Type, houseBase.Id))
                            {
                                Close(true);

                                return;
                            }
                        }

                        return;
                    }
                    else if (CurrentPropertyType == PropertyTypes.Business)
                    {
                        var biz = Player.LocalPlayer.GetData<Business>("Estate::CurrentData");

                        if (biz == null)
                            return;

                        if (id == "buy")
                        {
                            if ((bool)await Events.CallRemoteProc("Business::BuyGov", biz.Id))
                            {
                                Close(true);

                                return;
                            }
                        }
                        else if (id == "manage")
                        {
                            CEF.BusinessMenu.Show(biz);
                        }
                    }
                }
            });

            Events.Add("Estate::Show", (args) =>
            {
                var type = (Types)(int)args[0];

                if (type == Types.Offer)
                {
                    var subType = (int)args[1];

                    if (subType == 0)
                    {
                        var vData = Game.Data.Vehicles.Core.GetById((string)args[2]);
                        var vid = Utils.Convert.ToUInt32(args[3]);

                        var player = (Player)args[4];
                        var price = Utils.Convert.ToDecimal(args[5]);
                        var plate = (string)args[6];

                        ShowOfferVehicle(vData, player, price, vid, plate, true);
                    }
                    else if (subType == 1)
                    {
                        var business = Business.All[(int)args[2]];

                        var player = (Player)args[3];
                        var price = Utils.Convert.ToDecimal(args[4]);

                        ShowOfferBusiness(business, player, price, true);
                    }
                    else if (subType == 2 || subType == 3)
                    {
                        var id = Utils.Convert.ToUInt32(args[2]);

                        var houseBase = subType == 2 ? (HouseBase)House.All[id] : (HouseBase)Apartments.All[id];

                        var player = (Player)args[3];
                        var price = Utils.Convert.ToDecimal(args[4]);

                        ShowOfferHouseBase(houseBase, player, price, true);
                    }
                    else if (subType == 4)
                    {
                        var garage = Garage.All[Utils.Convert.ToUInt32(args[2])];

                        var player = (Player)args[3];
                        var price = Utils.Convert.ToDecimal(args[4]);

                        ShowOfferGarage(garage, player, price, true);
                    }
                }
                else if (type == Types.Info)
                {
                    var modelId = (string)args[0];
                    var vid = Utils.Convert.ToDecimal(args[1]);
                    var engine = (int)args[2];
                    var turbo = (bool)args[3];

                    ShowVehicleInfo(modelId, vid, engine, turbo, true);
                }
            });
        }

        public static async System.Threading.Tasks.Task ShowVehicleInfo(string id, decimal vid, int engine, bool turbo, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            var vData = Game.Data.Vehicles.Core.GetById(id);

            if (vData == null)
                return;

            CurrentType = Types.Info;
            CurrentPropertyType = PropertyTypes.Vehicle;

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "veh_info", null, new object[] { vData.Name, vid.ToString(), (engine == 255 ? 0 : engine + 1).ToString(), turbo, vData.HasCruiseControl, vData.HasAutoPilot, $"{(int)vData.FuelType}_{vData.Tank}", vData.TrunkData == null ? "0_0" : $"{vData.TrunkData.Slots}_{vData.TrunkData.MaxWeight}" });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static async System.Threading.Tasks.Task ShowHouseBaseInfo(HouseBase houseBase, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.Info;
            CurrentPropertyType = houseBase.Type == Game.Estates.Core.HouseTypes.House ? PropertyTypes.House : PropertyTypes.Apartments;

            Player.LocalPlayer.SetData("Estate::CurrentData", houseBase);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                }
            };

            if (houseBase is House rHouse)
            {
                CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "house", houseBase.Id, new object[] { houseBase.OwnerName, houseBase.Price, houseBase.Tax, (int)houseBase.RoomType, rHouse.GarageType == null ? "0" : ((int)rHouse.GarageType).ToString() }, houseBase.OwnerName == null ? null : (bool?)pData.OwnedHouses.Contains(houseBase));
            }
            else if (houseBase is Apartments rApartments)
            {
                CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "flat", rApartments.NumberInRoot + 1, new object[] { houseBase.OwnerName, houseBase.Price, houseBase.Tax, (int)houseBase.RoomType }, houseBase.OwnerName == null ? null : (bool?)pData.OwnedApartments.Contains(houseBase));
            }

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static async System.Threading.Tasks.Task ShowBusinessInfo(Business business, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.Info;
            CurrentPropertyType = PropertyTypes.Business;

            Player.LocalPlayer.SetData("Estate::CurrentData", business);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                }
            };

            CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "biz", null, new object[] { $"{business.Name} #{business.SubId}", business.Name, business.OwnerName, business.Price, business.Rent, System.Math.Round(business.Tax * 100, 0).ToString() }, business.OwnerName == null ? null : (bool?)pData.OwnedBusinesses.Contains(business));

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static async System.Threading.Tasks.Task ShowSellEstate(Player targetPlayer, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var tData = PlayerData.GetData(targetPlayer);

            if (tData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.SellEstate;

            var estToSell = new List<object>();

            var estIds = new List<(PropertyTypes, uint)>();

            foreach (var x in pData.OwnedHouses.ToList())
            {
                estToSell.Add(new object[] { "House", Locale.General.PropertyHouseString, Utils.Game.Misc.GetStreetName(x.Position), x.Class.ToString(), x.Price, x.Id });

                estIds.Add((PropertyTypes.House, x.Id));
            }

            foreach (var x in pData.OwnedApartments.ToList())
            {
                estToSell.Add(new object[] { "Flat", Locale.General.PropertyApartmentsString, ApartmentsRoot.All[x.RootId].Name, x.Class.ToString(), x.Price, x.NumberInRoot + 1 });

                estIds.Add((PropertyTypes.Apartments, x.Id));
            }

            foreach (var x in pData.OwnedGarages.ToList())
            {
                estToSell.Add(new object[] { "Garage", Locale.General.PropertyGarageString, GarageRoot.All[x.RootId].Name, x.ClassType.ToString(), x.Price, x.NumberInRoot + 1 });

                estIds.Add((PropertyTypes.Garage, x.Id));
            }

            if (estToSell.Count == 0)
                return;

            Player.LocalPlayer.SetData("Estate::CurrentData", estIds);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "sell", "sell", null, new object[] { targetPlayer.GetName(true, false, true), estToSell });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            TargetPlayer = targetPlayer;
        }

        public static async System.Threading.Tasks.Task ShowOfferHouseBase(HouseBase houseBase, Player targetPlayer, decimal price, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.Offer;

            Player.LocalPlayer.SetData("Estate::CurrentData", true);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            if (houseBase is House house)
            {
                CEF.Browser.Window.ExecuteJs("Estate.draw", "offer", "house", house.Id, new object[] { targetPlayer.GetName(true, false, true), price, house.Price, house.Tax, (int)house.RoomType, house.GarageType is Garage.Types gType ? (int)gType : 0 });
            }
            else if (houseBase is Apartments aps)
            {
                CEF.Browser.Window.ExecuteJs("Estate.draw", "offer", "flat", aps.NumberInRoot + 1, new object[] { targetPlayer.GetName(true, false, true), price, aps.Price, aps.Tax, (int)aps.RoomType });
            }

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static async System.Threading.Tasks.Task ShowOfferGarage(Garage garage, Player targetPlayer, decimal price, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.Offer;

            Player.LocalPlayer.SetData("Estate::CurrentData", true);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "offer", "garage", garage.NumberInRoot + 1, new object[] { targetPlayer.GetName(true, false, true), price, garage.Price, garage.Tax, (int)garage.RootId + 1, (int)garage.Type });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static async System.Threading.Tasks.Task ShowSellVehicle(Player targetPlayer, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var tData = PlayerData.GetData(targetPlayer);

            if (tData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.SellVehicle;

            var vehs = pData.OwnedVehicles.ToList();

            var estToSell = vehs.Select(x => new object[] { x.Data.GetEstateSvgName(), x.Data.BrandName, x.Data.SubName, x.Data.Class.ToString(), x.Data.GovPrice }).ToList();

            if (estToSell.Count == 0)
                return;

            Player.LocalPlayer.SetData("Estate::CurrentData", vehs);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "sell", "sell", null, new object[] { targetPlayer.GetName(true, false, true), estToSell });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            TargetPlayer = targetPlayer;
        }

        public static async System.Threading.Tasks.Task ShowOfferVehicle(Game.Data.Vehicles.Vehicle vData, Player targetPlayer, decimal price, uint vid, string plate, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.Offer;

            Player.LocalPlayer.SetData("Estate::CurrentData", true);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "offer", "veh", null, new object[] { targetPlayer.GetName(true, false, true), price, vData.Name, vData.GovPrice, vid, plate ?? Locale.Get("DOCS_VEHPASS_NOPLATE") });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static async System.Threading.Tasks.Task ShowSellBusiness(Player targetPlayer, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var tData = PlayerData.GetData(targetPlayer);

            if (tData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.SellBusiness;

            var businesses = pData.OwnedBusinesses.ToList();

            var estToSell = businesses.Select(x => new object[] { "Biz", x.Name, Utils.Game.Misc.GetStreetName(x.InfoColshape.Position), Locale.General.PropertyBusinessClass, x.Price, x.SubId }).ToList();

            if (estToSell.Count == 0)
                return;

            Player.LocalPlayer.SetData("Estate::CurrentData", businesses);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "sell", "sell", null, new object[] { targetPlayer.GetName(true, false, true), estToSell });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            TargetPlayer = targetPlayer;
        }

        public static async System.Threading.Tasks.Task ShowOfferBusiness(Business business, Player targetPlayer, decimal price, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            CurrentType = Types.Offer;

            Player.LocalPlayer.SetData("Estate::CurrentData", true);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "offer", "biz", null, new object[] { targetPlayer.GetName(true, false, true), price, business.Name, business.Price, business.Rent, System.Math.Round(business.Tax * 100, 1) });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            if (CurrentType == Types.Offer && ignoreTimeout)
                Offers.Reply(Offers.ReplyTypes.Deny);

            CloseColshape?.Destroy();

            CloseColshape = null;

            CEF.Browser.Render(Browser.IntTypes.Estate, false, false);

            CEF.Cursor.Show(false, false);

            CurrentType = null;
            CurrentPropertyType = null;

            Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            Player.LocalPlayer.ResetData("Estate::CurrentData");
        }
    }

    [Script(int.MaxValue)]
    public class EstateAgency
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.EstateAgency);

        private static int EscBindIdx { get; set; } = int.MinValue;

        public static bool WasShowed => EscBindIdx != int.MinValue;

        private static int AgencyId { get; set; }
        private static int PosId { get; set; }

        private static ExtraColshape CloseColshape { get; set; }

        public EstateAgency()
        {
            Events.Add("EstAgency::Close", (args) => Close(false));

            Events.Add("EstAgency::GPS", async (args) =>
            {
                var idS = ((string)args[0])?.Split('_');

                if (idS == null || idS.Length < 2)
                    return;

                if (idS[0] == "h")
                {
                    var houseId = uint.Parse(idS[1]);

                    var houseData = House.All[houseId];

                    if (houseData.OwnerName != null)
                    {
                        CEF.Notification.Show("House::AB");

                        return;
                    }

                    if (Estate.LastSent.IsSpam(1000, false, true))
                        return;

                    Estate.LastSent = Game.World.Core.ServerTime;

                    var res = (bool)await Events.CallRemoteProc("EstAgency::GPS", AgencyId, PosId, (byte)0);

                    if (res)
                    {
                        Game.Helpers.Blips.Core.CreateGPS(houseData.Position, Settings.App.Static.MainDimension, true);
                    }
                }
                else if (idS[0] == "a")
                {
                    var apsId = uint.Parse(idS[1]);

                    var apsData = Apartments.All[apsId];

                    if (apsData.OwnerName != null)
                    {
                        CEF.Notification.Show("House::AB");

                        return;
                    }

                    Game.Helpers.Blips.Core.CreateGPS(ApartmentsRoot.All[apsData.RootId].PositionEnter, Settings.App.Static.MainDimension, true, $"\n\nЭтаж: {ApartmentsRoot.All[apsData.RootId].Shell.StartFloor + apsData.FloorIdx}, кв. {apsData.NumberInRoot + 1}");
                }
                else if (idS[0] == "g")
                {
                    var garageId = uint.Parse(idS[1]);

                    var garageData = Garage.All[garageId];

                    if (garageData.OwnerName != null)
                    {
                        CEF.Notification.Show("House::AB");

                        return;
                    }

                    Game.Helpers.Blips.Core.CreateGPS(GarageRoot.All[garageData.RootId].EnterColshape.Position, Settings.App.Static.MainDimension, true, $"\n\nНомер гаража в комплексе: {garageData.NumberInRoot + 1}");
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(int agencyId, int posId, decimal houseGpsPrice, decimal apsGpsPrice, decimal garageGpsPrice)
        {
            if (IsActive)
                return;

            // id, name, price, tax, rooms, garage capacity
            var houses = House.All.Where(x => x.Value.OwnerName == null).Select(x => new object[] { $"h_{x.Key}", $"{Utils.Game.Misc.GetStreetName(x.Value.Position)} [#{x.Key}]", x.Value.Price, x.Value.Tax, (int)x.Value.RoomType, x.Value.GarageType == null ? 0 : (int)x.Value.GarageType });

            // id, name, price, tax, rooms
            var apartments = new List<object>();

            foreach (var x in ApartmentsRoot.All.Values)
            {
                var arName = x.Name;

                uint counter = 1;

                foreach (var y in x.AllApartments)
                {
                    if (y.OwnerName == null)
                        apartments.Add(new object[] { $"a_{y.Id}", string.Format(Locale.General.Blip.ApartmentsOwnedBlip, arName, counter), y.Price, y.Tax, (uint)y.RoomType });

                    counter++;
                }
            }

            // id, name, price, tax, garage capacity, complex num
            var garages = new List<object>();

            uint gCounter = 1;

            foreach (var x in GarageRoot.All.Values)
            {
                uint counter = 1;

                foreach (var y in x.AllGarages)
                {
                    if (y.OwnerName == null)
                        garages.Add(new object[] { $"g_{y.Id}", string.Format(Locale.General.Blip.GarageOwnedBlip, gCounter, counter), y.Price, y.Tax, (uint)y.Type, gCounter });
                }

                gCounter++;
            }

            await CEF.Browser.Render(Browser.IntTypes.EstateAgency, true, true);

            AgencyId = agencyId;
            PosId = posId;

            CEF.Browser.Window.ExecuteJs("EstAgency.draw", new object[] { new object[] { houses, apartments, garages, new object[] { houseGpsPrice, apsGpsPrice, garageGpsPrice } } });

            if (!WasShowed)
            {
                CEF.Browser.Window.ExecuteCachedJs("EstAgency.selectOption", "-info", 0);
            }
            else
            {
                CEF.Browser.Window.ExecuteCachedJs("EstAgency.selectOption", "", 0);
            }

            CEF.Cursor.Show(true, true);

            EscBindIdx = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                }
            };
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            CEF.Browser.Render(Browser.IntTypes.EstateAgency, false);

            CEF.Cursor.Show(false, false);

            Core.Unbind(EscBindIdx);

            EscBindIdx = -1;
        }
    }
}
