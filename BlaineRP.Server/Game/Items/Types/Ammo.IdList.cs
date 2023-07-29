using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Ammo
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "am_5.56", new ItemData("Патроны 5.56мм", 0.01f, "w_am_case", 1024) },
            { "am_7.62", new ItemData("Патроны 7.62мм", 0.01f, "w_am_case", 1024) },
            { "am_9", new ItemData("Патроны 9мм", 0.01f, "w_am_case", 1024) },
            { "am_11.43", new ItemData("Патроны 11.43мм", 0.015f, "w_am_case", 512) },
            { "am_12", new ItemData("Патроны 12мм", 0.015f, "w_am_case", 512) },
            { "am_12.7", new ItemData("Патроны 12.7мм", 0.015f, "w_am_case", 256) },
        };
    }
}