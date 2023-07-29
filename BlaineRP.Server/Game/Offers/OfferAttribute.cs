using System;

namespace BlaineRP.Server.Game.Management.Offers
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OfferAttribute : Attribute
    {
        public OfferType Type { get; set; }

        public OfferAttribute(OfferType type)
        {
            this.Type = type;
        }
    }
}