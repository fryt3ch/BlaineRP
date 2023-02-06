using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF.PhoneApps
{
    public class BSimApp
    {
        public static void Show(string number, uint balance, uint costMinCall, uint costCharSms)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            if (Phone.CurrentApp != Phone.AppTypes.BSim)
                Phone.CurrentApp = Phone.AppTypes.BSim;

            CEF.Browser.Window.ExecuteJs("Phone.drawBSimApp", new object[] { new object[] { number, balance, costMinCall, costCharSms } });
        }
    }
}
