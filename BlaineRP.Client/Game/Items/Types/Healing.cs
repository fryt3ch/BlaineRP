using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Healing : StatusChanger, IStackable
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IStackable
        {
            public ItemData(string name, float weight, int health, int maxAmount) : base(name, weight, 0, 0, health)
            {
                MaxAmount = maxAmount;
            }

            public int MaxAmount { get; set; }
        }
    }
}