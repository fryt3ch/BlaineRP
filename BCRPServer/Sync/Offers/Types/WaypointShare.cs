using GTANetworkAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.WaypointShare)]
    internal class WaypointShare : OfferBase
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

            var pos = offer.Data as Vector3;

            if (pos == null)
                return;

            tPlayer.TriggerEvent("Player::Waypoint::Set", pos.X, pos.Y);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

            if (!baseRes)
                return false;

            float posX, posY;

            try
            {
                var jObj = JObject.Parse(dataStr);

                posX = jObj["X"].ToObject<float>();
                posY = jObj["Y"].ToObject<float>();
            }
            catch (Exception ex)
            {
                returnObj = null;

                return false;
            }

            var offerData = new Vector3(posX, posY, 0f);

            offer.Data = offerData;

            return true;
        }
    }
}
