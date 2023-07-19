using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
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
                return;
            }

            var hpToRestore = (int)((object[])offer.Data)[1];
            var price = (uint)((object[])offer.Data)[0];

            var curHealth = tData.Player.Health;

            var hpDiff = Utils.GetCorrectDiff(curHealth, hpToRestore, 1, Properties.Settings.Static.PlayerMaxHealth);

            if (hpDiff != hpToRestore)
            {
                return;
            }

            ulong newBalanceT;

            if (!tData.TryRemoveCash(price, out newBalanceT, true, pData))
                return;

            var totalEarn = (uint)Math.Round(Game.Fractions.EMS.PlayerHealPriceFee * price);

            var totalFee = price - totalEarn;

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

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

            if (!baseRes)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.EMS;

            if (fData == null)
            {
                returnObj = 0;

                return false;
            }

            var curHealth = tData.Player.Health;

            var hpDiff = Utils.GetCorrectDiff(curHealth, Properties.Settings.Static.PlayerMaxHealth, 1, Properties.Settings.Static.PlayerMaxHealth);

            if (hpDiff <= 0)
            {
                returnObj = 0;

                return false;
            }

            var healPrice = (uint)Math.Round(Game.Fractions.EMS.PlayerHealOn1HpPrice - ((Game.Fractions.EMS.PlayerHealOn1HpPrice - Game.Fractions.EMS.PlayerHealOn99HpPrice) * (curHealth - 1d) / (Game.Fractions.EMS.PlayerHealOn99HpPrice - 1d)));

            offer.Data = new object[] { healPrice, hpDiff, fData.Type, };

            tData.Player.TriggerEvent("Offer::Show", pData.Player.Handle, type, healPrice);

            return false; // todo
        }
    }
}