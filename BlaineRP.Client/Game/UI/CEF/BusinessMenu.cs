using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.World;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;
using BlaineRP.Client.Sync;
using BlaineRP.Client.Utils;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Input.Core;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class BusinessMenu
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuBusiness);

        public static DateTime LastSent;

        private static int TempEscBind { get; set; }

        private const decimal MAX_MARGIN = 150m;
        private const decimal MAX_MARGIN_FARM = 100m;

        private static ExtraColshape CloseColshape { get; set; }

        public BusinessMenu()
        {
            TempEscBind = -1;

            LastSent = DateTime.MinValue;

            Events.Add("MenuBiz::Collect", async (args) =>
            {
                var biz = Player.LocalPlayer.GetData<Client.Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                var amountI = decimal.Parse(args[0].ToString());

                int amount;

                if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                if (LastSent.IsSpam(500, false, false))
                    return;

                LastSent = World.Core.ServerTime;

                var newAmountObj = await Events.CallRemoteProc("Business::TCash", biz.Id, amount);

                if (newAmountObj == null)
                    return;

                var newAmount = Utils.Convert.ToUInt64(newAmountObj);

                if (IsActive)
                {
                    CEF.Browser.Window.ExecuteJs("MenuBiz.setMoneyRegister", newAmount);
                }
            });

            Events.Add("MenuBiz::SellToGov", async (args) =>
            {
                var biz = Player.LocalPlayer.GetData<Client.Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                var approveContext = "MenuBizSellToGov";
                var approveTime = 5_000;

                if (CEF.Notification.HasApproveTimedOut(approveContext, World.Core.ServerTime, approveTime))
                {
                    if (LastSent.IsSpam(1_500, false, true))
                        return;

                    LastSent = World.Core.ServerTime;

                    CEF.Notification.SetCurrentApproveContext(approveContext, World.Core.ServerTime);

                    CEF.Notification.Show(CEF.Notification.Types.Question, Locale.Get("NOTIFICATION_HEADER_APPROVE"), string.Format(Locale.Notifications.Money.AdmitToSellGov1, Locale.Get("GEN_MONEY_0", Utils.Misc.GetGovSellPrice(biz.Price))), approveTime);
                }
                else
                {
                    CEF.Notification.ClearAll();

                    CEF.Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                    if ((bool)await Events.CallRemoteProc("Business::SellGov", biz.Id))
                    {
                        Close(true);
                    }
                }
            });

            Events.Add("MenuBiz::ExtraCharge", async (args) =>
            {
                var biz = Player.LocalPlayer.GetData<Client.Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                var margin = decimal.Parse(args[0].ToString());

                if (!margin.IsNumberValid(0, biz.Type == Client.Data.Locations.Business.Types.Farm ? MAX_MARGIN_FARM : MAX_MARGIN, out margin, true))
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

                LastSent = World.Core.ServerTime;

                if ((bool)await Events.CallRemoteProc("Business::SSMA", biz.Id, (ushort)margin))
                {
                    CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.BusinessNewMarginOwner0, margin));
                }
            });

            Events.Add("MenuBiz::CashCollect", async (args) =>
            {
                var state = (bool)args[0];

                var biz = Player.LocalPlayer.GetData<Client.Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                if (LastSent.IsSpam(500, false, false))
                    return;

                LastSent = World.Core.ServerTime;

                if ((bool)await Events.CallRemoteProc("Business::SSIS", biz.Id, state))
                {
                    CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), state ? Locale.Notifications.General.BusinessIncassationNowOn : Locale.Notifications.General.BusinessIncassationNowOff);

                    if (IsActive)
                    {
                        CEF.Browser.Window.ExecuteJs("MenuBiz.setCashCollect", state);
                    }
                }
            });

            Events.Add("MenuBiz::Delivery", async (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                var biz = Player.LocalPlayer.GetData<Client.Data.Locations.Business>("BusinessMenu::Business");

                if (biz == null)
                    return;

                var bizData1 = Player.LocalPlayer.GetData<object[]>("BusinessMenu::Business::Data1");

                if (bizData1 == null)
                    return;

                var id = (string)args[0];

                if (id == "pay")
                {
                    var amountI = decimal.Parse(args[1].ToString());

                    int amount;

                    if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                        return;

                    LastSent = World.Core.ServerTime;

                    var res = await Events.CallRemoteProc("Business::NDO", biz.Id, amount);

                    if (res == null)
                        return;

                    if (IsActive)
                    {
                        CEF.Browser.Window.ExecuteJs("MenuBiz.setMoneyAccount", res);

                        CEF.Browser.Window.ExecuteJs("MenuBiz.fillDelivery", true, amount, Locale.Get("BUSINESSMENU_ORDER_STATE_0"));
                    }
                }
                else if (id == "cancel")
                {
                    LastSent = World.Core.ServerTime;

                    var res = await Events.CallRemoteProc("Business::CAO", biz.Id);

                    if (res == null)
                        return;

                    if (IsActive)
                    {
                        CEF.Browser.Window.ExecuteJs("MenuBiz.setMoneyAccount", res);

                        CEF.Browser.Window.ExecuteJs("MenuBiz.fillDelivery", false, bizData1[0], bizData1[1]);
                    }
                }
            });

            Events.Add("MenuBiz::Close", (args) => Close(false));
        }

        public static async System.Threading.Tasks.Task Show(Client.Data.Locations.Business biz)
        {
            if (IsActive)
                return;

            var res = (JObject)await Events.CallRemoteProc("Business::ShowMenu", biz.Id);

            if (res == null)
                return;

            Players.CloseAll(true);

            var materialsBuyPrice = Utils.Convert.ToUInt32(res["MB"]);
            var deliveryPrice = Utils.Convert.ToUInt32(res["DP"]);

            Player.LocalPlayer.SetData("BusinessMenu::Business::Data1", new object[] { materialsBuyPrice, deliveryPrice });

            var info = new object[] { $"{biz.Name} #{biz.SubId}", biz.Name, biz.OwnerName ?? "null", biz.Price, biz.Rent, System.Math.Round(biz.Tax * 100, 0).ToString(), Utils.Convert.ToUInt64(res["C"]), Utils.Convert.ToUInt64(res["B"]), Utils.Convert.ToUInt32(res["M"]), materialsBuyPrice, Utils.Convert.ToUInt32(res["MS"]) };

            var manage = new List<object>() { new object[] { System.Math.Round((Utils.Convert.ToDecimal(res["MA"]) - 1m) * 100, 0), biz.Type == Client.Data.Locations.Business.Types.Farm ? MAX_MARGIN_FARM : MAX_MARGIN }, biz.Type == Client.Data.Locations.Business.Types.Farm, (bool)res["IS"], System.Math.Round(Utils.Convert.ToDecimal(res["IT"]) * 100, 0), };

            var delState = ((string)res["DS"]).Split('_');

            var delOrderAmount = uint.Parse(delState[0]);

            if (delOrderAmount > 0)
            {
                manage.AddRange(new object[] { true, delOrderAmount, Locale.Get(delState.Length > 1 ? "BUSINESSMENU_ORDER_STATE_1" : "BUSINESSMENU_ORDER_STATE_0") });
            }
            else
            {
                manage.AddRange(new object[] { false, materialsBuyPrice, deliveryPrice });
            }

            var currentDate = World.Core.ServerTime;

            var dates = new List<object>();

            var stats = RAGE.Util.Json.Deserialize<ulong[]>((string)res["S"]);

            for (int i = 0; i < stats.Length; i++)
                dates.Add(currentDate.AddDays(-(stats.Length - i - 1)).ToString("MM.dd"));

            await CEF.Browser.Render(Browser.IntTypes.MenuBusiness, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                }
            };

            CEF.Browser.Window.ExecuteJs("MenuBiz.draw", new object[] { new object[] { info, manage, new object[] { dates, stats } } });

            CEF.Cursor.Show(true, true);

            TempEscBind = Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            Player.LocalPlayer.SetData("BusinessMenu::Business", biz);
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            CEF.Browser.Render(Browser.IntTypes.MenuBusiness, false);

            CEF.Cursor.Show(false, false);

            Core.Unbind(TempEscBind);

            TempEscBind = -1;

            Player.LocalPlayer.ResetData("BusinessMenu::Business");
            Player.LocalPlayer.ResetData("BusinessMenu::Business::Data1");
        }
    }
}
