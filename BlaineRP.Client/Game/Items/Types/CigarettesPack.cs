using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class CigarettesPack : StatusChanger, IConsumable
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : StatusChanger.ItemData, Item.ItemData.IConsumable
        {
            public ItemData(string name, float weight, int mood, int maxAmount) : base(name, weight, 0, mood, 0)
            {
                MaxAmount = maxAmount;
            }

            public int MaxAmount { get; set; }
        }
    }
}