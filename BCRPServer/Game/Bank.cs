using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Game
{
    public class Bank
    {
        public const float ATM_TAX = 1.2f;

        public const float MOBILE_SEND_TAX = 1.25f;

        private static Dictionary<int, Vector3> Banks = new Dictionary<int, Vector3>()
        {

        };

        private static Dictionary<int, Vector3> ATMs = new Dictionary<int, Vector3>()
        {

        };

        public class Account
        {
            public PlayerData.PlayerInfo PlayerInfo { get; set; }

            /// <summary>Баланс</summary>
            public int Balance { get; set; }

            /// <summary>Баланс сбер. счета/summary>
            public int SavingsBalance { get; set; }

            /// <summary>Включено ли начисление процента со сбер.счета на дебебтовый счет?</summary>
            public bool SavingsToDebit { get; set; }

            /// <summary>Минимальный остаток на сбер. счете сегодня</summary>
            public int MinSavingsBalance { get; set; }

            /// <summary>Общая сумма переводов сегодня</summary>
            public uint TotalDayTransactions { get; set; }

            /// <summary>Тариф</summary>
            public Tariff Tariff { get; set; }

            public Account() { }

            public Account(PlayerData.PlayerInfo PlayerInfo, Tariff.Types TariffType)
            {
                this.PlayerInfo = PlayerInfo;

                this.Tariff = Tariff.All[TariffType];

                MySQL.BankAccountAdd(this);
            }

            public void SendMoney(PlayerData.PlayerInfo pInfo, int amount)
            {
                if (PlayerInfo.PlayerData == null)
                {
                    PlayerInfo.BankAccount.Balance -= amount;
                }
                else
                {
                    PlayerInfo.PlayerData.BankBalance -= amount;
                }

                if (pInfo.PlayerData == null)
                {
                    pInfo.BankAccount.Balance += amount;
                }
                else
                {
                    pInfo.PlayerData.BankBalance += amount;
                }

                TotalDayTransactions += (uint)amount;

                MySQL.BankAccountUpdate(this);

                MySQL.BankAccountUpdate(pInfo.BankAccount);
            }

            public void Deposit(int amount)
            {
                if (PlayerInfo.PlayerData == null)
                {
                    Balance += amount;
                }
                else
                {
                    PlayerInfo.PlayerData.BankBalance += amount;
                }

                MySQL.BankAccountUpdate(this);
            }

            public void Withdraw(int amount)
            {
                if (PlayerInfo.PlayerData == null)
                {
                    Balance -= amount;
                }
                else
                {
                    PlayerInfo.PlayerData.BankBalance -= amount;
                }

                MySQL.BankAccountUpdate(this);
            }

            public void DepositSavings(int amount)
            {
                if (PlayerInfo.PlayerData == null)
                {
                    PlayerInfo.BankAccount.Balance -= amount;
                }
                else
                {
                    PlayerInfo.PlayerData.BankBalance -= amount;
                }

                SavingsBalance += amount;

                MySQL.BankAccountUpdate(this);
            }

            public void WithdrawSavings(int amount)
            {
                SavingsBalance -= amount;

                if (PlayerInfo.PlayerData == null)
                {
                    PlayerInfo.BankAccount.Balance += amount;
                }
                else
                {
                    PlayerInfo.PlayerData.BankBalance += amount;
                }

                MinSavingsBalance = SavingsBalance;

                MySQL.BankAccountUpdate(this);
            }

            public int GiveSavingsBenefit(int amount)
            {
                if (Tariff.SavingsPercentage > 0f)
                {
                    var toGive = (int)Math.Round(MinSavingsBalance * Tariff.SavingsPercentage);

                    if (toGive == 0)
                    {
                        return 0;
                    }

                    if (SavingsToDebit || SavingsBalance + toGive > Tariff.MaxSavingsBalance)
                    {
                        if (PlayerInfo.PlayerData == null)
                        {
                            PlayerInfo.BankAccount.Balance += amount;
                        }
                        else
                        {
                            PlayerInfo.PlayerData.BankBalance += amount;
                        }
                    }
                    else
                    {
                        SavingsBalance += toGive;
                    }

                    MinSavingsBalance = SavingsBalance;

                    MySQL.BankAccountUpdate(this);
                }

                return 0;
            }
        }

        public class Tariff
        {
            public static Dictionary<Types, Tariff> All { get; private set; } = new Dictionary<Types, Tariff>()
            {
                { Types.Standart, new Tariff(Types.Standart, 25_000, 1_000_000, 0.7f, 10000, 5f, 5000) },

                { Types.StandartPlus, new Tariff(Types.StandartPlus, 50_000, 2_400_000, 1.5f, 100_000, 10f, 10_000) },

                { Types.Supreme, new Tariff(Types.Supreme, 125_000, 500_000, 3.2f, 700_000, 15f, 15_000) },
            };

            public enum Types
            {
                /// <summary>Стандартный</summary>
                Standart = 0,
                /// <summary>Расширенный</summary>
                StandartPlus,
                /// <summary>Продвинутый</summary>
                Supreme,
            }

            public Types Type { get; set; }

            /// <summary>Стоимость тарифа</summary>
            public int Price { get; set; }

            /// <summary>Максимальный баланс сбер. счета</summary>
            public int MaxSavingsBalance { get; set; }

            /// <summary>Лимит переводов в сутки</summary>
            public int TransactionDayLimit { get; set; }

            /// <summary>Процент кэшбека</summary>
            public float Cashback { get; set; }

            /// <summary>Максимальный кэшбек за покупку</summary>
            public int MaxCashback { get; set; }

            /// <summary>Процент сбер. счета/summary>
            public float SavingsPercentage { get; set; }

            public Tariff(Types Type, int Price, int MaxSavingsBalance, float SavingsPercentage, int TransactionDayLimit, float Cashback, int MaxCashback)
            {
                this.Type = Type;

                this.Price = Price;

                this.MaxSavingsBalance = MaxSavingsBalance;
                this.SavingsPercentage = SavingsPercentage;

                this.TransactionDayLimit = TransactionDayLimit;

                this.Cashback = Cashback;
                this.MaxCashback = MaxCashback;
            }
        }
    }
}
