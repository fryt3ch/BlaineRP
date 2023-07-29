using System.Collections.Generic;
using BlaineRP.Server.Game.Craft;
using BlaineRP.Server.Game.Craft.Workbenches;

namespace BlaineRP.Server.Game.Items
{
    public partial class Workbench
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "pwb_bbq_0", new ItemData("Гриль (компактный)", 5f, "prop_bbq_4", WorkbenchTypes.Grill) },
            { "pwb_bbq_1", new ItemData("Гриль (большой)", 10f, "prop_bbq_5", WorkbenchTypes.Grill) },

            { "pwb_bbq_2", new ItemData("Костёр", 1f, "prop_beach_fire", WorkbenchTypes.Grill) },
        };
    }
}