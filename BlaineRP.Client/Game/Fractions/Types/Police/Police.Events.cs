using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.UI.CEF;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.Game.Fractions
{
    public partial class Police
    {
        [Script(int.MaxValue)]
        public class Events
        {
            public Events()
            {
                RAGE.Events.Add("FPolice::ARRESTSC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var arrests = pData.CurrentFraction?.GetCurrentData<List<ArrestInfo>>("Arrests");

                        if (arrests == null)
                            return;

                        var id = Utils.Convert.ToUInt32(args[0]);

                        if (args.Length == 1)
                        {
                            var data = arrests.Where(x => x.Id == id).FirstOrDefault();

                            if (data == null)
                                return;

                            arrests.Remove(data);

                            if (ArrestsMenu.IsActive)
                            {
                                Browser.Window.ExecuteJs("MenuArrest.removeArrest", id);

                                if (ArrestsMenu.CurrentArrestId == id)
                                    RAGE.Events.CallLocal("MenuArrest::Button", 0);
                            }
                        }
                        else
                        {
                            var data = new ArrestInfo()
                            {
                                Id = id,
                                MemberName = (string)args[1],
                                TargetName = (string)args[2],
                                Time = DateTimeOffset.FromUnixTimeSeconds(Utils.Convert.ToInt64(args[3])).DateTime,
                            };

                            arrests.Add(data);

                            if (ArrestsMenu.IsActive)
                                Browser.Window.ExecuteJs("MenuArrest.addArrest", id, data.Time.ToString("dd.MM.yyyy HH:mm"), data.TargetName, data.MemberName);
                        }
                    });

                RAGE.Events.Add("FPolice::FINEC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var fines = pData.CurrentFraction?.GetCurrentData<List<FineInfo>>("Fines");

                        if (fines == null)
                            return;

                        var member = (string)args[0];
                        var target = (string)args[1];
                        var amount = Utils.Convert.ToUInt32(args[2]);
                        var reason = (string)args[3];
                        var time = DateTimeOffset.FromUnixTimeSeconds(Utils.Convert.ToInt64(args[4])).DateTime;

                        var fine = new FineInfo()
                        {
                            Amount = amount,
                            Member = member,
                            Reason = reason,
                            Target = target,
                            Time = time,
                        };

                        fines.Add(fine);

                        if (PoliceTabletPC.CurrentTab == 3 || PoliceTabletPC.LastTab == 3)
                        {
                            var i = fines.Count - 1;

                            var arg = PoliceTabletPC.GetFineRowList(fine, ref i);

                            arg.Insert(0, 1);

                            Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                        }

                        // todo
                    });

                RAGE.Events.Add("FPolice::CC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var calls = pData.CurrentFraction?.GetCurrentData<List<CallInfo>>("Calls");

                        if (calls == null)
                            return;

                        var rid = Utils.Convert.ToUInt16(args[0]);

                        if (args.Length < 4)
                        {
                            var call = calls.Where(x => x.Player.RemoteId == rid).FirstOrDefault();

                            if (call == null)
                                return;

                            calls.Remove(call);

                            if (PoliceTabletPC.CurrentTab == 2 || PoliceTabletPC.LastTab == 2)
                                Browser.Window.ExecuteJs("PoliceTablet.removeElem", rid);

                            // todo
                        }
                        else
                        {
                            var call = new CallInfo()
                            {
                                Type = Utils.Convert.ToByte(args[1]),
                                Time = World.Core.ServerTime,
                                Message = (string)args[2],
                                Position = (Vector3)args[3],
                                Player = Entities.Players.GetAtRemote(rid),
                            };

                            calls.Add(call);

                            if (PoliceTabletPC.CurrentTab == 2 || PoliceTabletPC.LastTab == 2)
                            {
                                var arg = PoliceTabletPC.GetCallRowList(call, Player.LocalPlayer.Position);

                                arg.Insert(0, 0);

                                Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                            }

                            // todo
                        }
                    });

                RAGE.Events.Add("FPolice::APBC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var apbs = pData.CurrentFraction?.GetCurrentData<List<APBInfo>>("APBs");

                        if (apbs == null)
                            return;

                        var uid = Utils.Convert.ToUInt32(args[0]);

                        if (args.Length < 4)
                        {
                            var apb = apbs.Where(x => x.Id == uid).FirstOrDefault();

                            if (apb == null)
                                return;

                            apbs.Remove(apb);

                            if (PoliceTabletPC.CurrentTab == 4 || PoliceTabletPC.LastTab == 4 || PoliceTabletPC.LastTab == 41)
                            {
                                if (Player.LocalPlayer.GetData<APBInfo>("PoliceTablet::APBViewId")?.Id == uid)
                                    PoliceTabletPC.TabBack();

                                Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);
                            }

                            // todo
                        }
                        else
                        {
                            var apb = new APBInfo()
                            {
                                Id = uid,
                                Time = World.Core.ServerTime,
                                TargetName = (string)args[1],
                                Member = (string)args[2],
                                Details = (string)args[3],
                            };

                            apbs.Add(apb);

                            if (PoliceTabletPC.CurrentTab == 4 || PoliceTabletPC.LastTab == 4 || PoliceTabletPC.LastTab == 41)
                            {
                                var arg = PoliceTabletPC.GetAPBRowList(apb);

                                arg.Insert(0, 2);

                                Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                            }

                            // todo
                        }
                    });

                RAGE.Events.Add("FPolice::GPSTC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var trackers = pData.CurrentFraction?.GetCurrentData<List<GPSTrackerInfo>>("GPSTrackers");

                        if (trackers == null)
                            return;

                        var uid = Utils.Convert.ToUInt32(args[0]);

                        if (args.Length < 3)
                        {
                            var gpsTracker = trackers.Where(x => x.Id == uid).FirstOrDefault();

                            if (gpsTracker == null)
                                return;

                            trackers.Remove(gpsTracker);

                            if (PoliceTabletPC.CurrentTab == 6 || PoliceTabletPC.LastTab == 6)
                                Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);

                            // todo
                        }
                        else
                        {
                            var gpsTracker = new GPSTrackerInfo() { Id = uid, InstallerStr = (string)args[1], VehicleStr = (string)args[2], };

                            trackers.Add(gpsTracker);

                            if (PoliceTabletPC.CurrentTab == 6 || PoliceTabletPC.LastTab == 6)
                            {
                                var arg = PoliceTabletPC.GetGPSTrackerRowList(gpsTracker);

                                arg.Insert(0, 4);

                                Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                            }

                            // todo
                        }
                    });

                RAGE.Events.Add("FPolice::NOTIFC",
                    (args) =>
                    {
                        var pData = PlayerData.GetData(Player.LocalPlayer);

                        if (pData == null)
                            return;

                        var notifics = pData.CurrentFraction?.GetCurrentData<List<NotificationInfo>>("Notifications");

                        if (notifics == null)
                            return;

                        var uid = Utils.Convert.ToUInt16(args[0]);

                        if (args.Length < 3)
                        {
                            var notific = notifics.Where(x => x.Id == uid).FirstOrDefault();

                            if (notific == null)
                                return;

                            notifics.Remove(notific);

                            if (PoliceTabletPC.CurrentTab == 5 || PoliceTabletPC.LastTab == 5)
                                Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);

                            // todo
                        }
                        else
                        {
                            var notific = new NotificationInfo()
                            {
                                Id = uid, Time = World.Core.ServerTime, Text = (string)args[1], Position = (Vector3)args[2],
                            };

                            notifics.Add(notific);

                            if (PoliceTabletPC.CurrentTab == 5 || PoliceTabletPC.LastTab == 5)
                            {
                                var arg = PoliceTabletPC.GetNotificationRowList(notific, Player.LocalPlayer.Position);

                                arg.Insert(0, 3);

                                Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                            }

                            // todo
                        }
                    });
            }
        }
    }
}