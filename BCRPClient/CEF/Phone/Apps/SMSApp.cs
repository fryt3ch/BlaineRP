using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using static BCRPClient.CEF.Phone;

namespace BCRPClient.CEF.PhoneApps
{
    public class SMSApp : Events.Script
    {
        public class SMS
        {
            public uint SenderNumber { get; set; }

            public uint ReceiverNumber { get; set; }

            public string Text { get; set; }

            public DateTime Date { get; set; }

            public SMS(string DataStr)
            {
                var data = DataStr.Split('_');

                SenderNumber = uint.Parse(data[0]);

                ReceiverNumber = uint.Parse(data[1]);

                Date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(data[2])).DateTime;

                Text = string.Join('_', data.Skip(3));
            }
        }

        public static DateTime LastSMSDeleteApproveTime { get; set; }

        public static bool AttachPos { get; set; }

        public SMSApp()
        {
            Events.Add("Phone::SendCoords", (args) =>
            {
                if (LastSent.IsSpam(250, false, false))
                    return;

                if (args == null || args.Length < 1)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.WrongCoordsSms);

                    return;
                }

                var coordsData = ((string)args[0])?.Split('_');

                if (coordsData.Length < 2)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.WrongCoordsSms);

                    return;
                }

                float x, y;

                if (!float.TryParse(coordsData[0], out x) || !float.TryParse(coordsData[1], out y))
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.WrongCoordsSms);

                    return;
                }

                LastSent = Sync.World.ServerTime;

                var coords = new Vector3(x, y, 0f);

                Additional.ExtraBlips.CreateGPS(coords, Player.LocalPlayer.Dimension, true);
            });

            Events.Add("Phone::SmsSend", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (LastSent.IsSpam(250, false, false))
                    return;

                if (args == null || args.Length < 2)
                    return;

                if (args.Length > 2 && (bool)args[2])
                {
                    LastSent = Sync.World.ServerTime;

                    AttachPos = !AttachPos;

                    if (AttachPos)
                    {
                        CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, Locale.Notifications.General.SmsSendAttachPosOn);
                    }
                    else
                    {
                        CEF.Notification.Show(Notification.Types.Information, Locale.Notifications.DefHeader, Locale.Notifications.General.SmsSendAttachPosOff);
                    }

                    return;
                }

                var numberStr = args[0]?.ToString();

                if (numberStr == null)
                    return;

                decimal numberD = 0;

                if (!decimal.TryParse(numberStr, out numberD))
                    return;

                uint number = 0;

                if (!numberD.IsNumberValid(1, uint.MaxValue, out number, false))
                    return;

                var text = ((string)args[1])?.Trim();

                if (!text.IsTextLengthValid(Settings.PHONE_SMS_MIN_LENGTH, Settings.PHONE_SMS_MAX_LENGTH, true))
                    return;

                LastSent = Sync.World.ServerTime;

                var smsStrData = (string)await Events.CallRemoteProc("Phone::SSMS", number, text, AttachPos);

                if (smsStrData == null)
                {
                    return;
                }
                else if (smsStrData.Length == 0)
                {
                    CEF.Notification.Show(Notification.Types.Error, Locale.Notifications.ErrorHeader, Locale.Notifications.General.SmsCantBeSentNow);

                    return;
                }

                var smsObj = new PhoneApps.SMSApp.SMS(smsStrData);

                var allSms = pData.AllSMS;

                var pNumber = pData.PhoneNumber;

                allSms.Add(smsObj);

                var chatList = PhoneApps.SMSApp.GetChatList(allSms, smsObj.ReceiverNumber, pNumber);

                if (chatList != null)
                {
                    PhoneApps.SMSApp.ShowChat(smsObj.ReceiverNumber, chatList, GetContactNameByNumberNull(smsObj.ReceiverNumber));
                }
                else
                {
                    Phone.ShowApp(pData, AppTypes.SMS);
                }
            });

            Events.Add("Phone::SmsDelete", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (LastSent.IsSpam(250, false, false))
                    return;

                var number = uint.Parse(args[0].ToString());

                var timePassed = Sync.World.ServerTime.Subtract(PhoneApps.SMSApp.LastSMSDeleteApproveTime).TotalMilliseconds;

                if (timePassed > 5000)
                {
                    CEF.Notification.Show(Notification.Types.Question, Locale.Notifications.ApproveHeader, Locale.Notifications.General.SmsDeleteConfirmText, 5000);

                    PhoneApps.SMSApp.LastSMSDeleteApproveTime = Sync.World.ServerTime;

                    return;
                }

                PhoneApps.SMSApp.LastSMSDeleteApproveTime = DateTime.MinValue;

                if (timePassed < 5000)
                    CEF.Notification.ClearAll();

                var numsToDel = new Dictionary<byte, PhoneApps.SMSApp.SMS>();

                var allSms = pData.AllSMS;

                for (int i = 0; i < allSms.Count; i++)
                {
                    if (allSms[i].SenderNumber == number || allSms[i].ReceiverNumber == number)
                        numsToDel.Add((byte)i, allSms[i]);
                }

                if (numsToDel.Count == 0)
                    return;

                LastSent = Sync.World.ServerTime;

                if (!(bool)await Events.CallRemoteProc("Phone::DSMS", numsToDel.Keys.ToArray()))
                    return;

                foreach (var x in numsToDel.Values)
                {
                    allSms.Remove(x);
                }

                Phone.ShowApp(pData, AppTypes.SMS);
            });

            Events.Add("Phone::CSMS", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var allSms = pData.AllSMS;

                if (args[0] is string strA)
                {
                    var smsObj = new PhoneApps.SMSApp.SMS(strA);

                    allSms.Add(smsObj);

                    var contName = GetContactNameByNumberNull(smsObj.SenderNumber);

                    if (CurrentApp == AppTypes.SMS)
                    {
                        var pNumber = pData.PhoneNumber;

                        if (CurrentAppTab == 1 && (CurrentExtraData as uint?) == smsObj.SenderNumber)
                        {
                            var chatList = PhoneApps.SMSApp.GetChatList(allSms, smsObj.SenderNumber, pNumber);

                            if (chatList != null)
                            {
                                PhoneApps.SMSApp.ShowChat(smsObj.SenderNumber, chatList, contName);

                                return;
                            }
                        }
                        else if (CurrentAppTab == -1)
                        {
                            Phone.ShowApp(pData, AppTypes.SMS);
                        }
                    }

                    if (Settings.Other.PhoneNotDisturb)
                        return;

                    if (CEF.HUD.IsActive)
                    {
                        if (contName == null)
                            contName = smsObj.SenderNumber.ToString();

                        if (smsObj.SenderNumber == 900)
                        {
                            CEF.Notification.ShowSmsFive(Notification.FiveNotificImgTypes.Bank, contName, smsObj.Text);
                        }
                        else if (smsObj.SenderNumber == 873)
                        {
                            CEF.Notification.ShowSmsFive(Notification.FiveNotificImgTypes.DeliveryService, contName, smsObj.Text);
                        }
                        else
                        {
                            CEF.Notification.ShowSmsFive(Notification.FiveNotificImgTypes.Default, contName, smsObj.Text);
                        }
                    }
                }
                else
                {
                    var smsNum = (int)args[0];

                    if (smsNum < 0 || smsNum >= allSms.Count)
                        return;

                    var smsObj = allSms[smsNum];

                    allSms.Remove(smsObj);

                    if (CurrentApp == AppTypes.SMS)
                    {
                        var pNumber = pData.PhoneNumber;

                        var tNumber = pNumber == smsObj.SenderNumber ? smsObj.ReceiverNumber : smsObj.SenderNumber;

                        var contName = GetContactNameByNumberNull(tNumber);

                        var b = CurrentAppTab == 1 && (CurrentExtraData as uint?) == tNumber;

                        if (b)
                        {
                            var chatList = PhoneApps.SMSApp.GetChatList(allSms, tNumber, pNumber);

                            if (chatList != null)
                            {
                                PhoneApps.SMSApp.ShowChat(tNumber, chatList, contName);

                                return;
                            }
                        }

                        if (CurrentAppTab == -1 || b)
                        {
                            Phone.ShowApp(pData, AppTypes.SMS);
                        }
                    }
                }
            });
        }

        //messages[i] = [phone_number, 'contact' || null, 'date', 'message']
        public static void ShowPreviews(object messages)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.SMS;

            Phone.CurrentAppTab = -1;

            if (messages != null)
                CEF.Browser.Window.ExecuteJs("Phone.drawSmsApp", messages);
            else
                CEF.Browser.Window.ExecuteJs("Phone.drawSmsApp();");
        }

        public static void ShowWriteNew(string pNumber)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            if (Phone.CurrentApp != AppTypes.SMS)
            {
                CEF.Browser.Window.ExecuteJs("Phone.drawSmsApp();");

                Phone.CurrentApp = Phone.AppTypes.SMS;
            }

            AttachPos = false;

            Phone.CurrentAppTab = 0;

            if (pNumber == null)
                CEF.Browser.Window.ExecuteJs("Phone.fillTyping();");
            else
                CEF.Browser.Window.ExecuteJs("Phone.fillTyping", pNumber);
        }

        public static void ShowChat(uint tNumber, object chatList, string tContactName)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            if (Phone.CurrentApp != AppTypes.SMS)
            {
                CEF.Browser.Window.ExecuteJs("Phone.drawSmsApp();");

                Phone.CurrentApp = Phone.AppTypes.SMS;
            }

            Phone.CurrentAppTab = 1;

            Phone.CurrentExtraData = tNumber;

            CEF.Browser.Window.ExecuteJs("Phone.fillFullSms", new object[] { new object[] { tNumber, chatList, tContactName } });
        }

        public static Dictionary<uint, SMS> GetSMSPreviews(List<SMS> list, uint pNumber)
        {
            var dict = new Dictionary<uint, CEF.PhoneApps.SMSApp.SMS>();

            list.ForEach(x =>
            {
                var targetNumber = x.SenderNumber == pNumber ? x.ReceiverNumber : x.SenderNumber;

                var lastSms = dict.GetValueOrDefault(targetNumber);

                if (lastSms != null)
                {
                    if (lastSms.Date > x.Date)
                        return;

                    dict[targetNumber] = x;
                }
                else
                {
                    dict.Add(targetNumber, x);
                }
            });

            return dict;
        }

        public static object GetChatList(List<SMS> list, uint tNumber, uint pNumber)
        {
            var res = list.Where(x => x.SenderNumber == tNumber || x.ReceiverNumber == tNumber).OrderBy(x => x.Date).Select(x => new object[] { x.SenderNumber == pNumber, x.Text, x.Date.ToString("HH:mm") }).ToList();

            return res.Count > 0 ? res : null;
        }
    }
}

