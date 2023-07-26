using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Accessory : Clothes
    {
        public new class ItemData : Clothes.ItemData
        {
            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, string sexAlternativeId = null) : base(name, weight, sex, drawable, textures, sexAlternativeId) { }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}