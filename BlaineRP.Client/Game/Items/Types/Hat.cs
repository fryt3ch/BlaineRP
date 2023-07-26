using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Hat : Clothes, Clothes.IToggleable, Clothes.IProp
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, ExtraData extraData = null, string sexAlternativeId = null) : base(name,
                weight,
                sex,
                drawable,
                textures,
                sexAlternativeId
            )
            {
                ExtraData = extraData;
            }

            public ExtraData ExtraData { get; set; }
        }
    }
}