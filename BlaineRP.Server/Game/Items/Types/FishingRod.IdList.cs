using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class FishingRod
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "rod_0", new ItemData("Удочка (обычн.)", "prop_fishing_rod_02", 1f) },
            { "rod_1", new ItemData("Удочка (улучш.)", "prop_fishing_rod_02", 1f) },
        };
    }
}