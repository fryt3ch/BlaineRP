using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.UI.CEF.Phone.Enums;
using BlaineRP.Client.Game.World;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Contacts
    {
        public Contacts()
        {
            Events.Add("Phone::ContactConfrim",
                async (args) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
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

                    string name = ((string)args[0])?.Trim();

                    if (!name.IsTextLengthValid(1, 24, true))
                        return;

                    for (var i = 0; i < name.Length; i++)
                    {
                        if (!char.IsLetterOrDigit(name[i]) && !char.IsWhiteSpace(name[i]))
                        {
                            Notification.ShowError(Locale.Notifications.General.StringOnlyLettersNumbersW);

                            return;
                        }
                    }

                    CEF.Phone.Phone.LastSent = Core.ServerTime;

                    if (!(bool)await Events.CallRemoteProc("Phone::CC", number, name))
                        return;

                    Dictionary<uint, string> allContacts = pData.Contacts;

                    if (!allContacts.TryAdd(number, name))
                        allContacts[number] = name;

                    if (allContacts.Count == 0)
                        ShowAll(null);
                    else
                        ShowAll(allContacts.OrderBy(x => x.Value)
                                           .Select(x => new object[]
                                                {
                                                    x.Value,
                                                    x.Key,
                                                }
                                            )
                                           .ToList()
                        );
                }
            );
        }

        public static void ShowAll(object list)
        {
            if (CEF.Phone.Phone.CurrentApp == AppTypes.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppTypes.Contacts;

            CEF.Phone.Phone.CurrentAppTab = -1;

            Browser.Window.ExecuteJs("Phone.drawContactsApp", list);
        }

        public static void ShowEdit(string number, string name)
        {
            if (CEF.Phone.Phone.CurrentApp == AppTypes.None)
                CEF.Phone.Phone.SwitchMenu(false);

            if (CEF.Phone.Phone.CurrentApp != AppTypes.Contacts)
            {
                Browser.Window.ExecuteJs("Phone.drawContactsApp();");

                CEF.Phone.Phone.CurrentApp = AppTypes.Contacts;
            }

            CEF.Phone.Phone.CurrentAppTab = 0;

            if (number == null && name == null)
                Browser.Window.ExecuteJs("Phone.contactEdit();");
            else
                Browser.Window.ExecuteJs("Phone.contactEdit",
                    new object[]
                    {
                        new object[]
                        {
                            name,
                            number,
                        },
                    }
                );
        }
    }
}