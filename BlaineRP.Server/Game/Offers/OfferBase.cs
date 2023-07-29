using BlaineRP.Server.EntitiesData.Players;
using BlaineRP.Server.Game.Management.Phone;
using BlaineRP.Server.Sync;

namespace BlaineRP.Server.Game.Management.Offers
{
    public abstract class OfferBase
    {
        public abstract void OnAccept(PlayerData pData, PlayerData tData, Offer offer);

        public abstract void OnCancel(PlayerData pData, PlayerData tData, Offer offer);

        public virtual bool IsRequestCorrect(PlayerData pData, PlayerData tData, OfferType type, string dataStr, out Offer offer, out object returnObj, out string text)
        {
            offer = null;
            text = string.Empty;

            if (pData.IsFrozen || pData.IsCuffed || pData.IsKnocked)
            {
                returnObj = 0;

                return false;
            }

            if (!pData.Player.IsNearToEntity(tData.Player, Properties.Settings.Static.ENTITY_INTERACTION_MAX_DISTANCE))
            {
                returnObj = 0;

                return false;
            }

            if (tData.PhoneStateType != PlayerPhoneState.Off || tData.CurrentWorkbench != null || tData.CurrentContainer != null || tData.IsAttachedToEntity != null || tData.CurrentBusiness != null || tData.IsFrozen)
            {
                returnObj = 1;

                pData.Player.Notify("Offer::TargetBusy");

                return false;
            }

            if (pData.ActiveOffer != null)
            {
                returnObj = 1;

                pData.Player.Notify("Offer::HasOffer");

                return false;
            }

            if (tData.ActiveOffer != null)
            {
                returnObj = 1;

                pData.Player.Notify("Offer::TargetHasOffer");

                return false;
            }

            returnObj = 255;

            return true;
        }
    }
}
