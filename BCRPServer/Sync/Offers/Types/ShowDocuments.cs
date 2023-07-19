using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.ShowPassport)]
    internal class ShowPassport : OfferBase
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

            pData.ShowPassport(tPlayer);

            tData.AddFamiliar(pData.Info);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            return base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);
        }
    }

    [Offer(Types.ShowLicenses)]
    internal class ShowLicenses : OfferBase
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

            pData.ShowLicences(tPlayer);

            tData.AddFamiliar(pData.Info);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            return base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);
        }
    }

    [Offer(Types.ShowMedicalCard)]
    internal class ShowMedicalCard : OfferBase
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

            if (pData.Info.MedicalCard == null)
                return;

            pData.Info.MedicalCard.Show(tPlayer, pData.Info);

            tData.AddFamiliar(pData.Info);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            return base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);
        }
    }

    [Offer(Types.ShowVehiclePassport)]
    internal class ShowVehiclePassport : OfferBase
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

            if (offer.Data is VehicleData.VehicleInfo vInfo)
            {
                if (!pData.OwnedVehicles.Contains(vInfo))
                    return;

                vInfo.ShowPassport(tPlayer);
            }
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

            if (!baseRes)
                return false;

            uint vid;

            try
            {
                var jObj = JObject.Parse(dataStr);

                vid = jObj["VID"].ToObject<uint>();
            }
            catch (Exception ex)
            {
                returnObj = null;

                return false;
            }

            var found = pData.OwnedVehicles.Where(x => x.VID == vid).FirstOrDefault();

            if (found == null)
            {
                returnObj = 0;

                return false;
            }

            offer.Data = found;

            return true;
        }
    }

    [Offer(Types.ShowFractionDocs)]
    internal class ShowFractionDocs : OfferBase
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

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (fData == null || fData.Type != (Game.Fractions.Types)offer.Data)
                return;

            pData.ShowFractionDocs(tPlayer, fData, pData.Info.FractionRank);

            tData.AddFamiliar(pData.Info);
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out customTargetShow);

            if (!baseRes)
                return false;

            if (!Game.Fractions.Fraction.IsMemberOfAnyFraction(pData, true))
            {
                returnObj = 1;

                return false;
            }

            var fData = Game.Fractions.Fraction.Get(pData.Fraction);

            if (!fData.MetaFlags.HasFlag(Game.Fractions.MetaFlagTypes.MembersHaveDocs))
            {
                returnObj = 0;

                return false;
            }

            offer.Data = fData.Type;

            return true;
        }
    }
}
