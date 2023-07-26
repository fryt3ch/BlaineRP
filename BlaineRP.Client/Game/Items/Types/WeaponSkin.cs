using System.Collections.Generic;

namespace BlaineRP.Client.Game.Items
{
    public class WeaponSkin : Item
    {
        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();

        public new class ItemData : Item.ItemData
        {
            public enum Types
            {
                UniDef = 0,
                UniMk2,
            }

            public ItemData(string name, float weight, Types type) : base(name, weight)
            {
                Type = type;
            }

            public Types Type { get; set; }
        }
    }
}