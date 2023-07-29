using System;
using System.Linq;

using BlaineRP.Server.Game.EntitiesData.Players;
using BlaineRP.Server.Game.Quests;
using BlaineRP.Server.Sync;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class DrivingSchool
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("DrSchool::CHL")]
            private static bool ChooseLicense(Player player, int schoolId, int licTypeNum, bool useCash)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return false;

                var pData = sRes.Data;

                if (player.Dimension != Properties.Settings.Static.MainDimension || pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return false;

                var school = Game.Misc.DrivingSchool.Get(schoolId);

                if (school == null)
                    return false;

                if (!Enum.IsDefined(typeof(LicenseType), licTypeNum))
                    return false;

                var licType = (LicenseType)licTypeNum;

                if (school.Position.DistanceTo(player.Position) > 10f)
                    return false;

                uint price;

                if (!Game.Misc.DrivingSchool.Prices.TryGetValue(licType, out price))
                    return false;

                if (pData.Info.Licenses.Contains(licType))
                {
                    player.Notify("DriveS::AHTL");

                    return false;
                }

                if (player.HasData("DRSCHOOL::T::BT"))
                    return false;

                if (pData.Info.Quests.ContainsKey(QuestType.DRSCHOOL0))
                {
                    player.Notify("DriveS::AHPT");

                    return false;
                }

                ulong newBalance;

                if (useCash)
                {
                    if (!pData.TryRemoveCash(price, out newBalance, true, null))
                        return false;

                    pData.SetCash(newBalance);
                }
                else
                {
                    if (!pData.HasBankAccount(true))
                        return false;

                    if (!pData.BankAccount.TryRemoveMoneyDebit(price, out newBalance, true, null))
                        return false;

                    pData.BankAccount.SetDebitBalance(newBalance, $"DrivingSchool_Test_{licType}");
                }

                player.SetData("DRSCHOOL::T::BT", licType);

                return true;
            }

            [RemoteEvent("DrSchool::PT")]
            private static void PassTest(Player player, int schoolId, byte okAmount, byte allAmount)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                var pData = sRes.Data;

                var school = Game.Misc.DrivingSchool.Get(schoolId);

                if (school == null)
                    return;

                if (!player.HasData("DRSCHOOL::T::BT"))
                    return;

                var licType = player.GetData<LicenseType>("DRSCHOOL::T::BT");

                player.ResetData("DRSCHOOL::T::BT");

                if (allAmount == 0)
                {
                    player.Notify("DriveS::TTF0");

                    return;
                }

                if (okAmount > allAmount)
                    return;

                var minimalAnswers = (byte)Math.Floor(allAmount * (Game.Misc.DrivingSchool.MinimalOkAnswersTestPercentage / 100m));

                if (okAmount < minimalAnswers)
                {
                    player.Notify("DriveS::TTF1", okAmount, minimalAnswers);

                    return;
                }

                Quest.StartQuest(pData, QuestType.DRSCHOOL0, 0, 0, $"{(int)licType}");

                return;
            }
        }
    }
}