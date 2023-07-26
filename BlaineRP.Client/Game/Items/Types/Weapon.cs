using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Weapon : Item, ITagged, IWearable
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Item.ItemData
        {
            public ItemData(string name, float weight, string ammoId, int maxAmmo, uint hash) : base(name, weight)
            {
                MaxAmmo = maxAmmo;

                AmmoId = ammoId;

                Hash = hash;
            }

            public int MaxAmmo { get; set; }

            public string AmmoId { get; set; }

            public uint Hash { get; set; }
        }
    }
}