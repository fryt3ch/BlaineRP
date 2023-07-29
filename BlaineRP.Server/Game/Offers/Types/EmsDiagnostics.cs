using BlaineRP.Server.Game.EntitiesData.Players;
using Newtonsoft.Json.Linq;

namespace BlaineRP.Server.Game.Offers
{
    [Offer(OfferType.EmsDiagnostics)]
    internal class EmsDiagnostics : OfferBase
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

            var sourceFractionType = (Game.Fractions.FractionType)offer.Data;

            if (pData.Fraction != sourceFractionType)
            {
                tPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));
                sPlayer.NotifyError(Language.Strings.Get("NTFC_OFFER_ERROR_1"));

                return;
            }

            sPlayer.TriggerEvent("Ems::ShowPlayerDiagnostics", tPlayer.Id, JObject.FromObject(new
            {
                hp = tPlayer.Health,
                mood = tData.Mood,
                satiety = tData.Satiety,
                da = tData.DrugAddiction,
                ws = tData.IsWounded,
                dType = MedicalCard.GetPlayerDiagnose(tData),
            }));
        }

        public override void OnCancel(PlayerData pData, PlayerData tData, Offer offer)
        {

        }

        public override bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
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

            offer = Offer.Create(pData, tData, type, -1, fData.Type);

            text = Language.Strings.Get("OFFER_EMS_DIAGNOSTICS_TEXT");

            return true;
        }
    }
}