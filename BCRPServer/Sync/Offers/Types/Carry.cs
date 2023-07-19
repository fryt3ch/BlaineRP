﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.Carry)]
    internal class Carry : OfferBase
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

            if (tPlayer.Vehicle == null && sPlayer.Vehicle == null && pData.CanPlayAnimNow() && tData.CanPlayAnimNow())
            {
                sPlayer.AttachEntity(tPlayer, AttachSystem.Types.Carry, null);
            }
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
