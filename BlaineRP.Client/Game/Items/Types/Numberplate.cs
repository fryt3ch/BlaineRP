using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Numberplate : Item, ITaggedFull
    {
        public new class ItemData : Item.ItemData
        {
            public int Number { get; set; }

            public ItemData(string name, float weight, int number) : base(name, weight)
            {
                this.Number = number;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}