using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BCRPClient.CEF
{
    class Bank : Events.Script
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

                if (LastSent.IsSpam(1000, false, false))
                    return;

                LastSent = DateTime.Now;

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

                var amountI = Convert.ToDecimal(args[2]);

                int amount;

                if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                    return;

                if (LastSent.IsSpam(1000, false, false))
                    return;

                LastSent = DateTime.Now;

                if (aId == "transfer")
                {
                    var cid = args[3].ToUInt32();

                    if (Player.LocalPlayer.HasData("Bank::LastCID") && Player.LocalPlayer.GetData<uint>("Bank::LastCID") == cid && Player.LocalPlayer.GetData<int>("Bank::LastAmount") == amount)
                    {
                        await Events.CallRemoteProc("Bank::Debit::Send", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), cid, amount, false);

                        Player.LocalPlayer.ResetData("Bank::LastCID");
                        Player.LocalPlayer.ResetData("Bank::LastAmount");
                    }
                    else
                    {
                        if ((bool)await Events.CallRemoteProc("Bank::Debit::Send", Player.LocalPlayer.GetData<int>("CurrentBank::Id"), cid, amount, true));
                        {
                            Player.LocalPlayer.SetData("Bank::LastCID", cid);
                            Player.LocalPlayer.SetData("Bank::LastAmount", amount);
                        }
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
                        var newSavingsValue = (int)await Events.CallRemoteProc("Bank::Savings::Operation", false, Player.LocalPlayer.GetData<int>("CurrentBank::Id"), aId == "deposit", amount);

                        if (newSavingsValue >= 0)
                        {
                            if (!IsActive)
                                return;

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

                    LastSent = DateTime.Now;
                }
            });

            Events.Add("MenuBank::Show", async (object[] args) =>
            {
                Sync.Players.CloseAll(true);

                Player.LocalPlayer.SetData("CurrentBank::Id", (int)args[0]);

                var tariffNum = (int)args[1];

                if (tariffNum < 0)
                {
                    await Show(null);
                }
                else
                {
                    var balance = args[2].ToUInt64();
                    var sendLimitCur = args[3].ToUInt64();
                    var savingsBalance = args[4].ToUInt64();
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

            if (Utils.IsAnyCefActive(true))
                return;

            await CEF.Browser.Render(Browser.IntTypes.MenuBank, true, true);

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

            TempBinds.Add(RAGE.Input.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close(false)));
        }

        public static void Close(bool ignoreTimeout = false)
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.MenuBank, false, false);

            CEF.Cursor.Show(false, false);

            foreach (var x in TempBinds)
                RAGE.Input.Unbind(x);

            TempBinds.Clear();

            Player.LocalPlayer.ResetData("CurrentBank::Id");

            Player.LocalPlayer.ResetData("Bank::LastCID");
            Player.LocalPlayer.ResetData("Bank::LastAmount");
        }

        public static void UpdateMoney(ulong value)
        {
            if (!IsActive)
                return;

            CEF.Browser.Window.ExecuteJs("MenuBank.setDebetBal", value);
        }
    }
}
