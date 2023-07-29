using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class VehicleJerrycan
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "vjc_0", new ItemData("Канистра с топливом", "w_am_jerrycan", 30, 2.5f, true) },
            { "vjc_1", new ItemData("Аккумулятор", "prop_car_battery_01", 30, 2.5f, false) },
        };
    }
}