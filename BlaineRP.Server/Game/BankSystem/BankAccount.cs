using System;
using BlaineRP.Server.Extensions.System;
using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.BankSystem
{
    public class BankAccount
    {
        public PlayerInfo PlayerInfo { get; set; }

        /// <summary>Баланс</summary>
        public ulong Balance { get; set; }

        /// <summary>Баланс сбер. счета/summary>
        public ulong SavingsBalance { get; set; }

        /// <summary>Включено ли начисление процента со сбер.счета на дебебтовый счет?</summary>
        public bool SavingsToDebit { get; set; }

        /// <summary>Минимальный остаток на сбер. счете сегодня</summary>
        public ulong MinSavingsBalance { get; set; }

        /// <summary>Общая сумма переводов сегодня</summary>
        public ulong TotalDayTransactions { get; set; }

        /// <summary>Тариф</summary>
        public Tariff Tariff { get; set; }

        public BankAccount() { }

        public BankAccount(PlayerInfo PlayerInfo, Tariff.TariffType TariffType)
        {
            this.PlayerInfo = PlayerInfo;

            this.Tariff = Tariff.All[TariffType];

            MySQL.BankAccountAdd(this);
        }

        public bool TryAddMoneyDebit(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Balance.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMoneyDebit(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!Balance.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {
                    if (PlayerInfo.PlayerData != null)
                    {
                        PlayerInfo.PlayerData.Player.Notify("Bank::NotEnough", Balance);
                    }
                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMoneyDebitUseCashback(ulong value, out ulong newBalance, out ulong totalCashback, bool notifyOnFault = true, PlayerData tData = null)
        {
            totalCashback = Tariff.GetCashback(value);

            return TryRemoveMoneyDebit(value - totalCashback, out newBalance, notifyOnFault, tData);
        }

        public void SetDebitBalance(ulong value, string reason)
        {
            if (PlayerInfo.PlayerData != null)
            {
                PlayerInfo.PlayerData.BankBalance = value;
            }
            else
            {
                Balance = value;
            }

            MySQL.BankAccountBalancesUpdate(this);
        }

        public bool TryAddMoneySavings(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!SavingsBalance.TryAdd(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public bool TryRemoveMoneySavings(ulong amount, out ulong newBalance, bool notifyOnFault = true, PlayerData tData = null)
        {
            if (!SavingsBalance.TrySubtract(amount, out newBalance))
            {
                if (notifyOnFault)
                {

                }

                return false;
            }

            return true;
        }

        public void SetSavingsBalance(ulong value, string reason)
        {
            SavingsBalance = value;

            MySQL.BankAccountBalancesUpdate(this);
        }

        public bool HasEnoughTransactionLimit(ulong amount, bool notifyOnFault = true)
        {
            if (Tariff.TransactionDayLimit == 0)
                return true;

            ulong newLimitSum;

            if (!TotalDayTransactions.TryAdd(amount, out newLimitSum))
            {
                return false;
            }

            if (newLimitSum > Tariff.TransactionDayLimit)
            {
                if (notifyOnFault)
                    PlayerInfo.PlayerData?.Player.Notify("Bank::DayLimitExceed");

                return false;
            }

            return true;
        }

        public void GiveSavingsBenefit()
        {
            var totalMoney = MinSavingsBalance > Tariff.MaxSavingsBalance ? Tariff.MaxSavingsBalance : MinSavingsBalance;

            var toGive = (ulong)Math.Round(totalMoney * Tariff.SavingsPercentage);

            if (toGive == 0)
                return;

            if (SavingsToDebit)
            {
                ulong newBalance;

                if (!TryAddMoneyDebit(toGive, out newBalance, true))
                    return;

                SetDebitBalance(newBalance, null);
            }
            else
            {
                ulong newSavingsBalance;

                if (!TryAddMoneySavings(toGive, out newSavingsBalance, true))
                    return;

                SetSavingsBalance(newSavingsBalance, null);
            }

            MinSavingsBalance = SavingsBalance;
        }
    }
}