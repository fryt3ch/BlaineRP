using System;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Animations;

namespace BlaineRP.Server.Game.Management.Offers
{
    [Offer(OfferType.Handshake)]
    internal class Handshake : OfferBase
    {
        private static TimeSpan AnimationTime { get; } = TimeSpan.FromMilliseconds(4_000);

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
                tPlayer.Position = sPlayer.GetFrontOf(0.85f);
                tPlayer.Heading = Utils.GetOppositeAngle(sPlayer.Heading);

                pData.PlayAnim(FastType.Handshake, AnimationTime);
                tData.PlayAnim(FastType.Handshake, AnimationTime);
            }

            pData.AddFamiliar(tData.Info);
            tData.AddFamiliar(pData.Info);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            text = Language.Strings.Get("OFFER_HANDSHAKE_TEXT");

            offer = Offer.Create(pData, tData, type, -1, null);

            return true;
        }
    }
}
