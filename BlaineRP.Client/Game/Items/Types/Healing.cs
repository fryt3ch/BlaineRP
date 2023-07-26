using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Healing : StatusChanger, IStackable
    {
        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public int MaxAmount { get; set; }

            public ItemData(string name, float weight, int health, int maxAmount) : base(name, weight, 0, 0, health)
            {
                this.MaxAmount = maxAmount;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}