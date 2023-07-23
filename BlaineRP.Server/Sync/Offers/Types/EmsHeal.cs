using System;

namespace BlaineRP.Server.Sync.Offers
{
    [Offer(Types.EmsHeal)]
    internal class EmsHeal : OfferBase
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

            var sourceFractionType = (Game.Fractions.Types)((object[])offer.Data)[2];

            if (pData.Fraction != sourceFractionType)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));

                return;
            }

            var hpToRestore = (int)((object[])offer.Data)[1];
            var price = (uint)((object[])offer.Data)[0];

            var curHealth = tData.Player.Health;

            var hpDiff = Utils.CalculateDifference(curHealth, hpToRestore, 1, Properties.Settings.Static.PlayerMaxHealth);

            if (hpDiff != hpToRestore)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_0"));

                return;
            }

            ulong newBalanceT;

            if (!tData.TryRemoveCash(price, out newBalanceT, true, pData))
                return;

            var totalEarn = (uint)Math.Round(Game.Fractions.EMS.PlayerHealPricePlayerGetCoef * price);

            ulong newBalanceP;

            if (!pData.TryAddCash(totalEarn, out newBalanceP, true, tData))
                return;

            tData.SetCash(newBalanceT);
            pData.SetCash(newBalanceP);

            tData.Player.SetHealth(curHealth + hpToRestore);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out string text)
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

            var curHealth = tData.Player.Health;

            var diff = Utils.CalculateDifference(curHealth, Properties.Settings.Static.PlayerMaxHealth, 1, Properties.Settings.Static.PlayerMaxHealth);

            if (diff <= 0)
            {
                pData.Player.NotifyError(Language.Strings.Get("NTFC_OFFER_EMS_HEAL_0"));

                returnObj = 1;

                return false;
            }

            var price = (uint)Math.Round(Game.Fractions.EMS.PlayerHealMaxPrice - ((Game.Fractions.EMS.PlayerHealMaxPrice - Game.Fractions.EMS.PlayerHealMinPrice) * (curHealth - 1d) / (Game.Fractions.EMS.PlayerHealMinPrice - 1d)));

            offer = Offer.Create(pData, tData, type, -1, new object[] { price, diff, fData.Type, });

            text = Language.Strings.Get("OFFER_EMS_HEAL_TEXT", "{0}", Language.Strings.Get("GEN_MONEY_0", price));

            return true;
        }
    }
}