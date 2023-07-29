using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Players;
using BlaineRP.Client.Game.World;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class SMS
    {
        public SMS()
        {
            Events.Add("Phone::SendCoords",
                (args) =>
                {
                    if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
                        return;

                    if (args == null || args.Length < 1)
                    {
                        Notification.ShowError(Locale.Notifications.General.WrongCoordsSms);

                        return;
                    }

                    string[] coordsData = ((string)args[0])?.Split('_');

                    if (coordsData.Length < 2)
                    {
                        Notification.ShowError(Locale.Notifications.General.WrongCoordsSms);

                        return;
                    }

                    float x, y;

                    if (!float.TryParse(coordsData[0], out x) || !float.TryParse(coordsData[1], out y))
                    {
                        Notification.ShowError(Locale.Notifications.General.WrongCoordsSms);

                        return;
                    }

                    CEF.Phone.Phone.LastSent = Core.ServerTime;

                    var coords = new Vector3(x, y, 0f);

                    Helpers.Blips.Core.CreateGPS(coords, Player.LocalPlayer.Dimension, true);
                }
            );

            Events.Add("Phone::SmsSend",
                async (args) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
                        return;

                    if (args == null || args.Length < 2)
                        return;

                    if (args.Length > 2 && (bool)args[2])
                    {
                        CEF.Phone.Phone.LastSent = Core.ServerTime;

                        AttachPos = !AttachPos;

                        if (AttachPos)
                            Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.SmsSendAttachPosOn);
                        else
                            Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.SmsSendAttachPosOff);

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

                    string text = ((string)args[1])?.Trim();

                    if (!text.IsTextLengthValid(Client.Settings.App.Static.PHONE_SMS_MIN_LENGTH, Client.Settings.App.Static.PHONE_SMS_MAX_LENGTH, true))
                        return;

                    CEF.Phone.Phone.LastSent = Core.ServerTime;

                    var smsStrData = (string)await Events.CallRemoteProc("Phone::SSMS", number, text, AttachPos);

                    if (smsStrData == null)
                    {
                        return;
                    }
                    else if (smsStrData.Length == 0)
                    {
                        Notification.ShowError(Locale.Notifications.General.SmsCantBeSentNow);

                        return;
                    }

                    var smsObj = new Message(smsStrData);

                    List<Message> allSms = pData.AllSMS;

                    uint pNumber = pData.PhoneNumber;

                    allSms.Add(smsObj);

                    object chatList = GetChatList(allSms, smsObj.ReceiverNumber, pNumber);

                    if (chatList != null)
                        ShowChat(smsObj.ReceiverNumber, chatList, CEF.Phone.Phone.GetContactNameByNumberNull(smsObj.ReceiverNumber));
                    else
                        CEF.Phone.Phone.ShowApp(pData, AppType.SMS);
                }
            );

            Events.Add("Phone::SmsDelete",
                async (args) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (CEF.Phone.Phone.LastSent.IsSpam(250, false, true))
                        return;

                    var number = uint.Parse(args[0].ToString());

                    var approveContext = $"PhoneSmsDelete_{number}";
                    var approveTime = 5_000;

                    if (Notification.HasApproveTimedOut(approveContext, Core.ServerTime, approveTime))
                    {
                        if (CEF.Phone.Phone.LastSent.IsSpam(1_500, false, true))
                            return;

                        CEF.Phone.Phone.LastSent = Core.ServerTime;

                        Notification.SetCurrentApproveContext(approveContext, Core.ServerTime);

                        Notification.Show(Notification.Types.Question, Locale.Get("NOTIFICATION_HEADER_APPROVE"), Locale.Notifications.General.SmsDeleteConfirmText, approveTime);
                    }
                    else
                    {
                        Notification.ClearAll();

                        Notification.SetCurrentApproveContext(null, DateTime.MinValue);

                        var numsToDel = new Dictionary<byte, Message>();

                        List<Message> allSms = pData.AllSMS;

                        for (var i = 0; i < allSms.Count; i++)
                        {
                            if (allSms[i].SenderNumber == number || allSms[i].ReceiverNumber == number)
                                numsToDel.Add((byte)i, allSms[i]);
                        }

                        if (numsToDel.Count == 0)
                            return;

                        CEF.Phone.Phone.LastSent = Core.ServerTime;

                        if (!(bool)await Events.CallRemoteProc("Phone::DSMS", numsToDel.Keys.ToArray()))
                            return;

                        foreach (Message x in numsToDel.Values)
                        {
                            allSms.Remove(x);
                        }

                        CEF.Phone.Phone.ShowApp(pData, AppType.SMS);
                    }
                }
            );

            Events.Add("Phone::CSMS",
                (args) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    List<Message> allSms = pData.AllSMS;

                    if (args[0] is string strA)
                    {
                        var smsObj = new Message(strA);

                        allSms.Add(smsObj);

                        string contName = CEF.Phone.Phone.GetContactNameByNumberNull(smsObj.SenderNumber);

                        if (CEF.Phone.Phone.CurrentApp == AppType.SMS)
                        {
                            uint pNumber = pData.PhoneNumber;

                            if (CEF.Phone.Phone.CurrentAppTab == 1 && CEF.Phone.Phone.CurrentExtraData as uint? == smsObj.SenderNumber)
                            {
                                object chatList = GetChatList(allSms, smsObj.SenderNumber, pNumber);

                                if (chatList != null)
                                {
                                    ShowChat(smsObj.SenderNumber, chatList, contName);

                                    return;
                                }
                            }
                            else if (CEF.Phone.Phone.CurrentAppTab == -1)
                            {
                                CEF.Phone.Phone.ShowApp(pData, AppType.SMS);
                            }
                        }

                        if (Client.Settings.User.Other.PhoneNotDisturb)
                            return;

                        if (HUD.IsActive)
                        {
                            if (contName == null)
                                contName = smsObj.SenderNumber.ToString();

                            if (smsObj.SenderNumber == 900)
                                Notification.ShowSmsFive(Notification.FiveNotificImgTypes.Bank, contName, smsObj.Text);
                            else if (smsObj.SenderNumber == 873)
                                Notification.ShowSmsFive(Notification.FiveNotificImgTypes.DeliveryService, contName, smsObj.Text);
                            else
                                Notification.ShowSmsFive(Notification.FiveNotificImgTypes.Default, contName, smsObj.Text);
                        }
                    }
                    else
                    {
                        var smsNum = (int)args[0];

                        if (smsNum < 0 || smsNum >= allSms.Count)
                            return;

                        Message smsObj = allSms[smsNum];

                        allSms.Remove(smsObj);

                        if (CEF.Phone.Phone.CurrentApp == AppType.SMS)
                        {
                            uint pNumber = pData.PhoneNumber;

                            uint tNumber = pNumber == smsObj.SenderNumber ? smsObj.ReceiverNumber : smsObj.SenderNumber;

                            string contName = CEF.Phone.Phone.GetContactNameByNumberNull(tNumber);

                            bool b = CEF.Phone.Phone.CurrentAppTab == 1 && CEF.Phone.Phone.CurrentExtraData as uint? == tNumber;

                            if (b)
                            {
                                object chatList = GetChatList(allSms, tNumber, pNumber);

                                if (chatList != null)
                                {
                                    ShowChat(tNumber, chatList, contName);

                                    return;
                                }
                            }

                            if (CEF.Phone.Phone.CurrentAppTab == -1 || b)
                                CEF.Phone.Phone.ShowApp(pData, AppType.SMS);
                        }
                    }
                }
            );
        }

        public static DateTime LastSMSDeleteApproveTime { get; set; }

        public static bool AttachPos { get; set; }

        //messages[i] = [phone_number, 'contact' || null, 'date', 'message']
        public static void ShowPreviews(object messages)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.SMS;

            CEF.Phone.Phone.CurrentAppTab = -1;

            if (messages != null)
                Browser.Window.ExecuteJs("Phone.drawSmsApp", messages);
            else
                Browser.Window.ExecuteJs("Phone.drawSmsApp();");
        }

        public static void ShowWriteNew(string pNumber)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            if (CEF.Phone.Phone.CurrentApp != AppType.SMS)
            {
                Browser.Window.ExecuteJs("Phone.drawSmsApp();");

                CEF.Phone.Phone.CurrentApp = AppType.SMS;
            }

            AttachPos = false;

            CEF.Phone.Phone.CurrentAppTab = 0;

            if (pNumber == null)
                Browser.Window.ExecuteJs("Phone.fillTyping();");
            else
                Browser.Window.ExecuteJs("Phone.fillTyping", pNumber);
        }

        public static void ShowChat(uint tNumber, object chatList, string tContactName)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            if (CEF.Phone.Phone.CurrentApp != AppType.SMS)
            {
                Browser.Window.ExecuteJs("Phone.drawSmsApp();");

                CEF.Phone.Phone.CurrentApp = AppType.SMS;
            }

            CEF.Phone.Phone.CurrentAppTab = 1;

            CEF.Phone.Phone.CurrentExtraData = tNumber;

            Browser.Window.ExecuteJs("Phone.fillFullSms",
                new object[]
                {
                    new object[]
                    {
                        tNumber,
                        chatList,
                        tContactName,
                    },
                }
            );
        }

        public static Dictionary<uint, Message> GetSMSPreviews(List<Message> list, uint pNumber)
        {
            var dict = new Dictionary<uint, Message>();

            list.ForEach(x =>
                {
                    uint targetNumber = x.SenderNumber == pNumber ? x.ReceiverNumber : x.SenderNumber;

                    Message lastSms = dict.GetValueOrDefault(targetNumber);

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
                }
            );

            return dict;
        }

        public static object GetChatList(List<Message> list, uint tNumber, uint pNumber)
        {
            var res = list.Where(x => x.SenderNumber == tNumber || x.ReceiverNumber == tNumber)
                          .OrderBy(x => x.Date)
                          .Select(x => new object[]
                               {
                                   x.SenderNumber == pNumber,
                                   x.Text,
                                   x.Date.ToString("HH:mm"),
                               }
                           )
                          .ToList();

            return res.Count > 0 ? res : null;
        }

        public class Message
        {
            public Message(string DataStr)
            {
                string[] data = DataStr.Split('_');

                SenderNumber = uint.Parse(data[0]);

                ReceiverNumber = uint.Parse(data[1]);

                Date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(data[2])).DateTime;

                Text = string.Join('_', data.Skip(3));
            }

            public uint SenderNumber { get; set; }

            public uint ReceiverNumber { get; set; }

            public string Text { get; set; }

            public DateTime Date { get; set; }
        }
    }
}