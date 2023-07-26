using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Scripts.Sync;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class BusinessMenu
    {
        private const decimal MAX_MARGIN = 150m;
        private const decimal MAX_MARGIN_FARM = 100m;

        public static DateTime LastSent;

        public BusinessMenu()
        {
            TempEscBind = -1;

            LastSent = DateTime.MinValue;

            Events.Add("MenuBiz::Collect",
                async (args) =>
                {
                    Business biz = Player.LocalPlayer.GetData<Business>("BusinessMenu::Business");

                    if (biz == null)
                        return;

                    var amountI = decimal.Parse(args[0].ToString());

                    int amount;

                    if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                        return;

                    if (LastSent.IsSpam(500, false, false))
                        return;

                    LastSent = World.Core.ServerTime;

                    object newAmountObj = await Events.CallRemoteProc("Business::TCash", biz.Id, amount);

                    if (newAmountObj == null)
                        return;

                    var newAmount = Utils.Convert.ToUInt64(newAmountObj);

                    if (IsActive)
                        Browser.Window.ExecuteJs("MenuBiz.setMoneyRegister", newAmount);
                }
            );

            Events.Add("MenuBiz::SellToGov",
                async (args) =>
                {
                    Business biz = Player.LocalPlayer.GetData<Business>("BusinessMenu::Business");

                    if (biz == null)
                        return;

                    var approveContext = "MenuBizSellToGov";
                    var approveTime = 5_000;

                    if (Notification.HasApproveTimedOut(approveContext, World.Core.ServerTime, approveTime))
                    {
                        if (LastSent.IsSpam(1_500, false, true))
                            return;

                        LastSent = World.Core.ServerTime;

                        Notification.SetCurrentApproveContext(approveContext, World.Core.ServerTime);

                        Notification.Show(Notification.Types.Question,
                            Locale.Get("NOTIFICATION_HEADER_APPROVE"),
                            string.Format(Locale.Notifications.Money.AdmitToSellGov1, Locale.Get("GEN_MONEY_0", Utils.Misc.GetGovSellPrice(biz.Price))),
                            approveTime
                        );
                    }
                    else
                    {
                        Notification.ClearAll();

                        Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                        if ((bool)await Events.CallRemoteProc("Business::SellGov", biz.Id))
                            Close(true);
                    }
                }
            );

            Events.Add("MenuBiz::ExtraCharge",
                async (args) =>
                {
                    Business biz = Player.LocalPlayer.GetData<Business>("BusinessMenu::Business");

                    if (biz == null)
                        return;

                    var margin = decimal.Parse(args[0].ToString());

                    if (!margin.IsNumberValid(0, biz.Type == BusinessTypes.Farm ? MAX_MARGIN_FARM : MAX_MARGIN, out margin, true))
                        return;

                    if (LastSent.IsSpam(1000, false, false))
                        return;

                    LastSent = World.Core.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Business::SSMA", biz.Id, (ushort)margin))
                        Notification.Show(Notification.Types.Information,
                            Locale.Get("NOTIFICATION_HEADER_DEF"),
                            string.Format(Locale.Notifications.General.BusinessNewMarginOwner0, margin)
                        );
                }
            );

            Events.Add("MenuBiz::CashCollect",
                async (args) =>
                {
                    var state = (bool)args[0];

                    Business biz = Player.LocalPlayer.GetData<Business>("BusinessMenu::Business");

                    if (biz == null)
                        return;

                    if (LastSent.IsSpam(500, false, false))
                        return;

                    LastSent = World.Core.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Business::SSIS", biz.Id, state))
                    {
                        Notification.Show(Notification.Types.Information,
                            Locale.Get("NOTIFICATION_HEADER_DEF"),
                            state ? Locale.Notifications.General.BusinessIncassationNowOn : Locale.Notifications.General.BusinessIncassationNowOff
                        );

                        if (IsActive)
                            Browser.Window.ExecuteJs("MenuBiz.setCashCollect", state);
                    }
                }
            );

            Events.Add("MenuBiz::Delivery",
                async (args) =>
                {
                    if (LastSent.IsSpam(500, false, false))
                        return;

                    Business biz = Player.LocalPlayer.GetData<Business>("BusinessMenu::Business");

                    if (biz == null)
                        return;

                    object[] bizData1 = Player.LocalPlayer.GetData<object[]>("BusinessMenu::Business::Data1");

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

                        object res = await Events.CallRemoteProc("Business::NDO", biz.Id, amount);

                        if (res == null)
                            return;

                        if (IsActive)
                        {
                            Browser.Window.ExecuteJs("MenuBiz.setMoneyAccount", res);

                            Browser.Window.ExecuteJs("MenuBiz.fillDelivery", true, amount, Locale.Get("BUSINESSMENU_ORDER_STATE_0"));
                        }
                    }
                    else if (id == "cancel")
                    {
                        LastSent = World.Core.ServerTime;

                        object res = await Events.CallRemoteProc("Business::CAO", biz.Id);

                        if (res == null)
                            return;

                        if (IsActive)
                        {
                            Browser.Window.ExecuteJs("MenuBiz.setMoneyAccount", res);

                            Browser.Window.ExecuteJs("MenuBiz.fillDelivery", false, bizData1[0], bizData1[1]);
                        }
                    }
                }
            );

            Events.Add("MenuBiz::Close", (args) => Close(false));
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.MenuBusiness);

        private static int TempEscBind { get; set; }

        private static ExtraColshape CloseColshape { get; set; }

        public static async System.Threading.Tasks.Task Show(Business biz)
        {
            if (IsActive)
                return;

            var res = (JObject)await Events.CallRemoteProc("Business::ShowMenu", biz.Id);

            if (res == null)
                return;

            Players.CloseAll(true);

            var materialsBuyPrice = Utils.Convert.ToUInt32(res["MB"]);
            var deliveryPrice = Utils.Convert.ToUInt32(res["DP"]);

            Player.LocalPlayer.SetData("BusinessMenu::Business::Data1",
                new object[]
                {
                    materialsBuyPrice,
                    deliveryPrice,
                }
            );

            var info = new object[]
            {
                $"{biz.Name} #{biz.SubId}",
                biz.Name,
                biz.OwnerName ?? "null",
                biz.Price,
                biz.Rent,
                Math.Round(biz.Tax * 100, 0).ToString(),
                Utils.Convert.ToUInt64(res["C"]),
                Utils.Convert.ToUInt64(res["B"]),
                Utils.Convert.ToUInt32(res["M"]),
                materialsBuyPrice,
                Utils.Convert.ToUInt32(res["MS"]),
            };

            var manage = new List<object>()
            {
                new object[]
                {
                    Math.Round((Utils.Convert.ToDecimal(res["MA"]) - 1m) * 100, 0),
                    biz.Type == BusinessTypes.Farm ? MAX_MARGIN_FARM : MAX_MARGIN,
                },
                biz.Type == BusinessTypes.Farm,
                (bool)res["IS"],
                Math.Round(Utils.Convert.ToDecimal(res["IT"]) * 100, 0),
            };

            string[] delState = ((string)res["DS"]).Split('_');

            var delOrderAmount = uint.Parse(delState[0]);

            if (delOrderAmount > 0)
                manage.AddRange(new object[]
                    {
                        true,
                        delOrderAmount,
                        Locale.Get(delState.Length > 1 ? "BUSINESSMENU_ORDER_STATE_1" : "BUSINESSMENU_ORDER_STATE_0"),
                    }
                );
            else
                manage.AddRange(new object[]
                    {
                        false,
                        materialsBuyPrice,
                        deliveryPrice,
                    }
                );

            DateTime currentDate = World.Core.ServerTime;

            var dates = new List<object>();

            ulong[] stats = RAGE.Util.Json.Deserialize<ulong[]>((string)res["S"]);

            for (var i = 0; i < stats.Length; i++)
            {
                dates.Add(currentDate.AddDays(-(stats.Length - i - 1)).ToString("MM.dd"));
            }

            await Browser.Render(Browser.IntTypes.MenuBusiness, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                },
            };

            Browser.Window.ExecuteJs("MenuBiz.draw",
                new object[]
                {
                    new object[]
                    {
                        info,
                        manage,
                        new object[]
                        {
                            dates,
                            stats,
                        },
                    },
                }
            );

            Cursor.Show(true, true);

            TempEscBind = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false));

            Player.LocalPlayer.SetData("BusinessMenu::Business", biz);
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            Browser.Render(Browser.IntTypes.MenuBusiness, false);

            Cursor.Show(false, false);

            Input.Core.Unbind(TempEscBind);

            TempEscBind = -1;

            Player.LocalPlayer.ResetData("BusinessMenu::Business");
            Player.LocalPlayer.ResetData("BusinessMenu::Business::Data1");
        }
    }
}