using BlaineRP.Client.Extensions.RAGE.Ui;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Vehicles
    {
        public Vehicles()
        {
        }

        public static void Show(object ownedList, object rentedList)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Vehicles;

            CEF.Phone.Phone.CurrentAppTab = -1;

            Browser.Window.ExecuteJs("Phone.drawVehApp",
                new object[]
                {
                    new object[]
                    {
                        ownedList,
                        rentedList,
                    },
                }
            );
        }
    }
}