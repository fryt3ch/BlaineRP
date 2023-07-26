namespace BlaineRP.Client
{
    public static partial class Locale
    {
        public static partial class Notifications
        {
            public static class Offers
            {
                public static string Header = "Предложение";
                public static string HeaderTrade = "Обмен";

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