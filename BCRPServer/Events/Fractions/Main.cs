﻿using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BCRPServer.Events.Fractions
{
    internal class Main : Script
    {
        private static Regex RankNamePattern = new Regex(@"^[a-zA-Zа-яА-Я0-9\s]{2,12}$", RegexOptions.Compiled);

        [RemoteProc("Fraction::VGPS")]
        private static Vector3 GetFractionVehicleCoordsGPS(Player player, uint vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return null;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return null;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return null;

            var vInfo = VehicleData.VehicleInfo.Get(vid);

            if (vInfo == null)
                return null;

            if (!fData.AllVehicles.ContainsKey(vInfo))
                return null;

            if (vInfo.VehicleData == null || vInfo.VehicleData.Vehicle.Dimension != Utils.Dimensions.Main)
            {
                return null;
            }

            return vInfo.VehicleData.Vehicle.Position;
        }

        [RemoteProc("Fraction::VCMR")]
        private static bool VehicleChangeMinRank(Player player, uint vid, byte newMinRank)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return false;

            if (newMinRank >= fData.Ranks.Count)
                return false;

            var vInfo = VehicleData.VehicleInfo.Get(vid);

            if (vInfo == null)
                return false;

            var vFData = fData.AllVehicles.GetValueOrDefault(vInfo);

            if (vFData == null || vFData.MinimalRank == newMinRank)
                return false;

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return false;

            fData.SetVehicleMinRank(vInfo, vFData, newMinRank, true);

            return true;
        }

        [RemoteProc("Fraction::VRSP")]
        private static bool VehicleRespawn(Player player, uint vid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return false;

            var vInfo = VehicleData.VehicleInfo.Get(vid);

            if (vInfo == null)
                return false;

            if (!fData.HasMemberPermission(pData.Info, 5, true))
                return false;

            var vFData = fData.AllVehicles.GetValueOrDefault(vInfo);

            if (vFData == null)
                return false;

            var curTime = Utils.GetCurrentTime();

            var targetTime = vFData.LastRespawnedTime.AddSeconds(Settings.FRACTION_VEHICLE_RESPAWN_CD);

            if (curTime < targetTime)
            {
                player.Notify("CDown::3", targetTime.Subtract(curTime).GetBeautyString());

                return false;
            }

            vFData.LastRespawnedTime = curTime;

            if (vInfo.VehicleData != null)
            {
                if (player.Vehicle == vInfo.VehicleData.Vehicle)
                    return false;

                vInfo.VehicleData.Delete(false);

                vInfo.Spawn();
            }
            else
            {
                vInfo.Spawn();
            }

            return true;
        }

        [RemoteProc("Fraction::MRC")]
        private static bool MemberRankChange(Player player, uint cid, byte newRank)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            return false; // todo

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return false;
        }

        [RemoteProc("Fraction::MF")]
        private static bool MemberFire(Player player, uint cid)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            return false; // todo

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return false;
        }

        [RemoteProc("Fraction::SL")]
        private static bool StorageLock(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return false;

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return false;

            if (fData.ContainerLocked == state)
                return false;

            fData.SetStorageLocked(state, true);

            return true;
        }

        [RemoteProc("Fraction::CWBL")]
        private static bool CreationWorkbenchLock(Player player, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return false;

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return false;

            if (fData.CreationWorkbenchLocked == state)
                return false;

            fData.SetCreationWorkbenchLocked(state, true);

            return true;
        }

        [RemoteProc("Fraction::RE")]
        private static JObject RankEdit(Player player, byte rankToEdit)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return null;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return null;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return null;

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return null;

            if (rankToEdit >= pData.Info.FractionRank)
            {
                player.Notify("Fraction::RE0");

                return null;
            }

            if (rankToEdit >= fData.Ranks.Count)
                return null;

            return JObject.FromObject(fData.Ranks[rankToEdit].Permissions);
        }

        [RemoteProc("Fraction::RUN")]
        private static bool RankUpdateName(Player player, byte rankToEdit, string newName)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return false;

            // todo check if not leader, but max rank

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return false;

            if (rankToEdit >= fData.Ranks.Count)
                return false;

            if (rankToEdit >= pData.Info.FractionRank)
            {
                player.Notify("Fraction::RE0");

                return false;
            }

            if (newName == null)
                return false;

            newName = newName.Trim();

            if (!RankNamePattern.IsMatch(newName))
                return false;

            foreach (var x in fData.Ranks)
                if (x.Name == newName)
                    return false;

            fData.SetRankName(rankToEdit, newName, true);

            return true;
        }

        [RemoteProc("Fraction::RUP")]
        private static bool RankUpdatePermission(Player player, byte rankToEdit, uint permissionId, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null)
                return false;

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return false;

            if (rankToEdit >= fData.Ranks.Count)
                return false;

            if (rankToEdit >= pData.Info.FractionRank)
            {
                player.Notify("Fraction::RE0");

                return false;
            }

            var rankData = fData.Ranks[rankToEdit];

            byte curState;

            var stateN = (byte)(state ? 1 : 0);

            if (!rankData.Permissions.TryGetValue(permissionId, out curState) || curState == stateN)
                return false;

            rankData.Permissions[permissionId] = stateN;

            MySQL.FractionUpdateRanks(fData);

            return true;
        }

        [RemoteProc("Fraction::UNIFS")]
        private static int UniformShow(Player player, int fractionTypeNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return int.MinValue;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum))
                return int.MinValue;

            var fType = (Game.Fractions.Types)fractionTypeNum;

            if (!Game.Fractions.Fraction.IsMember(pData, fType, false))
                return int.MinValue;

            var fData = Game.Fractions.Fraction.Get(fType);

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return int.MinValue;

            if (fData is Game.Fractions.IUniformable fDataUnif)
            {
                if (fDataUnif.LockerRoomPosition.DistanceTo(player.Position) > 5f)
                    return int.MinValue;

                var curUnif = pData.CurrentUniform;

                if (curUnif == null)
                    return -1;

                for (int i = 0; i < fDataUnif.UniformTypes.Count; i++)
                {
                    if (curUnif == fDataUnif.UniformTypes[i])
                        return i;
                }

                return -1;
            }

            return int.MinValue;
        }

        [RemoteEvent("Fraction::UNIFC")]
        private static void UniformChange(Player player, int fractionTypeNum, int uniformIdx)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum))
                return;

            var fType = (Game.Fractions.Types)fractionTypeNum;

            var fData = Game.Fractions.Fraction.Get(fType);

            if (!Game.Fractions.Fraction.IsMember(pData, fType, true))
                return;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return;

            if (fData is Game.Fractions.IUniformable fDataUnif)
            {
                if (fDataUnif.LockerRoomPosition.DistanceTo(player.Position) > 5f)
                    return;

                if (uniformIdx < 0)
                {
                    Game.Data.Customization.SetNoUniform(pData);
                }
                else
                {
                    if (uniformIdx >= fDataUnif.UniformTypes.Count)
                        return;

                    Game.Data.Customization.SetNoUniform(pData);

                    Game.Data.Customization.ApplyUniform(pData, fDataUnif.UniformTypes[uniformIdx]);
                }
            }
        }

        [RemoteProc("Fraction::CWBS")]
        private static byte CreationWorkbenchShow(Player player, int fractionTypeNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return 0;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(Game.Fractions.Types), fractionTypeNum))
                return 0;

            var fType = (Game.Fractions.Types)fractionTypeNum;

            if (!Game.Fractions.Fraction.IsMember(pData, fType, true))
                return 0;

            var fData = Game.Fractions.Fraction.Get(fType);

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                return 0;

            if (fData.CreationWorkbenchPosition.Position.DistanceTo(player.Position) > fData.CreationWorkbenchPosition.RotationZ + 2.5f)
                return 0;

            if (fData.CreationWorkbenchLocked && !fData.HasMemberPermission(pData.Info, 1, false))
            {
                player.Notify("Fraction::CWL");

                return 0;
            }

            return byte.MaxValue;
        }

        [RemoteEvent("Fraction::CWBC")]
        private static void CreationWorkbenchCreate(Player player, string itemId, int amount)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;
        }

        [RemoteProc("Fraction::NEWSE")]
        private static bool NewsEdit(Player player, int newsId, string text)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return false;

            text = text?.Trim();

            if (text == null)
                return false;

            if (text.Length < Settings.FRACTION_NEWS_MIN_CHAR || text.Length > Settings.FRACTION_NEWS_MAX_CHAR || text.Where(x => x == '\n').Count() > Settings.FRACTION_NEWS_MAX_NL)
                return false;

            if (newsId < 0)
            {
                if (fData.News.All.Count >= Settings.FRACTION_NEWS_MAX_COUNT)
                {
                    player.Notify("Fraction::NEWSMC");

                    return false;
                }

                fData.AddNews(text, true);
            }
            else
            {
                var curText = fData.News.All.GetValueOrDefault(newsId);

                if (curText == null)
                {
                    player.Notify("Fraction::NEWSDE");

                    return false;
                }

                if (curText == text)
                    return false;

                fData.EditNews(newsId, text, true);
            }

            return true;
        }

        [RemoteProc("Fraction::NEWSP")]
        private static bool NewsPin(Player player, int newsId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return false;

            fData.PinNews(fData.News.All.ContainsKey(newsId) ? newsId : -1, true);

            return false;
        }

        [RemoteProc("Fraction::NEWSD")]
        private static bool NewsDelete(Player player, int newsId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (!fData.IsLeaderOrWarden(pData.Info, true))
                return false;

            if (!fData.DeleteNews(newsId, true))
                return false;

            return true;
        }
    }
}
