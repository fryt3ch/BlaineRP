using System;
using BlaineRP.Server.EntitiesData.Players;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.Management.Offers
{
    [Offer(OfferType.PoliceFine)]
    internal class PoliceFine : OfferBase
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

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

            if (fData == null)
                return;

            if (!fData.HasMemberPermission(pData.Info, 14, true))
                return;

            var amount = (uint)((object[])offer.Data)[0];

            var reason = (string)((object[])offer.Data)[1];

            ulong newBalanceT, newBalanceS;

            if (!tData.TryRemoveCash(amount, out newBalanceT, true, pData))
                return;

            if (!pData.TryAddCash(amount, out newBalanceS, true, tData))
                return;

            tData.SetCash(newBalanceT);
            pData.SetCash(newBalanceS);

            fData.AddFine(pData, tData, new Game.Fractions.Police.FineInfo() { Amount = amount, Reason = reason, Time = Utils.GetCurrentTime(), Member = pData.Player.Name, Target = tData.Player.Name, });
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

            if (fData == null)
            {
                returnObj = 0;

                return false;
            }

            if (!fData.HasMemberPermission(pData.Info, 14, true))
            {
                returnObj = 1;

                return false;
            }

            int amount;
            string reason;

            try
            {
                var jObj = JObject.Parse(dataStr);

                amount = jObj["Amount"].ToObject<int>();
                reason = jObj["Reason"].ToObject<string>().Trim();
            }
            catch (Exception ex)
            {
                returnObj = 0;

                return false;
            }

            if (amount < Game.Fractions.Police.FineMinAmount || amount > Game.Fractions.Police.FineMaxAmount)
            {
                returnObj = 0;

                return false;
            }

            if (!Game.Fractions.Police.FineReasonRegex.IsMatch(reason))
            {
                returnObj = 0;

                return false;
            }

            offer = Offer.Create(pData, tData, type, -1, new object[] { (uint)amount, reason, });

            text = Language.Strings.Get("OFFER_POLICE_FINE_TEXT", "{0}", Language.Strings.Get("GEN_MONEY_0", amount), reason);

            return true;
        }
    }
}
