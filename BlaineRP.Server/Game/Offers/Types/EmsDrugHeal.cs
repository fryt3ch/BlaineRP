using System;
using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Extensions.System;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Offers
{
    [Offer(OfferType.EmsDrugHeal)]
    internal class EmsDrugHeal : OfferBase
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

            var sourceFractionType = (Game.Fractions.FractionType)((object[])offer.Data)[1];

            if (pData.Fraction != sourceFractionType)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));

                return;
            }

            var price = (uint)((object[])offer.Data)[0];

            var curDrugAddiction = tData.DrugAddiction;

            var diff = (byte)Math.Abs(Utils.CalculateDifference(curDrugAddiction, -Game.Fractions.EMS.PlayerDrugHealAmount, 0, Properties.Settings.Static.PlayerMaxDrugAddiction));

            if (diff == 0)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_0"));

                return;
            }

            var cdHashA = NAPI.Util.GetHashKey("EMS_DRUGAD_HEAL_A");

            var curTime = Utils.GetCurrentTime();

            TimeSpan cdATimeLeft;

            if (tData.Info.HasCooldown(cdHashA, curTime, out _, out cdATimeLeft, out _, 1d))
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_COOLDOWN_GEN_2", cdATimeLeft.GetBeautyString()));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_0"));

                return;
            }

            ulong newBalanceT;

            if (!tData.TryRemoveCash(price, out newBalanceT, true, pData))
                return;

            var totalEarn = (uint)Math.Round(Game.Fractions.EMS.PlayerDrugHealPricePlayerGetCoef * price);

            ulong newBalanceP;

            if (!pData.TryAddCash(totalEarn, out newBalanceP, true, tData))
                return;

            tData.SetCash(newBalanceT);
            pData.SetCash(newBalanceP);

            tData.DrugAddiction -= diff;

            tData.Info.SetCooldown(cdHashA, curTime, Game.Fractions.EMS.PlayerDrugHealCooldownA, true);
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

            var curDrugAddiction = tData.DrugAddiction;

            var diff = (byte)Math.Abs(Utils.CalculateDifference(curDrugAddiction, -Game.Fractions.EMS.PlayerDrugHealAmount, 0, Properties.Settings.Static.PlayerMaxDrugAddiction));

            if (diff == 0)
            {
                pData.Player.NotifyError(Language.Strings.Get("NTFC_OFFER_EMS_DRUGHEAL_0"));

                returnObj = 1;

                return false;
            }

            var price = Game.Fractions.EMS.PlayerDrugHealPrice;

            offer = Offer.Create(pData, tData, type, -1, new object[] { price, fData.Type, });

            text = Language.Strings.Get("OFFER_EMS_DRUGHEAL_TEXT", "{0}", Language.Strings.Get("GEN_MONEY_0", price));

            return true;
        }
    }
}