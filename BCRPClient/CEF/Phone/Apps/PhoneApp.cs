using RAGE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.CEF.PhoneApps
{
    public class PhoneApp
    {
        private static AsyncTask ActiveCallUpdateTask { get; set; }

        private static Dictionary<string[], Action> DefaultNumbersActions { get; set; } = new Dictionary<string[], Action>()
        {
            {
                new string[] { "*100#", "*102#" },

                async () =>
                {

                }
            },
        };

        public static async void Call(string number)
        {
            var defAction = DefaultNumbersActions.FirstOrDefault(x => x.Key.Contains(number)).Value;

            if (defAction != null)
            {
                defAction.Invoke();

                return;
            }
            else
            {
                uint numNumber;

                if (uint.TryParse(number, out numNumber))
                {
                    if ((bool)await Events.CallRemoteProc("Phone::CP", numNumber))
                    {

                    }
                    else
                    {
                        CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Players.PhoneNumberWrong1);
                    }

                    return;
                }
            }

            CEF.Notification.Show(CEF.Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Players.PhoneNumberWrong0);
        }

        public static void CallAnswer(bool accept)
        {

        }

        public static void ShowDefault(string number = null)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);
            
            if (Phone.CurrentApp != Phone.AppTypes.Phone)
                Phone.CurrentApp = Phone.AppTypes.Phone;

            if (number == null)
                CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] {  } });
            else
                CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] { number } });
        }

        public static void ShowIncomingCall(string number)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            if (Phone.CurrentApp != Phone.AppTypes.Phone)
                Phone.CurrentApp = Phone.AppTypes.Phone;

            CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] { true, number } });
        }

        public static void ShowActiveCall(string number, string text)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            if (Phone.CurrentApp != Phone.AppTypes.Phone)
                Phone.CurrentApp = Phone.AppTypes.Phone;

            CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] { false, number, text } });
        }

        public static void StartActiveCallUpdateTask(string number)
        {
            ActiveCallUpdateTask?.Cancel();

            var dateTime = DateTime.UtcNow;

            ActiveCallUpdateTask = new AsyncTask(() =>
            {
                if (Phone.CurrentApp != Phone.AppTypes.Phone)
                    return;

                CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] { false, number, DateTime.UtcNow.Subtract(dateTime).GetBeautyString() } });
            }, 1000, true, 0);

            ActiveCallUpdateTask.Run();
        }

        public static void CancelActiveCallUpdateTask()
        {
            if (ActiveCallUpdateTask != null)
            {
                ActiveCallUpdateTask.Cancel();

                ActiveCallUpdateTask = null;
            }
        }
    }
}
