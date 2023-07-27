using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Threading;
using BlaineRP.Server.UtilsT;

namespace BlaineRP.Server.Game.Misc
{
    public partial class FishBuyer
    {
        public const int FISHBUYER_COEF_UPDATE_TIMEOUT = 3 * 60 * 60 * 1000;

        private static List<FishBuyer> All { get; set; }

        public static FishBuyer Get(int idx) => idx < 0 || idx >= All.Count ? null : All[idx];

        private static Timer FishBuyersPricesUpdateTimer;

        [Properties.Settings.Static.ClientSync("fishBuyersBasePrices")]
        public static Dictionary<string, uint> BasePrices { get; private set; } = new Dictionary<string, uint>()
        {
            { "f_acod", 10 },
        };

        private static decimal[] PossibleCoefs = new decimal[]
        {
            1m, 0.9m, 0.8m, 0.7m, 0.6m, 0.5m,
        };

        public int Id => All.IndexOf(this);

        public decimal CurrentPriceCoef { get => decimal.Parse(World.Service.GetSharedData<string>($"FishBuyer::{Id}::C")); set => World.Service.SetSharedData($"FishBuyer::{Id}::C", value.ToString()); }

        public decimal SetRandomPriceCoef()
        {
            var coef = PossibleCoefs[SRandom.NextInt32(0, PossibleCoefs.Length)];

            CurrentPriceCoef = coef;

            return coef;
        }

        public bool TryGetPrice(string fishId, out uint price)
        {
            if (!BasePrices.TryGetValue(fishId, out price))
                return false;

            price = (uint)Math.Floor(price * CurrentPriceCoef);

            return true;
        }

        public FishBuyer()
        {

        }
    }
}
