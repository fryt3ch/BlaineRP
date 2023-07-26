using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.Businesses;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Estates;
using BlaineRP.Client.Game.Management;
using BlaineRP.Client.Game.UI.CEF.Phone.Apps;
using BlaineRP.Client.Game.UI.CEF.Phone.Enums;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF.Phone
{
    [Script(int.MaxValue)]
    public class Phone
    {
        public static DateTime LastSent;

        private static Dictionary<AppTypes, string> AppsJsNames = new Dictionary<AppTypes, string>()
        {
            { AppTypes.Settings, "settings" },
            { AppTypes.Vehicles, "veh" },
            { AppTypes.Bank, "bank" },
            { AppTypes.BSim, "bsim" },
            { AppTypes.Camera, "camera" },
            { AppTypes.Navigator, "gps" },
            { AppTypes.Radio, "radio" },
            { AppTypes.Phone, "phone" },
            { AppTypes.Contacts, "contacts" },
            { AppTypes.SMS, "sms" },
            { AppTypes.Browser, "browser" },
            { AppTypes.Taxi, "cab" },
        };

        public Phone()
        {
            Events.Add("Phone::OpenApp",
                async (args) =>
                {
                    if (LastSent.IsSpam(250, false, false))
                        return;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    string appIdStr = args[0] is string str ? str : null;

                    AppTypes appType = appIdStr == null ? AppTypes.None : GetAppTypeByJsName(appIdStr.Replace("_app", ""));

                    LastSent = World.Core.ServerTime;

                    ShowApp(pData, appType);
                }
            );

            Events.Add("Phone::Tab",
                async (args) =>
                {
                    if (LastSent.IsSpam(250, false, false))
                        return;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    int appTab = args[0] is string str ? str.GetHashCode() : (int)args[0];

                    LastSent = World.Core.ServerTime;

                    if (CurrentApp == AppTypes.Phone)
                    {
                        if (appTab == 0) // callhist
                        {
                            List<(uint PhoneNumber, Apps.Phone.EndedCallStatusTypes Status)> callHist = Apps.Phone.CallHistory;

                            if (callHist.Count == 0)
                            {
                                Apps.Phone.ShowCallHistory(null);
                            }
                            else
                            {
                                List<uint> blackList = pData.PhoneBlacklist;

                                Apps.Phone.ShowCallHistory(callHist.Select(x => new object[]
                                        {
                                            (int)x.Status,
                                            x.PhoneNumber,
                                            GetContactNameByNumberNull(x.PhoneNumber),
                                            blackList.Contains(x.PhoneNumber),
                                        }
                                    )
                                );
                            }
                        }
                        else if (appTab == 1) // blacklist
                        {
                            List<uint> blacklist = pData.PhoneBlacklist;

                            if (blacklist.Count == 0)
                                Apps.Phone.ShowBlacklist(null, null);
                            else
                                Apps.Phone.ShowBlacklist(blacklist.Select(x => new object[]
                                        {
                                            x,
                                            GetContactNameByNumberNull(x),
                                        }
                                    )
                                );
                        }
                    }
                    else if (CurrentApp == AppTypes.Settings)
                    {
                        if (appTab == "wallpaper".GetHashCode())
                        {
                            Browser.Window.ExecuteCachedJs("Phone.switchTabSettings", "wallpaper", "Выбор обоев");

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

                            SMS.ShowWriteNew(receiverNum);
                        }
                        else if (appTab == 1) // openChat
                        {
                            var targetNum = uint.Parse(args[1].ToString());

                            object chatList = SMS.GetChatList(pData.AllSMS, targetNum, pData.PhoneNumber);

                            if (chatList == null)
                                return;

                            SMS.ShowChat(targetNum, chatList, GetContactNameByNumberNull(targetNum));
                        }
                    }
                    else if (CurrentApp == AppTypes.Contacts)
                    {
                        if (appTab == 0) // add contact
                            Contacts.ShowEdit(null, null);
                    }
                    else if (CurrentApp == AppTypes.Bank)
                    {
                        if (appTab == 0) // send money
                        {
                            CurrentAppTab = 0;

                            Browser.Window.ExecuteJs("Phone.drawBankTab", 0);
                        }
                        else if (appTab == 1) // house
                        {
                            House house = pData.OwnedHouses.FirstOrDefault();

                            if (house == null)
                            {
                                Notification.ShowError(Locale.Notifications.General.NoOwnedHouse);

                                return;
                            }

                            string[] resData = ((string)await Events.CallRemoteProc("Bank::GHA", house.Id))?.Split('_');

                            if (resData == null)
                                return;

                            var balance = ulong.Parse(resData[0]);
                            var maxPaidDays = uint.Parse(resData[1]);
                            var minPaidDays = uint.Parse(resData[2]);

                            ulong maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * house.Tax;
                            uint minBalance = minPaidDays * house.Tax;

                            CurrentAppTab = 1;

                            Browser.Window.ExecuteJs("Phone.drawBankTab",
                                1,
                                new object[]
                                {
                                    balance,
                                    house.Tax,
                                }
                            );

                            Apps.Bank.CurrentTransactionAction = async (amount) =>
                            {
                                if (balance >= maxBalance)
                                {
                                    Notification.ShowError(Locale.Notifications.Money.MaximalBalanceAlready);

                                    return;
                                }

                                ulong nBalance = maxBalance == ulong.MaxValue ? pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance : maxBalance - balance;

                                if (nBalance == 0)
                                {
                                    Notification.ShowError(Locale.Notifications.Money.NotEnough);

                                    return;
                                }

                                if ((decimal)balance + amount > maxBalance)
                                {
                                    Notification.ShowError(string.Format(Locale.Notifications.Money.MaximalBalanceNear, Locale.Get("GEN_MONEY_0", maxBalance - balance)));

                                    return;
                                }

                                object resObj = await Events.CallRemoteProc("Bank::HBC", true, house.Id, -1, amount, false, true);

                                if (resObj == null)
                                    return;

                                balance = Utils.Convert.ToUInt64(resObj);

                                Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tab-info", 0, balance);
                            };
                        }
                        else if (appTab == 2) // flat
                        {
                            Apartments house = pData.OwnedApartments.FirstOrDefault();

                            if (house == null)
                            {
                                Notification.ShowError(Locale.Notifications.General.NoOwnedApartments);

                                return;
                            }

                            string[] resData = ((string)await Events.CallRemoteProc("Bank::GAA", house.Id))?.Split('_');

                            if (resData == null)
                                return;

                            var balance = ulong.Parse(resData[0]);
                            var maxPaidDays = uint.Parse(resData[1]);
                            var minPaidDays = uint.Parse(resData[2]);

                            ulong maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * house.Tax;
                            uint minBalance = minPaidDays * house.Tax;

                            CurrentAppTab = 2;

                            Browser.Window.ExecuteJs("Phone.drawBankTab",
                                2,
                                new object[]
                                {
                                    balance,
                                    house.Tax,
                                }
                            );

                            Apps.Bank.CurrentTransactionAction = async (amount) =>
                            {
                                if (balance >= maxBalance)
                                {
                                    Notification.ShowError(Locale.Notifications.Money.MaximalBalanceAlready);

                                    return;
                                }

                                ulong nBalance = maxBalance == ulong.MaxValue ? pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance : maxBalance - balance;

                                if (nBalance == 0)
                                {
                                    Notification.ShowError(Locale.Notifications.Money.NotEnough);

                                    return;
                                }

                                if ((decimal)balance + amount > maxBalance)
                                {
                                    Notification.ShowError(string.Format(Locale.Notifications.Money.MaximalBalanceNear, Locale.Get("GEN_MONEY_0", maxBalance - balance)));

                                    return;
                                }

                                object resObj = await Events.CallRemoteProc("Bank::HBC", false, house.Id, -1, amount, false, true);

                                if (resObj == null)
                                    return;

                                balance = Utils.Convert.ToUInt64(resObj);

                                Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tab-info", 0, balance);
                            };
                        }
                        else if (appTab == 3) // garage
                        {
                            Garage garage = pData.OwnedGarages.FirstOrDefault();

                            if (garage == null)
                            {
                                Notification.ShowError(Locale.Notifications.General.NoOwnedGarage);

                                return;
                            }

                            string[] resData = ((string)await Events.CallRemoteProc("Bank::GGA", garage.Id))?.Split('_');

                            if (resData == null)
                                return;

                            var balance = ulong.Parse(resData[0]);
                            var maxPaidDays = uint.Parse(resData[1]);
                            var minPaidDays = uint.Parse(resData[2]);

                            ulong maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * garage.Tax;
                            uint minBalance = minPaidDays * garage.Tax;

                            CurrentAppTab = 3;

                            Browser.Window.ExecuteJs("Phone.drawBankTab",
                                3,
                                new object[]
                                {
                                    balance,
                                    garage.Tax,
                                }
                            );

                            Apps.Bank.CurrentTransactionAction = async (amount) =>
                            {
                                if (balance >= maxBalance)
                                {
                                    Notification.ShowError(Locale.Notifications.Money.MaximalBalanceAlready);

                                    return;
                                }

                                ulong nBalance = maxBalance == ulong.MaxValue ? pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance : maxBalance - balance;

                                if (nBalance == 0)
                                {
                                    Notification.ShowError(Locale.Notifications.Money.NotEnough);

                                    return;
                                }

                                if ((decimal)balance + amount > maxBalance)
                                {
                                    Notification.ShowError(string.Format(Locale.Notifications.Money.MaximalBalanceNear, Locale.Get("GEN_MONEY_0", maxBalance - balance)));

                                    return;
                                }

                                object resObj = await Events.CallRemoteProc("Bank::GBC", garage.Id, -1, amount, false, true);

                                if (resObj == null)
                                    return;

                                balance = Utils.Convert.ToUInt64(resObj);

                                Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tab-info", 0, balance);
                            };
                        }
                        else if (appTab == 4) // business
                        {
                            Business biz = pData.OwnedBusinesses.FirstOrDefault();

                            if (biz == null)
                            {
                                Notification.ShowError(Locale.Notifications.General.NoOwnedBusiness);

                                return;
                            }

                            string[] resData = ((string)await Events.CallRemoteProc("Bank::GBA", biz.Id))?.Split('_');

                            if (resData == null)
                                return;

                            var balance = ulong.Parse(resData[0]);
                            var maxPaidDays = uint.Parse(resData[1]);
                            var minPaidDays = uint.Parse(resData[2]);

                            ulong maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * biz.Rent;
                            uint minBalance = minPaidDays * biz.Rent;

                            CurrentAppTab = 4;

                            Browser.Window.ExecuteJs("Phone.drawBankTab",
                                4,
                                new object[]
                                {
                                    balance,
                                    biz.Rent,
                                }
                            );

                            Apps.Bank.CurrentTransactionAction = async (amount) =>
                            {
                                if (balance >= maxBalance)
                                {
                                    Notification.ShowError(Locale.Notifications.Money.MaximalBalanceAlready);

                                    return;
                                }

                                ulong nBalance = maxBalance == ulong.MaxValue ? pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance : maxBalance - balance;

                                if (nBalance == 0)
                                {
                                    Notification.ShowError(Locale.Notifications.Money.NotEnough);

                                    return;
                                }

                                if ((decimal)balance + amount > maxBalance)
                                {
                                    Notification.ShowError(string.Format(Locale.Notifications.Money.MaximalBalanceNear, Locale.Get("GEN_MONEY_0", maxBalance - balance)));

                                    return;
                                }

                                object resObj = await Events.CallRemoteProc("Bank::BBC", biz.Id, -1, amount, false, true);

                                if (resObj == null)
                                    return;

                                balance = Utils.Convert.ToUInt64(resObj);

                                Browser.Window.ExecuteJs("Phone.updateInfoLine", "bank-tab-info", 0, balance);
                            };
                        }
                    }
                    else if (CurrentApp == AppTypes.Navigator)
                    {
                        GPS.ShowTab((string)args[0]);
                    }
                }
            );

            Events.Add("Phone::Tooltip",
                async (args) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (LastSent.IsSpam(250, false, false))
                        return;

                    LastSent = World.Core.ServerTime;

                    var actId = int.Parse(args[1].ToString());
                    object elem = args[2];

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

                            List<(uint PhoneNumber, Apps.Phone.EndedCallStatusTypes Status)> callHist = Apps.Phone.CallHistory;

                            if (pcIdx < 0 || pcIdx >= callHist.Count)
                                return;

                            uint number = callHist[pcIdx].PhoneNumber;

                            if (actId == 0) // call
                            {
                                var numStr = number.ToString();

                                Apps.Phone.ShowDefault(numStr);

                                Apps.Phone.Call(numStr);
                            }
                            else if (actId == 1) // sms
                            {
                                List<SMS.Message> allSms = pData.AllSMS;
                                uint pNumber = pData.PhoneNumber;

                                object chatList = SMS.GetChatList(allSms, number, pNumber);

                                if (chatList != null)
                                    SMS.ShowChat(number, chatList, GetContactNameByNumberNull(number));
                                else
                                    SMS.ShowWriteNew(number.ToString());
                            }
                            else if (actId == 2) // add contact
                            {
                                Contacts.ShowEdit(number.ToString(), null);
                            }
                            else if (actId == 3) // add/remove blacklist
                            {
                                Apps.Phone.BlacklistChange(number, !pData.PhoneBlacklist.Contains(number));
                            }
                        }
                    }
                    else if (CurrentApp == AppTypes.Contacts)
                    {
                        //var number = uint.Parse(elem.ToString());

                        if (actId == 0) // call
                        {
                            var numStr = elem.ToString();

                            Apps.Phone.ShowDefault(numStr);

                            Apps.Phone.Call(numStr);
                        }
                        else if (actId == 1) // sms
                        {
                            var numberStr = elem.ToString();

                            var number = uint.Parse(numberStr);

                            List<SMS.Message> allSms = pData.AllSMS;
                            uint pNumber = pData.PhoneNumber;

                            object chatList = SMS.GetChatList(allSms, number, pNumber);

                            if (chatList != null)
                                SMS.ShowChat(number, chatList, GetContactNameByNumberNull(number));
                            else
                                SMS.ShowWriteNew(numberStr);
                        }
                        else if (actId == 2) // edit
                        {
                            var numberStr = elem.ToString();

                            var number = uint.Parse(numberStr);

                            Contacts.ShowEdit(numberStr, GetContactNameByNumber(number));
                        }
                        else if (actId == 3) // delete
                        {
                            var numberStr = elem.ToString();

                            var number = uint.Parse(numberStr);

                            if (!(bool)await Events.CallRemoteProc("Phone::RC", number))
                                return;

                            Dictionary<uint, string> allContacts = pData.Contacts;

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
                                Events.CallRemote("Vehicles::LOWNV", vid);
                            }
                            else if (actId == 1) // evacuate to house
                            {
                                House house = pData.OwnedHouses.Where(x => x.GarageType != null).FirstOrDefault();

                                if (house == null)
                                {
                                    Notification.ShowError(Locale.Notifications.General.NoOwnedHouseWGarage);

                                    return;
                                }

                                if (VehicleData.GetData(Player.LocalPlayer.Vehicle)?.VID == vid)
                                {
                                    Notification.ShowError(Locale.Notifications.General.QuitThisVehicle);

                                    return;
                                }

                                Events.CallRemote("Vehicles::EVAOWNV", vid, true, house.Id);
                            }
                            else if (actId == 2) // evacuate to garage
                            {
                                Garage garage = pData.OwnedGarages.FirstOrDefault();

                                if (garage == null)
                                {
                                    Notification.ShowError(Locale.Notifications.General.NoOwnedGarage);

                                    return;
                                }

                                if (VehicleData.GetData(Player.LocalPlayer.Vehicle)?.VID == vid)
                                {
                                    Notification.ShowError(Locale.Notifications.General.QuitThisVehicle);

                                    return;
                                }

                                Events.CallRemote("Vehicles::EVAOWNV", vid, false, garage.Id);
                            }
                        }
                        else if (vOType == "rented")
                        {
                            var rid = ushort.Parse(elem.ToString());

                            if (actId == 0) // locate
                            {
                                Events.CallRemote("Vehicles::LRENV", rid);
                            }
                            else if (actId == 1) // stop rent
                            {
                                if (Player.LocalPlayer.Vehicle?.RemoteId == rid)
                                {
                                    Notification.ShowError(Locale.Notifications.General.QuitThisVehicle);

                                    return;
                                }

                                Events.CallRemote("VRent::Cancel", rid);
                            }
                        }
                    }
                }
            );

            Events.Add("Phone::Transaction",
                async (args) =>
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

                        LastSent = World.Core.ServerTime;

                        object resObj = await Events.CallRemoteProc("Phone::AB", amount);

                        if (resObj == null)
                            return;

                        var res = Utils.Convert.ToUInt32(resObj);

                        Browser.Window.ExecuteJs("Phone.updateInfoLine", "bsim-app-info", 1, res);
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

                            var approveContext = $"BankSendToPlayer_{cid}_{amount}";
                            var approveTime = 5_000;

                            if (Notification.HasApproveTimedOut(approveContext, World.Core.ServerTime, approveTime))
                            {
                                Notification.SetCurrentApproveContext(approveContext, World.Core.ServerTime);

                                if ((bool)await Events.CallRemoteProc("Bank::Debit::Send", -1, cid, amount, true))
                                    ;
                                {
                                }
                            }
                            else
                            {
                                Notification.ClearAll();

                                Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                                await Events.CallRemoteProc("Bank::Debit::Send", -1, cid, amount, false);
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

                            Apps.Bank.CurrentTransactionAction?.Invoke(amount);
                        }
                    }
                }
            );

            Events.Add("Phone::UpdateToggle",
                (args) =>
                {
                    if (LastSent.IsSpam(250, false, false))
                        return;

                    var toggleId = (string)args[0];

                    var state = (bool)args[1];

                    if (CurrentApp == AppTypes.Settings)
                        if (toggleId == "disturb")
                        {
                            Settings.User.Other.PhoneNotDisturb = state;

                            ToggleDoNotDisturb(state);
                            /*                        if (state)
                                                    {
    
                                                    }
                                                    else
                                                    {
    
                                                    }*/
                        }
                }
            );
        }

        public static bool IsActive { get; private set; }

        public static AppTypes CurrentApp { get; set; }

        public static int CurrentAppTab { get; set; } = -1;

        public static object CurrentExtraData { get; set; }

        private static int EscBindIdx { get; set; } = -1;

        public static AppTypes GetAppTypeByJsName(string jsName)
        {
            return AppsJsNames.Where(x => x.Value == jsName).Select(x => x.Key).FirstOrDefault();
        }

        public static async void ShowApp(PlayerData pData, AppTypes appType)
        {
            if (pData == null)
            {
                pData = PlayerData.GetData(Player.LocalPlayer);

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
                    string[] res = ((string)await Events.CallRemoteProc("Phone::GPD"))?.Split('_');

                    if (res == null)
                        return;

                    BSim.Show(pData.PhoneNumber.ToString(), uint.Parse(res[0]), uint.Parse(res[1]), uint.Parse(res[2]));
                }
                else if (appType == AppTypes.Phone)
                {
                    Apps.Phone.ShowDefault(null);
                }
                else if (appType == AppTypes.Camera)
                {
                    if (Scripts.Misc.Phone.CanUsePhoneAnim(true) && !PlayerActions.IsAnyActionActive(true, PlayerActions.Types.InVehicle))
                        Apps.Camera.Show();
                }
                else if (appType == AppTypes.Settings)
                {
                    Apps.Settings.Show();
                }
                else if (appType == AppTypes.Bank)
                {
                    string[] resData = ((string)await Events.CallRemoteProc("Bank::PAGD"))?.Split('_');

                    if (resData == null)
                        return;

                    Apps.Bank.Show(((Bank.TariffTypes)int.Parse(resData[0])).ToString(), pData.BankBalance, decimal.Parse(resData[1]));
                }
                else if (appType == AppTypes.Vehicles)
                {
                    var ownedVehs = pData.OwnedVehicles.Select(x => new object[]
                                              {
                                                  x.VID,
                                                  $"{x.Data.BrandName}<br>{x.Data.SubName}<br>[#{x.VID}]",
                                                  x.Data.Type.ToString(),
                                              }
                                          )
                                         .ToList();

                    var rentedVehs = Scripts.Sync.Vehicles.RentedVehicle.All.Select(x => new object[]
                                                 {
                                                     x.RemoteId,
                                                     $"{x.VehicleData.BrandName}<br>{x.VehicleData.SubName}<br>[#{(uint)x.RemoteId + 10_000}]",
                                                     x.VehicleData.Type.ToString(),
                                                 }
                                             )
                                            .ToList();

                    Vehicles.Show(ownedVehs.Count > 0 ? ownedVehs : null, rentedVehs.Count > 0 ? rentedVehs : null);
                }
                else if (appType == AppTypes.Contacts)
                {
                    Dictionary<uint, string> allContacts = pData.Contacts;

                    if (allContacts.Count == 0)
                        Contacts.ShowAll(null);
                    else
                        Contacts.ShowAll(allContacts.OrderBy(x => x.Value)
                                                    .Select(x => new object[]
                                                         {
                                                             x.Value,
                                                             x.Key,
                                                         }
                                                     )
                        );
                }
                else if (appType == AppTypes.SMS)
                {
                    List<SMS.Message> allSms = pData.AllSMS;

                    if (allSms.Count == 0)
                    {
                        SMS.ShowPreviews(null);
                    }
                    else
                    {
                        uint pNumber = pData.PhoneNumber;

                        SMS.ShowPreviews(SMS.GetSMSPreviews(allSms, pNumber)
                                            .Select(x => new object[]
                                                 {
                                                     x.Key,
                                                     GetContactNameByNumberNull(x.Key),
                                                     x.Value.Date.ToString("HH:mm"),
                                                     x.Value.Text,
                                                 }
                                             )
                        );
                    }
                }
                else if (appType == AppTypes.Taxi)
                {
                    Taxi.Show(pData);
                }
                else if (appType == AppTypes.Radio)
                {
                    Radio.Show();
                }
                else if (appType == AppTypes.Navigator)
                {
                    GPS.Show();
                }
            }
        }

        public static void Show()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.ActiveCall is Apps.Phone.CallInfo callInfo)
                Apps.Phone.ShowIncomingCall(GetContactNameByNumber(callInfo.Number));

            IsActive = true;

            if (HUD.SpeedometerEnabled)
                Browser.Switch(Browser.IntTypes.HUD_Speedometer, false);

            if (Browser.IsActive(Browser.IntTypes.HUD_Help))
                Browser.Switch(Browser.IntTypes.HUD_Help, false);

            Browser.SwitchTemp(Browser.IntTypes.Phone, true);

            Browser.Window.ExecuteCachedJs("Phone.showPhone", true);

            Cursor.Show(true, true);

            EscBindIdx = Input.Core.Bind(RAGE.Ui.VirtualKeys.Escape,
                true,
                () =>
                {
                    if (Chat.InputVisible)
                        return;

                    if (ActionBox.CurrentContextStr != null &&
                        (ActionBox.CurrentContextStr == "PhonePoliceCallInput" ||
                         ActionBox.CurrentContextStr == "PhoneMedicalCallInput" ||
                         ActionBox.CurrentContextStr == "Phone911Select"))
                    {
                        ActionBox.Close(false);

                        return;
                    }

                    Scripts.Misc.Phone.Toggle();
                }
            );
        }

        public static void Close()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (pData.ActiveCall != null)
                ShowApp(pData, AppTypes.None);

            IsActive = false;

            Browser.Window.ExecuteCachedJs("Phone.showPhone", false);

            if (HUD.SpeedometerMustBeEnabled)
                Browser.Switch(Browser.IntTypes.HUD_Speedometer, true);

            if (HUD.IsActive && !Settings.User.Interface.HideHints)
                Browser.Switch(Browser.IntTypes.HUD_Help, true);

            if (ActionBox.CurrentContextStr != null &&
                (ActionBox.CurrentContextStr == "PhonePoliceCallInput" ||
                 ActionBox.CurrentContextStr == "PhoneMedicalCallInput" ||
                 ActionBox.CurrentContextStr == "Phone911Select"))
                ActionBox.Close();

            Cursor.Show(false, false);

            Input.Core.Unbind(EscBindIdx);

            EscBindIdx = -1;
        }

        public static void Preload()
        {
            Browser.Switch(Browser.IntTypes.Phone, true);

            SetWallpaper(Settings.User.Other.PhoneWallpaperNum);

            Browser.Window.ExecuteJs("Phone.setDisturb", Settings.User.Other.PhoneNotDisturb);
        }

        public static void UpdateTime()
        {
            Browser.Window.ExecuteJs("Phone.setTime", World.Core.ServerTime.ToString("HH:mm dd.MM.yyyy"));
        }

        public static void SetWallpaper(int num)
        {
            Browser.Window.ExecuteJs("Phone.setWallpaper", num);
        }

        public static void ToggleDoNotDisturb(bool state)
        {
            Browser.Window.ExecuteJs("Phone.updateDisturb", state);
        }

        public static void SwitchMenu(bool state)
        {
            CurrentAppTab = -1;

            Browser.Window.ExecuteCachedJs("Phone.showMenu", state);
        }

        public static string GetContactNameByNumberNull(uint number)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return null;

            string defName = Locale.General.DefaultNumbersNames.GetValueOrDefault(number);

            if (defName != null)
                return defName;

            return pData.Contacts.GetValueOrDefault(number);
        }

        public static string GetContactNameByNumber(uint number)
        {
            return GetContactNameByNumberNull(number) ?? number.ToString();
        }
    }
}