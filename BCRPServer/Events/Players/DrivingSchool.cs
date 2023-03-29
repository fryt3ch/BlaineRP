using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Events.Players
{
    internal class DrivingSchool : Script
    {
        [RemoteProc("DrSchool::CHL")]
        private static bool ChooseLicense(Player player, int schoolId, int licTypeNum, bool useCash)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (player.Dimension != Utils.Dimensions.Main || pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var school = Game.Autoschool.Get(schoolId);

            if (school == null)
                return false;

            if (!Enum.IsDefined(typeof(PlayerData.LicenseTypes), licTypeNum))
                return false;

            var licType = (PlayerData.LicenseTypes)licTypeNum;

            if (school.Position.DistanceTo(player.Position) > 10f)
                return false;

            uint price;

            if (!Game.Autoschool.Prices.TryGetValue(licType, out price))
                return false;

            if (pData.Licenses.Contains(licType))
            {
                player.Notify("DriveS::AHTL");

                return false;
            }

            if (player.HasData("DRSCHOOL::T::BT"))
                return false;

            if (pData.Info.Quests.ContainsKey(Sync.Quest.QuestData.Types.DRSCHOOL0))
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

            var school = Game.Autoschool.Get(schoolId);

            if (school == null)
                return;

            if (!player.HasData("DRSCHOOL::T::BT"))
                return;

            var licType = player.GetData<PlayerData.LicenseTypes>("DRSCHOOL::T::BT");

            player.ResetData("DRSCHOOL::T::BT");

            if (allAmount == 0)
            {
                player.Notify("DriveS::TTF0");

                return;
            }

            if (okAmount > allAmount)
                return;

            var minimalAnswers = (byte)Math.Floor(allAmount * (Game.Autoschool.MinimalOkAnswersTestPercentage / 100m));

            if (okAmount < minimalAnswers)
            {
                player.Notify("DriveS::TTF1", okAmount, minimalAnswers);

                return;
            }

            Sync.Quest.StartQuest(pData, Sync.Quest.QuestData.Types.DRSCHOOL0, 0, 0, $"{(int)licType}");

            return;
        }
    }
}
