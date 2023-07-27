using System;
using BlaineRP.Server.EntitiesData.Players;

namespace BlaineRP.Server.Game.Management.Offers
{
    [Offer(OfferType.EmsPsychHeal)]
    internal class EmsPsychHeal : OfferBase
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

            var sourceFractionType = (Game.Fractions.FractionType)((object[])offer.Data)[2];

            if (pData.Fraction != sourceFractionType)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));

                return;
            }

            var moodToSet = (byte)((object[])offer.Data)[1];
            var price = (uint)((object[])offer.Data)[0];

            var curMood = tData.Mood;

            if (curMood >= moodToSet)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_0"));

                return;
            }

            ulong newBalanceT;

            if (!tData.TryRemoveCash(price, out newBalanceT, true, pData))
                return;

            var totalEarn = (uint)Math.Round(Game.Fractions.EMS.PlayerPsychHealPricePlayerGetCoef * price);

            ulong newBalanceP;

            if (!pData.TryAddCash(totalEarn, out newBalanceP, true, tData))
                return;

            tData.SetCash(newBalanceT);
            pData.SetCash(newBalanceP);

            tData.Mood = moodToSet;
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

            var curMood = tData.Mood;

            var setMoodAmount = Game.Fractions.EMS.PlayerPsychHealSetAmount;

            if (curMood >= setMoodAmount)
            {
                pData.Player.NotifyError("NTFC_OFFER_EMS_PSYCHHEAL_0");

                returnObj = 1;

                return false;
            }

            var price = Game.Fractions.EMS.PlayerPsychHealPrice;

            offer = Offer.Create(pData, tData, type, -1, new object[] { price, setMoodAmount, fData.Type, });

            text = Language.Strings.Get("OFFER_EMS_PSYCHHEAL_TEXT", "{0}", Language.Strings.Get("GEN_MONEY_0", price));

            return true;
        }
    }
}