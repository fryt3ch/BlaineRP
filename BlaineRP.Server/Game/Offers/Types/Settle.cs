using BlaineRP.Server.Game.EntitiesData.Players;

namespace BlaineRP.Server.Game.Offers
{
    [Offer(OfferType.Settle)]
    internal class Settle : OfferBase
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

            var houseBase = (Game.Estates.HouseBase)offer.Data;

            if (houseBase == null || houseBase != pData.CurrentHouseBase)
                return;

            if (houseBase.Owner != pData.Info)
                return;

            if (!tData.CanBeSettled(houseBase, true))
                return;

            houseBase.SettlePlayer(tData.Info, true, pData);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }
        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            var curHouseBase = pData.CurrentHouseBase;

            if (curHouseBase == null)
            {
                returnObj = 0;

                return false;
            }

            if (curHouseBase.Owner != pData.Info)
            {
                pData.Player.Notify("House::NotAllowed");

                returnObj = 1;

                return false;
            }

            if (curHouseBase.Type == Game.Estates.HouseBase.Types.House)
            {
                text = Language.Strings.Get("OFFER_SETTLE_TEXT_0");
            }
            else if (curHouseBase is Game.Estates.Apartments aps)
            {
                text = Language.Strings.Get("OFFER_SETTLE_TEXT_1");
            }

            offer = Offer.Create(pData, tData, type, -1, curHouseBase);

            return true;
        }
    }
}
