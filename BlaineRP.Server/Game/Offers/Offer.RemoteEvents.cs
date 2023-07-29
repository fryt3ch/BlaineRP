using System;
using BlaineRP.Server.EntitiesData.Players;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Management.Offers
{
    public partial class Offer
    {
        internal class RemoteEvents : Script
        {
            [RemoteProc("Offers::Send")]
            private static object Send(Player player, Player target, int typeNum, string dataStr)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return null;

                if (!Enum.IsDefined(typeof(OfferType), typeNum))
                    return null;

                PlayerData pData = sRes.Data;

                PlayerData tData = target.GetMainData();

                if (tData == null || tData == pData)
                    return null;

                var oType = (OfferType)typeNum;

                OfferBase offerBaseData = GetOfferBaseDataByType(oType);

                if (offerBaseData == null)
                    return null;

                Offer offer;
                object returnObj;
                string text;

                bool isRequestCorrect = offerBaseData.IsRequestCorrect(pData, tData, oType, dataStr, out offer, out returnObj, out text);

                if (!isRequestCorrect || offer == null)
                {
                    return returnObj;
                }
                else
                {
                    tData.Player.TriggerEvent("Offer::Show", player.Id, typeNum, text);

                    return returnObj;
                }
            }

            [RemoteEvent("Offers::Reply")]
            private static void Reply(Player player, int rTypeNum)
            {
                (bool IsSpammer, PlayerData Data) sRes = player.CheckSpamAttack();

                if (sRes.IsSpammer)
                    return;

                PlayerData pData = sRes.Data;

                if (!Enum.IsDefined(typeof(ReplyTypes), rTypeNum))
                    return;

                var rType = (ReplyTypes)rTypeNum;

                Offer offer = pData.ActiveOffer;

                if (offer == null)
                    return;

                if (pData == offer.Receiver)
                {
                    if (rType == ReplyTypes.Accept)
                        offer.Execute();
                    else
                        offer.Cancel(false, false, rType, false);
                }
                else
                {
                    offer.Cancel(false, true, rType, false);
                }
            }
        }
    }
}