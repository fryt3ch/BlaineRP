using BlaineRP.Client.Extensions.RAGE.Ui;
using System;

namespace BlaineRP.Client.CEF.PhoneApps
{
    [Script(int.MaxValue)]
    public class BankApp
    {
        public static Action<int> CurrentTransactionAction { get; set; }

        public static void Show(string tariffName, decimal bankBalance, decimal savingsBalance)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Bank;

            Phone.CurrentAppTab = -1;

            CEF.Browser.Window.ExecuteJs("Phone.drawBankApp", new object[] { new object[] { tariffName, bankBalance, savingsBalance } });
        }

        public static void UpdateBalance(decimal balance)
        {
            if (Phone.CurrentApp != Phone.AppTypes.Bank)
                return;

            CEF.Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tariff-info", 1, balance);
        }
    }
}
