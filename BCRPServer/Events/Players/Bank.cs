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

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
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

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
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

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
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

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

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

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

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

        [RemoteProc("Bank::GHA")]
        private static string GetHouseAccountData(Player player, uint houseId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            var house = Game.Estates.House.Get(houseId);

            if (house == null || house.Owner != pData.Info)
                return null;

            return $"{house.Balance}_{Settings.MAX_PAID_HOURS_HOUSE}_{Settings.MIN_PAID_HOURS_HOUSE}";
        }

        [RemoteProc("Bank::GAA")]
        private static string GetApartmentsAccountData(Player player, uint houseId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            var aps = Game.Estates.Apartments.Get(houseId);

            if (aps == null || aps.Owner != pData.Info)
                return null;

            return $"{aps.Balance}_{Settings.MAX_PAID_HOURS_APARTMENTS}_{Settings.MIN_PAID_HOURS_APARTMENTS}";
        }

        [RemoteProc("Bank::GGA")]
        private static string GetGarageAccountData(Player player, uint houseId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            var garge = Game.Estates.Garage.Get(houseId);

            if (garge == null || garge.Owner != pData.Info)
                return null;

            return $"{garge.Balance}_{Settings.MAX_PAID_HOURS_GARAGE}_{Settings.MIN_PAID_HOURS_GARAGE}";
        }

        [RemoteProc("Bank::GBA")]
        private static string GetBusinessAccountData(Player player, int businessId)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            var business = Game.Businesses.Business.Get(businessId);

            if (business == null || business.Owner != pData.Info)
                return null;

            return $"{business.Bank}_{Settings.MAX_PAID_HOURS_BUSINESS}_{Settings.MIN_PAID_HOURS_BUSINESS}";
        }

        [RemoteProc("Bank::HBC")]
        private static int HouseBalanceChange(Player player, uint houseId, int amount, bool useCash, bool add)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return -1;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return -1;

            if (amount <= 0)
                return -1;

            if (pData.BankAccount == null)
                return -1;

            var house = Game.Estates.House.Get(houseId);

            if (house == null || house.Owner != pData.Info)
                return -1;

            if (add)
            {
                var newAmount = house.Balance + amount;

                if (house.Tax * Settings.MAX_PAID_HOURS_HOUSE < newAmount)
                    return -1;

                if (useCash)
                {
                    if (!pData.HasEnoughCash(amount, true))
                        return -1;

                    pData.AddCash(-amount, true);
                }
                else
                {
                    if (pData.BankAccount.HasEnoughMoneyDebit(amount, false, true) < 0)
                        return -1;

                    pData.BankAccount.Withdraw(amount);
                }

                house.Balance = newAmount;
            }
            else
            {
                var newAmount = house.Balance - amount;

                if (newAmount <= 0)
                {
                    return -1;
                }

                if (house.Tax * Settings.MIN_PAID_HOURS_HOUSE > newAmount)
                    return -1;

                house.Balance = newAmount;

                if (useCash)
                {
                    pData.AddCash(amount, true);
                }
                else
                {
                    pData.BankAccount.Deposit(amount);
                }
            }

            MySQL.HouseUpdateBalance(house);

            return house.Balance;
        }
    }
}
