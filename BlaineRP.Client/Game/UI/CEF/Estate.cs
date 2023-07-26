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
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Estate
    {
        public enum Types
        {
            Info = 0,
            Offer = 1,
            SellEstate = 2,
            SellVehicle = 3,
            SellBusiness = 4,
        }

        public static DateTime LastSent;

        public Estate()
        {
            Events.Add("Estate::Action",
                async (object[] args) =>
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

                    LastSent = World.Core.ServerTime;

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
                                    return;

                                if (CurrentType == Types.SellVehicle)
                                {
                                    List<(uint VID, Data.Vehicles.Vehicle Data)> vehs =
                                        Player.LocalPlayer.GetData<List<(uint VID, Game.Data.Vehicles.Vehicle Data)>>("Estate::CurrentData");

                                    if (vehs == null)
                                        return;

                                    if (num >= vehs.Count)
                                        return;

                                    uint vid = vehs[num].VID;

                                    Close(true);

                                    Offers.Request(player,
                                        OfferTypes.SellVehicle,
                                        new
                                        {
                                            VID = vid,
                                            Price = price,
                                        }
                                    );
                                }
                                else if (CurrentType == Types.SellBusiness)
                                {
                                    List<Business> businesses = Player.LocalPlayer.GetData<List<Business>>("Estate::CurrentData");

                                    if (businesses == null)
                                        return;

                                    if (num >= businesses.Count)
                                        return;

                                    int businessId = businesses[num].Id;
                                    Close(true);

                                    Offers.Request(player,
                                        OfferTypes.SellBusiness,
                                        new
                                        {
                                            BID = businessId,
                                            Price = price,
                                        }
                                    );
                                }
                                else if (CurrentType == Types.SellEstate)
                                {
                                    List<(PropertyTypes, uint)> ids = Player.LocalPlayer.GetData<List<(PropertyTypes, uint)>>("Estate::CurrentData");

                                    if (ids == null)
                                        return;

                                    if (num >= ids.Count)
                                        return;

                                    (PropertyTypes, uint) estate = ids[num];

                                    Close(true);

                                    Offers.Request(player,
                                        OfferTypes.SellEstate,
                                        new
                                        {
                                            EST = (int)estate.Item1,
                                            UID = estate.Item2,
                                            Price = price,
                                        }
                                    );
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
                            Events.CallRemote("Trade::Accept");
                        else
                            Close(true);
                    }
                    else if (CurrentType == Types.Info)
                    {
                        if (CurrentPropertyType == PropertyTypes.House || CurrentPropertyType == PropertyTypes.Apartments)
                        {
                            HouseBase houseBase = Player.LocalPlayer.GetData<HouseBase>("Estate::CurrentData");

                            if (houseBase == null)
                                return;

                            if (id == "enter")
                                Events.CallRemote("House::Enter", (int)houseBase.Type, houseBase.Id);
                            else if (id == "mail")
                                Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), "Пока нет писем!");
                            // todo
                            else if (id == "buy")
                                if ((bool)await Events.CallRemoteProc("House::BuyGov", (int)houseBase.Type, houseBase.Id))
                                {
                                    Close(true);

                                    return;
                                }

                            return;
                        }
                        else if (CurrentPropertyType == PropertyTypes.Business)
                        {
                            Business biz = Player.LocalPlayer.GetData<Business>("Estate::CurrentData");

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
                                BusinessMenu.Show(biz);
                            }
                        }
                    }
                }
            );

            Events.Add("Estate::Show",
                (args) =>
                {
                    var type = (Types)(int)args[0];

                    if (type == Types.Offer)
                    {
                        var subType = (int)args[1];

                        if (subType == 0)
                        {
                            Data.Vehicles.Vehicle vData = Data.Vehicles.Core.GetById((string)args[2]);
                            var vid = Utils.Convert.ToUInt32(args[3]);

                            var player = (Player)args[4];
                            var price = Utils.Convert.ToDecimal(args[5]);
                            var plate = (string)args[6];

                            ShowOfferVehicle(vData, player, price, vid, plate, true);
                        }
                        else if (subType == 1)
                        {
                            Business business = Business.All[(int)args[2]];

                            var player = (Player)args[3];
                            var price = Utils.Convert.ToDecimal(args[4]);

                            ShowOfferBusiness(business, player, price, true);
                        }
                        else if (subType == 2 || subType == 3)
                        {
                            var id = Utils.Convert.ToUInt32(args[2]);

                            HouseBase houseBase = subType == 2 ? (HouseBase)House.All[id] : (HouseBase)Apartments.All[id];

                            var player = (Player)args[3];
                            var price = Utils.Convert.ToDecimal(args[4]);

                            ShowOfferHouseBase(houseBase, player, price, true);
                        }
                        else if (subType == 4)
                        {
                            Garage garage = Garage.All[Utils.Convert.ToUInt32(args[2])];

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
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.Estate);

        public static Types? CurrentType { get; set; }

        public static PropertyTypes? CurrentPropertyType { get; set; }

        private static int EscBindIdx { get; set; } = -1;

        private static ExtraColshape CloseColshape { get; set; }

        private static Player TargetPlayer { get; set; }

        public static async System.Threading.Tasks.Task ShowVehicleInfo(string id, decimal vid, int engine, bool turbo, bool showCursor = true)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (IsActive || CurrentType != null)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            Data.Vehicles.Vehicle vData = Data.Vehicles.Core.GetById(id);

            if (vData == null)
                return;

            CurrentType = Types.Info;
            CurrentPropertyType = PropertyTypes.Vehicle;

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            Browser.Window.ExecuteJs("Estate.draw",
                "info",
                "veh_info",
                null,
                new object[]
                {
                    vData.Name,
                    vid.ToString(),
                    (engine == 255 ? 0 : engine + 1).ToString(),
                    turbo,
                    vData.HasCruiseControl,
                    vData.HasAutoPilot,
                    $"{(int)vData.FuelType}_{vData.Tank}",
                    vData.TrunkData == null ? "0_0" : $"{vData.TrunkData.Slots}_{vData.TrunkData.MaxWeight}",
                }
            );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
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
            CurrentPropertyType = houseBase.Type == Estates.Core.HouseTypes.House ? PropertyTypes.House : PropertyTypes.Apartments;

            Player.LocalPlayer.SetData("Estate::CurrentData", houseBase);

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                },
            };

            if (houseBase is House rHouse)
                Browser.Window.ExecuteJs("Estate.draw",
                    "info",
                    "house",
                    houseBase.Id,
                    new object[]
                    {
                        houseBase.OwnerName,
                        houseBase.Price,
                        houseBase.Tax,
                        (int)houseBase.RoomType,
                        rHouse.GarageType == null ? "0" : ((int)rHouse.GarageType).ToString(),
                    },
                    houseBase.OwnerName == null ? null : (bool?)pData.OwnedHouses.Contains(houseBase)
                );
            else if (houseBase is Apartments rApartments)
                Browser.Window.ExecuteJs("Estate.draw",
                    "info",
                    "flat",
                    rApartments.NumberInRoot + 1,
                    new object[]
                    {
                        houseBase.OwnerName,
                        houseBase.Price,
                        houseBase.Tax,
                        (int)houseBase.RoomType,
                    },
                    houseBase.OwnerName == null ? null : (bool?)pData.OwnedApartments.Contains(houseBase)
                );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
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

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close();
                },
            };

            Browser.Window.ExecuteJs("Estate.draw",
                "info",
                "biz",
                null,
                new object[]
                {
                    $"{business.Name} #{business.SubId}",
                    business.Name,
                    business.OwnerName,
                    business.Price,
                    business.Rent,
                    Math.Round(business.Tax * 100, 0).ToString(),
                },
                business.OwnerName == null ? null : (bool?)pData.OwnedBusinesses.Contains(business)
            );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
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

            foreach (House x in pData.OwnedHouses.ToList())
            {
                estToSell.Add(new object[]
                    {
                        "House",
                        Locale.General.PropertyHouseString,
                        Utils.Game.Misc.GetStreetName(x.Position),
                        x.Class.ToString(),
                        x.Price,
                        x.Id,
                    }
                );

                estIds.Add((PropertyTypes.House, x.Id));
            }

            foreach (Apartments x in pData.OwnedApartments.ToList())
            {
                estToSell.Add(new object[]
                    {
                        "Flat",
                        Locale.General.PropertyApartmentsString,
                        ApartmentsRoot.All[x.RootId].Name,
                        x.Class.ToString(),
                        x.Price,
                        x.NumberInRoot + 1,
                    }
                );

                estIds.Add((PropertyTypes.Apartments, x.Id));
            }

            foreach (Garage x in pData.OwnedGarages.ToList())
            {
                estToSell.Add(new object[]
                    {
                        "Garage",
                        Locale.General.PropertyGarageString,
                        GarageRoot.All[x.RootId].Name,
                        x.ClassType.ToString(),
                        x.Price,
                        x.NumberInRoot + 1,
                    }
                );

                estIds.Add((PropertyTypes.Garage, x.Id));
            }

            if (estToSell.Count == 0)
                return;

            Player.LocalPlayer.SetData("Estate::CurrentData", estIds);

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            Browser.Window.ExecuteJs("Estate.draw",
                "sell",
                "sell",
                null,
                new object[]
                {
                    targetPlayer.GetName(true, false, true),
                    estToSell,
                }
            );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

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

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            if (houseBase is House house)
                Browser.Window.ExecuteJs("Estate.draw",
                    "offer",
                    "house",
                    house.Id,
                    new object[]
                    {
                        targetPlayer.GetName(true, false, true),
                        price,
                        house.Price,
                        house.Tax,
                        (int)house.RoomType,
                        house.GarageType is Garage.Types gType ? (int)gType : 0,
                    }
                );
            else if (houseBase is Apartments aps)
                Browser.Window.ExecuteJs("Estate.draw",
                    "offer",
                    "flat",
                    aps.NumberInRoot + 1,
                    new object[]
                    {
                        targetPlayer.GetName(true, false, true),
                        price,
                        aps.Price,
                        aps.Tax,
                        (int)aps.RoomType,
                    }
                );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
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

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            Browser.Window.ExecuteJs("Estate.draw",
                "offer",
                "garage",
                garage.NumberInRoot + 1,
                new object[]
                {
                    targetPlayer.GetName(true, false, true),
                    price,
                    garage.Price,
                    garage.Tax,
                    (int)garage.RootId + 1,
                    (int)garage.Type,
                }
            );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
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

            var estToSell = vehs.Select(x => new object[]
                                     {
                                         x.Data.GetEstateSvgName(),
                                         x.Data.BrandName,
                                         x.Data.SubName,
                                         x.Data.Class.ToString(),
                                         x.Data.GovPrice,
                                     }
                                 )
                                .ToList();

            if (estToSell.Count == 0)
                return;

            Player.LocalPlayer.SetData("Estate::CurrentData", vehs);

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            Browser.Window.ExecuteJs("Estate.draw",
                "sell",
                "sell",
                null,
                new object[]
                {
                    targetPlayer.GetName(true, false, true),
                    estToSell,
                }
            );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            TargetPlayer = targetPlayer;
        }

        public static async System.Threading.Tasks.Task ShowOfferVehicle(Game.Data.Vehicles.Vehicle vData,
                                                                         Player targetPlayer,
                                                                         decimal price,
                                                                         uint vid,
                                                                         string plate,
                                                                         bool showCursor = true)
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

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            Browser.Window.ExecuteJs("Estate.draw",
                "offer",
                "veh",
                null,
                new object[]
                {
                    targetPlayer.GetName(true, false, true),
                    price,
                    vData.Name,
                    vData.GovPrice,
                    vid,
                    plate ?? Locale.Get("DOCS_VEHPASS_NOPLATE"),
                }
            );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
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

            var estToSell = businesses.Select(x => new object[]
                                           {
                                               "Biz",
                                               x.Name,
                                               Utils.Game.Misc.GetStreetName(x.InfoColshape.Position),
                                               Locale.General.PropertyBusinessClass,
                                               x.Price,
                                               x.SubId,
                                           }
                                       )
                                      .ToList();

            if (estToSell.Count == 0)
                return;

            Player.LocalPlayer.SetData("Estate::CurrentData", businesses);

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            Browser.Window.ExecuteJs("Estate.draw",
                "sell",
                "sell",
                null,
                new object[]
                {
                    targetPlayer.GetName(true, false, true),
                    estToSell,
                }
            );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

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

            await Browser.Render(Browser.IntTypes.Estate, true, true);

            Browser.Window.ExecuteJs("Estate.draw",
                "offer",
                "biz",
                null,
                new object[]
                {
                    targetPlayer.GetName(true, false, true),
                    price,
                    business.Name,
                    business.Price,
                    business.Rent,
                    Math.Round(business.Tax * 100, 1),
                }
            );

            if (showCursor)
                Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            if (CurrentType == Types.Offer && ignoreTimeout)
                Offers.Reply(Offers.ReplyTypes.Deny);

            CloseColshape?.Destroy();

            CloseColshape = null;

            Browser.Render(Browser.IntTypes.Estate, false, false);

            Cursor.Show(false, false);

            CurrentType = null;
            CurrentPropertyType = null;

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;

            Player.LocalPlayer.ResetData("Estate::CurrentData");
        }
    }
}