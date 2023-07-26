using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class MiscStackable : Item, IStackable
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Item.ItemData, Item.ItemData.IStackable
        {
            public ItemData(string name, float weight, int maxAmount) : base(name, weight)
            {
                MaxAmount = maxAmount;
            }

            public int MaxAmount { get; set; }
        }
    }
}