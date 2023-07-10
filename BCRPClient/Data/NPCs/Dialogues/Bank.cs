using BCRPClient.CEF;
using RAGE;
using RAGE.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using static BCRPClient.Data.Dialogue;

namespace BCRPClient.Data.NPCs.Dialogues
{
    public class Bank : Events.Script
    {
        public Bank()
        {
            new Dialogue("bank_preprocess", null,

                async (args) =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (NPC.CurrentNPC == null)
                        return;

                    var hasAccount = (bool?)await Events.CallRemoteProc("Bank::HasAccount") == true;

                    if (NPC.CurrentNPC == null)
                        return;

                    var btns = new List<Button>();

                    if (hasAccount)
                    {
                        btns.Add(new Button("[Перейти к управлению счетом]", () =>
                        {
                            if (NPC.CurrentNPC?.Data is Data.Locations.Bank bankData)
                            {
                                if (NPC.LastSent.IsSpam(1000, false, false))
                                    return;

                                Events.CallRemote("Bank::Show", false, bankData.Id);
                            }
                        }));
                    }
                    else
                    {
                        btns.Add(new Button("Да, хочу стать клиентом вашего банка", () => { NPC.CurrentNPC?.ShowDialogue("bank_no_account_1"); }));
                    }

                    if (pData.OwnedHouses.FirstOrDefault() is Data.Locations.House house)
                        btns.Add(new Button("[Счет дома]", () => { NPC.CurrentNPC?.ShowDialogue("bank_preprocess_house", false, new object[] { house }); }));

                    if (pData.OwnedApartments.FirstOrDefault() is Data.Locations.Apartments aps)
                        btns.Add(new Button("[Счет квартиры]", () => { NPC.CurrentNPC?.ShowDialogue("bank_preprocess_aps", false, new object[] { aps }); }));

                    if (pData.OwnedGarages.FirstOrDefault() is Data.Locations.Garage garage)
                        btns.Add(new Button("[Счет гаража]", () => { NPC.CurrentNPC?.ShowDialogue("bank_preprocess_garage", false, new object[] { garage }); }));

                    if (pData.OwnedBusinesses.FirstOrDefault() is Data.Locations.Business biz)
                        btns.Add(new Button("[Счет бизнеса]", () => { NPC.CurrentNPC?.ShowDialogue("bank_preprocess_business", false, new object[] { biz }); }));

                    if (pData.CurrentFraction is Data.Fractions.Fraction fraction)
                        btns.Add(new Button("[Счет фракции]", () => { NPC.CurrentNPC?.ShowDialogue("bank_preprocess_fraction", false, new object[] { fraction }); }));

                    btns.Add(Button.DefaultExitButton);

                    var dg = AllDialogues[hasAccount ? "bank_has_account" : "bank_no_account_0"];

                    dg.Buttons = btns;

                    NPC.CurrentNPC.ShowDialogue(dg.Id);
                });

