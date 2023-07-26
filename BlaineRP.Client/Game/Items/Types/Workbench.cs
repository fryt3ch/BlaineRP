using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Workbench : PlaceableItem
    {
        public new class ItemData : PlaceableItem.ItemData
        {
            public ItemData(string name, float weight, uint model) : base(name, weight, model)
            {

            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}