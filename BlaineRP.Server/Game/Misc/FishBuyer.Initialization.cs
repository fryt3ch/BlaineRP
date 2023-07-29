using System.Collections.Generic;
using System.Threading;
using BlaineRP.Server.Game.NPCs;
using BlaineRP.Server.UtilsT;
using GTANetworkAPI;

namespace BlaineRP.Server.Game.Misc
{
    public partial class FishBuyer
    {
        public static void InitializeAll()
        {
            if (All != null)
                return;

            var pos1 = new Vector4(-55.288f, 1897.339f, 195.3613f, 68.66809f);

            Service.AddNpc("fishbuyer_0", new Vector3(pos1.X, pos1.Y, pos1.Z));

            All = new List<FishBuyer>()
            {
                new FishBuyer(),
            };

            var lines = new List<string>();

            lines.Add($"new {nameof(BlaineRP.Client.Game.Misc.FishBuyer)}({pos1.ToCSharpStr()});");

            Utils.FillFileToReplaceRegion(
                System.IO.Directory.GetCurrentDirectory() + Properties.Settings.Static.ClientScriptsTargetPath + @"\Game\Misc\FishBuyer.Initialization.cs",
                "TO_REPLACE",
                lines
            );

            FishBuyersPricesUpdateTimer = new Timer((obj) =>
                {
                    NAPI.Task.Run(() =>
                        {
                            foreach (var x in All)
                                x.SetRandomPriceCoef();
                        }
                    );
                },
                null,
                0,
                FISHBUYER_COEF_UPDATE_TIMEOUT
            );
        }
    }
}