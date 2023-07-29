using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Tent
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "tent_0", new ItemData("Палатка (серая)", 2f, "brp_p_tent_0_grey") },
            { "tent_1", new ItemData("Палатка (синяя)", 2f, "brp_p_tent_0_blue") },
            { "tent_2", new ItemData("Палатка (жёлтая)", 2f, "brp_p_tent_0_yellow") },
        };
    }
}