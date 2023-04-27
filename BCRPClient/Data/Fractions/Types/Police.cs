using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPClient.Data.Fractions
{
    public class Police : Fraction, IUniformable
    {
        public Police(Types Type, string Name, uint StorageContainerId, string ContainerPos, string CWbPos, byte MaxRank, string LockerRoomPositionsStr, string CreationWorkbenchPricesJs, string ArrestCellsPositionsJs, string ArrestMenuPositionsStr) : base(Type, Name, StorageContainerId, ContainerPos, CWbPos, MaxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(CreationWorkbenchPricesJs))
        {
            var lockerPoses = RAGE.Util.Json.Deserialize<Vector3[]>(LockerRoomPositionsStr);

            for (int i = 0; i < lockerPoses.Length; i++)
            {
                var pos = lockerPoses[i];

                var lockerRoomCs = new Additional.Cylinder(pos, 1f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.FractionLockerRoomInteract,

                    ActionType = Additional.ExtraColshape.ActionTypes.FractionInteract,

                    Data = $"{(int)Type}_{i}",
                };

                var lockerRoomText = new TextLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
                {
                    Font = 0,
                };
            }

            var arrestMenuPoses = RAGE.Util.Json.Deserialize<Vector3[]>(ArrestMenuPositionsStr);

            for (int i = 0; i < arrestMenuPoses.Length; i++)
            {
                var pos = arrestMenuPoses[i];

                pos.Z -= 1f;

                var arrestMenuCs = new Additional.Cylinder(pos, 1f, 2.5f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null)
                {
                    InteractionType = Additional.ExtraColshape.InteractionTypes.FractionPoliceArrestMenuInteract,

                    ActionType = Additional.ExtraColshape.ActionTypes.FractionInteract,

                    Data = $"{(int)Type}_{i}",
                };

                var arrestMenuText = new TextLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Управление задержанными\nв СИЗО", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.MAIN_DIMENSION)
                {
                    Font = 0,
                };
            }

            if (Type == Types.COP_LS)
            {
                UniformNames = new List<string>()
                {
                    "Стандартная форма",
                    "Форма для специальных операций",
                    "Форма руководства",
                };
            }

            this.ArrestCellsPositions = RAGE.Util.Json.Deserialize<Vector3[]>(ArrestCellsPositionsJs);
        }

        public static Dictionary<string, uint[]> NumberplatePrices { get; set; }

        public Vector3[] ArrestCellsPositions { get; set; }

        public List<string> UniformNames { get; set; }

        public override void OnStartMembership(params object[] args)
        {
            base.OnStartMembership(args);

            SetCurrentData("Calls", ((JArray)args[3]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new CallInfo() { Message = d[2], Type = byte.Parse(d[0]), Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[3])).DateTime, Position = new Vector3(float.Parse(d[4]), float.Parse(d[5]), float.Parse(d[6])), Player = RAGE.Elements.Entities.Players.GetAtRemote(ushort.Parse(d[1])) }; }).ToList());

            SetCurrentData("Fines", ((JArray)args[4]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new FineInfo() { Amount = uint.Parse(d[3]), Reason = d[4], Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[2])).DateTime, Member = d[0], Target = d[1] }; }).ToList());

            SetCurrentData("APBs", ((JArray)args[5]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new APBInfo() { Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[1])).DateTime, TargetName = d[2], Member = d[3], Details = d[4], Id = uint.Parse(d[0]) }; }).ToList());

            SetCurrentData("Notifications", ((JArray)args[6]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new NotificationInfo() { Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[1])).DateTime, Id = ushort.Parse(d[0]), Text = d[2], Position = d.Length > 4 ? new Vector3(float.Parse(d[3]), float.Parse(d[4]), float.Parse(d[5])) : null }; }).ToList());

            SetCurrentData("GPSTrackers", ((JArray)args[7]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new GPSTrackerInfo() { Id = uint.Parse(d[0]), InstallerStr = d[1], VehicleStr = d[2] }; }).ToList());

            SetCurrentData("Arrests", ((JArray)args[8]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new ArrestInfo() { Id = uint.Parse(d[0]), TargetName = d[1], MemberName = d[2], Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[3])).DateTime, }; }).ToList());

            SetCurrentData("ArrestsAmount", Convert.ToUInt32(args[9]));

            CEF.HUD.Menu.UpdateCurrentTypes(true, CEF.HUD.Menu.Types.Fraction_Police_TabletPC);

            KeyBinds.CurrentExtraAction0 = () => CuffPlayer(null, null);

            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("documents", 0, "fraction_docs");

            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("job", 10, "fraction_invite");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("job", 11, "police_search");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("job", 12, "cuffs");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("job", 13, "police_escort");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("job", 14, "prison");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("job", 15, "fine");
            CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("job", 0, "take_license");

            CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 16, "gps_tracker");
            CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 17, "police_search");
            CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 18, "player_to_veh");
            CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 19, "player_to_trunk");
            CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 0, "player_from_veh");

            SetCurrentData("LastCuffed", DateTime.MinValue);

            var arrestCs = new List<Additional.ExtraColshape>();

            if (Type == Types.COP_BLAINE)
            {
                arrestCs.Add(new Additional.Cuboid(new Vector3(-430.256775f, 5997.575f, 32.45621f), 8.5f, 10f, 3.7f, 135f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null));
            }
            else if (Type == Types.COP_LS)
            {
                arrestCs.Add(new Additional.Cuboid(new Vector3(472.494965f, -998.1451f, 25.3779182f), 21f, 11f, 3.5f, 0f, false, Utils.RedColor, Settings.MAIN_DIMENSION, null));
            }

            foreach (var x in arrestCs)
            {
                x.Name = "FRAC_COP_ARREST_CS";

                x.OnEnter = (cancel) =>
                {
                    Utils.ConsoleOutput("CAN ARREST");
                };

                x.OnExit = (cancel) =>
                {
                    Utils.ConsoleOutput("CAN NOT ARREST");
                };
            }
        }

        public override void OnEndMembership()
        {
            CEF.HUD.Menu.UpdateCurrentTypes(false, CEF.HUD.Menu.Types.Fraction_Police_TabletPC);

            KeyBinds.CurrentExtraAction0 = null;

            Additional.ExtraColshape.All.Values.Where(x => x.Name == "FRAC_COP_ARREST_CS").ToList().ForEach(x => x.Destroy());

            base.OnEndMembership();
        }

        public static async void ShowPoliceTabletPc()
        {
            var pData = Sync.Players.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var fData = pData.CurrentFraction;

            if (fData == null)
                return;

            var pName = Player.LocalPlayer.Name;

            var finesAmount = (uint)(fData.GetCurrentData<List<FineInfo>>("Fines")?.Where(x => x.Member.StartsWith(pName)).Count() ?? 0);

            var isOnDuty = fData.GetCurrentData<bool>("IsOnDuty");

            await CEF.PoliceTabletPC.Show(fData, isOnDuty, AllMembers.GetValueOrDefault(pData.CID)?.Rank ?? 0, finesAmount, fData.GetCurrentData<uint>("ArrestsAmount"));
        }

        public async void CuffPlayer(Player player, bool? state)
        {
            if (player == null)
                player = BCRPClient.Interaction.CurrentEntity as Player;

            if (player?.Exists != true)
                return;

            var tData = Sync.Players.GetData(player);

            if (tData == null)
                return;

            var cuffState = tData.IsCuffed;

            var lastSent = GetCurrentData<DateTime>("LastCuffed");

            if (lastSent.IsSpam(1500, false, true))
                return;

            SetCurrentData("LastCuffed", Sync.World.ServerTime);

            var res = (int)await Events.CallRemoteProc("Police::Cuff", player, state);

            if (res == byte.MaxValue)
            {
                if (cuffState)
                {
                    CEF.Notification.Show("Cuffs::0_0_1", Utils.GetPlayerName(player, true, false, true));
                }
                else
                {
                    CEF.Notification.Show("Cuffs::0_0_0", Utils.GetPlayerName(player, true, false, true));
                }
            }
        }

        public void EscortPlayer(Player player, bool? state)
        {
            if (player == null)
                player = BCRPClient.Interaction.CurrentEntity as Player;

            if (player?.Exists != true)
                return;

            var tData = Sync.Players.GetData(player);

            if (tData == null)
                return;

            if (!tData.IsCuffed)
            {
                // notify

                return;
            }

            if (tData.IsAttachedTo is Entity entity && entity != Player.LocalPlayer)
            {
                // notify

                return;
            }
        }

        public class CallInfo
        {
            public byte Type { get; set; }

            public Player Player { get; set; }

            public Vector3 Position { get; set; }

            public string Message { get; set; }

            public DateTime Time { get; set; }

            public CallInfo()
            {

            }
        }

        public class FineInfo
        {
            public string Member { get; set; }

            public string Target { get; set; }

            public uint Amount { get; set; }

            public string Reason { get; set; }

            public DateTime Time { get; set; }

            public FineInfo()
            {

            }
        }

        public class APBInfo
        {
            public uint Id { get; set; }

            public string TargetName { get; set; }

            public string Details { get; set; }

            public string Member { get; set; }

            public DateTime Time { get; set; }

            public APBInfo()
            {

            }
        }

        public class NotificationInfo
        {
            public ushort Id { get; set; }

            public string Text { get; set; }

            public Vector3 Position { get; set; }

            public DateTime Time { get; set; }

            public NotificationInfo()
            {

            }
        }

        public class GPSTrackerInfo
        {
            public uint Id { get; set; }

            public string VehicleStr { get; set; }

            public string InstallerStr { get; set; }

            public GPSTrackerInfo()
            {

            }
        }

        public class ArrestInfo
        {
            public uint Id { get; set; }

            public DateTime Time { get; set; }

            public string TargetName { get; set; }
            public string MemberName { get; set; }

            public ArrestInfo()
            {

            }
        }
    }

    public class PoliceEvents : Events.Script
    {
        public PoliceEvents()
        {
            Events.Add("FPolice::CC", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var calls = pData.CurrentFraction?.GetCurrentData<List<Police.CallInfo>>("Calls");

                if (calls == null)
                    return;

                var rid = Convert.ToUInt16(args[0]);

                if (args.Length < 4)
                {
                    var call = calls.Where(x => x.Player.RemoteId == rid).FirstOrDefault();

                    if (call == null)
                        return;

                    calls.Remove(call);

                    if (CEF.PoliceTabletPC.CurrentTab == 2 || CEF.PoliceTabletPC.LastTab == 2)
                    {
                        CEF.Browser.Window.ExecuteJs("PoliceTablet.removeElem", rid);
                    }

                    // todo
                }
                else
                {
                    var call = new Police.CallInfo() { Type = Convert.ToByte(args[1]), Time = Sync.World.ServerTime, Message = (string)args[2], Position = (Vector3)args[3], Player = RAGE.Elements.Entities.Players.GetAtRemote(rid) };

                    calls.Add(call);

                    if (CEF.PoliceTabletPC.CurrentTab == 2 || CEF.PoliceTabletPC.LastTab == 2)
                    {
                        var arg = CEF.PoliceTabletPC.GetCallRowList(call, Player.LocalPlayer.Position);

                        arg.Insert(0, 0);

                        CEF.Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                    }

                    // todo
                }
            });

            Events.Add("FPolice::APBC", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var apbs = pData.CurrentFraction?.GetCurrentData<List<Police.APBInfo>>("APBs");

                if (apbs == null)
                    return;

                var uid = Convert.ToUInt32(args[0]);

                if (args.Length < 4)
                {
                    var apb = apbs.Where(x => x.Id == uid).FirstOrDefault();

                    if (apb == null)
                        return;

                    apbs.Remove(apb);

                    if (CEF.PoliceTabletPC.CurrentTab == 4 || CEF.PoliceTabletPC.LastTab == 4 || CEF.PoliceTabletPC.LastTab == 41)
                    {
                        if (Player.LocalPlayer.GetData<Data.Fractions.Police.APBInfo>("PoliceTablet::APBViewId")?.Id == uid)
                        {
                            CEF.PoliceTabletPC.TabBack();
                        }

                        CEF.Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);
                    }

                    // todo
                }
                else
                {
                    var apb = new Police.APBInfo() { Id = uid, Time = Sync.World.ServerTime, TargetName = (string)args[1], Member = (string)args[2], Details = (string)args[3] };

                    apbs.Add(apb);

                    if (CEF.PoliceTabletPC.CurrentTab == 4 || CEF.PoliceTabletPC.LastTab == 4 || CEF.PoliceTabletPC.LastTab == 41)
                    {
                        var arg = CEF.PoliceTabletPC.GetAPBRowList(apb);

                        arg.Insert(0, 2);

                        CEF.Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                    }

                    // todo
                }
            });

            Events.Add("FPolice::GPSTC", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var trackers = pData.CurrentFraction?.GetCurrentData<List<Police.GPSTrackerInfo>>("GPSTrackers");

                if (trackers == null)
                    return;

                var uid = Convert.ToUInt32(args[0]);

                if (args.Length < 3)
                {
                    var gpsTracker = trackers.Where(x => x.Id == uid).FirstOrDefault();

                    if (gpsTracker == null)
                        return;

                    trackers.Remove(gpsTracker);

                    if (CEF.PoliceTabletPC.CurrentTab == 6 || CEF.PoliceTabletPC.LastTab == 6)
                    {
                        CEF.Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);
                    }

                    // todo
                }
                else
                {
                    var gpsTracker = new Police.GPSTrackerInfo() { Id = uid, InstallerStr = (string)args[1], VehicleStr = (string)args[2] };

                    trackers.Add(gpsTracker);

                    if (CEF.PoliceTabletPC.CurrentTab == 6 || CEF.PoliceTabletPC.LastTab == 6)
                    {
                        var arg = CEF.PoliceTabletPC.GetGPSTrackerRowList(gpsTracker);

                        arg.Insert(0, 4);

                        CEF.Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                    }

                    // todo
                }
            });

            Events.Add("FPolice::NOTIFC", (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var notifics = pData.CurrentFraction?.GetCurrentData<List<Police.NotificationInfo>>("Notifications");

                if (notifics == null)
                    return;

                var uid = Convert.ToUInt16(args[0]);

                if (args.Length < 3)
                {
                    var notific = notifics.Where(x => x.Id == uid).FirstOrDefault();

                    if (notific == null)
                        return;

                    notifics.Remove(notific);

                    if (CEF.PoliceTabletPC.CurrentTab == 5 || CEF.PoliceTabletPC.LastTab == 5)
                    {
                        CEF.Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);
                    }

                    // todo
                }
                else
                {
                    var notific = new Police.NotificationInfo() { Id = uid, Time = Sync.World.ServerTime, Text = (string)args[1], Position = (Vector3)args[2] };

                    notifics.Add(notific);

                    if (CEF.PoliceTabletPC.CurrentTab == 5 || CEF.PoliceTabletPC.LastTab == 5)
                    {
                        var arg = CEF.PoliceTabletPC.GetNotificationRowList(notific, Player.LocalPlayer.Position);

                        arg.Insert(0, 3);

                        CEF.Browser.Window.ExecuteJs("PoliceTablet.addElem", arg.ToArray());
                    }

                    // todo
                }
            });
        }
    }
}
