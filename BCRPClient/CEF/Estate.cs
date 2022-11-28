using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Offer,
            Sell,
        }

        public Estate()
        {
            LastSent = DateTime.MinValue;

            TempBinds = new List<int>();

            Events.Add("Estate::Action", (object[] args) =>
            {
                if (!IsActive)
                    return;

                var id = (string)args[0]; // enter/mail/buy/cancel/accept/decline

                if (!Player.LocalPlayer.HasData("Estate::CurrentData"))
                    return;

                if (CurrentPropertyType == Sync.Players.PropertyTypes.House)
                {
                    var house = Player.LocalPlayer.GetData<Data.Locations.House>("Estate::CurrentData");

                    if (house == null)
                        return;

                    if (id == "enter")
                    {
                        if (!LastSent.IsSpam(1000, false, false))
                        {
                            Events.CallRemote("House::Enter", house.Id);
                        }
                    }
                    else if (id == "mail")
                    {

                    }
                    else if (id == "buy")
                    {

                    }

                    return;
                }
            });

            Events.Add("EstAgency::Close", (object[] args) => Agency.Close(false));

            //RAGE.Input.Bind(RAGE.Ui.VirtualKeys.B, true, () => Agency.Show());
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

            CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "house", house.Id, new object[] { house.OwnerName, house.Price, 90, (int)house.RoomType, house.GarageType == null ? "0" : ((int)house.GarageType).ToString() }, house.OwnerName == null ? null : (bool?)pData.OwnedHouses.Contains(house));

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

            CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "biz", null, new object[] { $"{business.Name} #{business.SubId}", business.Name, business.OwnerName, 50000, 15, 15 }, business.OwnerName == null ? null : (bool?)pData.OwnedBusinesses.Contains(business));

            if (showCursor)
                CEF.Cursor.Show(true, true);

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

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
                var houses = Data.Locations.House.All.Where(x => x.Value.OwnerName == null).Select(x => new object[] { $"h_{x.Key}", $"{Utils.GetStreetName(x.Value.Position)} [#{x.Key}]", x.Value.Price, 90, (int)x.Value.RoomType, x.Value.GarageType == null ? 0 : (int)x.Value.GarageType });

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
