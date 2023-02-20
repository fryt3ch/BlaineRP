using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Text;

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

            public Additional.ExtraColshape ExitColshape { get; set; }

            public AsyncTask UpdateBlipTask { get; set; }

            public Blip DriverBlip { get; set; }
        }

        public TaxiApp()
        {
            Events.Add("Taxi::UO", (args) =>
            {
                if (args == null || args.Length == 0)
                {
                    if (CurrentOrderInfo != null)
                    {
                        CurrentOrderInfo.ExitColshape?.Delete();

                        CurrentOrderInfo.UpdateBlipTask?.Cancel();

                        CurrentOrderInfo = null;
                    }
                }
                else if (args.Length == 2)
                {
                    if (CurrentOrderInfo == null)
                        return;

                    CurrentOrderInfo.Driver = RAGE.Elements.Entities.Players.GetAtRemote((ushort)(int)args[0]);

                    if (CurrentOrderInfo.Driver == null)
                        return;

                    CurrentOrderInfo.DriverNumber = args[0].ToUInt32();

                    CurrentOrderInfo.UpdateBlipTask?.Cancel();

                    CurrentOrderInfo.DriverBlip = new Blip(198, CurrentOrderInfo.Driver.Position, "Водитель такси", 1f, 5, 255, 0f, false, 0, 0f, Settings.MAIN_DIMENSION);

                    CurrentOrderInfo.UpdateBlipTask = new AsyncTask(() =>
                    {
                        if (CurrentOrderInfo.DriverBlip?.Exists != true)
                            return;

                        var pos = CurrentOrderInfo.Driver?.Position;

                        if (pos == null)
                            return;

                        CurrentOrderInfo.DriverBlip.SetCoords(pos.X, pos.Y, pos.Z);
                    }, 2500, true, 0);
                }
                else if (args.Length == 1)
                {
                    if (args[0] is bool b)
                    {
                        if (!b)
                        {
                            if (CurrentOrderInfo == null)
                                return;

                            if (CurrentOrderInfo.UpdateBlipTask != null)
                            {
                                CurrentOrderInfo.UpdateBlipTask.Cancel();

                                CurrentOrderInfo.UpdateBlipTask = null;
                            }

                            CurrentOrderInfo.Driver = null;

                            CurrentOrderInfo.DriverBlip?.Destroy();
                        }
                    }
                }

                if (CEF.Phone.CurrentApp == Phone.AppTypes.Taxi)
                    CEF.Phone.ShowApp(null, Phone.AppTypes.Taxi);
            });

            Events.Add("Phone::CabAction", async (args) =>
            {
                if (CEF.Phone.LastSent.IsSpam(500, false, false))
                    return;

                var id = args[0].ToDecimal();

                if (CurrentOrderInfo == null)
                {
                    if (id == 0)
                    {
                        CEF.Phone.LastSent = DateTime.Now;

                        var res = (byte)(int)await Events.CallRemoteProc("Taxi::NO");

                        if (res == byte.MaxValue)
                        {
                            CurrentOrderInfo = new ClientOrderInfo()
                            {
                                Date = Utils.GetServerTime(),

                                ExitColshape = new Additional.Circle(Player.LocalPlayer.Position, 20f, true, new Utils.Colour(255, 0, 0, 125), Settings.MAIN_DIMENSION)
                                {
                                    OnExit = (cancel) =>
                                    {
                                        Events.CallRemote("Taxi::CO");
                                    }
                                }
                            };

                            if (CEF.Phone.CurrentApp == Phone.AppTypes.Taxi)
                                CEF.Phone.ShowApp(null, Phone.AppTypes.Taxi);
                        }
                        else
                        {

                        }
                    }
                }
                else
                {
                    if (CurrentOrderInfo.Driver == null)
                    {
                        if (id == 0)
                        {
                            CEF.Phone.LastSent = DateTime.Now;

                            Events.CallRemote("Taxi::CO");
                        }
                    }
                    else
                    {
                        if (id == 0)
                        {
                            // todo show sms app
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
                    CEF.Browser.Window.ExecuteJs("Phone.drawCabApp", 2, new object[] { "Водитель в пути", CurrentOrderInfo.Driver.GetSharedData<uint>("CID", 0).ToString() });
                }
            }
        }
    }
}
