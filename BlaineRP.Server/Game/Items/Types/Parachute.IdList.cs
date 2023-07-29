using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Parachute
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "parachute_0", new ItemData("Парашют", "p_parachute_s_shop", 0.35f) },
        };
    }
}