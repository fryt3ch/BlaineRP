using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;
using static BCRPClient.Additional.Camera;

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

        private const decimal MAX_MARGIN = 150m;

        public BusinessMenu()
        {
            TempEscBind = -1;

            LastSent = DateTime.MinValue;

            Events.Add("MenuBiz::Collect", async (args) =>
            {
                var biz = Player.LocalPlayer.GetData<Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                var amountI = decimal.Parse(args[0].ToString());

                int amount;

                if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                if (LastSent.IsSpam(500, false, false))
                    return;

                LastSent = DateTime.Now;

                var newAmountObj = await Events.CallRemoteProc("Business::TCash", biz.Id, amount);

                if (newAmountObj == null)
                    return;

                var newAmount = newAmountObj.ToUInt64();

                if (IsActive)
                {
                    CEF.Browser.Window.ExecuteJs("MenuBiz.setMoneyRegister", newAmount);
                }
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

            Events.Add("MenuBiz::ExtraCharge", async (args) =>
            {
                var biz = Player.LocalPlayer.GetData<Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                var margin = decimal.Parse(args[0].ToString());

                if (!margin.IsNumberValid(0, MAX_MARGIN, out margin, true))
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

                LastSent = DateTime.Now;

                if ((bool)await Events.CallRemoteProc("Business::SSMA", biz.Id, (ushort)margin))
                {
                    CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, string.Format(Locale.Notifications.General.BusinessNewMarginOwner0, margin));
                }
            });

            Events.Add("MenuBiz::CashCollect", async (args) =>
            {
                var state = (bool)args[0];

                var biz = Player.LocalPlayer.GetData<Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                if (LastSent.IsSpam(500, false, false))
                    return;

                LastSent = DateTime.Now;

                if ((bool)await Events.CallRemoteProc("Business::SSIS", biz.Id, state))
                {
                    CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, state ? Locale.Notifications.General.BusinessIncassationNowOn : Locale.Notifications.General.BusinessIncassationNowOff);

                    if (IsActive)
                    {
                        CEF.Browser.Window.ExecuteJs("MenuBiz.setCashCollect", state);
                    }
                }
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

            var info = new object[] { $"{biz.Name} #{biz.SubId}", biz.Name, biz.OwnerName ?? "null", biz.Price, biz.Rent, Math.Round(biz.Tax * 100, 0).ToString(), res["C"].ToUInt64(), res["B"].ToUInt64(), res["M"].ToUInt32(), res["MB"].ToUInt32(), res["MS"].ToUInt32() };

            var manage = new List<object>() { new object[] { Math.Round((res["MA"].ToDecimal() - 1m) * 100, 0), MAX_MARGIN }, false, (bool)res["IS"], Math.Round(res["IT"].ToDecimal() * 100, 0), };

            var delState = (DeliveryStates)(int)res["DS"];

            if (delState > DeliveryStates.NotOferred)
            {
                manage.AddRange(new object[] { true, (int)res["DO"], delState.ToString() });
            }
            else
            {
                manage.AddRange(new object[] { false, res["MB"].ToUInt32(), (int)res["DP"] });
            }

            var currentDate = Utils.GetServerTime();

            var dates = new List<object>();

            var stats = RAGE.Util.Json.Deserialize<ulong[]>((string)res["S"]);

            for (int i = 0; i < stats.Length; i++)
                dates.Add(currentDate.AddDays(-(stats.Length - i - 1)).ToString("MM.dd"));

            await CEF.Browser.Render(Browser.IntTypes.MenuBusiness, true, true);

            CEF.Browser.Window.ExecuteJs("MenuBiz.draw", new object[] { new object[] { info, manage, new object[] { dates, stats } } });

            CEF.Cursor.Show(true, true);

            TempEscBind = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

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

            KeyBinds.Unbind(TempEscBind);

            TempEscBind = -1;

            Player.LocalPlayer.ResetData("BusinessMenu::Business");
        }
    }
}
