using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;

namespace BCRPServer
{
    public class Bank : Script
    {
        private const float ATM_TAX = 1.2f;

        private const float MOBILE_SEND_TAX = 1.25f;

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
                Balance += amount;
            }

            public void Withdraw(int amount)
            {
                Balance -= amount;
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

        [RemoteEvent("Bank::Savings::ToDebitSett")]
        private static void SavingsToDebitSetting(Player player, int bankId, bool state)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.BankAccount == null)
                return;

            if (pData.BankAccount.SavingsToDebit == state)
                return;

            pData.BankAccount.SavingsToDebit = state;

            player.TriggerEvent("MenuBank::ToDebitSett", state);
        }

        [RemoteProc("Bank::Debit::Send")]
        private static bool SendMoney(Player player, bool isAtm, int bankId, uint cid, int amount, bool isRequest) // add check if player isn't near the bank / atm, if by mobile - tax add
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
                if (pData.BankAccount == null)
                {
                    player.TriggerEvent("MenuBank::Show", bankId, - 1);

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
