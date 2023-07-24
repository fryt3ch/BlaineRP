using BlaineRP.Client.CEF.Phone.Enums;
using BlaineRP.Client.Extensions.RAGE.Ui;

namespace BlaineRP.Client.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Vehicles
    {
        public Vehicles()
        {

        }

        public static void Show(object ownedList, object rentedList)
        {
            if (CEF.Phone.Phone.CurrentApp == AppTypes.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppTypes.Vehicles;

            CEF.Phone.Phone.CurrentAppTab = -1;

            Browser.Window.ExecuteJs("Phone.drawVehApp", new object[] { new object[] { ownedList, rentedList } });
        }
    }
}
