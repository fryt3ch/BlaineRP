using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BCRPClient.CEF
{
    public class PoliceTabletPC : Events.Script
    {
        public static bool IsActive => CEF.Browser.IsActive(Browser.IntTypes.PoliceTabletPC);

        public static byte CurrentTab { get; private set; }
        public static byte LastTab { get; private set; }

        private static int EscBindIdx { get; set; } = -1;

        public static DateTime LastSent;

        private static Dictionary<string, object> CurrentFoundPlayerData { get; set; }

        public PoliceTabletPC()
        {
            Events.Add("PoliceTablet::Code", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var code = Utils.ToByte(args[0]);

                if (code == 3)
                {
                    var curCallInfo = pData.CurrentFraction?.GetCurrentData<Data.Fractions.Police.CallInfo>("PoliceCallData");

                    if (curCallInfo == null)
                    {
                        CEF.Notification.ShowError(Locale.Get("POLICETABLET_CALL_NOTAKEN"));

                        return;
                    }

                    if (curCallInfo.Position.DistanceTo(Player.LocalPlayer.Position) > 10f)
                    {
                        CEF.Notification.ShowError(Locale.Get("POLICETABLET_CALL_FINISH_0", 10));

                        return;
                    }

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    var res = (bool)await Events.CallRemoteProc("Police::CODEF", curCallInfo.Player.RemoteId, curCallInfo.Type);
                }
                else
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    var res = (bool)await Events.CallRemoteProc("Police::CODE", code);

                    if (res)
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICETABLET_L_CODE", Locale.Get($"POLICETABLET_L_CT_{code}")));
                    }
                }
            });

            Events.Add("PoliceTablet::SearchPlayer", async (args) =>
            {
                var str = ((string)args[0])?.Trim();

                if (str == null || str.Length == 0)
                {
                    return;
                }

                sbyte searchType = -1;
                string searchStr = null;

                if (str[0] == '+') // phone
                {
                    var phoneNumberStr = str.Length == 1 ? "" : str.Substring(1);

                    uint phoneNumberT;

                    if (!uint.TryParse(phoneNumberStr, out phoneNumberT) || CEF.PhoneApps.PhoneApp.DefaultNumbersActions.Where(x => x.Key.Contains(phoneNumberStr)).Any())
                    {
                        CEF.Notification.Show("Police::DBS::PNF0");

                        return;
                    }

                    if (CurrentFoundPlayerData != null && CurrentFoundPlayerData.GetValueOrDefault("PhoneNumber") as decimal? == phoneNumberT)
                    {
                        CEF.Notification.Show("Police::DBS::PAF");

                        return;
                    }

                    searchType = 0;

                    searchStr = phoneNumberT.ToString();
                }
                else if (str.Split(' ') is string[] nameArr && nameArr.Length == 2) // name
                {
                    var nameT = nameArr[0].ToLower(); var surnameT = nameArr[1].ToLower();

                    if (nameT.Length > 0)
                        nameT = char.ToUpper(nameT[0]) + nameT.Substring(1);

                    if (surnameT.Length > 0)
                        surnameT = char.ToUpper(surnameT[0]) + surnameT.Substring(1);

                    if (!Utils.IsNameValid(nameT) || !Utils.IsNameValid(surnameT))
                    {
                        CEF.Notification.Show("Police::DBS::PNF1");

                        return;
                    }

                    if (CurrentFoundPlayerData != null && CurrentFoundPlayerData.GetValueOrDefault("Name") as string == nameT && CurrentFoundPlayerData.GetValueOrDefault("Surname") as string == surnameT)
                    {
                        CEF.Notification.Show("Police::DBS::PAF");

                        return;
                    }

                    searchType = 1;

                    searchStr = $"{nameT} {surnameT}";
                }
                else if (str[0] == '@') // veh plate
                {
                    var vehPlateStr = (str.Length > 1 ? str.Substring(1) : "").ToUpper();

                    if (!new Regex(@"^[A-Z0-9]{1,8}$").IsMatch(vehPlateStr))
                    {
                        CEF.Notification.Show("Police::DBS::PNF2");

                        return;
                    }

                    if (CurrentFoundPlayerData != null && (CurrentFoundPlayerData.GetValueOrDefault("Vehicles") as List<Tuple<Data.Vehicles.Vehicle, string, Utils.Colour>>)?.Where(x => x.Item2 == vehPlateStr).Any() == true)
                    {
                        CEF.Notification.Show("Police::DBS::PAF");

                        return;
                    }

                    searchType = 2;

                    searchStr = vehPlateStr;
                }
                else // pid
                {
                    uint pid;

                    if (!uint.TryParse(str, out pid))
                    {
                        CEF.Notification.Show("Police::DBS::PNF3");

                        return;
                    }

                    if (CurrentFoundPlayerData != null && (CurrentFoundPlayerData.GetValueOrDefault("CID") as decimal? == pid))
                    {
                        CEF.Notification.Show("Police::DBS::PAF");

                        return;
                    }

                    searchType = 3;

                    searchStr = str;
                }

                if (LastSent.IsSpam(1000, false, true))
                    return;

                LastSent = Sync.World.ServerTime;

                var resObj = await Events.CallRemoteProc("Police::DBS", searchType, searchStr);

                if (resObj is JObject res)
                {
                    if (!IsActive)
                        return;

                    var name = (string)res["N"];
                    var surname = (string)res["S"];
                    var cid = Utils.ToDecimal(res["I"]);
                    var birthDate = DateTimeOffset.FromUnixTimeSeconds(Utils.ToInt64(res["BD"])).DateTime;
                    var sex = (bool)res["G"];
                    var losSantosAllowed = (bool)res["LA"];
                    var phoneNumber = Utils.ToDecimal(res["PN"]);
                    var fractionData = res.ContainsKey("FT") ? Data.Fractions.Fraction.Get((Data.Fractions.Types)Utils.ToInt32(res["FT"])) : null;
                    var fractionRank = res.ContainsKey("FR") ? Utils.ToByte(res["FT"]) : (byte)0;

                    var houseData = res.ContainsKey("H") ? Data.Locations.House.All[Utils.ToUInt32(res["H"])] : null;
                    var apsData = res.ContainsKey("A") ? Data.Locations.Apartments.All[Utils.ToUInt32(res["A"])] : null;

                    var vehicles = ((JArray)res["V"]).ToObject<List<string>>().Select(x => { var sData = x.Split('&'); return new Tuple<Data.Vehicles.Vehicle, string, Utils.Colour>(Data.Vehicles.GetById(sData[0]), sData[1], new Utils.Colour(sData[2])); }).ToList();

                    CurrentFoundPlayerData = new Dictionary<string, object>()
                    {
                        { "Name", name },
                        { "Surname", surname },
                        { "CID", cid },
                        { "BirthDate", birthDate },
                        { "Sex", sex },
                        { "LosSantosAllowed", losSantosAllowed },
                        { "PhoneNumber", phoneNumber },
                        { "FractionData", fractionData },
                        { "FractionRank", fractionRank },
                        { "HouseData", houseData },
                        { "ApsData", apsData },
                        { "Vehicles", vehicles },
                    };

                    if (CurrentTab != 1)
                        LastTab = CurrentTab;
                    
                    CurrentTab = 1;

                    CEF.Browser.Window.ExecuteJs
                    (
                        "PoliceTablet.showPlayerInfo",

                        $"{name} {surname}",
                        cid,
                        birthDate.ToString("dd.MM.yyyy"),
                        sex,
                        losSantosAllowed,
                        phoneNumber,
                        houseData == null ? null : $"#{houseData.Id}, {Utils.GetStreetName(houseData.Position)}",
                        apsData == null ? null : $"#{apsData.Id}, {Data.Locations.ApartmentsRoot.All[apsData.RootId].Name}",
                        null, // organisation
                        fractionData == null ? null : $"{fractionData.Name} | {fractionData.GetRankName(fractionRank)}",
                        vehicles.Select(x => new object[] { x.Item1.Name, x.Item2 == null || x.Item2.Length == 0 ? null : x.Item2, x.Item3.HEXNoAlpha })
                    );
                }
                else if (resObj == null)
                {
                    CEF.Notification.Show($"Police::DBS::PNF{searchType}");

                    return;
                }
            });

            Events.Add("PoliceTablet::Action", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var actionId = (int)args[0];

                if (actionId == 0) // show calls
                {
                    ShowCallsTab(pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.CallInfo>>("Calls"));
                }
                else if (actionId == 1) // show fines
                {
                    ShowFinesTab(pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.FineInfo>>("Fines"));
                }
                else if (actionId == 2) // show APBs
                {
                    ShowAPBsTab(pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.APBInfo>>("APBs"));
                }
                else if (actionId == 3) // show notifications
                {
                    ShowNotificationsTab(pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.NotificationInfo>>("Notifications"));
                }
                else if (actionId == 4) // show gps trackers
                {
                    ShowGPSTrackersTab(pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.GPSTrackerInfo>>("GPSTrackers"));
                }
            });

            Events.Add("PoliceTablet::Back", (args) =>
            {
                TabBack();
            });

            Events.Add("PoliceTablet::ArrestShow", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if ((bool)args[0]) // add
                {
                    LastTab = CurrentTab;
                    CurrentTab = 41;

                    Player.LocalPlayer.ResetData("PoliceTablet::APBViewId");

                    CEF.Browser.Window.ExecuteJs("PoliceTablet.showArrestEdit", true);
                }
                else // view
                {
                    var id = Utils.ToUInt32(args[1]);

                    var apbData = pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.APBInfo>>("APBs")?.Where(x => x.Id == id).FirstOrDefault();

                    if (apbData == null)
                    {
                        return;
                    }

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    var largeDetails = (string)await Events.CallRemoteProc("Police::APBGLD", id);

                    if (largeDetails == null)
                        return;

                    LastTab = CurrentTab;
                    CurrentTab = 41;

                    Player.LocalPlayer.SetData("PoliceTablet::APBViewId", apbData);

                    CEF.Browser.Window.ExecuteJs("PoliceTablet.showArrestEdit", true, apbData.Time.ToString("dd.MM.yyyy HH:mm"), apbData.TargetName, apbData.Member, apbData.Details, largeDetails);
                }
            });

            Events.Add("PoliceTablet::ArrestButton", async (args) =>
            {
                if (Player.LocalPlayer.GetData<Data.Fractions.Police.APBInfo>("PoliceTablet::APBViewId") is Data.Fractions.Police.APBInfo apbInfo)
                {
                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Police::APBF", apbInfo.Id))
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICETABLET_APB_FINISH", apbInfo.Id));
                    }
                    else
                    {

                    }
                }
                else
                {
                    var name = (string)args[1];

                    var details = (string)args[2];

                    var largeDetails = (string)args[3];

                    // text check

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Police::APBA", name, details, largeDetails))
                    {
                        CEF.Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICETABLET_APB_ADD", 24));

                        TabBack();
                    }
                    else
                    {

                    }
                }
            });

            Events.Add("PoliceTablet::ShowGPS", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (CurrentTab == 2)
                {
                    var callRid = Utils.ToUInt16(args[0]);

                    var callInfo = pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.CallInfo>>("Calls")?.Where(x => x.Player.RemoteId == callRid).FirstOrDefault();

                    if (callInfo == null)
                    {
                        CEF.Notification.ShowError(Locale.Get("POLICETABLET_CALL_NOTEXISTS"));

                        return;
                    }

                    Additional.ExtraBlips.CreateGPS(callInfo.Position, Player.LocalPlayer.Dimension, true);
                }
                else if (CurrentTab == 5) // notific
                {

                }
                else if (CurrentTab == 6) // gps tracker
                {
                    var gpsTrackerId = Utils.ToUInt32(args[0]);

                    var gpsTrackerInfo = pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.GPSTrackerInfo>>("GPSTrackers")?.Where(x => x.Id == gpsTrackerId).FirstOrDefault();

                    if (gpsTrackerInfo == null)
                    {
                        CEF.Notification.ShowError(Locale.Get("POLICETABLET_GPSTR_NOTEXISTS"));

                        return;
                    }

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    var res = await Events.CallRemoteProc("Police::GPSTRL", gpsTrackerId);

                    if (res != null)
                    {
                        var vehicle = RAGE.Elements.Entities.Vehicles.GetAtRemote(Utils.ToUInt16(res));

                        if (vehicle?.Exists == true)
                        {
                            // todo
                        }
                    }
                }
            });

            Events.Add("PoliceTablet::ResidenceLocation", (args) =>
            {
                if (CurrentFoundPlayerData == null)
                    return;

                var idx = (int)args[0];

                if (idx == 0) // house
                {
                    var houseData = CurrentFoundPlayerData.GetValueOrDefault("HouseData") as Data.Locations.House;

                    if (houseData == null)
                        return;

                    if (Player.LocalPlayer.GetData<object>("House::CurrentHouse") == houseData)
                    {
                        CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICETABLET_RFIND_INHOUSENOW"));

                        return;
                    }
                    else
                    {
                        var pos = houseData.Position;

                        Additional.ExtraBlips.CreateGPS(pos, Player.LocalPlayer.Dimension, true);
                    }
                }
                else if (idx == 1) // aps
                {
                    var apsData = CurrentFoundPlayerData.GetValueOrDefault("ApsData") as Data.Locations.Apartments;

                    if (apsData == null)
                        return;

                    if (Player.LocalPlayer.GetData<object>("House::CurrentHouse") == apsData)
                    {
                        CEF.Notification.Show(Notification.Types.Information, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICETABLET_RFIND_INFLATNOW"));

                        return;
                    }
                    else
                    {
                        var aRoot = Data.Locations.ApartmentsRoot.All[apsData.RootId];

                        var pos = Player.LocalPlayer.GetData<Data.Locations.ApartmentsRoot>("ApartmentsRoot::Current") == aRoot ? apsData.Position : aRoot.PositionEnter;

                        Additional.ExtraBlips.CreateGPS(pos, Player.LocalPlayer.Dimension, true);
                    }
                }
                else if (idx == 2) // org
                {
                    // todo
                }
            });

            Events.Add("PoliceTablet::RemoveGPS", async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (CurrentTab == 6)
                {
                    var gpsTrackerId = Utils.ToUInt32(args[0]);

                    var gpsTrackerInfo = pData.CurrentFraction?.GetCurrentData<List<Data.Fractions.Police.GPSTrackerInfo>>("GPSTrackers")?.Where(x => x.Id == gpsTrackerId).FirstOrDefault();

                    if (gpsTrackerInfo == null)
                    {
                        CEF.Notification.ShowError(Locale.Get("POLICETABLET_GPSTR_NOTEXISTS"));

                        return;
                    }

                    if (LastSent.IsSpam(1000, false, true))
                        return;

                    LastSent = Sync.World.ServerTime;

                    if ((bool)await Events.CallRemoteProc("Police::GPSTRD", gpsTrackerId))
                    {
                        // notify
                    }
                }
            });
        }

        private static void ShowDefaultTab()
        {
            CurrentTab = 0;
            LastTab = 0;

            CEF.Browser.Window.ExecuteJs("PoliceTablet.switchContainer", -1);
        }

        public static void TabBack()
        {
            if (!IsActive)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            if (CurrentTab == 1)
            {
                if (CurrentFoundPlayerData != null)
                    CurrentFoundPlayerData = null;

                if (LastTab == 2 || LastTab == 3 || LastTab == 4 || LastTab == 5 || LastTab == 6)
                {
                    CurrentTab = LastTab;
                    LastTab = 0;

                    CEF.Browser.Window.ExecuteJs("PoliceTablet.switchContainer", 0);
                }
                else if (LastTab == 41)
                {
                    CurrentTab = LastTab;
                    LastTab = 4;

                    CEF.Browser.Window.ExecuteJs("PoliceTablet.switchContainer", 1);
                }
                else
                {
                    ShowDefaultTab();
                }
            }
            else if (CurrentTab == 2 || CurrentTab == 3 || CurrentTab == 4 || CurrentTab == 5 || CurrentTab == 6)
            {
                ShowDefaultTab();
            }
            else if (CurrentTab == 41)
            {
                CurrentTab = 4;
                LastTab = 0;

                CEF.Browser.Window.ExecuteJs("PoliceTablet.switchContainer", 0);
            }
        }

        public static async System.Threading.Tasks.Task Show(Data.Fractions.Fraction fData, bool isOnDuty, byte myRank, uint fineCount, uint arrestCount)
        {
            if (IsActive)
                return;

            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            await CEF.Browser.Render(Browser.IntTypes.PoliceTabletPC, true, true);

            CEF.Browser.Window.ExecuteJs("PoliceTablet.draw", fData.Name, new object[] { new object[] { $"{Player.LocalPlayer.Name}", $"{fData.GetRankName(myRank)} [{myRank}]", isOnDuty, fineCount, arrestCount } });

            EscBindIdx = KeyBinds.Bind(RAGE.Ui.VirtualKeys.Escape, true, () => Close());

            CEF.Cursor.Show(true, true);

            Events.CallRemote("Police::MRPCA", 0);
        }

        public static void ShowCallsTab(List<Data.Fractions.Police.CallInfo> allCalls)
        {
            if (!IsActive)
                return;

            if (allCalls == null)
                return;

            LastTab = 0;
            CurrentTab = 2;

            var pPos = Player.LocalPlayer.Position;

            CEF.Browser.Window.ExecuteJs("PoliceTablet.fillActionInformation", 0, allCalls.Where(x => x.Player?.Exists == true).OrderBy(x => x.Time).Select(x => GetCallRowList(x, pPos)).ToList());
        }

        public static List<object> GetCallRowList(Data.Fractions.Police.CallInfo x, Vector3 pPos) => new List<object> { x.Player.RemoteId, x.Time.ToString("HH:mm"), Locale.Get($"POLICETABLET_L_CT_{x.Type}"), $"{x.Player.Name}", Locale.Get("GEN_DIST_METERS_0", pPos.DistanceTo(x.Position).ToString("0.0")), x.Message.Length == 0 ? Locale.GetNullOtherwise($"POLICETABLET_L_CM_{x.Type}") ?? "" : x.Message };

        public static void ShowFinesTab(List<Data.Fractions.Police.FineInfo> allFines)
        {
            if (!IsActive)
                return;

            if (allFines == null)
                return;

            LastTab = 0;
            CurrentTab = 3;

            int i = 0;

            CEF.Browser.Window.ExecuteJs("PoliceTablet.fillActionInformation", 1, allFines.OrderByDescending(x => x.Time).Select(x => GetFineRowList(x, ref i)).ToList());
        }

        public static List<object> GetFineRowList(Data.Fractions.Police.FineInfo x, ref int i) => new List<object> { i++, x.Time.ToString("HH:mm"), x.Amount, x.Member, x.Target, x.Reason };

        public static void ShowAPBsTab(List<Data.Fractions.Police.APBInfo> allOrientations)
        {
            if (!IsActive)
                return;

            if (allOrientations == null)
                return;

            LastTab = 0;
            CurrentTab = 4;

            CEF.Browser.Window.ExecuteJs("PoliceTablet.fillActionInformation", 2, allOrientations.OrderBy(x => x.Time).Select(x => GetAPBRowList(x)).ToList());
        }

        public static List<object> GetAPBRowList(Data.Fractions.Police.APBInfo x) => new List<object>() { x.Id, x.Time.ToString("dd.MM HH:mm"), x.TargetName, x.Member, x.Details };

        public static void ShowNotificationsTab(List<Data.Fractions.Police.NotificationInfo> allNotifications)
        {
            if (!IsActive)
                return;

            if (allNotifications == null)
                return;

            LastTab = 0;
            CurrentTab = 5;

            var pPos = Player.LocalPlayer.Position;

            CEF.Browser.Window.ExecuteJs("PoliceTablet.fillActionInformation", 3, allNotifications.OrderByDescending(x => x.Time).Select(x => GetNotificationRowList(x, pPos)).ToList());
        }

        public static List<object> GetNotificationRowList(Data.Fractions.Police.NotificationInfo x, Vector3 pPos) => new List<object>() { x.Id, x.Time.ToString("HH:mm"), x.Text, x.Position == null ? "???" : x.Position.DistanceTo(pPos).ToString("0.0") };

        public static void ShowGPSTrackersTab(List<Data.Fractions.Police.GPSTrackerInfo> allGpsTrackers)
        {
            if (!IsActive)
                return;

            if (allGpsTrackers == null)
                return;

            LastTab = 0;
            CurrentTab = 6;

            CEF.Browser.Window.ExecuteJs("PoliceTablet.fillActionInformation", 4, allGpsTrackers.Select(x => GetGPSTrackerRowList(x)).ToList());
        }

        public static List<object> GetGPSTrackerRowList(Data.Fractions.Police.GPSTrackerInfo x) => new List<object>() { x.Id, x.Id, x.VehicleStr, x.InstallerStr, };

        public static void Close()
        {
            if (!IsActive)
                return;

            CEF.Browser.Render(Browser.IntTypes.PoliceTabletPC, false, false);

            Player.LocalPlayer.ResetData("PoliceTablet::APBViewId");

            if (CurrentFoundPlayerData != null)
                CurrentFoundPlayerData = null;

            KeyBinds.Unbind(EscBindIdx);

            EscBindIdx = -1;

            CurrentTab = 0;
            LastTab = 0;

            CEF.Cursor.Show(false, false);

            Events.CallRemote("Police::MRPCA", 1);
        }
    }
}
