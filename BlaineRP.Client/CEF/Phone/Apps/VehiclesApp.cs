using BlaineRP.Client.Extensions.RAGE.Ui;

namespace BlaineRP.Client.CEF.PhoneApps
{
    [Script(int.MaxValue)]
    public class VehiclesApp
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
