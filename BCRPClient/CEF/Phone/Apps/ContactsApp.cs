using RAGE;
using RAGE.Elements;
using System.Linq;
using static BCRPClient.CEF.Phone;

namespace BCRPClient.CEF.PhoneApps
{
    public class ContactsApp : Events.Script
    {
        public ContactsApp()
        {
            Events.Add("Phone::ContactConfrim", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (LastSent.IsSpam(500, false, false))
                    return;

                if (args == null || args.Length < 2)
                    return;

                var numberS = (string)args[1];

                if (numberS == null)
                    return;

                decimal numberD;
                uint number;

                if (!decimal.TryParse(numberS, out numberD) || !numberD.IsNumberValid(1, uint.MaxValue, out number, true))
                    return;

                var name = ((string)args[0])?.Trim();

                if (!name.IsTextLengthValid(1, 24, true))
                    return;

                for (int i = 0; i < name.Length; i++)
                {
                    if (!char.IsLetterOrDigit(name[i]) && !char.IsWhiteSpace(name[i]))
                    {
                        CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.StringOnlyLettersNumbersW);

                        return;
                    }
                }

                LastSent = Sync.World.ServerTime;

                if (!(bool)await Events.CallRemoteProc("Phone::CC", number, name))
                    return;

                var allContacts = pData.Contacts;

                if (!allContacts.TryAdd(number, name))
                    allContacts[number] = name;

                if (allContacts.Count == 0)
                {
                    PhoneApps.ContactsApp.ShowAll(null);
                }
                else
                {
                    PhoneApps.ContactsApp.ShowAll(allContacts.OrderBy(x => x.Value).Select(x => new object[] { x.Value, x.Key }));
                }
            });
        }

        public static void ShowAll(object list)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Contacts;

            Phone.CurrentAppTab = -1;

            CEF.Browser.Window.ExecuteJs("Phone.drawContactsApp", list);
        }

        public static void ShowEdit(string number, string name)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            if (Phone.CurrentApp != AppTypes.Contacts)
            {
                CEF.Browser.Window.ExecuteJs("Phone.drawContactsApp();");

                Phone.CurrentApp = Phone.AppTypes.Contacts;
            }

            Phone.CurrentAppTab = 0;

            if (number == null && name == null)
                CEF.Browser.Window.ExecuteJs("Phone.contactEdit();");
            else
                CEF.Browser.Window.ExecuteJs("Phone.contactEdit", new object[] { new object[] { name, number } });
        }
    }
}
