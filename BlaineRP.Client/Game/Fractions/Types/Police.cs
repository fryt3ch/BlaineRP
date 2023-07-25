using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BlaineRP.Client.Extensions.RAGE.Elements;
using BlaineRP.Client.Extensions.RAGE.Ui;
using BlaineRP.Client.Extensions.System;
using BlaineRP.Client.Game.EntitiesData;
using BlaineRP.Client.Game.EntitiesData.Components;
using BlaineRP.Client.Game.EntitiesData.Enums;
using BlaineRP.Client.Game.Fractions.Enums;
using BlaineRP.Client.Game.Management.Attachments.Enums;
using BlaineRP.Client.Game.Misc;
using BlaineRP.Client.Game.UI.CEF;
using BlaineRP.Client.Game.Wrappers;
using BlaineRP.Client.Game.Wrappers.Colshapes;
using BlaineRP.Client.Game.Wrappers.Colshapes.Enums;
using BlaineRP.Client.Game.Wrappers.Colshapes.Types;
using Newtonsoft.Json.Linq;
using RAGE;
using RAGE.Elements;
using Interaction = BlaineRP.Client.Game.Misc.Interaction;
using Players = BlaineRP.Client.Utils.Game.Players;

namespace BlaineRP.Client.Game.Fractions.Types
{
    public class Police : Fraction, IUniformable
    {
        public Police(FractionTypes type, string name, uint storageContainerId, string containerPos, string cWbPos, byte maxRank, string lockerRoomPositionsStr, string creationWorkbenchPricesJs, uint metaFlags, string arrestCellsPositionsJs, string arrestMenuPositionsStr) : base(type, name, storageContainerId, containerPos, cWbPos, maxRank, RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(creationWorkbenchPricesJs), metaFlags)
        {
            var lockerPoses = RAGE.Util.Json.Deserialize<Vector3[]>(lockerRoomPositionsStr);

            for (int i = 0; i < lockerPoses.Length; i++)
            {
                var pos = lockerPoses[i];

                var lockerRoomCs = new Cylinder(pos, 1f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = InteractionTypes.FractionLockerRoomInteract,

                    ActionType = ActionTypes.FractionInteract,

                    Data = $"{(int)type}_{i}",
                };

                var lockerRoomText = new ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Раздевалка", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };
            }

            var arrestMenuPoses = RAGE.Util.Json.Deserialize<Vector3[]>(arrestMenuPositionsStr);

            for (int i = 0; i < arrestMenuPoses.Length; i++)
            {
                var pos = arrestMenuPoses[i];

                pos.Z -= 1f;

                var arrestMenuCs = new Cylinder(pos, 1f, 2.5f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null)
                {
                    InteractionType = InteractionTypes.FractionPoliceArrestMenuInteract,

                    ActionType = ActionTypes.FractionInteract,

                    Data = $"{(int)type}_{i}",
                };

                var arrestMenuText = new ExtraLabel(new Vector3(pos.X, pos.Y, pos.Z + 1f), "Управление задержанными\nв СИЗО", new RGBA(255, 255, 255, 255), 5f, 0, false, Settings.App.Static.MainDimension)
                {
                    Font = 0,
                };
            }

            if (type == FractionTypes.COP_BLAINE)
            {
                UniformNames = new string[]
                {
                    "Стандартная форма",
                    "Форма для специальных операций",
                    "Форма руководства",
                };
            }

            this.ArrestCellsPositions = RAGE.Util.Json.Deserialize<Vector3[]>(arrestCellsPositionsJs);
        }

        public static Dictionary<string, uint[]> NumberplatePrices { get; set; }

        public Vector3[] ArrestCellsPositions { get; set; }

        public string[] UniformNames { get; set; }

