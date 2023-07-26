using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Gloves : Clothes
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Clothes.ItemData
        {
            public ItemData(string name, float weight, bool sex, int drawable, int[] textures, Dictionary<int, int> bestTorsos, string sexAlternativeId = null) : base(name,
                weight,
                sex,
                drawable,
                textures,
                sexAlternativeId
            )
            {
                BestTorsos = bestTorsos;
            }

            public Dictionary<int, int> BestTorsos { get; set; }
        }
    }
}