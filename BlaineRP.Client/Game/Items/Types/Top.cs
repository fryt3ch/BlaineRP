using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Top : Clothes, Clothes.IToggleable
    {
        public new class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, int bestTorso, ExtraData extraData = null, string sexAlternativeId = null) : base(name, weight, sex, drawable, textures, sexAlternativeId)
            {
                this.BestTorso = bestTorso;
                this.ExtraData = extraData;
            }

            public ItemData(bool sex, int drawable, int[] textures, int bestTorso, ExtraData extraData = null, string sexAlternativeId = null) : this(null, 0f, sex, drawable, textures, bestTorso, extraData, sexAlternativeId) { }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}