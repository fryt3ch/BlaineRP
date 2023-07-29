using System.Collections.Generic;

namespace BlaineRP.Server.Game.Items
{
    public partial class Numberplate
    {
        public static readonly Dictionary<string, Item.ItemData> IdList = new Dictionary<string, Item.ItemData>()
        {
            { "np_0", new ItemData("Номерной знак", "p_num_plate_01", 0) },
            { "np_1", new ItemData("Номерной знак", "p_num_plate_04", 1) },
            { "np_2", new ItemData("Номерной знак", "p_num_plate_02", 2) },
            { "np_3", new ItemData("Номерной знак", "p_num_plate_02", 3) },
            { "np_4", new ItemData("Номерной знак", "p_num_plate_01", 4) },
            { "np_5", new ItemData("Номерной знак", "p_num_plate_01", 5) },
        };
    }
}