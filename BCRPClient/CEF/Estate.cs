using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
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

            CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "house", house.Id, new object[] { house.OwnerName, house.Price, 90, (int)house.RoomType, "0" }, pData.OwnedHouses.Contains(house));

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

            CEF.Browser.Window.ExecuteJs("Estate.draw", "info", "biz", null, new object[] { business.Name, business.Name, business.OwnerName, 50000, 15, 15 }, pData.OwnedBusinesses.Contains(business));

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
    }
}
