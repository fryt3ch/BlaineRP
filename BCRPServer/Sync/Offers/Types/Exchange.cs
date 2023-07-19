using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.Exchange)]
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

            offer.TradeData = new Offer.Trade();
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

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            return base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);
        }
    }
}