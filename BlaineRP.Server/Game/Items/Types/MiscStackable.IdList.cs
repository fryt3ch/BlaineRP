using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class MiscStackable
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "mis_0", new ItemData("Приманка для рыбы", 0.02f, "prop_paints_can04", 1024) },
            { "mis_1", new ItemData("Червяк", 0.01f, "prop_paints_can04", 1024) },

            { "mis_gpstr", new ItemData("GPS-трекер", 0.05f, "lr_prop_carkey_fob", 5) },
            { "mis_lockpick", new ItemData("Отмычка", 0.01f, "prop_cuff_keys_01", 32) },
        };
    }
}