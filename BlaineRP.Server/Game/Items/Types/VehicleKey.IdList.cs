using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class VehicleKey
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "vk_0", new ItemData("Ключ от транспорта", 0.01f, "p_car_keys_01") },
        };
    }
}