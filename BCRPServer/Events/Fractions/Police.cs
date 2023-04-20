using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Fractions
{
    internal class Police : Script
    {
        [RemoteProc("Police::Cuff")]
        private static byte Cuff(Player player, Player target, byte stateN)
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

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return 0;

            var state = stateN == 0 ? false : true;

            if (stateN == 2)
            {
                state = !tData.IsCuffed;
            }
            else if (state == tData.IsCuffed)
            {
                return 2;
            }

            if (tData.IsKnocked || tData.IsFrozen)
                return 1;

            tData.IsCuffed = state;

            if (state)
            {
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
                if (tData.IsAttachedToEntity is Entity entity)
                {
                    entity.DetachEntity(tData.Player);
                }

                if (tData.Player.DetachObject(Sync.AttachSystem.Types.Cuffs))
                {
                    tData.Player.NotifyWithPlayer("Cuffs::0_1", player);
                }
                else
                {
                    tData.Player.DetachObject(Sync.AttachSystem.Types.CableCuffs);

                    tData.Player.NotifyWithPlayer("Cuffs::1_1", player);
                }

                if (tData.GeneralAnim == Sync.Animations.GeneralTypes.CuffedStatic0)
                    tData.StopGeneralAnim();
            }

            return byte.MaxValue;
        }

        [RemoteProc("Police::Escort")]
        private static byte Escort(Player player, Player target, byte stateN)
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

            if (!tData.IsCuffed)
                return 1;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return 0;

            var state = stateN == 0 ? false : true;

            var curAttachState = player.GetAttachmentData(target);

            if (curAttachState != null && curAttachState.Type == Sync.AttachSystem.Types.PoliceEscort)
            {
                if (stateN == 2 || !state)
                {
                    if (pData.GeneralAnim == Sync.Animations.GeneralTypes.PoliceEscort0)
                        pData.StopGeneralAnim();

                    player.DetachEntity(tData.Player);

                    // notify both
                }
                else
                {
                    return 2;
                }
            }
            else
            {
                if (tData.IsAttachedToEntity != null)
                    return 2;

                if (pData.AttachedEntities.Where(x => x.Type == Sync.AttachSystem.Types.PoliceEscort).Any())
                    return 3;

                if (stateN == 2 || state)
                {
                    pData.PlayAnim(Sync.Animations.GeneralTypes.PoliceEscort0);

                    player.AttachEntity(target, Sync.AttachSystem.Types.PoliceEscort);

                    // notify both
                }
                else
                {
                    return 2;
                }
            }

            return byte.MaxValue;
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

                tInfo = pid >= Utils.FirstCID ? PlayerData.PlayerInfo.Get(pid) : PlayerData.All.Values.Where(x => x.Player.Id == pid).FirstOrDefault()?.Info;
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

            if (Game.Fractions.Police.GetCallByCaller(player.Id) != null)
            {
                return false;
            }

            var callInfo = new Game.Fractions.Police.CallInfo() { Type = code, Position = player.Position, Message = string.Empty, Time = Utils.GetCurrentTime(), FractionType = code == 0 ? Game.Fractions.Types.None : fData.Type };

            Game.Fractions.Police.AddCall(player.Id, callInfo);

            return true;
        }

        [RemoteProc("Police::CODEF")]
        private static bool PoliceCodeFinish(Player player, ushort rid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
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

        [RemoteProc("Police::Call")]
        private static bool PoliceCall(Player player, string message)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (message == null)
                return false;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsCuffed || pData.IsFrozen || pData.IsKnocked)
                return false;

            if (Game.Fractions.Police.GetCallByCaller(player.Id) != null)
                return false;

            // text check

            var callInfo = new Game.Fractions.Police.CallInfo() { Type = 255, Position = player.Position, Message = message, Time = Utils.GetCurrentTime(), FractionType = Game.Fractions.Types.None, };

            Game.Fractions.Police.AddCall(player.Id, callInfo);

            return true;
        }
    }
}