            new Dialogue("bank_preprocess_house", null,

                async (args) =>
            {
                var pData = Sync.Players.GetData(Player.LocalPlayer);

                if (pData == null)
                    return;

                if (NPC.CurrentNPC == null)
                    return;

                var houseData = (Data.Locations.House)args[0];

                var data = ((string)await Events.CallRemoteProc("Bank::GHA", houseData.Id))?.Split('_');

                if (NPC.CurrentNPC == null)
                    return;

                if (data == null)
                    CloseCurrentDialogue();

                var balance = ulong.Parse(data[0]);
                var maxPaidDays = uint.Parse(data[1]);
                var minPaidDays = uint.Parse(data[2]);

                var maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * houseData.Tax;
                var minBalance = minPaidDays * houseData.Tax;

                var hoursLeft = balance / houseData.Tax;

                var currentDate = Sync.World.ServerTime;

                var currentDateZeros = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0, 0, currentDate.Kind);

                var targetDate = currentDateZeros.AddHours(hoursLeft > 30_000 ? 30_000 : hoursLeft);

                var timeLeft = targetDate.Subtract(currentDate);

                var enoughBalance = timeLeft.Hours > 0;

                var btns = new List<Button>();

                btns.Add(new Button("[Пополнить счет]", async () =>
                {
                    if (NPC.CurrentNPC == null)
                        return;

                    if (balance >= maxBalance)
                    {
                        CEF.Notification.ShowError(Locale.Notifications.Money.MaximalBalanceAlready);

                        return;
                    }

                    var nBalance = maxBalance == ulong.MaxValue ? (pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance) : maxBalance - balance;

                    if (nBalance == 0)
                    {
                        CEF.Notification.ShowError(Locale.Notifications.Money.NotEnough);

                        return;
                    }

                    var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                    if (bank == null)
                        return;

                    NPC.CurrentNPC.SwitchDialogue(false);

                    await CEF.ActionBox.ShowRange
                    (
                        "HouseBalanceChange", Locale.Actions.HouseBalanceDeposit, 0, nBalance, nBalance / 2, houseData.Tax, ActionBox.RangeSubTypes.MoneyRange,

                        CEF.ActionBox.DefaultBindAction,

                        (rType, amountD) => HouseBalanceChangeRangeActionBoxAction(rType, amountD, houseData, bank, true),

                        null
                    );
                }));

                if (balance > 0)
                {
                    btns.Add(new Button("[Снять со счета]", async () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        if (minBalance >= balance)
                        {
                            CEF.Notification.ShowError(string.Format(Locale.Notifications.Money.MinimalBalanceHouse, Utils.GetPriceString(minBalance)));

                            return;
                        }

                        var nBalance = balance - minBalance;

                        var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                        if (bank == null)
                            return;

                        NPC.CurrentNPC.SwitchDialogue(false);

                        await CEF.ActionBox.ShowRange
                        (
                            "HouseBalanceChange", Locale.Actions.HouseBalanceWithdraw, 0, nBalance, nBalance, houseData.Tax, ActionBox.RangeSubTypes.MoneyRange,

                            CEF.ActionBox.DefaultBindAction,

                            (rType, amountD) => HouseBalanceChangeRangeActionBoxAction(rType, amountD, houseData, bank, false),

                            null
                        );
                    }));
                }

                btns.Add(Button.DefaultBackButton);
                btns.Add(Button.DefaultExitButton);

                var dg = AllDialogues["bank_house_0"];

                dg.Buttons = btns;

