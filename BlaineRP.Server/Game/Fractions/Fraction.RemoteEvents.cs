using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.EntitiesData.Vehicles;
using BlaineRP.Server.Extensions.System;
using GTANetworkAPI;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.Fractions
{
    public abstract partial class Fraction
    {
        internal class RemoteEvents : Script
        {
            private static Regex RankNamePattern = new Regex(@"^[a-zA-Zа-яА-Я0-9\s]{2,12}$", RegexOptions.Compiled);

            [RemoteProc("Fraction::GMSD")]
            private static string GetFractionMenuServerData(Player player)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                    return null;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                return $"{fData.Balance}";
            }

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

                var vInfo = VehicleInfo.Get(vid);

                if (vInfo == null)
                    return null;

                if (!fData.AllVehicles.ContainsKey(vInfo))
                    return null;

                if (vInfo.VehicleData == null || vInfo.VehicleData.Vehicle.Dimension != Properties.Settings.Static.MainDimension)
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

                var vInfo = VehicleInfo.Get(vid);

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

                var vInfo = VehicleInfo.Get(vid);

                if (vInfo == null)
                    return false;

                if (!fData.HasMemberPermission(pData.Info, 6, true))
                    return false;

                var vFData = fData.AllVehicles.GetValueOrDefault(vInfo);

                if (vFData == null)
                    return false;

                var curTime = Utils.GetCurrentTime();

                var cdTimeDiff = vFData.LastRespawnedTime.Add(Game.Fractions.Fraction.VehicleRespawnCooldownTime).Subtract(curTime);

                if (cdTimeDiff.TotalSeconds > 0)
                {
                    player.NotifyError(Language.Strings.Get("NTFC_COOLDOWN_GEN_2", cdTimeDiff.GetBeautyString()));

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

                if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
                    return false;

                if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                    return false;

                var fData = Game.Fractions.Fraction.Get(pData.Fraction);

                if (fData == null)
                    return false;

                if (newRank >= fData.Ranks.Count)
                    newRank = (byte)(fData.Ranks.Count - 1);

                if (cid == pData.CID)
                    return false;

                var tInfo = PlayerInfo.Get(cid);

                if (tInfo == null)
                    return false;

                if (tInfo.Fraction != fData.Type || tInfo.FractionRank == newRank)
                    return false;

                var rankUp = false;

                if (tInfo.FractionRank < newRank)
                {
                    if (!fData.HasMemberPermission(pData.Info, 4, true))
                        return false;

                    rankUp = true;
                }
                else
                {
                    if (!fData.HasMemberPermission(pData.Info, 3, true))
                        return false;
                }

                if (tInfo.FractionRank >= pData.Info.FractionRank)
                {
                    player.Notify("Fraction::HRIBTY");

                    return false;
                }

                if (newRank >= pData.Info.FractionRank)
                {
                    player.Notify("Fraction::CSTR");

                    return false;
                }

                fData.SetPlayerRank(tInfo, newRank);

                if (tInfo.PlayerData != null)
                {
                    if (rankUp)
                    {
                        tInfo.PlayerData.Player.Notify("Fraction::RU", $"{player.Name} ({player.Id}) #{pData.CID}", $"{fData.Ranks[newRank].Name} - {newRank + 1}");
                    }
                    else
                    {
                        tInfo.PlayerData.Player.Notify("Fraction::RD", $"{player.Name} ({player.Id}) #{pData.CID}", $"{fData.Ranks[newRank].Name} - {newRank + 1}");
                    }
                }

                return true;
            }

            [RemoteProc("Fraction::MF")]
            private static bool MemberFire(Player player, uint cid)
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

                if (!fData.HasMemberPermission(pData.Info, 5, true))
                    return false;

                if (cid == pData.CID)
                    return false;

                var tInfo = PlayerInfo.Get(cid);

                if (tInfo == null)
                    return false;

                if (tInfo.Fraction != fData.Type)
                    return false;

                if (pData.Info.FractionRank <= tInfo.FractionRank)
                {
                    player.Notify("Fraction::HRIBTY");

                    return false;
                }

                fData.SetPlayerNoFraction(tInfo);

                if (tInfo.PlayerData != null)
                {
                    tInfo.PlayerData.Player.Notify("Fraction::F", $"{player.Name} ({player.Id}) #{pData.CID}");
                }

                return true;
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

                /*            if (rankToEdit >= pData.Info.FractionRank)
                        {
                            player.Notify("Fraction::RE0");

                            return null;
                        }*/

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
            private static int UniformShow(Player player, int fractionTypeNum, byte lockerIdx)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return int.MinValue;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return int.MinValue;

                var fType = (Game.Fractions.FractionType)fractionTypeNum;

                if (!Game.Fractions.Fraction.IsMember(pData, fType, false))
                    return int.MinValue;

                var fData = Game.Fractions.Fraction.Get(fType);

                if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                    return int.MinValue;

                if (fData is Game.Fractions.IUniformable fDataUnif)
                {
                    var pos = fDataUnif.GetLockerPosition(lockerIdx);

                    if (pos == null)
                        return int.MinValue;

                    if (pos.DistanceTo(player.Position) > 5f)
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
            private static void UniformChange(Player player, int fractionTypeNum, byte lockerIdx, int uniformIdx)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return;

                var fType = (Game.Fractions.FractionType)fractionTypeNum;

                var fData = Game.Fractions.Fraction.Get(fType);

                if (!Game.Fractions.Fraction.IsMember(pData, fType, true))
                    return;

                if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                    return;

                if (fData is Game.Fractions.IUniformable fDataUnif)
                {
                    var pos = fDataUnif.GetLockerPosition(lockerIdx);

                    if (pos == null)
                        return;

                    if (pos.DistanceTo(player.Position) > 5f)
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
            private static byte CreationWorkbenchShow(Player player, int fractionTypeNum, byte wbIdx)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return 0;

                var fType = (Game.Fractions.FractionType)fractionTypeNum;

                if (!Game.Fractions.Fraction.IsMember(pData, fType, true))
                    return 0;

                var fData = Game.Fractions.Fraction.Get(fType);

                if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                    return 0;

                var pos = fData.GetCreationWorkbenchPosition(wbIdx);

                if (pos == null || pos.Position.DistanceTo(player.Position) > pos.RotationZ + 2.5f)
                    return 0;

                if (fData.CreationWorkbenchLocked && !fData.HasMemberPermission(pData.Info, 1, false))
                {
                    player.Notify("Fraction::CWL");

                    return 0;
                }

                return byte.MaxValue;
            }

            [RemoteProc("Fraction::CWBC")]
            private static byte CreationWorkbenchCreate(Player player, int fractionTypeNum, byte wbIdx, string itemId, int amount)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return 0;

                var pData = sRes.Data;

                if (itemId == null || amount <= 0)
                    return 0;

                if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
                    return 0;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return 0;

                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)fractionTypeNum);

                if (fData == null)
                    return 0;

                if (!Game.Fractions.Fraction.IsMember(pData, fData.Type, true))
                    return 0;

                var pos = fData.GetCreationWorkbenchPosition(wbIdx);

                if (pos == null || pos.Position.DistanceTo(player.Position) > pos.RotationZ + 2.5f)
                    return 0;

                if (fData.CreationWorkbenchLocked && !fData.HasMemberPermission(pData.Info, 1, false))
                {
                    player.Notify("Fraction::CWL");

                    return 0;
                }

                uint price;

                if (!fData.CreationWorkbenchPrices.TryGetValue(itemId, out price))
                    return 0;

                try
                {
                    price *= (uint)amount;

                    uint newBalance;

                    if (!fData.TryRemoveMaterials(price, out newBalance, true, pData))
                        return 0;

                    Game.Items.Item item;

                    if (!pData.GiveItem(out item, itemId, 0, amount, true, true))
                        return 0;

                    if (fData.ItemTag != null)
                    {
                        if (item is Game.Items.Weapon weapon)
                        {
                            weapon.Tag = $"{fData.ItemTag}@{weapon.UID}";

                            weapon.Update();
                        }
                    }

                    fData.SetMaterials(newBalance, true);

                    return byte.MaxValue;
                }
                catch (Exception ex)
                {
                    return 0;
                }
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

                if (text.Length < Properties.Settings.Static.FRACTION_NEWS_MIN_CHAR || text.Length > Properties.Settings.Static.FRACTION_NEWS_MAX_CHAR || text.Where(x => x == '\n').Count() > Properties.Settings.Static.FRACTION_NEWS_MAX_NL)
                    return false;

                if (newsId < 0)
                {
                    if (fData.News.All.Count >= Properties.Settings.Static.FRACTION_NEWS_MAX_COUNT)
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
}