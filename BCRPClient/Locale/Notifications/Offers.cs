using System.Collections.Generic;

namespace BCRPClient
{
    public static partial class Locale
    {
        public static partial class Notifications
        {
            public static class Offers
            {
                public static string Header = "Предложение";
                public static string HeaderTrade = "Обмен";

                public static string OfferSettleHouse = "в свой дом";
                public static string OfferSettleApartments = "в свою квартиру";

                public static Dictionary<Sync.Offers.Types, string> Types = new Dictionary<Sync.Offers.Types, string>()
                {
                    { Sync.Offers.Types.Handshake, "{0} предлагает вам поздороваться" },

                    { Sync.Offers.Types.HeadsOrTails, "{0} предлагает вам сыграть в орел и решку" },

                    { Sync.Offers.Types.Exchange, "{0} предлагает вам обменяться" },
                    { Sync.Offers.Types.SellEstate, "{0} предлагает вам продажу недвижимости" },
                    { Sync.Offers.Types.SellVehicle, "{0} предлагает вам продажу транспорта" },
                    { Sync.Offers.Types.SellBusiness, "{0} предлагает вам продажу бизнеса" },

                    { Sync.Offers.Types.Settle, "{0} предлагает вам подселиться {1}" },

                    { Sync.Offers.Types.Carry, "{0} предлагает понести вас" },

                    { Sync.Offers.Types.Cash, "{0} предлагает вам ${1}" },

                    { Sync.Offers.Types.WaypointShare, "{0} предлагает вам свою метку" },

                    { Sync.Offers.Types.ShowPassport, "{0} предлагает вам посмотреть паспорт" },
                    { Sync.Offers.Types.ShowMedicalCard, "{0} предлагает вам посмотреть мед. карту" },
                    { Sync.Offers.Types.ShowVehiclePassport, "{0} предлагает вам посмотреть тех. паспорт" },
                    { Sync.Offers.Types.ShowLicenses, "{0} предлагает вам посмотреть лицензии" },
                    { Sync.Offers.Types.ShowResume, "{0} предлагает вам посмотреть резюме" },

                    { Sync.Offers.Types.InviteFraction, "{0} предлагает вам вступить во фракцию {1}" },
                    { Sync.Offers.Types.InviteOrganisation, "{0} предлагает вам вступить в организацию {1}" },
                };

                public static string Cancel = "Предложение было отменено!";
                public static string CancelBy = "Игрок отменил предложение!";

                public static string Sent = "Предложение успешно отправлено!";

                public static string TargetBusy = "Данный игрок сейчас занят!";
                public static string TargetHasOffer = "Данному игроку уже что-то предложили!";
                public static string PlayerHasOffer = "У вас уже есть активное предложение!";

                public static string PlayerNeedConfirm = "Другой игрок еще не подтвердил условия обмена. Подождите, пока он сделает это";
                public static string PlayerConfirmed = "Другой игрок подтвердил условия обмена. Чтобы согласиться и совершить обмен - сделайте то же самое";
                public static string PlayerConfirmedCancel = "Другой игрок отменил подтверждение условий обмена!";

                public static string TradeCompleted = "Обмен успешно завершен!";
                public static string TradeError = "Произошла ошибка! Попробуйте еще раз";

                public static string TradeNotEnoughMoney = "У вас недостаточно средств!";
                public static string TradeNotEnoughMoneyOther = "У другого игрока недостаточно средств!";
                public static string TradeNotEnoughSpaceOther = "У другого игрока недостаточно места в инвентаре!";

                public static string TradeNotEnoughPropertySpaceOther = "Другой игрок сейчас не может принять некоторое имущество!";
            }
        }
    }
}
