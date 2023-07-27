namespace BlaineRP.Server.Game.Management.Offers
{
    public partial class Offer
    {
        public class PropertySellData
        {
            public object Data { get; set; }

            public ulong Price { get; set; }

            public PropertySellData(object Data, ulong Price)
            {
                this.Data = Data;
                this.Price = Price;
            }
        }
    }
}