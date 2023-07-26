using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class VehicleKey : Item, ITaggedFull
    {
        public static Dictionary<string, ItemData> IdList { get; set; } = new Dictionary<string, ItemData>();
    }
}