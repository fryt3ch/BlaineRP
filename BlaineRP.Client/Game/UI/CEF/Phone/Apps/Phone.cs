using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Game.Input.Enums;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Phone
    {
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

        public enum EndedCallStatusTypes
        {
            OutgoingSuccess = 0,
            OutgoingError = 1,
            OutgoingBusy = 2,

            IncomingSuccess = 3,
            IncomingError = 4,
            IncomingBusy = 5,
        }

        public Phone()
        {
            Events.Add("Phone::Call",
                (args) =>
                {
                    if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
                        return;

                    var number = args[0]?.ToString();

                    if (number == null || number.Length == 0)
                        return;

                    CEF.Phone.Phone.LastSent = World.Core.ServerTime;

                    Call(number);
                }
            );

            Events.Add("Phone::ReplyCall",
                (args) =>
                {
                    if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
                        return;

                    var ans = (bool)args[0];

                    CEF.Phone.Phone.LastSent = World.Core.ServerTime;

                    Events.CallRemote("Phone::CA", ans);
                }
            );

            Events.Add("Phone::BlacklistChange",
                async (args) =>
                {
                    if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
                        return;

                    var pData = PlayerData.GetData(Player.LocalPlayer);

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
                }
            );

            Events.Add("Phone::ACS",
                (args) =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (args[0] is bool state)
                    {
                        if (state)
                        {
                            CallInfo callInfo = pData.ActiveCall;

                            if (callInfo == null)
                                return;

                            var pId = (int)args[1];

                            Player player = Entities.Players.All.Where(x => x.RemoteId == pId).FirstOrDefault();

                            if (player == null)
                                return;

                            callInfo.Player = player;
                            callInfo.StartDate = World.Core.ServerTime;

                            ShowActiveCall(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number), "");

                            StartActiveCallUpdateTask();

                            AddToCallHistory(callInfo.Number, callInfo.IsMeCaller ? EndedCallStatusTypes.OutgoingSuccess : EndedCallStatusTypes.IncomingSuccess);
                        }
                        else
                        {
                            var callInfo = new CallInfo(false, Utils.Convert.ToUInt32(args[1]));

                            pData.ActiveCall = callInfo;

                            if (BlaineRP.Client.Settings.User.Other.PhoneNotDisturb)
                            {
                                Events.CallRemote("Phone::CA", false);
                            }
                            else
                            {
                                if (CEF.Phone.Phone.IsActive)
                                    ShowIncomingCall(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number));
                                else
                                    Notification.ShowFiveCallNotification(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number),
                                        Locale.Get("PHONE_CALL_INCOMING_0"),
                                        string.Format(Locale.Get("PHONE_CALL_INCOMING_HINT_0"), Input.Core.Get(BindTypes.Phone).GetKeyString())
                                    );
                            }
                        }
                    }
                    else
                    {
                        CallInfo callInfo = pData.ActiveCall;

                        if (callInfo == null)
                            return;

                        pData.ActiveCall = null;

                        var cancelType = (CancelTypes)(int)args[0];

                        CancelActiveCallUpdateTask();

                        if (CEF.Phone.Phone.IsActive)
                            CEF.Phone.Phone.ShowApp(pData, AppType.None);

                        if (callInfo.Player != null)
                        {
                            callInfo.Player.VoiceVolume = 0f;

                            var callDurationText = string.Format(Locale.Get("PHONE_CALL_END_INFO_0"),
                                World.Core.ServerTime.Subtract(callInfo.StartDate).GetBeautyString()
                            );

                            if (cancelType == CancelTypes.ServerAuto)
                            {
                                Notification.ShowFiveCallNotification(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number),
                                    Locale.Get("PHONE_CALL_END_0"),
                                    callDurationText
                                );
                            }
                            else if (cancelType == CancelTypes.NotEnoughBalance)
                            {
                                if (callInfo.IsMeCaller)
                                    Notification.ShowFiveCallNotification(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number),
                                        Locale.Get("PHONE_CALL_END_3"),
                                        callDurationText
                                    );
                                else
                                    Notification.ShowFiveCallNotification(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number),
                                        Locale.Get("PHONE_CALL_END_4"),
                                        callDurationText
                                    );
                            }
                            else if (cancelType == CancelTypes.Caller)
                            {
                                if (callInfo.IsMeCaller)
                                    Notification.ShowFiveCallNotification(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number),
                                        Locale.Get("PHONE_CALL_END_1"),
                                        callDurationText
                                    );
                                else
                                    Notification.ShowFiveCallNotification(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number),
                                        Locale.Get("PHONE_CALL_END_2"),
                                        callDurationText
                                    );
                            }
                            else if (cancelType == CancelTypes.Receiver)
                            {
                                if (callInfo.IsMeCaller)
                                    Notification.ShowFiveCallNotification(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number),
                                        Locale.Get("PHONE_CALL_END_2"),
                                        callDurationText
                                    );
                                else
                                    Notification.ShowFiveCallNotification(CEF.Phone.Phone.GetContactNameByNumber(callInfo.Number),
                                        Locale.Get("PHONE_CALL_END_1"),
                                        callDurationText
                                    );
                            }
                        }
                        else
                        {
                            if (callInfo.IsMeCaller)
                                AddToCallHistory(callInfo.Number, EndedCallStatusTypes.OutgoingError);
                            else
                                AddToCallHistory(callInfo.Number, EndedCallStatusTypes.IncomingError);
                        }
                    }
                }
            );

            Events.Add("PoliceCall::Cancel",
                (args) =>
                {
                    var reason = Utils.Convert.ToInt32(args[0]);

                    ExtraColshape cs = Player.LocalPlayer.GetData<ExtraColshape>("PoliceCallWaitCs");

                    if (cs != null)
                    {
                        (cs.Data as ExtraColshape)?.Destroy();

                        cs.Destroy();

                        Helpers.Blips.Core.DestroyTrackerBlipByKey("PoliceCall");
                    }
                }
            );
        }

        private static AsyncTask ActiveCallUpdateTask { get; set; }

        public static Dictionary<string[], Action> DefaultNumbersActions { get; private set; } = new Dictionary<string[], Action>()
        {
            {
                new string[]
                {
                    "*100#",
                    "*102#",
                    "*105#",
                },
                async () =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    string[] res = ((string)await Events.CallRemoteProc("Phone::GPD"))?.Split('_');

                    if (res == null)
                        return;

                    Notification.Show(Notification.Types.Information,
                        Locale.Get("NOTIFICATION_HEADER_DEF"),
                        string.Format(Locale.Notifications.Money.PhoneBalanceInfo,
                            pData.PhoneNumber,
                            Locale.Get("GEN_MONEY_0", decimal.Parse(res[0])),
                            Locale.Get("GEN_MONEY_0", decimal.Parse(res[1])),
                            Locale.Get("GEN_MONEY_0", decimal.Parse(res[2]))
                        )
                    );
                }
            },
            {
                new string[]
                {
                    "911",
                    "112",
                },
                async () =>
                {
                    await ActionBox.ShowSelect("Phone911Select",
                        Locale.Get("PHONE_ECALL_0"),
                        new (decimal Id, string Text)[]
                        {
                            (102, Locale.Get("PHONE_ECALL_1")),
                            (103, Locale.Get("PHONE_ECALL_2")),
                        },
                        null,
                        null,
                        () =>
                        {
                            if (CEF.Phone.Phone.IsActive)
                                Browser.SwitchTemp(Browser.IntTypes.Phone, false);
                        },
                        async (rType, idD) =>
                        {
                            ActionBox.Close(false);

                            if (rType == ActionBox.ReplyTypes.Cancel)
                                return;

                            var idStr = idD.ToString();

                            Action action = DefaultNumbersActions.Where(x => x.Key.Contains(idStr)).Select(x => x.Value).FirstOrDefault();

                            action?.Invoke();
                        },
                        () =>
                        {
                            if (CEF.Phone.Phone.IsActive)
                            {
                                Browser.SwitchTemp(Browser.IntTypes.Phone, true);

                                Cursor.Show(true, true);
                            }
                        }
                    );
                }
            },
            {
                new string[]
                {
                    "102",
                },
                async () =>
                {
                    await ActionBox.ShowInputWithText("PhonePoliceCallInput",
                        Locale.Get("PHONE_ECALL_3"),
                        Locale.Get("PHONE_ECALL_5"),
                        24,
                        "",
                        null,
                        null,
                        () =>
                        {
                            if (CEF.Phone.Phone.IsActive)
                                Browser.SwitchTemp(Browser.IntTypes.Phone, false);
                        },
                        async (rType, str) =>
                        {
                            if (rType == ActionBox.ReplyTypes.Cancel)
                            {
                                ActionBox.Close(false);

                                return;
                            }

                            str = str?.Trim();

                            var pattern = new Regex(@"^[A-Za-zА-Яа-я0-9,./?$#@!%^&*()'+=\-\[\]]{1,24}$");

                            if (str == null || !pattern.IsMatch(str))
                            {
                                Notification.Show("Str::NM");

                                return;
                            }

                            var res = Utils.Convert.ToByte(await Events.CallRemoteProc("Police::Call", str));

                            if (res == 255)
                            {
                                Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("PHONE_ECALL_S_0"));

                                ActionBox.Close(false);

                                Vector3 pos = Player.LocalPlayer.GetCoords(false);

                                pos.Z -= 1f;

                                ExtraColshape cs1 = Player.LocalPlayer.GetData<ExtraColshape>("PoliceCallWaitCs");

                                if (cs1 != null)
                                {
                                    (cs1.Data as ExtraColshape)?.Destroy();

                                    cs1.Destroy();
                                }

                                ExtraColshape cs2 = null;

                                cs1 = new Cylinder(pos,
                                    BlaineRP.Client.Settings.App.Static.POLICE_CALL_MAX_WAIT_RANGE / 2,
                                    10f,
                                    false,
                                    Utils.Misc.RedColor,
                                    BlaineRP.Client.Settings.App.Static.MainDimension
                                )
                                {
                                    OnExit = (cancel) =>
                                    {
                                        if (cs1?.Exists != true)
                                            return;

                                        Notification.Show(Notification.Types.Information,
                                            Locale.Get("NOTIFICATION_HEADER_DEF"),
                                            Locale.Get("PHONE_ECALL_W_0", BlaineRP.Client.Settings.App.Static.POLICE_CALL_MAX_WAIT_RANGE / 2)
                                        );
                                    },
                                };

                                cs2 = new Cylinder(pos,
                                    BlaineRP.Client.Settings.App.Static.POLICE_CALL_MAX_WAIT_RANGE,
                                    10f,
                                    false,
                                    new Colour(0, 0, 255, 25),
                                    BlaineRP.Client.Settings.App.Static.MainDimension
                                )
                                {
                                    OnExit = async (cancel) =>
                                    {
                                        if (cs2?.Exists != true)
                                            return;

                                        object resCancel = await Events.CallRemoteProc("Police::Call", string.Empty);
                                    },
                                    Data = cs1,
                                };

                                Player.LocalPlayer.SetData("PoliceCallWaitCs", cs2);
                            }
                            else
                            {
                                if (res == 1)
                                    Notification.ShowError(Locale.Get("PHONE_ECALL_E_1"));
                                else if (res == 0)
                                    Notification.ShowError(Locale.Get("PHONE_ECALL_E_0"));
                            }
                        },
                        () =>
                        {
                            if (CEF.Phone.Phone.IsActive)
                            {
                                Browser.SwitchTemp(Browser.IntTypes.Phone, true);

                                Cursor.Show(true, true);
                            }
                        }
                    );
                }
            },
            {
                new string[]
                {
                    "103",
                },
                async () =>
                {
                    await ActionBox.ShowInputWithText("PhoneMedicalCallInput",
                        Locale.Get("PHONE_ECALL_4"),
                        Locale.Get("PHONE_ECALL_6"),
                        24,
                        "",
                        null,
                        null,
                        () =>
                        {
                            if (CEF.Phone.Phone.IsActive)
                                Browser.SwitchTemp(Browser.IntTypes.Phone, false);
                        },
                        async (rType, str) =>
                        {
                            if (rType == ActionBox.ReplyTypes.Cancel)
                            {
                                ActionBox.Close(false);

                                return;
                            }

                            str = str?.Trim();

                            var pattern = new Regex(@"^[A-Za-zА-Яа-я0-9,./?$#@!%^&*()'+=\-\[\]]{1,24}$");

                            if (str == null || !pattern.IsMatch(str))
                            {
                                Notification.Show("Str::NM");

                                return;
                            }

                            ActionBox.Close(false);

                            // call remote
                        },
                        () =>
                        {
                            if (CEF.Phone.Phone.IsActive)
                            {
                                Browser.SwitchTemp(Browser.IntTypes.Phone, true);

                                Cursor.Show(true, true);
                            }
                        }
                    );
                }
            },
        };

        public static List<(uint PhoneNumber, EndedCallStatusTypes Status)> CallHistory { get; private set; } = new List<(uint PhoneNumber, EndedCallStatusTypes Status)>();

        public static void AddToCallHistory(uint phoneNumber, EndedCallStatusTypes status)
        {
            if (CallHistory.Count >= 50)
                CallHistory.RemoveAt(0);

            CallHistory.Add((phoneNumber, status));
        }

        public static async void BlacklistChange(uint number, bool add)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            List<uint> blacklist = pData.PhoneBlacklist;

            if (add && blacklist.Contains(number))
            {
                Notification.ShowError(Locale.Notifications.General.AlreadyInPhoneBlacklist);

                return;
            }

            CEF.Phone.Phone.LastSent = World.Core.ServerTime;

            if ((bool)await Events.CallRemoteProc("Phone::BLC", number, add))
            {
                if (add)
                    blacklist.Add(number);
                else
                    blacklist.Remove(number);

                if (blacklist.Count == 0)
                    ShowBlacklist(null, null);
                else
                    ShowBlacklist(blacklist.Select(x => new object[]
                            {
                                x,
                                CEF.Phone.Phone.GetContactNameByNumberNull(x),
                            }
                        )
                    );
            }
        }

        public static async void Call(string number)
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            Action defAction = DefaultNumbersActions.Where(x => x.Key.Contains(number)).Select(x => x.Value).FirstOrDefault();

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
                        ShowActiveCall(number, Locale.Get("PHONE_CALL_OUTGOING_0"));

                        StartOutgoingCallUpdateTask();

                        pData.ActiveCall = new CallInfo(true, numNumber);
                    }
                    else if (res == 1)
                    {
                        Notification.ShowError(Locale.Notifications.Players.PhoneNumberWrong1);

                        CallHistory.Add((numNumber, EndedCallStatusTypes.OutgoingError));
                    }
                    else if (res == 2)
                    {
                        Notification.ShowError(Locale.Notifications.Players.PhoneNumberWrong2);

                        CallHistory.Add((numNumber, EndedCallStatusTypes.OutgoingError));
                    }

                    return;
                }
            }

            Notification.ShowError(Locale.Notifications.Players.PhoneNumberWrong0);
        }

        public static void ShowDefault(string number = null)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Phone;

            CEF.Phone.Phone.CurrentAppTab = -1;

            if (number == null)
                Browser.Window.ExecuteJs("Phone.drawPhoneApp",
                    new object[]
                    {
                        new object[]
                        {
                        },
                    }
                );
            else
                Browser.Window.ExecuteJs("Phone.drawPhoneApp",
                    new object[]
                    {
                        new object[]
                        {
                            number,
                        },
                    }
                );
        }

        public static void ShowIncomingCall(string number)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Phone;

            CEF.Phone.Phone.CurrentAppTab = 999;

            Browser.Window.ExecuteJs("Phone.drawPhoneApp",
                new object[]
                {
                    new object[]
                    {
                        true,
                        number,
                    },
                }
            );
        }

        public static void ShowActiveCall(string number, string text)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Phone;

            CEF.Phone.Phone.CurrentAppTab = 999;

            Browser.Window.ExecuteJs("Phone.drawPhoneApp",
                new object[]
                {
                    new object[]
                    {
                        false,
                        number,
                        text,
                    },
                }
            );
        }

        public static void ShowCallHistory(object list)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Phone;
            CEF.Phone.Phone.CurrentAppTab = 0;

            Browser.Window.ExecuteJs("Phone.drawCallList", list);
        }

        public static void ShowBlacklist(object list, string number = null)
        {
            if (CEF.Phone.Phone.CurrentApp == AppType.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppType.Phone;
            CEF.Phone.Phone.CurrentAppTab = 1;

            Browser.Window.ExecuteJs("Phone.drawBlackList", list);
        }

        public static void StartActiveCallUpdateTask()
        {
            ActiveCallUpdateTask?.Cancel();

            DateTime dateTime = DateTime.UtcNow;

            ActiveCallUpdateTask = new AsyncTask(() =>
                {
                    var pData = PlayerData.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    Player activeCallPlayer = pData.ActiveCall?.Player;

                    if (activeCallPlayer == null)
                        return;

                    activeCallPlayer.VoiceVolume = 1f;

                    if (CEF.Phone.Phone.CurrentApp != AppType.Phone)
                        return;

                    Browser.Window.ExecuteJs("Phone.updatePhoneStatus", DateTime.UtcNow.Subtract(dateTime).GetBeautyString());
                },
                1000,
                true,
                0
            );

            ActiveCallUpdateTask.Run();
        }

        public static void StartOutgoingCallUpdateTask()
        {
            ActiveCallUpdateTask?.Cancel();

            DateTime dateTime = DateTime.UtcNow;

            var dotsUsed = 3;

            ActiveCallUpdateTask = new AsyncTask(() =>
                {
                    if (CEF.Phone.Phone.CurrentApp != AppType.Phone)
                        return;

                    if (dotsUsed == 3)
                        dotsUsed = 0;

                    if (dotsUsed == 0)
                        Browser.Window.ExecuteJs("Phone.updatePhoneStatus", Locale.Get("PHONE_CALL_OUTGOING_0"));
                    else
                        Browser.Window.ExecuteJs("Phone.updatePhoneStatus", $"{Locale.Get("PHONE_CALL_OUTGOING_0")}{new string('.', dotsUsed)}");

                    dotsUsed++;
                },
                500,
                true,
                0
            );

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

        public class CallInfo
        {
            public CallInfo(bool IsMeCaller, uint Number)
            {
                this.IsMeCaller = IsMeCaller;
                this.Number = Number;
            }

            public bool IsMeCaller { get; set; }

            public Player Player { get; set; }

            public uint Number { get; set; }

            public DateTime StartDate { get; set; }
        }
    }
}