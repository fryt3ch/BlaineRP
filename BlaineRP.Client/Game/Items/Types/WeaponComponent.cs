using System.Collections.Generic;
using BlaineRP.Client.Game.Management.Weapons;

namespace BlaineRP.Client.Game.Items
{
    public class WeaponComponent : Item
    {
        public new class ItemData : Item.ItemData
        {
            public WeaponComponentTypes Type { get; set; }

            public ItemData(string name, float weight, WeaponComponentTypes type) : base(name, weight)
            {
                this.Type = type;
            }
        }

        public static Dictionary<string, Item.ItemData> IdList { get; set; } = new Dictionary<string, Item.ItemData>();
    }
}