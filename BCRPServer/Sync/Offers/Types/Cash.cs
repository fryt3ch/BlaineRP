﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.Cash)]
    internal class Cash : OfferBase
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

            var cash = Convert.ToUInt64(offer.Data);

            if (cash == 0)
                return;

            ulong pNewCash, tNewCash;

            if (!pData.TryRemoveCash(cash, out pNewCash, true, tData))
                return;

            if (!tData.TryAddCash(cash, out tNewCash, true, pData))
                return;

            pData.SetCash(pNewCash);
            tData.SetCash(tNewCash);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

            if (!baseRes)
                return false;

            int amount;

            try
            {
                var jObj = JObject.Parse(dataStr);

                amount = jObj["Amount"].ToObject<int>();
            }
            catch (Exception ex)
            {
                returnObj = null;

                return false;
            }

            if (amount <= 0)
            {
                returnObj = null;

                return false;
            }

            var amountU = (ulong)amount;

            if (!pData.TryRemoveCash(amountU, out _, true))
            {
                returnObj = null;

                return false;
            }

            offer.Data = amountU;

            tData.Player.TriggerEvent("Offer::Show", pData.Player.Handle, type, amountU);

            customTargetShow = true;

            return true;
        }
    }
}