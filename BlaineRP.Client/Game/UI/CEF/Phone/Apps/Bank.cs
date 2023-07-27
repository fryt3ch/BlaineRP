using System;
using BlaineRP.Client.Extensions.RAGE.Ui;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Bank
    {
        public static Action<int> CurrentTransactionAction { get; set; }

        public static void Show(string tariffName, decimal bankBalance, decimal savingsBalance)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Bank;

            CEF.Phone.Phone.CurrentAppTab = -1;

            Browser.Window.ExecuteJs("Phone.drawBankApp",
                new object[]
                {
                    new object[]
                    {
                        tariffName,
                        bankBalance,
                        savingsBalance,
                    },
                }
            );
        }

        public static void UpdateBalance(decimal balance)
        {
            if (CEF.Phone.Phone.CurrentApp != AppType.Bank)
                return;

            Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tariff-info", 1, balance);
        }
    }
}