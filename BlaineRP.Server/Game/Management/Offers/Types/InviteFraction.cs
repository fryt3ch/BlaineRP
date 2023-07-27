using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Management.Offers
{
    [Offer(OfferType.InviteFraction)]
    internal class InviteFraction : OfferBase
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

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null || fData.Type != (Game.Fractions.FractionType)offer.Data)
                return;

            if (tData.Fraction != Game.Fractions.FractionType.None)
                return;

            fData.SetPlayerFraction(tData.Info, 0);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            if (tData.Fraction != Game.Fractions.FractionType.None)
            {
                returnObj = 0;

                return false;
            }

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
            {
                returnObj = 1;

                return false;
            }

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (!fData.HasMemberPermission(pData.Info, 2, true))
            {
                returnObj = 1;

                return false;
            }

            offer = Offer.Create(pData, tData, type, -1, fData.Type);

            text = Language.Strings.Get("OFFER_INVITEFRACTION_TEXT", "{0}", fData.Name);

            return true;
        }
    }
}
