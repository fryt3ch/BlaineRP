using System;
using System.Collections.Generic;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Scripts.Sync;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF
{
    [Script(int.MaxValue)]
    public class Bank
    {
        public enum TariffTypes
        {
            /// <summary>Стандартный</summary>
            Standart = 0,

            /// <summary>Расширенный</summary>
            StandartPlus,

            /// <summary>Продвинутый</summary>
            Supreme,
        }

        private static DateTime LastSent;

        public Bank()
        {
            TempBinds = new List<int>();

            LastSent = DateTime.MinValue;

            Events.Add("MenuBank::Close",
                (object[] args) =>
                {
                    Close(false);
                }
            );

            Events.Add("MenuBank::Cash2Debet",
                async (object[] args) =>
                {
                    if (!IsActive)
                        return;

                    var state = (bool)args[0];

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = World.Core.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Bank::Savings::ToDebitSett", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), state))
                    {
                        if (!IsActive)
                            return;

                        Browser.Window.ExecuteJs("MenuBank.setCash2Debet", state);
                    }
                }
            );

            Events.Add("MenuBank::Action",
                async (object[] args) =>
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

                    LastSent = World.Core.ServerTime;

                    if (aId == "transfer")
                    {
                        var cid = Utils.Convert.ToUInt32(args[3]);

                        var approveContext = $"BankSendToPlayer_{cid}_{amount}";
                        var approveTime = 5_000;

                        if (Notification.HasApproveTimedOut(approveContext, World.Core.ServerTime, approveTime))
                        {
                            if (LastSent.IsSpam(1_500, false, true))
                                return;

                            LastSent = World.Core.ServerTime;

                            Notification.SetCurrentApproveContext(approveContext, World.Core.ServerTime);

                            if ((bool)await Events.CallRemoteProc("Bank::Debit::Send", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), cid, amount, true))
                                ;
                            {
                            }
                        }
                        else
                        {
                            Notification.ClearAll();

                            Notification.SetCurrentApproveContext(null, DateTime.MinValue);

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
                            string[] res = ((string)await Events.CallRemoteProc("Bank::Savings::Operation",
                                Player.LocalPlayer.GetData<int>("CurrentBank::Id"),
                                aId == "deposit",
                                amount
                            ))?.Split('_');

                            if (res == null)
                                return;

                            var newSavingsValue = Utils.Convert.ToUInt64(res[0]);
                            var newMinSavingsValue = Utils.Convert.ToUInt64(res[1]);

                            if (IsActive)
                                Browser.Window.ExecuteJs("MenuBank.setSavingsBal", newSavingsValue);
                        }
                    }
                }
            );

            Events.Add("MenuBank::BuyTariff",
                (object[] args) =>
                {
                    if (!IsActive)
                        return;

                    var tarrif = (TariffTypes)(int)args[0];

                    if (!LastSent.IsSpam(1000, false, false))
                    {
                        Events.CallRemote("Bank::Tariff::Buy", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), (int)tarrif);

                        LastSent = World.Core.ServerTime;
                    }
                }
            );

            Events.Add("MenuBank::Show",
                async (object[] args) =>
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

                        var param = new object[]
                        {
                            tariffNum,
                            balance,
                            sendLimitCur,
                            savingsBalance,
                            benefitsToDebit,
                        };

                        if (IsActive)
                        {
                            Browser.Window.ExecuteJs("MenuBank.draw",
                                new object[]
                                {
                                    param,
                                }
                            );

                            Browser.Window.ExecuteJs("MenuBank.setSavingsBal", savingsBalance);
                            Browser.Window.ExecuteJs("MenuBank.setDebetLim", sendLimitCur);

                            UpdateMoney(balance);
                        }
                        else
                        {
                            await Show(param);
                        }
                    }
                }
            );
        }

        public static bool IsActive => Browser.IsActive(Browser.IntTypes.MenuBank);

        private static List<int> TempBinds { get; set; }

        private static ExtraColshape CloseColshape { get; set; }

        public static async System.Threading.Tasks.Task Show(object[] args)
        {
            if (IsActive)
                return;

            if (Utils.Misc.IsAnyCefActive(true))
                return;

            await Browser.Render(Browser.IntTypes.MenuBank, true, true);

            CloseColshape = new Sphere(Player.LocalPlayer.Position, 2.5f, false, Utils.Misc.RedColor, uint.MaxValue, null)
            {
                OnExit = (cancel) =>
                {
                    if (CloseColshape?.Exists == true)
                        Close(false);
                },
            };

            if (args == null)
            {
                Browser.Window.ExecuteJs("MenuBank.draw();");

                Browser.Window.ExecuteJs("MenuBank.selectOption", 2);
            }
            else
            {
                Browser.Window.ExecuteJs("MenuBank.draw",
                    new object[]
                    {
                        args,
                    }
                );

                Browser.Window.ExecuteJs("MenuBank.selectOption", 0);
            }

            Cursor.Show(true, true);

            TempBinds.Add(Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CloseColshape?.Destroy();

            CloseColshape = null;

            Browser.Render(Browser.IntTypes.MenuBank, false, false);

            Cursor.Show(false, false);

            foreach (int x in TempBinds)
            {
                Input.Core.Unbind(x);
            }

            TempBinds.Clear();

            Player.LocalPlayer.ResetData("CurrentBank::Id");
        }

        public static void UpdateMoney(ulong value)
        {
            if (!IsActive)
                return;

            Browser.Window.ExecuteJs("MenuBank.setDebetBal", value);
        }
    }
}