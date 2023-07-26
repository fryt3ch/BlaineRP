using System;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.Helpers.Colshapes;
using BlaineRP.Client.Game.Helpers.Colshapes.Types;
using BlaineRP.Client.Input.Enums;
using BlaineRP.Client.UI.CEF.Phone.Enums;
using RAGE;
using RAGE.Elements;
using Core = BlaineRP.Client.Input.Core;

namespace BlaineRP.Client.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class Taxi
    {
        public static ClientOrderInfo CurrentOrderInfo { get; set; }

        public class ClientOrderInfo
        {
            public DateTime Date { get; set; }

            public Player Driver { get; set; }

            public uint DriverNumber { get; set; }

            public ExtraColshape ExitColshape1 { get; set; }
            public ExtraColshape ExitColshape2 { get; set; }
        }

        public Taxi()
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

                        Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Notifications.General.Taxi3);

                        Game.Helpers.Blips.Core.DestroyTrackerBlipByKey("Taxi");
                    }
                }
                else if (args.Length == 2)
                {
                    if (CurrentOrderInfo == null)
                        return;

                    CurrentOrderInfo.Driver = Entities.Players.GetAtRemote((ushort)(int)args[0]);

                    if (CurrentOrderInfo.Driver == null)
                        return;

                    CurrentOrderInfo.DriverNumber = Utils.Convert.ToUInt32(args[1]);

                    Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.Taxi0, CurrentOrderInfo.Driver.Name));
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

                            Notification.Show(Notification.Types.Error, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.Taxi1, CurrentOrderInfo.Driver.Name));

                            Game.Helpers.Blips.Core.DestroyTrackerBlipByKey("Taxi");
                        }
                        else
                        {
                            CurrentOrderInfo.ExitColshape1?.Destroy();
                            CurrentOrderInfo.ExitColshape2?.Destroy();

                            CurrentOrderInfo = null;

                            Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.Taxi2, Core.Get(BindTypes.SendCoordsToDriver).GetKeyString()));

                            Game.Helpers.Blips.Core.DestroyTrackerBlipByKey("Taxi");
                        }
                    }
                }

                if (CEF.Phone.Phone.CurrentApp == AppTypes.Taxi)
                    CEF.Phone.Phone.ShowApp(null, AppTypes.Taxi);
            });

            Events.Add("Phone::CabAction", async (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (CEF.Phone.Phone.LastSent.IsSpam(250, false, false))
                    return;

                var id = Utils.Convert.ToDecimal(args[0]);

                if (CurrentOrderInfo == null)
                {
                    if (id == 0)
                    {
                        CEF.Phone.Phone.LastSent = Game.World.Core.ServerTime;

                        var res = (bool)await Events.CallRemoteProc("Taxi::NO");

                        if (res)
                        {
                            var pos = Player.LocalPlayer.Position;

                            Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.TaxiOrdered, Utils.Game.Misc.GetStreetName(pos)));

                            pos.Z -= 1f;

                            CurrentOrderInfo = new ClientOrderInfo()
                            {
                                Date = Game.World.Core.ServerTime,

                                ExitColshape1 = new Cylinder(pos, Client.Settings.App.Static.TAXI_ORDER_MAX_WAIT_RANGE / 2, 10f, false, Utils.Misc.RedColor, Client.Settings.App.Static.MainDimension)
                                {
                                    OnExit = (cancel) =>
                                    {
                                        if (CurrentOrderInfo?.ExitColshape1?.Exists != true)
                                            return;

                                        Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), string.Format(Locale.Notifications.General.TaxiDistanceWarn, Client.Settings.App.Static.TAXI_ORDER_MAX_WAIT_RANGE / 2));
                                    }
                                },

                                ExitColshape2 = new Cylinder(pos, Client.Settings.App.Static.TAXI_ORDER_MAX_WAIT_RANGE, 10f, false, new Utils.Colour(0, 0, 255, 25), Client.Settings.App.Static.MainDimension)
                                {
                                    OnExit = (cancel) =>
                                    {
                                        if (CurrentOrderInfo?.ExitColshape2?.Exists != true)
                                            return;

                                        Events.CallRemote("Taxi::CO");
                                    }
                                }
                            };

                            if (CEF.Phone.Phone.CurrentApp == AppTypes.Taxi)
                                CEF.Phone.Phone.ShowApp(null, AppTypes.Taxi);
                        }
                        else
                        {
                            Notification.ShowError(Locale.Notifications.General.TaxiError);
                        }
                    }
                }
                else
                {
                    if (CurrentOrderInfo.Driver == null)
                    {
                        if (id == 0)
                        {
                            CEF.Phone.Phone.LastSent = Game.World.Core.ServerTime;

                            Events.CallRemote("Taxi::CO");
                        }
                    }
                    else
                    {
                        if (id == 0)
                        {
                            var allSms = pData.AllSMS;
                            var pNumber = pData.PhoneNumber;

                            var chatList = SMS.GetChatList(allSms, CurrentOrderInfo.DriverNumber, pNumber);

                            if (chatList != null)
                            {
                                SMS.ShowChat(CurrentOrderInfo.DriverNumber, chatList, CEF.Phone.Phone.GetContactNameByNumberNull(CurrentOrderInfo.DriverNumber));
                            }
                            else
                            {
                                SMS.ShowWriteNew(CurrentOrderInfo.DriverNumber.ToString());
                            }
                        }
                        else if (id == 1)
                        {
                            Phone.ShowDefault(CurrentOrderInfo.DriverNumber.ToString());
                        }
                    }
                }
            });
        }

        public static void Show(PlayerData pData)
        {
            if (CEF.Phone.Phone.CurrentApp == AppTypes.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppTypes.Taxi;

            CEF.Phone.Phone.CurrentAppTab = -1;

            if (CurrentOrderInfo == null)
            {
                Browser.Window.ExecuteJs("Phone.drawCabApp", 0, new object[] { pData.CID.ToString(), Utils.Game.Misc.GetStreetName(Player.LocalPlayer.Position) });
            }
            else
            {
                if (CurrentOrderInfo.Driver == null)
                {
                    Browser.Window.ExecuteJs("Phone.drawCabApp", 1, new object[] { "Идет поиск водителя", CurrentOrderInfo.Date.ToString("HH:mm dd.MM.yyyy") });
                }
                else
                {
                    Browser.Window.ExecuteJs("Phone.drawCabApp", 2, new object[] { "Водитель в пути", CurrentOrderInfo.Driver.GetSharedData("CID", 0).ToString() });
                }
            }
        }
    }
}
