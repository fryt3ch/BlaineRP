using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class Weapon : Item, ITagged, IWearable
    {
        public new class ItemData : Item.ItemData
        {
            public int MaxAmmo { get; set; }

            public string AmmoId { get; set; }

            public uint Hash { get; set; }

            public ItemData(string name, float weight, string ammoId, int maxAmmo, uint hash) : base(name, weight)
            {
                this.MaxAmmo = maxAmmo;

                this.AmmoId = ammoId;

                this.Hash = hash;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}