using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Asn1.Tsp;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Fractions
{
    internal class Police : Script
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

            if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return 0;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

            if (fData == null)
                return 0;

            if (!tData.Player.AreEntitiesNearby(player, 7.5f))
                return 0;

            if (tData.IsKnocked || tData.IsFrozen)
                return 1;

            var cuffAttach = tData.AttachedObjects.Where(x => x.Type == Sync.AttachSystem.Types.Cuffs || x.Type == Sync.AttachSystem.Types.CableCuffs).FirstOrDefault();

            if (state)
            {
                if (cuffAttach != null)
                {
                    if (cuffAttach.Type == Sync.AttachSystem.Types.Cuffs)
                        return 2;
                    else
                        return 3;
                }

                if (tData.IsAttachedToEntity is Entity entity)
                {
                    entity.DetachEntity(tData.Player);
                }

                tData.Player.DetachAllEntities();
                tData.Player.DetachAllObjectsInHand();

                tData.Player.AttachObject(Sync.AttachSystem.Models.Cuffs, Sync.AttachSystem.Types.Cuffs, -1, null);

                tData.PlayAnim(Sync.Animations.GeneralTypes.CuffedStatic0);

                tData.Player.NotifyWithPlayer("Cuffs::0_0", player);
            }
            else
            {
                if (cuffAttach == null)
                    return 2;

                if (cuffAttach.Type != Sync.AttachSystem.Types.Cuffs)
                    return 3;

                if (tData.IsAttachedToEntity is Entity entity)
                {
                    entity.DetachEntity(tData.Player);
                }

                if (tData.Player.DetachObject(Sync.AttachSystem.Types.Cuffs))
                {
                    tData.Player.NotifyWithPlayer("Cuffs::0_1", player);
                }

                if (tData.GeneralAnim == Sync.Animations.GeneralTypes.CuffedStatic0)
                    tData.StopGeneralAnim();
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

                if (pData.IsCuffed || pData.IsFrozen || pData.IsKnocked || pData.IsAttachedToEntity != null || pData.HasAnyHandAttachedObject || pData.AttachedEntities.Count > 0)
                    return 0;

                if (!tData.Player.AreEntitiesNearby(player, 7.5f))
                    return 0;

                if (!tData.IsCuffed)
                    return 1;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

                if (fData == null)
                    return 0;

                if (tData.Player.GetEntityIsAttachedTo() is Entity entityAttachedTo)
                {
                    var attachData = entityAttachedTo.GetAttachmentData(tData.Player);

                    if (attachData != null && attachData.Type == Sync.AttachSystem.Types.PoliceEscort)
                        return 0;
                }

                pData.PlayAnim(Sync.Animations.GeneralTypes.PoliceEscort0);

                player.AttachEntity(target, Sync.AttachSystem.Types.PoliceEscort);

                return 255;
            }
            else
            {
                var attachData = pData.AttachedEntities.Where(x => x.Type == Sync.AttachSystem.Types.PoliceEscort && x.EntityType == EntityType.Player).FirstOrDefault();

                if (attachData == null)
                    return 0;

                target = Utils.GetPlayerByID(attachData.Id);

                if (target == null)
                    return 0;

                pData.Player.DetachEntity(target);

                if (pData.GeneralAnim == Sync.Animations.GeneralTypes.PoliceEscort0)
                    pData.StopGeneralAnim();

                return 255;
            }
        }

        [RemoteProc("Police::Arrest")]
        private static byte Arrest(Player player, Player target, int fTypeNum, ushort time, string reason1, string reason2)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Fractions.Types), fTypeNum))
                return 0;

            var arrestFData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fTypeNum);

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

            if (!tData.Player.AreEntitiesNearby(pData.Player, 7.5f))
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

                var activePunishment = tData.Punishments.Where(x => (x.Type == Sync.Punishment.Types.Arrest || x.Type == Sync.Punishment.Types.FederalPrison || x.Type == Sync.Punishment.Types.NRPPrison) && x.IsActive()).FirstOrDefault();

                if (activePunishment != null)
                    return 2;

                if (time <= 0 || time > Game.Fractions.Police.ARREST_MAX_MINS || !Game.Fractions.Police.ArrestReason1Regex.IsMatch(reason1) || !Game.Fractions.Police.ArrestReason2Regex.IsMatch(reason2))
                    return 0;

                var curTime = Utils.GetCurrentTime();

                var punishment = new Sync.Punishment(Sync.Punishment.GetNextId(), Sync.Punishment.Types.Arrest, $"{reason1}^{reason2}", curTime, DateTimeOffset.FromUnixTimeSeconds(time * 60).DateTime, pData.CID)
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
                    return 0;
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

            PlayerData.PlayerInfo tInfo = null;

            if (searchType == 0) // by phone
            {
                uint phoneNumber;

                if (!uint.TryParse(searchStr, out phoneNumber))
                    return 0;

                tInfo = PlayerData.PlayerInfo.All.Values.Where(x => x.PhoneNumber == phoneNumber).FirstOrDefault();
            }
            else if (searchType == 1) // by name
            {
                var nameArr = searchStr.Split(' ');

                if (nameArr.Length != 2)
                    return 0;

                var name = nameArr[0]; var surname = nameArr[1];

                if (!Utils.IsNameValid(name) || !Utils.IsNameValid(surname))
                    return 0;

                tInfo = PlayerData.PlayerInfo.All.Values.Where(x => x.Name == name && x.Surname == surname).FirstOrDefault();
            }
            else if (searchType == 2) // by veh plate
            {
                if (!Utils.NumberplatePattern.IsMatch(searchStr))
                    return 0;

                tInfo = VehicleData.VehicleInfo.All.Values.Where(x => x.RegisteredNumberplate == searchStr).FirstOrDefault()?.FullOwnerPlayer;
            }
            else if (searchType == 3) // by pid
            {
                uint pid;

                if (!uint.TryParse(searchStr, out pid))
                    return 0;

                tInfo = pid >= Settings.META_UID_FIRST_CID ? PlayerData.PlayerInfo.Get(pid) : PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info;
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
                { "F", (int)tInfo.Fraction },

                { "V", JArray.FromObject(tInfo.OwnedVehicles.Select(x => $"{x.ID}&{x.RegisteredNumberplate ?? string.Empty}&{x.Tuning.Colour1.HEX}").ToList()) },
            };

            if (tInfo.OwnedHouses.FirstOrDefault() is Game.Estates.House house)
            {
                obj.Add("H", house.Id);
            }

            if (tInfo.OwnedApartments.FirstOrDefault() is Game.Estates.Apartments aps)
            {
                obj.Add("A", aps.Id);
            }

            Sync.Chat.SendLocal(Sync.Chat.Types.Do, player, "Сведения о человеке найдены в служебном планшете.", null);

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

            if (apbInfo.FractionType != Game.Fractions.Types.None && apbInfo.FractionType != fData.Type)
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

            if (existingCall != null)
            {
                if (pData.HasCooldown(cdHash, Utils.GetCurrentTime(), Game.Fractions.Police.EXTRA_CALL_CD_TIMEOUT, out _, out _, out _, 3, true))
                    return false;

                Game.Fractions.Police.RemoveCall(player.Id, existingCall, 0, null);
            }

            var callInfo = new Game.Fractions.Police.CallInfo() { Type = code, Position = player.Position, Message = string.Empty, Time = Utils.GetCurrentTime(), FractionType = code == 0 ? Game.Fractions.Types.None : fData.Type };

            Game.Fractions.Police.AddCall(player.Id, callInfo);

            pData.Info.SetCooldown(cdHash, Utils.GetCurrentTime(), false);

            return true;
        }

        [RemoteProc("Police::CODEF")]
        private static bool PoliceCodeFinish(Player player, ushort rid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Settings.MAIN_DIMENSION || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

            if (fData == null)
                return false;

            if (!fData.HasMemberPermission(pData.Info, 19, true))
                return false;

            var callInfo = Game.Fractions.Police.GetCallByCaller(rid);

            if (callInfo == null || (callInfo.FractionType != Game.Fractions.Types.None || callInfo.FractionType != fData.Type))
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

            var vInfo = VehicleData.VehicleInfo.Get(gpsTrackerInfo.VID);

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

            if (gpsTrackerInfo.FractionType != Game.Fractions.Types.None && gpsTrackerInfo.FractionType != fData.Type)
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

            if (!vData.Vehicle.AreEntitiesNearby(player, 7.5f))
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

            player.InventoryUpdate(Game.Items.Inventory.Groups.Items, slot, Game.Items.Item.ToClientJson(gpsTracker, Game.Items.Inventory.Groups.Items));

            var id = Game.Fractions.Police.AddGPSTracker(new Game.Fractions.Police.GPSTrackerInfo() { VID = vData.VID, FractionType = allDepsSee ? Game.Fractions.Types.None : fData.Type, InstallerStr = $"{pData.Player.Name}", VehicleStr = $"{vData.Data.Name} [{vData.Info.RegisteredNumberplate ?? string.Empty}]" });

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
                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, "достал(а) служебный планшет", null);
            }
            else if (actionNum == 1)
            {
                Sync.Chat.SendLocal(Sync.Chat.Types.Me, player, "убрал(а) служебный планшет", null);
            }
        }

        [RemoteProc("Police::ARGI")]
        private static object PoliceArrestGetInfo(Player player, int fractionTypeNum, byte menuPosIdx, uint punishmentId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum))
                return null;

            if (player.Dimension != Settings.MAIN_DIMENSION)
                return null;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum) as Game.Fractions.Police;

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

            if (!Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum))
                return false;

            if (player.Dimension != Settings.MAIN_DIMENSION || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum) as Game.Fractions.Police;

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

            var tInfo = PlayerData.PlayerInfo.Get(arrestInfo.TargetCID);

            if (tInfo == null)
                return false;

            arrestInfo.PunishmentData.OnFinish(tInfo, 1, pData);

            arrestInfo.PunishmentData.AmnestyInfo = new Sync.Punishment.Amnesty() { CID = pData.CID, Date = Utils.GetCurrentTime(), Reason = reason, };

            MySQL.UpdatePunishmentAmnesty(arrestInfo.PunishmentData);

            fData.SendFractionChatMessage($"{pData.Player.Name} ({pData.Player.Id}) закрыл дело #{arrestInfo.PunishmentData.Id} и выпустил из СИЗО {tInfo.Name} {tInfo.Surname} ({reason})");

            return true;
        }

        [RemoteProc("Police::ARCT")]
        private static object PoliceArrestChangeTime(Player player, int fractionTypeNum, byte menuPosIdx, uint punishmentId, short timeChange, string reason)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum))
                return null;

            if (player.Dimension != Settings.MAIN_DIMENSION || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return null;

            var fData = Game.Fractions.Fraction.Get((Game.Fractions.Types)fractionTypeNum) as Game.Fractions.Police;

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

            if (timeChange == 0 || timeChange < Game.Fractions.Police.ARREST_C_MIN_MINS || timeChange > Game.Fractions.Police.ARREST_C_MAX_MINS)
                return null;

            reason = reason.Trim();

            if (!Game.Fractions.Police.ArrestChangeReasonRegex.IsMatch(reason))
                return null;

            var tInfo = PlayerData.PlayerInfo.Get(arrestInfo.TargetCID);

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

                fData.SendFractionChatMessage($"{pData.Player.Name} ({pData.Player.Id}) уменьшил срок в СИЗО на {timeChange} мин. по делу #{arrestInfo.PunishmentData.Id} ({reason})");
            }
            else
            {
                var maxMins = Game.Fractions.Police.ARREST_MAX_MINS_ADD - curSecs / 60;

                if (timeChange > maxMins)
                {
                    player.Notify("ArrestMenu::E4", maxMins);

                    return null;
                }

                fData.SendFractionChatMessage($"{pData.Player.Name} ({pData.Player.Id}) увеличил срок в СИЗО на {timeChange} мин. по делу #{arrestInfo.PunishmentData.Id} ({reason})");
            }

            arrestInfo.PunishmentData.EndDate = arrestInfo.PunishmentData.EndDate.AddMinutes(timeChange);

            MySQL.UpdatePunishmentEndDate(arrestInfo.PunishmentData);

            var timeStamp = arrestInfo.PunishmentData.EndDate.GetUnixTimestamp();

            if (tInfo.PlayerData != null)
            {
                tInfo.PlayerData.Player.TriggerEvent("Player::Punish", arrestInfo.PunishmentData.Id, 0, ushort.MaxValue, timeStamp, null);
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
                if (player.Dimension != Settings.MAIN_DIMENSION || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
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

                var callInfo = new Game.Fractions.Police.CallInfo() { Type = 255, Position = player.Position, Message = message, Time = Utils.GetCurrentTime(), FractionType = Game.Fractions.Types.None, };

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

            if (!pData.Player.AreEntitiesNearby(tData.Player, 7.5f))
                return null;

            if (licTypeS == null || licTypeS.Length == 0)
            {
                return tData.Licenses;
            }
            else
            {
                int licTypeN;

                if (!int.TryParse(licTypeS, out licTypeN) || !Enum.IsDefined(typeof(PlayerData.LicenseTypes), licTypeN))
                    return null;

                var licType = (PlayerData.LicenseTypes)licTypeN;

                if (!Game.Fractions.Police.AllowedLicenceTypesToRemove.Contains(licType))
                    return 0;

                tData.RemoveLicense(licType);

                return 255;
            }
        }
    }
}
