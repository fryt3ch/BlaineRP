using BCRPServer.Game.Bank;
using BCRPServer.Game.Items;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Markup;
using static BCRPServer.Sync.Offers;

namespace BCRPServer.Sync
{
    public class Offers : Script
    {
        /// <summary>Список всех активных предложений</summary>
        public static List<Offer> AllOffers { get; private set; }

        public Offers()
        {
            AllOffers = new List<Offer>();
        }

        public enum Types
        {
            /// <summary>Рукопожатие</summary>
            Handshake = 0,
            /// <summary>Обмен</summary>
            Exchange,
            /// <summary>Нести игркока</summary>
            Carry,
            /// <summary>Приглашение во фракцию</summary>
            InviteFraction,
            /// <summary>Приглашение в организацию</summary>
            InviteOrganisation,
            Cash,
        }

        private static Dictionary<Types, Dictionary<bool, Action<PlayerData, PlayerData, Offer>>> OfferActions = new Dictionary<Types, Dictionary<bool, Action<PlayerData, PlayerData, Offer>>>()
        {
            {
                Types.Handshake,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (tPlayer.Vehicle == null && sPlayer.Vehicle == null && !pData.CanPlayAnim() && !tData.CanPlayAnim())
                            {
                                tPlayer.Position = sPlayer.GetFrontOf(0.85f);
                                tPlayer.Heading = Utils.GetOppositeAngle(sPlayer.Heading);

                                pData.PlayAnim(Animations.FastTypes.Handshake);
                                tData.PlayAnim(Animations.FastTypes.Handshake);
                            }

                            pData.AddFamiliar(tData);
                        }
                    }
                }
            },

            {
                Types.Carry,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            if (tPlayer.Vehicle == null && sPlayer.Vehicle == null && !pData.CanPlayAnim() && !tData.CanPlayAnim())
                            {
                                sPlayer.AttachEntity(tPlayer, AttachSystem.Types.Carry);
                            }
                        }
                    }
                }
            },

            {
                Types.Cash,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        async (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            int cash = (offer.Data as int?) ?? 0;

                            if (cash <= 0)
                                return;

                            pData.AddCash(-cash, true);

                            tData.AddCash(cash, true);
                        }
                    }
                }
            },

            {
                Types.Exchange,

                new Dictionary<bool, Action<PlayerData, PlayerData, Offer>>()
                {
                    {
                        true,

                        (pData, tData, offer) =>
                        {
                            offer.Cancel(true, false, ReplyTypes.AutoCancel, true);

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                                return;

                            sPlayer.CloseAll();
                            tPlayer.CloseAll();

                            sPlayer.TriggerEvent("Inventory::Show", 3);
                            tPlayer.TriggerEvent("Inventory::Show", 3);

                            offer.TradeData = new Offer.Trade();

                            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);
                        }
                    },

                    {
                        false,

                        (pData, tData, offer) =>
                        {
                            offer.TradeData = null;

                            if (pData == null || tData == null)
                                return;

                            var sPlayer = pData.Player;
                            var tPlayer = tData.Player;

                            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                                return;

                            sPlayer.CloseAll();
                            tPlayer.CloseAll();
                        }
                    }
                }
            }
        };

        public enum ReturnTypes
        {
            Error = -1,
            SourceBusy, TargetBusy,
            SourceHasOffer, TargetHasOffer,
            NotEnoughMoneySource, NotEnoughMoneyTarget,
            Success,
        }

        public enum ReplyTypes
        {
            Deny = 0,
            Accept,
            Busy,
            AutoCancel,
        }

        public class Offer
        {
            #region Trade Subclass
            public class Trade
            {
                public class TradeItem
                {
                    public Game.Items.Item ItemRoot { get; set; }
                    public int Amount { get; set; }

                    public TradeItem(Game.Items.Item ItemRoot, int Amount)
                    {
                        this.ItemRoot = ItemRoot;
                        this.Amount = Amount;
                    }

                    public string ToClientJson() => ItemRoot == null ? "null" : (new object[] { ItemRoot.ID, Amount, ItemRoot is IStackable ? ItemRoot.BaseWeight : ItemRoot.Weight, Game.Items.Items.GetItemTag(ItemRoot) }).SerializeToJson();
                }

                public TradeItem[] SenderItems { get; set; }
                public TradeItem[] ReceiverItems { get; set; }

                public int SenderMoney { get; set; }
                public int ReceiverMoney { get; set; }

                public bool SenderReady { get; set; }
                public bool ReceiverReady { get; set; }

                public (CEF.Inventory.Results Result, PlayerData PlayerError) Execute(PlayerData pData, PlayerData tData)
                {
                    var senderFreeSlots = pData.Items.Where(x => x == null).Count();

                    var senderItems = SenderItems.Where(x => x != null && x.ItemRoot != null).ToList();
                    var receiverItems = ReceiverItems.Where(x => x != null && x.ItemRoot != null).ToList();

                    var senderRemoveSlots = senderItems.Where(x => (((x.ItemRoot as Game.Items.IStackable)?.Amount ?? 1) - x.Amount) == 0).Count();
                    var receiverRemoveSlots = receiverItems.Where(x => (((x.ItemRoot as Game.Items.IStackable)?.Amount ?? 1) - x.Amount) == 0).Count();

                    if (senderFreeSlots + senderRemoveSlots < receiverItems.Count)
                        return (CEF.Inventory.Results.NoSpace, pData);

                    var receiverFreeSlots = tData.Items.Where(x => x == null).Count();

                    if (receiverFreeSlots + receiverRemoveSlots < senderItems.Count)
                        return (CEF.Inventory.Results.NoSpace, pData);

                    var senderCurrentWeight = pData.Items.Sum(x => x?.Weight ?? 0f);

                    var senderRemoveWeight = senderItems.Sum(x => x.Amount * x.ItemRoot.BaseWeight);
                    var receiverRemoveWeight = receiverItems.Sum(x => x.Amount * x.ItemRoot.BaseWeight);

                    if (senderCurrentWeight - senderRemoveWeight + receiverRemoveWeight > Settings.MAX_INVENTORY_WEIGHT)
                        return (CEF.Inventory.Results.NoSpace, pData);

                    var receiverCurrentWeight = tData.Items.Sum(x => x?.Weight ?? 0f);

                    if (receiverCurrentWeight - receiverRemoveWeight + senderRemoveWeight > Settings.MAX_INVENTORY_WEIGHT)
                        return (CEF.Inventory.Results.NoSpace, tData);

                    var moneyRes = ((Func<(CEF.Inventory.Results Result, PlayerData PlayerError)>)(() =>
                    {
                        if (pData.Cash < SenderMoney)
                            return (CEF.Inventory.Results.NotEnoughMoney, pData);

                        if (tData.Cash < ReceiverMoney)
                            return (CEF.Inventory.Results.NotEnoughMoney, tData);

                        return (CEF.Inventory.Results.Success, null);
                    })).Invoke();

                    if (moneyRes.Result != CEF.Inventory.Results.Success)
                        return moneyRes;

                    if (ReceiverMoney > 0)
                    {
                        pData.AddCash(ReceiverMoney, true);
                        tData.AddCash(-ReceiverMoney, true);
                    }

                    if (SenderMoney > 0)
                    {
                        tData.AddCash(SenderMoney, true);
                        pData.AddCash(-SenderMoney, true);
                    }

                    var senderSlotsToUpdate = new List<(int, string)>();
                    var receiverSlotsToUpdate = new List<(int, string)>();

                    for (int i = 0; i < senderItems.Count; i++)
                    {
                        if (senderItems[i].ItemRoot is Game.Items.IStackable && senderItems[i].Amount < (senderItems[i].ItemRoot as Game.Items.IStackable).Amount)
                        {
                            for (int j = 0; j < pData.Items.Length; j++)
                                if (pData.Items[j] == senderItems[i].ItemRoot)
                                {
                                    (pData.Items[j] as Game.Items.IStackable).Amount -= senderItems[i].Amount;

                                    senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], CEF.Inventory.Groups.Items)));

                                    break;
                                }

                            senderItems[i].ItemRoot = Game.Items.Items.CreateItem(senderItems[i].ItemRoot.ID, 0, senderItems[i].Amount, false);

                            if (senderItems[i].ItemRoot == null)
                                return (CEF.Inventory.Results.Error, null);
                        }
                        else
                        {
                            for (int j = 0; j < pData.Items.Length; j++)
                                if (pData.Items[j] == senderItems[i].ItemRoot)
                                {
                                    pData.Items[j] = null;

                                    senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], CEF.Inventory.Groups.Items)));

                                    break;
                                }
                        }
                    }

                    for (int i = 0; i < receiverItems.Count; i++)
                    {
                        if (receiverItems[i].ItemRoot is Game.Items.IStackable && receiverItems[i].Amount < (receiverItems[i].ItemRoot as Game.Items.IStackable).Amount)
                        {
                            for (int j = 0; j < tData.Items.Length; j++)
                                if (tData.Items[j] == receiverItems[i].ItemRoot)
                                {
                                    (tData.Items[j] as Game.Items.IStackable).Amount -= receiverItems[i].Amount;

                                    receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], CEF.Inventory.Groups.Items)));

                                    break;
                                }

                            receiverItems[i].ItemRoot = Game.Items.Items.CreateItem(receiverItems[i].ItemRoot.ID, 0, receiverItems[i].Amount, false);

                            if (receiverItems[i].ItemRoot == null)
                                return (CEF.Inventory.Results.Error, null);
                        }
                        else
                        {
                            for (int j = 0; j < tData.Items.Length; j++)
                                if (tData.Items[j] == receiverItems[i].ItemRoot)
                                {
                                    tData.Items[j] = null;

                                    receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], CEF.Inventory.Groups.Items)));

                                    break;
                                }
                        }
                    }

                    for (int i = 0; i < senderItems.Count; i++)
                    {
                        for (int j = 0; j < tData.Items.Length; j++)
                        {
                            if (tData.Items[j] == null)
                            {
                                tData.Items[j] = senderItems[i].ItemRoot;

                                receiverSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(tData.Items[j], CEF.Inventory.Groups.Items)));

                                break;
                            }
                        }
                    }

                    for (int i = 0; i < receiverItems.Count; i++)
                    {
                        for (int j = 0; j < pData.Items.Length; j++)
                        {
                            if (pData.Items[j] == null)
                            {
                                pData.Items[j] = receiverItems[i].ItemRoot;

                                senderSlotsToUpdate.Add((j, Game.Items.Item.ToClientJson(pData.Items[j], CEF.Inventory.Groups.Items)));

                                break;
                            }
                        }
                    }

                    MySQL.CharacterItemsUpdate(pData.Info);
                    MySQL.CharacterItemsUpdate(tData.Info);

                    for (int i = 0; i < senderSlotsToUpdate.Count; i++)
                        pData.Player.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Items, senderSlotsToUpdate[i].Item1, senderSlotsToUpdate[i].Item2);

                    for (int i = 0; i < receiverSlotsToUpdate.Count; i++)
                        tData.Player.TriggerEvent("Inventory::Update", (int)CEF.Inventory.Groups.Items, receiverSlotsToUpdate[i].Item1, receiverSlotsToUpdate[i].Item2);

                    return (CEF.Inventory.Results.Success, null);
                }

                public Trade()
                {
                    SenderItems = new TradeItem[5];
                    ReceiverItems = new TradeItem[5];

                    SenderMoney = 0;
                    ReceiverMoney = 0;

                    SenderReady = false;
                    ReceiverReady = false;
                }
            }
            #endregion

            /// <summary>Сущность игрока, который отправил предложение</summary>
            public PlayerData Sender { get; set; }

            /// <summary>Сущность игрока, которому отправлено предложение</summary>
            public PlayerData Receiver { get; set; }

            /// <summary>Тип предложения</summary>
            public Types Type { get; set; }

            public Trade TradeData { get; set; }

            /// <summary>CancellationTokenSource предложения</summary>
            private CancellationTokenSource CTS { get; set; }

            public object Data { get; set; }
            
            /// <summary>Новое предложение</summary>
            /// <param name="Sender">Сущность игрока, который отправил предложение</param>
            /// <param name="Receiver">Сущность игрока, которому отправлено предложение</param>
            /// <param name="Type">Тип предложения</param>
            /// <param name="Duration">Время действия предложения (если -1 - будет использовано стандартное время)</param>
            public Offer(PlayerData Sender, PlayerData Receiver, Types Type, int Duration = -1, object Data = null)
            {
                this.Sender = Sender;
                this.Receiver = Receiver;

                this.Type = Type;

                this.Data = Data;

                if (Duration == -1)
                    Duration = Settings.OFFER_DEFAULT_DURATION;

                this.CTS = new CancellationTokenSource();

                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(Duration, CTS.Token);

                        if (CTS?.IsCancellationRequested == false)
                        {
                            NAPI.Task.Run(() =>
                            {
                                if (CTS == null)
                                    return;

                                Cancel(false, false, ReplyTypes.AutoCancel, false);
                            });
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                });
            }

            /// <summary>Метод для отмены предложения и удаления его из списка активных предложения</summary>
            public void Cancel(bool success = false, bool isSender = false, ReplyTypes rType = ReplyTypes.AutoCancel, bool justCancelCts = false)
            {
                bool ctsNull = CTS == null;

                if (ctsNull)
                {               
                    OfferActions[Type].GetValueOrDefault(false)?.Invoke(Sender, Receiver, this);
                }

                CTS?.Cancel();
                CTS = null;

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

                AllOffers.Remove(this);
            }

            public void Execute()
            {
                if (CTS == null)
                    return;

                OfferActions[Type][true].Invoke(Sender, Receiver, this);
            }

            public static Offer Get(PlayerData pData) => AllOffers.Where(x => x.Sender == pData || x.Receiver == pData).FirstOrDefault();

            public static Offer Create(PlayerData pData, PlayerData tData, Types type, int duration = -1, object data = null)
            {
                var offer = new Offer(pData, tData, type, duration, data);

                AllOffers.Add(offer);

                return offer;
            }
        }

        [RemoteEvent("Offers::Send")]
        private static void Send(Player player, Player target, int type, string data)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;
            var tData = target.GetMainData();

            if (tData == null || tData.Player?.Exists != true)
                return;

            object dataObj = null;

            ReturnTypes res = ((Func<ReturnTypes>)(() =>
            {
                if (!Enum.IsDefined(typeof(Types), type))
                    return ReturnTypes.Error;

                try
                {
                    dataObj = data.DeserializeFromJson<object>();
                }
                catch (Exception ex)
                {
                    return ReturnTypes.Error;
                }

                var oType = (Types)type;

                if (oType == Types.Cash)
                {
                    try
                    {
                        dataObj = Convert.ToInt32(dataObj);
                    }
                    catch (Exception ex)
                    {
                        return ReturnTypes.Error;
                    }
                }

                if (!pData.Player.AreEntitiesNearby(tData.Player, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                    return ReturnTypes.Error;

                if (pData.IsBusy)
                    return ReturnTypes.SourceBusy;

                if (tData.IsBusy)
                    return ReturnTypes.TargetBusy;

                if (oType == Types.Cash)
                {
                    int cash = (dataObj as int?) ?? 0;

                    if (cash < 0 || cash == 0)
                        return ReturnTypes.Error;

                    if (pData.Cash < cash)
                        return ReturnTypes.NotEnoughMoneySource;
                }

                if (Offer.Get(pData) != null)
                    return ReturnTypes.SourceHasOffer;

                if (Offer.Get(tData) != null)
                    return ReturnTypes.TargetHasOffer;

                Offer.Create(pData, tData, oType, -1, dataObj);

                return ReturnTypes.Success;
            })).Invoke();

            switch (res)
            {
                case ReturnTypes.Success:
                    data = dataObj.SerializeToJson();

                    target.TriggerEvent("Offer::Show", player.Handle, type, data);

                    player.TriggerEvent("Offer::Reply::Server", true, false, false);
                    player.Notify("Offer::Sent");
                    break;

                case ReturnTypes.SourceBusy:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Player::Busy");
                    break;

                case ReturnTypes.TargetBusy:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Offer::TargetBusy");
                    break;

                case ReturnTypes.SourceHasOffer:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Offer::HasOffer");
                    break;

                case ReturnTypes.TargetHasOffer:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Offer::TargetHasOffer");
                    break;

                case ReturnTypes.Error:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    break;

                case ReturnTypes.NotEnoughMoneySource:
                    player.TriggerEvent("Offer::Reply::Server", false, false, true);
                    player.Notify("Trade::NotEnoughMoney");
                    break;
            }
        }

        [RemoteEvent("Offers::Reply")]
        private static void Reply(Player player, int rTypeNum)
        {
            var sRes = player.CheckSpamAttack();

            if (sRes.IsSpammer)
                return;

            var pData = sRes.Data;

            if (!Enum.IsDefined(typeof(ReplyTypes), rTypeNum))
                return;

            var rType = (ReplyTypes)rTypeNum;

            var offer = Offer.Get(pData);

            if (offer == null)
                return;

            var tData = offer.Sender == pData ? offer.Receiver : offer.Sender;

            if (pData == offer.Receiver)
            {
                if (rType == ReplyTypes.Accept)
                {
                   offer.Execute();
                }
                else
                {
                    offer.Cancel(false, false, rType, false);
                }
            }
            else
            {
                offer.Cancel(false, true, rType, false);
            }
        }
    }
}