                NPC.CurrentNPC.ShowDialogue(dg.Id, true, null, Utils.GetPriceString(balance), Utils.GetPriceString(houseData.Tax), enoughBalance ? $"Вашего баланса хватит еще на {timeLeft.GetBeautyString()}" : $"Срочно пополните баланс, иначе в {currentDate.Hour + 1}:00 ваш дом перейдет в собственность государства!");

            });

            new Dialogue("bank_preprocess_apartments", null,

                async (args) =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (NPC.CurrentNPC == null)
                        return;

                    var apsData = (Data.Locations.Apartments)args[0];

                    var data = ((string)await Events.CallRemoteProc("Bank::GAA", apsData.Id))?.Split('_');

                    if (NPC.CurrentNPC == null)
                        return;

                    if (data == null)
                        CloseCurrentDialogue();

                    var balance = ulong.Parse(data[0]);
                    var maxPaidDays = uint.Parse(data[1]);
                    var minPaidDays = uint.Parse(data[2]);

                    var maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * apsData.Tax;
                    var minBalance = minPaidDays * apsData.Tax;

                    var hoursLeft = balance / apsData.Tax;

                    var currentDate = Sync.World.ServerTime;

                    var currentDateZeros = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0, 0, currentDate.Kind);

                    var targetDate = currentDateZeros.AddHours(hoursLeft > 30_000 ? 30_000 : hoursLeft);

                    var timeLeft = targetDate.Subtract(currentDate);

                    var enoughBalance = timeLeft.Hours > 0;

                    var btns = new List<Button>();

                    btns.Add(new Button("[Пополнить счет]", async () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        if (balance >= maxBalance)
                        {
                            CEF.Notification.ShowError(Locale.Notifications.Money.MaximalBalanceAlready);

                            return;
                        }

                        var nBalance = maxBalance == ulong.MaxValue ? (pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance) : maxBalance - balance;

                        if (nBalance == 0)
                        {
                            CEF.Notification.ShowError(Locale.Notifications.Money.NotEnough);

                            return;
                        }

                        var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                        if (bank == null)
                            return;

                        NPC.CurrentNPC.SwitchDialogue(false);

                        await CEF.ActionBox.ShowRange
                        (
                            "HouseBalanceChange", Locale.Actions.ApartmentsBalanceDeposit, 0, nBalance, nBalance / 2, apsData.Tax, ActionBox.RangeSubTypes.MoneyRange,

                            CEF.ActionBox.DefaultBindAction,

                            (rType, amountD) => HouseBalanceChangeRangeActionBoxAction(rType, amountD, apsData, bank, true),

                            null
                        );
                    }));

                    if (balance > 0)
                    {
                        btns.Add(new Button("[Снять со счета]", async () =>
                        {
                            if (NPC.CurrentNPC == null)
                                return;

                            if (minBalance >= balance)
                            {
                                CEF.Notification.ShowError(Locale.Notifications.Money.MinimalBalanceHouse);

                                return;
                            }

                            var nBalance = balance - minBalance;

                            var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                            if (bank == null)
                                return;

                            NPC.CurrentNPC.SwitchDialogue(false);

                            await CEF.ActionBox.ShowRange
                            (
                                "HouseBalanceChange", Locale.Actions.ApartmentsBalanceWithdraw, 0, nBalance, nBalance, apsData.Tax, ActionBox.RangeSubTypes.MoneyRange,

                                CEF.ActionBox.DefaultBindAction,

                                (rType, amountD) => HouseBalanceChangeRangeActionBoxAction(rType, amountD, apsData, bank, false),

                                null
                            );
                        }));
                    }

                    btns.Add(Button.DefaultBackButton);
                    btns.Add(Button.DefaultExitButton);

                    var dg = AllDialogues["bank_aps_0"];

                    dg.Buttons = btns;

                    NPC.CurrentNPC.ShowDialogue(dg.Id, true, null, Utils.GetPriceString(balance), Utils.GetPriceString(apsData.Tax), enoughBalance ? $"Вашего баланса хватит еще на {timeLeft.GetBeautyString()}" : $"Срочно пополните баланс, иначе в {currentDate.Hour + 1}:00 ваша квартира перейдет в собственность государства!");

                });

            new Dialogue("bank_preprocess_garage", null,

                async (args) =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (NPC.CurrentNPC == null)
                        return;

                    var garageData = (Data.Locations.Garage)args[0];

                    var data = ((string)await Events.CallRemoteProc("Bank::GGA", garageData.Id))?.Split('_');

                    if (NPC.CurrentNPC == null)
                        return;

                    if (data == null)
                        CloseCurrentDialogue();

                    var balance = ulong.Parse(data[0]);
                    var maxPaidDays = uint.Parse(data[1]);
                    var minPaidDays = uint.Parse(data[2]);

                    var maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * garageData.Tax;
                    var minBalance = minPaidDays * garageData.Tax;

                    var hoursLeft = balance / garageData.Tax;

                    var currentDate = Sync.World.ServerTime;

                    var currentDateZeros = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0, 0, currentDate.Kind);

                    var targetDate = currentDateZeros.AddHours(hoursLeft > 30_000 ? 30_000 : hoursLeft);

                    var timeLeft = targetDate.Subtract(currentDate);

                    var enoughBalance = timeLeft.Hours > 0;

                    var btns = new List<Button>();

                    btns.Add(new Button("[Пополнить счет]", async () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        if (balance >= maxBalance)
                        {
                            CEF.Notification.ShowError(Locale.Notifications.Money.MaximalBalanceAlready);

                            return;
                        }

                        var nBalance = maxBalance == ulong.MaxValue ? (pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance) : maxBalance - balance;

                        if (nBalance == 0)
                        {
                            CEF.Notification.ShowError(Locale.Notifications.Money.NotEnough);

                            return;
                        }

                        var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                        if (bank == null)
                            return;

                        NPC.CurrentNPC.SwitchDialogue(false);

                        await CEF.ActionBox.ShowRange
                        (
                            "GarageBalanceChange", Locale.Actions.GarageBalanceDeposit, 0, nBalance, nBalance / 2, garageData.Tax, ActionBox.RangeSubTypes.MoneyRange,

                            CEF.ActionBox.DefaultBindAction,

                            (rType, amountD) => GarageBalanceChangeRangeActionBoxAction(rType, amountD, garageData, bank, true),

                            null
                        );
                    }));

                    if (balance > 0)
                    {
                        btns.Add(new Button("[Снять со счета]", async () =>
                        {
                            if (NPC.CurrentNPC == null)
                                return;

                            if (minBalance >= balance)
                            {
                                CEF.Notification.ShowError(Locale.Notifications.Money.MinimalBalanceHouse);

                                return;
                            }

                            var nBalance = balance - minBalance;

                            var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                            if (bank == null)
                                return;

                            NPC.CurrentNPC.SwitchDialogue(false);

                            await CEF.ActionBox.ShowRange
                            (
                                "GarageBalanceChange", Locale.Actions.GarageBalanceWithdraw, 0, nBalance, nBalance, garageData.Tax, ActionBox.RangeSubTypes.MoneyRange,

                                CEF.ActionBox.DefaultBindAction,

                                (rType, amountD) => GarageBalanceChangeRangeActionBoxAction(rType, amountD, garageData, bank, false),

                                null
                            );
                        }));
                    }

                    btns.Add(Button.DefaultBackButton);
                    btns.Add(Button.DefaultExitButton);

                    var dg = AllDialogues["bank_garage_0"];

                    dg.Buttons = btns;

                    NPC.CurrentNPC.ShowDialogue(dg.Id, true, null, Utils.GetPriceString(balance), Utils.GetPriceString(garageData.Tax), enoughBalance ? $"Вашего баланса хватит еще на {timeLeft.GetBeautyString()}" : $"Срочно пополните баланс, иначе в {currentDate.Hour + 1}:00 Ваш гараж перейдет в собственность государства!");

                });

            new Dialogue("bank_preprocess_business", null,

                async (args) =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (NPC.CurrentNPC == null)
                        return;

                    var businessData = (Data.Locations.Business)args[0];

                    var data = ((string)await Events.CallRemoteProc("Bank::GBA", businessData.Id))?.Split('_');

                    if (NPC.CurrentNPC == null)
                        return;

                    if (data == null)
                        CloseCurrentDialogue();

                    var balance = ulong.Parse(data[0]);
                    var maxPaidDays = uint.Parse(data[1]);
                    var minPaidDays = uint.Parse(data[2]);

                    var maxBalance = maxPaidDays == 0 ? ulong.MaxValue : maxPaidDays * businessData.Rent;
                    var minBalance = minPaidDays * businessData.Rent;

                    var hoursLeft = balance / businessData.Rent;

                    var currentDate = Sync.World.ServerTime;

                    var currentDateZeros = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, currentDate.Hour, 0, 0, 0, currentDate.Kind);

                    var targetDate = currentDateZeros.AddHours(hoursLeft > 30_000 ? 30_000 : hoursLeft);

                    var timeLeft = targetDate.Subtract(currentDate);

                    var enoughBalance = timeLeft.Hours > 0;

                    var btns = new List<Button>();

                    btns.Add(new Button("[Пополнить счет]", async () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        if (balance >= maxBalance)
                        {
                            CEF.Notification.ShowError(Locale.Notifications.Money.MaximalBalanceAlready);

                            return;
                        }

                        var nBalance = maxBalance == ulong.MaxValue ? (pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance) : maxBalance - balance;

                        if (nBalance == 0)
                        {
                            CEF.Notification.ShowError(Locale.Notifications.Money.NotEnough);

                            return;
                        }

                        var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                        if (bank == null)
                            return;

                        NPC.CurrentNPC.SwitchDialogue(false);

                        await CEF.ActionBox.ShowRange
                        (
                            "BusinessBalanceChange", Locale.Actions.BusinessBalanceDeposit, 0, nBalance, nBalance / 2, businessData.Rent, ActionBox.RangeSubTypes.MoneyRange,

                            CEF.ActionBox.DefaultBindAction,

                            (rType, amountD) => BusinessBalanceChangeRangeActionBoxAction(rType, amountD, businessData, bank, true),

                            null
                        );
                    }));

                    if (balance > 0)
                    {
                        btns.Add(new Button("[Снять со счета]", async () =>
                        {
                            if (NPC.CurrentNPC == null)
                                return;

                            if (minBalance >= balance)
                            {
                                CEF.Notification.ShowError(Locale.Notifications.Money.MinimalBalanceHouse);

                                return;
                            }

                            var nBalance = balance - minBalance;

                            var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                            if (bank == null)
                                return;

                            NPC.CurrentNPC.SwitchDialogue(false);

                            await CEF.ActionBox.ShowRange
                            (
                                "BusinessBalanceChange", Locale.Actions.BusinessBalanceWithdraw, 0, nBalance, nBalance, businessData.Rent, ActionBox.RangeSubTypes.MoneyRange,

                                CEF.ActionBox.DefaultBindAction,

                                (rType, amountD) => BusinessBalanceChangeRangeActionBoxAction(rType, amountD, businessData, bank, false),

                                null
                            );
                        }));
                    }

                    btns.Add(Button.DefaultBackButton);
                    btns.Add(Button.DefaultExitButton);

                    var dg = AllDialogues["bank_business_0"];

                    dg.Buttons = btns;

                    NPC.CurrentNPC.ShowDialogue(dg.Id, true, null, Utils.GetPriceString(balance), Utils.GetPriceString(businessData.Rent), enoughBalance ? $"Вашего баланса хватит еще на {timeLeft.GetBeautyString()}" : $"Срочно пополните баланс, иначе в {currentDate.Hour + 1}:00 Ваш бизнес перейдет в собственность государства!");

                });

            new Dialogue("bank_preprocess_fraction", null,

                async (args) =>
                {
                    var pData = Sync.Players.GetData(Player.LocalPlayer);

                    if (pData == null)
                        return;

                    if (NPC.CurrentNPC == null)
                        return;

                    var fData = (Data.Fractions.Fraction)args[0];

                    var data = ((string)await Events.CallRemoteProc("Bank::GFA", (int)fData.Type))?.Split('_');

                    if (NPC.CurrentNPC == null)
                        return;

                    if (data == null)
                        CloseCurrentDialogue();

                    var balance = ulong.Parse(data[0]);

                    var btns = new List<Button>();

                    btns.Add(new Button("[Пополнить счет]", async () =>
                    {
                        if (NPC.CurrentNPC == null)
                            return;

                        var nBalance = pData.Cash > pData.BankBalance ? pData.Cash : pData.BankBalance;

                        if (nBalance == 0)
                        {
                            CEF.Notification.ShowError(Locale.Notifications.Money.NotEnough);

                            return;
                        }

                        var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                        if (bank == null)
                            return;

                        NPC.CurrentNPC.SwitchDialogue(false);

                        await CEF.ActionBox.ShowRange
                        (
                            "FractionBalanceChange", Locale.Actions.FractionBalanceDeposit, 0, nBalance, nBalance / 2, 1, ActionBox.RangeSubTypes.MoneyRange,

                            CEF.ActionBox.DefaultBindAction,

                            (rType, amountD) => FractionBalanceChangeRangeActionBoxAction(rType, amountD, fData, bank, true),

                            null
                        );
                    }));

                    if (balance > 0)
                    {
                        btns.Add(new Button("[Снять со счета]", async () =>
                        {
                            if (NPC.CurrentNPC == null)
                                return;

                            if (balance <= 0)
                            {
                                CEF.Notification.ShowError(Locale.Notifications.Money.NoBalanceHouse);

                                return;
                            }

                            var nBalance = balance;

                            var bank = NPC.CurrentNPC.Data as Data.Locations.Bank;

                            if (bank == null)
                                return;

                            NPC.CurrentNPC.SwitchDialogue(false);

                            await CEF.ActionBox.ShowRange
                            (
                                "FractionBalanceChange", Locale.Actions.FractionBalanceWithdraw, 0, nBalance, nBalance, 1, ActionBox.RangeSubTypes.MoneyRange,

                                CEF.ActionBox.DefaultBindAction,

                                (rType, amountD) => FractionBalanceChangeRangeActionBoxAction(rType, amountD, fData, bank, false),

                                null
                            );
                        }));
                    }

                    btns.Add(Button.DefaultBackButton);
                    btns.Add(Button.DefaultExitButton);

                    var dg = AllDialogues["bank_fraction_0"];

                    dg.Buttons = btns;

                    NPC.CurrentNPC.ShowDialogue(dg.Id, true, null, Utils.GetPriceString(balance));

                });

            new Dialogue("bank_has_account", "Здравствуйте, могу ли я Вам чем-нибудь помочь?", null);

            new Dialogue("bank_no_account_0", "Здравствуйте, могу ли я Вам чем-нибудь помочь?", null);

            new Dialogue("bank_no_account_1", "Отлично!\nУ нас есть несколько выгодных тарифов, ознакомьтесь с ними и выберите интересующий", null,

                new Button("[Перейти к тарифам]", () =>
                {
                    if (NPC.CurrentNPC?.Data is Data.Locations.Bank bankData)
                    {
                        if (NPC.LastSent.IsSpam(1000, false, false))
                            return;

                        Events.CallRemote("Bank::Show", false, bankData.Id);
                    }
                }),

                Button.DefaultBackButton,

                Button.DefaultExitButton

            );

            new Dialogue("bank_house_0", "Баланс на счете Вашего дома составляет {0}, налог в час - {1}.\n{2}", null);

            new Dialogue("bank_aps_0", "Баланс на счете Вашей квартиры составляет {0}, налог в час - {1}.\n{2}", null);

            new Dialogue("bank_garage_0", "Баланс на счете Вашего гаража составляет {0}, налог в час - {1}.\n{2}", null);

            new Dialogue("bank_business_0", "Баланс на счете Вашего бизнеса составляет {0}, стоимость аренды помещения в час - {1}.\n{2}", null);

            new Dialogue("bank_fraction_0", "Баланс на счете Вашей фракции составляет {0}.", null);
        }

        private static async void HouseBalanceChangeRangeActionBoxAction(CEF.ActionBox.ReplyTypes rType, decimal amountD, Data.Locations.HouseBase house, Data.Locations.Bank bank, bool add)
        {
            if (CEF.ActionBox.LastSent.IsSpam(1000, false, true))
                return;

            int amount;

            if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                return;

            CEF.ActionBox.LastSent = Sync.World.ServerTime;

            var useCash = rType == CEF.ActionBox.ReplyTypes.OK;

            var resObj = await Events.CallRemoteProc("Bank::HBC", house.Type == Sync.House.HouseTypes.House, house.Id, bank.Id, amount, useCash, add);

            if (resObj == null)
                return;

            CEF.ActionBox.Close(true);
        }

        private static async void GarageBalanceChangeRangeActionBoxAction(CEF.ActionBox.ReplyTypes rType, decimal amountD, Data.Locations.Garage garage, Data.Locations.Bank bank, bool add)
        {
            if (CEF.ActionBox.LastSent.IsSpam(1000, false, true))
                return;

            int amount;

            if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                return;

            CEF.ActionBox.LastSent = Sync.World.ServerTime;

            var useCash = rType == CEF.ActionBox.ReplyTypes.OK;

            var resObj = await Events.CallRemoteProc("Bank::GBC", garage.Id, bank.Id, amount, useCash, add);

            if (resObj == null)
                return;

            CEF.ActionBox.Close(true);
        }

        private static async void BusinessBalanceChangeRangeActionBoxAction(CEF.ActionBox.ReplyTypes rType, decimal amountD, Data.Locations.Business biz, Data.Locations.Bank bank, bool add)
        {
            if (CEF.ActionBox.LastSent.IsSpam(1000, false, true))
                return;

            int amount;

            if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                return;

            CEF.ActionBox.LastSent = Sync.World.ServerTime;

            var useCash = rType == CEF.ActionBox.ReplyTypes.OK;

            var resObj = await Events.CallRemoteProc("Bank::BBC", biz.Id, bank.Id, amount, useCash, add);

            if (resObj == null)
                return;

            CEF.ActionBox.Close(true);
        }

        private static async void FractionBalanceChangeRangeActionBoxAction(CEF.ActionBox.ReplyTypes rType, decimal amountD, Data.Fractions.Fraction fData, Data.Locations.Bank bank, bool add)
        {
            if (CEF.ActionBox.LastSent.IsSpam(1000, false, true))
                return;

            int amount;

            if (!amountD.IsNumberValid(1, int.MaxValue, out amount, true))
                return;

            CEF.ActionBox.LastSent = Sync.World.ServerTime;

            var useCash = rType == CEF.ActionBox.ReplyTypes.OK;

            var resObj = await Events.CallRemoteProc("Bank::FBC", (int)fData.Type, bank.Id, amount, useCash, add);

            if (resObj == null)
                return;

            CEF.ActionBox.Close(true);
        }
    }
}
