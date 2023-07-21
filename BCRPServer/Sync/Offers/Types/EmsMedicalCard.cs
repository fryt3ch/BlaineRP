using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [Offer(Types.EmsMedicalCard)]
    internal class EmsMedicalCard : OfferBase
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

            var sourceFractionType = (Game.Fractions.Types)offer.Data;

            if (pData.Fraction != sourceFractionType)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));

                return;
            }

            if (pData.IsWounded)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));

                return;
            }

            PlayerData.MedicalCard.DiagnoseTypes diagType = PlayerData.MedicalCard.GetPlayerDiagnose(tData);

            var curTime = Utils.GetCurrentTime();

            var medicalCard = new PlayerData.MedicalCard(curTime, pData.Info, diagType);

            tData.UpdateMedicalCard(medicalCard);

            tPlayer.NotifySuccess(Language.Strings.Get("NTFC_OFFER_EMS_MEDCARD_0"));
            sPlayer.NotifySuccess(Language.Strings.Get("NTFC_OFFER_EMS_MEDCARD_1"));
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            var baseRes = base.IsRequestCorrect(pData, tData, type, dataStr, out offer, out returnObj, out text);

            if (!baseRes)
                return false;

            var fData = Game.Fractions.Fraction.Get(pData.Fraction) as Game.Fractions.EMS;

            if (fData == null)
            {
                returnObj = 0;

                return false;
            }

            if (tData.IsWounded)
            {
                pData.Player.NotifyError(Language.Strings.Get("NTFC_OFFER_EMS_MEDCARD_E_0"));

                return false;
            }

            offer = Offer.Create(pData, tData, type, -1, fData.Type);

            text = Language.Strings.Get("OFFER_EMS_MEDCARD_TEXT");

            return true;
        }
    }
}