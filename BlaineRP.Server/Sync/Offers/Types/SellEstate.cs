using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace BlaineRP.Server.Sync.Offers
{
    [Offer(Types.SellEstate)]
    internal class SellEstate : OfferBase
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
                if (psData.Data is Game.Estates.HouseBase houseBase)
                {
                    if (!pData.OwnedHouses.Contains(houseBase) && !pData.OwnedApartments.Contains(houseBase))
                        return;

                    tPlayer.CloseAll();

                    tPlayer.TriggerEvent("Estate::Show", 1, houseBase.Type == Game.Estates.HouseBase.Types.House ? 2 : 3, houseBase.Id, sPlayer, psData.Price);

                    offer.TradeData = new Offer.Trade()
                    {
                        SenderReady = true,

                        ReceiverMoney = psData.Price,
                    };

                    offer.TradeData.SenderHouseBases.Add(houseBase);
                }
                else if (psData.Data is Game.Estates.Garage garage)
                {
                    if (!pData.OwnedGarages.Contains(garage))
                        return;

                    tPlayer.CloseAll();

                    tPlayer.TriggerEvent("Estate::Show", 1, 4, garage.Id, sPlayer, psData.Price);

                    offer.TradeData = new Offer.Trade()
                    {
                        SenderReady = true,

                        ReceiverMoney = psData.Price,
                    };

                    offer.TradeData.SenderGarages.Add(garage);
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

            PlayerData.PropertyTypes estType;
            uint estId;
            int price;

            try
            {
                var jObj = JObject.Parse(dataStr);

                estType = jObj["EST"].ToObject<PlayerData.PropertyTypes>();
                estId = jObj["UID"].ToObject<uint>();
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

            if (estType == PlayerData.PropertyTypes.House)
            {
                var house = pData.OwnedHouses.Where(x => x.Id == estId).FirstOrDefault();

                if (house == null)
                {
                    returnObj = 0;

                    return false;
                }

                offer = Offer.Create(pData, tData, type, -1, new Offer.PropertySellData(house, (ulong)price));

                text = Language.Strings.Get("OFFER_SELL_ESTATE_TEXT");

                return true;
            }
            else if (estType == PlayerData.PropertyTypes.Apartments)
            {
                var aps = pData.OwnedApartments.Where(x => x.Id == estId).FirstOrDefault();

                if (aps == null)
                {
                    returnObj = 0;

                    return false;
                }

                offer = Offer.Create(pData, tData, type, -1, new Offer.PropertySellData(aps, (ulong)price));

                text = Language.Strings.Get("OFFER_SELL_ESTATE_TEXT");

                return true;
            }
            else if (estType == PlayerData.PropertyTypes.Garage)
            {
                var garage = pData.OwnedGarages.Where(x => x.Id == estId).FirstOrDefault();

                if (garage == null)
                {
                    returnObj = 0;

                    return false;
                }

                offer = Offer.Create(pData, tData, type, -1, new Offer.PropertySellData(garage, (ulong)price));

                text = Language.Strings.Get("OFFER_SELL_ESTATE_TEXT");

                return true;
            }
            else
            {
                returnObj = 0;

                return false;
            }
        }
    }
}
