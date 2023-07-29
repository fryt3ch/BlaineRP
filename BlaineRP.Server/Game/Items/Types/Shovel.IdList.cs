using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Shovel
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "shovel_0", new ItemData("Лопата", "prop_tool_shovel2", 1.5f) },
        };
    }
}