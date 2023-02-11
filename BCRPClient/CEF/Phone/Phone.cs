using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using static BCRPClient.Additional.Camera;

namespace BCRPClient.CEF
{
    public class Phone : Events.Script
    {
        public static bool IsActive { get; private set; }

        public static AppTypes CurrentApp { get; set; }

        public static int CurrentAppTab { get; set; } = -1;

        public static object CurrentExtraData { get; set; }

        public static DateTime LastSent;

        private static List<int> TempBinds { get; set; } = new List<int>();

        public enum AppTypes
        {
            None = 0,

            Settings,
            Vehicles,
            Bank,
            BSim,
            Camera,
            Navigator,
            Music,
            Phone,
            Contacts,
            SMS,
            Browser,
            Taxi,
        }

        private static Dictionary<AppTypes, string> AppsJsNames = new Dictionary<AppTypes, string>()
        {
            { AppTypes.Settings, "settings" },
            { AppTypes.Vehicles, "veh" },
            { AppTypes.Bank, "bank" },
            { AppTypes.BSim, "bsim" },
            { AppTypes.Camera, "camera" },
            { AppTypes.Navigator, "gps" },
            { AppTypes.Music, "music" },
            { AppTypes.Phone, "phone" },
            { AppTypes.Contacts, "contacts" },
            { AppTypes.SMS, "sms" },
            { AppTypes.Browser, "browser" },
            { AppTypes.Taxi, "taxi" },
        };

        public static AppTypes GetAppTypeByJsName(string jsName) => AppsJsNames.Where(x => x.Value == jsName).Select(x => x.Key).FirstOrDefault();

        public Phone()
        {
            Events.Add("Phone::OpenApp", async (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                var pData = Sync.Players.GetData(RAGE.Elements.Player.LocalPlayer);

                if (pData == null)
                    return;

                var appIdStr = args[0] is string str ? str : null;

                var appType = appIdStr == null ? AppTypes.None : GetAppTypeByJsName(appIdStr.Replace("_app", ""));

                LastSent = DateTime.Now;

                ShowApp(pData, appType);
            });

            Events.Add("Phone::Tab", async (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                var pData = Sync.Players.GetData(RAGE.Elements.Player.LocalPlayer);

                if (pData == null)
                    return;

                var appTab = args[0] is string str ? str.GetHashCode() : (int)args[0];

                LastSent = DateTime.Now;

                if (CurrentApp == AppTypes.Phone)
                {
                    if (appTab == 0) // callhist
                    {
                        var callHist = PhoneApps.PhoneApp.CallHistory;

                        if (callHist.Count == 0)
                        {
                            PhoneApps.PhoneApp.ShowCallHistory(null);
                        }
                        else
                        {
                            var blackList = pData.PhoneBlacklist;

                            PhoneApps.PhoneApp.ShowCallHistory(callHist.Select(x => new object[] { (int)x.Status, x.PhoneNumber, GetContactNameByNumberNull(x.PhoneNumber), blackList.Contains(x.PhoneNumber) }));
                        }
                    }
                    else if (appTab == 1) // blacklist
                    {
                        var blacklist = pData.PhoneBlacklist;

                        if (blacklist.Count == 0)
                        {
                            PhoneApps.PhoneApp.ShowBlacklist(null, null);
                        }
                        else
                        {
                            PhoneApps.PhoneApp.ShowBlacklist(blacklist.Select(x => new object[] { x, GetContactNameByNumberNull(x) }));
                        }
                    }
                }
                else if (CurrentApp == AppTypes.Settings)
                {
                    if (appTab == "wallpaper".GetHashCode())
                    {
                        CEF.Browser.Window.ExecuteCachedJs("Phone.switchTabSettings", "wallpaper", "Выбор обоев");

                        CurrentAppTab = appTab;
                    }
                }
                else if (CurrentApp == AppTypes.SMS)
                {
                    if (appTab == 0) // typeSms
                    {
                        var receiverNum = args[1]?.ToString();

                        if (receiverNum == "0")
                            receiverNum = null;

                        PhoneApps.SMSApp.ShowWriteNew(receiverNum);
                    }
                    else if (appTab == 1) // openChat
                    {
                        var targetNum = uint.Parse(args[1].ToString());

                        var chatList = PhoneApps.SMSApp.GetChatList(pData.AllSMS, targetNum, pData.PhoneNumber);

                        if (chatList == null)
                            return;

                        PhoneApps.SMSApp.ShowChat(targetNum, chatList, GetContactNameByNumberNull(targetNum));
                    }
                }
                else if (CurrentApp == AppTypes.Contacts)
                {
                    if (appTab == 0) // add contact
                    {
                        PhoneApps.ContactsApp.ShowEdit(null, null);
                    }
                }
                else if (CurrentApp == AppTypes.Bank)
                {
                    if (appTab == 0) // send money
                    {
                        CurrentAppTab = 0;

                        CEF.Browser.Window.ExecuteJs("Phone.drawBankTab", 0);
                    }
                    else if (appTab == 1) // house
                    {
                        var house = pData.OwnedHouses.FirstOrDefault();

                        if (house == null)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.NoOwnedHouse);

                            return;
                        }

                        var resData = ((string)await Events.CallRemoteProc("Bank::GHA", house.Id))?.Split('_');

                        if (resData == null)
                            return;

                        var balance = ulong.Parse(resData[0]);
                        var maxPaidDays = uint.Parse(resData[1]);
                        var minPaidDays = uint.Parse(resData[2]);

                        var maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * house.Tax;
                        var minBalance = minPaidDays * house.Tax;

                        CurrentAppTab = 1;

                        CEF.Browser.Window.ExecuteJs("Phone.drawBankTab", 1, new object[] { balance, house.Tax });

                        PhoneApps.BankApp.CurrentTransactionAction = async (amount) =>
                        {
                            if (balance >= maxBalance)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Money.MaximalBalanceAlready);

                                return;
                            }

                            var nBalance = maxBalance == ulong.MaxValue ? (pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance) : maxBalance - balance;

                            if (nBalance == 0)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Money.NotEnough);

                                return;
                            }

