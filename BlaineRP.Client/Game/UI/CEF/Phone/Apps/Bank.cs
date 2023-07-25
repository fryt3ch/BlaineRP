using BlaineRP.Client.CEF.Phone.Enums;
using BlaineRP.Client.Extensions.RAGE.Ui;
using System;

namespace BlaineRP.Client.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Bank
    {
        public static Action<int> CurrentTransactionAction { get; set; }

        public static void Show(string tariffName, decimal bankBalance, decimal savingsBalance)
        {
            if (CEF.Phone.Phone.CurrentApp == AppTypes.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppTypes.Bank;

            CEF.Phone.Phone.CurrentAppTab = -1;

            Browser.Window.ExecuteJs("Phone.drawBankApp", new object[] { new object[] { tariffName, bankBalance, savingsBalance } });
        }

        public static void UpdateBalance(decimal balance)
        {
            if (CEF.Phone.Phone.CurrentApp != AppTypes.Bank)
                return;

            Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tariff-info", 1, balance);
        }
    }
}
