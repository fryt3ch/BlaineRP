using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class VehicleRepairKit : Item, IConsumable
    {
        public new class ItemData : Item.ItemData, Item.ItemData.IConsumable
        {
            public int MaxAmount { get; set; }

            public ItemData(string name, float weight, int maxAmount) : base(name, weight)
            {
                this.MaxAmount = maxAmount;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}