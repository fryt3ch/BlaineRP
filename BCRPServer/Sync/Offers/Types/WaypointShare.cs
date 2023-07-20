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

            if (!sPlayer.IsNearToEntity(tPlayer, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            var pos = offer.Data as Vector3;

            if (pos == null)
                return;

            tPlayer.TriggerEvent("Player::Waypoint::Set", pos.X, pos.Y);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

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
                returnObj = 0;

                return false;
            }

            offer = Offer.Create(pData, tData, type, -1, new Vector3(posX, posY, 0f));

            text = Language.Strings.Get("OFFER_WAYPOINTSHARE_TEXT");

            return true;
        }
    }
}
