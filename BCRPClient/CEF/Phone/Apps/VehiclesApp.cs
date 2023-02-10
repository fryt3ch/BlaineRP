using RAGE;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPClient.CEF.PhoneApps
{
    public class VehiclesApp : Events.Script
    {
        public VehiclesApp()
        {

        }

        public static void Show(object ownedList, object rentedList)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Vehicles;

            Phone.CurrentAppTab = -1;

            CEF.Browser.Window.ExecuteJs("Phone.drawVehApp", new object[] { new object[] { ownedList, rentedList } });
        }
    }
}
