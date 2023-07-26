using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Hat : Clothes, Clothes.IToggleable, Clothes.IProp
    {
        public new class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public ExtraData ExtraData { get; set; }

            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, ExtraData extraData = null, string sexAlternativeId = null) : base(name, weight, sex, drawable, textures, sexAlternativeId)
            {
                this.ExtraData = extraData;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}