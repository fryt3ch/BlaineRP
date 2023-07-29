using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BlaineRP.Server.Game.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Offers
{
    public enum ReplyTypes
    {
        Deny = 0,
        Accept,
        Busy,
        AutoCancel,
    }

    public partial class Offer
    {
        private static Dictionary<OfferType, OfferBase> _offerBases;

        private static List<Offer> _allOffers = new List<Offer>();

        /// <summary>Сущность игрока, который отправил предложение</summary>
        public PlayerData Sender { get; set; }

        /// <summary>Сущность игрока, которому отправлено предложение</summary>
        public PlayerData Receiver { get; set; }

        /// <summary>Тип предложения</summary>
        public OfferType Type { get; set; }

        public Trade TradeData { get; set; }

        /// <summary>Timer предложения</summary>
        private Timer Timer { get; set; }

        public object Data { get; set; }

        /// <summary>Новое предложение</summary>
        /// <param name="Sender">Сущность игрока, который отправил предложение</param>
        /// <param name="Receiver">Сущность игрока, которому отправлено предложение</param>
        /// <param name="Type">Тип предложения</param>
        public Offer(PlayerData Sender, PlayerData Receiver, OfferType Type, object Data = null)
        {
            this.Sender = Sender;
            this.Receiver = Receiver;

            this.Type = Type;

            this.Data = Data;
        }

        /// <summary>Метод для отмены предложения и удаления его из списка активных предложения</summary>
        public void Cancel(bool success = false, bool isSender = false, ReplyTypes rType = ReplyTypes.AutoCancel, bool justCancelCts = false)
        {
            var ctsNull = Timer == null;

            if (ctsNull)
            {
                var oBase = GetOfferBaseDataByType(Type);

                oBase.OnCancel(Sender, Receiver, this);
            }
            else
            {
                Timer.Dispose();

                Timer = null;
            }

            var sender = Sender?.Player;
            var receiver = Receiver?.Player;

            if (sender?.Exists == true)
            {
                sender.TriggerEvent("Offer::Reply::Server", false, justCancelCts, ctsNull);

                if (!success)
                {
                    if (rType == ReplyTypes.Deny && !isSender)
                        sender.Notify("Offer::CancelBy");
                    else if (isSender || rType == ReplyTypes.AutoCancel)
                        sender.Notify("Offer::Cancel");
                    else if (rType == ReplyTypes.Busy)
                        sender.Notify("Offer::TargetBusy");
                }
            }

            if (receiver?.Exists == true)
            {
                receiver.TriggerEvent("Offer::Reply::Server", false, justCancelCts, ctsNull);

                if (!success)
                {
                    if (rType == ReplyTypes.Deny && isSender)
                        receiver.Notify("Offer::CancelBy");
                    else if (rType != ReplyTypes.Busy)
                        receiver.Notify("Offer::Cancel");
                }
            }

            if (justCancelCts)
                return;

            _allOffers.Remove(this);
        }

        public void Execute()
        {
            if (Timer == null)
                return;

            var oBase = GetOfferBaseDataByType(Type);

            oBase.OnAccept(Sender, Receiver, this);
        }

        public static Offer Create(PlayerData pData, PlayerData tData, OfferType type, int duration = -1, object data = null)
        {
            var offer = new Offer(pData, tData, type, data);

            if (duration == -1)
                duration = Properties.Settings.Static.OFFER_DEFAULT_DURATION;

            offer.Timer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    if (offer.Timer == null)
                        return;

                    offer.Cancel(false, false, ReplyTypes.AutoCancel, false);
                });
            }, null, duration, Timeout.Infinite);

            _allOffers.Add(offer);

            return offer;
        }

        public static Offer GetByPlayer(PlayerData pData) => _allOffers.Where(x => x.Sender == pData || x.Receiver == pData).FirstOrDefault();

        public static Offer GetBySender(PlayerData pData) => _allOffers.Where(x => x.Sender == pData).FirstOrDefault();

        public static Offer GetByReceiver(PlayerData pData) => _allOffers.Where(x => x.Receiver == pData).FirstOrDefault();

        public static OfferBase GetOfferBaseDataByType(OfferType type) => _offerBases.GetValueOrDefault(type);
    }
}
