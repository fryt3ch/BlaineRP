using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Parachute : Item, IUsable
    {
        public new class ItemData : Item.ItemData
        {
            public ItemData(string name, float weight) : base(name, weight)
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}