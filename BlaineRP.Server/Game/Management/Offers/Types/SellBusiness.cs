using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using BlaineRP.Server.EntityData.Players;

namespace BlaineRP.Server.Sync.Offers
{
    [Offer(Types.SellBusiness)]
    internal class SellBusiness : OfferBase
    {
        public override void OnAccept(PlayerData pData, PlayerData tData, Offer offer)
        {
            offer.Cancel(true, false, ReplyTypes.AutoCancel, true);

            if (pData == null || tData == null)
                return;

            var sPlayer = pData.Player;
            var tPlayer = tData.Player;

            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                return;

            if (!sPlayer.IsNearToEntity(tPlayer, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            if (offer.Data is Offer.PropertySellData psData)
            {
                if (psData.Data is Game.Businesses.Business businessInfo)
                {
                    if (!pData.OwnedBusinesses.Contains(businessInfo))
                        return;

                    tPlayer.CloseAll();

                    tPlayer.TriggerEvent("Estate::Show", 1, 1, businessInfo.ID, sPlayer, psData.Price);

                    offer.TradeData = new Offer.Trade()
                    {
                        SenderReady = true,

                        ReceiverMoney = psData.Price,
                    };

                    offer.TradeData.SenderBusinesses.Add(businessInfo);
                }
            }
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {
            offer.TradeData = null;

            if (tData != null)
            {
                var tPlayer = tData.Player;

                tPlayer?.CloseAll();
            }
        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            uint businessId;
            int price;

            try
            {
                var jObj = JObject.Parse(dataStr);

                businessId = jObj["BID"].ToObject<uint>();
                price = jObj["Price"].ToObject<int>();
            }
            catch (Exception ex)
            {
                returnObj = 0;

                return false;
            }

            if (price <= 0)
            {
                returnObj = 0;

                return false;
            }

            var bInfo = pData.OwnedBusinesses.Where(x => x.ID == businessId).FirstOrDefault();

            if (bInfo == null)
            {
                returnObj = 0;

                return false;
            }

            offer = Offer.Create(pData, tData, type, -1, new Offer.PropertySellData(bInfo, (ulong)price));

            text = Language.Strings.Get("OFFER_SELL_BUSINESS_TEXT");

            return true;
        }
    }
}
