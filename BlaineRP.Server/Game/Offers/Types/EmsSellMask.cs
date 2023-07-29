using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Offers
{
    [Offer(OfferType.EmsSellMask)]
    internal class EmsSellMask : OfferBase
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

            var sourceFractionType = (Game.Fractions.FractionType)offer.Data;

            if (pData.Fraction != sourceFractionType)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));

                return;
            }

            return; //todo
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.EMS;

            if (fData == null)
            {
                returnObj = 0;

                return false;
            }

            var price = Game.Fractions.EMS.PlayerSellMaskPrice;

            offer = Offer.Create(pData, tData, type, -1, new object[] { fData.Type, price, });

            text = Language.Strings.Get("OFFER_EMS_SELLMASK_TEXT", "{0}", Language.Strings.Get("GEN_MONEY_0", price));

            return true;
        }
    }
}