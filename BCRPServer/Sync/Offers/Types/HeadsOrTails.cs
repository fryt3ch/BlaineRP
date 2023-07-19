using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.HeadsOrTails)]
    internal class HeadsOrTails : OfferBase
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

            Sync.Chat.SendLocal(Chat.MessageTypes.Me, sPlayer, Language.Strings.Get("CHAT_PLAYER_HEADSORTAILS_0"));

            var res = SRandom.NextInt32(0, 2) == 0;

            Sync.Chat.SendLocal(Chat.MessageTypes.Do, sPlayer, res ? Language.Strings.Get("CHAT_PLAYER_HEADSORTAILS_1") : Language.Strings.Get("CHAT_PLAYER_HEADSORTAILS_2"));
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            return base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);
        }
    }
}
