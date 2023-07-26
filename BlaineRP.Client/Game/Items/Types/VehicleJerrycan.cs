using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class VehicleJerrycan : Item, IConsumable
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Item.ItemData, Item.ItemData.IConsumable
        {
            public ItemData(string name, float weight, int maxAmount, bool isPetrol) : base(name, weight)
            {
                MaxAmount = maxAmount;

                IsPetrol = isPetrol;
            }

            public bool IsPetrol { get; set; }
            public int MaxAmount { get; set; }
        }
    }
}