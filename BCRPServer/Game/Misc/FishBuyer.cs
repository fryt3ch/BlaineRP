using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BCRPServer.Game.Misc
{
    public class FishBuyer
    {
        public const int FISHBUYER_COEF_UPDATE_TIMEOUT = 3 * 60 * 60 * 1000;

        private static List<FishBuyer> All { get; set; }

        public static FishBuyer Get(int idx) => idx < 0 || idx >= All.Count ? null : All[idx];

        private static Timer FishBuyersPricesUpdateTimer;

        public static Dictionary<string, uint> BasePrices { get; private set; } = new Dictionary<string, uint>()
        {
            { "f_acod", 10 },
        };

        private static decimal[] PossibleCoefs = new decimal[]
        {
            1m, 0.9m, 0.8m, 0.7m, 0.6m, 0.5m,
        };

        public int Id => All.IndexOf(this);

        public decimal CurrentPriceCoef { get => decimal.Parse(Sync.World.GetSharedData<string>($"FishBuyer::{Id}::C")); set => Sync.World.SetSharedData($"FishBuyer::{Id}::C", value.ToString()); }

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

        public static void InitializeAll()
        {
            if (All != null)
                return;

            var pos1 = new Utils.Vector4(-55.288f, 1897.339f, 195.3613f, 68.66809f);

            Events.NPC.NPC.AddNpc("fishbuyer_0", new Vector3(pos1.X, pos1.Y, pos1.Z));

            All = new List<FishBuyer>()
            {
                new FishBuyer(), 
            };

            var lines = new List<string>();

            lines.Add($"FishBuyer.BasePrices = RAGE.Util.Json.Deserialize<Dictionary<string, uint>>(\"{BasePrices.SerializeToJson().Replace('"', '\'')}\");");

            lines.Add($"new FishBuyer({pos1.ToCSharpStr()});");

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() + Settings.ClientScriptsTargetLocationsLoaderPath, "FISHBUYERS_TO_REPLACE", lines);

            FishBuyersPricesUpdateTimer = new Timer((obj) =>
            {
                NAPI.Task.Run(() =>
                {
                    foreach (var x in All)
                        x.SetRandomPriceCoef();
                });
            }, null, 0, FISHBUYER_COEF_UPDATE_TIMEOUT);
        }

        public FishBuyer()
        {

        }
    }
}
