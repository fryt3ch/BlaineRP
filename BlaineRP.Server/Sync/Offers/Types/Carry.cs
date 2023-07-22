using System;
using System.Collections.Generic;
using System.Text;

namespace BlaineRP.Server.Sync.Offers
{
    [Offer(Types.Carry)]
    internal class Carry : OfferBase
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

            if (tPlayer.Vehicle == null && sPlayer.Vehicle == null && pData.CanPlayAnimNow() && tData.CanPlayAnimNow())
            {
                sPlayer.AttachEntity(tPlayer, AttachSystem.Types.Carry, null);
            }
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            text = Language.Strings.Get("OFFER_CARRY_TEXT");

            offer = Offer.Create(pData, tData, type, -1, null);

            return true;
        }
    }
}
