using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Management.Offers
{
    [Offer(OfferType.Exchange)]
    internal class Exchange : OfferBase
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

            sPlayer.CloseAll();
            tPlayer.CloseAll();

            sPlayer.TriggerEvent("Inventory::Show", 3);
            tPlayer.TriggerEvent("Inventory::Show", 3);

            offer.TradeData = new Trade();
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {
            offer.TradeData = null;

            if (pData != null)
            {
                var sPlayer = pData.Player;

                sPlayer?.CloseAll();
            }

            if (tData != null)
            {
                var tPlayer = tData.Player;

                tPlayer?.CloseAll();
            }
        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            text = Language.Strings.Get("OFFER_EXCHANGE_TEXT");

            offer = Offer.Create(pData, tData, type, -1, null);

            return true;
        }
    }
}