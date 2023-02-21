using GTANetworkAPI;
using System;
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
        private static bool SendMoney(Player player, int bankId, uint cid, int amountI, bool isRequest) // add check if player isn't near the bank, if by mobile (bankId < 0) - tax add
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return false;

            var pData = sRes.Data;

            if (pData.BankAccount == null || amountI <= 0)
                return false;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return false;

            var amount = (ulong)amountI;

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

            if (tInfo == null || tInfo == pData.Info)
            {
                player.Notify("Bank::TargetNotFound");

                return false;
            }

            if (tInfo.BankAccount == null)
            {
                player.Notify("Bank::NoAccountTarget");

                return false;
            }

            ulong newBalanceP;

            if (!pData.BankAccount.TryRemoveMoneyDebit(amount, out newBalanceP, true))
                return false;

            if (!pData.BankAccount.HasEnoughTransactionLimit(amount))
                return false;

            if (isRequest)
            {
                if (bankId < 0)
                    player.Notify("Bank::SendApproveP", amount, tInfo.Name, tInfo.Surname, MobileSendTaxClientVisual);
                else
                    player.Notify("Bank::SendApprove", amount, tInfo.Name, tInfo.Surname);

                return true;
            }

            ulong newBalanceT;

            if (!tInfo.BankAccount.TryAddMoneyDebit(bankId < 0 ? (ulong)Math.Floor(amount * (1 - Game.Bank.MOBILE_SEND_TAX)) : amount, out newBalanceT))
                return false;

            pData.BankAccount.TotalDayTransactions += amount;

            pData.BankAccount.SetDebitBalance(newBalanceP, $"#{pData.CID} SENT ${amount} TO #{tInfo.CID}");
            tInfo.BankAccount.SetDebitBalance(newBalanceT, $"#{tInfo.CID} GOT ${amount} FROM #{pData.CID}");

            return true;
        }

        [RemoteEvent("Bank::Debit::Operation")]
        private static void Operation(Player player, bool isAtm, int bankId, bool add, int amountI)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (pData.BankAccount == null || amountI <= 0)
                return;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return;

            var amount = (ulong)amountI;

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
                ulong newCash;

                if (!pData.TryRemoveCash(amount, out newCash, true))
                    return;

                ulong newBalance;

                if (!pData.BankAccount.TryAddMoneyDebit(amount, out newBalance, true))
                    return;

                pData.SetCash(newCash);
                pData.BankAccount.SetDebitBalance(newBalance, null);
            }
            else
            {
                ulong newBalance;

                if (!pData.BankAccount.TryRemoveMoneyDebit(amount, out newBalance, true))
                    return;

                ulong newCash;

                if (!pData.TryAddCash(isAtm ? (ulong)Math.Floor(amount * (1 - Game.Bank.ATM_TAX)) : amount, out newCash, true))
                    return;

                pData.BankAccount.SetDebitBalance(newBalance, null);
                pData.SetCash(newCash);
            }
        }

        [RemoteEvent("Bank::Tariff::Buy")]
        private static void BuyTariff(Player player, int bankId, int tariffNum)
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
                ulong newCash;

                if (!pData.TryRemoveCash(tariff.Price, out newCash, true))
                    return;

                pData.SetCash(newCash);

                pData.BankAccount = new Account(pData.Info, tariffType);

                player.TriggerEvent("MenuBank::Show", bankId, tariffType, pData.BankAccount.Balance, pData.BankAccount.TotalDayTransactions, pData.BankAccount.SavingsBalance, pData.BankAccount.SavingsToDebit);
            }
            else
            {
                ulong newBalance;

                if (!pData.BankAccount.TryRemoveMoneyDebit(tariff.Price, out newBalance, true, null))
                    return;

                pData.BankAccount.SetDebitBalance(newBalance, $"#{pData.CID} BUY Tariff.{tariffType}");

                pData.BankAccount.Tariff = tariff;

                MySQL.BankAccountTariffUpdate(pData.BankAccount);

                player.TriggerEvent("MenuBank::Show", bankId, tariffNum, pData.BankAccount.Balance, pData.BankAccount.TotalDayTransactions, pData.BankAccount.SavingsBalance, pData.BankAccount.SavingsToDebit);
            }
        }

        [RemoteEvent("Bank::Show")]
        private static void Show(Player player, bool isAtm, int bankId)
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
                    player.TriggerEvent("ATM::Show", bankId, (float)ATM_TAX);

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

            return $"{house.Balance}_{Settings.MAX_PAID_HOURS_HOUSE_APS}_{Settings.MIN_PAID_HOURS_HOUSE_APS}";
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

            return $"{aps.Balance}_{Settings.MAX_PAID_HOURS_HOUSE_APS}_{Settings.MIN_PAID_HOURS_HOUSE_APS}";
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

        [RemoteProc("Bank::BBC")]
        private static ulong? BusinessBalanceChange(Player player, int businessId, int bankId, int amountI, bool useCash, bool add)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            if (amountI <= 0)
                return null;

            var amount = (ulong)amountI;

            if (pData.BankAccount == null)
                return null;

            var business = Game.Businesses.Business.Get(businessId);

            if (business == null || business.Owner != pData.Info)
                return null;

            if (bankId >= 0)
            {
                if (!IsPlayerNearBank(player, bankId))
                    return null;
            }
            else
            {
                if (!add || useCash)
                    return null;
            }

            if (add)
            {
                ulong newBalance;

                if (!business.TryAddMoneyBank(amount, out newBalance, true))
                    return null;

                var maxHours = Settings.MAX_PAID_HOURS_BUSINESS;

                if (maxHours > 0 && (business.Rent * maxHours < newBalance))
                    return null;

                if (useCash)
                {
                    ulong newCash;

                    if (!pData.TryRemoveCash(amount, out newCash, true))
                        return null;

                    pData.SetCash(newCash);
                }
                else
                {
                    ulong newBankBalance;

                    if (!pData.BankAccount.TryRemoveMoneyDebit(amount, out newBankBalance, true))
                        return null;

                    pData.BankAccount.SetDebitBalance(newBankBalance, null);
                }

                business.SetBank(newBalance);
            }
            else
            {
                ulong newBalance;

                if (!business.TryRemoveMoneyBank(amount, out newBalance, true))
                    return null;

                if (business.Rent * Settings.MIN_PAID_HOURS_BUSINESS > newBalance)
                    return null;

                if (useCash)
                {
                    ulong newCash;

                    if (!pData.TryAddCash(amount, out newCash, true))
                        return null;

                    pData.SetCash(newCash);
                }
                else
                {
                    ulong newBankBalance;

                    if (!pData.BankAccount.TryAddMoneyDebit(amount, out newBankBalance, true))
                        return null;

                    pData.BankAccount.SetDebitBalance(newBankBalance, null);
                }

                business.SetBank(newBalance);
            }

            MySQL.BusinessUpdateBalances(business, false);

            return business.Bank;
        }

        [RemoteProc("Bank::HBC")]
        private static ulong? HouseBalanceChange(Player player, bool isHouse, uint houseId, int bankId, int amountI, bool useCash, bool add)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            if (amountI <= 0)
                return null;

            var amount = (ulong)amountI;

            if (pData.BankAccount == null)
                return null;

            var house = isHouse ? (Game.Estates.HouseBase)Game.Estates.House.Get(houseId) : (Game.Estates.HouseBase)Game.Estates.Apartments.Get(houseId);

            if (house == null || house.Owner != pData.Info)
                return null;

            if (bankId >= 0)
            {
                if (!IsPlayerNearBank(player, bankId))
                    return null;
            }
            else
            {
                if (!add || useCash)
                    return null;
            }

            if (add)
            {
                ulong newBalance;

                if (!house.TryAddMoneyBalance(amount, out newBalance, true))
                    return null;

                var maxHours = Settings.MAX_PAID_HOURS_HOUSE_APS;

                if (maxHours > 0 && ((uint)(house.Tax * Settings.MAX_PAID_HOURS_HOUSE_APS) < newBalance))
                    return null;

                if (useCash)
                {
                    ulong newCash;

                    if (!pData.TryRemoveCash(amount, out newCash, true))
                        return null;

                    pData.SetCash(newCash);
                }
                else
                {
                    ulong newBankBalance;

                    if (!pData.BankAccount.TryRemoveMoneyDebit(amount, out newBankBalance, true))
                        return null;

                    pData.BankAccount.SetDebitBalance(newBankBalance, null);
                }

                house.SetBalance(newBalance, null);
            }
            else
            {
                ulong newBalance;

                if (!house.TryRemoveMoneyBalance(amount, out newBalance, true))
                    return null;

                if ((uint)(house.Tax * Settings.MIN_PAID_HOURS_HOUSE_APS) > newBalance)
                    return null;

                if (useCash)
                {
                    ulong newCash;

                    if (!pData.TryAddCash(amount, out newCash, true))
                        return null;

                    pData.SetCash(newCash);
                }
                else
                {
                    ulong newBankBalance;

                    if (!pData.BankAccount.TryAddMoneyDebit(amount, out newBankBalance, true))
                        return null;

                    pData.BankAccount.SetDebitBalance(newBankBalance, null);
                }

                house.SetBalance(newBalance, null);
            }

            MySQL.HouseUpdateBalance(house);

            return house.Balance;
        }

        [RemoteProc("Bank::GBC")]
        private static ulong? GarageBalanceChange(Player player, uint garageId, int bankId, int amountI, bool useCash, bool add)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                return null;

            if (amountI <= 0)
                return null;

            var amount = (ulong)amountI;

            if (pData.BankAccount == null)
                return null;

            var garage = Game.Estates.Garage.Get(garageId);

            if (garage == null || garage.Owner != pData.Info)
                return null;

            if (bankId >= 0)
            {
                if (!IsPlayerNearBank(player, bankId))
                    return null;
            }
            else
            {
                if (!add || useCash)
                    return null;
            }

            if (add)
            {
                ulong newBalance;

                if (!garage.TryAddMoneyBalance(amount, out newBalance, true))
                    return null;

                var maxHours = Settings.MAX_PAID_HOURS_GARAGE;

                if (maxHours > 0 && ((uint)(garage.Tax * Settings.MAX_PAID_HOURS_GARAGE) < newBalance))
                    return null;

                if (useCash)
                {
                    ulong newCash;

                    if (!pData.TryRemoveCash(amount, out newCash, true))
                        return null;

                    pData.SetCash(newCash);
                }
                else
                {
                    ulong newBankBalance;

                    if (!pData.BankAccount.TryRemoveMoneyDebit(amount, out newBankBalance, true))
                        return null;

                    pData.BankAccount.SetDebitBalance(newBankBalance, null);
                }

                garage.SetBalance(newBalance, null);
            }
            else
            {
                ulong newBalance;

                if (!garage.TryRemoveMoneyBalance(amount, out newBalance, true))
                    return null;

                if ((uint)(garage.Tax * Settings.MIN_PAID_HOURS_GARAGE) > newBalance)
                    return null;

                if (useCash)
                {
                    ulong newCash;

                    if (!pData.TryAddCash(amount, out newCash, true))
                        return null;

                    pData.SetCash(newCash);
                }
                else
                {
                    ulong newBankBalance;

                    if (!pData.BankAccount.TryAddMoneyDebit(amount, out newBankBalance, true))
                        return null;

                    pData.BankAccount.SetDebitBalance(newBankBalance, null);
                }

                garage.SetBalance(newBalance, null);
            }

            return garage.Balance;
        }

        [RemoteProc("Bank::PAGD")]
        private static string PhoneAppGetData(Player player)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return null;

            var pData = sRes.Data;

            if (!pData.HasBankAccount(true))
                return null;

            return $"{(int)pData.BankAccount.Tariff.Type}_{pData.BankAccount.SavingsBalance}";
        }
    }
}
