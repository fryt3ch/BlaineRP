using RAGE;
using RAGE.Elements;
using System;

namespace BCRPClient.CEF.PhoneApps
{
    public class TaxiApp : Events.Script
    {
        public static ClientOrderInfo CurrentOrderInfo { get; set; }

        public class ClientOrderInfo
        {
            public DateTime Date { get; set; }

            public Player Driver { get; set; }

            public uint DriverNumber { get; set; }

            public Additional.ExtraColshape ExitColshape1 { get; set; }
            public Additional.ExtraColshape ExitColshape2 { get; set; }
        }

        public TaxiApp()
        {
            Events.Add("Taxi::UO", (args) =>
            {
                if (args == null || args.Length == 0)
                {
                    if (CurrentOrderInfo != null)
                    {
                        CurrentOrderInfo.ExitColshape1?.Destroy();
                        CurrentOrderInfo.ExitColshape2?.Destroy();

                        CurrentOrderInfo = null;

                        CEF.Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.Taxi3);

                        Additional.ExtraBlips.DestroyTrackerBlipByKey("Taxi");
                    }
                }
                else if (args.Length == 2)
                {
                    if (CurrentOrderInfo == null)
                        return;

                    CurrentOrderInfo.Driver = RAGE.Elements.Entities.Players.GetAtRemote((ushort)(int)args[0]);

                    if (CurrentOrderInfo.Driver == null)
                        return;

                    CurrentOrderInfo.DriverNumber = Utils.ToUInt32(args[1]);

                    CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.Taxi0, CurrentOrderInfo.Driver.Name));
                }
                else if (args.Length == 1)
                {
                    if (args[0] is bool b)
                    {
                        if (CurrentOrderInfo == null)
                            return;

                        if (!b)
                        {
                            CurrentOrderInfo.Driver = null;

                            CEF.Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.Taxi1, CurrentOrderInfo.Driver.Name));

                            Additional.ExtraBlips.DestroyTrackerBlipByKey("Taxi");
                        }
                        else
                        {
                            CurrentOrderInfo.ExitColshape1?.Destroy();
                            CurrentOrderInfo.ExitColshape2?.Destroy();

                            CurrentOrderInfo = null;

                            CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.Taxi2, KeyBinds.Get(KeyBinds.Types.SendCoordsToDriver).GetKeyString()));

                            Additional.ExtraBlips.DestroyTrackerBlipByKey("Taxi");
                        }
                    }
                }

                if (CEF.Phone.CurrentApp == Phone.AppTypes.Taxi)
                    CEF.Phone.ShowApp(null, Phone.AppTypes.Taxi);
            });

            Events.Add("Phone::CabAction", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (CEF.Phone.LastSent.IsSpam(250, false, false))
                    return;

                var id = Utils.ToDecimal(args[0]);

                if (CurrentOrderInfo == null)
                {
                    if (id == 0)
                    {
                        CEF.Phone.LastSent = Sync.World.ServerTime;

                        var res = (bool)await Events.CallRemoteProc("Taxi::NO");

                        if (res)
                        {
                            var pos = Player.LocalPlayer.Position;

                            CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.TaxiOrdered, Utils.GetStreetName(pos)));

                            pos.Z -= 1f;

                            CurrentOrderInfo = new ClientOrderInfo()
                            {
                                Date = Sync.World.ServerTime,

                                ExitColshape1 = new Additional.Cylinder(pos, Settings.TAXI_ORDER_MAX_WAIT_RANGE / 2, 10f, false, Utils.RedColor, Settings.MAIN_DIMENSION)
                                {
                                    OnExit = (cancel) =>
                                    {
                                        if (CurrentOrderInfo?.ExitColshape1?.Exists != true)
                                            return;

                                        CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.TaxiDistanceWarn, Settings.TAXI_ORDER_MAX_WAIT_RANGE / 2));
                                    }
                                },

                                ExitColshape2 = new Additional.Cylinder(pos, Settings.TAXI_ORDER_MAX_WAIT_RANGE, 10f, false, new Utils.Colour(0, 0, 255, 25), Settings.MAIN_DIMENSION)
                                {
                                    OnExit = (cancel) =>
                                    {
                                        if (CurrentOrderInfo?.ExitColshape2?.Exists != true)
                                            return;

                                        Events.CallRemote("Taxi::CO");
                                    }
                                }
                            };

                            if (CEF.Phone.CurrentApp == Phone.AppTypes.Taxi)
                                CEF.Phone.ShowApp(null, Phone.AppTypes.Taxi);
                        }
                        else
                        {
                            CEF.Notification.ShowError(Locale.Notifications.General.TaxiError);
                        }
                    }
                }
                else
                {
                    if (CurrentOrderInfo.Driver == null)
                    {
                        if (id == 0)
                        {
                            CEF.Phone.LastSent = Sync.World.ServerTime;

                            Events.CallRemote("Taxi::CO");
                        }
                    }
                    else
                    {
                        if (id == 0)
                        {
                            var allSms = pData.AllSMS;
                            var pNumber = pData.PhoneNumber;

                            var chatList = PhoneApps.SMSApp.GetChatList(allSms, CurrentOrderInfo.DriverNumber, pNumber);

                            if (chatList != null)
                            {
                                PhoneApps.SMSApp.ShowChat(CurrentOrderInfo.DriverNumber, chatList, Phone.GetContactNameByNumberNull(CurrentOrderInfo.DriverNumber));
                            }
                            else
                            {
                                PhoneApps.SMSApp.ShowWriteNew(CurrentOrderInfo.DriverNumber.ToString());
                            }
                        }
                        else if (id == 1)
                        {
                            CEF.PhoneApps.PhoneApp.ShowDefault(CurrentOrderInfo.DriverNumber.ToString());
                        }
                    }
                }
            });
        }

        public static void Show(Sync.Players.PlayerData pData)
        {
            if (Phone.CurrentApp == Phone.AppTypes.None)
                Phone.SwitchMenu(false);

            Phone.CurrentApp = Phone.AppTypes.Taxi;

            Phone.CurrentAppTab = -1;

            if (CurrentOrderInfo == null)
            {
                CEF.Browser.Window.ExecuteJs("Phone.drawCabApp", 0, new object[] { pData.CID.ToString(), Utils.GetStreetName(Player.LocalPlayer.Position) });
            }
            else
            {
                if (CurrentOrderInfo.Driver == null)
                {
                    CEF.Browser.Window.ExecuteJs("Phone.drawCabApp", 1, new object[] { "Идет поиск водителя", CurrentOrderInfo.Date.ToString("HH:mm dd.MM.yyyy") });
                }
                else
                {
                    CEF.Browser.Window.ExecuteJs("Phone.drawCabApp", 2, new object[] { "Водитель в пути", CurrentOrderInfo.Driver.GetSharedData<int>("CID", 0).ToString() });
                }
            }
        }
    }
}