                            if ((decimal)balance + amount > maxBalance)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, string.Format(Locale.Notifications.Money.MaximalBalanceNear, Utils.GetPriceString(maxBalance - balance)));

                                return;
                            }

                            var resObj = await Events.CallRemoteProc("Bank::HBC", true, house.Id, -1, amount, false, true);

                            if (resObj == null)
                                return;

                            balance = resObj.ToUInt64();

                            CEF.Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tab-info", 0, balance);
                        };
                    }
                    else if (appTab == 2) // flat
                    {
                        var house = pData.OwnedApartments.FirstOrDefault();

                        if (house == null)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.NoOwnedApartments);

                            return;
                        }

                        var resData = ((string)await Events.CallRemoteProc("Bank::GAA", house.Id))?.Split('_');

                        if (resData == null)
                            return;

                        var balance = ulong.Parse(resData[0]);
                        var maxPaidDays = uint.Parse(resData[1]);
                        var minPaidDays = uint.Parse(resData[2]);

                        var maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * house.Tax;
                        var minBalance = minPaidDays * house.Tax;

                        CurrentAppTab = 2;

                        CEF.Browser.Window.ExecuteJs("Phone.drawBankTab", 2, new object[] { balance, house.Tax });

                        PhoneApps.BankApp.CurrentTransactionAction = async (amount) =>
                        {
                            if (balance >= maxBalance)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Money.MaximalBalanceAlready);

                                return;
                            }

                            var nBalance = maxBalance == ulong.MaxValue ? (pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance) : maxBalance - balance;

                            if (nBalance == 0)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Money.NotEnough);

                                return;
                            }

                            if ((decimal)balance + amount > maxBalance)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, string.Format(Locale.Notifications.Money.MaximalBalanceNear, Utils.GetPriceString(maxBalance - balance)));

                                return;
                            }

                            var resObj = await Events.CallRemoteProc("Bank::HBC", false, house.Id, -1, amount, false, true);

                            if (resObj == null)
                                return;

                            balance = resObj.ToUInt64();

                            CEF.Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tab-info", 0, balance);
                        };
                    }
                    else if (appTab == 3) // garage
                    {
                        var garage = pData.OwnedGarages.FirstOrDefault();

                        if (garage == null)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.NoOwnedGarage);

                            return;
                        }

                        var resData = ((string)await Events.CallRemoteProc("Bank::GGA", garage.Id))?.Split('_');

                        if (resData == null)
                            return;

                        var balance = ulong.Parse(resData[0]);
                        var maxPaidDays = uint.Parse(resData[1]);
                        var minPaidDays = uint.Parse(resData[2]);

                        var maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * garage.Tax;
                        var minBalance = minPaidDays * garage.Tax;

                        CurrentAppTab = 3;

                        CEF.Browser.Window.ExecuteJs("Phone.drawBankTab", 3, new object[] { balance, garage.Tax });

                        PhoneApps.BankApp.CurrentTransactionAction = async (amount) =>
                        {
                            if (balance >= maxBalance)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Money.MaximalBalanceAlready);

                                return;
                            }

                            var nBalance = maxBalance == ulong.MaxValue ? (pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance) : maxBalance - balance;

                            if (nBalance == 0)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Money.NotEnough);

                                return;
                            }

                            if ((decimal)balance + amount > maxBalance)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, string.Format(Locale.Notifications.Money.MaximalBalanceNear, Utils.GetPriceString(maxBalance - balance)));

                                return;
                            }

                            var resObj = await Events.CallRemoteProc("Bank::GBC", garage.Id, -1, amount, false, true);

                            if (resObj == null)
                                return;

                            balance = resObj.ToUInt64();

                            CEF.Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tab-info", 0, balance);
                        };
                    }
                    else if (appTab == 4) // business
                    {
                        var biz = pData.OwnedBusinesses.FirstOrDefault();

                        if (biz == null)
                        {
                            CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.NoOwnedBusiness);

                            return;
                        }

                        var resData = ((string)await Events.CallRemoteProc("Bank::GBA", biz.Id))?.Split('_');

                        if (resData == null)
                            return;

                        var balance = ulong.Parse(resData[0]);
                        var maxPaidDays = uint.Parse(resData[1]);
                        var minPaidDays = uint.Parse(resData[2]);

                        var maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * biz.Rent;
                        var minBalance = minPaidDays * biz.Rent;

                        CurrentAppTab = 4;

                        CEF.Browser.Window.ExecuteJs("Phone.drawBankTab", 4, new object[] { balance, biz.Rent });

                        PhoneApps.BankApp.CurrentTransactionAction = async (amount) =>
                        {
                            if (balance >= maxBalance)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Money.MaximalBalanceAlready);

                                return;
                            }

                            var nBalance = maxBalance == ulong.MaxValue ? (pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance) : maxBalance - balance;

                            if (nBalance == 0)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.Money.NotEnough);

                                return;
                            }

                            if ((decimal)balance + amount > maxBalance)
                            {
                                CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, string.Format(Locale.Notifications.Money.MaximalBalanceNear, Utils.GetPriceString(maxBalance - balance)));

                                return;
                            }

                            var resObj = await Events.CallRemoteProc("Bank::BBC", biz.Id, -1, amount, false, true);

                            if (resObj == null)
                                return;

                            balance = resObj.ToUInt64();

                            CEF.Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tab-info", 0, balance);
                        };
                    }
                }
            });

            Events.Add("Phone::Tooltip", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (LastSent.IsSpam(500, false, false))
                    return;

                LastSent = DateTime.Now;

                var actId = int.Parse(args[1].ToString());
                var elem = args[2];

                if (elem == null)
                    return;

                if (CurrentApp == AppTypes.Phone)
                {
                    if (CurrentAppTab == 0)
                    {
                        var pcIdxS = elem?.ToString();

                        if (pcIdxS == null)
                            return;

                        int pcIdx;

                        if (!int.TryParse(pcIdxS, out pcIdx))
                            return;

                        var callHist = PhoneApps.PhoneApp.CallHistory;

                        if (pcIdx < 0 || pcIdx >= callHist.Count)
                            return;

                        var number = callHist[pcIdx].PhoneNumber;

                        if (actId == 0) // call
                        {
                            var numStr = number.ToString();

                            PhoneApps.PhoneApp.ShowDefault(numStr);

                            PhoneApps.PhoneApp.Call(numStr);
                        }
                        else if (actId == 1) // sms
                        {
                            var allSms = pData.AllSMS;
                            var pNumber = pData.PhoneNumber;

                            var chatList = PhoneApps.SMSApp.GetChatList(allSms, number, pNumber);

                            if (chatList != null)
                            {
                                PhoneApps.SMSApp.ShowChat(number, chatList, GetContactNameByNumberNull(number));
                            }
                            else
                            {
                                PhoneApps.SMSApp.ShowWriteNew(number.ToString());
                            }
                        }
                        else if (actId == 2) // add contact
                        {
                            PhoneApps.ContactsApp.ShowEdit(number.ToString(), null);
                        }
                        else if (actId == 3) // add/remove blacklist
                        {
                            PhoneApps.PhoneApp.BlacklistChange(number, !pData.PhoneBlacklist.Contains(number));
                        }
                    }
                }
                else if (CurrentApp == AppTypes.Contacts)
                {
                    //var number = uint.Parse(elem.ToString());

                    if (actId == 0) // call
                    {
                        var numStr = elem.ToString();

                        PhoneApps.PhoneApp.ShowDefault(numStr);

                        PhoneApps.PhoneApp.Call(numStr);
                    }
                    else if (actId == 1) // sms
                    {
                        var numberStr = elem.ToString();

                        var number = uint.Parse(numberStr);

                        var allSms = pData.AllSMS;
                        var pNumber = pData.PhoneNumber;

                        var chatList = PhoneApps.SMSApp.GetChatList(allSms, number, pNumber);

                        if (chatList != null)
                        {
                            PhoneApps.SMSApp.ShowChat(number, chatList, GetContactNameByNumberNull(number));
                        }
                        else
                        {
                            PhoneApps.SMSApp.ShowWriteNew(numberStr);
                        }
                    }
                    else if (actId == 2) // edit
                    {
                        var numberStr = elem.ToString();

                        var number = uint.Parse(numberStr);

                        PhoneApps.ContactsApp.ShowEdit(numberStr, GetContactNameByNumber(number));
                    }
                    else if (actId == 3) // delete
                    {
                        var numberStr = elem.ToString();

                        var number = uint.Parse(numberStr);

                        if (!(bool)await Events.CallRemoteProc("Phone::RC", number))
                            return;

                        var allContacts = pData.Contacts;

                        allContacts.Remove(number);

                        ShowApp(pData, AppTypes.Contacts);
                    }
                }
                else if (CurrentApp == AppTypes.Vehicles)
                {
                    var vOType = (string)args[0];

                    if (vOType == "owned")
                    {
                        var vid = uint.Parse(elem.ToString());

                        if (actId == 0) // locate
                        {

                        }
                        else if (actId == 1) // evacuate
                        {

                        }
                    }
                    else if (vOType == "rented")
                    {
                        var rid = ushort.Parse(elem.ToString());

                        if (actId == 0) // locate
                        {

                        }
                        else if (actId == 1) // stop rent
                        {

                        }
                    }
                }
            });

            Events.Add("Phone::Transaction", async (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                if (CurrentApp == AppTypes.BSim)
                {
                    var amountStr = args[0]?.ToString();

                    if (amountStr == null)
                        return;

                    decimal amountI;

                    if (!decimal.TryParse(amountStr, out amountI))
                        return;

                    int amount;

                    if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                        return;

                    LastSent = DateTime.Now;

                    var resObj = await Events.CallRemoteProc("Phone::AB", amount);

                    if (resObj == null)
                        return;

                    var res = resObj.ToUInt32();

                    CEF.Browser.Window.ExecuteJs("Phone.updateInfoLine", "bsim-app-info", 1, res);
                }
                else if (CurrentApp == AppTypes.Bank)
                {
                    if (CurrentAppTab == 0)
                    {
                        var cidStr = args[0]?.ToString();

                        if (cidStr == null)
                            return;

                        decimal cidI;

                        if (!decimal.TryParse(cidStr, out cidI))
                            return;

                        uint cid;

                        if (!cidI.IsNumberValid(1, uint.MaxValue, out cid, true))
                            return;

                        var amountStr = args[1]?.ToString();

                        if (amountStr == null)
                            return;

                        decimal amountI;

                        if (!decimal.TryParse(amountStr, out amountI))
                            return;

                        int amount;

                        if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                            return;

                        if (Player.LocalPlayer.HasData("Bank::LastCID") && Player.LocalPlayer.GetData<uint>("Bank::LastCID") == cid && Player.LocalPlayer.GetData<int>("Bank::LastAmount") == amount)
                        {
                            await Events.CallRemoteProc("Bank::Debit::Send", -1, cid, amount, false);

                            Player.LocalPlayer.ResetData("Bank::LastCID");
                            Player.LocalPlayer.ResetData("Bank::LastAmount");
                        }
                        else
                        {
                            if ((bool)await Events.CallRemoteProc("Bank::Debit::Send", -1, cid, amount, true))
                            {
                                Player.LocalPlayer.SetData("Bank::LastCID", cid);
                                Player.LocalPlayer.SetData("Bank::LastAmount", amount);
                            }
                        }
                    }
                    else if (CurrentAppTab >= 1 && CurrentAppTab <= 4)
                    {
                        var amountStr = args[0]?.ToString();

                        if (amountStr == null)
                            return;

                        decimal amountI;

                        if (!decimal.TryParse(amountStr, out amountI))
                            return;

                        int amount;

                        if (!amountI.IsNumberValid(1, int.MaxValue, out amount, true))
                            return;

                        PhoneApps.BankApp.CurrentTransactionAction?.Invoke(amount);
                    }
                }
            });

            Events.Add("Phone::UpdateToggle", (args) =>
            {
                if (LastSent.IsSpam(500, false, false))
                    return;

                var toggleId = (string)args[0];

                var state = (bool)args[1];

                if (CurrentApp == AppTypes.Settings)
                {
                    if (toggleId == "disturb")
                    {
                        Settings.Other.PhoneNotDisturb = state;

                        ToggleDoNotDisturb(state);

/*                        if (state)
                        {

                        }
                        else
                        {

                        }*/
                    }
                }
            });
        }

        public static async void ShowApp(Sync.Players.PlayerData pData, AppTypes appType)
        {
            if (pData == null)
            {
                pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;
            }

            if (appType == AppTypes.None)
            {
                CurrentApp = AppTypes.None;

                SwitchMenu(true);
            }
            else
            {
                if (appType == AppTypes.BSim)
                {
                    var res = ((string)await Events.CallRemoteProc("Phone::GPD"))?.Split('_');

                    if (res == null)
                        return;

                    PhoneApps.BSimApp.Show(pData.PhoneNumber.ToString(), uint.Parse(res[0]), uint.Parse(res[1]), uint.Parse(res[2]));
                }
                else if (appType == AppTypes.Phone)
                {
                    PhoneApps.PhoneApp.ShowDefault(null);
                }
                else if (appType == AppTypes.Camera)
                {
                    if (PhoneApps.CameraApp.CanShow())
                    {
                        PhoneApps.CameraApp.Show();
                    }
                }
                else if (appType == AppTypes.Settings)
                {
                    PhoneApps.SettingsApp.Show();
                }
                else if (appType == AppTypes.Bank)
                {
                    var resData = ((string)await Events.CallRemoteProc("Bank::PAGD"))?.Split('_');

                    if (resData == null)
                        return;

                    PhoneApps.BankApp.Show(((CEF.Bank.TariffTypes)int.Parse(resData[0])).ToString(), pData.BankBalance, decimal.Parse(resData[1]));
                }
                else if (appType == AppTypes.Vehicles)
                {
                    var ownedVehs = pData.OwnedVehicles.Select(x => new object[] { x.VID, $"{x.Data.BrandName}<br>{x.Data.SubName}<br>[#{x.VID}]", x.Data.Type.ToString() }).ToList();

                    var rentedVehs = Sync.Vehicles.RentedVehicle.All.Select(x => new object[] { x.RemoteId, $"{x.VehicleData.BrandName}<br>{x.VehicleData.SubName}<br>[#{x.RemoteId}]", x.VehicleData.Type.ToString() }).ToList();

                    PhoneApps.VehiclesApp.Show(ownedVehs.Count > 0 ? ownedVehs : null, rentedVehs.Count > 0 ? rentedVehs : null);
                }
                else if (appType == AppTypes.Navigator)
                {

                }
                else if (appType == AppTypes.Contacts)
                {
                    var allContacts = pData.Contacts;

                    if (allContacts.Count == 0)
                    {
                        PhoneApps.ContactsApp.ShowAll(null);
                    }
                    else
                    {
                        PhoneApps.ContactsApp.ShowAll(allContacts.OrderBy(x => x.Value).Select(x => new object[] { x.Value, x.Key }));
                    }
                }
                else if (appType == AppTypes.SMS)
                {
                    var allSms = pData.AllSMS;

                    if (allSms.Count == 0)
                    {
                        PhoneApps.SMSApp.ShowPreviews(null);
                    }
                    else
                    {
                        var pNumber = pData.PhoneNumber;

                        PhoneApps.SMSApp.ShowPreviews(PhoneApps.SMSApp.GetSMSPreviews(allSms, pNumber).Select(x => new object[] { x.Key, GetContactNameByNumberNull(x.Key), x.Value.Date.ToString("HH:mm"), x.Value.Text }));
                    }
                }
                else if (appType == AppTypes.Taxi)
                {

                }
            }
        }

        public static void Show()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.ActiveCall is PhoneApps.PhoneApp.CallInfo callInfo)
            {
                PhoneApps.PhoneApp.ShowIncomingCall(GetContactNameByNumber(callInfo.Number));
            }

            IsActive = true;

            if (CEF.HUD.SpeedometerEnabled)
                CEF.Browser.Switch(Browser.IntTypes.HUD_Speedometer, false);

            if (CEF.Browser.IsActive(Browser.IntTypes.HUD_Help))
                CEF.Browser.Switch(Browser.IntTypes.HUD_Help, false);

            CEF.Browser.Window.ExecuteCachedJs("Phone.showPhone", true);

            KeyBinds.CursorNotFreezeInput = true;

            CEF.Cursor.Show(false, true);

            TempBinds.Add(KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () =>
            {
                if (CEF.Chat.InputVisible)
                    return;

                Sync.Phone.Toggle();
            }));
        }

        public static void Close()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.ActiveCall != null)
            {
                CEF.Phone.ShowApp(pData, AppTypes.None);
            }

            IsActive = false;

            CEF.Browser.Window.ExecuteCachedJs("Phone.showPhone", false);

            if (CEF.HUD.SpeedometerMustBeEnabled)
                CEF.Browser.Switch(Browser.IntTypes.HUD_Speedometer, true);

            if (CEF.HUD.IsActive && !Settings.Interface.HideHints)
                CEF.Browser.Switch(Browser.IntTypes.HUD_Help, true);

            KeyBinds.CursorNotFreezeInput = false;

            CEF.Cursor.Show(false, false);

            TempBinds.ForEach(x => KeyBinds.Unbind(x));

            TempBinds.Clear();

            Player.LocalPlayer.ResetData("Bank::LastCID");
            Player.LocalPlayer.ResetData("Bank::LastAmount");
        }

        public static void Preload()
        {
            CEF.Browser.Switch(Browser.IntTypes.Phone, true);

            SetWallpaper(Settings.Other.PhoneWallpaperNum);

            CEF.Browser.Window.ExecuteJs("Phone.setDisturb", Settings.Other.PhoneNotDisturb);
        }

        public static void SetWallpaper(int num)
        {
            CEF.Browser.Window.ExecuteJs("Phone.setWallpaper", num);
        }

        public static void ToggleDoNotDisturb(bool state)
        {
            CEF.Browser.Window.ExecuteJs("Phone.updateDisturb", state);
        }

        public static void SwitchMenu(bool state)
        {
            CurrentAppTab = -1;

            CEF.Browser.Window.ExecuteCachedJs("Phone.showMenu", state);
        }

        public static string GetContactNameByNumberNull(uint number)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return null;

            var defName = Locale.General.DefaultNumbersNames.GetValueOrDefault(number);

            if (defName != null)
                return defName;

            return pData.Contacts.GetValueOrDefault(number);
        }

        public static string GetContactNameByNumber(uint number) => GetContactNameByNumberNull(number) ?? number.ToString();
    }
}
