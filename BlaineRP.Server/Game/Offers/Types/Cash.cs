using System;
using BlaineRP.Server.EntitiesData.Players;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.Management.Offers
{
    [Offer(OfferType.Cash)]
    internal class Cash : OfferBase
    {
        public override void OnAccept(PlayerData pData, PlayerData tData, Offer offer)
        {
            offer.Cancel(true, false, ReplyTypes.AutoCancel, false);

            if (pData == null || tData == null)
                return;

            var sPlayer = pData.Player;
            var tPlayer = tData.Player;

            if (sPlayer?.Exists != true || tPlayer?.Exists != true)
                return;

            if (!sPlayer.IsNearToEntity(tPlayer, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var cash = Convert.ToUInt64(offer.Data);

            if (cash == 0)
                return;

            ulong pNewCash, tNewCash;

            if (!pData.TryRemoveCash(cash, out pNewCash, true, tData))
                return;

            if (!tData.TryAddCash(cash, out tNewCash, true, pData))
                return;

            pData.SetCash(pNewCash);
            tData.SetCash(tNewCash);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            int amount;

            try
            {
                var jObj = JObject.Parse(dataStr);

                amount = jObj["Amount"].ToObject<int>();
            }
            catch (Exception ex)
            {
                returnObj = null;

                return false;
            }

            if (amount <= 0)
            {
                returnObj = null;

                return false;
            }

            var amountU = (ulong)amount;

            if (!pData.TryRemoveCash(amountU, out _, true))
            {
                returnObj = null;

                return false;
            }

            offer = Offer.Create(pData, tData, type, -1, amountU);

            text = Language.Strings.Get("OFFER_CASH_TEXT", "{0}", Language.Strings.Get("GEN_MONEY_0", amountU));

            return true;
        }
    }
}