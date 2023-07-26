using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class VehicleJerrycan : Item, IConsumable
    {
        public new class ItemData : Item.ItemData, Item.ItemData.IConsumable
        {
            public int MaxAmount { get; set; }

            public bool IsPetrol { get; set; }

            public ItemData(string name, float weight, int maxAmount, bool isPetrol) : base(name, weight)
            {
                this.MaxAmount = maxAmount;

                this.IsPetrol = isPetrol;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}