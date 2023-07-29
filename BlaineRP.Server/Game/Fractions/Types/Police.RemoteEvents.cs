using System;
using System.Collections.Generic;
using System.Linq;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.Animations;
using BlaineRP.Server.Game.Attachments;
using BlaineRP.Server.Game.Containers;
using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.EntitiesData.Vehicles;
using BlaineRP.Server.Game.Inventory;
using BlaineRP.Server.Game.Management;
using BlaineRP.Server.Game.Management.Chat;
using BlaineRP.Server.Game.Management.Punishments;
using BlaineRP.Server.Sync;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.Fractions
{
    public partial class Police
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Police::Cuff")]
            private static byte Cuff(Player player, Player target, bool state)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                var tData = target?.GetMainData();

                if (tData == null || tData == pData)
                    return 0;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || pData.IsAttachedToEntity != null || pData.HasAnyHandAttachedObject || pData.AttachedEntities.Any())
                    return 0;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return 0;

                if (!tData.Player.IsNearToEntity(player, 7.5f))
                    return 0;

                if (tData.IsKnocked || tData.IsFrozen)
                    return 1;

                var cuffAttach = tData.AttachedObjects.Where(x => x.Type == AttachmentType.Cuffs || x.Type == AttachmentType.CableCuffs).FirstOrDefault();

                if (state)
                {
                    if (cuffAttach != null)
                    {
                        if (cuffAttach.Type == AttachmentType.Cuffs)
                            return 2;
                        else
                            return 3;
                    }

                    tData.Player.AttachObject(Attachments.Service.Models.Cuffs, AttachmentType.Cuffs, -1, null);

                    tData.Player.NotifyWithPlayer("Cuffs::0_0", player);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_CUFFS_ON_0"), target);
                }
                else
                {
                    if (cuffAttach == null)
                        return 2;

                    if (tData.Player.DetachObject(cuffAttach.Type))
                    {
                        if (cuffAttach.Type == AttachmentType.Cuffs)
                        {
                            tData.Player.NotifyWithPlayer("Cuffs::0_1", player);

                            Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_CUFFS_OFF_0"), target);
                        }
                        else
                        {
                            tData.Player.NotifyWithPlayer("Cuffs::1_1", player);

                            Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_CUFFS_OFF_1"), target);
                        }
                    }
                }

                return byte.MaxValue;
            }

            [RemoteProc("Police::Escort")]
            private static byte Escort(Player player, Player target, bool state)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (state)
                {
                    var tData = target?.GetMainData();

                    if (tData == null || tData == pData)
                        return 0;

                    if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || pData.IsAttachedToEntity != null || pData.HasAnyHandAttachedObject || pData.AttachedEntities.Any())
                        return 0;

                    if (!tData.Player.IsNearToEntity(player, 7.5f))
                        return 0;

                    if (!tData.IsCuffed)
                        return 1;

                    var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                    if (fData == null)
                        return 0;

                    if (tData.Player.GetEntityIsAttachedTo() is Entity entityAttachedTo)
                    {
                        var attachData = entityAttachedTo.GetAttachmentData(tData.Player);

                        if (attachData != null && attachData.Type == AttachmentType.PoliceEscort)
                            return 0;
                    }

                    pData.PlayAnim(GeneralType.PoliceEscort0);

                    player.AttachEntity(target, AttachmentType.PoliceEscort, null);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_ESCORT_ON"), target);

                    return 255;
                }
                else
                {
                    var attachData = pData.AttachedEntities.Where(x => x.Type == AttachmentType.PoliceEscort && x.EntityType == EntityType.Player).FirstOrDefault();

                    if (attachData == null)
                        return 0;

                    target = Utils.GetPlayerByID(attachData.Id);

                    if (target == null)
                        return 0;

                    pData.Player.DetachEntity(target);

                    if (pData.GeneralAnim == GeneralType.PoliceEscort0)
                        pData.StopGeneralAnim();

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_ESCORT_OFF"), target);

                    return 255;
                }
            }

            [RemoteProc("Police::FPTV")]
            private static byte ForcePlayerToVehicle(Player player, Vehicle veh, byte seatIdx)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (pData.IsFrozen || pData.IsKnocked || pData.IsCuffed)
                    return 0;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return 0;

                var attachData = pData.AttachedEntities.Where(x => (x.Type == AttachmentType.PoliceEscort || x.Type == AttachmentType.Hostage || x.Type == AttachmentType.Carry) && x.EntityType == EntityType.Player).FirstOrDefault();

                if (attachData == null)
                    return 0;

                var target = Utils.GetPlayerByID(attachData.Id);

                var tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return 0;

                var vData = veh.GetMainData();

                if (vData == null)
                    return 0;

                if (!tData.Player.IsNearToEntity(player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                if (!vData.Vehicle.IsNearToEntity(player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                if (seatIdx == 255)
                {
                    if (vData.AttachedEntities.Where(x => x.Type == AttachmentType.VehicleTrunk).Any())
                        return 0;

                    if (vData.TrunkLocked)
                        return 2;

                    vData.Vehicle.AttachEntity(target, AttachmentType.VehicleTrunk, null);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PUSHTOVEH_1", "{0}", vData.GetName(1)), target);

                    return 255;
                }
                else
                {
                    if (seatIdx < 1 || seatIdx >= vData.Vehicle.MaxOccupants || vData.Vehicle.GetEntityInVehicleSeat(seatIdx) != null)
                        return 0;

                    if (vData.Locked)
                        return 1;

                    player.DetachEntity(target);

                    target.WarpToVehicleSeat(veh, seatIdx, 500);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PUSHTOVEH_0", "{0}", vData.GetName(1)), target);

                    return 255;
                }
            }

            [RemoteProc("Police::FPFV")]
            private static byte ForcePlayerFromVehicle(Player player, Vehicle veh, Player target)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (pData.IsFrozen || pData.IsKnocked || pData.IsCuffed || pData.IsAttachedToEntity != null || pData.AttachedEntities.Any() || pData.HasAnyHandAttachedObject)
                    return 0;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return 0;

                var tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return 0;

                var vData = veh.GetMainData();

                if (vData == null)
                    return 0;

                if (!tData.Player.IsNearToEntity(player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                if (!vData.Vehicle.IsNearToEntity(player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                if (!tData.IsKnocked && !tData.IsCuffed)
                    return 0;

                if (target.Vehicle == veh)
                {
                    if (tData.VehicleSeat < 1)
                        return 0;

                    if (vData.Locked)
                        return 1;

                    target.WarpOutOfVehicle();

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_OUTOFVEH_0", "{0}", vData.GetName(1)), target);

                    return 255;
                }
                else
                {
                    var attachData = vData.AttachedEntities.Where(x => x.Type == AttachmentType.VehicleTrunk && x.Id == target.Id && x.EntityType == EntityType.Player).FirstOrDefault();

                    if (attachData == null)
                        return 0;

                    if (vData.TrunkLocked)
                        return 2;

                    veh.DetachEntity(target);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_OUTOFVEH_1", "{0}", vData.GetName(1)), target);

                    return 255;
                }
            }

            [RemoteProc("Police::PMaskOff")]
            private static byte PolicePlayerMaskOff(Player player, Player target)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (pData.IsFrozen || pData.IsKnocked || pData.IsCuffed || pData.IsAttachedToEntity != null || pData.AttachedEntities.Any() || pData.HasAnyHandAttachedObject)
                    return 0;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return 0;

                var tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return 0;

                if (!tData.Player.IsNearToEntity(player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                if (!tData.IsCuffed)
                    return 0;

                var currentMask = tData.WearedMask;

                if (currentMask == null)
                    return 1;

                currentMask.Unwear(tData);

                currentMask.Delete();

                tData.Player.InventoryUpdate(GroupTypes.Accessories, 1, Game.Items.Item.ToClientJson(null, GroupTypes.Accessories));

                Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_MASKOFF_0"), target);

                tData.Player.NotifyWithPlayer("Police::PMASKOFF_0", player);

                return 255;
            }

            [RemoteProc("Police::Arrest")]
            private static byte Arrest(Player player, Player target, int fTypeNum, ushort time, string reason1, string reason2)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fTypeNum))
                    return 0;

                var arrestFData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)fTypeNum);

                if (arrestFData == null)
                    return 0;

                if (pData.IsFrozen || pData.IsKnocked || pData.IsCuffed)
                    return 0;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return 0;

                var tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return 0;

                if (!tData.Player.IsNearToEntity(pData.Player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                reason1 = reason1.Trim();
                reason2 = reason2.Trim();

                if (fData == arrestFData)
                {
                    if (!fData.HasMemberPermission(pData.Info, 10, true))
                        return 1;

                    if (fData.ArrestColshapePosition.Position.DistanceTo(player.Position) > fData.ArrestColshapePosition.RotationZ + 2.5f)
                        return 0;

                    if (!tData.IsCuffed)
                        return 0;

                    var activePunishment = tData.Punishments.Where(x => (x.Type == PunishmentType.Arrest || x.Type == PunishmentType.FederalPrison || x.Type == PunishmentType.NRPPrison) && x.IsActive()).FirstOrDefault();

                    if (activePunishment != null)
                        return 2;

                    if (time <= 0 || time > Game.Fractions.Police.ArrestMaxTime.TotalMinutes || !Game.Fractions.Police.ArrestReason1Regex.IsMatch(reason1) || !Game.Fractions.Police.ArrestReason2Regex.IsMatch(reason2))
                        return 0;

                    var curTime = Utils.GetCurrentTime();

                    var punishment = new Punishment(Punishment.GetNextId(), PunishmentType.Arrest, $"{reason1}^{reason2}", curTime, DateTimeOffset.FromUnixTimeSeconds(time * 60).DateTime, pData.CID)
                    {
                        AdditionalData = $"0_{(int)fData.Type}",
                    };

                    fData.AddActiveArrest(new Game.Fractions.Police.ArrestInfo() { PunishmentData = punishment, MemberName = $"{player.Name}", TargetName = $"{target.Name}", TargetCID = tData.CID });

                    tData.Info.Punishments.Add(punishment);

                    MySQL.AddPunishment(tData.Info, pData.Info, punishment);

                    fData.SetPlayerToPrison(tData, false);
                    tData.ResetUpdateTimer();

                    tData.Player.TriggerEvent("Player::Punish", punishment.Id, (int)punishment.Type, pData.Player.Id, punishment.EndDate.GetUnixTimestamp(), reason1, punishment.AdditionalData);

                    try
                    {
                        Game.Fractions.Police.SetPlayerArrestAmount(pData.Info, (ushort)(Game.Fractions.Police.GetPlayerArrestAmount(pData.Info) + 1));
                    }
                    catch (Exception ex)
                    {

                    }
                }
                else
                {
                    if (arrestFData is Game.Fractions.Prison prisonData)
                    {
                        if (!fData.HasMemberPermission(pData.Info, 13, true))
                            return 1;

                        return 0;
                    }
                    else
                    {
                        return 77;
                    }
                }

                return 0;
            }

            [RemoteProc("Police::DBS")]
            private static object GetPersonInfoFromDataBase(Player player, byte searchType, string searchStr)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (searchStr == null)
                    return 0;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return 0;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                if (fData == null)
                    return 0;

                if (!fData.HasMemberPermission(pData.Info, 16, true))
                    return 0;

                PlayerInfo tInfo = null;

                if (searchType == 0) // by phone
                {
                    uint phoneNumber;

                    if (!uint.TryParse(searchStr, out phoneNumber))
                        return 0;

                    tInfo = PlayerInfo.All.Values.Where(x => x.PhoneNumber == phoneNumber).FirstOrDefault();
                }
                else if (searchType == 1) // by name
                {
                    var nameArr = searchStr.Split(' ');

                    if (nameArr.Length != 2)
                        return 0;

                    var name = nameArr[0]; var surname = nameArr[1];

                    if (!Events.Players.CharacterCreation.CharacterNameRegex.IsMatch(name) || !Events.Players.CharacterCreation.CharacterSurnameRegex.IsMatch(surname))
                        return 0;

                    tInfo = PlayerInfo.All.Values.Where(x => x.Name == name && x.Surname == surname).FirstOrDefault();
                }
                else if (searchType == 2) // by veh plate
                {
                    if (!Utils.NumberplatePattern.IsMatch(searchStr))
                        return 0;

                    tInfo = VehicleInfo.All.Values.Where(x => x.RegisteredNumberplate == searchStr).FirstOrDefault()?.FullOwnerPlayer;
                }
                else if (searchType == 3) // by pid
                {
                    uint pid;

                    if (!uint.TryParse(searchStr, out pid))
                        return 0;

                    tInfo = pid >= Properties.Settings.Profile.Current.Game.CIDBaseOffset ? PlayerInfo.Get(pid) : PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info;
                }

                if (tInfo == null)
                    return null;

                var obj = new JObject()
                {
                    { "I", tInfo.CID },
                    { "N", tInfo.Name },
                    { "S", tInfo.Surname },
                    { "BD", tInfo.BirthDate.GetUnixTimestamp() },
                    { "G", tInfo.Sex },
                    { "LA", tInfo.LosSantosAllowed },
                    { "PN", tInfo.PhoneNumber },

                    { "V", JArray.FromObject(tInfo.OwnedVehicles.Select(x => $"{x.ID}&{x.RegisteredNumberplate ?? string.Empty}&{x.Tuning.Colour1.HEX}")) },
                };

                var tFData = Game.Fractions.Fraction.Get(tInfo.Fraction);

                if (tFData != null && tFData.MetaFlags.HasFlag(Game.Fractions.Fraction.FlagTypes.MembersHaveDocs))
                {
                    obj.Add("FT", (int)tFData.Type);
                    obj.Add("FR", tInfo.FractionRank);
                }

                if (tInfo.OwnedHouses.FirstOrDefault() is Game.Estates.House house)
                {
                    obj.Add("H", house.Id);
                }

                if (tInfo.OwnedApartments.FirstOrDefault() is Game.Estates.Apartments aps)
                {
                    obj.Add("A", aps.Id);
                }

                Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_POLICE_TABLETPC_FOUND"), null);

                return obj;
            }

            [RemoteProc("Police::APBF")]
            private static bool PoliceApbFinish(Player player, uint apbId)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return false;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return false;

                if (!fData.HasMemberPermission(pData.Info, 17, true))
                    return false;

                var apbInfo = Game.Fractions.Police.GetAPB(apbId);

                if (apbInfo == null)
                    return false;

                if (apbInfo.FractionType != Game.Fractions.FractionType.None && apbInfo.FractionType != fData.Type)
                {
                    player.Notify("Police::APB::NYD");

                    return false;
                }

                Game.Fractions.Police.RemoveAPB(apbId);

                return true;
            }

            [RemoteProc("Police::APBA")]
            private static bool PoliceApbAdd(Player player, string targetName, string details, string largeDetails)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (targetName == null || details == null || largeDetails == null)
                    return false;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return false;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return false;

                if (!fData.HasMemberPermission(pData.Info, 17, true))
                    return false;

                // check texts

                var apbInfo = new Game.Fractions.Police.APBInfo() { FractionType = fData.Type, TargetName = targetName, Details = details, LargeDetails = largeDetails, Member = $"{pData.Player.Name}", Time = Utils.GetCurrentTime() };

                Game.Fractions.Police.AddAPB(apbInfo);

                return true;
            }

            [RemoteProc("Police::CODE")]
            private static bool PoliceCode(Player player, byte code)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (code != 0 && code != 1 && code != 2)
                    return false;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return false;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return false;

                if (!fData.HasMemberPermission(pData.Info, 18, true))
                    return false;

                var existingCall = Game.Fractions.Police.GetCallByCaller(player.Id);

                var cdHash = NAPI.Util.GetHashKey("POLICE_EXTRA_CODE");

                var curTime = Utils.GetCurrentTime();

                if (existingCall != null)
                {
                    TimeSpan timeLeft;

                    if (pData.Info.HasCooldown(cdHash, curTime, out _, out timeLeft, out _, 1d))
                    {
                        player.NotifyError(Language.Strings.Get("NTFC_COOLDOWN_GEN_2", timeLeft.GetBeautyString()));

                        return false;
                    }

                    Game.Fractions.Police.RemoveCall(player.Id, existingCall, 0, null);
                }

                var callInfo = new Game.Fractions.Police.CallInfo() { Type = code, Position = player.Position, Message = string.Empty, Time = curTime, FractionType = code == 0 ? Game.Fractions.FractionType.None : fData.Type };

                Game.Fractions.Police.AddCall(player.Id, callInfo);

                pData.Info.SetCooldown(cdHash, curTime, Game.Fractions.Police.CallExtraCooldownTime, false);

                return true;
            }

            [RemoteProc("Police::CODEF")]
            private static bool PoliceCodeFinish(Player player, ushort rid)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return false;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return false;

                if (!fData.HasMemberPermission(pData.Info, 19, true))
                    return false;

                var callInfo = Game.Fractions.Police.GetCallByCaller(rid);

                if (callInfo == null || (callInfo.FractionType != Game.Fractions.FractionType.None || callInfo.FractionType != fData.Type))
                    return false;

                if (callInfo.Position.DistanceTo(player.Position) > 15f)
                    return false;

                Game.Fractions.Police.RemoveCall(rid, callInfo, 1, pData);

                return true;
            }

            [RemoteProc("Police::APBGLD")]
            private static string PoliceAPBGetLargeDetails(Player player, uint uid)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                var apb = Game.Fractions.Police.GetAPB(uid);

                if (apb == null)
                    return null;

                return apb.LargeDetails;
            }

            [RemoteProc("Police::GPSTRL")]
            private static ushort? PoliceGPSTrackerLocate(Player player, uint uid)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return null;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                var gpsTrackerInfo = Game.Fractions.Police.GetGPSTrackerById(uid);

                if (gpsTrackerInfo == null)
                    return null;

                var vInfo = VehicleInfo.Get(gpsTrackerInfo.VID);

                if (vInfo == null)
                {
                    player.Notify("Vehicle::KENS");

                    return null;
                }

                if (!Sync.Vehicles.TryLocateOwnedVehicle(pData, vInfo))
                    return null;

                return vInfo.VehicleData?.Vehicle.Id;
            }

            [RemoteProc("Police::GPSTRD")]
            private static bool PoliceGPSTrackerDestroy(Player player, uint uid)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return false;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return false;

                if (!fData.HasMemberPermission(pData.Info, 21, true))
                    return false;

                var gpsTrackerInfo = Game.Fractions.Police.GetGPSTrackerById(uid);

                if (gpsTrackerInfo == null)
                    return false;

                if (gpsTrackerInfo.FractionType != Game.Fractions.FractionType.None && gpsTrackerInfo.FractionType != fData.Type)
                {
                    player.Notify("Police::GPSTR::NYD");

                    return false;
                }

                Game.Fractions.Police.RemoveGPSTracker(uid, gpsTrackerInfo);

                return true;
            }

            [RemoteProc("Police::GPSTRI")]
            private static object PoliceGPSTrackerInstall(Player player, Vehicle vehicle, int slot, bool allDepsSee)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (!pData.CanUseInventory(true) || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return null;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                if (!fData.HasMemberPermission(pData.Info, 20, true))
                    return null;

                var vData = vehicle.GetMainData();

                if (vData == null)
                    return null;

                if (!vData.Vehicle.IsNearToEntity(player, 7.5f))
                    return null;

                if (slot < 0 || slot >= pData.Items.Length)
                    return null;

                var gpsTracker = pData.Items[slot];

                if (gpsTracker == null || gpsTracker.ID != "mis_gpstr")
                    return null;

                if (gpsTracker is Game.Items.IStackable stackable && stackable.Amount > 1)
                {
                    stackable.Amount -= 1;

                    gpsTracker.Update();
                }
                else
                {
                    gpsTracker.Delete();

                    gpsTracker = null;
                }

                player.InventoryUpdate(GroupTypes.Items, slot, Game.Items.Item.ToClientJson(gpsTracker, GroupTypes.Items));

                var id = Game.Fractions.Police.AddGPSTracker(new Game.Fractions.Police.GPSTrackerInfo() { VID = vData.VID, FractionType = allDepsSee ? Game.Fractions.FractionType.None : fData.Type, InstallerStr = $"{pData.Player.Name}", VehicleStr = $"{vData.Data.Name} [{vData.Info.RegisteredNumberplate ?? string.Empty}]" });

                return id;
            }

            [RemoteEvent("Police::MRPCA")]
            private static void PoliceMemberPoliceRPChatAction(Player player, byte actionNum)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return;

                if (actionNum == 0)
                {
                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_POLICE_TABLETPC_ON"), null);
                }
                else if (actionNum == 1)
                {
                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_POLICE_TABLETPC_OFF"), null);
                }
            }

            [RemoteProc("Police::TPCS")]
            private static object PoliceTabletPCShow(Player player)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                return $"{(fData.IsPlayerInAnyUniform(pData) ? 1 : 0)}_{Game.Fractions.Police.GetPlayerArrestAmount(pData.Info)}";
            }

            [RemoteProc("Police::ARGI")]
            private static object PoliceArrestGetInfo(Player player, int fractionTypeNum, byte menuPosIdx, uint punishmentId)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return null;

                if (player.Dimension != Properties.Settings.Static.MainDimension)
                    return null;

                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)fractionTypeNum) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                var menuPos = fData.GetArrestMenuPosition(menuPosIdx);

                if (menuPos == null || menuPos.DistanceTo(player.Position) > 5f)
                    return null;

                var arrestInfo = fData.GetArrestInfoById(punishmentId);

                if (arrestInfo == null)
                    return null;

                long secondsPassed;

                var secondsLeft = arrestInfo.PunishmentData.GetSecondsLeft(out secondsPassed);

                return $"{arrestInfo.PunishmentData.StartDate.GetUnixTimestamp()}_{secondsLeft}_{secondsPassed}_{arrestInfo.TargetName}_{arrestInfo.TargetCID}_{arrestInfo.MemberName}_{arrestInfo.PunishmentData.Reason}";
            }

            [RemoteProc("Police::ARF")]
            private static bool PoliceArrestFree(Player player, int fractionTypeNum, byte menuPosIdx, uint punishmentId, string reason)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return false;

                if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return false;

                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)fractionTypeNum) as Game.Fractions.Police;

                if (fData == null)
                    return false;

                var menuPos = fData.GetArrestMenuPosition(menuPosIdx);

                if (menuPos == null || menuPos.DistanceTo(player.Position) > 5f)
                    return false;

                var arrestInfo = fData.GetArrestInfoById(punishmentId);

                if (arrestInfo == null)
                    return false;

                if (reason == null)
                    return true;

                reason = reason.Trim();

                if (!Game.Fractions.Police.ArrestChangeReasonRegex.IsMatch(reason))
                    return false;

                var tInfo = PlayerInfo.Get(arrestInfo.TargetCID);

                if (tInfo == null)
                    return false;

                arrestInfo.PunishmentData.OnFinish(tInfo, 1, pData, reason);

                arrestInfo.PunishmentData.AmnestyInfo = new Punishment.Amnesty() { CID = pData.CID, Date = Utils.GetCurrentTime(), Reason = reason, };

                MySQL.UpdatePunishmentAmnesty(arrestInfo.PunishmentData);

                fData.SendFractionChatMessage(Language.Strings.Get("CHAT_POLICE_ARRESTCHANGE_2", $"{pData.Player.Name} ({pData.Player.Id})", $"#{arrestInfo.PunishmentData.Id}", $"{tInfo.Name} {tInfo.Surname}", reason));

                return true;
            }

            [RemoteProc("Police::ARCT")]
            private static object PoliceArrestChangeTime(Player player, int fractionTypeNum, byte menuPosIdx, uint punishmentId, short timeChange, string reason)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return null;

                if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return null;

                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)fractionTypeNum) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                var menuPos = fData.GetArrestMenuPosition(menuPosIdx);

                if (menuPos == null || menuPos.DistanceTo(player.Position) > 5f)
                    return null;

                var arrestInfo = fData.GetArrestInfoById(punishmentId);

                if (arrestInfo == null)
                    return null;

                if (!arrestInfo.PunishmentData.IsActive())
                    return null;

                if (reason == null)
                    return true;

                if (timeChange == 0 || timeChange < Game.Fractions.Police.ArrestMinTimeChange.TotalMinutes || timeChange > Game.Fractions.Police.ArrestMaxTimeChange.TotalMinutes)
                    return null;

                reason = reason.Trim();

                if (!Game.Fractions.Police.ArrestChangeReasonRegex.IsMatch(reason))
                    return null;

                var tInfo = PlayerInfo.Get(arrestInfo.TargetCID);

                if (tInfo == null)
                    return null;

                var curSecs = arrestInfo.PunishmentData.EndDate.GetUnixTimestamp();

                if (timeChange < 0)
                {
                    var minMins = (curSecs - int.Parse(arrestInfo.PunishmentData.AdditionalData.Split('_')[0])) / -60 + 1;

                    if (timeChange < minMins)
                    {
                        player.Notify("ArrestMenu::E3", minMins);

                        return null;
                    }

                    fData.SendFractionChatMessage(Language.Strings.Get("CHAT_POLICE_ARRESTCHANGE_0", $"{pData.Player.Name} ({pData.Player.Id})", $"{-timeChange}", $"#{arrestInfo.PunishmentData.Id} ({reason})"));
                }
                else
                {
                    var maxMins = Game.Fractions.Police.ArrestMaxTimeAfterAdd.TotalMinutes - curSecs / 60;

                    if (timeChange > maxMins)
                    {
                        player.Notify("ArrestMenu::E4", maxMins);

                        return null;
                    }

                    fData.SendFractionChatMessage(Language.Strings.Get("CHAT_POLICE_ARRESTCHANGE_1", $"{pData.Player.Name} ({pData.Player.Id})", $"{timeChange}", $"#{arrestInfo.PunishmentData.Id} ({reason})"));
                }

                arrestInfo.PunishmentData.EndDate = arrestInfo.PunishmentData.EndDate.AddMinutes(timeChange);

                MySQL.UpdatePunishmentEndDate(arrestInfo.PunishmentData);

                var timeStamp = arrestInfo.PunishmentData.EndDate.GetUnixTimestamp();

                if (tInfo.PlayerData != null)
                {
                    tInfo.PlayerData.Player.TriggerEvent("Player::Punish", arrestInfo.PunishmentData.Id, arrestInfo.PunishmentData.Type, pData.Player.Id, timeStamp, reason);
                }

                return timeStamp;
            }

            [RemoteProc("Police::Call")]
            private static byte PoliceCall(Player player, string message)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (message == null || message.Length == 0)
                {
                    var existingCall = Game.Fractions.Police.GetCallByCaller(player.Id);

                    if (existingCall == null)
                        return 0;

                    Game.Fractions.Police.RemoveCall(player.Id, existingCall, 0, pData);

                    return 255;
                }
                else
                {
                    if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                        return 0;

                    var existingCall = Game.Fractions.Police.GetCallByCaller(player.Id);

                    if (existingCall != null)
                        return 1;

                    message = message.Trim();

                    if (!Game.Fractions.Police.PoliceCallReasonRegex.IsMatch(message))
                        return 0;

                    /*            var cdHash = NAPI.Util.GetHashKey("POLICE_DEF_CALL");

                            var curTime = Utils.GetCurrentTime();

                            if (pData.HasCooldown(cdHash, curTime, Game.Fractions.Police.DEF_CALL_CD_TIMEOUT, out _, out _, out _, 3, true))
                                return 2;*/

                    var callInfo = new Game.Fractions.Police.CallInfo() { Type = 255, Position = player.Position, Message = message, Time = Utils.GetCurrentTime(), FractionType = Game.Fractions.FractionType.None, };

                    Game.Fractions.Police.AddCall(player.Id, callInfo);

                    //pData.Info.SetCooldown(cdHash, curTime, false);

                    return 255;
                }
            }

            [RemoteProc("Police::RmLic")]
            private static object PoliceRemoveLicense(Player player, Player target, string licTypeS)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return null;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                if (!fData.HasMemberPermission(pData.Info, 15, true))
                    return null;

                var tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return null;

                if (!pData.Player.IsNearToEntity(tData.Player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return null;

                if (licTypeS == null || licTypeS.Length == 0)
                {
                    return tData.Info.Licenses;
                }
                else
                {
                    int licTypeN;

                    if (!int.TryParse(licTypeS, out licTypeN) || !Enum.IsDefined(typeof(LicenseType), licTypeN))
                        return null;

                    var licType = (LicenseType)licTypeN;

                    if (!Game.Fractions.Police.AllowedLicenceTypesToRemove.Contains(licType))
                        return 0;

                    tData.Info.RemoveLicense(licType);

                    return 255;
                }
            }

            [RemoteProc("Police::PlayerSearch")]
            private static object PolicePlayerSearch(Player player, Player target, int sType)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return null;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                var tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return null;

                if (!pData.Player.IsNearToEntity(tData.Player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return null;

                if (!tData.IsCuffed)
                    return null;

                if (sType == -1 || sType == -2)
                {
                    var types = new HashSet<byte>() { 0, 1, 2, };

                    if (tData.Bag != null)
                        types.Add(3);

                    if (tData.Holster != null)
                        types.Add(4);

                    if (sType == -1)
                    {
                        Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_START"), target);
                    }

                    return types;
                }
                else if (sType == 0)
                {
                    var docTypes = new HashSet<byte>() { 0, 1, };

                    if (tData.Info.MedicalCard != null)
                        docTypes.Add(2);

                    // add resume (3)

                    var tFData = Game.Fractions.Fraction.Get(tData.Fraction);

                    if (tFData != null)
                    {
                        if (tFData.MetaFlags.HasFlag(Game.Fractions.Fraction.FlagTypes.MembersHaveDocs))
                            docTypes.Add(4);
                    }

                    Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_DOCS_FOUND", "{0}", docTypes.Count), target);

                    return docTypes;
                }
                else if (sType == 1)
                {
                    if (!tData.CanUseInventory(false))
                        return 1;

                    var items = new List<string>();

                    for (int i = 0; i < tData.Weapons.Length; i++)
                    {
                        var x = tData.Weapons[i];

                        if (x != null && Game.Fractions.Police.IsItemConfiscatable(x))
                        {
                            items.Add($"{x.UID}^{x.ID}^{Game.Items.Stuff.GetItemAmount(x)}^{Game.Items.Stuff.GetItemTag(x)}");
                        }
                    }

                    if (tData.Armour != null && Game.Fractions.Police.IsItemConfiscatable(tData.Armour))
                    {
                        var x = tData.Armour;

                        items.Add($"{x.UID}^{x.ID}^{Game.Items.Stuff.GetItemAmount(x)}^{Game.Items.Stuff.GetItemTag(x)}");
                    }

                    Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_ITEMS_2_FOUND", "{0}", items.Count), target);

                    return items;
                }
                else if (sType == 2)
                {
                    if (!tData.CanUseInventory(false))
                        return 1;

                    var items = new List<string>();

                    for (int i = 0; i < tData.Items.Length; i++)
                    {
                        var x = tData.Items[i];

                        if (x != null && Game.Fractions.Police.IsItemConfiscatable(x))
                        {
                            items.Add($"{x.UID}^{x.ID}^{Game.Items.Stuff.GetItemAmount(x)}^{Game.Items.Stuff.GetItemTag(x)}");
                        }
                    }

                    Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_ITEMS_0_FOUND", "{0}", items.Count), target);

                    return items;
                }
                else if (sType == 3)
                {
                    if (!tData.CanUseInventory(false))
                        return 1;

                    if (tData.Bag == null)
                        return 2;

                    var items = new List<string>();

                    for (int i = 0; i < tData.Bag.Items.Length; i++)
                    {
                        var x = tData.Bag.Items[i];

                        if (x != null && Game.Fractions.Police.IsItemConfiscatable(x))
                        {
                            items.Add($"{x.UID}^{x.ID}^{Game.Items.Stuff.GetItemAmount(x)}^{Game.Items.Stuff.GetItemTag(x)}");
                        }
                    }

                    Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_ITEMS_1_FOUND", "{0}", items.Count), target);

                    return items;
                }
                else if (sType == 4)
                {
                    if (!tData.CanUseInventory(false))
                        return 1;

                    if (tData.Holster == null)
                        return 2;

                    var items = new List<string>();

                    if (tData.Holster.Items[0] != null && Game.Fractions.Police.IsItemConfiscatable(tData.Holster.Items[0]))
                    {
                        var x = tData.Holster.Items[0];

                        items.Add($"{x.UID}^{x.ID}^{Game.Items.Stuff.GetItemAmount(x)}^{Game.Items.Stuff.GetItemTag(x)}");
                    }

                    Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_ITEMS_3_FOUND", "{0}", items.Count));

                    return items;
                }

                return null;
            }

            [RemoteProc("Police::PlayerSearchSD")]
            private static object PolicePlayerSearchShowDocs(Player player, Player target, int dType)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return null;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                var tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return null;

                if (!pData.Player.IsNearToEntity(tData.Player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return null;

                if (!tData.IsCuffed)
                    return null;

                if (dType == 0)
                {
                    tData.ShowPassport(pData.Player);

                    pData.Info.AddFamiliar(tData.Info);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_DOCS_LOOK_0"), target);

                    return true;
                }
                else if (dType == 1)
                {
                    tData.ShowLicences(pData.Player);

                    pData.Info.AddFamiliar(tData.Info);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_DOCS_LOOK_1"), target);

                    return true;
                }
                else if (dType == 2)
                {
                    if (tData.Info.MedicalCard == null)
                        return false;

                    tData.Info.MedicalCard.Show(pData.Player, tData.Info);

                    pData.Info.AddFamiliar(tData.Info);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_DOCS_LOOK_2"), target);

                    return true;
                }
                else if (dType == 3)
                {
                    // resume

                    return null;
                }
                else if (dType == 4)
                {
                    var tFData = Game.Fractions.Fraction.Get(tData.Fraction);

                    if (tFData == null || !tFData.MetaFlags.HasFlag(Game.Fractions.Fraction.FlagTypes.MembersHaveDocs))
                        return false;

                    tData.ShowFractionDocs(player, tFData, tData.Info.FractionRank);

                    pData.Info.AddFamiliar(tData.Info);

                    Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_DOCS_LOOK_4"), target);

                    return true;
                }

                return null;
            }

            [RemoteProc("Police::PlayerSearchIC")]
            private static byte PolicePlayerSearchItemConfiscate(Player player, Player target, int cType, uint itemUid)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return 0;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return 0;

                var tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return 0;

                if (!pData.Player.IsNearToEntity(tData.Player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                if (!tData.IsCuffed)
                    return 0;

                if (!tData.CanUseInventory())
                    return 0;

                Game.Items.Item item = null;
                int itemIdx = -1;

                var gType = GroupTypes.Items;

                if (cType == 1)
                {
                    if (tData.Armour?.UID == itemUid)
                    {
                        itemIdx = 0;

                        item = tData.Armour;

                        gType = GroupTypes.Armour;
                    }
                    else
                    {
                        for (int i = 0; i < tData.Weapons.Length; i++)
                        {
                            if (tData.Weapons[i] != null && tData.Weapons[i].UID == itemUid)
                            {
                                item = tData.Weapons[i];

                                itemIdx = i;

                                gType = GroupTypes.Weapons;

                                break;
                            }
                        }
                    }
                }
                else if (cType == 2)
                {
                    gType = GroupTypes.Items;

                    for (int i = 0; i < tData.Items.Length; i++)
                    {
                        if (tData.Items[i] != null && tData.Items[i].UID == itemUid)
                        {
                            item = tData.Items[i];

                            itemIdx = i;

                            break;
                        }
                    }
                }
                else if (cType == 3)
                {
                    gType = GroupTypes.Bag;

                    if (tData.Bag == null)
                        return 2;

                    for (int i = 0; i < tData.Bag.Items.Length; i++)
                    {
                        if (tData.Bag.Items[i] != null && tData.Bag.Items[i].UID == itemUid)
                        {
                            item = tData.Bag.Items[i];

                            itemIdx = i;

                            break;
                        }
                    }
                }
                else if (cType == 4)
                {
                    gType = GroupTypes.Holster;

                    if (tData.Holster == null)
                        return 2;

                    if (tData.Holster.Items[0] != null && tData.Holster.Items[0].UID == itemUid)
                    {
                        item = tData.Holster.Items[0];

                        itemIdx = 0;
                    }
                }

                if (item == null)
                    return 1;

                if (!Game.Fractions.Police.IsItemConfiscatable(item))
                    return 3;

                var amount = Game.Items.Stuff.GetItemAmount(item);

                if (!pData.TryGiveExistingItem(item, amount, true, true))
                    return 4;

                if (cType == 1)
                {
                    if (gType == GroupTypes.Weapons)
                    {
                        tData.Weapons[itemIdx] = null;

                        MySQL.CharacterWeaponsUpdate(tData.Info);

                        target.InventoryUpdate(gType, itemIdx, Game.Items.Item.ToClientJson(tData.Weapons[itemIdx], gType));
                    }
                    else if (gType == GroupTypes.Armour)
                    {
                        tData.Armour = null;

                        MySQL.CharacterArmourUpdate(tData.Info);

                        target.InventoryUpdate(gType, itemIdx, Game.Items.Item.ToClientJson(tData.Armour, gType));
                    }
                }
                else if (cType == 2)
                {
                    tData.Items[itemIdx] = null;

                    MySQL.CharacterItemsUpdate(tData.Info);

                    target.InventoryUpdate(gType, itemIdx, Game.Items.Item.ToClientJson(tData.Items[itemIdx], gType));
                }
                else if (cType == 3)
                {
                    tData.Bag.Items[itemIdx] = null;

                    tData.Bag.Update();

                    target.InventoryUpdate(gType, itemIdx, Game.Items.Item.ToClientJson(tData.Items[itemIdx], gType));
                }
                else if (cType == 4)
                {
                    tData.Holster.Items[itemIdx] = null;

                    tData.Holster.Update();

                    target.InventoryUpdate(gType, itemIdx, Game.Items.Item.ToClientJson(tData.Holster.Items[itemIdx], gType));
                }

                Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_CONFISCATE_0", $"{Game.Items.Stuff.GetItemNameWithTag(item, Game.Items.Stuff.GetItemTag(item), out _)} x{amount}"));
                Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_PSEARCH_CONFISCATE_1"), target);

                return 255;
            }

            [RemoteProc("Police::VehicleSearch")]
            private static object PoliceVehicleSearch(Player player, Vehicle target, int sType)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return null;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return null;

                var tData = target.GetMainData();

                if (tData == null)
                    return null;

                if (!pData.Player.IsNearToEntity(target, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return null;

                if (sType == -1 || sType == -2)
                {
                    var types = new HashSet<byte>() { 0, };

                    if (sType == -1)
                    {
                        Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_VSEARCH_START", tData.GetName(1)));
                    }

                    return types;
                }
                else if (sType == 0)
                {
                    if (tData.Info.TID == 0)
                        return 0;

                    var trunk = Container.Get(tData.Info.TID);

                    if (trunk == null)
                        return null;

                    if (!tData.CanUseTrunk(pData, true))
                        return null;

                    var items = new List<string>();

                    for (int i = 0; i < trunk.Items.Length; i++)
                    {
                        var x = trunk.Items[i];

                        if (x != null && Game.Fractions.Police.IsItemConfiscatable(x))
                        {
                            items.Add($"{x.UID}^{x.ID}^{Game.Items.Stuff.GetItemAmount(x)}^{Game.Items.Stuff.GetItemTag(x)}");
                        }
                    }

                    Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_VSEARCH_ITEMS_FOUND_0", tData.GetName(1), items.Count));

                    return items;
                }

                return null;
            }

            [RemoteProc("Police::VehicleSearchIC")]
            private static byte PoliceVehicleSearchItemConfiscate(Player player, Vehicle target, int cType, uint itemUid)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                    return 0;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return 0;

                var tData = target.GetMainData();

                if (tData == null)
                    return 0;

                if (!pData.Player.IsNearToEntity(tData.Vehicle, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                    return 0;

                if (tData.Info.TID == 0)
                    return 0;

                var trunk = Container.Get(tData.Info.TID);

                if (trunk == null)
                    return 0;

                if (!tData.CanUseTrunk(pData, true))
                    return 0;

                Game.Items.Item item = null;
                int itemIdx = -1;

                if (cType == 0)
                {
                    for (int i = 0; i < trunk.Items.Length; i++)
                    {
                        if (trunk.Items[i] != null && trunk.Items[i].UID == itemUid)
                        {
                            item = trunk.Items[i];

                            itemIdx = i;

                            break;
                        }
                    }
                }

                if (item == null)
                    return 1;

                if (!Game.Fractions.Police.IsItemConfiscatable(item))
                    return 3;

                var amount = Game.Items.Stuff.GetItemAmount(item);

                if (!pData.TryGiveExistingItem(item, amount, true, true))
                    return 4;

                if (cType == 0)
                {
                    trunk.Items[itemIdx] = null;

                    trunk.Update();

                    var players = trunk.GetPlayersObservingArray();

                    if (players.Length > 0)
                        Utils.InventoryUpdate(GroupTypes.Container, itemIdx, Game.Items.Item.ToClientJson(trunk.Items[itemIdx], GroupTypes.Container), players);
                }

                Management.Chat.Service.SendLocal(MessageType.Do, player, Language.Strings.Get("CHAT_PLAYER_VSEARCH_CONFISCATE_0", $"{Game.Items.Stuff.GetItemNameWithTag(item, Game.Items.Stuff.GetItemTag(item), out _)} x{amount}"));
                Management.Chat.Service.SendLocal(MessageType.Me, player, Language.Strings.Get("CHAT_PLAYER_VSEARCH_CONFISCATE_1", tData.GetName(1)));

                return 255;
            }
        }
    }
}