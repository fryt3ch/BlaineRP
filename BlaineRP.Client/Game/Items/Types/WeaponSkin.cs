using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class WeaponSkin : Item
    {
        public new class ItemData : Item.ItemData
        {
            public enum Types
            {
                UniDef = 0,
                UniMk2,
            }

            public Types Type { get; set; }

            public ItemData(string name, float weight, Types type) : base(name, weight)
            {
                this.Type = type;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}