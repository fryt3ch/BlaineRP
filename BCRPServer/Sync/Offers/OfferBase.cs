using System;
using System.Collections.Generic;
using System.Text;

namespace BCRPServer.Sync.Offers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OfferAttribute : Attribute
    {
        public Types Type { get; set; }

        public OfferAttribute(Types type)
        {
            this.Type = type;
        }
    }

    public abstract class OfferBase
    {
        public abstract void OnAccept(PlayerData pData, PlayerData tData, Offer offer);

        public abstract void OnCancel(PlayerData pData, PlayerData tData, Offer offer);

        public virtual bool IsRequestCorrect(PlayerData pData, PlayerData tData, Types type, string dataStr, out Offer offer, out object returnObj, out bool customTargetShow)
        {
            offer = null;
            customTargetShow = false;

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

            if (tData.PhoneStateType != Sync.Players.PhoneStateTypes.Off || tData.CurrentWorkbench != null || tData.CurrentContainer != null || tData.IsAttachedToEntity != null || tData.CurrentBusiness != null || tData.IsFrozen)
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

            offer = Offer.Create(pData, tData, type, -1, null);

            return true;
        }
    }
}
