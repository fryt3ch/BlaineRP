using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF
{
    public class HouseMenu : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuHome);

        private static DateTime LastSent;

        public HouseMenu()
        {
            LastSent = DateTime.MinValue;

            Events.Add("HouseMenu::Show", (object[] args) =>
            {
                var data = RAGE.Util.Json.Deserialize<List<JObject>>((string)args[0]);

                Show(data.Select(x => new object[] { (uint)x["C"], (string)x["N"] + " " + x["S"], RAGE.Util.Json.Deserialize<bool[]>((string)x["P"]) }).ToArray());
            });
        }

        public static async System.Threading.Tasks.Task Show(object[] settlers)
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
                return;

            await CEF.Browser.Render(Browser.IntTypes.MenuHome, true, true);

            CEF.Cursor.Show(true, true);
        }

        public static void ShowRequest()
        {
            if (IsActive)
                return;

            if (Utils.IsAnyCefActive(true))
                return;

            if (!Player.LocalPlayer.HasData("House::CurrentHouse"))
            {
                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.House.NotInAnyHouse);

                return;
            }

            if (LastSent.IsSpam(1000, false, false))
                return;

            Events.CallRemote("HouseMenu::Show");

            LastSent = DateTime.Now;
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;


        }
    }
}
