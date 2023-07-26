using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Numberplate : Item, ITaggedFull
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Item.ItemData
        {
            public ItemData(string name, float weight, int number) : base(name, weight)
            {
                Number = number;
            }

            public int Number { get; set; }
        }
    }
}