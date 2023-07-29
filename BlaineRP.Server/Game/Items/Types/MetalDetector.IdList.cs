using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class MetalDetector
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "metaldet_0", new ItemData("Металлоискатель", 2f) },
        };
    }
}