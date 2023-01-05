using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF
{
    public class BusinessMenu : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuBusiness);

        public static DateTime LastSent;

        private static int TempEscBind { get; set; }

        public enum DeliveryStates
        {
            NotOferred = 0,
            Offered,
            Incoming,
        }

        public BusinessMenu()
        {
            TempEscBind = -1;

            LastSent = DateTime.MinValue;

            Events.Add("MenuBiz::Collect", async (args) =>
            {
                var amount = args[0] is string str0 ? int.Parse(str0) : (int)args[0];

                if (!IsActive)
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

/*                var newAmount = (int)await Events.CallRemoteProc("Business::TakeCash", amount);

                if (newAmount < 0)
                    return;*/
            });

            Events.Add("MenuBiz::SellToGov", async (args) =>
            {
                if (LastSent.IsSpam(1000, false, false))
                    return;

                var biz = Player.LocalPlayer.GetData<Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                LastSent = DateTime.Now;

                if ((bool)await Events.CallRemoteProc("Business::SellGov", biz.Id))
                {
                    Close(true);
                }
            });

            Events.Add("MenuBiz::ExtraCharge", (args) =>
            {
                var margin = (int)args[0] / 100f;
            });

            Events.Add("MenuBiz::CashCollect", (args) =>
            {
                var state = (bool)args[0];
            });

            Events.Add("MenuBiz::Delivery", (args) =>
            {
                var id = (string)args[0];

                if (id == "pay")
                {
                    var amount = args[0] is string str0 ? int.Parse(str0) : (int)args[0];
                }
                else if (id == "cancel")
                {

                }
            });

            Events.Add("MenuBiz::Close", (args) => Close(false));
        }

        public static async System.Threading.Tasks.Task Show(Data.Locations.Business biz)
        {
            if (IsActive)
                return;

            var res = (JObject)await Events.CallRemoteProc("Business::ShowMenu", biz.Id);

            if (res == null)
                return;

            Sync.Players.CloseAll(true);

            var info = new object[] { $"{biz.Name} #{biz.SubId}", biz.Name, biz.OwnerName ?? "null", biz.Price, biz.Rent, Math.Round(biz.Tax * 100, 1), (int)res["C"], (int)res["B"], (int)res["M"] };

            var manage = new List<object>() { Math.Floor(((float)res["MA"] - 1) * 100), false, (bool)res["IS"], Math.Floor((float)res["IT"] * 100), };

            var delState = (DeliveryStates)(int)res["DS"];

            if (delState > DeliveryStates.NotOferred)
            {
                manage.AddRange(new object[] { true, (int)res["DO"], delState.ToString() });
            }
            else
            {
                manage.AddRange(new object[] { false, (int)res["MB"], (int)res["DP"] });
            }

            var currentDate = Utils.GetServerTime();

            var dates = new List<object>();

            var stats = RAGE.Util.Json.Deserialize<int[]>((string)res["S"]);

            for (int i = 0; i < stats.Length; i++)
                dates.Add(currentDate.AddDays(-(stats.Length - i - 1)).ToString("MM.dd"));

            await CEF.Browser.Render(Browser.IntTypes.MenuBusiness, true, true);

            CEF.Browser.Window.ExecuteJs("MenuBiz.draw", new object[] { new object[] { info, manage, new object[] { dates, stats } } });

            CEF.Cursor.Show(true, true);

            TempEscBind = RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            Player.LocalPlayer.SetData("BusinessMenu::Business", biz);
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            if (!ignoreTimeout && LastSent.IsSpam(500, false, false))
                return;

            LastSent = DateTime.Now;

            CEF.Browser.Render(Browser.IntTypes.MenuBusiness, false);

            CEF.Cursor.Show(false, false);

            RAGE.Input.Unbind(TempEscBind);

            TempEscBind = -1;

            Player.LocalPlayer.ResetData("BusinessMenu::Business");
        }
    }
}
