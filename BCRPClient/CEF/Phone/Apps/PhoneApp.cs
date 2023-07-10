using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static BCRPClient.CEF.Phone;

namespace BCRPClient.CEF.PhoneApps
{
    public class PhoneApp : Events.Script
    {
        private static AsyncTask ActiveCallUpdateTask { get; set; }

        public static Dictionary<string[], Action> DefaultNumbersActions { get; private set; } = new Dictionary<string[], Action>()
        {
            {
                new string[] { "*100#", "*102#", "*105#" },

                async () =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    var res = ((string)await Events.CallRemoteProc("Phone::GPD"))?.Split('_');

                    if (res == null)
                        return;

                    CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.Money.PhoneBalanceInfo, pData.PhoneNumber, Utils.GetPriceString(decimal.Parse(res[0])), Utils.GetPriceString(decimal.Parse(res[1])), Utils.GetPriceString(decimal.Parse(res[2]))));
                }
            },

            {
                new string[] { "911", "112" },

                async () =>
                {
                    await CEF.ActionBox.ShowSelect
                    (
                        "Phone911Select", Locale.Get("PHONE_ECALL_0"), new (decimal Id, string Text)[] { (102, Locale.Get("PHONE_ECALL_1")), (103, Locale.Get("PHONE_ECALL_2")) }, null, null,

                        () =>
                        {
                            if (CEF.Phone.IsActive)
                                CEF.Browser.SwitchTemp(Browser.IntTypes.Phone, false);
                        },

                        async (rType, idD) =>
                        {
                            CEF.ActionBox.Close(false);

                            if (rType == ActionBox.ReplyTypes.Cancel)
                                return;

                            var idStr = idD.ToString();

                            var action = DefaultNumbersActions.Where(x => x.Key.Contains(idStr)).Select(x => x.Value).FirstOrDefault();

                            action?.Invoke();
                        },

                        () =>
                        {
                            if (CEF.Phone.IsActive)
                            {
                                CEF.Browser.SwitchTemp(Browser.IntTypes.Phone, true);

                                CEF.Cursor.Show(true, true);
                            }
                        }
                    );
                }
            },

            {
                new string[] { "102" },

                async () =>
                {
                    await CEF.ActionBox.ShowInputWithText
                    (
                        "PhonePoliceCallInput", Locale.Get("PHONE_ECALL_3"), Locale.Get("PHONE_ECALL_5"), 24, "", null, null,

                        () =>
                        {
                            if (CEF.Phone.IsActive)
                                CEF.Browser.SwitchTemp(Browser.IntTypes.Phone, false);
                        },

                        async (rType, str) =>
                        {
                            if (rType == ActionBox.ReplyTypes.Cancel)
                            {
                                CEF.ActionBox.Close(false);

                                return;
                            }

                            str = str?.Trim();

                            var pattern = new Regex(@"^[A-Za-zА-Яа-я0-9,./?$#@!%^&*()'+=\-\[\]]{1,24}$");

                            if (str == null || !pattern.IsMatch(str))
                            {
                                CEF.Notification.Show("Str::NM");

                                return;
                            }

                            var res = Utils.ToByte(await Events.CallRemoteProc("Police::Call", str));

                            if (res == 255)
                            {
                                CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("PHONE_ECALL_S_0"));

                                CEF.ActionBox.Close(false);

                                var pos = Player.LocalPlayer.GetCoords(false);

                                pos.Z -= 1f;

                                var cs1 = Player.LocalPlayer.GetData<Additional.ExtraColshape>("PoliceCallWaitCs");

                                if (cs1 != null)
                                {
                                    (cs1.Data as Additional.ExtraColshape)?.Destroy();

                                    cs1.Destroy();
                                }

                                Additional.ExtraColshape cs2 = null;

                                cs1 = new Additional.Cylinder(pos, Settings.POLICE_CALL_MAX_WAIT_RANGE / 2, 10f, false, Utils.RedColor, Settings.MAIN_DIMENSION)
                                {
                                    OnExit = (cancel) =>
                                    {
                                        if (cs1?.Exists != true)
                                            return;

                                        CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("PHONE_ECALL_W_0", Settings.POLICE_CALL_MAX_WAIT_RANGE / 2));
                                    }
                                };

                                cs2 = new Additional.Cylinder(pos, Settings.POLICE_CALL_MAX_WAIT_RANGE, 10f, false, new Utils.Colour(0, 0, 255, 25), Settings.MAIN_DIMENSION)
                                {
                                    OnExit = async (cancel) =>
                                    {
                                        if (cs2?.Exists != true)
                                            return;

                                        var resCancel = await Events.CallRemoteProc("Police::Call", string.Empty);
                                    },

                                    Data = cs1,
                                };

                                Player.LocalPlayer.SetData("PoliceCallWaitCs", cs2);
                            }
                            else
                            {
                                if (res == 1)
                                    CEF.Notification.ShowError(Locale.Get("PHONE_ECALL_E_1"));
                                else if (res == 0)
                                    CEF.Notification.ShowError(Locale.Get("PHONE_ECALL_E_0"));
                            }
                        },

                        () =>
                        {
                            if (CEF.Phone.IsActive)
                            {
                                CEF.Browser.SwitchTemp(Browser.IntTypes.Phone, true);

                                CEF.Cursor.Show(true, true);
                            }
                        }
                    );
                }
            },

            {
                new string[] { "103" },

                async () =>
                {
                    await CEF.ActionBox.ShowInputWithText
                    (
                        "PhoneMedicalCallInput", Locale.Get("PHONE_ECALL_4"), Locale.Get("PHONE_ECALL_6"), 24, "", null, null,

                        () =>
                        {
                            if (CEF.Phone.IsActive)
                                CEF.Browser.SwitchTemp(Browser.IntTypes.Phone, false);
                        },

                        async (rType, str) =>
                        {
                            if (rType == ActionBox.ReplyTypes.Cancel)
                            {
                                CEF.ActionBox.Close(false);

                                return;
                            }

                            str = str?.Trim();

                            var pattern = new Regex(@"^[A-Za-zА-Яа-я0-9,./?$#@!%^&*()'+=\-\[\]]{1,24}$");

                            if (str == null || !pattern.IsMatch(str))
                            {
                                CEF.Notification.Show("Str::NM");

                                return;
                            }

                            CEF.ActionBox.Close(false);

                            // call remote
                        },

                        () =>
                        {
                            if (CEF.Phone.IsActive)
                            {
                                CEF.Browser.SwitchTemp(Browser.IntTypes.Phone, true);

                                CEF.Cursor.Show(true, true);
                            }
                        }
                    );
                }
            },
        };

        public class CallInfo
        {
            public bool IsMeCaller { get; set; }

            public Player Player { get; set; }

            public uint Number { get; set; }

            public DateTime StartDate { get; set; }

            public CallInfo(bool IsMeCaller, uint Number)
            {
                this.IsMeCaller = IsMeCaller;
                this.Number = Number;
            }
        }

        public enum EndedCallStatusTypes
        {
            OutgoingSuccess = 0,
            OutgoingError = 1,
            OutgoingBusy = 2,

            IncomingSuccess = 3,
            IncomingError = 4,
            IncomingBusy = 5,
        }

        public enum CancelTypes : byte
        {
            /// <summary>Вызов отменен сервером</summary>
            ServerAuto = 0,
            /// <summary>Вызов отменен первым игроком</summary>
            Caller,
            /// <summary>Вызов отменен вторым игроком</summary>
            Receiver,
            /// <summary>Вызов отменен по причине недостатка средств</summary>
            NotEnoughBalance,
        }

        public static List<(uint PhoneNumber, EndedCallStatusTypes Status)> CallHistory { get; private set; } = new List<(uint PhoneNumber, EndedCallStatusTypes Status)>();

        public static void AddToCallHistory(uint phoneNumber, EndedCallStatusTypes status)
        {
            if (CallHistory.Count >= 50)
                CallHistory.RemoveAt(0);

            CallHistory.Add((phoneNumber, status));
        }

        public PhoneApp()
        {
            Events.Add("Phone::Call", (args) =>
            {
                if (LastSent.IsSpam(250, false, false))
                    return;

                var number = args[0]?.ToString();

                if (number == null || number.Length == 0)
                    return;

                LastSent = Sync.World.ServerTime;

                PhoneApps.PhoneApp.Call(number);
            });

            Events.Add("Phone::ReplyCall", (args) =>
            {
                if (LastSent.IsSpam(250, false, false))
                    return;

                var ans = (bool)args[0];

                LastSent = Sync.World.ServerTime;

                Events.CallRemote("Phone::CA", ans);
            });

            Events.Add("Phone::BlacklistChange", async (args) =>
            {
                if (LastSent.IsSpam(250, false, false))
                    return;

                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (args == null || args.Length < 2)
                    return;

                var add = (bool)args[1];

                var numberS = args[0]?.ToString();

                if (numberS == null)
                    return;

                decimal numberD;

                if (!decimal.TryParse(numberS, out numberD))
                    return;

                uint number;

                if (!numberD.IsNumberValid(1, uint.MaxValue, out number, true))
                    return;

                BlacklistChange(number, add);
            });

            Events.Add("Phone::ACS", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (args[0] is bool state)
                {
                    if (state)
                    {
                        var callInfo = pData.ActiveCall;

                        if (callInfo == null)
                            return;

                        var pId = (int)args[1];

                        var player = RAGE.Elements.Entities.Players.All.Where(x => x.RemoteId == pId).FirstOrDefault();

                        if (player == null)
                            return;

                        callInfo.Player = player;
                        callInfo.StartDate = Sync.World.ServerTime;

                        ShowActiveCall(GetContactNameByNumber(callInfo.Number), "");

                        StartActiveCallUpdateTask();

                        AddToCallHistory(callInfo.Number, callInfo.IsMeCaller ? EndedCallStatusTypes.OutgoingSuccess : EndedCallStatusTypes.IncomingSuccess);
                    }
                    else
                    {
                        var callInfo = new CallInfo(false, Utils.ToUInt32(args[1]));

                        pData.ActiveCall = callInfo;

                        if (Settings.Other.PhoneNotDisturb)
                        {
                            Events.CallRemote("Phone::CA", false);
                        }
                        else
                        {
                            if (CEF.Phone.IsActive)
                            {
                                ShowIncomingCall(GetContactNameByNumber(callInfo.Number));
                            }
                            else
                            {
                                CEF.Notification.ShowFiveCallNotification(GetContactNameByNumber(callInfo.Number), Locale.General.FiveNotificationIncCallSubj, string.Format(Locale.General.FiveNotificationIncCallText, KeyBinds.Get(KeyBinds.Types.Phone).GetKeyString()));
                            }
                        }
                    }
                }
                else
                {
                    var callInfo = pData.ActiveCall;

                    if (callInfo == null)
                        return;

                    pData.ActiveCall = null;

                    var cancelType = (CancelTypes)(int)args[0];

                    CancelActiveCallUpdateTask();

                    if (CEF.Phone.IsActive)
                    {
                        CEF.Phone.ShowApp(pData, AppTypes.None);
                    }

                    if (callInfo.Player != null)
                    {
                        callInfo.Player.VoiceVolume = 0f;

                        var callDurationText = string.Format(Locale.General.FiveNotificationEndedCallTextT, Sync.World.ServerTime.Subtract(callInfo.StartDate).GetBeautyString());

                        if (cancelType == CancelTypes.ServerAuto)
                        {
                            CEF.Notification.ShowFiveCallNotification(GetContactNameByNumber(callInfo.Number), Locale.General.FiveNotificationEndCallSubj0, callDurationText);
                        }
                        else if (cancelType == CancelTypes.NotEnoughBalance)
                        {
                            if (callInfo.IsMeCaller)
                            {
                                CEF.Notification.ShowFiveCallNotification(GetContactNameByNumber(callInfo.Number), Locale.General.FiveNotificationEndCallSubj10, callDurationText);
                            }
                            else
                            {
                                CEF.Notification.ShowFiveCallNotification(GetContactNameByNumber(callInfo.Number), Locale.General.FiveNotificationEndCallSubj20, callDurationText);
                            }
                        }
                        else if (cancelType == CancelTypes.Caller)
                        {
                            if (callInfo.IsMeCaller)
                            {
                                CEF.Notification.ShowFiveCallNotification(GetContactNameByNumber(callInfo.Number), Locale.General.FiveNotificationEndCallSubj1, callDurationText);
                            }
                            else
                            {
                                CEF.Notification.ShowFiveCallNotification(GetContactNameByNumber(callInfo.Number), Locale.General.FiveNotificationEndCallSubj2, callDurationText);
                            }
                        }
                        else if (cancelType == CancelTypes.Receiver)
                        {
                            if (callInfo.IsMeCaller)
                            {
                                CEF.Notification.ShowFiveCallNotification(GetContactNameByNumber(callInfo.Number), Locale.General.FiveNotificationEndCallSubj2, callDurationText);
                            }
                            else
                            {
                                CEF.Notification.ShowFiveCallNotification(GetContactNameByNumber(callInfo.Number), Locale.General.FiveNotificationEndCallSubj1, callDurationText);
                            }
                        }
                    }
                    else
                    {
                        if (callInfo.IsMeCaller)
                        {
                            AddToCallHistory(callInfo.Number, EndedCallStatusTypes.OutgoingError);
                        }
                        else
                        {
                            AddToCallHistory(callInfo.Number, EndedCallStatusTypes.IncomingError);
                        }
                    }
                }
            });

            Events.Add("PoliceCall::Cancel", (args) =>
            {
                var reason = Utils.ToInt32(args[0]);

                var cs = Player.LocalPlayer.GetData<Additional.ExtraColshape>("PoliceCallWaitCs");

                if (cs != null)
                {
                    (cs.Data as Additional.ExtraColshape)?.Destroy();

                    cs.Destroy();

                    Additional.ExtraBlips.DestroyTrackerBlipByKey("PoliceCall");
                }
            });
        }

        public static async void BlacklistChange(uint number, bool add)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var blacklist = pData.PhoneBlacklist;

            if (add && blacklist.Contains(number))
            {
                CEF.Notification.ShowError(Locale.Notifications.General.AlreadyInPhoneBlacklist);

                return;
            }

            LastSent = Sync.World.ServerTime;

            if ((bool)await Events.CallRemoteProc("Phone::BLC", number, add))
            {
                if (add)
                    blacklist.Add(number);
                else
                    blacklist.Remove(number);

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

        public static async void Call(string number)
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var defAction = DefaultNumbersActions.Where(x => x.Key.Contains(number)).Select(x => x.Value).FirstOrDefault();

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
                    var res = (int)await Events.CallRemoteProc("Phone::CP", numNumber);

                    if (res == byte.MaxValue)
                    {
                        ShowActiveCall(number, Locale.General.PhoneOutgoingCall);

                        StartOutgoingCallUpdateTask();

                        pData.ActiveCall = new CallInfo(true, numNumber);
                    }
                    else if (res == 1)
                    {
                        CEF.Notification.ShowError(Locale.Notifications.Players.PhoneNumberWrong1);

                        CallHistory.Add((numNumber, EndedCallStatusTypes.OutgoingError));
                    }
                    else if (res == 2)
                    {
                        CEF.Notification.ShowError(Locale.Notifications.Players.PhoneNumberWrong2);

                        CallHistory.Add((numNumber, EndedCallStatusTypes.OutgoingError));
                    }

                    return;
                }
            }

            CEF.Notification.ShowError(Locale.Notifications.Players.PhoneNumberWrong0);
        }

        public static void ShowDefault(string number = null)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Phone;

            Phone.CurrentAppTab = -1;

            if (number == null)
                CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] { } });
            else
                CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] { number } });
        }

        public static void ShowIncomingCall(string number)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Phone;

            Phone.CurrentAppTab = 999;

            CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] { true, number } });
        }

        public static void ShowActiveCall(string number, string text)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Phone;

            Phone.CurrentAppTab = 999;

            CEF.Browser.Window.ExecuteJs("Phone.drawPhoneApp", new object[] { new object[] { false, number, text } });
        }

        public static void ShowCallHistory(object list)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Phone;
            Phone.CurrentAppTab = 0;

            CEF.Browser.Window.ExecuteJs("Phone.drawCallList", list);
        }

        public static void ShowBlacklist(object list, string number = null)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Phone;
            Phone.CurrentAppTab = 1;

            CEF.Browser.Window.ExecuteJs("Phone.drawBlackList", list);
        }

        public static void StartActiveCallUpdateTask()
        {
            ActiveCallUpdateTask?.Cancel();

            var dateTime = DateTime.UtcNow;

            ActiveCallUpdateTask = new AsyncTask(() =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var activeCallPlayer = pData.ActiveCall?.Player;

                if (activeCallPlayer == null)
                    return;

                activeCallPlayer.VoiceVolume = 1f;

                if (Phone.CurrentApp != Phone.AppTypes.Phone)
                    return;

                CEF.Browser.Window.ExecuteJs("Phone.updatePhoneStatus", DateTime.UtcNow.Subtract(dateTime).GetBeautyString());
            }, 1000, true, 0);

            ActiveCallUpdateTask.Run();
        }

        public static void StartOutgoingCallUpdateTask()
        {
            ActiveCallUpdateTask?.Cancel();

            var dateTime = DateTime.UtcNow;

            var dotsUsed = 3;

            ActiveCallUpdateTask = new AsyncTask(() =>
            {
                if (Phone.CurrentApp != Phone.AppTypes.Phone)
                    return;

                if (dotsUsed == 3)
                    dotsUsed = 0;

                if (dotsUsed == 0)
                    CEF.Browser.Window.ExecuteJs("Phone.updatePhoneStatus", Locale.General.PhoneOutgoingCall);
                else
                    CEF.Browser.Window.ExecuteJs("Phone.updatePhoneStatus", $"{Locale.General.PhoneOutgoingCall}{new string('.', dotsUsed)}");

                dotsUsed++;
            }, 500, true, 0);

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
