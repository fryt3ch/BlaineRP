using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class VehicleRepairKit
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "vrk_0", new ItemData("Ремонтный набор S", "imp_prop_tool_box_01b", 3, 1f) },
            { "vrk_1", new ItemData("Ремонтный набор M", "gr_prop_gr_tool_box_01a", 7, 1.5f) },
        };
    }
}