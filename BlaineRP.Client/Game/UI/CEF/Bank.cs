using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using BlaineRP.Client.Input;
using BlaineRP.Client.Sync;
using Core = BlaineRP.Client.Input.Core;

namespace BlaineRP.Client.CEF
{
    [Script(int.MaxValue)]
    public class Bank
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.MenuBank);

        private static DateTime LastSent;

        public enum TariffTypes
        {
            /// <summary>Стандартный</summary>
            Standart = 0,
            /// <summary>Расширенный</summary>
            StandartPlus,
            /// <summary>Продвинутый</summary>
            Supreme,
        }

        private static List<int> TempBinds { get; set; }

        private static Additional.ExtraColshape CloseColshape { get; set; }

        public Bank()
        {
            TempBinds = new List<int>();

            LastSent = DateTime.MinValue;

            Events.Add("MenuBank::Close", (object[] args) =>
            {
                Close(false);
            });

            Events.Add("MenuBank::Cash2Debet", async (object[] args) =>
            {
                if (!IsActive)
                    return;

                bool state = (bool)args[0];

                if (LastSent.IsSpam(1000, false, true))
                    return;

                LastSent = Sync.World.ServerTime;

                if ((bool)await Events.CallRemoteProc("Bank::Savings::ToDebitSett", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), state))
                {
                    if (!IsActive)
                        return;

                    CEF.Browser.Window.ExecuteJs("MenuBank.setCash2Debet", state);
                }
            });

            Events.Add("MenuBank::Action", async (object[] args) =>
            {
                if (!IsActive)
                    return;

                var id = (string)args[0];

                var aId = (string)args[1];

                var amountI = Utils.Convert.ToDecimal(args[2]);

                int amount;

                if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                if (LastSent.IsSpam(1000, false, true))
                    return;

                LastSent = Sync.World.ServerTime;

                if (aId == "transfer")
                {
                    var cid = Utils.Convert.ToUInt32(args[3]);

                    var approveContext = $"BankSendToPlayer_{cid}_{amount}";
                    var approveTime = 5_000;

                    if (CEF.Notification.HasApproveTimedOut(approveContext, Sync.World.ServerTime, approveTime))
                    {
                        if (LastSent.IsSpam(1_500, false, true))
                            return;

                        LastSent = Sync.World.ServerTime;

                        CEF.Notification.SetCurrentApproveContext(approveContext, Sync.World.ServerTime);

                        if ((bool)await Events.CallRemoteProc("Bank::Debit::Send", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), cid, amount, true)) ;
                        {

                        }
                    }
                    else
                    {
                        CEF.Notification.ClearAll();

                        CEF.Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                        await Events.CallRemoteProc("Bank::Debit::Send", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), cid, amount, false);
                    }
                }
                else
                {
                    if (id == "debit")
                    {
                        Events.CallRemote("Bank::Debit::Operation", false, Player.LocalPlayer.GetData<int>("CurrentBank::Id"), aId == "deposit", amount);
                    }
                    else if (id == "savings")
                    {
                        var res = ((string)await Events.CallRemoteProc("Bank::Savings::Operation", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), aId == "deposit", amount))?.Split('_');

                        if (res == null)
                            return;

                        var newSavingsValue = Utils.Convert.ToUInt64(res[0]);
                        var newMinSavingsValue = Utils.Convert.ToUInt64(res[1]);

                        if (IsActive)
                        {
                            CEF.Browser.Window.ExecuteJs("MenuBank.setSavingsBal", newSavingsValue);
                        }
                    }
                }
            });

            Events.Add("MenuBank::BuyTariff", (object[] args) =>
            {
                if (!IsActive)
                    return;

                var tarrif = (TariffTypes)(int)args[0];

                if (!LastSent.IsSpam(1000, false, false))
                {
                    Events.CallRemote("Bank::Tariff::Buy", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), (int)tarrif);

                    LastSent = Sync.World.ServerTime;
                }
            });

            Events.Add("MenuBank::Show", async (object[] args) =>
            {
                Players.CloseAll(true);

                Player.LocalPlayer.SetData("CurrentBank::Id", (int)args[0]);

                var tariffNum = (int)args[1];

                if (tariffNum < 0)
                {
                    await Show(null);
                }
                else
                {
                    var balance = Utils.Convert.ToUInt64(args[2]);
                    var sendLimitCur = Utils.Convert.ToUInt64(args[3]);
                    var savingsBalance = Utils.Convert.ToUInt64(args[4]);
                    var benefitsToDebit = (bool)args[5];

                    var param = new object[] { tariffNum, balance, sendLimitCur, savingsBalance, benefitsToDebit };

                    if (IsActive)
                    {
                        CEF.Browser.Window.ExecuteJs("MenuBank.draw", new object[] { param });

                        CEF.Browser.Window.ExecuteJs("MenuBank.setSavingsBal", savingsBalance);
                        CEF.Browser.Window.ExecuteJs("MenuBank.setDebetLim", sendLimitCur);

                        UpdateMoney(balance);
                    }
                    else
                    {
                        await Show(param);
                    }
                }
            });
        }

        public static async System.Threading.Tasks.Task Show(object[] args)
        {
            if (IsActive)
                return;

            if (Misc.IsAnyCefActive(true))
                return;

            await CEF.Browser.Render(Browser.IntTypes.MenuBank, true, true);

            CloseColshape = new Additional.Sphere(Player.LocalPlayer.Position, 2.5f, false, Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                }
            };

            if (args == null)
            {
                CEF.Browser.Window.ExecuteJs("MenuBank.draw();");

                CEF.Browser.Window.ExecuteJs("MenuBank.selectOption", 2);
            }
            else
            {
                CEF.Browser.Window.ExecuteJs("MenuBank.draw", new object[] { args });

                CEF.Browser.Window.ExecuteJs("MenuBank.selectOption", 0);
            }

            CEF.Cursor.Show(true, true);

            TempBinds.Add(Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            CEF.Browser.Render(Browser.IntTypes.MenuBank, false, false);

            CEF.Cursor.Show(false, false);

            foreach (var x in TempBinds)
                Core.Unbind(x);

            TempBinds.Clear();

            Player.LocalPlayer.ResetData("CurrentBank::Id");
        }

        public static void UpdateMoney(ulong value)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuBank.setDebetBal", value);
        }
    }
}
