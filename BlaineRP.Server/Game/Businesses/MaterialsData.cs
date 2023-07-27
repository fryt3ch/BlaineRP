using System.Collections.Generic;

namespace BlaineRP.Server.Game.Businesses
{
    public class MaterialsData
    {
        public uint BuyPrice { get; set; }

        public uint SellPrice { get; set; }

        public uint RealPrice { get; set; }

        public uint MaxMaterialsBalance { get; set; }

        public uint MaxMaterialsPerOrder { get; set; }

        public Dictionary<string, uint> Prices { get; set; }

        public MaterialsData(uint BuyPrice, uint SellPrice, uint RealPrice, uint MaxMaterialsBalance = 500_000, uint maxMaterialsPerOrder = 100_000) // todo maxMatBalance&maxMatPerOrder - individual
        {
            this.BuyPrice = BuyPrice;
            this.SellPrice = SellPrice;
            this.RealPrice = RealPrice;

            this.Prices = Prices;
            this.MaxMaterialsBalance = MaxMaterialsBalance;
            this.MaxMaterialsPerOrder = maxMaterialsPerOrder;
        }
    }
}