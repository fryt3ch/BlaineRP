using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using BlaineRP.Server.EntityData.Players;

namespace BlaineRP.Server.Sync.Offers
{
    public enum Types
    {
        /// <summary>Рукопожатие</summary>
        Handshake = 0,
        /// <summary>Обмен</summary>
        Exchange,
        /// <summary>Нести игркока</summary>
        Carry,
        /// <summary>Сыграть в орел и решка</summary>
        HeadsOrTails,
        /// <summary>Приглашение во фракцию</summary>
        InviteFraction,
        /// <summary>Приглашение в организацию</summary>
        InviteOrganisation,
        /// <summary>Передать наличные</summary>
        Cash,
        /// <summary>Показать паспорт</summary>
        ShowPassport,
        /// <summary>Показать мед. карту</summary>
        ShowMedicalCard,
        /// <summary>Показать лицензии</summary>
        ShowLicenses,
        /// <summary>Показать тех. паспорт</summary>
        ShowVehiclePassport,
        /// <summary>Показать резюме</summary>
        ShowResume,
        /// <summary>Показать удостоверение</summary>
        ShowFractionDocs,
        /// <summary>Продажа имущества</summary>
        PropertySell,
        /// <summary>Поделиться меткой</summary>
        WaypointShare,
        /// <summary>Подселить в дом/квартиру</summary>
        Settle,
        /// <summary>Продать недвижимость</summary>
        SellEstate,
        /// <summary>Продать транспорт</summary>
        SellVehicle,
        /// <summary>Продать бизнес</summary>
        SellBusiness,
        /// <summary>Штраф полиции</summary>
        PoliceFine,
        /// <summary>Лечение от врача</summary>
        EmsHeal,
        /// <summary>Лечение (психики) от врача</summary>
        EmsPsychHeal,
        /// <summary>Лечение (наркозавимиости) от врача</summary>
        EmsDrugHeal,
        /// <summary>Проверка здоровья от врача</summary>
        EmsDiagnostics,
        /// <summary>Выдача мед. карты от врача</summary>
        EmsMedicalCard,
        /// <summary>Продажа мед. маски от врача</summary>
        EmsSellMask,
        /// <summary>Использовать мед. предмет для лечения</summary>
        GiveHealingItem,
    }

    public enum ReplyTypes
    {
        Deny = 0,
        Accept,
        Busy,
        AutoCancel,
    }

    public partial class Offer
    {
        private static Dictionary<Types, OfferBase> _offerBases;

        private static List<Offer> _allOffers = new List<Offer>();

        /// <summary>Сущность игрока, который отправил предложение</summary>
        public PlayerData Sender { get; set; }

        /// <summary>Сущность игрока, которому отправлено предложение</summary>
        public PlayerData Receiver { get; set; }

        /// <summary>Тип предложения</summary>
        public Types Type { get; set; }

        public Trade TradeData { get; set; }

        /// <summary>Timer предложения</summary>
        private Timer Timer { get; set; }

        public object Data { get; set; }

        /// <summary>Новое предложение</summary>
        /// <param name="Sender">Сущность игрока, который отправил предложение</param>
        /// <param name="Receiver">Сущность игрока, которому отправлено предложение</param>
        /// <param name="Type">Тип предложения</param>
        public Offer(PlayerData Sender, PlayerData Receiver, Types Type, object Data = null)
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

        public static Offer Create(PlayerData pData, PlayerData tData, Types type, int duration = -1, object data = null)
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

        public static OfferBase GetOfferBaseDataByType(Types type) => _offerBases.GetValueOrDefault(type);

        public static void Load()
        {
            if (_offerBases != null)
                return;

            _offerBases = new Dictionary<Types, OfferBase>();

            foreach (var x in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass && x.BaseType == typeof(OfferBase) && x.Namespace?.StartsWith("BCRPServer.Sync.Offers") == true))
            {
                var attr = x.GetCustomAttribute<OfferAttribute>();

                if (attr == null)
                    continue;

                var obj = (OfferBase)Activator.CreateInstance(x);

                if (!_offerBases.TryAdd(attr.Type, obj))
                    _offerBases[attr.Type] = obj;

                //Console.WriteLine(x.Name);
            }
        }
    }
}
