using BCRPServer.Game.Data;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace BCRPServer.Game.Fractions
{
    public class Police : Fraction, IUniformable
    {
        public Police(Types Type, string Name) : base(Type, Name)
        {

        }

        public override string ClientData => $"Fractions.Types.{Type}, \"{Name}\", {ContainerId}, \"{ContainerPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPositions.SerializeToJson().Replace('\"', '\'')}\", {Ranks.Count - 1}, \"{LockerRoomPositions.SerializeToJson().Replace('\"', '\'')}\", \"{CreationWorkbenchPrices.SerializeToJson().Replace('"', '\'')}\", {(uint)MetaFlags}, \"{ArrestCellsPositions.Select(x => x.Position).SerializeToJson().Replace('"', '\'')}\", \"{ArrestMenuPositions.SerializeToJson().Replace('\"', '\'')}\"";

        public static Dictionary<string, uint[]> NumberplatePrices { get; } = new Dictionary<string, uint[]>()
        {
            {
                "np_0",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_1",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_2",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_3",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },

            {
                "np_4",

                new uint[]
                {
                    500_000, 450_000, 400_000, 150_000, 50_000, 15_000, 5_500, 1_500,
                }
            },
        };

        public static uint VehicleNumberplateRegPrice { get; set; } = 1_000;
        public static uint VehicleNumberplateUnRegPrice { get; set; } = 500;

        public static TimeSpan ArrestMaxTime { get; } = TimeSpan.FromHours(12);
        public static TimeSpan ArrestMaxTimeAfterAdd { get; } = TimeSpan.FromHours(16);
        public static TimeSpan ArrestMaxTimeChange { get; } = TimeSpan.FromHours(2);
        public static TimeSpan ArrestMinTimeChange { get; } = TimeSpan.FromHours(-2);

        public static TimeSpan CallExtraCooldownTime { get; } = TimeSpan.FromMinutes(1);
        public static TimeSpan CallDefaultCooldownTime { get; } = TimeSpan.FromMinutes(5);

        public const uint FineMinAmount = 100;
        public const uint FineMaxAmount = 100_000;

        public static Regex ArrestReason1Regex { get; } = new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{1,18}$", RegexOptions.Compiled);
        public static Regex ArrestReason2Regex { get; } = new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{0,100}$", RegexOptions.Compiled);
        public static Regex ArrestChangeReasonRegex { get; } = new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{1,18}$", RegexOptions.Compiled);
        public static Regex FineReasonRegex { get; } = new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{1,18}$", RegexOptions.Compiled);
        public static Regex PoliceCallReasonRegex { get; } = new Regex(@"^[A-Za-zА-Яа-я0-9,./?$#@!%^&*()'+=\-\[\]]{1,24}$", RegexOptions.Compiled);

        public static HashSet<PlayerData.LicenseTypes> AllowedLicenceTypesToRemove { get; } = new HashSet<PlayerData.LicenseTypes>() { PlayerData.LicenseTypes.A, PlayerData.LicenseTypes.B, PlayerData.LicenseTypes.C, PlayerData.LicenseTypes.D, PlayerData.LicenseTypes.Fly, PlayerData.LicenseTypes.Sea, PlayerData.LicenseTypes.Weapons, PlayerData.LicenseTypes.Hunting, };

        private static Dictionary<ushort, CallInfo> Calls { get; set; } = new Dictionary<ushort, CallInfo>();

        private static Dictionary<ushort, NotificationInfo> Notifications { get; set; } = new Dictionary<ushort, NotificationInfo>();

        private static Dictionary<uint, GPSTrackerInfo> GPSTrackers { get; set; } = new Dictionary<uint, GPSTrackerInfo>();

        private Dictionary<uint, ArrestInfo> ActiveArrests { get; set; } = new Dictionary<uint, ArrestInfo>();

        public static UidHandlerUInt32 APBUidHandler { get; set; } = new UidHandlerUInt32(1);
        public static UidHandlerUInt32 GPSTrackerUidHandler { get; set; } = new UidHandlerUInt32(0);
        public static UidHandlerUInt16 NotificationUidHandler { get; set; } = new UidHandlerUInt16(0);

        private static Dictionary<uint, APBInfo> APBs { get; set; } = new Dictionary<uint, APBInfo>();

        private List<FineInfo> Fines { get; set; } = new List<FineInfo>();

        public List<Customization.UniformTypes> UniformTypes { get; set; }

        public Vector3[] LockerRoomPositions { get; set; }

        public Utils.Vector4[] ArrestCellsPositions { get; set; }

        public Vector3[] ArrestMenuPositions { get; set; }

        public Utils.Vector4 ArrestFreePosition { get; set; }

        public Utils.Vector4 ArrestColshapePosition { get; set; }

        private static int LastArrestCellPositionUsed { get; set; }

        public bool IsPlayerInAnyUniform(PlayerData pData, bool notifyIfNot = false)
        {
            if (pData.CurrentUniform is Customization.UniformTypes uType)
                return UniformTypes.Contains(uType);

            if (notifyIfNot)
            {
                pData.Player.Notify("Fraction::NIUF");
            }

            return false;
        }

        public Vector3 GetNextArrestCellPosition()
        {
            var pos = LastArrestCellPositionUsed >= ArrestCellsPositions.Length ? ArrestCellsPositions[LastArrestCellPositionUsed = 0] : ArrestCellsPositions[LastArrestCellPositionUsed++];

            return new Vector3(pos.X, pos.Y, pos.Z);
        }

        public void SetPlayerToPrison(PlayerData pData, bool justTeleport)
        {
            if (!justTeleport)
            {
                Utils.RemoveAllWeapons(pData, true, true);

                pData.Uncuff();
            }

            var pos = GetNextArrestCellPosition();

            pData.Player.Teleport(pos, false, Properties.Settings.Static.MainDimension, null, false);
        }

        public void SetPlayerFromPrison(PlayerData pData)
        {
            var pos = new Utils.Vector4(ArrestFreePosition.X, ArrestFreePosition.Y, ArrestFreePosition.Z, ArrestFreePosition.RotationZ);

            pData.Player.Teleport(pos.Position, false, Properties.Settings.Static.MainDimension, pos.RotationZ, false);
        }

        public static CallInfo GetCallByCaller(ushort rid) => Calls.GetValueOrDefault(rid);

        public static GPSTrackerInfo GetGPSTrackerById(uint id) => GPSTrackers.GetValueOrDefault(id);

        public static uint AddGPSTracker(GPSTrackerInfo info)
        {
            var id = GPSTrackerUidHandler.MoveNextUid();

            GPSTrackers.Add(id, info);

            Player[] membersToTrigger;

            if (info.FractionType == Types.None)
            {
                var listAll = new List<Player>();

                foreach (var x in Fraction.All.Values.Where(x => x is Police).ToList())
                    listAll.AddRange(x.AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player));

                membersToTrigger = listAll.ToArray();
            }
            else
            {
                membersToTrigger = Fraction.Get(info.FractionType).AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player).ToArray();
            }

            if (membersToTrigger.Length > 0)
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(membersToTrigger, "FPolice::GPSTC", id, info.InstallerStr, info.VehicleStr);
            }

            return id;
        }

        public static void RemoveGPSTracker(uint id, GPSTrackerInfo info)
        {
            if (GPSTrackers.Remove(id))
            {
                GPSTrackerUidHandler.SetUidAsFree(id);

                Player[] membersToTrigger;

                if (info.FractionType == Types.None)
                {
                    var listAll = new List<Player>();

                    foreach (var x in Fraction.All.Values.Where(x => x is Police).ToList())
                        listAll.AddRange(x.AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player));

                    membersToTrigger = listAll.ToArray();
                }
                else
                {
                    membersToTrigger = Fraction.Get(info.FractionType).AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player).ToArray();
                }

                if (membersToTrigger.Length > 0)
                {
                    NAPI.ClientEvent.TriggerClientEventToPlayers(membersToTrigger, "FPolice::GPSTC", id);
                }
            }
        }

        public static void RemoveNotification(ushort id, NotificationInfo info)
        {
            if (Notifications.Remove(id))
            {
                Player[] membersToTrigger;

                if (info.FractionType == Types.None)
                {
                    var listAll = new List<Player>();

                    foreach (var x in Fraction.All.Values.Where(x => x is Police).ToList())
                        listAll.AddRange(x.AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player));

                    membersToTrigger = listAll.ToArray();
                }
                else
                {
                    membersToTrigger = Fraction.Get(info.FractionType).AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player).ToArray();
                }

                if (membersToTrigger.Length > 0)
                {
                    NAPI.ClientEvent.TriggerClientEventToPlayers(membersToTrigger, "FPolice::NOTIFC", id);
                }
            }
        }

        public static void AddNotificationTracker(NotificationInfo info)
        {
            var id = NotificationUidHandler.MoveNextUid();

            Notifications.Add(id, info);

            Player[] membersToTrigger;

            if (info.FractionType == Types.None)
            {
                var listAll = new List<Player>();

                foreach (var x in Fraction.All.Values.Where(x => x is Police).ToList())
                    listAll.AddRange(x.AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player));

                membersToTrigger = listAll.ToArray();
            }
            else
            {
                membersToTrigger = Fraction.Get(info.FractionType).AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player).ToArray();
            }

            if (membersToTrigger.Length > 0)
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(membersToTrigger, "FPolice::NOTIFC", id, info.Text, info.Position);
            }
        }

        public static void AddCall(ushort rid, CallInfo cInfo)
        {
            Calls.Add(rid, cInfo);

            Player[] membersToTrigger;

            if (cInfo.FractionType == Types.None)
            {
                var listAll = new List<Player>();

                foreach (var x in Fraction.All.Values.Where(x => x is Police).ToList())
                    listAll.AddRange(x.AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player));

                membersToTrigger = listAll.ToArray();
            }
            else
            {
                membersToTrigger = Fraction.Get(cInfo.FractionType).AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player).ToArray();
            }

            if (membersToTrigger.Length > 0)
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(membersToTrigger, "FPolice::CC", rid, cInfo.Type, cInfo.Message, cInfo.Position);
            }
        }

        public static void RemoveCall(ushort rid, CallInfo cInfo, byte reason, PlayerData initData)
        {
            if (Calls.Remove(rid))
            {
                Player[] membersToTrigger;

                if (cInfo.FractionType == Types.None)
                {
                    var listAll = new List<Player>();

                    foreach (var x in Fraction.All.Values.Where(x => x is Police).ToList())
                        listAll.AddRange(x.AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player));

                    membersToTrigger = listAll.ToArray();
                }
                else
                {
                    membersToTrigger = Fraction.Get(cInfo.FractionType).AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player).ToArray();
                }

                if (membersToTrigger.Length > 0)
                {
                    NAPI.ClientEvent.TriggerClientEventToPlayers(membersToTrigger, "FPolice::CC", rid);
                }

                var player = Utils.GetPlayerByID(rid);

                if (player != null)
                {
                    player.TriggerEvent("PoliceCall::Cancel", reason);
                }
            }
        }

        public static void AddAPBOnLoad(uint id, APBInfo apbInfo)
        {
            APBs.Add(id, apbInfo);

            APBUidHandler.TryUpdateLastAddedMaxUid(id);
        }

        public static void RemoveAPBOnLoad(uint id)
        {
            APBs.Remove(id);

            APBUidHandler.SetUidAsFree(id);
        }

        public static APBInfo GetAPB(uint id) => APBs.GetValueOrDefault(id);

        public static void RemoveAPB(uint id)
        {
            if (APBs.Remove(id))
            {
                var listAll = new List<Player>();

                foreach (var x in Fraction.All.Values.Where(x => x is Police).ToList())
                    listAll.AddRange(x.AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player));

                Player[] membersToTrigger = listAll.ToArray();

                if (membersToTrigger.Length > 0)
                {
                    NAPI.ClientEvent.TriggerClientEventToPlayers(membersToTrigger, "FPolice::APBC", id);
                }

                APBUidHandler.SetUidAsFree(id);

                MySQL.PoliceAPBDelete(id);
            }
        }

        public static void AddAPB(APBInfo apbInfo)
        {
            var id = APBUidHandler.MoveNextUid();

            APBs.Add(id, apbInfo);

            var listAll = new List<Player>();

            foreach (var x in Fraction.All.Values.Where(x => x is Police).ToList())
                listAll.AddRange(x.AllMembers.Where(x => x.PlayerData != null).Select(x => x.PlayerData.Player));

            Player[] membersToTrigger = listAll.ToArray();

            if (membersToTrigger.Length > 0)
            {
                NAPI.ClientEvent.TriggerClientEventToPlayers(membersToTrigger, "FPolice::APBC", id, apbInfo.TargetName, apbInfo.Member, apbInfo.Details);
            }

            MySQL.PoliceAPBAdd(id, apbInfo);
        }

        protected override void FractionDataTriggerEvent(PlayerData pData)
        {
            var callsObj = Calls.Where(x => x.Value.FractionType == Types.None || x.Value.FractionType == Type).Select(x => $"{x.Value.Type}_{x.Key}_{x.Value.Message}_{x.Value.Time.GetUnixTimestamp()}_{x.Value.Position.X}_{x.Value.Position.Y}_{x.Value.Position.Z}").ToList();
            var finesObj = Fines.Select(x => $"{x.Member}_{x.Target}_{x.Time.GetUnixTimestamp()}_{x.Amount}_{x.Reason}").ToList();
            var apbsObj = APBs.Select(x => $"{x.Key}_{x.Value.Time.GetUnixTimestamp()}_{x.Value.TargetName}_{x.Value.Member}_{x.Value.Details}").ToList();
            var notificationsObj = Notifications.Select(x => $"{x.Key}_{x.Value.Time.GetUnixTimestamp()}_{x.Value.Text}_{(x.Value.Position == null ? string.Empty : $"{x.Value.Position.X}_{x.Value.Position.Y}_{x.Value.Position.Z}")}").ToList();
            var gpsTrackersObj = GPSTrackers.Where(x => x.Value.FractionType == Types.None || x.Value.FractionType == Type).Select(x => $"{x.Key}_{x.Value.InstallerStr}_{x.Value.VehicleStr}").ToList();

            var arrestsObj = ActiveArrests.Select(x => $"{x.Key}_{x.Value.TargetName}_{x.Value.MemberName}_{x.Value.PunishmentData.StartDate.GetUnixTimestamp()}").ToList();

            var arrestsAmount = GetPlayerArrestAmount(pData.Info);

            pData.Player.TriggerEvent("Player::SCF", (int)Type, News.SerializeToJson(), AllVehicles.Select(x => $"{x.Key.VID}&{x.Key.VID}&{x.Value.MinimalRank}").ToList(), AllMembers.Select(x => $"{x.CID}&{x.Name} {x.Surname}&{x.FractionRank}&{(x.IsOnline ? 1 : 0)}&{GetMemberStatus(x)}&{x.LastJoinDate.GetUnixTimestamp()}").ToList(), callsObj, finesObj, apbsObj, notificationsObj, gpsTrackersObj, arrestsObj, arrestsAmount);
        }

        public static ushort GetPlayerArrestAmount(PlayerData.PlayerInfo pInfo) => pInfo.GetTempData<ushort>("Police::Arrests", 0);
        public static void SetPlayerArrestAmount(PlayerData.PlayerInfo pInfo, ushort amount) => pInfo.SetTempData("Police::Arrests", amount);
        public static bool ResetPlayerArrestAmount(PlayerData.PlayerInfo pInfo) => pInfo.ResetTempData("Police::Arrests");

        public override void PostInitialize()
        {
            base.PostInitialize();

            var allPunishments = PlayerData.PlayerInfo.All.Values.ToDictionary(x => x, x => x.Punishments.Where(x => x.Type == Sync.Punishment.Types.Arrest && x.IsActive())).ToList();

            foreach (var y in allPunishments)
            {
                foreach (var x in y.Value)
                {
                    var dataS = x.AdditionalData?.Split('_');

                    if (dataS == null)
                        continue;

                    var fractionType = (Types)int.Parse(dataS[1]);

                    if (fractionType != Type)
                        continue;

                    var memberInfo = PlayerData.PlayerInfo.Get(x.PunisherID);
                    var targetInfo = y.Key;

                    ActiveArrests.Add(x.Id, new ArrestInfo() { TargetCID = y.Key.CID, PunishmentData = x, MemberName = memberInfo == null ? "null" : $"{memberInfo.Name} {memberInfo.Surname}", TargetName = $"{targetInfo.Name} {targetInfo.Surname}" });
                }
            }
        }

        public void AddFine(PlayerData pData, PlayerData tData, FineInfo fine)
        {
            Fines.Add(fine);

            TriggerEventToMembers("FPolice::FINEC", fine.Member, fine.Target, fine.Amount, fine.Reason, fine.Time.GetUnixTimestamp());
        }

        public void AddActiveArrest(ArrestInfo arrestInfo)
        {
            ActiveArrests.Add(arrestInfo.PunishmentData.Id, arrestInfo);

            TriggerEventToMembers("FPolice::ARRESTSC", arrestInfo.PunishmentData.Id, arrestInfo.MemberName, arrestInfo.TargetName, arrestInfo.PunishmentData.StartDate.GetUnixTimestamp());
        }

        public bool RemoveActiveArrest(uint id)
        {
            if (ActiveArrests.Remove(id))
            {
                TriggerEventToMembers("FPolice::ARRESTSC", id);

                return true;
            }

            return false;
        }

        public Vector3 GetArrestMenuPosition(byte idx) => idx >= ArrestMenuPositions.Length ? null : ArrestMenuPositions[idx];

        public ArrestInfo GetArrestInfoById(uint id) => ActiveArrests.GetValueOrDefault(id);

        public static bool IsItemConfiscatable(Game.Items.Item item)
        {
            if (item is Game.Items.Weapon || item is Game.Items.Ammo || item is Game.Items.Armour)
                return true;

            return false;
        }

        public class CallInfo
        {
            public byte Type { get; set; }

            public Vector3 Position { get; set; }

            public string Message { get; set; }

            public DateTime Time { get; set; }

            public Types FractionType { get; set; }

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
            [JsonProperty(PropertyName = "TN")]
            public string TargetName { get; set; }

            [JsonProperty(PropertyName = "D")]
            public string Details { get; set; }

            [JsonProperty(PropertyName = "LD")]
            public string LargeDetails { get; set; }

            [JsonProperty(PropertyName = "M")]
            public string Member { get; set; }

            [JsonProperty(PropertyName = "F")]
            public Types FractionType { get; set; }

            [JsonProperty(PropertyName = "T")]
            public DateTime Time { get; set; }

            public APBInfo()
            {

            }
        }

        public class NotificationInfo
        {
            public string Text { get; set; }

            public Vector3 Position { get; set; }

            public DateTime Time { get; set; }

            public Types FractionType { get; set; }

            public NotificationInfo()
            {

            }
        }

        public class GPSTrackerInfo
        {
            public uint VID { get; set; }

            public string VehicleStr { get; set; }

            public string InstallerStr { get; set; }

            public Types FractionType { get; set; }

            public GPSTrackerInfo()
            {

            }
        }

        public class ArrestInfo
        {
            public string TargetName { get; set; }

            public string MemberName { get; set; }

            public Sync.Punishment PunishmentData { get; set; }

            public uint TargetCID { get; set; }

            public ArrestInfo()
            {

            }
        }
    }
}
