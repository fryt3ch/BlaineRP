using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Under : Clothes, Clothes.IToggleable
    {
        public new class ItemData : Clothes.ItemData, Clothes.ItemData.IToggleable
        {
            public Top.ItemData BestTop { get; set; }

            public int BestTorso { get; set; }

            public ExtraData ExtraData { get; set; }

            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, Top.ItemData bestTop, int bestTorso, ExtraData extraData = null, string sexAlternativeId = null) : base(name, weight, sex, drawable, textures, sexAlternativeId)
            {
                this.BestTop = bestTop;
                this.ExtraData = extraData;

                this.BestTorso = bestTorso;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}