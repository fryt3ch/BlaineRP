using System.Collections.Generic;

namespace BlaineRP.Server.Game.Misc
{
    public partial class MarketStall
    {
        public static void Initialize()
        {
            if (All != null)
                return;

            All = new MarketStall[]
            {
                new MarketStall(-1162.669f, -1715.937f, 3.603473f, -125f + 90f),
                new MarketStall(-1167.805f, -1708.603f, 3.667917f, -125f + 90f),
                new MarketStall(-1172.509f, -1698.171f, 3.7616f, -90f + 90f),
                new MarketStall(-1164.441f, -1692.301f, 3.767002f, -35f + 90f),
                new MarketStall(-1151.815f, -1683.46f, 3.767002f, -35f + 90f),
                new MarketStall(-1140.888f, -1682.408f, 3.767002f, 20f + 90f),
                new MarketStall(-1133.602f, -1689.125f, 3.767002f, 55f + 90f),
                new MarketStall(-1128.458f, -1696.47f, 3.767002f, 55f + 90f),
                new MarketStall(-1138.354f, -1704.733f, 3.767002f, 145f + 90f),
                new MarketStall(-1150.549f, -1713.272f, 3.611554f, 145f + 90f),
                new MarketStall(-1161.594f, -1706.65f, 3.702462f, 55f + 90f),
                new MarketStall(-1155.441f, -1694.278f, 3.767002f, 155f + 90f),
                new MarketStall(-1149.936f, -1699.669f, 3.723997f, -35f + 90f),
                new MarketStall(-1138.576f, -1692.609f, 3.786295f, -125f + 90f),
            };

            var lines = new List<string>();

            for (int i = 0; i < All.Length; i++)
            {
                var x = All[i];

                lines.Add($"new {nameof(BlaineRP.Client.Game.Misc.MarketStall)}({i}, {x.Position.ToCSharpStr()});");
            }

            Utils.FillFileToReplaceRegion(System.IO.Directory.GetCurrentDirectory() +
                                          Properties.Settings.Static.ClientScriptsTargetPath +
                                          @"\Game\Misc\MarketStall.Initialization.cs",
                "TO_REPLACE",
                lines
            );
        }
    }
}