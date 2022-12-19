using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace BCRPClient.CEF
{
    class Estate : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.Estate);

        private static DateTime LastSent;

        public static Types? CurrentType { get; set; }

        public static Sync.Players.PropertyTypes? CurrentPropertyType { get; set; }

        private static List<int> TempBinds { get; set; }

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
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            Events.Add("Estate::Action", async (object[] args) =>
            {
                if (!IsActive)
                    return;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var id = (string)args[0]; // enter/mail/buy/cancel/accept/decline

                if (!Player.LocalPlayer.HasData("Estate::CurrentData"))
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

                LastSent = DateTime.Now;

                if (CurrentType == Types.SellVehicle || CurrentType == Types.SellBusiness || CurrentType == Types.SellEstate)
                {
                    if (id == "cancel")
                    {
                        Close(true);

                        return;
                    }
                    else if (id == "accept")
                    {
                        var price = (int)args[1];
                        var num = (int)args[2];

                        if (BCRPClient.Interaction.CurrentEntity is Player player)
                        {
                            if (price <= 0)
                            {
                                return;
                            }

                            if (CurrentType == Types.SellVehicle)
                            {
                                var vehs = Player.LocalPlayer.GetData<List<(uint VID, Data.Vehicles.Vehicle Data)>>("Estate::CurrentData");

                                if (vehs == null)
                                    return;

                                if (num >= vehs.Count)
                                    return;

                                var vid = vehs[num].VID;

                                Close(true);

                                Sync.Offers.Request(player, Sync.Offers.Types.SellVehicle, $"{vid}_{price}");
                            }
                            else if (CurrentType == Types.SellBusiness)
                            {
                                var businesses = Player.LocalPlayer.GetData<List<Data.Locations.Business>>("Estate::CurrentData");

                                if (businesses == null)
                                    return;

                                if (num >= businesses.Count)
                                    return;

                                var businessId = businesses[num].Id;

                                Close(true);

                                Sync.Offers.Request(player, Sync.Offers.Types.SellBusiness, $"{businessId}_{price}");
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
                    if (CurrentPropertyType == Sync.Players.PropertyTypes.House)
                    {
                        var house = Player.LocalPlayer.GetData<Data.Locations.House>("Estate::CurrentData");

                        if (house == null)
                            return;

                        if (id == "enter")
                        {
                            Events.CallRemote("House::Enter", house.Id);
                        }
                        else if (id == "mail")
                        {

                        }
                        else if (id == "buy")
                        {

                        }

                        return;
                    }
                    else if (CurrentPropertyType == Sync.Players.PropertyTypes.Business)
                    {
                        var biz = Player.LocalPlayer.GetData<Data.Locations.Business>("Estate::CurrentData");

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

            Events.Add("EstAgency::Close", (object[] args) => Agency.Close(false));

            Events.Add("Estate::Show", (args) =>
            {
                var type = (Types)(int)args[0];

                if (type == Types.Offer)
                {
                    var subType = (int)args[1];

                    if (subType == 0)
                    {
                        var vData = Data.Vehicles.GetById((string)args[2]);
                        var vid = (uint)(int)args[3];

                        var player = (Player)args[4];
                        var price = (int)args[5];
                        var plate = (string)args[6];

                        ShowOfferVehicle(vData, player, price, vid, plate, true);
                    }
                    else if (subType == 1)
                    {
                        var business = Data.Locations.Business.All[(int)args[2]];

                        var player = (Player)args[3];
                        var price = (int)args[4];

                        ShowOfferBusiness(business, player, price, true);
                    }
                }
            });
        }

        public static async System.Threading.Tasks.Task ShowHouseInfo(Data.Locations.House house, bool showCursor = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            CurrentType = Types.Info;
            CurrentPropertyType = Sync.Players.PropertyTypes.House;

            Player.LocalPlayer.SetData("Estate::CurrentData", house);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "house", house.Id, new object[] { house.OwnerName, house.Price, house.Tax, (int)house.RoomType, house.GarageType == null ? "0" : ((int)house.GarageType).ToString() }, house.OwnerName == null ? null : (bool?)pData.OwnedHouses.Contains(house));

            if (showCursor)
                CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static async System.Threading.Tasks.Task ShowBusinessInfo(Data.Locations.Business business, bool showCursor = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            CurrentType = Types.Info;
            CurrentPropertyType = Sync.Players.PropertyTypes.Business;

            Player.LocalPlayer.SetData("Estate::CurrentData", business);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "biz", null, new object[] { $"{business.Name} #{business.SubId}", business.Name, business.OwnerName, business.Price, business.Rent, Math.Round(business.Tax * 100, 1) }, business.OwnerName == null ? null : (bool?)pData.OwnedBusinesses.Contains(business));

            if (showCursor)
                CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static async System.Threading.Tasks.Task ShowSellEstate(Player targetPlayer, bool showCursor = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var tData = Sync.Players.GetData(targetPlayer);

            if (tData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            CurrentType = Types.SellEstate;

            var estToSell = new List<object>();

            estToSell.AddRange(pData.OwnedHouses.SelectMany(x => new object[] { Sync.Players.PropertyTypes.House.ToString(), Locale.General.PropertyHouseString, Utils.GetStreetName(x.Position), x.Class.ToString(), x.Price, x.Id }));

            if (estToSell.Count == 0)
                return;

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "sell", "sell", null, new object[] { targetPlayer.GetName(true, false, true), estToSell });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static async System.Threading.Tasks.Task ShowSellVehicle(Player targetPlayer, bool showCursor = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var tData = Sync.Players.GetData(targetPlayer);

            if (tData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.IsAnyCefActive(true))
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

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static async System.Threading.Tasks.Task ShowOfferVehicle(Data.Vehicles.Vehicle vData, Player targetPlayer, int price, uint vid, string plate, bool showCursor = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            CurrentType = Types.Offer;

            Player.LocalPlayer.SetData("Estate::CurrentData", true);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "offer", "veh", null, new object[] { targetPlayer.GetName(true, false, true), price, vData.Name, vData.GovPrice, vid, plate ?? Locale.General.Documents.VehiclePassportNoPlate });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(true)));
        }

        public static async System.Threading.Tasks.Task ShowSellBusiness(Player targetPlayer, bool showCursor = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var tData = Sync.Players.GetData(targetPlayer);

            if (tData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            CurrentType = Types.SellBusiness;

            var businesses = pData.OwnedBusinesses.ToList();

            var estToSell = businesses.Select(x => new object[] { "Biz", x.Name, Utils.GetStreetName(x.InfoColshape.Position), Locale.General.PropertyBusinessClass, x.Price, x.SubId }).ToList();

            if (estToSell.Count == 0)
                return;

            Player.LocalPlayer.SetData("Estate::CurrentData", businesses);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "sell", "sell", null, new object[] { targetPlayer.GetName(true, false, true), estToSell });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static async System.Threading.Tasks.Task ShowOfferBusiness(Data.Locations.Business business, Player targetPlayer, int price, bool showCursor = true)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            CurrentType = Types.Offer;

            Player.LocalPlayer.SetData("Estate::CurrentData", true);

            await CEF.Browser.Render(Browser.IntTypes.Estate, true, true);

            CEF.Browser.Window.ExecuteJs("Estate.draw", "offer", "biz", null, new object[] { targetPlayer.GetName(true, false, true), price, business.Name, business.Price, business.Rent, Math.Round(business.Tax * 100, 1) });

            if (showCursor)
                CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(true)));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            if (CurrentType == Types.Offer && ignoreTimeout)
                Sync.Offers.Reply(Sync.Offers.ReplyTypes.Deny);

            CEF.Browser.Render(Browser.IntTypes.Estate, false, false);

            CEF.Cursor.Show(false, false);

            CurrentType = null;
            CurrentPropertyType = null;

            foreach (var x in TempBinds)
                RAGE.Input.Unbind(x);

            TempBinds.Clear();

            Player.LocalPlayer.ResetData("Estate::CurrentData");
        }

        public class Agency
        {
            public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.EstateAgency);

            private static bool WasShowed { get; set; }

            public static async System.Threading.Tasks.Task Show()
            {
                if (IsActive)
                    return;

                // id, name, price, tax, rooms, garage capacity
                var houses = Data.Locations.House.All.Where(x => x.Value.OwnerName == null).Select(x => new object[] { $"h_{x.Key}", $"{Utils.GetStreetName(x.Value.Position)} [#{x.Key}]", x.Value.Price, x.Value.Tax, (int)x.Value.RoomType, x.Value.GarageType == null ? 0 : (int)x.Value.GarageType });

                // id, name, price, tax, rooms
                var apartments = new object[] {};

                // id, name, price, tax, garage capacity
                var garages = new object[] {};

                await CEF.Browser.Render(Browser.IntTypes.EstateAgency, true, true);

                CEF.Browser.Window.ExecuteJs("EstAgency.draw", new object[] { new object[] { houses, apartments, garages } });

                //CEF.Browser.Window.ExecuteCachedJs("EstAgency.selectOption", LastNavId);

                if (!WasShowed)
                {
                    WasShowed = true;

                    CEF.Browser.Window.ExecuteCachedJs("EstAgency.selectOption", "-info", 0);
                }
                else
                {
                    CEF.Browser.Window.ExecuteCachedJs("EstAgency.selectOption", "", 0);
                }

                CEF.Cursor.Show(true, true);
            }

            public static void Close(bool ignoreTimeout = false)
            {
                if (!IsActive)
                    return;

                CEF.Browser.Render(Browser.IntTypes.EstateAgency, false);

                CEF.Cursor.Show(false, false);
            }
        }
    }
}
