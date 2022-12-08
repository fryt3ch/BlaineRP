using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using static BCRPServer.Game.Bank;

namespace BCRPServer.Events.Players
{
    class Bank : Script
    {

        [RemoteProc("Bank::Savings::ToDebitSett")]
        private static bool SavingsToDebitSetting(Player player, int bankId, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.BankAccount == null)
                return false;

            if (!IsPlayerNearBank(player, bankId))
                return false;

            if (pData.BankAccount.SavingsToDebit == state)
                return true;

            pData.BankAccount.SavingsToDebit = state;

            return true;
        }

        [RemoteProc("Bank::Debit::Send")]
        private static bool SendMoney(Player player, int bankId, uint cid, int amount, bool isRequest) // add check if player isn't near the bank, if by mobile (bankId < 0) - tax add
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.BankAccount == null || amount <= 0)
                return false;

            // if mobile
            if (bankId < 0)
            {

            }
            else
            {
                if (!IsPlayerNearBank(player, bankId))
                    return false;
            }

            var tInfo = PlayerData.PlayerInfo.Get(cid);

            if (tInfo == null || tInfo.CID == pData.CID)
            {
                player.Notify("Bank::TargetNotFound");

                return false;
            }

            if (tInfo.BankAccount == null)
            {
                player.Notify("Bank::NoAccountTarget");

                return false;
            }

            if (pData.BankAccount.Balance < amount)
            {
                player.Notify("Bank::NotEnough", pData.BankAccount.Balance);

                return false;
            }

            if (pData.BankAccount.TotalDayTransactions + (uint)amount > pData.BankAccount.Tariff.TransactionDayLimit)
            {
                player.Notify("Bank::DayLimitExceed");

                return false;
            }

            if (isRequest)
            {
                player.Notify("Bank::SendApprove", amount, tInfo.Name, tInfo.Surname);

                return true;
            }

            pData.BankAccount.SendMoney(tInfo, amount);

            return true;
        }

        [RemoteEvent("Bank::Debit::Operation")]
        private static void Operation(Player player, bool isAtm, int bankId, bool add, int amount) // add check if player isn't near the bank / atm
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.BankAccount == null || amount <= 0)
                return;

            if (isAtm)
            {
                if (!IsPlayerNearAtm(player, bankId))
                    return;
            }
            else
            {
                if (!IsPlayerNearBank(player, bankId))
                    return;
            }

            if (add)
            {
                if (!pData.AddCash(-amount, true))
                    return;

                pData.BankAccount.Deposit(amount);
            }
            else
            {
                if (pData.BankAccount.Balance < amount)
                {
                    player.Notify("Bank::NotEnough", pData.BankAccount.Balance);

                    return;
                }

                if (!pData.AddCash(amount, true))
                    return;

                pData.BankAccount.Withdraw(amount); // add atm tax
            }
        }

        [RemoteEvent("Bank::Tariff::Buy")]
        private static void BuyTariff(Player player, int bankId, int tariffNum) // add check if player isn't near the bank / atm
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!IsPlayerNearBank(player, bankId))
                return;

            if (!Enum.IsDefined(typeof(Tariff.Types), tariffNum))
                return;

            var tariffType = (Tariff.Types)tariffNum;

            var tariff = Tariff.All[tariffType];

            if (pData.BankAccount == null)
            {
                if (!pData.AddCash(-tariff.Price, true))
                    return;

                pData.BankAccount = new Account(pData.Info, tariffType);

                player.TriggerEvent("MenuBank::Show", bankId, tariffType, pData.BankAccount.Balance, pData.BankAccount.TotalDayTransactions, pData.BankAccount.SavingsBalance, pData.BankAccount.SavingsToDebit);
            }
            else
            {
                if (pData.BankAccount.Balance < tariff.Price)
                {
                    player.Notify("Bank::NotEnough", pData.BankAccount.Balance);

                    return;
                }

                pData.BankBalance -= tariff.Price;

                pData.BankAccount.Tariff = tariff;

                if (pData.BankAccount.SavingsBalance > tariff.MaxSavingsBalance)
                {
                    var diff = pData.BankAccount.SavingsBalance - tariff.MaxSavingsBalance;

                    pData.BankBalance += diff;
                    pData.BankAccount.SavingsBalance = tariff.MaxSavingsBalance;
                }

                player.TriggerEvent("MenuBank::Show", bankId, tariffNum, pData.BankAccount.Balance, pData.BankAccount.TotalDayTransactions, pData.BankAccount.SavingsBalance, pData.BankAccount.SavingsToDebit);
            }
        }

        [RemoteEvent("Bank::Show")]
        private static void Show(Player player, bool isAtm, int bankId) // add check if player isn't near the bank / atm
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (isAtm)
            {
                if (!IsPlayerNearAtm(player, bankId))
                    return;

                if (pData.BankAccount == null)
                {
                    player.Notify("Bank::NoAccount");

                    return;
                }
                else
                {
                    player.TriggerEvent("ATM::Show", bankId, ATM_TAX);

                    return;
                }
            }
            else
            {
                if (!IsPlayerNearBank(player, bankId))
                    return;

                if (pData.BankAccount == null)
                {
                    player.TriggerEvent("MenuBank::Show", bankId, -1);

                    return;
                }
                else
                {
                    player.TriggerEvent("MenuBank::Show", bankId, (int)pData.BankAccount.Tariff.Type, pData.BankAccount.Balance, pData.BankAccount.TotalDayTransactions, pData.BankAccount.SavingsBalance, pData.BankAccount.SavingsToDebit);

                    return;
                }
            }
        }

        [RemoteProc("Bank::HasAccount")]
        private static bool HasAccount(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            return pData.BankAccount != null;
        }
    }
}
