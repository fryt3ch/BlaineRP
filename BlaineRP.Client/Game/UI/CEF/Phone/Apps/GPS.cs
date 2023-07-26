using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Client.Extensions.RAGE;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Game.Helpers.Blips;
using BlaineRP.Client.UI.CEF.Phone.Enums;
using BlaineRP.Client.Utils;
using RAGE;
using RAGE.Elements;

namespace BlaineRP.Client.UI.CEF.Phone.Apps
{
    [Script(int.MaxValue)]
    public class GPS
    {
        private static Dictionary<string, Dictionary<string, Dictionary<string, RAGE.Ui.Cursor.Vector2>>> AllPositions = new Dictionary<string, Dictionary<string, Dictionary<string, RAGE.Ui.Cursor.Vector2>>>()
        {
            {
                "money",

                new Dictionary<string, Dictionary<string, RAGE.Ui.Cursor.Vector2>>()
                {
                    { "banks", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_bank&closest0", null } } },
                    { "atms", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_atm&closest1", null } } },
                }
            },

            {
                "clothes",

                new Dictionary<string, Dictionary<string, RAGE.Ui.Cursor.Vector2>>()
                {
                    { "clothes1", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_clothes1&closest1", null } } },
                    { "clothes2", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_clothes2&closest1", null } } },
                    { "clothes3", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_clothes3&closest1", null } } },
                }
            },

            {
                "bizother",

                new Dictionary<string, Dictionary<string, RAGE.Ui.Cursor.Vector2>>()
                {
                    { "market", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_market&closest1", null } } },
                    { "gas", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_gas&closest2", null } } },
                    { "tuning", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_tuning&closest1", null } } },
                    { "weapon", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_weapon&closest1", null } } },
                    { "furn", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_furn&closest1", null } } },
                    { "farm", new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { "a_farm&closest2", null } } },
                }
            },
        };

        private static AsyncTask RouteUpdateTask { get; set; }

        private static ExtraBlip CurrentRouteBlip { get; set; }

        public GPS()
        {
            Events.Add("Phone::ClearRoute", (args) =>
            {
                if (CurrentRouteBlip != null)
                {
                    if (RouteUpdateTask != null)
                    {
                        RouteUpdateTask.Cancel();

                        RouteUpdateTask = null;
                    }

                    if (CEF.Phone.Phone.CurrentApp == AppTypes.Navigator && CEF.Phone.Phone.CurrentAppTab == -1)
                        Browser.Window.ExecuteJs("Phone.removeCurRoute();");

                    CurrentRouteBlip.Destroy();
                }
            });

            Events.Add("Phone::ShowRoute", (args) =>
            {
                var routeId = (string)args[0];

                var searchRouteId = $"{routeId}&";

                RAGE.Ui.Cursor.Vector2 route;

                foreach (var x in AllPositions.Values)
                {
                    foreach (var y in x.Values)
                    {
                        foreach (var t in y)
                        {
                            if (t.Key.StartsWith(searchRouteId))
                            {
                                routeId = t.Key;

                                route = t.Value;

                                if (route == null)
                                {
                                    var tPos = new Vector3(0f, 0f, 0f);

                                    var pPos = Player.LocalPlayer.Position;

                                    var minDist = float.MaxValue;
                                    string closestId = null;

                                    foreach (var z in y)
                                    {
                                        if (z.Value == null)
                                            continue;

                                        tPos.X = z.Value.X;
                                        tPos.Y = z.Value.Y;

                                        var dist = pPos.DistanceIgnoreZ(tPos);

                                        if (dist <= minDist)
                                        {
                                            route = z.Value;
                                            closestId = z.Key;

                                            minDist = dist;
                                        }
                                    }

                                    if (closestId == null || route == null)
                                        return;

                                    CurrentRouteBlip?.Destroy();

                                    CurrentRouteBlip = Core.CreateGPS(new Vector3(route.X, route.Y, 0f), uint.MaxValue, true);

                                    CurrentRouteBlip.Blip.SetData("GPSRouteId", closestId);

                                    var data = closestId.Split('&');

                                    var name = Locale.GPSApp.Names.GetValueOrDefault(data[1]) ?? data[1];

                                    if (data.Length == 3)
                                        name = $"{name}{data[2]}";

                                    CurrentRouteBlip.Blip.SetData("GPSRouteName", name);
                                }
                                else
                                {
                                    CurrentRouteBlip?.Destroy();

                                    CurrentRouteBlip = Core.CreateGPS(new Vector3(route.X, route.Y, 0f), uint.MaxValue, true);

                                    CurrentRouteBlip.Blip.SetData("GPSRouteId", routeId);

                                    var data = routeId.Split('&');

                                    var name = Locale.GPSApp.Names.GetValueOrDefault(data[1]) ?? data[1];

                                    if (data.Length == 3)
                                        name = $"{name}{data[2]}";

                                    CurrentRouteBlip.Blip.SetData("GPSRouteName", name);
                                }

                                return;
                            }
                        }
                    }
                }
            });
        }

        public static Action<int> CurrentTransactionAction { get; set; }

        public static void Show()
        {
            if (CEF.Phone.Phone.CurrentApp == AppTypes.None)
                CEF.Phone.Phone.SwitchMenu(false);

            CEF.Phone.Phone.CurrentApp = AppTypes.Navigator;

            CEF.Phone.Phone.CurrentAppTab = -1;

            string curRouteId = null;

            if (CurrentRouteBlip?.Blip?.DoesExist() != true)
            {
                CurrentRouteBlip?.Destroy();

                CurrentRouteBlip = null;
            }
            else
            {
                curRouteId = CurrentRouteBlip.Blip.GetData<string>("GPSRouteId");

                if (curRouteId == null)
                {
                    CurrentRouteBlip.Destroy();

                    CurrentRouteBlip = null;
                }
            }

            RouteUpdateTask?.Cancel();

            if (curRouteId == null)
            {
                RouteUpdateTask = null;

                Browser.Window.ExecuteJs("Phone.drawGpsApp", null, AllPositions.Select(x => new object[] { Locale.GPSApp.Names.GetValueOrDefault(x.Key) ?? x.Key, x.Value.Select(y => new object[] { y.Key, Locale.GPSApp.Names.GetValueOrDefault(y.Key) ?? y.Key }) }));
            }
            else
            {
                Browser.Window.ExecuteJs("Phone.drawGpsApp", new object[] { "asd", "asd1" }, AllPositions.Select(x => new object[] { Locale.GPSApp.Names.GetValueOrDefault(x.Key) ?? x.Key, x.Value.Select(y => new object[] { y.Key, Locale.GPSApp.Names.GetValueOrDefault(y.Key) ?? y.Key }) }));

                RouteUpdateTask = new AsyncTask(() =>
                {
                    if (CEF.Phone.Phone.CurrentApp == AppTypes.Navigator && CEF.Phone.Phone.CurrentAppTab == -1)
                    {
                        if (CurrentRouteBlip?.Exists != true)
                        {
                            CurrentRouteBlip = null;

                            Browser.Window.ExecuteJs("Phone.removeCurRoute();");
                        }
                        else
                        {
                            Browser.Window.ExecuteJs("Phone.updateCurRoute", new List<object>() { CurrentRouteBlip.Blip.GetData<string>("GPSRouteName") ?? "null", $"({Player.LocalPlayer.Position.DistanceIgnoreZ(CurrentRouteBlip.Position).ToString("0.000")} m.)" });
                        }
                    }
                }, 1000, true, 0);

                RouteUpdateTask.Run();
            }
        }

        public static void ShowTab(string sectionId)
        {
            var subSections = AllPositions.Values.Select(x => x.GetValueOrDefault(sectionId)).Where(x => x != null).FirstOrDefault();

            if (subSections == null)
                return;

            CEF.Phone.Phone.CurrentAppTab = 1;

            if (RouteUpdateTask != null)
            {
                RouteUpdateTask.Cancel();

                RouteUpdateTask = null;
            }

            Browser.Window.ExecuteJs("Phone.fillGpsRoutes", new object[] { new object[] { Locale.GPSApp.Names.GetValueOrDefault(sectionId) ?? sectionId, subSections.Select(x => { var data = x.Key.Split('&'); var name = Locale.GPSApp.Names.GetValueOrDefault(data[1]) ?? data[1]; return new object[] { data[0], data.Length == 3 ? $"{name}{data[2]}" : name }; }) } }, true);
        }

        public static bool AddPosition(string sectionId, string subSectionId, string routeId, string routeNameId, RAGE.Ui.Cursor.Vector2 pos)
        {
            var section = AllPositions.GetValueOrDefault(sectionId);

            if (section == null)
            {
                section = new Dictionary<string, Dictionary<string, RAGE.Ui.Cursor.Vector2>>() { { subSectionId, new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { $"{routeId}&{routeNameId}", pos } } } };

                AllPositions.Add(sectionId, section);

                return true;
            }
            else
            {
                var subSection = section.GetValueOrDefault(subSectionId);

                if (subSection == null)
                {
                    var routeHashCode = routeId.GetHashCode();

                    subSection = new Dictionary<string, RAGE.Ui.Cursor.Vector2>() { { $"{routeId}&{routeNameId}", pos } };

                    section.Add(subSectionId, subSection);

                    return true;
                }
                else
                {
                    if (subSection.TryAdd($"{routeId}&{routeNameId}", pos))
                    {
                        return true;
                    }

                    return false;
                }
            }
        }
    }
}