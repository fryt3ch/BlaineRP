using BlaineRP.Client.Extensions.RAGE.Ui;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    public class BSim
    {
        public static void Show(string number, uint balance, uint costMinCall, uint costCharSms)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.BSim;

            CEF.Phone.Phone.CurrentAppTab = -1;

            Browser.Window.ExecuteJs("Phone.drawBSimApp",
                new object[]
                {
                    new object[]
                    {
                        number,
                        balance,
                        costMinCall,
                        costCharSms,
                    },
                }
            );
        }
    }
}