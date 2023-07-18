using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync.Offers
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

            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
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

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

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
                returnObj = null;

                return false;
            }

            if (price <= 0)
            {
                returnObj = null;

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

                offer.Data = new Offer.PropertySellData(house, (ulong)price);

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

                offer.Data = new Offer.PropertySellData(aps, (ulong)price);

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

                offer.Data = new Offer.PropertySellData(garage, (ulong)price);

                return true;
            }
            else
            {
                returnObj = null;

                return false;
            }
        }
    }
}
