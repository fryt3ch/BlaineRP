using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.Settle)]
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

            var houseBase = pData.CurrentHouseBase;

            if (houseBase == null || houseBase.Owner != pData.Info)
                return;

            if (!tData.CanBeSettled(houseBase, true))
                return;

            houseBase.SettlePlayer(tData.Info, true, pData);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }
        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

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

            tData.Player.TriggerEvent("Offer::Show", pData.Player.Handle, type, curHouseBase.Type);

            customTargetShow = true;

            return true;
        }
    }
}
