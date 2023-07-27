using System;
using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.BankSystem
{
    public partial class Bank
    {
        private class RemoteEvents : Script
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

                var tInfo = PlayerInfo.Get(cid);

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

                if (!tInfo.BankAccount.TryAddMoneyDebit(bankId < 0 ? (ulong)Math.Floor(amount * (1 - Game.BankSystem.Bank.MOBILE_SEND_TAX)) : amount, out newBalanceT))
                    return false;

                pData.BankAccount.TotalDayTransactions += amount;

                pData.BankAccount.SetDebitBalance(newBalanceP, $"#{pData.CID} SENT ${amount} TO #{tInfo.CID}");
                tInfo.BankAccount.SetDebitBalance(newBalanceT, $"#{tInfo.CID} GOT ${amount} FROM #{pData.CID}");

                return true;
            }

            [RemoteEvent("Bank::Debit::Operation")]
            private static void DebitOperation(Player player, bool isAtm, int bankId, bool add, int amountI)
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

                    if (!pData.TryAddCash(isAtm ? (ulong)Math.Floor(amount * (1 - Game.BankSystem.Bank.ATM_TAX)) : amount, out newCash, true))
                        return;

                    pData.BankAccount.SetDebitBalance(newBalance, null);
                    pData.SetCash(newCash);
                }
            }

            [RemoteProc("Bank::Savings::Operation")]
            private static object SavingsOperation(Player player, int bankId, bool add, int amountI)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (pData.BankAccount == null || amountI <= 0)
                    return null;

                if (pData.IsKnocked || pData.IsCuffed || pData.IsFrozen)
                    return null;

                var amount = (ulong)amountI;

                if (!IsPlayerNearBank(player, bankId))
                    return null;

                ulong newBalanceSavings;
                ulong newBalanceDebit;

                if (add)
                {
                    if (!pData.BankAccount.TryRemoveMoneyDebit(amount, out newBalanceDebit, true))
                        return null;

                    if (pData.BankAccount.SavingsBalance + amount > pData.BankAccount.Tariff.MaxSavingsBalance)
                    {
                        amount = pData.BankAccount.SavingsBalance - pData.BankAccount.Tariff.MaxSavingsBalance;

                        if (amount <= 0)
                        {
                            player.Notify("Bank::MSB");

                            return null;
                        }
                    }

                    if (!pData.BankAccount.TryAddMoneySavings(amount, out newBalanceSavings, true))
                        return null;

                    pData.BankAccount.SetDebitBalance(newBalanceDebit, null);
                    pData.BankAccount.SetSavingsBalance(newBalanceSavings, null);
                }
                else
                {
                    if (!pData.BankAccount.TryRemoveMoneySavings(amount, out newBalanceSavings, true))
                        return null;

                    if (!pData.BankAccount.TryAddMoneyDebit(amount, out newBalanceDebit, true))
                        return null;

                    pData.BankAccount.SetSavingsBalance(newBalanceSavings, null);
                    pData.BankAccount.SetDebitBalance(newBalanceDebit, null);

                    if (newBalanceSavings < pData.BankAccount.MinSavingsBalance)
                        pData.BankAccount.MinSavingsBalance = newBalanceSavings;
                }

                return $"{newBalanceSavings}_{pData.BankAccount.MinSavingsBalance}";
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

                if (!Enum.IsDefined(typeof(Tariff.TariffType), tariffNum))
                    return;

                var tariffType = (Tariff.TariffType)tariffNum;

                var tariff = Tariff.All[tariffType];

                if (pData.BankAccount == null)
                {
                    ulong newCash;

                    if (!pData.TryRemoveCash(tariff.Price, out newCash, true))
                        return;

                    pData.SetCash(newCash);

                    pData.BankAccount = new BankAccount(pData.Info, tariffType);

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

                return $"{house.Balance}_{Properties.Settings.Static.MAX_PAID_HOURS_HOUSE_APS}_{Properties.Settings.Static.MIN_PAID_HOURS_HOUSE_APS}";
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

                return $"{aps.Balance}_{Properties.Settings.Static.MAX_PAID_HOURS_HOUSE_APS}_{Properties.Settings.Static.MIN_PAID_HOURS_HOUSE_APS}";
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

                return $"{garge.Balance}_{Properties.Settings.Static.MAX_PAID_HOURS_GARAGE}_{Properties.Settings.Static.MIN_PAID_HOURS_GARAGE}";
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

                return $"{business.Bank}_{Properties.Settings.Static.MAX_PAID_HOURS_BUSINESS}_{Properties.Settings.Static.MIN_PAID_HOURS_BUSINESS}";
            }

            [RemoteProc("Bank::GFA")]
            private static string GetFractionAccountData(Player player, int fractionTypeNum)
            {
                var sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                var pData = sRes.Data;

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return null;

                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)fractionTypeNum);

                if (fData == null)
                    return null;

                if (!Game.Fractions.Fraction.IsMember(pData, fData.Type, true))
                    return null;

                return $"{fData.Balance}";
            }

            [RemoteProc("Bank::FBC")]
            private static ulong? FractionBalanceChange(Player player, int fractionTypeNum, int bankId, int amountI, bool useCash, bool add)
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

                if (!Enum.IsDefined(typeof(Game.Fractions.FractionType), fractionTypeNum))
                    return null;

                var fData = Game.Fractions.Fraction.Get((Game.Fractions.FractionType)fractionTypeNum);

                if (fData == null)
                    return null;

                if (!Game.Fractions.Fraction.IsMember(pData, fData.Type, true))
                    return null;

                if (bankId >= 0)
                {
                    if (!IsPlayerNearBank(player, bankId))
                        return null;
                }
                else
                {
                    return null;
                }

                if (add)
                {
                    ulong newBalance;

                    if (!fData.TryAddMoney(amount, out newBalance, true))
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
                        if (!pData.HasBankAccount(true))
                            return null;

                        ulong newBankBalance;

                        if (!pData.BankAccount.TryRemoveMoneyDebit(amount, out newBankBalance, true))
                            return null;

                        pData.BankAccount.SetDebitBalance(newBankBalance, null);
                    }

                    fData.SetBalance(newBalance, true);
                }
                else
                {
                    if (!fData.IsLeaderOrWarden(pData.Info, true))
                        return null;

                    ulong newBalance;

                    if (!fData.TryRemoveMoney(amount, out newBalance, true))
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
                        if (!pData.HasBankAccount(true))
                            return null;

                        ulong newBankBalance;

                        if (!pData.BankAccount.TryAddMoneyDebit(amount, out newBankBalance, true))
                            return null;

                        pData.BankAccount.SetDebitBalance(newBankBalance, null);
                    }

                    fData.SetBalance(newBalance, true);

                    fData.TriggerEventToMembers("Chat::ShowServerMessage", $"[FRACTION] {pData.Player.Name} ({pData.Player.Id}) #{pData.CID} снял ${amount} со счёта фракции.");
                }
                return fData.Balance;
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

                    var maxHours = Properties.Settings.Static.MAX_PAID_HOURS_BUSINESS;

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
                        if (!pData.HasBankAccount(true))
                            return null;

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

                    if (business.Rent * Properties.Settings.Static.MIN_PAID_HOURS_BUSINESS > newBalance)
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
                        if (!pData.HasBankAccount(true))
                            return null;

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

                    var maxHours = Properties.Settings.Static.MAX_PAID_HOURS_HOUSE_APS;

                    if (maxHours > 0 && ((uint)(house.Tax * Properties.Settings.Static.MAX_PAID_HOURS_HOUSE_APS) < newBalance))
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
                        if (!pData.HasBankAccount(true))
                            return null;

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

                    if ((uint)(house.Tax * Properties.Settings.Static.MIN_PAID_HOURS_HOUSE_APS) > newBalance)
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
                        if (!pData.HasBankAccount(true))
                            return null;

                        ulong newBankBalance;

                        if (!pData.BankAccount.TryAddMoneyDebit(amount, out newBankBalance, true))
                            return null;

                        pData.BankAccount.SetDebitBalance(newBankBalance, null);
                    }

                    house.SetBalance(newBalance, null);
                }

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

                    var maxHours = Properties.Settings.Static.MAX_PAID_HOURS_GARAGE;

                    if (maxHours > 0 && ((uint)(garage.Tax * Properties.Settings.Static.MAX_PAID_HOURS_GARAGE) < newBalance))
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
                        if (!pData.HasBankAccount(true))
                            return null;

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

                    if ((uint)(garage.Tax * Properties.Settings.Static.MIN_PAID_HOURS_GARAGE) > newBalance)
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
                        if (!pData.HasBankAccount(true))
                            return null;

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
}