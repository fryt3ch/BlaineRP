using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Ring
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "ring_m_0", new ItemData("Золотое кольцо с бриллиантами", true, "brp_p_ring_0_0", "ring_f_0") },
            { "ring_m_1", new ItemData("Золотое кольцо с красным камнем", true, "brp_p_ring_1_0", "ring_f_1") },

            { "ring_f_0", new ItemData("Золотое кольцо с бриллиантами", false, "brp_p_ring_0_0", "ring_m_0") },
            { "ring_f_1", new ItemData("Золотое кольцо с красным камнем", false, "brp_p_ring_1_0", "ring_m_1") },
        };
    }
}