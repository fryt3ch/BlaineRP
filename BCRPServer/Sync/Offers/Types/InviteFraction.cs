using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.InviteFraction)]
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

            if (!sPlayer.AreEntitiesNearby(tPlayer, Settings.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null || fData.Type != (Game.Fractions.Types)offer.Data)
                return;

            if (tData.Fraction != Game.Fractions.Types.None)
                return;

            fData.SetPlayerFraction(tData.Info, 0);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

            if (!baseRes)
                return false;

            if (tData.Fraction != Game.Fractions.Types.None)
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

            offer.Data = fData.Type;

            tData.Player.TriggerEvent("Offer::Show", pData.Player.Handle, type, offer.Data);

            customTargetShow = true;

            return true;
        }
    }
}
