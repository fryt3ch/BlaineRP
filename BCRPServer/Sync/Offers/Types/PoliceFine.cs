using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.PoliceFine)]
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

            if (!sPlayer.AreEntitiesNearby(tPlayer, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.Police;

            if (fData == null)
                return;

            if (!fData.HasMemberPermission(pData.Info, 14, true))
                return;

            var d = ((string)offer.Data).Split('_');

            var amount = uint.Parse(d[0]);

            var reason = d[1];

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

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

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
                returnObj = null;

                return false;
            }

            if (amount < Game.Fractions.Police.FineMinAmount || amount > Game.Fractions.Police.FineMaxAmount)
            {
                returnObj = null;

                return false;
            }

            if (!Game.Fractions.Police.FineReasonRegex.IsMatch(reason))
            {
                returnObj = null;

                return false;
            }

            offer.Data = $"{amount}_{reason}";

            tData.Player.TriggerEvent("Offer::Show", pData.Player.Handle, type, offer.Data);

            customTargetShow = true;

            return true;
        }
    }
}