        public override void OnStartMembership(params object[] args)
        {
            base.OnStartMembership(args);

            SetCurrentData("Calls", ((JArray)args[3]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new CallInfo() { Message = d[2], Type = byte.Parse(d[0]), Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[3])).DateTime, Position = new Vector3(float.Parse(d[4]), float.Parse(d[5]), float.Parse(d[6])), Player = RAGE.Elements.Entities.Players.GetAtRemote(ushort.Parse(d[1])) }; }).ToList());

            SetCurrentData("Fines", ((JArray)args[4]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new FineInfo() { Amount = uint.Parse(d[3]), Reason = d[4], Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[2])).DateTime, Member = d[0], Target = d[1] }; }).ToList());

            SetCurrentData("APBs", ((JArray)args[5]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new APBInfo() { Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[1])).DateTime, TargetName = d[2], Member = d[3], Details = d[4], Id = uint.Parse(d[0]) }; }).ToList());

            SetCurrentData("Notifications", ((JArray)args[6]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new NotificationInfo() { Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[1])).DateTime, Id = ushort.Parse(d[0]), Text = d[2], Position = d.Length > 4 ? new Vector3(float.Parse(d[3]), float.Parse(d[4]), float.Parse(d[5])) : null }; }).ToList());

            SetCurrentData("GPSTrackers", ((JArray)args[7]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new GPSTrackerInfo() { Id = uint.Parse(d[0]), InstallerStr = d[1], VehicleStr = d[2] }; }).ToList());

            SetCurrentData("Arrests", ((JArray)args[8]).ToObject<List<string>>().Select(x => { var d = x.Split('_'); return new ArrestInfo() { Id = uint.Parse(d[0]), TargetName = d[1], MemberName = d[2], Time = DateTimeOffset.FromUnixTimeSeconds(long.Parse(d[3])).DateTime, }; }).ToList());

            HUD.Menu.UpdateCurrentTypes(true, HUD.Menu.Types.Fraction_Police_TabletPC);

            Input.Core.CurrentExtraAction0 = () =>
            {
                if (Interaction.CurrentEntity is Player player)
                {
                    PlayerCuff(player, null);
                }
                else if (Interaction.CurrentEntity is Vehicle vehicle)
                {
                    if (Browser.IsAnyCEFActive)
                        return;

                    PlayerToVehicle(vehicle);
                }
            };

            Input.Core.CurrentExtraAction1 = () =>
            {
                if (Interaction.CurrentEntity is Player player)
                {
                    PlayerEscort(player, null);
                }
                else if (Interaction.CurrentEntity is Vehicle vehicle)
                {
                    if (Browser.IsAnyCEFActive)
                        return;

                    PlayerFromVehicle(vehicle);
                }
            };

            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("documents", 0, "fraction_docs");

            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 10, "fraction_invite");
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 11, "police_search");
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 12, "cuffs");
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 13, "uncuff_any");
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 14, "police_escort");
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 15, "prison");
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 0, "fine");
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 1, "take_license");
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 2, "mask_off");

            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 16, "gps_tracker");
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 17, "police_search");
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 18, "player_to_veh");
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 19, "player_from_veh");

            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.AddAction("job", "gps_tracker", (entity) => { var veh = entity as Vehicle; if (veh == null) return; GPSTrackerVehicleInstall(veh); });
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.AddAction("job", "player_to_veh", (entity) => { var veh = entity as Vehicle; if (veh == null) return; PlayerToVehicle(veh); });
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.AddAction("job", "player_from_veh", (entity) => { var veh = entity as Vehicle; if (veh == null) return; PlayerFromVehicle(veh); });
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.AddAction("job", "police_search", (entity) => { var veh = entity as Vehicle; if (veh == null) return; VehicleSearch(veh, null); });

            Game.UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "fine", (entity) => { var player = entity as Player; if (player == null) return; PlayerFine(player); });
            Game.UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "take_license", (entity) => { var player = entity as Player; if (player == null) return; PlayerRemoveLicense(player); });
            Game.UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "cuffs", (entity) => { var player = entity as Player; if (player == null) return; PlayerCuff(player, true); });
            Game.UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "uncuff_any", (entity) => { var player = entity as Player; if (player == null) return; PlayerCuff(player, false); });
            Game.UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "police_escort", (entity) => { var player = entity as Player; if (player == null) return; PlayerEscort(player, null); });
            Game.UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "prison", (entity) => { var player = entity as Player; if (player == null) return; PlayerArrest(player); });
            Game.UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "police_search", (entity) => { var player = entity as Player; if (player == null) return; PlayerSearch(player, null); });
            Game.UI.CEF.Interaction.CharacterInteractionInfo.AddAction("char_job", "mask_off", (entity) => { var player = entity as Player; if (player == null) return; PlayerMaskOff(player); });

            var arrestCs = new List<ExtraColshape>();

            if (Type == FractionTypes.COP_BLAINE)
            {
                arrestCs.Add(new Cuboid(new Vector3(-430.256775f, 5997.575f, 32.45621f), 8.5f, 10f, 3.7f, 135f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null) { Data = FractionTypes.COP_BLAINE });
            }
            else if (Type == FractionTypes.COP_LS)
            {
                arrestCs.Add(new Cuboid(new Vector3(472.494965f, -998.1451f, 25.3779182f), 21f, 11f, 3.5f, 0f, false, Utils.Misc.RedColor, Settings.App.Static.MainDimension, null) { Data = FractionTypes.COP_LS });
            }

            foreach (var x in arrestCs)
            {
                x.Name = "FRAC_COP_ARREST_CS";

                x.OnEnter = (cancel) =>
                {
                    //Utils.ConsoleOutput("CAN ARREST");

                    if (x.Data is FractionTypes fType)
                        Player.LocalPlayer.SetData("PoliceArrestFType", fType);
                };

                x.OnExit = (cancel) =>
                {
                    Player.LocalPlayer.ResetData("PoliceArrestFType");

                    //Utils.ConsoleOutput("CAN NOT ARREST");
                };
            }
        }

        public override void OnEndMembership()
        {
            HUD.Menu.UpdateCurrentTypes(false, HUD.Menu.Types.Fraction_Police_TabletPC);

            Input.Core.CurrentExtraAction0 = null;
            Input.Core.CurrentExtraAction1 = null;

            ExtraColshape.All.Where(x => x.Name == "FRAC_COP_ARREST_CS").ToList().ForEach(x => x.Destroy());

            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("documents", 0, null);

            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 10, null);
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 11, null);
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 12, null);
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 13, null);
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 14, null);
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 15, null);
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 0, null);
            Game.UI.CEF.Interaction.CharacterInteractionInfo.ReplaceExtraLabel("char_job", 1, null);

            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 16, null);
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 17, null);
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 18, null);
            Game.UI.CEF.Interaction.OutVehicleInteractionInfo.ReplaceExtraLabel("job", 19, null);

            PoliceTabletPC.Close();
            ArrestsMenu.Close();

            base.OnEndMembership();
        }

        public async static void ShowPoliceTabletPc()
        {
            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var fData = pData.CurrentFraction;

            if (fData == null)
                return;

            var pName = Player.LocalPlayer.Name;

            var finesAmount = (uint)(fData.GetCurrentData<List<FineInfo>>("Fines")?.Where(x => x.Member.StartsWith(pName)).Count() ?? 0);

            var res = ((string)await Events.CallRemoteProc("Police::TPCS"))?.Split('_');

            if (res == null)
                return;

            var isOnDuty = res[0] == "1";
            var arrestsAmount = uint.Parse(res[1]);

            await PoliceTabletPC.Show(fData, isOnDuty, AllMembers.GetValueOrDefault(pData.CID)?.Rank ?? 0, finesAmount, arrestsAmount);
        }

        public async void PlayerCuff(Player player, bool? state)
        {
            if (player?.Exists != true)
                return;

            var tData = PlayerData.GetData(player);

            if (tData == null)
                return;

            var cuffAttach = tData.AttachedObjects?.Where(x => x.Type == AttachmentTypes.Cuffs || x.Type == AttachmentTypes.CableCuffs).FirstOrDefault();

            var cuffState = state == null ? cuffAttach == null : (bool)state;

            if (cuffState)
            {
                if (cuffAttach != null)
                {
                    if (cuffAttach.Type == AttachmentTypes.Cuffs)
                    {
                        Notification.ShowError(Locale.Get("POLICE_CUFFS_E_0"), -1);

                        return;
                    }
                    else
                    {
                        Notification.ShowError(Locale.Get("POLICE_CUFFS_E_2"), -1);

                        return;
                    }
                }
            }
            else
            {
                if (cuffAttach == null || cuffAttach.Type != AttachmentTypes.Cuffs)
                {
                    Notification.ShowError(Locale.Get("POLICE_CUFFS_E_1"), -1);

                    return;
                }
            }

            var lastSent = GetCurrentData<DateTime>("LastCuffed");

            if (lastSent.IsSpam(500, false, true))
                return;

            SetCurrentData("LastCuffed", Game.World.Core.ServerTime);

            var res = (int)await Events.CallRemoteProc("Police::Cuff", player, cuffState);

            if (res == byte.MaxValue)
            {
                if (cuffState)
                {
                    Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICE_CUFFS_N_0", Players.GetPlayerName(player, true, false, true)));
                }
                else
                {
                    Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICE_CUFFS_N_1", Players.GetPlayerName(player, true, false, true)));
                }
            }
        }

        public async void PlayerEscort(Player player, bool? state)
        {
            if (player?.Exists != true)
                return;

            var tData = PlayerData.GetData(player);

            if (tData == null)
                return;

            var escortAttach = tData.AttachedEntities?.Where(x => x.Type == AttachmentTypes.PoliceEscort).FirstOrDefault();

            var escortState = state == null ? escortAttach == null : (bool)state;

            if (escortState)
            {
                if (!tData.IsCuffed)
                {
                    Notification.ShowError(Locale.Get("POLICE_ESCORT_E_0"), -1);

                    return;
                }

                if (escortAttach != null)
                {
                    if (tData.IsAttachedTo != Player.LocalPlayer)
                    {
                        Notification.ShowError(Locale.Get("POLICE_ESCORT_E_2"), -1);
                    }
                    else
                    {
                        Notification.ShowError(Locale.Get("POLICE_ESCORT_E_3"), -1);
                    }

                    return;
                }

                if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed, PlayerActions.Types.OtherAnimation, PlayerActions.Types.Animation, PlayerActions.Types.Scenario, PlayerActions.Types.FastAnimation, PlayerActions.Types.InVehicle, PlayerActions.Types.Shooting, PlayerActions.Types.Reloading, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.NotOnFoot, PlayerActions.Types.IsSwimming, PlayerActions.Types.HasItemInHands, PlayerActions.Types.IsAttachedTo))
                    return;
            }
            else
            {
                if (escortAttach == null || escortAttach.Type != AttachmentTypes.PoliceEscort || tData.IsAttachedTo != Player.LocalPlayer)
                {
                    Notification.ShowError(Locale.Get("POLICE_ESCORT_E_1"), -1);

                    return;
                }
            }

            var lastSent = GetCurrentData<DateTime>("LastEscorted");

            if (lastSent.IsSpam(500, false, true))
                return;

            SetCurrentData("LastEscorted", Game.World.Core.ServerTime);

            var res = (int)await Events.CallRemoteProc("Police::Escort", player, escortState);

            if (res == byte.MaxValue)
            {
                /*                if (escortState)
                                {
                                    CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICE_CUFFS_N_0", Utils.GetPlayerName(player, true, false, true)));
                                }
                                else
                                {
                                    CEF.Notification.Show(CEF.Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICE_CUFFS_N_1", Utils.GetPlayerName(player, true, false, true)));
                                }*/
            }
        }

        public async void PlayerArrest(Player player)
        {
            var tData = PlayerData.GetData(player);

            if (tData == null)
                return;

            if (!tData.IsCuffed)
            {
                Notification.ShowError(Locale.Get("POLICE_ESCORT_E_0"), -1);

                return;
            }

            var arrestFType = Player.LocalPlayer.GetData<FractionTypes>("PoliceArrestFType");

            var fData = Fraction.Get(arrestFType);

            if (fData == null)
            {
                Notification.ShowError(Locale.Get("POLICE_ARREST_E_0"), -1);

                return;
            }

            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed))
                return;

            await Documents.ShowPoliceBlank(true, $"{fData.Name}", $"{player.Name}", $"{Player.LocalPlayer.Name}", Game.World.Core.ServerTime.ToString("dd.MM.yyyy HH:mm"), new string[] { "", "", "", Locale.Get(fData is Prison ? "POLICE_ARREST_TIME_L_1" : "POLICE_ARREST_TIME_L_0") }, async (args) =>
            {
                var rType = (int)args[0];

                if (rType == 0)
                {
                    var reason1Str = ((string)args[1])?.Trim();
                    var timeStr = ((string)args[2])?.Trim();
                    var reason2Str = ((string)args[3])?.Trim();

                    arrestFType = Player.LocalPlayer.GetData<FractionTypes>("PoliceArrestFType");

                    fData = Fraction.Get(arrestFType);

                    if (player?.Exists != true || player.Position.DistanceTo(Player.LocalPlayer.Position) > 5f || !tData.IsCuffed || fData == null)
                    {
                        Documents.Close();

                        return;
                    }

                    if (!(new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{1,18}$")).IsMatch(reason1Str))
                    {
                        Notification.ShowError(Locale.Get("POLICE_ARREST_E_1"), -1);

                        return;
                    }

                    if (!(new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{0,100}$")).IsMatch(reason2Str))
                    {
                        Notification.ShowError(Locale.Get("POLICE_ARREST_E_2"), -1);

                        return;
                    }

                    ulong time;

                    if (!ulong.TryParse(timeStr, out time))
                    {
                        Notification.ShowError(Locale.Get("POLICE_ARREST_E_3"), -1);

                        return;
                    }

                    if (!((decimal)time).IsNumberValid<decimal>(1, short.MaxValue, out _, true))
                        return;

                    var res = await Events.CallRemoteProc("Police::Arrest", player, (int)arrestFType, time, reason1Str, reason2Str);

                    Documents.Close();
                }
                else if (rType == 1)
                {
                    Documents.Close();
                }
                else
                {
                    if (rType == 77)
                    {
                        Notification.Show(Notification.Types.Error, Locale.Get("POLICE_ARREST_E_5"), Locale.Get("POLICE_ARREST_E_3"), -1);
                    }

                    return;
                }
            });
        }

        public async void GPSTrackerVehicleInstall(Vehicle vehicle)
        {
            var gpsTrackerItemId = "mis_gpstr";

            int itemIdx = -1;

            for (int i = 0; i < Inventory.ItemsParams.Length; i++)
            {
                var x = Inventory.ItemsParams[i];

                if (x == null)
                    continue;

                if (x.Id == gpsTrackerItemId)
                {
                    itemIdx = i;

                    break;
                }
            }

            if (itemIdx < 0)
            {
                Notification.Show("Inventory::NoItem");

                return;
            }

            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed, PlayerActions.Types.OtherAnimation, PlayerActions.Types.Animation, PlayerActions.Types.Scenario, PlayerActions.Types.FastAnimation, PlayerActions.Types.Shooting, PlayerActions.Types.Reloading, PlayerActions.Types.Climbing, PlayerActions.Types.Falling, PlayerActions.Types.Ragdoll, PlayerActions.Types.Jumping, PlayerActions.Types.NotOnFoot, PlayerActions.Types.IsSwimming, PlayerActions.Types.HasItemInHands, PlayerActions.Types.IsAttachedTo))
                return;

            await ActionBox.ShowSelect("PoliceGPSTrackerDepSelect", Locale.Get("POLICE_GPSTR_0"), new (decimal Id, string Text)[] { (0, Locale.Get("POLICE_GPSTR_1")), (1, Locale.Get("POLICE_GPSTR_2")) }, null, null, ActionBox.DefaultBindAction, async (rType, id) =>
            {
                if (rType == ActionBox.ReplyTypes.Cancel || vehicle?.Exists != true || vehicle.Position.DistanceTo(Player.LocalPlayer.Position) > 10f || Inventory.ItemsParams[itemIdx]?.Id != gpsTrackerItemId)
                {
                    ActionBox.Close(true);

                    return;
                }

                var allDepsSee = id == 1;

                if (ActionBox.LastSent.IsSpam(500, false, true))
                    return;

                ActionBox.LastSent = Game.World.Core.ServerTime;

                var res = await Events.CallRemoteProc("Police::GPSTRI", vehicle, itemIdx, allDepsSee);

                if (res != null)
                {
                    var resId = Utils.Convert.ToDecimal(res);

                    ActionBox.Close(true);

                    Notification.Show(Notification.Types.Success, Locale.Get("NOTIFICATION_HEADER_DEF"), Locale.Get("POLICE_GPSTR_3", resId), 5_000);
                }
            }, null);
        }

        public async void PlayerFine(Player player)
        {
            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed))
                return;

            await ActionBox.ShowInputWithText("PolicePlayerFine", "Выписать штраф", "Введите сумму, на которую вы хотите оштрафовать гражданина и причину.\n\nПример: 500, Езда по встречной полосе", 100, "", null, null, ActionBox.DefaultBindAction, (rType, str) =>
            {
                if (rType == ActionBox.ReplyTypes.Cancel)
                {
                    ActionBox.Close(true);

                    return;
                }

                var strD = str?.Trim()?.Split(',');

                int fineSum;

                if (strD.Length < 2 || !int.TryParse(strD[0], out fineSum))
                {
                    Notification.ShowError(Locale.Get("ARRESTMENU_E_0"));

                    return;
                }

                if (!((decimal)fineSum).IsNumberValid<decimal>(100, 100_000, out _, true))
                {
                    return;
                }

                var reason = string.Join(',', strD.Skip(1)).Trim();

                if (!(new Regex(@"^[0-9a-zA-Zа-яА-Я\-\s,()!.?:+]{1,18}$").IsMatch(reason)))
                {
                    Notification.ShowError(Locale.Get("ARRESTMENU_E_2"));

                    return;
                }

                ActionBox.Close(true);

                Offers.Request(player, OfferTypes.PoliceFine, new { Amount = fineSum, Reason = reason, });

            }, null);
        }

        public async void PlayerRemoveLicense(Player player, List<LicenseTypes> licenses = null)
        {
            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed))
                return;

            if (licenses == null)
            {
                licenses = (await Events.CallRemoteProc("Police::RmLic", player, string.Empty) as JArray)?.ToObject<List<int>>().Select(x => (LicenseTypes)x).ToList();
            }

            if (licenses == null)
                return;

            if (licenses.Count == 0)
            {
                Notification.ShowError(Locale.Get("POLICE_RMLIC_E_2"), -1);

                ActionBox.Close(true);

                return;
            }

            await ActionBox.ShowSelect("PolicePlayerRmLic", "Лишить лицензии", licenses.Select(x => ((decimal)x, Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(x.GetType(), x.ToString(), "NAME_0") ?? "null"))).ToArray(), null, null, ActionBox.DefaultBindAction, async (rType, id) =>
            {
                if (rType == ActionBox.ReplyTypes.Cancel)
                {
                    ActionBox.Close(true);

                    return;
                }

                var licType = (LicenseTypes)id;

                if (ActionBox.LastSent.IsSpam(500, false, true))
                    return;

                ActionBox.LastSent = Game.World.Core.ServerTime;

                var res = await Events.CallRemoteProc("Police::RmLic", player, $"{id}");

                if (res == null)
                {
                    ActionBox.Close(true);

                    return;
                }
                else if (res is int resI)
                {
                    if (resI == 255)
                    {
                        Notification.ShowError(Locale.Get("POLICE_RMLIC_S_0", player.GetName(true, false, true), Locale.Get(Language.Strings.GetKeyFromTypeByMemberName(licType.GetType(), licType.ToString(), "NAME_0") ?? "null")), -1);

                        licenses.Remove(licType);

                        PlayerRemoveLicense(player, licenses);

                        return;
                    }
                    else if (resI == 1)
                    {
                        Notification.ShowError(Locale.Get("POLICE_RMLIC_E_1"), -1);

                        licenses.Remove(licType);

                        PlayerRemoveLicense(player, licenses);

                        return;
                    }
                    else if (resI == 0)
                    {
                        Notification.ShowError(Locale.Get("POLICE_RMLIC_E_0"), -1);

                        return;
                    }
                }
            }, null);
        }

        public async void PlayerSearch(Player player, object[] args = null)
        {
            if (PlayerData.GetData(player)?.IsCuffed != true)
            {
                Notification.ShowError(Locale.Get("POLICE_ESCORT_E_0"), -1);

                return;
            }

            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed))
                return;

            var res = await Events.CallRemoteProc("Police::PlayerSearch", player, args == null ? -1 : -2);

            if (res is int resB)
            {
                return;
            }
            else if (res == null)
            {
                Notification.ShowErrorDefault();

                return;
            }

            await ActionBox.ShowSelect("PolicePlayerSearchOptSelect", Locale.Get("POLICE_PSEARCH_L_0", player.GetName(true, false, true)), ((JArray)res).ToObject<List<int>>().OrderBy(x => x).Select(x => ((decimal)x, Locale.Get($"POLICE_PSEARCH_L_O_{x}"))).ToArray(), null, null, ActionBox.DefaultBindAction, async (rType, id) =>
            {
                if (rType != ActionBox.ReplyTypes.OK)
                {
                    ActionBox.Close(true);

                    return;
                }

                var lastSent = GetCurrentData<DateTime>("PSearchLastSent");

                if (lastSent.IsSpam(1000, false, true))
                    return;

                SetCurrentData("PSearchLastSent", Game.World.Core.ServerTime);

                res = await Events.CallRemoteProc("Police::PlayerSearch", player, id);

                if (id == 0)
                {
                    if (res is int resB)
                    {
                        return;
                    }
                    else if (res == null)
                    {
                        Notification.ShowErrorDefault();

                        ActionBox.Close(true);

                        return;
                    }

                    ActionBox.Close(false);

                    await ActionBox.ShowSelect("PolicePlayerSearchDocs", Locale.Get("POLICE_PSEARCH_L_1", player.GetName(true, false, true)), ((JArray)res).ToObject<List<int>>().Select(x => ((decimal)x, Locale.Get($"POLICE_PSEARCH_L_D_{x}"))).ToArray(), Locale.Get("ACTIONBOX_BTN_DOCSLOOK_0"), Locale.Get("ACTIONBOX_BTN_BACK_0"), ActionBox.DefaultBindAction, async (rType, id) =>
                    {
                        if (rType != ActionBox.ReplyTypes.OK)
                        {
                            ActionBox.Close(false);

                            PlayerSearch(player, new object[] { });

                            return;
                        }

                        var lastSent = GetCurrentData<DateTime>("PSearchLastSent");

                        if (lastSent.IsSpam(1000, false, true))
                            return;

                        SetCurrentData("PSearchLastSent", Game.World.Core.ServerTime);

                        res = await Events.CallRemoteProc("Police::PlayerSearchSD", player, (int)id);

                        if (res is bool resB)
                        {
                            if (resB)
                            {
                                ActionBox.Close(true);
                            }
                            else
                            {
                                Notification.ShowError(Locale.Get("POLICE_PSEARCH_E_0"), -1);
                            }
                        }
                    }, null);
                }
                else if (id == 1 || id == 2 || id == 3 || id == 4)
                {
                    if (res == null)
                    {
                        Notification.ShowErrorDefault();

                        ActionBox.Close(true);

                        return;
                    }

                    if (res is int resB)
                    {
                        if (id == 1)
                        {
                            if (resB == 1)
                                Notification.ShowErrorDefault();
                        }
                        else if (id == 2)
                        {
                            if (resB == 1)
                                Notification.ShowErrorDefault();
                        }
                        else if (id == 3)
                        {
                            if (resB == 1)
                                Notification.ShowErrorDefault();
                            else if (resB == 2)
                                Notification.ShowError(Locale.Get("POLICE_PSEARCH_E_2"), -1);
                        }
                        else if (id == 4)
                        {
                            if (resB == 1)
                                Notification.ShowErrorDefault();
                            else if (resB == 2)
                                Notification.ShowError(Locale.Get("POLICE_PSEARCH_E_3"), -1);
                        }

                        return;
                    }

                    var items = ((JArray)res).ToObject<List<string>>().Select(x => { var d = x.Split('^'); return (decimal.Parse(d[0]), d[1], int.Parse(d[2]), d[3]); }).ToList();

                    if (items.Count == 0)
                    {
                        Notification.ShowError(Locale.Get("POLICE_PSEARCH_E_1"), -1);

                        return;
                    }

                    ActionBox.Close(false);

                    async void showSelectItemToConfiscate()
                    {
                        await ActionBox.ShowSelect("PolicePlayerSearchItems", Locale.Get("POLICE_PSEARCH_L_2", player.GetName(true, false, true)), items.Select(x => { var iType = Client.Data.Items.GetType(x.Item2, true); return (x.Item1, Client.Data.Items.GetNameWithTag(x.Item2, iType, x.Item4, out _) + $" x{x.Item3}"); }).ToArray(), Locale.Get("ACTIONBOX_BTN_CONFISCATE_0"), Locale.Get("ACTIONBOX_BTN_BACK_0"), ActionBox.DefaultBindAction, async (rType, itemUid) =>
                        {
                            if (rType != ActionBox.ReplyTypes.OK)
                            {
                                ActionBox.Close(false);

                                PlayerSearch(player, new object[] { });

                                return;
                            }

                            res = await Events.CallRemoteProc("Police::PlayerSearchIC", player, id, (uint)itemUid);

                            if (res is int resB)
                            {
                                if (resB == 0)
                                {
                                    Notification.ShowErrorDefault();

                                    ActionBox.Close(true);
                                }
                                else if (resB == 255)
                                {
                                    var t = items.Where(x => x.Item1 == itemUid).FirstOrDefault();

                                    items.Remove(t);

                                    ActionBox.Close(false);

                                    if (items.Count == 0)
                                    {
                                        PlayerSearch(player, new object[] { });
                                    }
                                    else
                                    {
                                        showSelectItemToConfiscate();
                                    }
                                }
                                else
                                {

                                }
                            }
                        }, null);
                    }

                    showSelectItemToConfiscate();
                }
            }, null);
        }

        public async void VehicleSearch(Vehicle vehicle, object[] args = null)
        {
            if (PlayerActions.IsAnyActionActive(true, PlayerActions.Types.Knocked, PlayerActions.Types.Frozen, PlayerActions.Types.Cuffed))
                return;

            var res = await Events.CallRemoteProc("Police::VehicleSearch", vehicle, args == null ? -1 : -2);

            if (res is int resB)
            {
                return;
            }
            else if (res == null)
            {
                Notification.ShowErrorDefault();

                return;
            }

            await ActionBox.ShowSelect("PoliceVehicleSearchOptSelect", Locale.Get("POLICE_VSEARCH_L_0", Utils.Game.Vehicles.GetVehicleName(vehicle, 1)), ((JArray)res).ToObject<List<int>>().OrderBy(x => x).Select(x => ((decimal)x, Locale.Get($"POLICE_PSEARCH_L_O_{x}"))).ToArray(), null, null, ActionBox.DefaultBindAction, async (rType, id) =>
            {
                if (rType != ActionBox.ReplyTypes.OK)
                {
                    ActionBox.Close(true);

                    return;
                }

                var lastSent = GetCurrentData<DateTime>("PSearchLastSent");

                if (lastSent.IsSpam(1000, false, true))
                    return;

                SetCurrentData("PSearchLastSent", Game.World.Core.ServerTime);

                res = await Events.CallRemoteProc("Police::VehicleSearch", vehicle, id);

                if (id == 0)
                {
                    if (res == null)
                    {
                        Notification.ShowErrorDefault();

                        ActionBox.Close(true);

                        return;
                    }

                    if (res is int resB)
                    {
                        if (id == 0)
                        {
                            if (resB == 0)
                                Notification.ShowErrorDefault();
                        }

                        return;
                    }

                    var items = ((JArray)res).ToObject<List<string>>().Select(x => { var d = x.Split('^'); return (decimal.Parse(d[0]), d[1], int.Parse(d[2]), d[3]); }).ToList();

                    if (items.Count == 0)
                    {
                        Notification.ShowError(Locale.Get("POLICE_PSEARCH_E_1"), -1);

                        return;
                    }

                    ActionBox.Close(false);

                    async void showSelectItemToConfiscate()
                    {
                        await ActionBox.ShowSelect("PoliceVehicleSearchItems", Locale.Get("POLICE_VSEARCH_L_1", Utils.Game.Vehicles.GetVehicleName(vehicle, 1)), items.Select(x => { var iType = Client.Data.Items.GetType(x.Item2, true); return (x.Item1, Client.Data.Items.GetNameWithTag(x.Item2, iType, x.Item4, out _) + $" x{x.Item3}"); }).ToArray(), Locale.Get("ACTIONBOX_BTN_CONFISCATE_0"), Locale.Get("ACTIONBOX_BTN_BACK_0"), ActionBox.DefaultBindAction, async (rType, itemUid) =>
                        {
                            if (rType != ActionBox.ReplyTypes.OK)
                            {
                                ActionBox.Close(false);

                                VehicleSearch(vehicle, new object[] { });

                                return;
                            }

                            res = await Events.CallRemoteProc("Police::VehicleSearchIC", vehicle, id, (uint)itemUid);

                            if (res is int resB)
                            {
                                if (resB == 0)
                                {
                                    Notification.ShowErrorDefault();

                                    ActionBox.Close(true);
                                }
                                else if (resB == 255)
                                {
                                    var t = items.Where(x => x.Item1 == itemUid).FirstOrDefault();

                                    items.Remove(t);

                                    ActionBox.Close(false);

                                    if (items.Count == 0)
                                    {
                                        VehicleSearch(vehicle, new object[] { });
                                    }
                                    else
                                    {
                                        showSelectItemToConfiscate();
                                    }
                                }
                                else
                                {

                                }
                            }
                        }, null);
                    }

                    showSelectItemToConfiscate();
                }
            }, null);
        }

        public async void PlayerMaskOff(Player player)
        {
            var tData = PlayerData.GetData(player);

            if (tData == null)
                return;

            if (!tData.IsMasked)
            {
                Notification.ShowError(Locale.Get("POLICE_PMASKOFF_E_0"));

                return;
            }

            if (!tData.IsCuffed)
            {
                Notification.ShowError(Locale.Get("POLICE_ESCORT_E_0"));

                return;
            }

            var res = (int)await Events.CallRemoteProc("Police::PMaskOff", player);

            if (res == 255)
            {
                Notification.ShowSuccess(Locale.Get("POLICE_PMASKOFF_S_0", player.GetName(true, true, true)));

                return;
            }
            else if (res == 0)
            {
                Notification.ShowErrorDefault();

                return;
            }
        }

        public async void PlayerToVehicle(Vehicle vehicle)
        {
            var vData = Game.EntitiesData.VehicleData.GetData(vehicle);

            if (vData == null)
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var attachData = pData.AttachedEntities?.Where(x => (x.Type == AttachmentTypes.PoliceEscort || x.Type == AttachmentTypes.Hostage || x.Type == AttachmentTypes.Carry) && x.EntityType == RAGE.Elements.Type.Player).FirstOrDefault();

            if (attachData == null)
            {
                Notification.ShowError(Locale.Get("POLICE_PTOVEH_E_1"));

                return;
            }

            var freeSeats = new List<(decimal, string)>();

            for (int i = 0; i < vehicle.GetMaxNumberOfPassengers(); i++)
            {
                if (vehicle.IsSeatFree(i, 0))
                    freeSeats.Add((i + 1, Locale.Get("POLICE_PTOVEH_L_0", i + 2)));
            }

            var trunkAttach = Game.Management.Attachments.Core.GetEntityEntityAttachments(vehicle)?.Where(x => x.Type == AttachmentTypes.VehicleTrunk).FirstOrDefault();

            if (trunkAttach == null && vehicle.DoesHaveDoor(5) > 0)
                freeSeats.Add((255, Locale.Get("POLICE_PTOVEH_L_1")));

            if (freeSeats.Count == 0)
            {
                Notification.ShowError(Locale.Get("POLICE_PTOVEH_E_0"));

                return;
            }

            await ActionBox.ShowSelect("PolicePlayerToVehicleSeatSelect", Locale.Get("POLICE_PTOVEH_L_2"), freeSeats.ToArray(), null, null, ActionBox.DefaultBindAction, async (rType, id) =>
            {
                if (rType != ActionBox.ReplyTypes.OK)
                {
                    ActionBox.Close(true);

                    return;
                }

                var seatIdx = (int)id;

                var res = (int)await Events.CallRemoteProc("Police::FPTV", vehicle, (byte)seatIdx);

                if (res == 255)
                {
                    ActionBox.Close(true);

                    return;
                }
                else if (res == 0)
                {
                    Notification.ShowErrorDefault();

                    return;
                }
                else if (res == 1)
                {
                    Notification.ShowError(Locale.Get("VEHICLE_DOORS_LOCKED_E_0"));
                }
                else if (res == 2)
                {
                    Notification.ShowError(Locale.Get("VEHICLE_TRUNK_LOCKED_E_0"));
                }
            }, null);
        }

        public async void PlayerFromVehicle(Vehicle vehicle)
        {
            var vData = Game.EntitiesData.VehicleData.GetData(vehicle);

            if (vData == null)
                return;

            var pData = PlayerData.GetData(Player.LocalPlayer);

            if (pData == null)
                return;

            var players = new List<Player>();

            for (int i = 0; i < vehicle.GetMaxNumberOfPassengers(); i++)
            {
                var player = Utils.Game.Misc.GetPlayerByHandle(vehicle.GetPedInSeat(i, 0), true);

                if (player?.Exists == true && player != Player.LocalPlayer && (PlayerData.GetData(player)?.IsCuffed == true || PlayerData.GetData(player)?.IsKnocked == true))
                    players.Add(player);
            }

            var trunkAttach = Game.Management.Attachments.Core.GetEntityEntityAttachments(vehicle)?.Where(x => x.Type == AttachmentTypes.VehicleTrunk && x.EntityType == RAGE.Elements.Type.Player).FirstOrDefault();

            if (trunkAttach != null)
            {
                var player = Entities.Players.GetAtRemote(trunkAttach.RemoteID);

                if (player?.Exists == true && player != Player.LocalPlayer && (PlayerData.GetData(player)?.IsCuffed == true || PlayerData.GetData(player)?.IsKnocked == true))
                    players.Add(player);
            }

            if (players.Count == 0)
            {
                Notification.ShowError(Locale.Get("POLICE_PFROMVEH_E_0"));

                return;
            }

            await ActionBox.ShowSelect("PolicePlayerFromVehicleSeatSelect", Locale.Get("POLICE_PFROMVEH_L_0"), players.Select(x => ((decimal)players.IndexOf(x), x.GetName(true, false, true))).ToArray(), null, null, ActionBox.DefaultBindAction, async (rType, id) =>
            {
                if (rType != ActionBox.ReplyTypes.OK)
                {
                    ActionBox.Close(true);

                    return;
                }

                var player = players[(int)id];

                var res = (int)await Events.CallRemoteProc("Police::FPFV", vehicle, player);

                if (res == 255)
                {
                    ActionBox.Close(true);

                    return;
                }
                else if (res == 0)
                {
                    Notification.ShowErrorDefault();

                    return;
                }
                else if (res == 1)
                {
                    Notification.ShowError(Locale.Get("VEHICLE_DOORS_LOCKED_E_0"));
                }
                else if (res == 2)
                {
                    Notification.ShowError(Locale.Get("VEHICLE_TRUNK_LOCKED_E_0"));
                }
            }, null);
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

    [Script(int.MaxValue)]
    public class PoliceEvents
    {
        public PoliceEvents()
        {
            Events.Add("FPolice::ARRESTSC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var arrests = pData.CurrentFraction?.GetCurrentData<List<Police.ArrestInfo>>("Arrests");

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
                        {
                            Events.CallLocal("MenuArrest::Button", 0);
                        }
                    }
                }
                else
                {
                    var data = new Police.ArrestInfo() { Id = id, MemberName = (string)args[1], TargetName = (string)args[2], Time = DateTimeOffset.FromUnixTimeSeconds(Utils.Convert.ToInt64(args[3])).DateTime };

                    arrests.Add(data);

                    if (ArrestsMenu.IsActive)
                    {
                        Browser.Window.ExecuteJs("MenuArrest.addArrest", id, data.Time.ToString("dd.MM.yyyy HH:mm"), data.TargetName, data.MemberName);
                    }
                }
            });

            Events.Add("FPolice::FINEC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var fines = pData.CurrentFraction?.GetCurrentData<List<Police.FineInfo>>("Fines");

                if (fines == null)
                    return;

                var member = (string)args[0];
                var target = (string)args[1];
                var amount = Utils.Convert.ToUInt32(args[2]);
                var reason = (string)args[3];
                var time = DateTimeOffset.FromUnixTimeSeconds(Utils.Convert.ToInt64(args[4])).DateTime;

                var fine = new Police.FineInfo() { Amount = amount, Member = member, Reason = reason, Target = target, Time = time, };

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

            Events.Add("FPolice::CC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var calls = pData.CurrentFraction?.GetCurrentData<List<Police.CallInfo>>("Calls");

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
                    {
                        Browser.Window.ExecuteJs("PoliceTablet.removeElem", rid);
                    }

                    // todo
                }
                else
                {
                    var call = new Police.CallInfo() { Type = Utils.Convert.ToByte(args[1]), Time = Game.World.Core.ServerTime, Message = (string)args[2], Position = (Vector3)args[3], Player = RAGE.Elements.Entities.Players.GetAtRemote(rid) };

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

            Events.Add("FPolice::APBC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var apbs = pData.CurrentFraction?.GetCurrentData<List<Police.APBInfo>>("APBs");

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
                        if (Player.LocalPlayer.GetData<Police.APBInfo>("PoliceTablet::APBViewId")?.Id == uid)
                        {
                            PoliceTabletPC.TabBack();
                        }

                        Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);
                    }

                    // todo
                }
                else
                {
                    var apb = new Police.APBInfo() { Id = uid, Time = Game.World.Core.ServerTime, TargetName = (string)args[1], Member = (string)args[2], Details = (string)args[3] };

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

            Events.Add("FPolice::GPSTC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var trackers = pData.CurrentFraction?.GetCurrentData<List<Police.GPSTrackerInfo>>("GPSTrackers");

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
                    {
                        Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);
                    }

                    // todo
                }
                else
                {
                    var gpsTracker = new Police.GPSTrackerInfo() { Id = uid, InstallerStr = (string)args[1], VehicleStr = (string)args[2] };

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

            Events.Add("FPolice::NOTIFC", (args) =>
            {
                var pData = PlayerData.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                var notifics = pData.CurrentFraction?.GetCurrentData<List<Police.NotificationInfo>>("Notifications");

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
                    {
                        Browser.Window.ExecuteJs("PoliceTablet.removeElem", uid);
                    }

                    // todo
                }
                else
                {
                    var notific = new Police.NotificationInfo() { Id = uid, Time = Game.World.Core.ServerTime, Text = (string)args[1], Position = (Vector3)args[2] };

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
