using System;
using System.Collections.Generic;

namespace BlaineRP.Server.Game.BankSystem
{
    public partial class Tariff
    {
        public static Dictionary<TariffType, Tariff> All { get; private set; } = new Dictionary<TariffType, Tariff>()
        {
            { TariffType.Standart, new Tariff(TariffType.Standart, 25_000, 1_000_000, 0.7m, 10000, 0.05m, 5000) },

            { TariffType.StandartPlus, new Tariff(TariffType.StandartPlus, 50_000, 2_400_000, 1.5m, 100_000, 0.1m, 10_000) },

            { TariffType.Supreme, new Tariff(TariffType.Supreme, 125_000, 500_000, 3.2m, 700_000, 0.15m, 15_000) },
        };

        public TariffType Type { get; set; }

        /// <summary>Стоимость тарифа</summary>
        public uint Price { get; set; }

        /// <summary>Максимальный баланс сбер. счета</summary>
        public uint MaxSavingsBalance { get; set; }

        /// <summary>Лимит переводов в сутки</summary>
        public uint TransactionDayLimit { get; set; }

        /// <summary>Процент кэшбека</summary>
        public decimal Cashback { get; set; }

        /// <summary>Максимальный кэшбек за покупку</summary>
        public uint MaxCashback { get; set; }

        /// <summary>Процент сбер. счета/summary>
        public decimal SavingsPercentage { get; set; }

        public Tariff(TariffType Type, uint Price, uint MaxSavingsBalance, decimal SavingsPercentage, uint TransactionDayLimit, decimal Cashback, uint MaxCashback)
        {
            this.Type = Type;

            this.Price = Price;

            this.MaxSavingsBalance = MaxSavingsBalance;
            this.SavingsPercentage = SavingsPercentage;

            this.TransactionDayLimit = TransactionDayLimit;

            this.Cashback = Cashback;
            this.MaxCashback = MaxCashback;
        }

        public ulong GetCashback(ulong value)
        {
            if (Cashback <= 0)
                return 0;

            var cb = (ulong)Math.Floor(value * Cashback);

            if (cb >= value)
                return 0;

            if (cb > MaxCashback)
                return MaxCashback;

            return cb;
        }
    }
}