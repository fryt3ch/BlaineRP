namespace BlaineRP.Server.Sync.Offers
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

            if (!sPlayer.IsNearToEntity(tPlayer, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
                return;

            Sync.Chat.SendLocal(Chat.MessageTypes.Me, sPlayer, Language.Strings.Get("CHAT_PLAYER_HEADSORTAILS_0"));

            var res = SRandom.NextInt32(0, 2) == 0;

            Sync.Chat.SendLocal(Chat.MessageTypes.Do, sPlayer, res ? Language.Strings.Get("CHAT_PLAYER_HEADSORTAILS_1") : Language.Strings.Get("CHAT_PLAYER_HEADSORTAILS_2"));
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            text = Language.Strings.Get("OFFER_HEADSORTAILS_TEXT");

            offer = Offer.Create(pData, tData, type, -1, null);

            return true;
        }
    }
}
